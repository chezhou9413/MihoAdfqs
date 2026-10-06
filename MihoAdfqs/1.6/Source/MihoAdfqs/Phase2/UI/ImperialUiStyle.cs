using UnityEngine;
using Verse;

namespace MihoAdfqs.Phase2.UI
{
    //以黑色底板、炭黑卡片和血红按钮统一帝国界面。
    [StaticConstructorOnStartup]
    internal static class ImperialUiStyle
    {
        public static readonly Color Ink = new Color(0.96f, 0.95f, 0.92f);
        public static readonly Color Muted = new Color(0.76f, 0.76f, 0.76f);
        public static readonly Color Red = new Color(0.055f, 0.055f, 0.055f);
        public static readonly Color PanelRed = new Color(0.115f, 0.115f, 0.115f);
        public static readonly Color DeepRed = new Color(0.025f, 0.025f, 0.025f);
        public static readonly Color Edge = new Color(0.46f, 0.10f, 0.07f);
        public static readonly Color ButtonFace = new Color(0.38f, 0.065f, 0.04f);
        public static readonly Color Highlight = new Color(0.53f, 0.09f, 0.06f);
        public static readonly Color Accent = new Color(0.94f, 0.25f, 0.18f);
        public static readonly Color Signal = new Color(0.91f, 0.90f, 0.87f);
        public static readonly Color Warning = new Color(0.98f, 0.53f, 0.43f);
        public static readonly Color IconTint = new Color(0.90f, 0.90f, 0.88f);
        public static readonly Texture2D Cross = ContentFinder<Texture2D>.Get("UI/MihoPhase2/Communications/HeadquartersCross");
        public static readonly Texture2D CommandBackground = SolidColorMaterials.NewSolidColorTexture(ButtonFace);
        public static readonly Texture2D BarFill = SolidColorMaterials.NewSolidColorTexture(Accent);
        public static readonly Texture2D BarEmpty = SolidColorMaterials.NewSolidColorTexture(DeepRed);

        //炭黑卡片比黑色外框更亮，以暗红边线分隔内容。
        public static void Panel(Rect rect)
        {
            Widgets.DrawBoxSolidWithOutline(rect, PanelRed, Edge);
        }

        //保留标志轮廓的亮度，绘制完恢复调用方的乘色。
        public static void Mark(Rect rect)
        {
            Color color = GUI.color;
            GUI.color = IconTint;
            Widgets.DrawTextureFitted(rect, Cross, 1);
            GUI.color = color;
        }
    }
}
