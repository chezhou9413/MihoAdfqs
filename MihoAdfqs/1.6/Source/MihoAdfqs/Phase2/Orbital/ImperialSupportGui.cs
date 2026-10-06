using MihoAdfqs.Phase2.Communications;
using MihoAdfqs.Phase2.UI;
using UnityEngine;
using Verse;

namespace MihoAdfqs.Phase2.Orbital
{
    //支援面板共享黑色底板、明亮文字与红色选中边线。
    [StaticConstructorOnStartup]
    internal static class ImperialSupportGui
    {
        public static readonly Color Ink = ImperialUiStyle.Ink;
        public static readonly Color Muted = ImperialUiStyle.Muted;
        private static readonly Color Accent = ImperialUiStyle.Accent;
        private static readonly Color Background = ImperialUiStyle.Red;
        private static readonly Color Card = ImperialUiStyle.PanelRed;
        private static readonly Color Edge = ImperialUiStyle.Edge;
        private static readonly Color IconBackground = new Color(.07f, .07f, .07f);
        private static readonly Color IconEdge = ImperialUiStyle.Edge;
        public static readonly Texture2D BarFill = ImperialUiStyle.BarFill;
        public static readonly Texture2D BarEmpty = ImperialUiStyle.BarEmpty;

        //整体外框保留简洁线条，不使用金属纹理。
        public static void DrawShell(Rect rect) => Widgets.DrawBoxSolidWithOutline(rect,Background,Edge);

        //目录卡片比外框更亮，用清晰边线分隔。
        public static void Panel(Rect rect) => Widgets.DrawBoxSolidWithOutline(rect,Card,Edge);

        //图标使用独立深灰底框并保留原色，红色打击图示不再融入红色卡片。
        public static void Icon(Rect rect, Texture2D texture)
        {
            Color previous = GUI.color;
            GUI.color = Color.white;
            Widgets.DrawBoxSolidWithOutline(rect.ExpandedBy(4), IconBackground, IconEdge);
            Widgets.DrawTextureFitted(rect, texture, 1f);
            GUI.color = previous;
        }

        //可用按钮以红色突出，禁用按钮保持文字可读并停止悬停反馈。
        public static bool Command(Rect rect,string label,float hover,bool hangUp=false,bool disabled=false)
        {
            if (disabled) hover = 0;
            Color face = disabled ? ImperialUiStyle.DeepRed : hangUp ? Card : ImperialUiStyle.ButtonFace;
            Color highlight = hangUp ? Card : ImperialUiStyle.Highlight;
            Widgets.DrawBoxSolidWithOutline(rect,Color.Lerp(face,highlight,hover),Color.Lerp(Edge,Accent,hover));
            if (hover>0) Widgets.DrawBoxSolid(new Rect(rect.x+1,rect.y+1,3,rect.height-2),Accent);
            HeadquartersGui.Label(new Rect(rect.x+12,rect.y+6,rect.width-24,rect.height-12),
                label,disabled || hangUp ? Muted : Ink);
            return !disabled && Widgets.ButtonInvisible(rect);
        }
    }
}
