#!/usr/bin/env python3
"""
Generate localization coverage report by comparing references/Base and Mods/QudJP/Localization.
"""

from __future__ import annotations

import argparse
import json
import re
import sys
import xml.etree.ElementTree as ET
from collections import Counter
from pathlib import Path
from typing import Dict, List


def get_localized_path(base_path: Path, localization_path: Path, base_file: Path) -> Path:
    relative = base_file.relative_to(base_path)
    localized_name = relative.stem + ".jp" + relative.suffix
    return localization_path / relative.parent / localized_name


INVALID_XML_REF = re.compile(r"&#(x[0-9a-fA-F]+|\d+);")
INVALID_XML_CHAR = re.compile(r"[\x00-\x08\x0B\x0C\x0E-\x1F]")
TRANSLATABLE_ATTRS = {
    "displayname",
    "short",
    "long",
    "description",
    "text",
    "string",
    "message",
    "label",
    "tooltip",
    "singular",
    "plural",
}


def sanitize_xml(text: str) -> str:
    def replace_ref(match: re.Match[str]) -> str:
        value = match.group(1)
        codepoint = int(value[1:], 16) if value.startswith("x") else int(value)
        if codepoint in (0x9, 0xA, 0xD) or codepoint >= 0x20:
            return match.group(0)
        return ""

    text = INVALID_XML_REF.sub(replace_ref, text)
    return INVALID_XML_CHAR.sub("", text)


def collect_object_names(path: Path) -> List[str]:
    try:
        text = sanitize_xml(path.read_text(encoding="utf-8-sig"))
        root = ET.fromstring(text)
    except Exception as exc:  # noqa: BLE001
        print(f"[WARN] Failed to parse XML '{path}': {exc}", file=sys.stderr)
        return []

    cache: Dict[int, bool] = {}

    def has_translatable_text(elem: ET.Element) -> bool:
        key = id(elem)
        cached = cache.get(key)
        if cached is not None:
            return cached

        for attr_name, attr_value in elem.attrib.items():
            if attr_name.lower() in TRANSLATABLE_ATTRS and attr_value.strip():
                cache[key] = True
                return True

        if (elem.text or "").strip():
            cache[key] = True
            return True

        for child in list(elem):
            if has_translatable_text(child):
                cache[key] = True
                return True

        cache[key] = False
        return False

    names: List[str] = []
    for elem in root.iter():
        name = elem.attrib.get("Name") or elem.attrib.get("ID") or elem.attrib.get("Id")
        if name:
            if not has_translatable_text(elem):
                continue
            names.append(name)
    return names


def build_report(base_path: Path, localization_path: Path, base_filters: List[str] | None = None) -> List[Dict[str, str]]:
    report: List[Dict[str, str]] = []
    filters = [flt.lower() for flt in base_filters] if base_filters else []

    for base_file in sorted(base_path.rglob("*")):
        if not base_file.is_file():
            continue
        if base_file.suffix.lower() not in {".xml", ".txt"}:
            continue

        relative_base = base_file.relative_to(base_path).as_posix()
        if filters:
            relative_lower = relative_base.lower()
            filename_lower = base_file.name.lower()

            def matches(filter_value: str) -> bool:
                if "/" in filter_value or "\\" in filter_value:
                    return filter_value in relative_lower
                return filename_lower == filter_value

            if not any(matches(filter_value) for filter_value in filters):
                continue

        localized = get_localized_path(base_path, localization_path, base_file)
        relative_localized = localized.relative_to(localization_path).as_posix()

        if not localized.exists():
            report.append(
                {
                    "BaseFile": relative_base,
                    "Localized": relative_localized,
                    "ObjectName": None,
                    "Status": "file-missing",
                }
            )
            continue

        if base_file.suffix.lower() != ".xml":
            report.append(
                {"BaseFile": relative_base, "Localized": relative_localized, "ObjectName": None, "Status": "ok"}
            )
            continue

        base_names = collect_object_names(base_file)
        if not base_names:
            report.append(
                {"BaseFile": relative_base, "Localized": relative_localized, "ObjectName": None, "Status": "ok"}
            )
            continue

        localized_names = set(name.lower() for name in collect_object_names(localized))
        missing = [name for name in base_names if name.lower() not in localized_names]

        if not missing:
            report.append(
                {"BaseFile": relative_base, "Localized": relative_localized, "ObjectName": None, "Status": "ok"}
            )
        else:
            for name in missing:
                report.append(
                    {"BaseFile": relative_base, "Localized": relative_localized, "ObjectName": name, "Status": "object-missing"}
                )
    return report


def print_table(entries: List[Dict[str, str]]) -> None:
    if not entries:
        print("No entries.")
        return

    col_widths = {
        "Status": max(len("Status"), max(len(entry["Status"]) for entry in entries)),
        "BaseFile": max(len("BaseFile"), max(len(entry["BaseFile"]) for entry in entries)),
    }

    header = f"{'Status'.ljust(col_widths['Status'])}  {'BaseFile'.ljust(col_widths['BaseFile'])}  Object"
    print(header)
    print("-" * len(header))
    for entry in entries:
        object_name = entry["ObjectName"] or ""
        print(
            f"{entry['Status'].ljust(col_widths['Status'])}  "
            f"{entry['BaseFile'].ljust(col_widths['BaseFile'])}  "
            f"{object_name}"
        )


def summarize(report: List[Dict[str, str]]) -> None:
    counts = Counter(entry["Status"] for entry in report)
    print()
    for status, count in sorted(counts.items()):
        print(f"{status:<16}: {count:4}")


def build_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(description="Compare base data with localization files.")
    parser.add_argument(
        "--base-path",
        type=Path,
        default=Path(__file__).resolve().parents[1] / "references" / "Base",
        help="Path to references/Base (default: %(default)s)",
    )
    parser.add_argument(
        "--localization-path",
        type=Path,
        default=Path(__file__).resolve().parents[1] / "Mods" / "QudJP" / "Localization",
        help="Path to Mods/QudJP/Localization (default: %(default)s)",
    )
    parser.add_argument(
        "--base",
        action="append",
        dest="base_filters",
        help="Only inspect base files whose names (or relative paths) match the given values. Can be repeated.",
    )
    parser.add_argument(
        "--missing-only",
        action="store_true",
        help="Display only entries where Status != ok.",
    )
    parser.add_argument(
        "--json-path",
        type=Path,
        help="Optional path to write the full report as JSON.",
    )
    return parser


def main() -> None:
    parser = build_parser()
    args = parser.parse_args()

    base_path = args.base_path.expanduser()
    localization_path = args.localization_path.expanduser()

    if not base_path.exists():
        parser.error(f"Base path not found: {base_path}")
    if not localization_path.exists():
        parser.error(f"Localization path not found: {localization_path}")

    report = build_report(base_path, localization_path, args.base_filters)
    filtered = [entry for entry in report if entry["Status"] != "ok"] if args.missing_only else report

    print_table(filtered)
    summarize(report)

    if args.json_path:
        args.json_path.parent.mkdir(parents=True, exist_ok=True)
        args.json_path.write_text(json.dumps(report, ensure_ascii=False, indent=2), encoding="utf-8")
        print(f"\nReport written to {args.json_path}")


if __name__ == "__main__":
    main()
