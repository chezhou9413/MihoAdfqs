"""面向玩家的简短模块介绍。"""
import math
from artwork import Artwork, DEEP, EDGE, ACCENT, INK, MUTED, BLUE
from sprites import pawn_sprite, medical_sprite, cultivator_sprite


#封面只介绍派系与已有内容。
def overview():
    art=Artwork(1050,"美狐第三帝国","帝国狂想曲","美狐派系扩展")
    art.cross(878,437,390,EDGE,DEEP)
    art.text(70,246,"一群全副武装的狐狸",43,role="title")
    art.paragraph(74,320,"第三帝国带来了新的武器、服装和建筑，也有几位特别的客人会来到你的殖民地。",510,32)
    art.image(pawn_sprite("fuhrer"),664,257,380,440)
    art.image("Weapon/MihoPhase2/MihoPhase2_SupernovaHeavyLauncher.png",70,475,387,178,rotate=-44)
    art.image("Building/MihoPhase2/Fortress/MihoPhase2_ImperialFortressComplete88mm.png",369,472,264,244)
    art.rule(52,767,1096,"与第三帝国打交道")
    art.paragraph(74,835,"用轨道通讯器联络帝国，接受委托、换取补给，或在战斗中呼叫舰炮和援军。帝国也有自己的研究与装备。",1047,31)
    return art


#挑选几类武器说明用途。
def weapons():
    art=Artwork(1130,"帝国军械","步枪、机枪、火箭筒，还有超新星系列武器。","武器")
    rows=[("制式武器","Weapon/mg42.png","从鲁格手枪到MG42机枪，给你的士兵配上一套帝国军备。"),
          ("HK416下挂","Weapon/MihoPhase2/MihoPhase2_HK416GrenadeLauncher.png","步枪下面还能带一门榴弹发射器或霰弹枪，需要时切换使用。"),
          ("电磁与激光","Weapon/MihoPhase2/MihoPhase2_SupernovaLaserRifle.png","电磁步枪用来精准射击，激光步枪则以持续光束攻击敌人。"),
          ("超新星重炮","Weapon/MihoPhase2/MihoPhase2_SupernovaHeavyLauncher.png","可以换用穿甲、高爆、纳米毒气和辐射弹，按敌人的情况选择。")]
    for i,(title,path,copy) in enumerate(rows):
        y=224+i*195
        art.image(path,70,y,365,150,rotate=-44 if i>1 else 0)
        art.text(488,y+9,title,37,role="bold")
        art.paragraph(488,y+65,copy,609,30)
        if i<3:
            art.line([(52,y+178),(1148,y+178)],EDGE)
    return art


#角色按身份和主要能力介绍，不列生成条件与数值。
def people():
    art=Artwork(1260,"三位特殊角色","秋霜、赫尔默与提亚娜。","人物")
    rows=[("fuhrer","阿道夫·秋霜","第三帝国的元首。加入殖民地后，可以召来亲卫援助，也能带你与帝国建立联系。"),
          ("helmer","赫尔默·佐格","元首的亲卫队长。擅长枪战，能进入血液风暴，还会翻滚躲避攻击。"),
          ("tiana","提亚娜","帝国的首位米莉拉公民。灵感上来时，能做出传奇品质的武器和衣服。")]
    for i,(kind,name,copy) in enumerate(rows):
        y=230+i*312
        art.ellipse(90,y+218,285,43,DEEP)
        art.image(pawn_sprite(kind),83,y-6,298,272)
        art.text(454,y+48,name,44,role="title")
        art.paragraph(454,y+124,copy,637,32)
        if i<2:
            art.line([(52,y+289),(1148,y+289)],EDGE)
    return art


#护盾的图解只配穿戴者能感受到的效果。
def shield(art,cx,cy,radius,color,stage="normal"):
    art.ellipse(cx-radius,cy-radius,radius*2,radius*2,DEEP,color,2)
    for j in range(-7,8):
        for i in range(-7,8):
            u=(i+(j%2)*.5)*.225; v=j*.1949
            if math.hypot(u,v)>1.43 or stage=="break" and (i*19+j*7)%5<2:
                continue
            pts=[]
            for edge in range(6):
                a=math.pi/3*edge+math.pi/6; b=a+math.pi/3
                for k in range(4):
                    t=k/4; p=u+.125*((1-t)*math.cos(a)+t*math.cos(b)); q=v+.125*((1-t)*math.sin(a)+t*math.sin(b))
                    rho=math.hypot(p,q); scale=math.sin(rho)/max(rho,.001)
                    dx=18*((i*7+j)%3-1) if stage=="break" else 0
                    pts.append((cx+radius*p*scale+dx,cy+radius*q*scale))
            hit=stage=="hit" and math.hypot(u-.55,v+.18)<.48
            shade="#793844" if hit else "#233842" if color==BLUE else "#44372d"
            art.polygon(pts,shade,"#e68a81" if hit else color,1)
    art.ellipse(cx-radius+3,cy-radius+3,radius*2-6,radius*2-6,None,INK,1)


#两件护盾装备使用现有贴图。
def shields():
    art=Artwork(1140,"个人护盾","替穿戴者挡下伤害，破碎之后会重新充能。","装备")
    shield(art,335,498,237,BLUE)
    art.image(pawn_sprite("trooper"),220,332,230,325)
    art.image("apparel/MihoPhase2/HaliviShieldBelt/MihoPhase2_HaliviShieldBelt.png",666,242,142,104)
    art.text(660,378,"哈利维护盾腰带",37,role="bold")
    art.paragraph(660,438,"展开金色护盾，为穿戴者提供保护。",444,31)
    art.image("apparel/MihoPhase2/SupernovaShieldGenerator/MihoPhase2_SupernovaShieldGenerator.png",666,555,142,104)
    art.text(660,691,"超新星护盾发生器",37,role="bold")
    art.paragraph(660,752,"蓝色护盾更能承受攻击，充能也更快。",444,31)
    art.rule(52,914,1096,"受击、破碎，再重新展开")
    art.paragraph(74,981,"打中的地方会变红、凹陷。护盾被击碎时，能量薄片会散开，恢复之后重新拼合。",1050,30)
    return art


#建筑说明集中在玩家能使用的功能。
def buildings():
    art=Artwork(1320,"帝国建筑","守住殖民地，也照顾日常的生产与生活。","建筑")
    rows=[("帝国堡垒","Building/MihoPhase2/Fortress/MihoPhase2_ImperialFortressComplete88mm.png","可以换装30毫米或88毫米炮，守卫殖民地。记得给它补充钢铁。"),
          ("医疗舱",medical_sprite(),"治疗伤势、补回缺失的部件，还能复活死者或改变种族。"),
          ("农用培养机",cultivator_sprite(),"在室内种植作物，成熟后自动收割。播种和照料还是要靠你的种植人员。"),
          ("工业设备","Building/MihoPhase2/MihoPhase2_Supernova3DPrinter.png","打印机、聚合器和药物制造台，为帝国装备提供材料和补给。")]
    for i,(title,image,copy) in enumerate(rows):
        y=222+i*252
        art.image(image,82,y,280,213)
        art.text(432,y+28,title,40,role="bold")
        art.paragraph(432,y+98,copy,661,31)
        if i<3:
            art.line([(52,y+236),(1148,y+236)],EDGE)
    return art
