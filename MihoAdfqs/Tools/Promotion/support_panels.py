"""给玩家介绍轨道支援的用途。"""
import json
from artwork import Artwork, ROOT, EDGE, MUTED, BLUE


#用游戏中的图标介绍各项战备。
def support_icon(art,kind,x,y,size):
    art.image(f"UI/MihoPhase2/Support/{kind}.png",x,y,size,size)


#攻击与增援使用各自的红蓝图标，介绍页不列参数表。
def orbital_panel():
    art=Artwork(1500,"轨道支援","联络第三帝国，呼叫舰炮、援军或物资补给。","支援")
    data=json.loads((ROOT/"Assets/Promotion/support.json").read_text(encoding="utf-8"))
    for i,row in enumerate(data["strikes"]):
        x=52+i%2*558; y=227+i//2*209
        support_icon(art,row["icon"],x+12,y+20,112)
        art.text(x+145,y+23,row["name"],33,role="bold")
        art.paragraph(x+145,y+83,row["intro"],388,29)
        if i<4:
            art.line([(x,y+190),(x+535,y+190)],EDGE)
    art.rule(52,883,1096,"援军与补给",BLUE)
    for i,row in enumerate(data["supplies"]):
        x=55+i*219
        support_icon(art,row["icon"],x+46,962,107)
        art.text(x+99,1102,row["name"],28,anchor="center",role="bold")
        art.paragraph(x+5,1160,row["intro"],195,26,line_height=37)
    art.line([(52,1312),(1148,1312)],EDGE)
    art.text(72,1343,"战备支援兵也会自己呼叫炮击、空降兵和临时堡垒。",29)
    art.text(72,1390,"炮击也会波及友军，记得留出撤离的地方。",27,MUTED)
    return art
