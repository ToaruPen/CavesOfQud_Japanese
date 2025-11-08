#!/usr/bin/env python3
"""
Apply LLM translations back into Mods/QudJP/Localization/ObjectBlueprints/*.jp.xml.
"""

from __future__ import annotations

import argparse
import json
import sys
from pathlib import Path
from typing import Dict, List, Tuple

REPO_ROOT = Path(__file__).resolve().parents[1]


def load_json(path: Path) -> dict:
    return json.loads(path.read_text(encoding="utf-8"))


def build_translation_map(data: object) -> Dict[str, str]:
    """
    Accepts either:
    - {"translations": [{"id": "...", "translation": "..."}]}
    - [{"id": "...", "translation": "..."}]
    - {"id1": "translation", ...} (legacy / quick testing)
    """
    entries: List[dict]
    if isinstance(data, dict) and "translations" in data:
        entries = data["translations"]  # type: ignore[assignment]
    elif isinstance(data, list):
        entries = data  # type: ignore[assignment]
    elif isinstance(data, dict):
        entries = [{"id": key, "translation": value} for key, value in data.items()]
    else:
        raise SystemExit("Translations JSON must be a dict or list.")

    mapping: Dict[str, str] = {}
    for entry in entries:
        if not isinstance(entry, dict):
            raise SystemExit("Each translation entry must be an object with 'id' and 'translation'.")
        attr_id = entry.get("id")
        translation = entry.get("translation")
        if not attr_id:
            raise SystemExit(f"Missing 'id' in translation entry: {entry}")
        if translation is None:
            raise SystemExit(f"Missing 'translation' for id '{attr_id}'.")
        if attr_id in mapping:
            raise SystemExit(f"Duplicate translation for id '{attr_id}'.")
        if not isinstance(translation, str):
            raise SystemExit(f"Translation for id '{attr_id}' must be a string.")
        mapping[attr_id] = translation
    return mapping


def normalize_translation(value: str) -> str:
    value = value.replace("\r\n", "\n").replace("\r", "\n")
    return value.replace("&#10;", "\n")


def escape_attribute(value: str) -> str:
    escaped = normalize_translation(value)
    escaped = escaped.replace("&", "&amp;").replace('"', "&quot;").replace("<", "&lt;").replace(">", "&gt;")
    escaped = escaped.replace("\n", "&#10;")
    return escaped


def format_attributes(pairs: List[Tuple[str, str]]) -> str:
    return " ".join(f'{key}="{escape_attribute(value)}"' for key, value in pairs)


def build_object_snippet(object_entry: dict, translations: Dict[str, str]) -> List[str]:
    name = object_entry["name"]
    if not name:
        raise SystemExit("Object entry is missing 'name'.")

    attributes = [("Name", name)]
    for attr in object_entry.get("attributes", []):
        attributes.append((attr["name"], attr["value"]))
    attributes.append(("Replace", "true"))

    object_lines = [f"  <object {format_attributes(attributes)}>"]

    parts = object_entry.get("parts", [])
    if not parts:
        raise SystemExit(f"Object '{name}' has no parts with translatable attributes.")

    for part in parts:
        part_pairs: List[Tuple[str, str]] = []
        for attr in part.get("attributes", []):
            attr_name = attr["name"]
            if attr_name == "Replace":
                continue
            value = attr["value"]
            if attr.get("translate"):
                attr_id = attr["id"]
                if attr_id not in translations:
                    raise SystemExit(f"Missing translation for id '{attr_id}' (object '{name}').")
                value = translations[attr_id]
            part_pairs.append((attr_name, value))
        part_pairs.append(("Replace", "true"))
        object_lines.append(f"    <part {format_attributes(part_pairs)} />")

    object_lines.append("  </object>")
    return object_lines


def ensure_target_available(target: Path, object_names: List[str]) -> str:
    text = target.read_text(encoding="utf-8")
    for name in object_names:
        marker = f'<object Name="{name}"'
        if marker in text:
            raise SystemExit(f"Target file already contains object '{name}'. Aborting to prevent duplicates.")
    return text


