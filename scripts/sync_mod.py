#!/usr/bin/env python3
"""
Mirror Mods/QudJP into the live Mods folder used by the game client.
"""

from __future__ import annotations

import argparse
import filecmp
import os
import platform
import shutil
from pathlib import Path
from typing import Iterable, List, Set

EXCLUDE_DEFAULT = {"obj", ".git", ".vs", "bin"}


def default_source_path() -> Path:
    return Path(__file__).resolve().parents[1] / "Mods" / "QudJP"


def default_target_path() -> Path:
    home = Path.home()
    system = platform.system()
    if system == "Windows":
        return home / "AppData/LocalLow/Freehold Games/CavesOfQud/Mods/QudJP"
    if system == "Darwin":
        return home / "Library/Application Support/Freehold Games/CavesOfQud/Mods/QudJP"
    return home / ".config/unity3d/Freehold Games/CavesOfQud/Mods/QudJP"


def should_exclude(relative_path: Path, excluded_names: Set[str]) -> bool:
    return any(part in excluded_names for part in relative_path.parts if part not in (".", ".."))


def iter_source_files(source: Path, excluded_names: Set[str]) -> Iterable[Path]:
    for root, dirs, files in os.walk(source):
        rel_dir = Path(root).relative_to(source)
        dirs[:] = [d for d in dirs if not should_exclude(rel_dir / d, excluded_names)]
        for file_name in files:
            relative = rel_dir / file_name
            if should_exclude(relative, excluded_names):
                continue
            yield relative


def copy_file(source: Path, target: Path, dry_run: bool) -> None:
    if dry_run:
        print(f"[COPY] {source} -> {target}")
        return
    target.parent.mkdir(parents=True, exist_ok=True)
    shutil.copy2(source, target)


def remove_path(path: Path, dry_run: bool) -> None:
    if dry_run:
        print(f"[REMOVE] {path}")
        return
    if path.is_dir():
        shutil.rmtree(path)
    else:
        path.unlink(missing_ok=True)


def sync(source: Path, target: Path, dry_run: bool, exclude_fonts: bool) -> None:
    excluded = set(EXCLUDE_DEFAULT)
    if exclude_fonts:
        excluded.add("Fonts")

    print(f"Source : {source}")
    print(f"Target : {target}")
    print(f"Mode   : {'dry-run' if dry_run else 'mirror'}")

    if not source.exists():
        raise SystemExit(f"Source path '{source}' does not exist.")
    target.mkdir(parents=True, exist_ok=True)

    tracked_files: Set[Path] = set()
    tracked_dirs: Set[Path] = {Path(".")}

    for relative in iter_source_files(source, excluded):
        tracked_files.add(relative)
        tracked_dirs.add(relative.parent)

        source_file = source / relative
        target_file = target / relative

        needs_copy = True
        if target_file.exists() and target_file.is_file():
            try:
                needs_copy = not filecmp.cmp(source_file, target_file, shallow=False)
            except OSError:
                needs_copy = True
        if needs_copy:
            copy_file(source_file, target_file, dry_run)

    # Remove orphaned files
    for root, dirs, files in os.walk(target, topdown=False):
        rel_dir = Path(root).relative_to(target)
        if should_exclude(rel_dir, excluded):
            continue

        for file_name in files:
            relative = rel_dir / file_name
            if should_exclude(relative, excluded) or relative in tracked_files:
                continue
            remove_path(Path(root) / file_name, dry_run)

        for dir_name in dirs:
            relative = rel_dir / dir_name
            dir_path = Path(root) / dir_name
            if should_exclude(relative, excluded):
                continue
            if relative not in tracked_dirs and not any(dir_path.iterdir()):
                remove_path(dir_path, dry_run)

    print("Sync completed.")


def build_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(description="Mirror Mods/QudJP into the live Mods folder.")
    parser.add_argument(
        "--source",
        type=Path,
        default=default_source_path(),
        help="Source mod folder (default: repository Mods/QudJP).",
    )
    parser.add_argument(
        "--target",
        type=Path,
        default=default_target_path(),
        help="Destination mod folder (defaults to the platform-specific Mods directory).",
    )
    parser.add_argument(
        "--dry-run",
        action="store_true",
        help="List actions without copying or deleting files.",
    )
    parser.add_argument(
        "--exclude-fonts",
        action="store_true",
        help="Skip the Fonts directory (useful if the runtime already generated fonts).",
    )
    return parser


def main() -> None:
    parser = build_parser()
    args = parser.parse_args()
    sync(args.source.expanduser(), args.target.expanduser(), args.dry_run, args.exclude_fonts)


if __name__ == "__main__":
    main()
