import csv
import hashlib
import subprocess
import xml.etree.ElementTree as ET
from pathlib import Path
import numpy as np
import soundfile as sf
import librosa
import torch
from transformers import ClapModel, ClapProcessor
from ImportStratagemBallAudio import import_call_in

MOD = Path(__file__).resolve().parents[2]
RAW = Path(r'E:\ModdevMics\Assets\Helldivers2\ImperialSupport')
CACHE = Path(r'E:\ModdevMics\Tools\Helldivers2AudioClassifier\hf_cache\models--laion--clap-htsat-unfused\snapshots\8fa0f1c6d0433df6e97c127f64b2a1d6c0dcda8a')
FFMPEG = Path(r'E:\ModdevMics\Tools\Filediver\v0.7.55\filediver-cli\ffmpeg.exe')
#来源Bank与用途分开记录，语义候选不冒充Wwise原始事件名。
FAMILIES = {
    'Incoming': (('stratagems_orbital_380mm_he',), 'An incoming artillery shell whistling and rushing through the air before impact.', .3, 4.5, 1, 1.0, 45),
    'Barrage120': (('stratagems_orbital_380mm_he',), 'An artillery shell exploding with a sharp explosive bang and debris.', .5, 6, 2, 1.13, 70),
    'Barrage380': (('stratagems_orbital_380mm_he',), 'A massive artillery explosion with a deep rumbling bass boom and debris.', .5, 8, 2, .86, 80),
    'Napalm': (('stratagems_orbital_napalm_barrage',), 'An incendiary bomb exploding with a fiery blast and roaring flames.', .5, 8, 2, .95, 72),
    'Gas': (('stratagems_orbital_gas_strike',), 'A gas canister bursting and hissing, releasing a cloud of gas.', .2, 6, 1, 1.0, 57),
    'EMP': (('stratagems_orbital_ems_strike',), 'A powerful electromagnetic pulse with an electrical discharge and buzzing energy blast.', .3, 7, 1, 1.0, 65),
}

#读取可独立解码的媒体，排除已保留的预取片段与静音。
def read_candidates():
    groups = {}
    with (RAW/'音频库内嵌媒体映射.csv').open(encoding='utf-8-sig') as stream:
        for row in csv.DictReader(stream):
            bank = Path(row['音频库路径']).name
            if bank not in {b for family in FAMILIES.values() for b in family[0]}:
                continue
            path = RAW/'Ogg'/(row['内嵌媒体ID']+'.ogg')
            if not path.exists():
                continue
            info = sf.info(str(path))
            if not .12 <= info.duration <= 8:
                continue
            samples, rate = sf.read(str(path), dtype='float32', always_2d=True)
            peak = float(np.max(np.abs(samples)))
            if peak < .025:
                continue
            onset = float(np.mean(samples[:min(len(samples),int(rate*.2))]**2))
            energy = float(np.mean(samples**2))
            rank = peak * (1+min(3,onset/max(energy,1e-9)))
            groups.setdefault(bank,[]).append((rank,path,info.duration))
    return {bank:sorted(items,reverse=True)[:64] for bank,items in groups.items()}

