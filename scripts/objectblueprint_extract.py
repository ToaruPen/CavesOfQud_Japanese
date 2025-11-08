#!/usr/bin/env python3
"""
Extract untranslated ObjectBlueprint entries and stringify their translatable
attributes so they can be handed to an LLM for translation.
"""

from __future__ import annotations

import argparse
import json
import re
from dataclasses import dataclass
from datetime import datetime, timezone
from pathlib import Path
from typing import Dict, Iterable, List, Optional, Sequence, Tuple

REPO_ROOT = Path(__file__).resolve().parents[1]
BASE_ROOT = REPO_ROOT / "references" / "Base"
LOCALIZATION_ROOT = REPO_ROOT / "Mods" / "QudJP" / "Localization"

INVALID_XML_REF = re.compile(r"&#(x[0-9a-fA-F]+|\d+);")
INVALID_XML_CHAR = re.compile(r"[\x00-\x08\x0B\x0C\x0E-\x1F]")
COLOR_TAG_RE = re.compile(r"\{\{([^|}]+)\|([^}]*)\}\}")

TRANSLATABLE_EXACT = {
    "short",
    "long",
    "flavor",
    "flavor1",
    "flavor2",
    "flavor3",
    "flavor4",
    "verb",
    "verb1",
    "verb2",
    "verb3",
    "subtitle",
    "title",
}

TRANSLATABLE_SUBSTRINGS = (
    "description",
    "displayname",
    "name",
    "text",
    "message",
    "article",
    "tooltip",
    "lore",
    "story",
    "speech",
    "chant",
    "gospel",
    "hagiograph",
    "verse",
    "poem",
)


def sanitize_xml(text: str) -> str:
    def replace_ref(match: re.Match[str]) -> str:
        value = match.group(1)
        codepoint = int(value[1:], 16) if value.startswith("x") else int(value)
        if codepoint in (0x9, 0xA, 0xD) or codepoint >= 0x20:
            return match.group(0)
        return ""

    text = INVALID_XML_REF.sub(replace_ref, text)
    return INVALID_XML_CHAR.sub("", text)


def load_xml(path: Path):
    import xml.etree.ElementTree as ET

    if not path.exists():
        raise FileNotFoundError(path)
    text = path.read_text(encoding="utf-8-sig")
    sanitized = sanitize_xml(text)
    return ET.fromstring(sanitized)


def normalize_path(path: Path) -> str:
    try:
        return path.resolve().relative_to(REPO_ROOT).as_posix()
    except ValueError:
        return str(path.resolve())


def should_translate_attribute(name: str) -> bool:
    lowered = name.lower()
    if lowered == "name":
        return False
    if lowered in TRANSLATABLE_EXACT:
        return True
    return any(token in lowered for token in TRANSLATABLE_SUBSTRINGS)


def classify_string(part_name: str, attr_name: str) -> str:
    lowered_attr = attr_name.lower()
    lowered_part = (part_name or "").lower()
    if lowered_attr == "displayname":
        return "display-name"
    if lowered_attr in {"short", "long"} and lowered_part == "description":
        return f"description-{lowered_attr}"
    if lowered_attr == "behaviordescription":
        return "behavior-description"
    if lowered_part == "rulesdescription" and lowered_attr == "text":
        return "rules-text"
    if "name" in lowered_attr:
        return "proper-name"
    if "description" in lowered_attr:
        return "description"
    if "text" in lowered_attr:
        return "ui-text"
    if "message" in lowered_attr:
        return "message"
    return "generic"


def extract_color_tags(value: str) -> List[Dict[str, str]]:
    tags: List[Dict[str, str]] = []
    for match in COLOR_TAG_RE.finditer(value):
        tags.append({"code": match.group(1), "text": match.group(2)})
    return tags


@dataclass
class AttributeEntry:
    name: str
    value: str
    translate: bool
    part_index: int
    object_name: str
    part_name: str

    @property
    def attr_id(self) -> str:
        return f"{self.object_name}:{self.part_index}:{self.part_name}:{self.name}"


def iter_objects(root) -> Iterable[Tuple[int, object]]:
    for index, elem in enumerate(root.findall("./object")):
        yield index, elem


def collect_object_names(root) -> Dict[str, int]:
    names: Dict[str, int] = {}
    for index, elem in iter_objects(root):
        name = elem.attrib.get("Name")
        if name:
            names[name.lower()] = index
    return names


def format_attributes(attrib: Dict[str, str]) -> List[Dict[str, str]]:
    ordered_keys = sorted(
        attrib.keys(),
        key=lambda key: (0 if key == "Name" else 1, key),
    )
    return [{"name": key, "value": attrib[key]} for key in ordered_keys]


