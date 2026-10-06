using MihoAdfqs.Phase2.Communications;
using MihoAdfqs.Phase2.UI;
using UnityEngine;
using Verse;

namespace MihoAdfqs.Buildings.UI
{
    //按中文实际行高排列建筑面板的说明、操作按钮和进度。
    internal sealed class BuildingControlListing
    {
        private readonly float width;
        public float Height { get; private set; }

        //使用滚动内容区的宽度进行布局。
        public BuildingControlListing(float width) { this.width = width; }

        //显示可换行说明，不截断动态名称与禁用原因。
        public void Label(string text, bool muted = false)
        {
            Text.Anchor = TextAnchor.UpperLeft; Text.WordWrap = true;
            float height = Mathf.Max(Text.LineHeight, Text.CalcHeight(text, width));
            GUI.color = muted ? ImperialUiStyle.Muted : ImperialUiStyle.Ink;
            Widgets.Label(new Rect(0, Height, width, height), text);
            GUI.color = Color.white;
            Height += height + 10;
        }

        //按钮下直接显示禁用原因，悬停也可查看。
        public bool Button(string label, string disabled = null)
        {
            float height = Mathf.Max(44, Text.CalcHeight(label, width - 66) + 16);
            Rect rect = new Rect(0, Height, width, height);
            Text.WordWrap = true;
            bool clicked;
            if (disabled == null) clicked = HeadquartersGui.Command(rect, label, Mouse.IsOver(rect) ? 1 : 0);
            else
            {
                Widgets.DrawBoxSolidWithOutline(rect, ImperialUiStyle.DeepRed, ImperialUiStyle.Edge);
                HeadquartersGui.Label(new Rect(rect.x + 12, rect.y + 6, rect.width - 24, rect.height - 12), label, ImperialUiStyle.Muted);
                TooltipHandler.TipRegion(rect, disabled);
                clicked = false;
            }
            Height += height + 8;
            if (disabled != null) Label("不可用：" + disabled, true);
            return clicked;
        }

        //进度条与文字各占一行，避免中文盖住条形区域。
        public void Progress(float fraction, string text)
        {
            Label(text);
            Widgets.FillableBar(new Rect(0, Height, width, 12), Mathf.Clamp01(fraction),
                ImperialUiStyle.BarFill, ImperialUiStyle.BarEmpty, false);
            Height += 26;
        }
    }
}
