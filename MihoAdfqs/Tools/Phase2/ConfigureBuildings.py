from pathlib import Path
import xml.etree.ElementTree as ET
import copy

ROOT = Path(__file__).resolve().parents[2]/'1.6/Defs'

#职责：查找或创建节点并写入文本。
def put(parent, tag, value):
    child = parent.find(tag)
    if child is None: child = ET.SubElement(parent,tag)
    child.text = str(value)
    return child

#职责：按UTF-8格式保存指定模块文件。
def write(tree,path):
    ET.indent(tree,space='  ')
    tree.write(path,encoding='utf-8',xml_declaration=True)

#职责：为既有建筑接入自动生产和农业运行类型。
def main():
    for filename in ('AdvancedProductionBenches.xml','BeerBarrel.xml'):
        path = ROOT/'Phase2/Buildings'/filename
        tree = ET.parse(path)
        for thing in tree.findall('ThingDef'):
            put(thing,'thingClass','MihoAdfqs.Buildings.Production.Building_AutomatedProcessor')
            put(thing,'tickerType','Normal')
            put(thing,'description','人工装料100工作量后自动运行。'+('每2400工作量对应一天，断电暂停，产物品质固定普通。' if filename.startswith('Advanced') else '一百份玉米在约三十度环境累计发酵三十天，温度不合适时暂停。'))
        for recipe in tree.findall('RecipeDef'):
            put(recipe,'description','放入一百份玉米，在29～31℃环境发酵三十天。')
        write(tree,path)
    path = ROOT/'Phase2/Buildings/AgricultureBuildings.xml'
    tree = ET.parse(path)
    for thing in tree.findall('ThingDef'):
        if thing.findtext('defName') == 'MihoPhase2_IndoorCultivator':
            put(thing,'thingClass','MihoAdfqs.Buildings.Agriculture.Building_IndoorCultivator')
            put(thing,'description','人工播种、通电后自动收割成熟作物。培养机隔绝枯萎病，并提供作物所需的光照。')
    write(tree,path)
    path = ROOT/'Phase2/Items/MaterialRecipes.xml'
    tree = ET.parse(path)
    if not any(r.findtext('defName') == 'MihoPhase2_MakeIndustrialSteelPlateBulk4' for r in tree.findall('RecipeDef')):
        recipe = copy.deepcopy(tree.findall('RecipeDef')[0])
        put(recipe,'defName','MihoPhase2_MakeIndustrialSteelPlateBulk4'); put(recipe,'label','制作工业级钢板 x4'); put(recipe,'workAmount',160)
        for count in recipe.findall('ingredients/li/count'): count.text = str(int(count.text)*4)
        recipe.find('products/MihoPhase2_IndustrialSteelPlate').text = '4'
        tree.getroot().append(recipe)
    write(tree,path)
    path = ROOT/'Apparel/Apparel_MihoPhase2_NightVision.xml'
    tree = ET.parse(path)
    for thing in tree.findall('ThingDef'):
        if thing.findtext('defName') == 'MihoPhase2_PSOSpecialNVGHelmet':
            if not any(c.get('Class') == 'MihoAdfqs.Combat.Armor.CompProperties_ArmorReserve' for c in thing.find('comps')):
                put(ET.SubElement(thing.find('comps'),'li',{'Class':'MihoAdfqs.Combat.Armor.CompProperties_ArmorReserve'}),'capacity',500)
    write(tree,path)
    for path in (ROOT/'Phase2/Apparel').glob('*.xml'):
        text = path.read_text(encoding='utf-8').replace('<li>TableTailor</li>','<li>HandTailoringBench</li>')
        path.write_text(text,encoding='utf-8')

if __name__ == '__main__':
    main()
