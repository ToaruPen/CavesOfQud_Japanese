#!/usr/bin/env python3
"""
Apply LLM translations back into Mods/QudJP/Localization/ObjectBlueprints/*.jp.xml.
"""

from __future__ import annotations

import argparse
import json
import sys
from copy import deepcopy
from pathlib import Path
from typing import Dict, List
import re
import xml.etree.ElementTree as ET

REPO_ROOT = Path(__file__).resolve().parents[1]
INVALID_XML_REF = re.compile(r"&#(x[0-9a-fA-F]+|\d+);")
INVALID_XML_CHAR = re.compile(r"[\x00-\x08\x0B\x0C\x0E-\x1F]")


def sanitize_xml(text: str) -> str:
    def replace_ref(match: re.Match[str]) -> str:
        value = match.group(1)
        codepoint = int(value[1:], 16) if value.startswith("x") else int(value)
        if codepoint in (0x9, 0xA, 0xD) or codepoint >= 0x20:
            return match.group(0)
        return ""

    text = INVALID_XML_REF.sub(replace_ref, text)
    return INVALID_XML_CHAR.sub("", text)


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


def resolve_repo_path(path_str: str) -> Path:
    path = Path(path_str)
    if not path.is_absolute():
        path = (REPO_ROOT / path).resolve()
    return path


def load_base_objects(base_path: Path) -> Dict[str, ET.Element]:
    if not base_path.exists():
        raise SystemExit(f"Base file not found: {base_path}")
    text = base_path.read_text(encoding="utf-8-sig")
    sanitized = sanitize_xml(text)
    root = ET.fromstring(sanitized)
    objects: Dict[str, ET.Element] = {}
    for elem in root.findall("./object"):
        name = elem.attrib.get("Name")
        if not name or name in objects:
            continue
        objects[name] = elem
    return objects


def indent_element(elem: ET.Element, level: int = 0, space: str = "  ") -> None:
    children = list(elem)
    if not children:
        return
    indent_text = "\n" + space * (level + 1)
    for child in children:
        child.tail = indent_text
        indent_element(child, level + 1, space)
    children[-1].tail = "\n" + space * level


def serialize_object_element(element: ET.Element) -> str:
    wrapper = ET.Element("root")
    wrapper.append(deepcopy(element))
    try:
        ET.indent(wrapper, space="  ", level=0)  # type: ignore[attr-defined]
    except AttributeError:
        indent_element(wrapper, level=0)
    xml = ET.tostring(wrapper, encoding="unicode")
    start = xml.find(">") + 1
    end = xml.rfind("</root>")
    return xml[start:end].strip()


def apply_translations_to_object(element: ET.Element, object_entry: dict, translations: Dict[str, str]) -> None:
    parts = element.findall("./part")
    parts_count = len(parts)

    for part_entry in object_entry.get("parts", []):
        part_index = part_entry.get("index")
        if part_index is None or part_index >= parts_count:
            raise SystemExit(
                f"Object '{object_entry.get('name')}' part index {part_index} is out of range "
                f"(found {parts_count} parts in base file)."
            )
        target_part = parts[part_index]
        for attr in part_entry.get("attributes", []):
            if not attr.get("translate"):
                continue
            attr_id = attr.get("id")
            attr_name = attr.get("name")
            if not attr_id or not attr_name:
                raise SystemExit(f"Malformed attribute entry on object '{object_entry.get('name')}'.")
            if attr_id not in translations:
                raise SystemExit(f"Missing translation for id '{attr_id}' (object '{object_entry.get('name')}'.)")
            if attr_name not in target_part.attrib:
                print(
                    f"[WARN] Skipping attribute '{attr_name}' on part '{target_part.attrib.get('Name')}' "
                    f"for object '{object_entry.get('name')}' because it does not exist in the base definition.",
                    file=sys.stderr,
                )
                continue
            target_part.set(attr_name, normalize_translation(translations[attr_id]))


def build_object_element(
    object_entry: dict, translations: Dict[str, str], base_objects: Dict[str, ET.Element]
) -> ET.Element:
    name = object_entry.get("name")
    if not name:
        raise SystemExit("Object entry is missing 'name'.")
    base_element = base_objects.get(name)
    if base_element is None:
        raise SystemExit(
            f"Object '{name}' was not found in the base file; "
            "rerun the extractor to refresh the payload."
        )
    element = deepcopy(base_element)
    element.set("Replace", "true")
    apply_translations_to_object(element, object_entry, translations)
    return element


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

    localized_path = payload.get("localized_file")
    base_file_in_payload = payload.get("base_file")
    if not localized_path:
        raise SystemExit("Payload is missing 'localized_file'. Use --target to specify the destination manually.")
    if not base_file_in_payload:
        raise SystemExit("Payload is missing 'base_file'. Please regenerate the payload with the latest extractor.")

    base_path = resolve_repo_path(base_file_in_payload)
    base_objects = load_base_objects(base_path)

    sorted_objects = sorted(objects, key=lambda obj: obj.get("base_index", 0))
    snippet_lines: List[str] = []
    for obj in sorted_objects:
        if obj.get("already_localized"):
            raise SystemExit(
                f"Object '{obj.get('name')}' is marked as already localized; "
                "rerun the extractor without --include-present to avoid duplicates."
            )
        element = build_object_element(obj, translation_map, base_objects)
        snippet_lines.append(serialize_object_element(element))

    snippet = "\n\n".join(snippet_lines).rstrip()

    target_path = args.target.expanduser().resolve() if args.target else resolve_repo_path(localized_path)
    if not target_path.exists():
        raise SystemExit(f"Target XML file not found: {target_path}")

    current_text = ensure_target_available(target_path, [obj["name"] for obj in sorted_objects])
    insert_snippet(target_path, snippet, current_text, args.dry_run)


if __name__ == "__main__":
    main()
