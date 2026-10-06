from pathlib import Path
import shutil
import xml.etree.ElementTree as ET

ROOT = Path(__file__).resolve().parents[2]
FA = ROOT / 'FacialAnimation'


#职责：以UTF-8保存按功能分组的XML定义。
def write(path, root):
    path.parent.mkdir(parents=True, exist_ok=True)
    ET.indent(root, space='  ')
    ET.ElementTree(root).write(path, encoding='utf-8', xml_declaration=True)


#职责：创建带文本内容的XML字段。
def field(parent, name, text):
    result = ET.SubElement(parent, name)
    result.text = str(text)
    return result


#职责：保留原始贴图，通过文件副本匹配FA已有形状名，不更改图片内容。
def aliases(person):
    mouth = {'normal': 'Closed', 'open': 'Open', 'MihoPhase2_Smile': 'Happy', 'MihoPhase2_Down': 'Closed'}
    if person == 'Tiana':
        mouth = {'normal': 'Normal', 'open': 'Open', 'MihoPhase2_Smile': 'Normal', 'MihoPhase2_Down': 'Angry', 'close': 'Closed'}
    mappings = {
        'Brows': ('Brow', {'normal': 'Normal', 'angled': 'Angry', 'flat': 'Happy', 's-shaped': 'Sad'}),
        'Eyes': ('Eye', {'normal': 'Normal'}),
        'Lids': ('Lid', {'normal': 'Open', 'close': 'Closed', 'half': 'Closed'}),
        'Mouth': ('Mouth', mouth),
    }
    for folder, (part, shapes) in mappings.items():
        source = ROOT / 'Textures/FacialAnimation/MihoPhase2' / person / folder / 'Unisex'
        target = FA / 'Textures/MihoPhase2Faces' / person / folder / 'Unisex'
        target.mkdir(parents=True, exist_ok=True)
        for alias, shape in shapes.items():
            prefix = f'MihoPhase2_{person}{part}_{shape}'
            files = list(source.glob(prefix + '_*.png'))
            if not files:
                raise ValueError(f'缺少表情贴图：{source / prefix}')
            for image in files:
                shutil.copyfile(image, target / (alias + image.name[len(prefix):]))


#职责：按种族和专属基因绑定五官类型，普通人物使用无基因限制的类型。
def types():
    parts = {'Head': 'Head', 'Eyeball': 'Eyes', 'Lid': 'Lids', 'Brow': 'Brows', 'Mouth': 'Mouth'}
    for person, race, gene in [('ImperialMiho', 'Alien_Miho', None),
                               ('HelmerZog', 'Alien_Miho', 'Gene_MihoPhase2_Experiment45'),
                               ('Tiana', 'Milira_Race', 'Gene_MihoPhase2_FirstStandard')]:
        root = ET.Element('Defs')
        for part, folder in parts.items():
            node = ET.SubElement(root, f'FacialAnimation.{part}TypeDef')
            field(node, 'defName', f'MihoPhase2_{person}_{part}')
            field(node, 'label', {'ImperialMiho': '帝国美狐', 'HelmerZog': '赫尔默', 'Tiana': '提亚娜'}[person] + '表情部件')
            field(node, 'raceName', race)
            if person == 'ImperialMiho' or (person == 'HelmerZog' and part == 'Head'):
                path = f'FacialAnimation/Adfqs/{folder}'
            elif person == 'Tiana' and part == 'Head':
                path = 'Things/Pawn/Milira/Heads_Blank/MiliraHead'
            else:
                path = f'MihoPhase2Faces/{person}/{folder}'
            field(node, 'texPath', path)
            field(node, 'shader', 'Map/CutoutSkin' if part == 'Head' else 'Map/Transparent')
            field(node, 'enableUnisexTexPath', 'true')
            if part != 'Head':
                field(node, 'minColor', 'RGB(255,255,255)')
                field(node, 'maxColor', 'RGB(255,255,255)')
            if gene:
                field(field(node, 'targetGeneDefs', ''), 'li', gene)
        write(FA / f'Defs/FaceTypeDefs/{person}.xml', root)