def extract_object_data(obj, base_index: int) -> Tuple[Dict[str, object], List[Dict[str, object]]]:
    object_name = obj.attrib.get("Name")
    inherits = obj.attrib.get("Inherits")
    object_entry = {
        "name": object_name,
        "base_index": base_index,
        "inherits": inherits,
        "attributes": format_attributes({k: v for k, v in obj.attrib.items() if k != "Name"}),
        "parts": [],
    }
    pending_strings: List[Dict[str, object]] = []

    for part_index, part in enumerate(obj.findall("./part")):
        part_name = part.attrib.get("Name", "")
        attr_entries: List[AttributeEntry] = []
        has_translate_attr = False

        ordered_keys = sorted(part.attrib.keys(), key=lambda key: (0 if key == "Name" else 1, key))
        for attr_name in ordered_keys:
            attr_value = part.attrib[attr_name]
            attr_entry = AttributeEntry(
                name=attr_name,
                value=attr_value,
                translate=should_translate_attribute(attr_name),
                part_index=part_index,
                object_name=object_name,
                part_name=part_name,
            )
            if attr_entry.translate and attr_name != "Name":
                has_translate_attr = True
                color_tags = extract_color_tags(attr_value)
                pending_strings.append(
                    {
                        "id": attr_entry.attr_id,
                        "object": object_name,
                        "inherits": inherits,
                        "part": part_name,
                        "attribute": attr_name,
                        "source": attr_value,
                        "type": classify_string(part_name, attr_name),
                        "color_tags": color_tags,
                        "contains_newline": "\n" in attr_value,
                        "contains_markup": "&" in attr_value or "&#" in attr_value,
                        "order": {
                            "object_index": base_index,
                            "part_index": part_index,
                            "attribute": attr_name,
                        },
                    }
                )
            attr_entries.append(attr_entry)

        if has_translate_attr:
            object_entry["parts"].append(
                {
                    "name": part_name,
                    "index": part_index,
                    "attributes": [
                        {
                            "id": entry.attr_id,
                            "name": entry.name,
                            "value": entry.value,
                            "translate": entry.translate and entry.name != "Name",
                        }
                        for entry in attr_entries
                    ],
                }
            )

    return object_entry, pending_strings


def build_default_localized_path(base_file: Path) -> Optional[Path]:
    try:
        rel = base_file.resolve().relative_to(BASE_ROOT)
    except ValueError:
        return None
    localized_name = rel.stem + ".jp" + rel.suffix
    return (LOCALIZATION_ROOT / rel.parent / localized_name).resolve()


def filter_objects(names: Sequence[str] | None) -> Optional[set[str]]:
    if not names:
        return None
    return {name.lower() for name in names}


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Extract untranslated ObjectBlueprint entries from an XML file.")
    parser.add_argument(
        "--base-file",
        type=Path,
        default=BASE_ROOT / "ObjectBlueprints" / "Items.xml",
        help="Path to the base XML file (default: references/Base/ObjectBlueprints/Items.xml).",
    )
    parser.add_argument(
        "--localized-file",
        type=Path,
        help="Path to the localized XML file. Defaults to Mods/QudJP/Localization relative to the base file.",
    )
    parser.add_argument(
        "--output",
        type=Path,
        help="Optional path to write the JSON payload. Defaults to stdout.",
    )
    parser.add_argument(
        "--object",
        dest="objects",
        action="append",
        help="Limit extraction to specific object names. Can be repeated.",
    )
    parser.add_argument(
        "--max-objects",
        type=int,
        help="Stop after emitting this many objects.",
    )
    parser.add_argument(
        "--include-present",
        action="store_true",
        help="Include objects that already exist in the localized file.",
    )
    return parser.parse_args()


def main() -> None:
    args = parse_args()
    base_file = args.base_file.expanduser().resolve()
    localized_file = args.localized_file.expanduser().resolve() if args.localized_file else build_default_localized_path(base_file)
    if localized_file is None:
        raise SystemExit("Unable to infer --localized-file; please specify it explicitly.")

    if not base_file.exists():
        raise SystemExit(f"Base file not found: {base_file}")

    localized_exists = localized_file.exists()

    base_root = load_xml(base_file)
    localized_root = load_xml(localized_file) if localized_exists else None

    localized_names = collect_object_names(localized_root) if localized_root is not None else {}
    localized_name_set = set(localized_names.keys())
    filters = filter_objects(args.objects)

    objects_payload: List[Dict[str, object]] = []
    strings_payload: List[Dict[str, object]] = []
    total_objects = 0

    for base_index, obj in iter_objects(base_root):
        total_objects += 1
        object_name = obj.attrib.get("Name")
        if not object_name:
            continue
        lowered_name = object_name.lower()
        if filters and lowered_name not in filters:
            continue
        already_localized = lowered_name in localized_name_set
        if already_localized and not args.include_present:
            continue

        object_entry, pending_strings = extract_object_data(obj, base_index)
        if not object_entry["parts"]:
            continue

        object_entry["status"] = "missing" if not already_localized else "present"
        object_entry["already_localized"] = already_localized
        object_entry["localized_index"] = localized_names.get(lowered_name)

        objects_payload.append(object_entry)
        strings_payload.extend(pending_strings)

        if args.max_objects and len(objects_payload) >= args.max_objects:
            break

    output = {
        "generated_at": datetime.now(timezone.utc).isoformat(),
        "base_file": normalize_path(base_file),
        "localized_file": normalize_path(localized_file),
        "localized_exists": localized_exists,
        "stats": {
            "base_object_count": total_objects,
            "localized_object_count": len(localized_name_set),
            "objects_in_payload": len(objects_payload),
            "strings_pending": len(strings_payload),
        },
        "translatable_attribute_rules": {
            "exact": sorted(TRANSLATABLE_EXACT),
            "substrings": list(TRANSLATABLE_SUBSTRINGS),
        },
        "objects": objects_payload,
        "pending_strings": strings_payload,
    }

    if args.output:
        args.output.parent.mkdir(parents=True, exist_ok=True)
        args.output.write_text(json.dumps(output, ensure_ascii=False, indent=2), encoding="utf-8")
        print(f"Wrote {len(objects_payload)} objects / {len(strings_payload)} strings to {args.output}")
    else:
        print(json.dumps(output, ensure_ascii=False, indent=2))


if __name__ == "__main__":
    main()
