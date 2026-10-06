from pathlib import Path
import xml.etree.ElementTree as ET

ROOT = Path(__file__).resolve().parents[2] / '1.6' / 'Defs'


#职责：替换指定节点的子内容，保持重复执行时结果一致。
def replace(parent, tag, xml):
    previous = parent.find(tag)
    if previous is not None:
        parent.remove(previous)
    parent.append(ET.fromstring(xml))


#职责：保存生成配置并保留中文与 UTF-8 编码。
def save(tree, path):
    ET.indent(tree, space='  ')
    tree.write(path, encoding='utf-8', xml_declaration=True)


#职责：建立亲卫、队长和提亚娜的实际基因组合及固定生成背景。
def main():
    genes_path = ROOT / 'GeneDefs' / 'Gene_Miho.xml'
    genes = ET.parse(genes_path)
    root = genes.getroot()
    standard = root.find("GeneDef[defName='Gene_MihoPhase2_FirstStandard']/statFactors")
    damage = standard.find('IncomingDamageFactor')
    if damage is not None:
        standard.remove(damage)
    first = root.find("GeneDef[defName='Gene_MihoPhase2_FirstStandard']/biostatArc")
    first.text = '0'
    helmer = root.find("GeneDef[defName='Gene_MihoPhase2_Experiment45']/statFactors")
    if helmer.find('StaggerDurationFactor') is None:
        ET.SubElement(helmer, 'StaggerDurationFactor').text = '0'
    for name, additions in [
        ('Xeno_MihoPhase2_Tiana', ['Gene_MihoPhase2_Experiment317']),
        ('Xeno_MihoPhase2_HelmerZog', ['Gene_MihoPhase2_Experiment325', 'Gene_MihoPhase2_Experiment4'])
    ]:
        entries = root.find(f"XenotypeDef[defName='{name}']/genes")
        for entry in additions:
            if entry not in [li.text for li in entries]:
                ET.SubElement(entries, 'li').text = entry
    guard_name = 'Xeno_MihoPhase2_LeaderGuard'
    if root.find(f"XenotypeDef[defName='{guard_name}']") is None:
        guard = ET.SubElement(root, 'XenotypeDef')
        for key, value in [('defName', guard_name), ('label', '元首亲卫血统'), ('iconPath', 'Fc/enp'), ('inheritable', 'false')]:
            ET.SubElement(guard, key).text = value
        entries = ET.SubElement(guard, 'genes')
        for li in root.find("XenotypeDef[defName='Xeno_MihoPhase2_ImperialMiho']/genes"):
            ET.SubElement(entries, 'li').text = li.text
        ET.SubElement(entries, 'li').text = 'Gene_MihoPhase2_Experiment4'
    save(genes, genes_path)

    kinds_path = ROOT / 'PawnKindDefs' / 'PawnKinds_MihoPhase2_Military.xml'
    kinds = ET.parse(kinds_path)
    for kind, xeno in [('LeaderGuard', guard_name), ('LeaderGuardCaptain', 'Xeno_MihoPhase2_HelmerZog')]:
        node = kinds.getroot().find(f"PawnKindDef[defName='MihoPhase2_{kind}']")
        replace(node, 'xenotypeSet', f'<xenotypeSet Inherit="False"><xenotypeChances><{xeno}>1</{xeno}></xenotypeChances></xenotypeSet>')
        if kind == 'LeaderGuardCaptain':
            for stage in ['Child', 'Adult']:
                story = 'Childhood' if stage == 'Child' else 'Adulthood'
                replace(node, f'fixed{stage}Backstories', f'<fixed{stage}Backstories Inherit="False"><li>MihoPhase2_{story}_Helmer</li></fixed{stage}Backstories>')
            for tag in ['minGenerationAge', 'maxGenerationAge']:
                replace(node, tag, f'<{tag}>74</{tag}>')
    save(kinds, kinds_path)


if __name__ == '__main__':
    main()
