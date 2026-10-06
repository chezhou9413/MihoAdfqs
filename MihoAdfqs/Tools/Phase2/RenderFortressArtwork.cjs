const fs = require('fs');
const path = require('path');
const sharp = require('C:/Users/30978/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/sharp');

//复用底座和炮管原画，通过SVG图层生成建造图标和完整蓝图。
async function main() {
    const mod = path.resolve(__dirname, '../..');
    const textures = path.join(mod, 'Textures/Building/MihoPhase2/Fortress');
    const source = path.join(mod, 'Assets/UI/Buildings');
    fs.mkdirSync(source, {recursive: true});
    const image = name => 'data:image/png;base64,' + fs.readFileSync(path.join(textures, name + '.png')).toString('base64');
    const base = image('MihoPhase2_ImperialFortressBase');
    for (const [name, top] of [['30mm', 'Autocannon30mm'], ['88mm', 'Cannon88mm']]) {
        const svg = `<svg xmlns="http://www.w3.org/2000/svg" xmlns:xlink="http://www.w3.org/1999/xlink" width="512" height="512" viewBox="0 0 512 512"><image width="512" height="512" xlink:href="${base}"/><g transform="translate(256 256) rotate(180) scale(${6.1502 / 6.2061} 1) translate(-256 -256)"><image width="512" height="512" xlink:href="${image('MihoPhase2_ImperialFortress_' + top)}"/></g></svg>`;
        const filename = 'MihoPhase2_ImperialFortressComplete' + name;
        fs.writeFileSync(path.join(source, filename + '.svg'), svg, 'utf8');
        await sharp(Buffer.from(svg)).png().toFile(path.join(textures, filename + '.png'));
    }
    console.log('已导出两种完整堡垒建造图与蓝图。');
}

main().catch(error => { console.error(error); process.exitCode = 1; });
