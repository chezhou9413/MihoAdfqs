from pathlib import Path
import xml.etree.ElementTree as ET

ROOT = Path(__file__).resolve().parents[2]
P = 'MihoPhase2_'
MATERIALS = {'板':P+'IndustrialSteelPlate','聚':P+'Polyethylene','星':P+'NovaMaterial','钢':'Steel','零':'ComponentIndustrial','高':'ComponentSpacer','布':'Cloth'}
ARMOR = [
    ('DemiHeavyArmor','德米重型全身防弹盔甲',3600,3400,(1.6,1.4,1.4),750,{'板':200,'钢':400,'聚':60,'零':12,'高':2},False,{'MeleeDodgeChance':5}, {}, 'ApparelAutoTend'),
    ('DemiHeavyArmorAlpha','德米重型全身防弹盔甲（阿尔法）',4200,4600,(1.6,1.4,1.4),1500,{'板':250,'钢':500,'聚':80,'零':12,'高':2},False,{'MeleeDodgeChance':5}, {}, 'ApparelAutoTend'),
    ('DefenseForceUniform','国防军制服',1400,600,(.9,1.1,.4),250,{'钢':200,'聚':40},False,{'MeleeDodgeChance':1}, {}, None),
    ('DefenseForceHelmet','国防军头盔',750,400,(1.2,.8,.9),250,{'钢':100,'板':5},True,{'MeleeDodgeChance':1}, {}, None),
    ('DefenseForceCap','国防军帽子',750,350,(.4,.4,.2),0,{'钢':100,'板':5},True,{'MeleeDodgeChance':1,'PawnBeauty':1}, {}, None),
    ('SupernovaDropHelmet','超新星轨道空降部队头盔',4750,4700,(1.8,1.8,2),1500,{'钢':600,'板':120,'聚':100,'零':20,'高':12,'星':1},True,{'MeleeDodgeChance':8,'ShootingAccuracyPawn':8,'VacuumResistance':1}, {'AimingDelayFactor':.25}, 'ApparelRegeneration'),
    ('SupernovaDropArmor','超新星轨道空降部队盔甲',6750,5600,(2,2,2),3000,{'钢':800,'板':300,'聚':200,'零':20,'高':16,'星':2},False,{'MeleeDodgeChance':10,'MoveSpeed':4,'VacuumResistance':1}, {'AimingDelayFactor':.25}, 'ApparelRegeneration'),
]
CLOTHES = [
    ('TianaMaidDress','米莉拉女仆装（第三帝国款）',400,200,(.4,.4,0),140,{}, {'PawnBeauty':3},{}),
    ('TianaChant','颂歌',500,200,(.8,.8,0),200,{}, {'PawnBeauty':5,'MoveSpeed':2},{}),
    ('HelmerZogSuit','西服',1000,800,(1,1,.6),140,{'聚':100},{'PawnBeauty':3,'MeleeDodgeChance':3},{'StaggerDurationFactor':0}),
    ('HelmerZogCasual','常服',400,200,(.4,1.4,0),80,{'聚':50},{'MeleeDodgeChance':3},{'StaggerDurationFactor':0}),
    ('WineFoxCosplay','酒狐cos服',800,400,(.6,.6,1.2),140,{}, {'PawnBeauty':2},{'WorkSpeedGlobal':2.2}),
    ('FashionOutfit','时尚装束',400,200,(.4,.2,0),180,{}, {'PawnBeauty':3},{}),
    ('HoodieOutfit','卫衣套装',400,200,(.4,.6,0),220,{}, {'MoveSpeed':1},{}),
    ('MaidDress','女仆装',400,200,(.4,.2,0),120,{}, {'MoveSpeed':1},{'WorkSpeedGlobal':1.2}),
    ('ResearchCoat','科学院专用研究服',700,400,(.4,.2,0),220,{}, {'CarryingCapacity':10},{'WorkSpeedGlobal':1.2,'ResearchSpeed':2}),
    ('MitaCap','米塔帽子',200,80,(.4,.4,0),80,{}, {},{}),
    ('FoxMask','狐狸面具',200,100,(.2,.2,0),0,{'WoodLog':40}, {},{}),
]

#职责：写入简单值或字典节点，保持文件为可读UTF-8 XML。
def node(parent, name, value=None, **attrs):
    element = ET.SubElement(parent, name, attrs)
    if isinstance(value, dict):
        for key, item in value.items():
            node(element, MATERIALS.get(key,key), item)
    elif isinstance(value, (list,tuple)):
        for item in value:
            node(element,'li',item)
    elif value is not None:
        element.text = str(value)
    return element

#职责：按文件职责输出单个装备定义及其附属效果。
def save(root, name):
    path = ROOT/'1.6/Defs/Phase2/Apparel'/f'{name}.xml'
    path.parent.mkdir(parents=True, exist_ok=True)
    ET.indent(root, space='  ')
    ET.ElementTree(root).write(path, encoding='utf-8', xml_declaration=True)

