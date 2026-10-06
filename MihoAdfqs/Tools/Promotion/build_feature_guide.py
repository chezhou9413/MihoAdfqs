"""生成六张玩家介绍图、SVG源稿、工坊长图和本地预览。"""
from pathlib import Path
import argparse
import html
import shutil
import xml.etree.ElementTree as ET
from PIL import Image, ImageDraw
from artwork import BASE, INK, font
from feature_panels import overview, weapons, people, shields, buildings
from support_panels import orbital_panel


#生成发布图片和可编辑源稿。
def main():
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--output",type=Path,default=Path("E:/ModdevMics/Outputs/MihoAdfqs/PlayerIntroduction"))
    args=parser.parse_args()
    output=args.output.resolve(); output.mkdir(parents=True,exist_ok=True)
    panels=[("01_美狐第三帝国",overview),("02_帝国人物",people),("03_帝国军械",weapons),
            ("04_轨道支援",orbital_panel),("05_帝国建筑",buildings),("06_个人护盾",shields)]
    images=[]
    web=output/"Workshop640"; web.mkdir(exist_ok=True)
    for name,builder in panels:
        art=builder(); image=art.save(output,name); images.append(image)
        ET.parse(output/f"{name}.svg")
        image.resize((640,round(image.height*640/1200)),Image.Resampling.LANCZOS).save(web/f"{name}.png",optimize=True)
        print(f"生成 {name}：1200 × {image.height}")
    contact=Image.new("RGB",(1200,900),BASE)
    for i,((name,_),image) in enumerate(zip(panels,images)):
        preview=image.copy(); preview.thumbnail((382,397),Image.Resampling.LANCZOS)
        x=9+i%3*400+(382-preview.width)//2; y=12+i//3*450
        contact.paste(preview,(x,y))
        d=ImageDraw.Draw(contact)
        d.text((14+i%3*400,y+410),name[3:],font=font(12),fill=INK)
    contact.save(output/"总览拼图.png",optimize=True)
    long_image=Image.new("RGB",(1200,sum(image.height for image in images)),BASE)
    y=0
    for image in images:
        long_image.paste(image,(0,y)); y+=image.height
    long_image.save(output/"工坊介绍长图.png",optimize=True)
    figures="\n".join(f'<figure><a href="{html.escape(name)}.png" target="_blank"><img src="{html.escape(name)}.png" alt="{html.escape(name[3:])}"></a><figcaption>{html.escape(name[3:])} <a href="{html.escape(name)}.svg">SVG源稿</a></figcaption></figure>' for name,_ in panels)
    preview=f'''<!doctype html><html lang="zh-CN"><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><title>美狐第三帝国</title>
<style>body{{margin:0;background:#240609;color:#ece3d5;font:18px DengXian,SimHei,sans-serif}}header{{max-width:1240px;margin:44px auto;padding:0 22px}}h1{{font-size:34px}}p{{color:#bca8a2}}main{{max-width:1280px;margin:auto;display:grid;grid-template-columns:repeat(2,minmax(0,1fr));gap:28px;padding:22px}}figure{{margin:0}}img{{display:block;width:100%;height:auto}}figcaption{{padding:12px}}a{{color:#d9a19a}}figcaption a{{float:right}}@media(max-width:760px){{main{{grid-template-columns:1fr}}}}</style>
<header><h1>美狐第三帝国</h1><p>认识帝国的角色、军备、支援和建筑。</p><a href="工坊介绍长图.png">完整长图</a></header><main>{figures}</main></html>'''
    (output/"预览.html").write_text(preview,encoding="utf-8")
    archive=shutil.make_archive(str(output.parent/"MihoAdfqs_玩家介绍图"),"zip",output)
    print(f"输出：{output}\n打包：{archive}")


if __name__=="__main__":
    main()
