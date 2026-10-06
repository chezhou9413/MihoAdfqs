"""用项目贴图组合人物与建筑内部，仅作为介绍图示意。"""
from pathlib import Path
from PIL import Image, ImageDraw
from artwork import TEX

MILIRA = Path("E:/steam/steamapps/workshop/content/294100/3256974620/Content/Textures/Milira/Pawn")
MILIRA_FA = Path("E:/steam/steamapps/workshop/content/294100/3260848700/Content1.6/Textures")


#头部图层一起缩放，并以下颌对齐衣领，头发与帽子不会各自漂移。
def pawn_sprite(kind):
    result=Image.new("RGBA",(512,660))
    def layer(path,y=140,x=0):
        result.alpha_composite(Image.open(TEX/path).convert("RGBA"),(x,y))
    def head_layer(path,scale=.75,neck=337):
        image=Image.open(path).convert("RGBA")
        side=round(512*scale)
        image=image.resize((side,side),Image.Resampling.LANCZOS)
        result.alpha_composite(image,(round(256-256*scale),round(neck-332*scale)))
    if kind=="tiana":
        for wing in ("BodyAddon/RightWing/RightWingFront_south.png", "BodyAddon/LeftWing/LeftWingFront_south.png"):
            result.alpha_composite(Image.open(MILIRA/wing).convert("RGBA"),(0,140))
        head_layer(TEX/"SpecialPawn/MihoPhase2/Tiana/HairBack/MihoPhase2_TianaHairBack_south.png",.85)
        result.alpha_composite(Image.open(MILIRA/"Body/Naked_Female_south.png").convert("RGBA"),(0,140))
        layer("apparel/MihoPhase2/TianaChant/MihoPhase2_TianaChant_south.png")
        #使用游戏实际的空白脸型，再覆盖提亚娜的五官，避免叠出两套眼睛。
        head_layer(MILIRA_FA/"Things/Pawn/Milira/Heads_Blank/MiliraHead/Unisex/normal_south.png",.85)
        head_layer(TEX/"SpecialPawn/MihoPhase2/Tiana/Face/MihoPhase2_TianaFace_Open_south.png",.85)
        head_layer(TEX/"SpecialPawn/MihoPhase2/Tiana/Hair/MihoPhase2_TianaHair_south.png",.85)
        head_layer(TEX/"SpecialPawn/MihoPhase2/Tiana/Halo/MihoPhase2_TianaHalo_south.png",.85)
    elif kind=="helmer":
        layer("AlienRace/adfqs/MihoPhase2/HelmerZog/Tail/MihoPhase2_HelmerZogTail_south.png")
        layer("AlienRace/adfqs/MihoPhase2/HelmerZog/Body/MihoPhase2_HelmerZogBody_south.png")
        layer("apparel/MihoPhase2/HelmerZogSuit/MihoPhase2_HelmerZogSuit_south.png")
        head_layer(TEX/"FacialAnimation/Adfqs/Head/Unisex/normal_south.png")
        for path in ("Eyes/Unisex/MihoPhase2_HelmerZogEye_Normal_south.png", "Brows/Unisex/MihoPhase2_HelmerZogBrow_Normal_south.png", "Mouth/Unisex/MihoPhase2_HelmerZogMouth_Closed_south.png"):
            head_layer(TEX/"FacialAnimation/MihoPhase2/HelmerZog"/path)
        head_layer(TEX/"AlienRace/adfqs/MihoPhase2/HelmerZog/Hair/MihoPhase2_HelmerZogHair_south.png")
        head_layer(TEX/"AlienRace/adfqs/MihoPhase2/HelmerZog/Ear/MihoPhase2_HelmerZogEar_south.png")
    else:
        if kind=="fuhrer":
            layer("AlienRace/adfqs/Tail/Tail_south.png")
        layer("AlienRace/adfqs/body/body_south.png")
        clothing="Fuhrer/FuhrerCoat_Female_south.png" if kind=="fuhrer" else "MihoPhase2/SupernovaDropArmor/MihoPhase2_SupernovaDropArmor_south.png"
        layer("apparel/"+clothing)
        head_layer(TEX/"AlienRace/adfqs/head/head_south.png")
        head_layer(TEX/"AlienRace/adfqs/hair/hair_south.png")
        head_layer(TEX/"AlienRace/adfqs/Ear/ear_south.png")
        if kind=="fuhrer":
            head_layer(TEX/"apparel/Fuhrer/FuhrerHelmet_south.png")
        else:
            head_layer(TEX/"apparel/MihoPhase2/SupernovaDropHelmet/MihoPhase2_SupernovaDropHelmet_south.png")
    return result.crop(result.getbbox())


#患者示意与顶盖分开，展示医疗舱的上下层关系。
def medical_sprite():
    base=Image.open(TEX/"Building/MihoPhase2/MedicalPod/MihoPhase2_SupernovaMedicalPodInterior_south.png").convert("RGBA")
    patient=pawn_sprite("trooper")
    patient.thumbnail((120,238))
    base.alpha_composite(patient,((512-patient.width)//2,96))
    base.alpha_composite(Image.open(TEX/"Building/MihoPhase2/MedicalPod/MihoPhase2_SupernovaMedicalPodCover_south.png").convert("RGBA"))
    return base


#内部植物与顶盖共享同一原始画布，窗口和外壳不会因裁剪边界不同而错位。
def cultivator_sprite():
    base=Image.open(TEX/"Building/MihoPhase2/IndoorCultivator/MihoPhase2_IndoorCultivatorInterior_east.png").convert("RGBA")
    draw=ImageDraw.Draw(base)
    for a in range(3):
        for b in range(5):
            x=212+a*45; y=140+b*47
            draw.line([(x,y+22),(x,y-3)],fill="#91ad63",width=3)
            draw.ellipse((x-21,y-11,x+2,y+2),fill="#5d954c")
            draw.ellipse((x+1,y-22,x+24,y-8),fill="#7dad5e")
    base.alpha_composite(Image.open(TEX/"Building/MihoPhase2/IndoorCultivator/MihoPhase2_IndoorCultivatorCover_east.png").convert("RGBA"))
    return base