#职责：创建具备配方、贴图、身体覆盖及统计数值的装备。
def create(name,label,hp,work,ratings,cost,head=False,civil=False,offsets=None,factors=None,reserve=0,medical=None,cloth=0):
    root = ET.Element('Defs')
    parent = P+('ApparelHatBase' if head else 'ApparelClothingBase') if civil else P+('ApparelHeadArmorBase' if head else 'ApparelArmorBase')
    thing = node(root,'ThingDef',ParentName=parent)
    node(thing,'defName',P+name); node(thing,'label',label)
    description=label+'。'+('帝国民用服装。' if civil else '帝国制式装备。')
    if name.startswith('SupernovaDrop'):
        description='由帝国“卡列维”科研站开发的顶级'+('头盔' if head else '护甲')+'，针对战场上任何可能出现的情况，均具有应对方式。据说，其形象源自帝国某个热度很高的网络游戏，只是相比游戏中近乎没有任何防御力的盔甲，防御力要高上十几倍。'
    if reserve:
        description+=f'拥有{reserve}点抵消值，命中覆盖部位时先消耗抵消值，耗尽后再损耗装备耐久；抵消值不会自行恢复，只能使用卡列维军用防弹插板修复，每块恢复250点。'
    if medical == 'ApparelRegeneration':
        description+='穿戴后自动包扎伤口，每秒为每个受伤部件恢复2点伤势。'
    node(thing,'description',description)
    tex = f'apparel/MihoPhase2/{name}/{P}{name}'
    if name == 'DefenseForceCap': tex = 'apparel/MihoPhase2/MitaCap/MihoPhase2_MitaCap'
    itemtex = tex if (ROOT/'Textures'/f'{tex}.png').exists() else tex+'_south'
    node(thing,'graphicData',{'texPath':itemtex,'graphicClass':'Graphic_Single'})
    stats = {'MaxHitPoints':hp,'WorkToMake':work,'Mass':1 if civil else 8,'ArmorRating_Sharp':ratings[0],'ArmorRating_Blunt':ratings[1],'ArmorRating_Heat':ratings[2]}
    node(thing,'statBases',stats)
    node(thing,'costList',cost)
    if cloth:
        node(thing,'stuffCategories',['Fabric']); node(thing,'costStuffCount',cloth)
    recipe = node(thing,'recipeMaker')
    research = 'CivilianApparel' if civil else 'Nova' if name.startswith('Supernova') else 'MilitaryApparel' if name.startswith('Defense') else 'SpecialForces'
    node(recipe,'researchPrerequisite',P+'Research_'+research)
    node(recipe,'recipeUsers',['TableTailor','ElectricTailoringBench'] if civil else ['TableMachining','FabricationBench',P+'Supernova3DPrinter'],Inherit='False')
    comps = node(thing,'comps')
    if reserve:
        node(comps,'li',{'capacity':reserve},Class='MihoAdfqs.Combat.Armor.CompProperties_ArmorReserve')
    if medical:
        node(comps,'li',{'hediff':P+medical},Class='CompProperties_CauseHediff_Apparel')
    if offsets: node(thing,'equippedStatOffsets',offsets)
    if factors:
        effect = P+name+'Effect'
        node(comps,'li',{'hediff':effect},Class='CompProperties_CauseHediff_Apparel')
        hediff = node(root,'HediffDef')
        node(hediff,'defName',effect); node(hediff,'label',label); node(hediff,'hediffClass','HediffWithComps'); node(hediff,'isBad','false')
        node(node(hediff,'comps'),'li',Class='HediffCompProperties_RemoveIfApparelDropped')
        node(node(node(hediff,'stages'),'li'),'statFactors',factors)
    apparel = node(thing,'apparel')
    node(apparel,'bodyPartGroups',['FullHead'] if head else ['Torso','Neck','Shoulders','Arms','Legs'])
    node(apparel,'layers',['Overhead' if head else 'Shell'])
    node(apparel,'wornGraphicPath',tex)
    tag = 'Civilian' if civil else 'Orbital' if name.startswith('Supernova') else 'Defense' if name.startswith('Defense') else 'Guard'
    node(apparel,'tags',[P+tag],Inherit='False')
    save(root,name)

#职责：生成二期全套装备，专属人物外观与护甲分文件维护。
def main():
    for name,label,hp,work,ratings,reserve,cost,head,offsets,factors,medical in ARMOR:
        create(name,label,hp,work,ratings,cost,head,offsets=offsets,factors=factors,reserve=reserve,medical=medical)
    for name,label,hp,work,ratings,cloth,cost,offsets,factors in CLOTHES:
        create(name,label,hp,work,ratings,cost,name in ('MitaCap','FoxMask'),True,offsets,factors,cloth=cloth)
    from ConfigureApparelRequirements import main as configure_requirements
    configure_requirements()

if __name__ == '__main__':
    main()
