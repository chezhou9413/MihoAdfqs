from pathlib import Path
import xml.etree.ElementTree as ET

#职责：将即时药物行为接入既有配方，并校正意识维持描述。
def main():
    root = Path(__file__).resolve().parents[2] / '1.6/Defs/Phase2/Medicine'
    path = root/'Injectors.xml'
    tree = ET.parse(path)
    for thing in tree.findall('ThingDef'):
        name = thing.findtext('defName', '')
        doers = thing.find('ingestible/outcomeDoers')
        if doers is None or 'Activation' in name:
            continue
        cls = 'MihoAdfqs.Phase2.Medicine.IngestionOutcomeDoer_Phase2'
        if any(d.get('Class') == cls for d in doers):
            continue
        doer = ET.SubElement(doers, 'li', {'Class':cls})
        if any(s in name for s in ('Treatment','Stimulant','Emergency')):
            ET.SubElement(doer,'heal').text = 'true'
        if 'Emergency' in name:
            ET.SubElement(doer,'restore').text = 'true'
        if 'Conversion' in name:
            ET.SubElement(doer,'convert').text = 'true'
    ET.indent(tree, space='  ')
    tree.write(path, encoding='utf-8', xml_declaration=True)
    path = root/'InjectorHediffs.xml'
    tree = ET.parse(path)
    for hediff in tree.findall('HediffDef'):
        for cap in hediff.findall('./stages/li/capMods/li'):
            if cap.findtext('capacity') == 'Consciousness':
                node = cap.find('setMax')
                if node is None:
                    node = ET.SubElement(cap,'setMax')
                node.text = '1'
        if hediff.findtext('defName') == 'MihoPhase2_EmergencyInjectorEffect':
            hediff.find('description').text = '注射时重建所有断裂肢体并修复伤口。六小时内维持意识、消除疼痛与出血，并以五倍基础速率消耗营养。'
        if hediff.findtext('defName') == 'MihoPhase2_HumaneConversionInjectorEffect':
            hediff.find('description').text = '同化药剂将目标随机转化为帝国美狐或帝国米莉拉，保留身份和记忆。医疗舱可以指定转化种族。'
    ET.indent(tree, space='  ')
    tree.write(path, encoding='utf-8', xml_declaration=True)

if __name__ == '__main__':
    main()
