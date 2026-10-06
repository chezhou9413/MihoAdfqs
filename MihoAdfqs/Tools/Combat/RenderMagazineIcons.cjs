const fs = require('fs');
const path = require('path');
const sharp = require('C:/Users/30978/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/sharp');

//从可编辑的SVG导出透明弹药图标，保留各类弹药的原始比例。
async function main() {
    const mod = path.resolve(__dirname, '../..');
    const source = path.join(mod, 'Assets/UI/Weapons');
    const target = path.join(mod, 'Textures/UI/MihoPhase2/Weapons');
    for (const type of ['Round', 'Shell', 'Rocket', 'Grenade', 'Shotgun', 'Laser']) {
        const name = 'Magazine' + type;
        await sharp(fs.readFileSync(path.join(source, name + '.svg')), {density: 192})
            .png().toFile(path.join(target, name + '.png'));
    }
    console.log('已导出六种透明弹药图标。');
}

main().catch(error => { console.error(error); process.exitCode = 1; });
