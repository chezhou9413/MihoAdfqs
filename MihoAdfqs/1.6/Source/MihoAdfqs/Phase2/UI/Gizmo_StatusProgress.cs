using System;
using UnityEngine;
using Verse;

namespace MihoAdfqs.Phase2.UI
{
    //以铁十字红色面板显示器官成长、护盾和建筑资源。
    public class Gizmo_StatusProgress : Gizmo
    {
        private readonly string title;
        private readonly Func<float> progress;
        private readonly Func<string> value;
        private readonly Func<string> detail;

        //每次绘制从提供器读取当前状态。
        public Gizmo_StatusProgress(string title, Func<float> progress, Func<string> value, Func<string> detail)
        {
            this.title = title; this.progress = progress; this.value = value; this.detail = detail;
            Order = -90;
        }

        //宽度遵循当前Gizmo网格的可用空间。
        public override float GetWidth(float maxWidth) => Mathf.Min(maxWidth, 230f);

        //按实际字体行高布局，放不下的说明保留在提示中。
        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
        {
            GameFont font = Text.Font;
            TextAnchor anchor = Text.Anchor;
            bool wrap = Text.WordWrap;
            Color color = GUI.color;
            try
            {
                var rect = new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), 75f);
                GUI.color = Color.white;
                ImperialUiStyle.Panel(rect);
                Text.Font = GameFont.Tiny; Text.Anchor = TextAnchor.MiddleLeft; Text.WordWrap = false;
                float titleHeight = Mathf.Max(18, Text.LineHeight);
                ImperialUiStyle.Mark(new Rect(rect.x + 6, rect.y + 5, 17, 17));
                GUI.color = ImperialUiStyle.Ink;
                Rect titleRect = new Rect(rect.x + 29, rect.y + 3, rect.width - 35, titleHeight);
                Widgets.Label(titleRect, title.Truncate(titleRect.width));
                Text.Font = GameFont.Small; Text.Anchor = TextAnchor.MiddleCenter;
                var bar = new Rect(rect.x + 6, titleRect.yMax + 3, rect.width - 12, Text.LineHeight + 2);
                GUI.color = Color.white;
                Widgets.FillableBar(bar, Mathf.Clamp01(progress()), ImperialUiStyle.BarFill, ImperialUiStyle.BarEmpty, true);
                GUI.color = ImperialUiStyle.Ink;
                Widgets.Label(bar, value());
                Text.Font = GameFont.Tiny; Text.Anchor = TextAnchor.MiddleLeft;
                if (bar.yMax + Text.LineHeight + 6 <= rect.yMax)
                {
                    GUI.color = ImperialUiStyle.Muted;
                    Rect detailRect = new Rect(rect.x + 6, bar.yMax + 2, rect.width - 12, Text.LineHeight);
                    Widgets.Label(detailRect, detail().Truncate(detailRect.width));
                }
                TooltipHandler.TipRegion(rect, title + "\n" + value() + "\n" + detail());
                return new GizmoResult(GizmoState.Clear);
            }
            finally { Text.Font = font; Text.Anchor = anchor; Text.WordWrap = wrap; GUI.color = color; }
        }
    }
}