#按指定Bank与声音用途筛选，保留每个候选的相似度。
def main():
    torch.set_num_threads(6)
    groups = read_candidates()
    processor = ClapProcessor.from_pretrained(str(CACHE),local_files_only=True)
    model = ClapModel.from_pretrained(str(CACHE),local_files_only=True).eval()
    names = list(FAMILIES)
    with torch.inference_mode():
        features = model.get_text_features(**processor(text=[FAMILIES[n][1] for n in names],padding=True,return_tensors='pt')).pooler_output
        text_features = torch.nn.functional.normalize(features,dim=-1)
    scores, rows = {}, []
    for bank,items in groups.items():
        for _,path,duration in items:
            if path.stem not in scores:
                audio,_ = librosa.load(str(path),sr=48000,mono=True)
                with torch.inference_mode():
                    features = model.get_audio_features(**processor(audio=audio,sampling_rate=48000,return_tensors='pt')).pooler_output
                    scores[path.stem] = (torch.nn.functional.normalize(features,dim=-1) @ text_features.T)[0].numpy()
            rows.append({'Bank':bank,'MediaID':path.stem,'Duration':duration,
                **{name:round(float(scores[path.stem][i]),5) for i,name in enumerate(names)}})
        print(f'{bank}: 已分析{len(items)}个候选',flush=True)
    with (RAW/'support-candidates.csv').open('w',encoding='utf-8-sig',newline='') as stream:
        writer = csv.DictWriter(stream,fieldnames=list(rows[0])); writer.writeheader(); writer.writerows(rows)
    sounds = ET.Element('Defs')
    call_in, provenance = import_call_in()
    sounds.append(call_in)
    target = MOD/'Sounds/Combat/HD2/Support'
    target.mkdir(parents=True,exist_ok=True)
    for name,(banks,prompt,minimum,maximum,count,pitch,volume) in FAMILIES.items():
        candidates = sorted((row for row in rows if row['Bank'] in banks and minimum<=row['Duration']<=maximum),key=lambda row:row[name],reverse=True)
        selected = []; seen = set()
        for row in candidates:
            if row['MediaID'] in seen: continue
            selected.append(row); seen.add(row['MediaID'])
            if len(selected)==count: break
        if len(selected)!=count: raise RuntimeError(f'{name}缺少可用候选')
        sound = ET.SubElement(sounds,'SoundDef')
        ET.SubElement(sound,'defName').text = 'MihoSupport_'+name
        ET.SubElement(sound,'context').text = 'Any' if name=='CallIn' else 'MapOnly'
        ET.SubElement(sound,'maxVoices').text = '6' if name=='Barrage120' else '4'
        ET.SubElement(sound,'maxSimultaneous').text = '6' if name=='Barrage120' else '4' if name in ('Barrage380','Napalm') else '2'
        sub = ET.SubElement(ET.SubElement(sound,'subSounds'),'li')
        if name=='CallIn': ET.SubElement(sub,'onCamera').text = 'true'
        ET.SubElement(sub,'volumeRange').text = f'{volume*.92:.1f}~{volume}'
        ET.SubElement(sub,'pitchRange').text = f'{pitch*.98:.3f}~{pitch*1.02:.3f}'
        if name!='CallIn': ET.SubElement(sub,'distRange').text = '10~100'
        grains = ET.SubElement(sub,'grains')
        for row in selected:
            source = RAW/'Ogg'/(row['MediaID']+'.ogg')
            filename = name+'_'+row['MediaID']
            output = target/(filename+'.ogg')
            filters = 'silenceremove=start_periods=1:start_duration=0.003:start_threshold=-45dB,highpass=f=40,alimiter=limit=0.90:level=false'
            #来袭声压缩到实体炮弹的半秒飞行段，避免落地后仍拖着长哨声。
            if name == 'Incoming':
                speed = row['Duration'] / .5
                while speed > 2:
                    filters += ',atempo=2'; speed /= 2
                filters += f',atempo={speed:.6f},atrim=duration=0.5,afade=t=out:st=0.45:d=0.05'
            subprocess.run([str(FFMPEG),'-hide_banner','-loglevel','error','-y','-i',str(source),'-af',filters,'-ar','44100','-ac','1','-c:a','libvorbis','-q:a','6',str(output)],check=True)
            grain = ET.SubElement(grains,'li',{'Class':'AudioGrain_Clip'})
            ET.SubElement(grain,'clipPath').text = 'Combat/HD2/Support/'+filename
            provenance.append({'用途':name,'Bank':row['Bank'],'媒体ID':row['MediaID'],'相似度':row[name],
                '源SHA256':hashlib.sha256(source.read_bytes()).hexdigest(),'源路径':str(source),
                '导出路径':str(output.relative_to(MOD)),'说明':'本机HD2原始Bank媒体；用途为CLAP匹配候选，非Wwise事件名。'})
        print(name,[(row['MediaID'],row[name]) for row in selected],flush=True)
    ET.indent(sounds,space='  ')
    (MOD/'1.6/Defs/Phase2/Effects/ImperialSupportSounds.xml').write_text('<?xml version="1.0" encoding="utf-8"?>\n'+ET.tostring(sounds,encoding='unicode')+'\n',encoding='utf-8')
    with (target/'HD2支援音频来源.csv').open('w',encoding='utf-8-sig',newline='') as stream:
        writer = csv.DictWriter(stream,fieldnames=list(provenance[0])); writer.writeheader(); writer.writerows(provenance)

if __name__=='__main__':
    main()
