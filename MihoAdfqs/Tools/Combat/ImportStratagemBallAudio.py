import csv
import hashlib
import json
import struct
import subprocess
import xml.etree.ElementTree as ET
from pathlib import Path

MOD = Path(__file__).resolve().parents[2]
RAW = Path(r'E:\ModdevMics\Assets\Helldivers2\StratagemBall')
FFMPEG = Path(r'E:\ModdevMics\Tools\Filediver\v0.7.55\filediver-cli\ffmpeg.exe')
DECODER = Path(r'C:\Users\30978\.codex\skills\extract-helldivers2-assets\scripts\wem_decoder.exe')
#红色事件747886和蓝色事件1032351623共用706071705层；每个随机组各取一个原始变体。
LAYERS = ((465632577, .30, 0), (612965449, .45, -4), (497066532, .20, -2))
FILENAME = 'CallIn_StratagemBall'


#按DIDX索引提取战备球公共展开层，保留原始Bank和WEM。
def extract_layers():
    bank = RAW/'content/audio/foley_player.bnk'
    data = bank.read_bytes()
    chunks, position = {}, 0
    while position < len(data):
        tag, size = struct.unpack_from('<4sI', data, position)
        chunks[tag] = (position + 8, size)
        position += 8 + size
    offset, size = chunks[b'DIDX']
    base = chunks[b'DATA'][0]
    required = {media for media, _, _ in LAYERS}
    wem = RAW/'Wem'
    wem.mkdir(exist_ok=True)
    for position in range(offset, offset + size, 12):
        media, start, length = struct.unpack_from('<III', data, position)
        if media in required:
            (wem/f'{media}.wem').write_bytes(data[base+start:base+start+length])
            required.remove(media)
    if required:
        raise RuntimeError(f'foley_player缺少战备球媒体：{required}')
    subprocess.run([str(DECODER), str(wem), str(RAW/'Ogg'), str(FFMPEG), '3'], check=True)
    return bank


#合成公共启动层，保留原事件的层次、相对增益和展开顺序。
def import_call_in():
    bank = extract_layers()
    target = MOD/'Sounds/Combat/HD2/Support'
    output = target/(FILENAME+'.ogg')
    inputs, filters, provenance = [], [], []
    for index, (media, delay, gain) in enumerate(LAYERS):
        source = RAW/'Ogg'/f'{media}.ogg'
        inputs += ['-i', str(source)]
        filters.append(f'[{index}:a]volume={gain}dB,adelay={round(delay*1000)}:all=1[a{index}]')
        provenance.append({'用途': 'CallIn', 'Bank': 'foley_player', '媒体ID': str(media), '相似度': '',
            '源SHA256': hashlib.sha256(source.read_bytes()).hexdigest(), '源路径': str(source),
            '导出路径': str(output.relative_to(MOD)),
            '说明': f'战备红/蓝启动事件747886/1032351623公共层706071705；延迟{delay:.2f}秒，增益{gain}dB；随机组固定变体。'})
    filters.append('[a0][a1][a2]amix=inputs=3:normalize=0,alimiter=limit=0.90:level=false[out]')
    subprocess.run([str(FFMPEG), '-hide_banner', '-loglevel', 'error', '-y', *inputs,
        '-filter_complex', ';'.join(filters), '-map', '[out]', '-ar', '44100', '-ac', '1',
        '-c:a', 'libvorbis', '-q:a', '6', str(output)], check=True)
    sound = ET.fromstring(f'''<SoundDef>
      <defName>MihoSupport_CallIn</defName><context>Any</context>
      <maxVoices>4</maxVoices><maxSimultaneous>2</maxSimultaneous>
      <subSounds><li><onCamera>true</onCamera><volumeRange>44.2~48</volumeRange>
      <pitchRange>0.980~1.020</pitchRange><grains><li Class="AudioGrain_Clip">
      <clipPath>Combat/HD2/Support/{FILENAME}</clipPath></li></grains></li></subSounds>
    </SoundDef>''')
    evidence = {'参数来源': 'https://github.com/shalzuth/HelldiversData/blob/master/data/entities/StratagemBallComponentData.json',
        '原始Bank': str(bank), 'BankSHA256': hashlib.sha256(bank.read_bytes()).hexdigest(),
        '红色启动事件': 747886, '蓝色启动事件': 1032351623, '公共展开层': 706071705,
        '红色播放动作': 1003338319, '蓝色播放动作': 226268198,
        '媒体': [{'ID': media, '延迟秒': delay, '增益dB': gain,
            'WemSHA256': hashlib.sha256((RAW/'Wem'/f'{media}.wem').read_bytes()).hexdigest()}
            for media, delay, gain in LAYERS],
        '说明': '从本机Bank事件图提取公共展开层。随机延迟采用中间值，随机媒体组各固定一个变体；不包含红蓝专属持续提示层或Wwise插件生成的声音。'}
    (Path(__file__).with_name('StratagemBallEvent.json')).write_text(
        json.dumps(evidence, ensure_ascii=False, indent=2)+'\n', encoding='utf-8')
    return sound, provenance


#单独替换启动声，其他支援音频不重新导入。
def main():
    sound, provenance = import_call_in()
    path = MOD/'1.6/Defs/Phase2/Effects/ImperialSupportSounds.xml'
    root = ET.parse(path).getroot()
    previous = next(node for node in root if node.findtext('defName') == 'MihoSupport_CallIn')
    position = list(root).index(previous)
    root.remove(previous)
    root.insert(position, sound)
    ET.indent(root, space='  ')
    path.write_text('<?xml version="1.0" encoding="utf-8"?>\n'+ET.tostring(root, encoding='unicode')+'\n', encoding='utf-8')
    manifest = MOD/'Sounds/Combat/HD2/Support/HD2支援音频来源.csv'
    with manifest.open(encoding='utf-8-sig') as stream:
        rows = [row for row in csv.DictReader(stream) if row['用途'] != 'CallIn']
    with manifest.open('w', encoding='utf-8-sig', newline='') as stream:
        writer = csv.DictWriter(stream, fieldnames=list(provenance[0]))
        writer.writeheader()
        writer.writerows(provenance + rows)
    print(f'战备球公共展开声：{FILENAME}.ogg', flush=True)


if __name__ == '__main__':
    main()
