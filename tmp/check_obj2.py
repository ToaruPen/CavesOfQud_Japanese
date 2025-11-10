import re
import xml.etree.ElementTree as ET
from pathlib import Path
INVALID_XML_REF = re.compile(r"&#(x[0-9a-fA-F]+|\d+);")
INVALID_XML_CHAR = re.compile(r"[\x00-\x08\x0B\x0C\x0E-\x1F]")

def sanitize_xml(text: str) -> str:
    def replace_ref(match):
        value = match.group(1)
        codepoint = int(value[1:], 16) if value.startswith('x') else int(value)
        if codepoint in (0x9, 0xA, 0xD) or codepoint >= 0x20:
            return match.group(0)
        return ''
    text = INVALID_XML_REF.sub(replace_ref, text)
    return INVALID_XML_CHAR.sub('', text)

text = Path('Mods/QudJP/Localization/ObjectBlueprints/Creatures.jp.xml').read_text(encoding='utf-8')
root = ET.fromstring(sanitize_xml(text))
objs = root.findall('./object')
names = {elem.attrib.get('Name'): idx for idx, elem in enumerate(objs)}
print('count', len(objs))
print('Mayor Nuntu' in names)
print(names.get('Mayor Nuntu'))
