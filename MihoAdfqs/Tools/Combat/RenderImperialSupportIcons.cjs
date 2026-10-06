const fs = require('fs');
const path = require('path');
const sharp = require('C:/Users/30978/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/sharp');

//把手绘矢量图导出为透明游戏纹理，同时生成现有支援的拼图。
async function main() {
  const mod = path.resolve(__dirname, '../..');
  const source = path.join(mod, 'Assets/UI/Support');
  const target = path.join(mod, 'Textures/UI/MihoPhase2/Support');
  fs.mkdirSync(target, {recursive:true});
  const markerTarget = path.join(target, 'Markers');
  fs.mkdirSync(markerTarget, {recursive:true});
  const names = {Bombard:'轨道精准打击 · 1发', Barrage120:'轨道120mm火力网', Napalm:'轨道凝固汽油弹火力网', Barrage380:'轨道380mm火力网', Gas:'轨道毒气打击', EMP:'轨道EMP打击', Soldiers:'轨道士兵', Medicine:'药物补给', Blueprint:'新星蓝图', Guards:'召唤亲卫', Mobilize:'永久动员'};
  const height = Math.ceil(Object.keys(names).length/6)*310;
  let preview = `<svg xmlns="http://www.w3.org/2000/svg" width="1536" height="${height}" viewBox="0 0 1536 ${height}"><rect width="1536" height="${height}" fill="#202323"/>`;
  let markerPreview = '<svg xmlns="http://www.w3.org/2000/svg" width="1100" height="280" viewBox="0 0 1100 280"><rect width="1100" height="140" fill="#71654f"/><rect y="140" width="1100" height="140" fill="#bcbfb2"/>';
  let index = 0;
  for (const [name,label] of Object.entries(names)) {
    const svg = fs.readFileSync(path.join(source,name+'.svg'),'utf8');
    await sharp(Buffer.from(svg), {density:288}).resize(512,512).png().toFile(path.join(target, name+'.png'));
    //地图图标单独加浅色细轮廓，保持透明，面板图标沿用暗色调。
    const marker = svg.replace(/(<svg[^>]*>)/, '$1<defs><filter id="markerEdge" x="-12%" y="-12%" width="124%" height="124%"><feMorphology in="SourceAlpha" operator="dilate" radius="1.3" result="expanded"/><feFlood flood-color="#e7e3d6" flood-opacity=".95" result="edgeColor"/><feComposite in="edgeColor" in2="expanded" operator="in" result="solidEdge"/><feComposite in="solidEdge" in2="SourceAlpha" operator="out" result="outline"/><feMerge><feMergeNode in="outline"/><feMergeNode in="SourceGraphic"/></feMerge></filter></defs><g filter="url(#markerEdge)">').replace('</svg>', '</g></svg>');
    await sharp(Buffer.from(marker), {density:288}).resize(512,512).png().toFile(path.join(markerTarget, name+'.png'));
    for (let row=0;row<2;row++) {
      const ringColor = index < 6 ? '#c74750' : '#4a94bf';
      markerPreview += `<g transform="translate(${index*100+6},${row*140+8})"><circle cx="44" cy="44" r="40" fill="none" stroke="${ringColor}" stroke-width="2.5"/>${marker.replace('<svg ', '<svg x="10" y="10" width="68" height="68" ')}<text x="44" y="108" text-anchor="middle" font-family="Microsoft YaHei" font-size="12" fill="${row ? '#24272a' : '#f1ede2'}">${label.replace('轨道','').replace('火力网','').replace('凝固汽油弹','燃烧弹')}</text></g>`;
    }
    preview += `<g transform="translate(${index%6*256},${Math.floor(index/6)*310+12})" opacity=".8">${svg.replace('<svg ', '<svg x="24" y="12" width="208" height="208" ')}<text x="128" y="265" text-anchor="middle" font-family="Microsoft YaHei" font-size="19" fill="#d6d1c4">${label}</text></g>`;
    index++;
  }
  const shellDir = path.join(mod, 'Textures/Thing/MihoPhase2');
  fs.mkdirSync(shellDir,{recursive:true});
  await sharp(path.join(source,'OrbitalShell.svg'), {density:288}).resize(128,256).png().toFile(path.join(shellDir,'OrbitalShell.png'));
  const previewFile = 'E:/ModdevMics/Outputs/MihoAdfqs/ImperialSupportIcons.png';
  fs.mkdirSync(path.dirname(previewFile),{recursive:true});
  await sharp(Buffer.from(preview+'</svg>')).png().toFile(previewFile);
  await sharp(Buffer.from(markerPreview+'</svg>')).png().toFile(path.join(path.dirname(previewFile), 'ImperialSupportMarkerPreview.png'));
  console.log('已导出'+Object.keys(names).length+'类支援图标与实体轨道炮弹纹理：'+previewFile);
}
main().catch(error => { console.error(error); process.exitCode=1; });
