#!/usr/bin/env python3
"""
Tail Player.log (or any chosen log file) with optional filtering and mirroring to disk.
"""

from __future__ import annotations

import argparse
import os
import re
import sys
import time
from collections import deque
from pathlib import Path
from typing import Iterable, Optional, TextIO

DEFAULT_PATTERN = r"\[JP\]|\bException\b|TMP_Text|SelectableTextMenuItem|Tooltip|Popup"
DEFAULT_POLL_INTERVAL = 0.25


def default_log_path() -> Path:
    home = Path.home()
    return home / "AppData/LocalLow/Freehold Games/CavesOfQud/Player.log"


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Watch Player.log with optional filtering.")
    parser.add_argument(
        "--log-path",
        type=Path,
        default=None,
        help="Log file to watch (default: Player.log under LocalLow).",
    )
    parser.add_argument(
        "--tail",
        type=int,
        default=80,
        metavar="N",
        help="Number of existing lines to show before following (default: 80).",
    )
    parser.add_argument(
        "--pattern",
        default=DEFAULT_PATTERN,
        help="Regex pattern used to filter lines (default focuses on JP/Exception logs).",
    )
    parser.add_argument(
        "--no-filter",
        action="store_true",
        help="Show every line without applying the regex filter.",
    )
    parser.add_argument(
        "--once",
        action="store_true",
        help="Print the current tail and exit without waiting for new lines.",
    )
    parser.add_argument(
        "--output",
        type=Path,
        default=None,
        help="Optional file to append the filtered output to.",
    )
    parser.add_argument(
        "--interval",
        type=float,
        default=DEFAULT_POLL_INTERVAL,
        help="Polling interval in seconds when waiting for new lines (default: 0.25).",
    )
    return parser.parse_args()


def ensure_log_path(path: Path) -> Path:
    resolved = path.expanduser().resolve()
    if not resolved.exists():
        raise FileNotFoundError(
            f"Log file '{resolved}' does not exist. Launch the game once to create Player.log."
        )
    return resolved


def open_output_file(path: Optional[Path]) -> Optional[TextIO]:
    if path is None:
        return None

    resolved = path.expanduser()
    if not resolved.is_absolute():
        resolved = Path.cwd() / resolved

    resolved.parent.mkdir(parents=True, exist_ok=True)
    return resolved.open("a", encoding="utf-8")


def emit_line(
    line: str,
    *,
    stdout: TextIO,
    sink: Optional[TextIO],
) -> None:
    stdout.write(line)
    stdout.flush()
    if sink is not None:
        sink.write(line)
        sink.flush()


def show_tail(
    handle: TextIO,
    limit: int,
) -> Iterable[str]:
    if limit <= 0:
        return []

    handle.seek(0, os.SEEK_SET)
    buffer: deque[str] = deque(maxlen=limit)
    for line in handle:
        buffer.append(line)
    return list(buffer)


def follow_file(
    handle: TextIO,
    path: Path,
    *,
    interval: float,
) -> Iterable[str]:
    while True:
        position = handle.tell()
        line = handle.readline()
        if line:
            yield line
            continue

        yield None  # Indicates no new data available right now.
        time.sleep(interval)

        try:
            size = path.stat().st_size
        except FileNotFoundError:
            # Wait until the file reappears.
            time.sleep(interval)
            continue

        if size < position:
            # File was truncated or rotated; reopen from start.
            handle.close()
            time.sleep(interval)
            handle = path.open("r", encoding="utf-8", errors="ignore")


def main() -> None:
    args = parse_args()
    log_path = ensure_log_path(args.log_path or default_log_path())
    regex: Optional[re.Pattern[str]] = None
    if not args.no_filter and args.pattern:
        regex = re.compile(args.pattern)

    with log_path.open("r", encoding="utf-8", errors="ignore") as handle, open_output_file(args.output) as output:
        for line in show_tail(handle, args.tail):
            if regex and not regex.search(line):
                continue
            emit_line(line, stdout=sys.stdout, sink=output)

        if args.once:
            return

        for line in follow_file(handle, log_path, interval=args.interval):
            if line is None:
                continue

            if regex and not regex.search(line):
                continue

            emit_line(line, stdout=sys.stdout, sink=output)


if __name__ == "__main__":
    try:
        main()
    except KeyboardInterrupt:
        pass
