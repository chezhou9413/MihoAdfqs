from pathlib import Path
import xml.etree.ElementTree as ET

ROOT = Path(__file__).resolve().parents[2] / '1.6' / 'Defs' / 'Weapons'


#职责：配置精确矩形范围及中心、边缘、墙体伤害，保留其它弹头数据。
def configure(file, name, damage, values):
    path = ROOT / file
    tree = ET.parse(path)
    thing = tree.getroot().find(f"ThingDef[defName='{name}']")
    thing.find('thingClass').text = 'MihoAdfqs.Combat.Projectiles.Projectile_AreaPayload'
    projectile = thing.find('projectile')
    projectile.find('damageAmountBase').text = str(damage)
    for tag in ('explosionRadius', 'explosionDamageFalloff', 'explosionDelay'):
        node = projectile.find(tag)
        if node is not None:
            projectile.remove(node)
    extensions = thing.find('modExtensions')
    if extensions is None:
        extensions = ET.SubElement(thing, 'modExtensions')
    old = extensions.find("li[@Class='MihoAdfqs.Combat.Projectiles.AreaPayload']")
    if old is not None:
        extensions.remove(old)
    payload = ET.SubElement(extensions, 'li', {'Class': 'MihoAdfqs.Combat.Projectiles.AreaPayload'})
    for key, value in values.items():
        ET.SubElement(payload, key).text = str(value)
    ET.indent(tree, space='  ')
    tree.write(path, encoding='utf-8', xml_declaration=True)


#职责：应用手雷、下挂和 Q11 的伤害分区。
def main():
    configure('Weapon_MihoPhase2_Nova_Grenade.xml', 'MihoPhase2_Projectile_SupernovaFusionGrenade', 300,
              {'width': 5, 'centerWidth': 3, 'centerDamage': 1500})
    configure('Weapon_Q11Improved.xml', 'Bullet_Q11Improved_Explosive', 40,
              {'width': 3, 'centerWidth': 1, 'centerDamage': 140})
    path = ROOT / 'Weapon_MihoPhase2_HK416Variants.xml'
    tree = ET.parse(path)
    for name in ('MihoPhase2_Projectile_HK416Grenade', 'MihoPhase2_Projectile_HK416Shotgun'):
        thing = tree.getroot().find(f"ThingDef[defName='{name}']")
        if thing.find('thingClass') is None:
            ET.SubElement(thing, 'thingClass').text = 'MihoAdfqs.Combat.Projectiles.Projectile_AreaPayload'
    tree.write(path, encoding='utf-8', xml_declaration=True)
    configure(path.name, 'MihoPhase2_Projectile_HK416Grenade', 40, {'width': 3, 'wallDamage': 200})
    configure(path.name, 'MihoPhase2_Projectile_HK416Shotgun', 10,
              {'width': 3, 'height': 1, 'orientToFlight': 'true'})


if __name__ == '__main__':
    main()
