using MihoAdfqs.Phase2.UI;
using UnityEngine;
using Verse;

namespace MihoAdfqs.Phase2.Communications
{
    //提供红色铁十字联络台的配色和动态按钮。
    [StaticConstructorOnStartup]
    internal static class HeadquartersGui
    {
        public static readonly Color Ink = ImperialUiStyle.Ink;
        public static readonly Color Muted = ImperialUiStyle.Muted;
        public static readonly Color Accent = ImperialUiStyle.Accent;
        public static readonly Color Edge = ImperialUiStyle.Edge;
        public static readonly Color Signal = ImperialUiStyle.Signal;
        public static readonly Texture2D Cross = ImperialUiStyle.Cross;

        //用最暗的底板和横带衬托卡片及按钮。
        public static void DrawShell(Rect rect)
        {
            Widgets.DrawBoxSolid(rect, ImperialUiStyle.Red);
            Widgets.DrawBoxSolid(new Rect(rect.x, rect.y, rect.width, 10), ImperialUiStyle.DeepRed);
            Widgets.DrawBoxSolid(new Rect(rect.x, rect.yMax - 10, rect.width, 10), ImperialUiStyle.DeepRed);
            Color color = GUI.color;
            GUI.color = Edge;
            Widgets.DrawBox(rect.ContractedBy(15), 1);
            GUI.color = color;
        }

        //以较亮的炭黑内容区衬托浅色文字。
        public static void Panel(Rect rect) => ImperialUiStyle.Panel(rect);

        //绘制居中文本并恢复文字锚点和乘色。
        public static void Label(Rect rect, string text, Color color, TextAnchor anchor = TextAnchor.MiddleLeft)
        {
            Color previous = GUI.color;
            TextAnchor previousAnchor = Text.Anchor;
            GUI.color = color;
            Text.Anchor = anchor;
            Widgets.Label(rect, text);
            Text.Anchor = previousAnchor;
            GUI.color = previous;
        }

        //只在重绘时推进悬停动画，避免一次GUI事件重复积分。
        public static void UpdateHover(Rect rect, ref float hover, float delta)
        {
            if (Event.current.type == EventType.Repaint)
                hover = Mathf.MoveTowards(hover, Mouse.IsOver(rect) ? 1 : 0, delta * 7);
        }

        //用红色悬停渐变和移动箭头反馈输入。
        public static bool Command(Rect rect, string label, float hover, bool hangUp = false)
        {
            Color face = hangUp ? ImperialUiStyle.PanelRed : ImperialUiStyle.ButtonFace;
            Color highlight = hangUp ? ImperialUiStyle.PanelRed : ImperialUiStyle.Highlight;
            bool pressed = Mouse.IsOver(rect) && Input.GetMouseButton(0);
            Color fill = Color.Lerp(face, highlight, hover);
            if (pressed) fill *= .8f;
            fill.a = 1;
            Widgets.DrawBoxSolidWithOutline(rect, fill, Color.Lerp(Edge, Accent, hover * .55f));
            Widgets.DrawBoxSolid(new Rect(rect.x + 1, rect.y + 8, 3, rect.height - 16),
                Color.Lerp(Edge, Accent, hover * .55f));
            float shift = pressed ? 1 : 0;
            Label(new Rect(rect.x + 20 + shift, rect.y + 6 + shift, rect.width - 46,
                rect.height - 12), label, Ink);
            float x = rect.xMax - 16 + hover * 2;
            float y = rect.center.y;
            if (Event.current.type == EventType.Repaint)
            {
                Widgets.DrawLine(new Vector2(x - 4,y - 4), new Vector2(x,y), Accent, 1);
                Widgets.DrawLine(new Vector2(x,y), new Vector2(x - 4,y + 4), Accent, 1);
            }
            return Widgets.ButtonInvisible(rect);
        }
    }
}
