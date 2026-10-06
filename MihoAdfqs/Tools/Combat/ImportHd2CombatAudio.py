import csv
import hashlib
import subprocess
import xml.etree.ElementTree as ET
from pathlib import Path
import numpy as np
import soundfile as sf

MOD = Path(__file__).resolve().parents[2]
RAW = Path(r'E:\ModdevMics\Assets\Helldivers2\MihoCombat')
FFMPEG = Path(r'E:\ModdevMics\Tools\Filediver\v0.7.55\filediver-cli\ffmpeg.exe')
WORK = RAW / 'Processed'
SOUNDS = MOD / 'Sounds' / 'Combat' / 'HD2'

#映射保留真实Bank与媒体ID；同型号改型共享来源，但分别调整音调和音量。
FAMILIES = {
    'Pistol': ('hornet_pistol', ['469255939','476691000'], .6),
    'Suppressed': ('m6c_pistol', ['570421238','823457118'], .35),
    'Liberator': ('wep_ar19_liberator', ['94681415','627885530'], .42),
    'Justice': ('wep_ar_justice', ['755859388','979640435'], .38),
    'Adjudicator': ('wep_cr1_adjudicator', ['646862171','1014377450'], .45),
    'Dominator': ('wep_jar5_dominator', ['117805662','525957057'], .55),
    'Eruptor': ('wep_jpr_eruptor', ['845705203','101340640'], .7),
    'Sniper': ('wep_sar1_sniper', ['553167497','1014728585'], .8),
    'Railgun': ('wep_railgun', ['4188536','152480282'], .7),
    'Stalwart': ('wep_mg105_stalwart', ['212791666','626890771'], .23),
    'Breaker': ('wep_sg225_breaker', ['85772572','1009506144'], .65),
    'Punisher': ('wep_sg8_punisher', ['116200355','232772099'], .65),
    'Knight': ('wpn_mp98_knight', ['1054944351','631187499'], .3),
    'GrenadeLaunch': ('wep_grenade_launcher', ['135900770','617482634'], .65),
    'Eat': ('wep_eat17_expendable_at_rifle', ['462238529','579640446'], .85),
    'Autocannon': ('wep_autocannon', ['1013945195','995529820'], .8),
    'Recoilless': ('wep_recoilless_ap', ['1018262330','491849867'], .8),
    'GrenadeBlast': ('wep_gre_at_explosive', ['67904039','921309796'], 1.85),
    'HeavyBlast': ('wep_expendable_massive_rocket_launcher', ['88733658','69615532'], 2.5),
    'LaserLoop': ('wep_scythe', ['528256306'], 3),
    'FlameLoop': ('wep_flamethrower', ['13725339'], 3),
    'ReloadMagazine': ('wep_smg45_defender', ['807985691','76349284'], .9),
    'ReloadHeavy': ('wep_mg105_stalwart', ['410134813','706323621'], .55),
    'ReloadEnergy': ('wep_scythe', ['706323621','869814175'], .55),
    'ReloadLock': ('wep_sar1_sniper', ['694452004'], .28),
}
#武器Def、音色族、音调、音量。
WEAPONS = {
    'Gun_LugerP08_Charge': ('Pistol', 1.04, 70),
    'MihoPhase2_USP': ('Pistol', .92, 65),
    'MihoPhase2_USPSuppressed': ('Suppressed', 1.07, 37),
    'Gun_HKFourOneSix': ('Liberator', 1.04, 68),
    'MihoPhase2_HK416GrenadeLauncher': ('Liberator', 1.01, 68),
    'MihoPhase2_HK416Shotgun': ('Liberator', .98, 68),
    'Gun_Stg44_Charge': ('Justice', .91, 73),
    'MihoPhase2_G36_Def': ('Adjudicator', 1.08, 68),
    'Weapon_AshTwelve': ('Dominator', .88, 77),
    'Weapon_Q11Improved': ('Eruptor', 1.08, 76),
    'MihoPhase2_DSR1_Def': ('Sniper', 1.02, 77),
    'MihoPhase2_DSR50_Def': ('Sniper', .78, 82),
    'Weapon_type10Gun': ('Sniper', .87, 80),
    'Gun_Pzb38_Charge': ('Recoilless', 1.12, 82),
    'Gun_MG42_Charge': ('Stalwart', 1.02, 66),
    'MihoPhase2_AA12_Def': ('Breaker', .95, 75),
    'MihoPhase2_MP7_Def': ('Knight', 1.14, 62),
    'MihoPhase2_SupernovaEMPrecisionRifle': ('Railgun', .88, 82),
    'MihoPhase2_SupernovaEMMarksmanRifle': ('Railgun', 1.18, 75),
    'MihoPhase2_SupernovaHeavyLauncher': ('Autocannon', .8, 85),
    'Gun_Panzerfaust': ('Eat', .92, 80),
    'MihoPhase2_SupernovaLaserRifle': ('LaserLoop', 1.08, 50),
    'Weapon_BeamFlamethrower': ('FlameLoop', .94, 56),
    'MihoPhase2_Gun_Fortress30mm': ('Autocannon', 1.03, 77),
    'MihoPhase2_Gun_Fortress88mm': ('Recoilless', .72, 87),
}

