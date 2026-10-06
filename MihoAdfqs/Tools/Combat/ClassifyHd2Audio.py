import csv
from collections import Counter
from pathlib import Path
import numpy as np
import soundfile as sf
import librosa
import torch
from transformers import ClapModel, ClapProcessor

ROOT = Path(r'E:\ModdevMics\Assets\Helldivers2\MihoCombat')
CACHE = Path(r'E:\ModdevMics\Tools\Helldivers2AudioClassifier\hf_cache\models--laion--clap-htsat-unfused\snapshots\8fa0f1c6d0433df6e97c127f64b2a1d6c0dcda8a')
PROMPTS = {
    '开火': 'The loud explosive crack of a gun firing a single shot, a rifle gunshot sound effect.',
    '爆炸': 'A powerful explosion with a deep bass boom and debris, a grenade explosion sound effect.',
    '激光': 'A continuous science fiction laser beam firing with an electronic buzzing hum.',
    '火焰': 'A flamethrower spraying a continuous roaring stream of burning flames.',
    '装填': 'A gun being reloaded, metallic clicks, cocking a weapon and inserting a magazine.',
    '空壳': 'A bullet shell casing dropping to the ground and clinking on a hard surface.',
    '操作': 'A soft mechanical click, button or switch, a small movement of equipment.',
    '语音': 'A person speaking words, a human voice talking.',
}

#依据原始Bank和媒体ID筛选候选，保留分类置信度供人工核对。
def main():
    torch.set_num_threads(6)
    groups = {}
    with (ROOT / '音频库内嵌媒体映射.csv').open(encoding='utf-8-sig') as stream:
        media = list(csv.DictReader(stream))
    frequency = Counter(row['内嵌媒体ID'] for row in media)
    for row in media:
            path = ROOT / 'Ogg' / (row['内嵌媒体ID'] + '.ogg')
            if not path.exists():
                continue
            info = sf.info(str(path))
            bank = row['音频库路径']
            continuous = 'scythe' in bank or 'flamethrower' in bank
            if not 0.14 <= info.duration <= (16 if continuous else 3.5):
                continue
            audio, rate = sf.read(str(path), dtype='float32', always_2d=True)
            mono = audio.mean(axis=1)
            peak = float(np.max(np.abs(mono)))
            if peak < 0.025:
                continue
            onset = np.flatnonzero(np.abs(mono) > peak * 0.25)
            attack = float(np.mean(mono[:min(len(mono), int(rate * .15))] ** 2))
            total = float(np.mean(mono ** 2))
            rank = peak * (0.4 + min(3, attack / max(total, 1e-9)))
            if len(onset) and onset[0] / rate > .2:
                rank *= .4
            if frequency[path.stem] == 1:
                rank *= 5
            groups.setdefault(bank, []).append((rank, path, info.duration))
    processor = ClapProcessor.from_pretrained(str(CACHE), local_files_only=True)
    model = ClapModel.from_pretrained(str(CACHE), local_files_only=True).eval()
    with torch.inference_mode():
        text = model.get_text_features(**processor(text=list(PROMPTS.values()), padding=True, return_tensors='pt')).pooler_output
        text = torch.nn.functional.normalize(text, dim=-1)
    rows = []
    categories = list(PROMPTS)
    for bank, candidates in groups.items():
        candidates.sort(key=lambda item: item[0], reverse=True)
        #火焰和激光的循环声不强调瞬态，因此额外保留最长的候选。
        selected = candidates[:16]
        if 'scythe' in bank or 'flamethrower' in bank:
            selected += sorted(candidates, key=lambda item: item[2], reverse=True)[:14]
        seen = set()
        for _, path, duration in selected:
            if path.stem in seen:
                continue
            seen.add(path.stem)
            audio, _ = librosa.load(str(path), sr=48000, mono=True)
            with torch.inference_mode():
                features = model.get_audio_features(**processor(audio=audio, sampling_rate=48000, return_tensors='pt')).pooler_output
                similarity = (torch.nn.functional.normalize(features, dim=-1) @ text.T)[0].numpy()
            order = np.argsort(similarity)[::-1]
            probabilities = torch.softmax(torch.tensor(similarity) * 20, dim=0).numpy()
            rows.append({'Bank': bank, 'MediaID': path.stem, 'Duration': round(duration, 4),
                         'Category': categories[order[0]], 'Score': round(float(similarity[order[0]]), 4),
                         'Gap': round(float(probabilities[order[0]] - probabilities[order[1]]), 4),
                         'ShotScore': round(float(similarity[0]), 4), 'ExplosionScore': round(float(similarity[1]), 4),
                         'BeamScore': round(float(similarity[2]), 4), 'FlameScore': round(float(similarity[3]), 4),
                         'Path': str(path)})
        print(f'{bank}: 已识别 {len(seen)} 个候选', flush=True)
        with (ROOT / 'candidates.csv').open('w', encoding='utf-8-sig', newline='') as stream:
            writer = csv.DictWriter(stream, fieldnames=list(rows[0]))
            writer.writeheader()
            writer.writerows(rows)

if __name__ == '__main__':
    main()
