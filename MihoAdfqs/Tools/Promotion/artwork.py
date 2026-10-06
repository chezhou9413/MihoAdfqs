"""用同一套绘图指令输出 PNG 和可编辑 SVG。"""
from pathlib import Path
from io import BytesIO
import base64
import html
import math
from functools import lru_cache
from PIL import Image, ImageDraw, ImageFont

ROOT = Path(__file__).resolve().parents[2]
TEX = ROOT / "Textures"
BASE, DEEP, PANEL = "#420e11", "#240609", "#4f1316"
EDGE, ACCENT, INK, MUTED, BLUE = "#712c2d", "#ba6b64", "#ece3d5", "#bca8a2", "#6094aa"
FONTS = {"title": "simhei.ttf", "body": "Deng.ttf", "bold": "Dengb.ttf", "number": "bahnschrift.ttf"}


#所有字体与贴图读取都缓存，排版反复测量时不重复打开文件。
@lru_cache(None)
def font(size, role="body"):
    return ImageFont.truetype(str(Path("C:/Windows/Fonts") / FONTS[role]), int(size * 2))


@lru_cache(None)
def texture(relative):
    image = Image.open(TEX / relative).convert("RGBA")
    return image.crop(image.getbbox())


#输出图使用两倍分辨率绘制，再缩小以保留细线和中文边缘。
class Artwork:
    def __init__(self, height, title, subtitle, topic):
        self.width, self.height = 1200, height
        self.bitmap = Image.new("RGBA", (2400, height * 2), BASE)
        self.draw = ImageDraw.Draw(self.bitmap)
        self.svg = [f'<svg xmlns="http://www.w3.org/2000/svg" width="1200" height="{height}" viewBox="0 0 1200 {height}">']
        self.rect(0, 0, 1200, height, BASE)
        self.polygon([(24, 24), (1176, 24), (1176, height-52), (1148, height-24), (24, height-24)], None, EDGE, 2)
        self.rect(24, 24, 1152, 16, DEEP)
        self.cross(82, 112, 80, INK, DEEP)
        self.text(150, 69, title, 53, role="title")
        self.text(150, 133, subtitle, 25, MUTED)
        self.line([(52, 185), (1148, 185)], EDGE, 2)
        self.rect(52, 182, 112, 6, ACCENT)
        self.text(52, height-73, "帝国狂想曲  /  美狐第三帝国", 22, MUTED)
        self.text(1148, height-73, topic, 22, MUTED, anchor="right")

    #排版的实色矢量直接写入画布，避免每条细线分配整张图层。
    def paint(self, callback):
        callback(self.draw)

    def rect(self, x, y, w, h, fill, stroke=None, sw=1, radius=0):
        box = (int(x*2), int(y*2), int((x+w)*2), int((y+h)*2))
        self.paint(lambda d: d.rounded_rectangle(box, radius=int(radius*2), fill=fill, outline=stroke, width=int(sw*2)))
        self.svg.append(f'<rect x="{x}" y="{y}" width="{w}" height="{h}" rx="{radius}" fill="{fill or "none"}" stroke="{stroke or "none"}" stroke-width="{sw}"/>')

    def polygon(self, points, fill=None, stroke=None, sw=1):
        scaled = [(int(x*2), int(y*2)) for x, y in points]
        def draw(d):
            d.polygon(scaled, fill=fill)
            if stroke:
                d.line(scaled+[scaled[0]], fill=stroke, width=int(sw*2), joint="curve")
        self.paint(draw)
        pts = " ".join(f"{x:.2f},{y:.2f}" for x, y in points)
        self.svg.append(f'<polygon points="{pts}" fill="{fill or "none"}" stroke="{stroke or "none"}" stroke-width="{sw}" stroke-linejoin="round"/>')

    def line(self, points, color=ACCENT, width=2):
        self.paint(lambda d: d.line([(int(x*2), int(y*2)) for x,y in points], fill=color, width=int(width*2), joint="curve"))
        pts = " ".join(f"{x:.2f},{y:.2f}" for x,y in points)
        self.svg.append(f'<polyline points="{pts}" fill="none" stroke="{color}" stroke-width="{width}"/>')

    def ellipse(self, x, y, w, h, fill=None, stroke=None, sw=1):
        self.paint(lambda d: d.ellipse((int(x*2), int(y*2), int((x+w)*2), int((y+h)*2)), fill=fill, outline=stroke, width=int(sw*2)))
        self.svg.append(f'<ellipse cx="{x+w/2}" cy="{y+h/2}" rx="{w/2}" ry="{h/2}" fill="{fill or "none"}" stroke="{stroke or "none"}" stroke-width="{sw}"/>')

    def text_width(self, text, size, role="body"):
        return font(size, role).getlength(text)/2

    def text(self, x, y, value, size=29, color=INK, role="body", anchor="left"):
        width = self.text_width(value, size, role)
        left = x-width if anchor=="right" else x-width/2 if anchor=="center" else x
        if left < 0 or left+width > self.width+1 or y+size > self.height:
            raise ValueError(f"文字越界：{value}")
        self.draw.text((int(left*2), int(y*2)), value, font=font(size, role), fill=color, anchor="lt")
        family = {"title":"SimHei", "body":"DengXian", "bold":"DengXian", "number":"Bahnschrift"}[role]
        weight = "700" if role in ("title", "bold") else "400"
        self.svg.append(f'<text x="{left:.2f}" y="{y}" fill="{color}" font-family="{family}" font-size="{size}" font-weight="{weight}" dominant-baseline="text-before-edge">{html.escape(value)}</text>')

    #按中文实际字宽换行，长段落不会越过说明区域。
    def paragraph(self, x, y, text, width, size=28, color=INK, line_height=None, role="body"):
        line_height = line_height or int(size*1.42)
        for paragraph in text.split("\n"):
            line = ""
            for char in paragraph:
                if line and self.text_width(line+char, size, role) > width and char not in "，。；！？：、）”」》…":
                    self.text(x, y, line, size, color, role)
                    y += line_height
                    line = ""
                line += char
            if line:
                self.text(x, y, line, size, color, role)
            y += line_height
        return y

    def image(self, image, x, y, w, h, rotate=0, crop=True):
        if isinstance(image, str):
            image = texture(image)
        if rotate:
            image = image.rotate(rotate, Image.Resampling.BICUBIC, expand=True)
        if crop:
            image = image.crop(image.getbbox())
        scale = min(w/image.width, h/image.height)
        iw, ih = image.width*scale, image.height*scale
        ix, iy = x+(w-iw)/2, y+(h-ih)/2
        resized = image.resize((round(iw*2),round(ih*2)), Image.Resampling.LANCZOS)
        self.bitmap.alpha_composite(resized, (round(ix*2),round(iy*2)))
        stream = BytesIO()
        image.save(stream, format="PNG")
        data = base64.b64encode(stream.getvalue()).decode("ascii")
        self.svg.append(f'<image x="{ix:.2f}" y="{iy:.2f}" width="{iw:.2f}" height="{ih:.2f}" href="data:image/png;base64,{data}"/>')

    #铁十字只作为项目既有视觉徽记，采用浅色描边和内层血红。
    def cross(self, cx, cy, size, outer=INK, inner=BASE):
        #先用常见四臂轮廓，再缩放绘制内层，保证中心连接。
        shape = [(-.25,-.5),(.25,-.5),(.16,-.16),(.5,-.25),(.5,.25),(.16,.16),(.25,.5),(-.25,.5),(-.16,.16),(-.5,.25),(-.5,-.25),(-.16,-.16)]
        self.polygon([(cx+x*size,cy+y*size) for x,y in shape], outer)
        self.polygon([(cx+x*size*.83,cy+y*size*.83) for x,y in shape], inner)

    def rule(self, x, y, w, title, color=ACCENT):
        self.rect(x, y, w, 43, DEEP)
        self.rect(x, y, 5, 43, color)
        self.text(x+17, y+7, title, 29, role="bold")

    def arrow(self, points, color=ACCENT, width=3):
        self.line(points, color, width)
        x,y=points[-1]; px,py=points[-2]; a=math.atan2(y-py,x-px)
        self.polygon([(x,y),(x-13*math.cos(a-.45),y-13*math.sin(a-.45)),(x-13*math.cos(a+.45),y-13*math.sin(a+.45))],color)

    def save(self, directory, name):
        directory.mkdir(parents=True, exist_ok=True)
        image = self.bitmap.convert("RGB").resize((1200,self.height), Image.Resampling.LANCZOS)
        image.save(directory / f"{name}.png", optimize=True)
        (directory / f"{name}.svg").write_text("\n".join(self.svg+["</svg>"]),encoding="utf-8")
        return image
