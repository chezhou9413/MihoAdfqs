from pathlib import Path
import xml.etree.ElementTree as ET

ROOT = Path(__file__).resolve().parents[2] / '1.6' / 'Defs' / 'Phase2' / 'Items'


#职责：写回保持UTF-8且可读缩进的定义文件。
def save(tree, path):
    ET.indent(tree, space='  ')
    tree.write(path, encoding='utf-8', xml_declaration=True)


#职责：使用美狐依赖的实际原料定义，并补齐装配与打印机的批量配方入口。
def materials():
    path = ROOT / 'MaterialRecipes.xml'
    tree = ET.parse(path)
    for recipe in tree.getroot().findall('RecipeDef'):
        name = recipe.findtext('defName')
        product = list(recipe.find('products'))[0]
        amount = int(product.text)
        for ingredient in recipe.findall('ingredients/li'):
            filter_node = ingredient.find('filter')
            if filter_node.find("categories/li[.='StoneBlocks']") is not None:
                filter_node.clear()
                ET.SubElement(ET.SubElement(filter_node, 'thingDefs'), 'li').text = 'Miho_Ceramics'
            for node in filter_node.findall('thingDefs/li'):
                if node.text == 'Shard':
                    node.text = 'Miho_ExoticMatter'
        recipe.find('description').text = f'按标准工艺制作{amount}份{product.tag.replace("MihoPhase2_IndustrialSteelPlate", "工业级钢板").replace("MihoPhase2_Polyethylene", "聚乙烯").replace("MihoPhase2_NovaMaterial", "新星")}。'
        users = recipe.find('recipeUsers')
        users.clear()
        if 'NovaMaterial' in name:
            benches = ['MihoPhase2_NovaAggregator']
        else:
            benches = ['MihoPhase2_Supernova3DPrinter']
            if amount <= 4:
                benches += ['TableMachining', 'FabricationBench']
        for bench in benches:
            ET.SubElement(users, 'li').text = bench
    save(tree, path)


#职责：将曲奇和纪念面包中的花瓣引用绑定至美狐种植产物。
def flowers():
    for name in ('Foods.xml', 'CommemorativeBread.xml'):
        path = ROOT / name
        tree = ET.parse(path)
        for recipe in tree.getroot().findall('RecipeDef'):
            for node in recipe.findall('ingredients/li/filter/thingDefs/li'):
                if node.text == 'PsychoidLeaves':
                    node.text = 'RawPosFlower'
                if name == 'CommemorativeBread.xml' and node.text == 'RawRice':
                    node.text = 'MihoPhase2_RawWheat'
            description = recipe.find('description')
            description.text = description.text.replace('灵性植物叶片', '秘灵花瓣')
        save(tree, path)


#职责：统一应用材料和食品配方引用。
def main():
    materials()
    flowers()


if __name__ == '__main__':
    main()
