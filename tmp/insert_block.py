import sys
from pathlib import Path
if len(sys.argv) != 2:
    raise SystemExit("usage: insert_block.py <blockfile>")
block = Path(sys.argv[1]).read_text(encoding="utf-8")
path = Path(r"Mods/QudJP/Localization/Conversations.jp.xml")
text = path.read_text(encoding="utf-8")
marker = "  <!-- Gritgate Intercom -->"
idx = text.find(marker)
if idx == -1:
    raise SystemExit("marker not found")
text = text[:idx] + block + "\n" + text[idx:]
path.write_text(text, encoding="utf-8")
