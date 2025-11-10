import xml.etree.ElementTree as ET
from pathlib import Path
root = ET.fromstring(Path(r"tmp/eskhind_base.xml").read_text(encoding="utf-8"))
stay = root.find('.//start[@ID="StayLong"]')
print(ET.tostring(stay, encoding='unicode'))
