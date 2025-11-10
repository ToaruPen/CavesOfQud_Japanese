import xml.etree.ElementTree as ET
from pathlib import Path
text = Path('Mods/QudJP/Localization/ObjectBlueprints/Creatures.jp.xml').read_text(encoding='utf-8')
root = ET.fromstring(text)
objs = root.findall('./object')
names = {elem.attrib.get('Name') for elem in objs}
print('count', len(objs))
print('Mayor Nuntu' in names)
