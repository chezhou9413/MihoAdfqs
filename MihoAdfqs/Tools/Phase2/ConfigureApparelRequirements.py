from pathlib import Path
import xml.etree.ElementTree as ET

ROOT = Path(__file__).resolve().parents[2] / '1.6' / 'Defs'
P = 'MihoPhase2_'
METAL = {'PSOStandardNVGHelmet', 'PSOSpecialNVGHelmet', 'DemiHeavyArmor', 'DemiHeavyArmorAlpha',
         'DefenseForceUniform', 'DefenseForceHelmet', 'DefenseForceCap', 'SupernovaDropHelmet', 'SupernovaDropArmor'}
TORSO = {'TianaMaidDress', 'HelmerZogCasual', 'WineFoxCosplay', 'FashionOutfit', 'HoodieOutfit', 'MaidDress', 'ResearchCoat'}
SEWING = ['HandTailoringBench', 'ElectricTailoringBench']
ASSEMBLY = ['TableMachining', 'FabricationBench']
PRINTER = [P + 'Supernova3DPrinter']


#职责：设置唯一子节点，避免重复执行时累积XML条目。
def set_value(parent, name, value):
    child = parent.find(name)
    if child is None:
        child = ET.SubElement(parent, name)
    child.text = str(value) if value is not None else None
    return child


#职责：重建列表并显式替代父级列表。
def set_list(parent, name, values):
    child = set_value(parent, name, None)
    child.clear()
    child.set('Inherit', 'False')
    for value in values:
        ET.SubElement(child, 'li').text = value


#职责：按装备用途修正材料选择、配料、覆盖范围及制作设备。
def configure(thing):
    full_name = thing.findtext('defName', '')
    if not full_name.startswith(P) or full_name.endswith('Base'):
        return False
    name = full_name[len(P):]
    recipe = thing.find('recipeMaker')
    if recipe is None:
        return False
    costs = thing.find('costList')
    if name in METAL:
        steel = costs.find('Steel')
        if steel is not None:
            set_value(thing, 'costStuffCount', steel.text)
            costs.remove(steel)
        set_list(thing, 'stuffCategories', ['Metallic', 'Fabric'] if name == 'DefenseForceCap' else ['Metallic'])
    if name == 'FoxMask':
        for wood in costs.findall('WoodLog'):
            costs.remove(wood)
        set_value(thing, 'costStuffCount', 40)
        set_list(thing, 'stuffCategories', ['Woody'])
    if name in ('TianaChant', 'WineFoxCosplay', 'ResearchCoat'):
        set_value(costs, 'RawPosFlower', {'TianaChant': 10, 'WineFoxCosplay': 20, 'ResearchCoat': 40}[name])
    if name == 'WineFoxCosplay':
        set_value(costs, 'DevilstrandCloth', 20)
    if thing.find('stuffCategories') is not None:
        baseline = 'WoodLog' if name == 'FoxMask' else 'Cloth' if name == 'DefenseForceCap' or name not in METAL and 'Shield' not in name else 'Steel'
        extensions = thing.find('modExtensions')
        if extensions is None:
            extensions = ET.SubElement(thing, 'modExtensions')
        extension = extensions.find("li[@Class='MihoAdfqs.Phase2.Materials.ApparelMaterialBaseline']")
        if extension is None:
            extension = ET.SubElement(extensions, 'li', {'Class': 'MihoAdfqs.Phase2.Materials.ApparelMaterialBaseline'})
        set_value(extension, 'material', baseline)
        set_value(thing.find('statBases'), 'StuffEffectMultiplierArmor', 0)
    if name.startswith('Defense') or name.startswith('SupernovaDrop'):
        users = SEWING + ASSEMBLY + PRINTER
    elif name in METAL or name in ('HaliviShieldBelt', 'SupernovaShieldGenerator'):
        users = ['FabricationBench'] + PRINTER
    elif name in ('AssaultTowerShield', 'KaleviArmorPlate'):
        users = ASSEMBLY + PRINTER
    else:
        users = SEWING + PRINTER
    set_list(recipe, 'recipeUsers', users)
    if name in TORSO:
        set_list(thing.find('apparel'), 'bodyPartGroups', ['Torso'])
    if name in ('PSOStandardNVGHelmet', 'PSOSpecialNVGHelmet'):
        offsets = thing.find('equippedStatOffsets')
        for offset in offsets.findall('AimingDelayFactor'):
            offsets.remove(offset)
        if name == 'PSOSpecialNVGHelmet':
            set_value(offsets, 'MeleeDodgeChance', 3)
    return True


#职责：应用分目录维护的二期装备配方，不重建已有专属图形及组件。
def main():
    paths = list((ROOT / 'Phase2' / 'Apparel').glob('*.xml'))
    paths.append(ROOT / 'Apparel' / 'Apparel_MihoPhase2_NightVision.xml')
    for path in paths:
        tree = ET.parse(path)
        changed = False
        for thing in tree.getroot().findall('ThingDef'):
            changed = configure(thing) or changed
        if changed:
            ET.indent(tree, space='  ')
            tree.write(path, encoding='utf-8', xml_declaration=True)


if __name__ == '__main__':
    main()
