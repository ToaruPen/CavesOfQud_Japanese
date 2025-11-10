import sys
import xml.etree.ElementTree as ET
from pathlib import Path
conv_id = sys.argv[1]
root = ET.parse('references/Base/Conversations.xml').getroot()
for conv in root.findall('conversation'):
    if conv.get('ID') == conv_id:
        print(ET.tostring(conv, encoding='unicode'))
        break
else:
    raise SystemExit(f"conversation {conv_id} not found")