def insert_snippet(target: Path, snippet: str, current_text: str, dry_run: bool) -> None:
    closing = "</objects>"
    idx = current_text.rfind(closing)
    if idx == -1:
        raise SystemExit("Could not find </objects> in target file.")

    before = current_text[:idx].rstrip()
    after = current_text[idx:]
    new_content = f"{before}\n\n{snippet.rstrip()}\n\n{after}"

    if dry_run:
        print(snippet)
    else:
        target.write_text(new_content, encoding="utf-8")
        print(f"Inserted {snippet.count('<object')} objects into {target}")


def resolve_repo_path(path_str: str) -> Path:
    path = Path(path_str)
    if not path.is_absolute():
        path = (REPO_ROOT / path).resolve()
    return path


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Reinsert translated ObjectBlueprint snippets into the JP XML file.")
    parser.add_argument(
        "--payload",
        type=Path,
        required=True,
        help="Path to the JSON payload produced by scripts/objectblueprint_extract.py.",
    )
    parser.add_argument(
        "--translations",
        type=Path,
        required=True,
        help="Path to the LLM response JSON (see Docs/tasks/objectblueprints_llm.md).",
    )
    parser.add_argument(
        "--target",
        type=Path,
        help="Target XML file to modify. Defaults to the 'localized_file' recorded in the payload.",
    )
    parser.add_argument(
        "--dry-run",
        action="store_true",
        help="Print the generated snippet instead of modifying the XML file.",
    )
    return parser.parse_args()


def main() -> None:
    args = parse_args()

    payload_path = args.payload.expanduser().resolve()
    translations_path = args.translations.expanduser().resolve()

    if not payload_path.exists():
        raise SystemExit(f"Payload not found: {payload_path}")
    if not translations_path.exists():
        raise SystemExit(f"Translations file not found: {translations_path}")

    payload = load_json(payload_path)
    translation_map = build_translation_map(load_json(translations_path))

    objects = payload.get("objects", [])
    if not objects:
        raise SystemExit("Payload contains no objects to insert.")

    pending_strings = payload.get("pending_strings", []) or []
    expected_ids = {entry["id"] for entry in pending_strings if isinstance(entry, dict) and "id" in entry}
    if not expected_ids:
        # Fall back to scanning the object entries if pending_strings is absent.
        for obj in payload.get("objects", []):
            for part in obj.get("parts", []):
                for attr in part.get("attributes", []):
                    if attr.get("translate") and "id" in attr:
                        expected_ids.add(attr["id"])
    missing = expected_ids - set(translation_map.keys())
    extra = set(translation_map.keys()) - expected_ids
    if missing:
        raise SystemExit(f"Translations JSON is missing {len(missing)} ids (e.g., {next(iter(missing))}).")
    if extra:
        print(f"[WARN] {len(extra)} translations do not match any pending string (e.g., {next(iter(extra))}).", file=sys.stderr)

    sorted_objects = sorted(objects, key=lambda obj: obj.get("base_index", 0))
    snippet_lines: List[str] = []
    for obj in sorted_objects:
        if obj.get("already_localized"):
            raise SystemExit(
                f"Object '{obj.get('name')}' is marked as already localized; "
                "rerun the extractor without --include-present to avoid duplicates."
            )
        snippet_lines.extend(build_object_snippet(obj, translation_map))
        snippet_lines.append("")  # blank line between objects

    snippet = "\n".join(line for line in snippet_lines if line is not None).rstrip()

    if args.target:
        target_path = args.target.expanduser().resolve()
    else:
        localized = payload.get("localized_file")
        if not localized:
            raise SystemExit("Payload is missing 'localized_file'. Use --target to specify the destination manually.")
        target_path = resolve_repo_path(localized)

    if not target_path.exists():
        raise SystemExit(f"Target XML file not found: {target_path}")

    current_text = ensure_target_available(target_path, [obj["name"] for obj in sorted_objects])

    insert_snippet(target_path, snippet, current_text, args.dry_run)


if __name__ == "__main__":
    main()