#先对片段进行有限增益、切尾和循环交叉淡化，再导出独立OGG。
def process_audio(family, media_id, duration):
    source = RAW / 'Ogg' / (media_id + '.ogg')
    samples, rate = sf.read(str(source), dtype='float32', always_2d=True)
    peak = float(np.max(np.abs(samples)))
    gain = min(2.5, .78 / max(.01, peak))
    loop = family.endswith('Loop')
    start = 2 if loop else 0
    wav = WORK / (family + '_' + media_id + '.wav')
    filters = f'volume={gain:.5f},highpass=f=45,alimiter=limit=0.92:level=false'
    if not loop:
        filters = 'silenceremove=start_periods=1:start_duration=0.003:start_threshold=-45dB,' + filters
        filters += f',atrim=duration={duration},afade=t=out:st={max(.02,duration-.08)}:d=0.08'
    subprocess.run([str(FFMPEG), '-hide_banner','-loglevel','error','-y','-ss',str(start),'-i',str(source),
                    '-t',str(duration),'-af',filters,'-ar','44100','-ac','1',str(wav)], check=True)
    if loop:
        audio, sample_rate = sf.read(str(wav), dtype='float32')
        overlap = int(sample_rate * .12)
        mix = np.linspace(0, 1, overlap, dtype='float32')
        joined = np.concatenate((audio[overlap:-overlap], audio[-overlap:] * (1-mix) + audio[:overlap] * mix))
        sf.write(str(wav), joined, sample_rate, subtype='PCM_16')
    target = SOUNDS / (family + '_' + media_id + '.ogg')
    subprocess.run([str(FFMPEG),'-hide_banner','-loglevel','error','-y','-i',str(wav),'-c:a','libvorbis','-q:a','6',str(target)], check=True)
    return source, target

#创建随机变体与轻微音调波动，连续武器采用持续声音而非逐发叠播。
def sound_def(root, name, family, pitch, volume):
    sound = ET.SubElement(root, 'SoundDef')
    ET.SubElement(sound,'defName').text = name
    ET.SubElement(sound,'context').text = 'MapOnly'
    ET.SubElement(sound,'maxVoices').text = '8' if family == 'Stalwart' else '5'
    ET.SubElement(sound,'maxSimultaneous').text = '3'
    loop = family.endswith('Loop')
    if loop:
        ET.SubElement(sound,'sustain').text = 'true'
        ET.SubElement(sound,'priorityMode').text = 'PrioritizeNearest'
        ET.SubElement(sound,'sustainFadeoutTime').text = '0.10'
    sub = ET.SubElement(ET.SubElement(sound,'subSounds'),'li')
    ET.SubElement(sub,'volumeRange').text = f'{volume*.95:.1f}~{volume:.1f}'
    ET.SubElement(sub,'pitchRange').text = f'{pitch*.98:.3f}~{pitch*1.02:.3f}'
    ET.SubElement(sub,'distRange').text = '8~65' if volume < 80 else '10~85'
    if loop:
        ET.SubElement(sub,'sustainLoop').text = 'true'
        ET.SubElement(sub,'muteWhenPaused').text = 'true'
    grains = ET.SubElement(sub,'grains')
    for media_id in FAMILIES[family][1]:
        grain = ET.SubElement(grains,'li', {'Class':'AudioGrain_Clip'})
        ET.SubElement(grain,'clipPath').text = 'Combat/HD2/' + family + '_' + media_id

#按本地定义是否已有字段选择Add或Replace，生成可审查的静态XML补丁。
def set_field(patch, xpath, key, value, exists):
    operation = ET.SubElement(patch,'Operation',{'Class':'PatchOperationReplace' if exists else 'PatchOperationAdd'})
    ET.SubElement(operation,'xpath').text = xpath + ('/' + key if exists else '')
    node = ET.SubElement(ET.SubElement(operation,'value'),key)
    if value is None:
        node.set('IsNull','True')
    else:
        node.text = str(value)