#职责：向美狐注入缺少的FA组件，避免与其它兼容补丁重复注入。
def components():
    root = ET.Element('Patch')
    race = 'Defs/AlienRace.ThingDef_AlienRace[defName="Alien_Miho"]'
    conditional = ET.SubElement(root, 'Operation', Class='PatchOperationConditional')
    field(conditional, 'xpath', race + '/comps')
    action = ET.SubElement(conditional, 'nomatch', Class='PatchOperationAdd')
    field(action, 'xpath', race)
    ET.SubElement(ET.SubElement(action, 'value'), 'comps')
    for name in ['DrawFaceGraphicsComp', 'HeadControllerComp', 'EyeballControllerComp', 'LidControllerComp',
                 'BrowControllerComp', 'MouthControllerComp', 'SkinControllerComp', 'LidOptionControllerComp',
                 'EmotionControllerComp', 'FacialAnimationControllerComp']:
        conditional = ET.SubElement(root, 'Operation', Class='PatchOperationConditional')
        field(conditional, 'xpath', race + f'/comps/li[compClass="FacialAnimation.{name}"]')
        action = ET.SubElement(conditional, 'nomatch', Class='PatchOperationAdd')
        field(action, 'xpath', race + '/comps')
        field(ET.SubElement(ET.SubElement(action, 'value'), 'li'), 'compClass', f'FacialAnimation.{name}')
    write(FA / 'Patches/Components.xml', root)


#职责：建立完整的美狐基础、眨眼、进食、战斗、睡眠及情绪动画。
def animations():
    root = ET.Element('Defs')
    entries = [
        ('Normal', 0, None, {}, [(1, {'head': 'normal', 'eyeball': 'normal', 'brow': 'normal', 'lid': 'normal', 'mouth': 'normal'})]),
        ('Blink', 100, None, {'roopIntervalMin': 150, 'roopIntervalMax': 280}, [(6, {'lid': 'close'})]),
        ('Happy', 50, None, {'targetMoodMin': 0.65}, [(1, {'brow': 'flat', 'mouth': 'MihoPhase2_Smile'})]),
        ('Sad', 50, None, {'targetMoodMax': 0.25}, [(1, {'brow': 's-shaped', 'mouth': 'MihoPhase2_Down'})]),
        ('Pain', 150, None, {'targetPainMin': 0.35}, [(1, {'lid': 'close', 'mouth': 'MihoPhase2_Down'})]),
        ('Combat', 200, ['AttackStatic', 'AttackMelee', 'Wait_Combat'], {}, [(1, {'brow': 'angled', 'mouth': 'MihoPhase2_Down'})]),
        ('Eat', 200, ['Ingest'], {}, [(8, {'mouth': 'open'}), (8, {'mouth': 'normal'})]),
        ('Rest', 300, ['LayDown'], {}, [(1, {'lid': 'close', 'mouth': 'normal'})]),
    ]
    for name, priority, jobs, parameters, frames in entries:
        node = ET.SubElement(root, 'FacialAnimation.FaceAnimationDef')
        field(node, 'defName', 'MihoPhase2_Face_' + name)
        field(node, 'raceName', 'Alien_Miho')
        field(node, 'priority', priority)
        if jobs:
            for job in jobs:
                field(node.find('targetJobs') if node.find('targetJobs') is not None else ET.SubElement(node, 'targetJobs'), 'li', job)
        for key, value in parameters.items():
            field(node, key, value)
        frame_list = ET.SubElement(node, 'animationFrames')
        for duration, shapes in frames:
            frame = ET.SubElement(frame_list, 'li')
            field(frame, 'duration', duration)
            for part, shape in shapes.items():
                field(frame, part + 'ShapeDef', shape)
        field(node, 'applyWhenStandingOnly', 'false')
    write(FA / 'Defs/AnimationDefs/ImperialMiho.xml', root)
    root = ET.Element('Defs')
    node = ET.SubElement(root, 'FacialAnimation.FaceAdjustmentDef')
    field(node, 'defName', 'MihoPhase2_FaceAdjustment')
    field(node, 'RaceName', 'Alien_Miho')
    field(node, 'Size', '(1.125,1.125)')
    write(FA / 'Defs/FaceAdjustment.xml', root)


#职责：注册只属于本模组的额外嘴型，并复用元首已有贴图。
def shapes():
    root = ET.Element('Defs')
    for old, name in [('smile', 'MihoPhase2_Smile'), ('down', 'MihoPhase2_Down')]:
        node = ET.SubElement(root, 'FacialAnimation.MouthShapeDef')
        field(node, 'defName', name)
        field(node, 'altShapeDef', 'normal')
        source = ROOT / 'Textures/FacialAnimation/Adfqs/Mouth/Unisex'
        target = FA / 'Textures/FacialAnimation/Adfqs/Mouth/Unisex'
        target.mkdir(parents=True, exist_ok=True)
        for image in source.glob(old + '_*.png'):
            shutil.copyfile(image, target / (name + image.name[len(old):]))
    write(FA / 'Defs/FaceShapeDefs/ImperialMouth.xml', root)


#职责：完整生成表情配置和对照用贴图别名。
def main():
    for person in ['HelmerZog', 'Tiana']:
        aliases(person)
    types()
    components()
    shapes()
    animations()
    print('已生成美狐FA组件、八组动画、三组人物类型及特殊五官贴图别名。')


if __name__ == '__main__':
    main()
