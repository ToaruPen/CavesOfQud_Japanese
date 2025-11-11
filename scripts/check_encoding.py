#!/usr/bin/env python3
"""
Scan text files for common mojibake sequences (繧/縺/蜑/...).
"""

from __future__ import annotations

import argparse
import re
import sys
from pathlib import Path
from typing import Iterable, List, Sequence, Tuple, Set

MOJIBAKE_PATTERN = re.compile(r"[繧縺蜑菴譛螳鬘讖蝨驛遘莨蟾驕髢遉鬮遯霑霆鬟閧閻]")
DEFAULT_PATHS = ("Docs", "Mods/QudJP")
DEFAULT_EXTENSIONS = (".md", ".xml", ".txt", ".csv")
REPO_ROOT = Path(__file__).resolve().parents[1]
DEFAULT_IGNORES = {
    Path("Docs/utf8_safety.md"),
    Path("Docs/translation_process.md"),
}


def resolve_ignore(path: Path) -> Path:
    return path if path.is_absolute() else (REPO_ROOT / path)


def iter_candidate_files(paths: Sequence[Path], extensions: Sequence[str], ignore: Set[Path]) -> Iterable[Path]:
    lowered = {ext.lower() for ext in extensions}
    for root in paths:
        if not root.exists():
            print(f"[WARN] Skipping missing path: {root}", file=sys.stderr)
            continue
        for path in root.rglob("*"):
            if path.is_file() and path.suffix.lower() in lowered:
                if path.resolve() in ignore:
                    continue
                yield path


def scan_file(path: Path) -> Tuple[bool, str]:
    data = path.read_bytes()
    if not data:
        return False, ""

    text = data.decode("utf-8", errors="ignore")
    match = MOJIBAKE_PATTERN.search(text)
    if not match:
        return False, ""

    for line in text.splitlines():
        if MOJIBAKE_PATTERN.search(line):
            return True, line.strip()[:120]
    return True, ""


def build_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(description="Scan files for mojibake sequences.")
    parser.add_argument(
        "--path",
        action="append",
        dest="paths",
        type=Path,
        help=f"Root path to scan (defaults to {', '.join(DEFAULT_PATHS)}). May be specified multiple times.",
    )
    parser.add_argument(
        "--extension",
        action="append",
        dest="extensions",
        help=f"File extension to include (defaults to {', '.join(DEFAULT_EXTENSIONS)}).",
    )
    parser.add_argument(
        "--ignore",
        action="append",
        dest="ignore",
        type=Path,
        help="Path to ignore (relative to repo root). May be specified multiple times.",
    )
    parser.add_argument(
        "--fail-on-issues",
        action="store_true",
        help="Exit with status 1 if any suspicious sequences are found.",
    )
    return parser


def main() -> None:
    parser = build_parser()
    args = parser.parse_args()

    def normalize_scan_root(raw: Path | str) -> Path:
        path = Path(raw)
        repo_path = (REPO_ROOT / path).expanduser()
        return repo_path.resolve()

    paths = [normalize_scan_root(p) for p in (args.paths or DEFAULT_PATHS)]
    extensions = args.extensions or DEFAULT_EXTENSIONS
    ignore: Set[Path] = {resolve_ignore(p).resolve() for p in DEFAULT_IGNORES}
    if args.ignore:
        for entry in args.ignore:
            ignore.add(resolve_ignore(entry).resolve())

    issues: List[Tuple[Path, str]] = []
    for file_path in iter_candidate_files(paths, extensions, ignore):
        has_issue, snippet = scan_file(file_path)
        if has_issue:
            issues.append((file_path, snippet))

    if not issues:
        print("✅ No mojibake-style sequences detected.")
        raise SystemExit(0)

    print(f"⚠️  Detected {len(issues)} file(s) with suspicious sequences:\n")
    for file_path, snippet in issues:
        print(f"- {file_path}")
        if snippet:
            print(f"  {snippet}")
    if args.fail_on_issues:
        raise SystemExit(1)


if __name__ == "__main__":
    main()