#输出UTF-8文本，保持中文资源记录可直接阅读。
def save_xml(path, root):
    path.parent.mkdir(parents=True, exist_ok=True)
    ET.indent(root, space='  ')
    path.write_text('<?xml version="1.0" encoding="utf-8"?>\n' + ET.tostring(root,encoding='unicode') + '\n', encoding='utf-8')

#保留现有伤害、射速和弹种，只改变声音、枪口闪光与弹丸外观。
def main():
    SOUNDS.mkdir(parents=True, exist_ok=True)
    WORK.mkdir(parents=True, exist_ok=True)
    provenance = []
    for family, (bank, media_ids, duration) in FAMILIES.items():
        for media_id in media_ids:
            source, target = process_audio(family, media_id, duration)
            provenance.append({'音色族':family,'Bank':bank,'媒体ID':media_id,'源路径':str(source),
                '源SHA256':hashlib.sha256(source.read_bytes()).hexdigest(),'导出路径':str(target.relative_to(MOD)),
                '用途说明':'Bank来源已确认；动作属于CLAP筛选与音色匹配，不代表原始事件名。'})
    sounds = ET.Element('Defs')
    patch = ET.Element('Patch')
    definitions = {}
    for path in (MOD/'1.6'/'Defs').rglob('*.xml'):
        for definition in ET.parse(path).getroot().findall('ThingDef'):
            name = definition.findtext('defName')
            if name: definitions[name] = definition
    projectiles = {}
    for weapon, (family, pitch, volume) in WEAPONS.items():
        definition = definitions[weapon]
        sound_name = 'MihoCombat_Fire_' + weapon
        sound_def(sounds, sound_name, family, pitch, volume)
        reload_family = 'ReloadEnergy' if family in ('Railgun','LaserLoop','FlameLoop') else 'ReloadHeavy' if family in ('Autocannon','Recoilless','Eat','Stalwart') else 'ReloadMagazine'
        reload_start = 'MihoCombat_Reload_' + weapon
        reload_finish = reload_start + '_Complete'
        sound_def(sounds,reload_start,reload_family,pitch,42)
        sound_def(sounds,reload_finish,'ReloadLock',pitch,38)
        for index, verb in enumerate(definition.findall('verbs/li'), start=1):
            xpath = f'Defs/ThingDef[defName="{weapon}"]/verbs/li[{index}]'
            secondary = verb.findtext('secondary') == 'true'
            if family == 'FlameLoop':
                set_field(patch,xpath,'soundCastBeam',sound_name,verb.find('soundCastBeam') is not None)
                continue
            if secondary:
                secondary_family = 'GrenadeLaunch' if 'GrenadeLauncher' in weapon else 'Punisher'
                secondary_sound = sound_name + '_Underbarrel'
                sound_def(sounds,secondary_sound,secondary_family,1,74)
                cast = secondary_sound
            else:
                cast = None if family == 'LaserLoop' else sound_name
            set_field(patch,xpath,'soundCast',cast,verb.find('soundCast') is not None)
            set_field(patch,xpath,'soundCastTail',None,verb.find('soundCastTail') is not None)
            set_field(patch,xpath,'muzzleFlashScale',0 if family == 'LaserLoop' else 1.3,
                      verb.find('muzzleFlashScale') is not None)
            if verb.get('Class') == 'MihoAdfqs.Combat.Weapons.VerbProperties_Magazine':
                if secondary:
                    secondary_reload = reload_start + '_Underbarrel'
                    sound_def(sounds,secondary_reload,'ReloadHeavy',1.1,42)
                    set_field(patch,xpath,'reloadSound',secondary_reload,False)
                else:
                    set_field(patch,xpath,'reloadSound',reload_start,False)
                set_field(patch,xpath,'reloadCompleteSound',reload_finish,False)
            names = [verb.findtext('defaultProjectile')] + [node.text for node in verb.findall('ammunition/li')]
            for name in filter(None,names):
                projectiles[name] = family
    sound_def(sounds,'MihoCombat_GrenadeBlast','GrenadeBlast',1,78)
    sound_def(sounds,'MihoCombat_HeavyBlast','HeavyBlast',.9,87)
    sound_def(sounds,'MihoCombat_OrbitalBlast','HeavyBlast',.72,90)
    projectiles['MihoPhase2_Projectile_SupernovaFusionGrenade'] = 'Fusion'
    for name, family in projectiles.items():
        definition = definitions[name]
        payload = definition.find('modExtensions/li[@Class="MihoAdfqs.Combat.Projectiles.AreaPayload"]')
        shotgun = payload is not None and payload.findtext('orientToFlight') == 'true'
        explosive = family in ('Fusion','Eruptor','GrenadeLaunch','Eat','Autocannon') or payload is not None and not shotgun
        if name.endswith('HeavyAP') or 'Fortress30' in name:
            explosive = False
        color = '(0.35,0.78,1,1)' if family == 'Railgun' else '(1,0.17,0.06,1)' if family == 'LaserLoop' else '(1,0.6,0.18,1)'
        if 'Nanite' in name: color = '(0.38,0.94,0.3,1)'
        if 'Radiation' in name: color = '(0.7,0.92,0.2,1)'
        size = 4 if family == 'Fusion' else 2.5 if explosive else .65 if family in ('Railgun','Sniper','Recoilless') else .4
        operation = ET.SubElement(patch,'Operation',{'Class':'PatchOperationAddModExtension'})
        ET.SubElement(operation,'xpath').text = f'Defs/ThingDef[defName="{name}"]'
        extension = ET.SubElement(ET.SubElement(operation,'value'),'li',{'Class':'MihoAdfqs.Combat.Effects.CombatVisuals'})
        for key, value in {'color':color,'tracerWidth':0 if family == 'Fusion' else .17 if family in ('Railgun','Autocannon') else .085,
            'tracerLength':3 if family == 'Railgun' else 1.6,'impactScale':size,'energy':str(family in ('Railgun','LaserLoop')).lower(),
            'beam':str(family == 'LaserLoop').lower(),'smoke':str(explosive).lower(),
            'physicalProjectile':str(explosive or name.endswith('HeavyAP') or 'Fortress30' in name).lower()}.items():
            ET.SubElement(extension,key).text = str(value)
        if family == 'LaserLoop': ET.SubElement(extension,'loopSound').text = 'MihoCombat_Fire_MihoPhase2_SupernovaLaserRifle'
        if explosive:
            blast = 'MihoCombat_HeavyBlast' if family in ('Fusion','Autocannon') else 'MihoCombat_GrenadeBlast'
            xpath = f'Defs/ThingDef[defName="{name}"]/projectile'
            projectile_node = definition.find('projectile')
            set_field(patch,xpath,'soundExplode',blast,projectile_node.find('soundExplode') is not None)
            if payload is not None: ET.SubElement(extension,'impactSound').text = blast
    save_xml(MOD/'1.6'/'Defs'/'Phase2'/'Effects'/'CombatSounds.xml',sounds)
    save_xml(MOD/'1.6'/'Patches'/'CombatPresentation.xml',patch)
    audit = MOD/'Assets'/'CombatVfx'/'HD2音频来源.csv'
    with audit.open('w',encoding='utf-8-sig',newline='') as stream:
        writer = csv.DictWriter(stream,fieldnames=list(provenance[0]))
        writer.writeheader(); writer.writerows(provenance)
    mapping = MOD/'Docs'/'战斗音画映射.md'
    lines = ['# 战斗音画映射','', '本机《绝地潜兵2》音频的个人本地适配，原始Bank、WEM和OGG保留在 E:\\ModdevMics\\Assets\\Helldivers2\\MihoCombat。',
        '', '音效来源由Bank与媒体ID确认，开火/爆炸/循环片段由CLAP候选筛选和剪辑确定；这不是Wwise原始事件名映射，也未进行游戏内听感验收。',
        '', '| 武器 | 音色来源Bank | 音调 | 音量 |','|---|---|---:|---:|']
    for name,(family,pitch,volume) in WEAPONS.items():
        lines.append(f'| {definitions[name].findtext("label")} | {FAMILIES[family][0]} | {pitch} | {volume} |')
    lines += ['', 'HK416下挂分别使用榴弹发射器与惩罚者霰弹候选。爆炸使用反坦克手雷与大型火箭弹音库，轨道轰炸使用更低音调的大型爆炸。',
        '', '循环声音进行120毫秒交叉淡化，开火声切除静音前缀、缩短尾音；OGG统一44.1kHz单声道，有限增益和限幅，随机变体避免机械重复。',
        '', '装弹开始与完成分别播放装填和上膛声，传统弹匣、重武器和能量武器使用不同候选；下挂拥有独立装填声，喷火器燃料任务也播放开始与完成音效。',
        '', '三个程序化Shader分别处理亮芯曳光/连续激光/喷火锥、冲击环/火花、烟尘；使用ChezhouLib导入Windows资源包。伤害、弹匣和射速保留原定义。',
        '', '游戏素材权利属于原权利方；此导入用于本机个人原型，未取得公开分发许可。']
    mapping.write_text('\n'.join(lines)+'\n',encoding='utf-8')
    print(f'已导出 {len(provenance)} 个音频变体，覆盖 {len(WEAPONS)} 件远程武器/炮台，{len(projectiles)} 种弹头。',flush=True)

if __name__ == '__main__':
    main()
