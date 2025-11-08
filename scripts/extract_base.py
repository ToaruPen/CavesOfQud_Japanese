#!/usr/bin/env python3
"""
Mirror the game's StreamingAssets/Base folder into references/Base.

Usage:
    python scripts/extract_base.py --game-path "C:\\Program Files..."
"""

from __future__ import annotations

import argparse
import shutil
import sys
from pathlib import Path

TARGETS = [
    "Conversations.xml",
    "Books.xml",
    "Commands.xml",
    "EmbarkModules.xml",
    "Manual.xml",
    "Options.xml",
    "Mutations.xml",
    "Corpus",
    "ObjectBlueprints",
]

CANDIDATE_BASE_SUBPATHS = [
    Path("StreamingAssets/Base"),
    Path("CoQ_Data/StreamingAssets/Base"),
    Path("CoQ.app/Contents/Resources/Data/StreamingAssets/Base"),
    Path("Contents/Resources/Data/StreamingAssets/Base"),
]


def find_base_root(game_path: Path) -> Path:
    for relative in CANDIDATE_BASE_SUBPATHS:
        candidate = (game_path / relative).expanduser()
        if candidate.exists():
            return candidate.resolve()
    rel_list = "\n  - ".join(str(p) for p in CANDIDATE_BASE_SUBPATHS)
    raise SystemExit(
        f"Could not locate StreamingAssets/Base under '{game_path}'.\n"
        "Checked:\n"
        f"  - {rel_list}\n"
        "Pass the directory that contains CoQ_Data or CoQ.app via --game-path."
    )


def copy_item(source: Path, destination: Path) -> None:
    if source.is_dir():
        if destination.exists():
            shutil.rmtree(destination)
        shutil.copytree(source, destination)
    else:
        destination.parent.mkdir(parents=True, exist_ok=True)
        shutil.copy2(source, destination)


def extract(game_path: Path, output_path: Path) -> None:
    base_root = find_base_root(game_path)
    output_path.mkdir(parents=True, exist_ok=True)

    print(f"GamePath   : {game_path}")
    print(f"Base data  : {base_root}")
    print(f"OutputPath : {output_path}")

    for target in TARGETS:
        source = base_root / target
        if not source.exists():
            print(f"[WARN] Missing {source}", file=sys.stderr)
            continue
        destination = output_path / target
        print(f"Copying {target}")
        copy_item(source, destination)

    print("Base extraction completed.")


def build_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(description="Copy StreamingAssets/Base into references/Base.")
    parser.add_argument(
        "--game-path",
        type=Path,
        required=True,
        help="Path to the Caves of Qud installation directory (the folder containing CoQ_Data or CoQ.app).",
    )
    parser.add_argument(
        "--output-path",
        type=Path,
        default=Path(__file__).resolve().parents[1] / "references" / "Base",
        help="Destination for extracted data (defaults to references/Base).",
    )
    return parser


def main() -> None:
    parser = build_parser()
    args = parser.parse_args()
    extract(args.game_path.expanduser(), args.output_path.expanduser())


if __name__ == "__main__":
    main()
