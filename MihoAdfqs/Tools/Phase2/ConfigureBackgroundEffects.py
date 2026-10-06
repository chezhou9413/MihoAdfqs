from pathlib import Path
import xml.etree.ElementTree as ET

ROOT = Path(__file__).resolve().parents[2]
PREFIX = 'MihoPhase2_'
RULES = {
    'Childhood_IdolLoveChild': {'offsets': {'PawnBeauty': 3}},
    'Childhood_NeighborMiho': {'offsets': {'PawnBeauty': 3}},
    'Childhood_PainfulPast': {'mood': -3, 'factors': {'Fertility': 0}},
    'Childhood_BrattyMiho': {'factors': {'Fertility': 0.5}},
    'Childhood_GiftedMiho': {'factors': {'Fertility': 1.5}},
    'Childhood_ImperialMilira': {'factors': {'Fertility': 1.5}},
    'Adulthood_DefenseSoldier': {'colleagueDeathPenalty': 3},
    'Adulthood_DefenseVeteran': {'oldGunshot': True},
    'Adulthood_DefenseNCO': {'commander': True},
    'Adulthood_SpecialForcesSoldier': {'shotTime': 0.5, 'aimTime': 0.25},
    'Adulthood_OrbitalDropTrooper': {'shotTime': 0.25, 'aimTime': 0.25},
    'Adulthood_CelebrityChef': {'factors': {'FoodPoisonChance': 0, 'CookSpeed': 0.5}},
    'Adulthood_MiliraCookingTeacher': {'factors': {'FoodPoisonChance': 0}, 'heartDisease': True},
    'Adulthood_IdolMiho': {'offsets': {'PawnBeauty': 6}},
    'Adulthood_CatLover': {'catSlaughter': True},
    'Adulthood_ShrineMaiden': {'offsets': {'PawnBeauty': 2}},
    'Adulthood_Intern': {'mood': -5, 'factors': {'RestRateMultiplier': 1 / 1.5}},
    'Adulthood_CivilianMilira': {'factors': {'FoodPoisonChance': 0.35}},
    'Adulthood_FoolishMilira': {'mentalImmune': True},
    'Adulthood_MiliraChef': {'factors': {'FoodPoisonChance': 0.25}},
    'Adulthood_MiliraHomebody': {'homebody': True},
    'Adulthood_Helmer': {'mentalImmune': True},
}


#职责：以明确UTF-8编码保存生成的项目XML。
def write(path, root):
    path.parent.mkdir(parents=True, exist_ok=True)
    ET.indent(root, space='  ')
    ET.ElementTree(root).write(path, encoding='utf-8', xml_declaration=True)


#职责：写入单个XML字段，布尔量采用框架能够读取的格式。
def field(parent, key, value):
    node = ET.SubElement(parent, key)
    node.text = str(value).lower() if isinstance(value, bool) else str(value)
    return node


#职责：给现有背景附加机制参数，保留叙事、技能和原有其它扩展。
def backgrounds():
    found = set()
    for path in (ROOT / '1.6/Defs').rglob('*.xml'):
        root = ET.parse(path).getroot()
        changed = False
        for story in root.findall('BackstoryDef'):
            name = story.findtext('defName', '')
            key = name.removeprefix(PREFIX)
            if key not in RULES:
                continue
            found.add(key)
            extensions = story.find('modExtensions')
            if extensions is None:
                extensions = ET.SubElement(story, 'modExtensions')
            for old in list(extensions):
                if old.get('Class') == 'MihoAdfqs.Phase2.Backgrounds.BackgroundEffects':
                    extensions.remove(old)
            extension = ET.SubElement(extensions, 'li', Class='MihoAdfqs.Phase2.Backgrounds.BackgroundEffects')
            for item, value in RULES[key].items():
                if isinstance(value, dict):
                    stats = ET.SubElement(extension, item)
                    for stat, amount in value.items():
                        field(stats, stat, amount)
                else:
                    field(extension, item, value)
            changed = True
        if changed:
            write(path, root)
    if found != set(RULES):
        raise ValueError('缺少背景定义：' + ', '.join(sorted(set(RULES) - found)))


#职责：定义可见指挥效果和背景心情状态。
def effects():
    root = ET.Element('Defs')
    aura = ET.SubElement(root, 'HediffDef')
    field(aura, 'defName', 'MihoPhase2_CommandAura')
    field(aura, 'label', '高效指挥')
    field(aura, 'description', '处于友方士官十五格范围内，远程武器连发间隔和轮次冷却乘以50%。多个士官不会重复叠加。')
    field(aura, 'hediffClass', 'HediffWithComps')
    field(aura, 'isBad', False)
    field(aura, 'initialSeverity', 1)
    comp = ET.SubElement(ET.SubElement(aura, 'comps'), 'li', Class='HediffCompProperties_Disappears')
    field(comp, 'disappearsAfterTicks', '120~120')
    thought = ET.SubElement(root, 'ThoughtDef')
    field(thought, 'defName', 'MihoPhase2_BackgroundMood')
    field(thought, 'thoughtClass', 'MihoAdfqs.Phase2.Backgrounds.Thought_BackgroundMood')
    field(thought, 'workerClass', 'MihoAdfqs.Phase2.Backgrounds.ThoughtWorker_BackgroundMood')
    stage = ET.SubElement(ET.SubElement(thought, 'stages'), 'li')
    field(stage, 'label', '往事与生活习惯')
    field(stage, 'description', '过往经历会持续影响心情。死宅连续离开室内超过十小时后额外降低心情，回到室内后解除。')
    field(stage, 'baseMoodEffect', 0)
    write(ROOT / '1.6/Defs/Phase2/Backgrounds/BackgroundEffects.xml', root)


#职责：为两个人形种族注入背景离室计时组件。
def components():
    root = ET.Element('Patch')
    for name in ['Alien_Miho', 'Milira_Race']:
        race = f'Defs/AlienRace.ThingDef_AlienRace[defName="{name}"]'
        operation = ET.SubElement(root, 'Operation', Class='PatchOperationConditional')
        field(operation, 'xpath', race + '/comps')
        missing = ET.SubElement(operation, 'nomatch', Class='PatchOperationAdd')
        field(missing, 'xpath', race)
        ET.SubElement(ET.SubElement(missing, 'value'), 'comps')
        operation = ET.SubElement(root, 'Operation', Class='PatchOperationAdd')
        field(operation, 'xpath', race + '/comps')
        field(ET.SubElement(ET.SubElement(operation, 'value'), 'li'), 'compClass', 'MihoAdfqs.Phase2.Backgrounds.Comp_BackgroundExposure')
    write(ROOT / '1.6/Patches/Phase2BackgroundComponents.xml', root)


#职责：同步完整背景机制及所需组件和状态定义。
def main():
    backgrounds()
    effects()
    components()
    print(f'已接入 {len(RULES)} 项背景扩展及心情、光环和离室组件。')


if __name__ == '__main__':
    main()
