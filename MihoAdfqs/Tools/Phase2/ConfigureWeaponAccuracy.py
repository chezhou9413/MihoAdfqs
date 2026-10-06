from pathlib import Path
import xml.etree.ElementTree as ET

ROOT = Path(__file__).resolve().parents[2] / '1.6' / 'Defs' / 'Weapons'
P = 'MihoPhase2_'
CURVES = {
    'Weapon_AshTwelve': [(20, .95), (40, .8), (60, .75)],
    'Gun_HKFourOneSix': [(20, .95), (40, .8), (65, .75)],
    'Gun_LugerP08_Charge': [(5, .95), (20, .8), (35, .75)],
    'Gun_MG42_Charge': [(20, .65), (40, .7), (95, .35)],
    'Gun_Pzb38_Charge': [(10, .25), (60, .9), (120, .75)],
    'Weapon_Q11Improved': [(20, .9), (40, .7), (50, .25)],
    'Gun_Stg44_Charge': [(20, .85), (30, .7), (40, .65)],
    'Weapon_type10Gun': [(10, .25), (80, .9), (140, .85)],
    P + 'HK416GrenadeLauncher': [(20, .95), (40, .8), (65, .75)],
    P + 'HK416Shotgun': [(20, .95), (40, .8), (65, .75)],
    P + 'USPSuppressed': [(25, 1)],
    P + 'USP': [(25, 1)],
    P + 'G36': [(20, .95), (40, .9), (70, .95)],
    P + 'MP7': [(10, 1), (20, .9), (35, .95)],
    P + 'DSR1': [(10, .1), (80, .9), (150, .95)],
    P + 'DSR50': [(10, .1), (80, .9), (150, .95)],
    P + 'AA12': [(10, 1), (15, .7), (20, .55)],
}


#职责：按需求中的明确距离和百分比写入命中曲线，节点之间线性插值。
def main():
    for path in ROOT.glob('*.xml'):
        tree = ET.parse(path)
        changed = False
        for thing in tree.getroot().findall('ThingDef'):
            values = CURVES.get(thing.findtext('defName'))
            if values is None:
                continue
            verb = thing.find('verbs/li')
            curve = verb.find('accuracyByDistance')
            if curve is None:
                curve = ET.SubElement(verb, 'accuracyByDistance')
            curve.clear()
            points = ET.SubElement(curve, 'points')
            for distance, accuracy in [(0, values[0][1])] + values:
                ET.SubElement(points, 'li').text = f'({distance}, {accuracy})'
            changed = True
        if changed:
            ET.indent(tree, space='  ')
            tree.write(path, encoding='utf-8', xml_declaration=True)


if __name__ == '__main__':
    main()
