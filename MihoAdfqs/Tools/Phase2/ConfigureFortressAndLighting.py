from pathlib import Path
import xml.etree.ElementTree as ET

ROOT = Path(__file__).resolve().parents[2] / '1.6' / 'Defs' / 'Phase2' / 'Buildings'


#职责：写入单个直接子节点并保持其它定义不变。
def set_value(parent, tag, value):
    node = parent.find(tag)
    if node is None:
        node = ET.SubElement(parent, tag)
    node.text = value


#职责：将两种堡垒建造定义接入同建筑炮台切换及共用钢铁库存。
def fortress():
    path = ROOT / 'FortressTurrets.xml'
    tree = ET.parse(path)
    for thing in tree.getroot().findall('ThingDef'):
        name = thing.findtext('defName')
        if name in ('MihoPhase2_ImperialFortress30mm', 'MihoPhase2_ImperialFortress88mm'):
            set_value(thing, 'thingClass', 'MihoAdfqs.Buildings.Defense.Building_ImperialFortress')
            set_value(thing, 'description', '共用500钢铁库存的无人堡垒。消耗库存内100钢铁切换炮台；30毫米500发弹仓，88毫米50发弹仓，每发消耗1钢铁。空仓通电装填5秒。')
            fuel = thing.find("comps/li[@Class='CompProperties_Refuelable']")
            for tag, value in {'fuelCapacity': '500', 'initialFuelPercent': '0', 'fuelConsumptionRate': '0', 'fuelLabel': '钢铁库存',
                               'fuelGizmoLabel': '钢铁库存', 'outOfFuelMessage': '需要装填钢铁'}.items():
                set_value(fuel, tag, value)
            set_value(thing.find('building'), 'turretBurstWarmupTime', '1.5')
            if name.endswith('88mm'):
                set_value(thing, 'designationCategory', None)
                thing.find('designationCategory').set('IsNull', 'True')
        elif name.startswith('MihoPhase2_Gun_Fortress'):
            set_value(thing.find('verbs/li'), 'verbClass', 'MihoAdfqs.Buildings.Defense.Verb_Fortress')
        elif name == 'MihoPhase2_Shell_Fortress88mm':
            set_value(thing, 'thingClass', 'MihoAdfqs.Combat.Projectiles.Projectile_AreaPayload')
            radius = thing.find('projectile/explosionRadius')
            if radius is not None:
                thing.find('projectile').remove(radius)
            mods = thing.find('modExtensions')
            if mods is None:
                mods = ET.SubElement(thing, 'modExtensions')
            payload = mods.find("li[@Class='MihoAdfqs.Combat.Projectiles.AreaPayload']")
            if payload is None:
                payload = ET.SubElement(mods, 'li', {'Class': 'MihoAdfqs.Combat.Projectiles.AreaPayload'})
            set_value(payload, 'width', '3')
    ET.indent(tree, space='  ')
    tree.write(path, encoding='utf-8', xml_declaration=True)


#职责：使泛光灯准确覆盖13乘13，培养机仅覆盖自身占地，不保留圆形灯光组件。
def lighting():
    path = ROOT / 'AgricultureBuildings.xml'
    tree = ET.parse(path)
    for thing in tree.getroot().findall('ThingDef'):
        lamp = thing.findtext('defName') == 'MihoPhase2_ImperialAgriculturalFloodlight'
        comps = thing.find('comps')
        for node in list(comps):
            if node.get('Class') in ('CompProperties_Glower', 'CompProperties_Schedule', 'MihoAdfqs.Buildings.Agriculture.CompProperties_CropLight'):
                comps.remove(node)
        props = ET.SubElement(comps, 'li', {'Class': 'MihoAdfqs.Buildings.Agriculture.CompProperties_CropLight'})
        set_value(props, 'width', '13')
        set_value(props, 'occupiedOnly', 'false' if lamp else 'true')
        if lamp:
            set_value(thing, 'thingClass', 'Building')
            set_value(thing, 'description', '通电时照亮以自身为中心的13×13区域，为无遮挡农作物提供100%光照。植物按自身昼夜规律休息。')
            for radius in thing.findall('specialDisplayRadius'):
                thing.remove(radius)
    ET.indent(tree, space='  ')
    tree.write(path, encoding='utf-8', xml_declaration=True)


#职责：应用堡垒和农业照明的维护配置。
def main():
    fortress()
    lighting()


if __name__ == '__main__':
    main()
