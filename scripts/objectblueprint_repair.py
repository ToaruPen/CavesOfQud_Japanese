#!/usr/bin/env python3
"""
Rebuild Mods/QudJP/Localization/ObjectBlueprints/*.jp.xml so they contain
the complete object definitions instead of the truncated snippets produced
by older insertion scripts.
"""

from __future__ import annotations

import argparse
from copy import deepcopy
from pathlib import Path
from typing import Dict, List, Optional, Tuple
import re
import xml.etree.ElementTree as ET

REPO_ROOT = Path(__file__).resolve().parents[1]
BASE_ROOT = REPO_ROOT / "references" / "Base" / "ObjectBlueprints"
LOCALIZATION_ROOT = REPO_ROOT / "Mods" / "QudJP" / "Localization" / "ObjectBlueprints"

INVALID_XML_REF = re.compile(r"&#(x[0-9a-fA-F]+|\d+);")
INVALID_XML_CHAR = re.compile(r"[\x00-\x08\x0B\x0C\x0E-\x1F]")


def resolve_path(path_str: Optional[str]) -> Path:
    if not path_str:
        raise SystemExit("A path argument is required.")
    path = Path(path_str)
    if not path.is_absolute():
        path = (REPO_ROOT / path).resolve()
    return path


def infer_base_path(localized_path: Path) -> Path:
    if localized_path.parts[-1].endswith(".jp.xml"):
        base_name = localized_path.name.replace(".jp.xml", ".xml")
    else:
        raise SystemExit(f"Unable to infer base filename from {localized_path}")
    return (BASE_ROOT / base_name).resolve()


def sanitize_xml(text: str) -> str:
    def replace_ref(match: re.Match[str]) -> str:
        value = match.group(1)
        codepoint = int(value[1:], 16) if value.startswith("x") else int(value)
        if codepoint in (0x9, 0xA, 0xD) or codepoint >= 0x20:
            return match.group(0)
        return ""

    text = INVALID_XML_REF.sub(replace_ref, text)
    return INVALID_XML_CHAR.sub("", text)


def load_object_map(path: Path) -> Dict[str, ET.Element]:
    if not path.exists():
        raise SystemExit(f"XML file not found: {path}")
    text = path.read_text(encoding="utf-8-sig")
    sanitized = sanitize_xml(text)
    root = ET.fromstring(sanitized)
    mapping: Dict[str, ET.Element] = {}
    for elem in root.findall("./object"):
        name = elem.attrib.get("Name")
        if not name or name in mapping:
            continue
        mapping[name] = elem
    return mapping


def indent_element(elem: ET.Element, level: int = 0, space: str = "  ") -> None:
    children = list(elem)
    if not children:
        return
    indent_text = "\n" + space * (level + 1)
    for child in children:
        child.tail = indent_text
        indent_element(child, level + 1, space)
    children[-1].tail = "\n" + space * level


def match_children(base_children: List[ET.Element], localized_children: List[ET.Element]) -> List[Tuple[ET.Element, ET.Element]]:
    used = [False] * len(base_children)
    pairs: List[Tuple[ET.Element, ET.Element]] = []
    for loc in localized_children:
        name = loc.attrib.get("Name")
        idx = None
        if name:
            for i, base_child in enumerate(base_children):
                if used[i]:
                    continue
                if base_child.tag == loc.tag and base_child.attrib.get("Name") == name:
                    idx = i
                    break
        if idx is None:
            for i, base_child in enumerate(base_children):
                if used[i]:
                    continue
                if base_child.tag == loc.tag:
                    idx = i
                    break
        if idx is None:
            raise SystemExit(f"Failed to match child <{loc.tag}> while repairing object '{loc.attrib.get('Name','')}'.")
        used[idx] = True
        pairs.append((base_children[idx], loc))
    return pairs


def clone_with_overrides(base_obj: ET.Element, localized_obj: ET.Element) -> ET.Element:
    merged = deepcopy(base_obj)
    for attr, value in localized_obj.attrib.items():
        if attr in {"Name", "Inherits"}:
            continue
        merged.set(attr, value)
    if "Load" in merged.attrib:
        del merged.attrib["Load"]
    merged.set("Replace", "true")

    base_children = list(merged)
    localized_children = [child for child in localized_obj if isinstance(child.tag, str)]
    if localized_children:
        for base_child, loc_child in match_children(base_children, localized_children):
            for attr, value in loc_child.attrib.items():
                if attr == "Name":
                    continue
                base_child.set(attr, value)
    return merged


def repair_file(base_path: Path, localized_path: Path, output_path: Optional[Path] = None) -> None:
    base_objects = load_object_map(base_path)
    localized_text = localized_path.read_text(encoding="utf-8-sig")
    localized_root = ET.fromstring(sanitize_xml(localized_text))

    new_root = ET.Element(localized_root.tag, localized_root.attrib)
    for child in localized_root:
        if child.tag != "object":
            new_root.append(deepcopy(child))
            continue

        name = child.attrib.get("Name")
        if not name:
            continue
        base_obj = base_objects.get(name)
        if base_obj is None:
            # Object exists only in the localized file (custom content). Keep as-is.
            new_root.append(deepcopy(child))
            continue
        new_root.append(clone_with_overrides(base_obj, child))

    tree = ET.ElementTree(new_root)
    try:
        ET.indent(tree, space="  ", level=0)  # type: ignore[attr-defined]
    except AttributeError:
        indent_element(new_root, level=0)

    destination = output_path or localized_path
    tree.write(destination, encoding="utf-8", xml_declaration=True)
    print(f"Repaired {localized_path} -> {destination}")


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Restore truncated ObjectBlueprint JP files to full definitions.")
    parser.add_argument("--localized", type=str, required=True, help="Path to the JP XML file to repair.")
    parser.add_argument(
        "--base",
        type=str,
        help="Path to the base ObjectBlueprints XML. Defaults to references/Base/ObjectBlueprints/<name>.xml.",
    )
    parser.add_argument("--output", type=str, help="Optional output path. Defaults to in-place overwrite.")
    return parser.parse_args()


def main() -> None:
    args = parse_args()
    localized_path = resolve_path(args.localized)
    base_path = resolve_path(args.base) if args.base else infer_base_path(localized_path)
    output_path = resolve_path(args.output) if args.output else None
    repair_file(base_path, localized_path, output_path)


if __name__ == "__main__":
    main()
