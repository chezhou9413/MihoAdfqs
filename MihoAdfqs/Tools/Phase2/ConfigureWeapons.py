from pathlib import Path
import copy
import xml.etree.ElementTree as ET

ROOT = Path(__file__).resolve().parents[2] / '1.6/Defs/Weapons'
PREFIX = 'MihoPhase2_'
DATA = {
    'Weapon_AshTwelve': (20, 3.5, 650, '点射 半自动 全自动'),
    'Gun_HKFourOneSix': (45, 4, 900, '点射 全自动'),
    'Gun_LugerP08_Charge': (12, 2, 330, '点射 半自动'),
    'Gun_MG42_Charge': (150, 10, 1200, '点射 全自动'),
    'Gun_Pzb38_Charge': (5, 8, 60, '点射'),
    'Weapon_Q11Improved': (6, 10, 60, '点射'),
    'Gun_Stg44_Charge': (25, 3, 550, '点射 半自动'),
    'Weapon_type10Gun': (5, 10, 60, '点射'),
    PREFIX+'HK416GrenadeLauncher': (60, 4, 900, '点射 全自动'),
    PREFIX+'HK416Shotgun': (60, 4, 900, '点射 全自动'),
    PREFIX+'USPSuppressed': (15, 1.5, 400, '点射 全自动'),
    PREFIX+'USP': (15, 2, 440, '点射 全自动'),
    PREFIX+'G36': (30, 2.5, 750, '点射 全自动'),
    PREFIX+'MP7': (45, 1.5, 1000, '点射 全自动'),
    PREFIX+'DSR1': (10, 6.5, 60, '点射'),
    PREFIX+'DSR50': (8, 8.5, 60, '点射'),
    PREFIX+'AA12': (24, 2.5, 300, '点射 全自动'),
    PREFIX+'SupernovaHeavyLauncher': (2, 4, 60, '点射'),
    PREFIX+'SupernovaLaserRifle': (300, 3, 600, '全自动'),
    PREFIX+'SupernovaEMPrecisionRifle': (25, 4.5, 300, '点射 全自动'),
    PREFIX+'SupernovaEMMarksmanRifle': (35, 3.5, 450, '点射 全自动'),
    PREFIX+'SupernovaFusionGrenade': (1, 45, 60, '点射'),
}

#职责：写入一个确定的节点值，保留同节点下其他配置。
def set_value(node, key, value):
    child = node.find(key)
    if child is None:
        child = ET.SubElement(node, key)
    child.text = str(value)
    return child

#职责：将原版动词配置为独立弹匣动词。
def configure(verb, settings, secondary=False, pellets=1):
    capacity, seconds, rpm, modes = settings
    verb.set('Class', 'MihoAdfqs.Combat.Weapons.VerbProperties_Magazine')
    for key, value in {'verbClass':'MihoAdfqs.Combat.Weapons.Verb_Magazine', 'magazineSize':capacity,
                       'reloadSeconds':seconds, 'secondary':str(secondary).lower(), 'pellets':pellets,
                       'burstShotCount':1, 'preciseBurstTicks':3600/rpm, 'ticksBetweenBurstShots':max(1, round(3600/rpm))}.items():
        set_value(verb, key, value)
    mode_node = set_value(verb, 'fireModes', '')
    mode_node.clear()
    for mode in modes.split():
        ET.SubElement(mode_node, 'li').text = mode

#职责：为全部二期枪械写入弹匣数据，恢复HK416主枪并保留下挂。
def main():
    hk = ET.parse(ROOT/'Weapon_HK416.xml').find('./ThingDef/verbs/li')
    for path in ROOT.glob('*.xml'):
        tree = ET.parse(path, parser=ET.XMLParser(target=ET.TreeBuilder(insert_comments=True)))
        changed = False
        for thing in tree.getroot().findall('ThingDef'):
            name = thing.findtext('defName')
            if name not in DATA:
                continue
            changed = True
            verbs = thing.find('verbs')
            if name in (PREFIX+'HK416GrenadeLauncher', PREFIX+'HK416Shotgun'):
                if len(verbs) == 1:
                    verbs.insert(0, copy.deepcopy(hk))
                grenade = name.endswith('GrenadeLauncher')
                configure(verbs[1], (1,6,60,'点射') if grenade else (5,2,300,'点射'), True, 1)
                set_value(verbs[1], 'guaranteedAccuracy', 'true')
                set_value(verbs[1], 'label', '下挂榴弹' if grenade else '下挂霰弹')
                set_value(verbs[0], 'label', 'HK416主枪')
                for key, value in {'AccuracyTouch':0.95,'AccuracyShort':0.95,'AccuracyMedium':0.8,'AccuracyLong':0.75}.items():
                    set_value(thing.find('statBases'), key, value)
            configure(verbs[0], DATA[name], pellets=8 if name == PREFIX+'AA12' else 1)
            if 'Supernova' in name:
                set_value(verbs[0], 'guaranteedAccuracy', 'true')
                set_value(verbs[0], 'canGoWild', 'false')
            if name == PREFIX+'SupernovaHeavyLauncher':
                ammo = set_value(verbs[0], 'ammunition', '')
                ammo.clear()
                for suffix in ('HeavyAP','HeavyHE','Nanite','Radiation'):
                    ET.SubElement(ammo, 'li').text = PREFIX+'Projectile_Supernova'+suffix
            set_value(thing.find('statBases'), 'RangedWeapon_Cooldown', 0.1)
        if changed:
            for comment in list(tree.getroot()):
                if comment.tag is ET.Comment and any(s in comment.text for s in ('固定采用','固定以','近似持续')):
                    tree.getroot().remove(comment)
            ET.indent(tree, space='  ')
            tree.write(path, encoding='utf-8', xml_declaration=True)
    from ConfigureWeaponAccuracy import main as configure_accuracy
    configure_accuracy()

if __name__ == '__main__':
    main()
