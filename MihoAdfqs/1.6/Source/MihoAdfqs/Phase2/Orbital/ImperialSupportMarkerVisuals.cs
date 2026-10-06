using MihoAdfqs.Combat.Effects;
using MihoAdfqs.Phase2.UI;
using UnityEngine;
using Verse;

namespace MihoAdfqs.Phase2.Orbital
{
    //定位信标与地图倒计时复用现有光束材质和原版抗锯齿绘制。
    internal static class ImperialSupportMarkerVisuals
    {
        //地图平面上的纵向投影形成向屏幕上方延伸的光柱，底端固定在支援中心。
        internal static void Beacon(IntVec3 cell, float seconds)
        {
            Vector3 origin = cell.ToVector3Shifted();
            Vector3 top = origin + Vector3.forward * 18f;
            float phase = Mathf.Repeat(seconds / 1.25f, 1f);
            float pulse = .5f + .5f * Mathf.Sin(phase * Mathf.PI * 2f);
            CombatVfxMaterials.Beam(origin, top, new Color(.85f, .012f, .026f, .08f + pulse * .04f),
                1.5f + pulse * .2f, false);
            CombatVfxMaterials.Beam(origin, top, new Color(.95f, .025f, .045f, .45f + pulse * .16f),
                .65f + pulse * .1f, true);
            CombatVfxMaterials.Burst(origin, 1.2f, new Color(.9f, .018f, .035f, .28f + pulse * .12f),
                0f, 0f);
            CombatVfxMaterials.Burst(origin, 1.2f + phase * 2.4f, new Color(.9f, .025f, .045f, .35f),
                phase, 0f, ring: true);
        }

        //计时条按文字测量后的尺寸绘制，圆角背景衬托秒数与进度。
        internal static void Countdown(Rect rect, string seconds, float remaining, bool active, bool strike)
        {
            Color accent = active ? ImperialUiStyle.Warning
                : strike ? ImperialUiStyle.Accent : ImperialUiStyle.Signal;
            if (Event.current.type == EventType.Repaint)
            {
                Rounded(rect, new Color(.46f, .10f, .07f, .95f), rect.height * .5f);
                Rect inner = rect.ContractedBy(1f);
                Rounded(inner, new Color(.025f, .025f, .025f, .94f), inner.height * .5f);
                Vector2 center = new Vector2(rect.x + 18f, rect.center.y);
                Arc(center, 11f, 1f, new Color(.32f, .32f, .32f, .75f));
                Arc(center, 11f, remaining, accent);
                //剩余弧线的首尾使用圆形端点，部分进度不再跳整段。
                if (remaining > 0f)
                {
                    Dot(center + Vector2.down * 11f, accent);
                    float angle = remaining * Mathf.PI * 2f - Mathf.PI * .5f;
                    Dot(center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * 11f, accent);
                }
            }
            GUI.color = ImperialUiStyle.Ink;
            Widgets.Label(new Rect(rect.x + 34f, rect.y, rect.width - 42f, rect.height), seconds);
            GUI.color = Color.white;
        }

        //抗锯齿圆弧保留末段的真实角度，在刻之间平滑收缩。
        private static void Arc(Vector2 center, float radius, float progress, Color color)
        {
            const int segments = 96;
            float end = progress * segments;
            for (int i = 0; i < Mathf.CeilToInt(end); i++)
            {
                float a = i * Mathf.PI * 2f / segments - Mathf.PI * .5f;
                float b = Mathf.Min(i + 1f, end) * Mathf.PI * 2f / segments - Mathf.PI * .5f;
                Vector2 start = center + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * radius;
                Vector2 finish = center + new Vector2(Mathf.Cos(b), Mathf.Sin(b)) * radius;
                Widgets.DrawLine(start, finish, color, .8f);
            }
        }

        //圆形端帽与计时条共用Unity圆角纹理绘制，不生成临时贴图。
        private static void Dot(Vector2 center, Color color)
        {
            Rounded(new Rect(center.x - 1.2f, center.y - 1.2f, 2.4f, 2.4f), color, 1.2f);
        }

        //使用实际矩形高度计算圆角半径。
        private static void Rounded(Rect rect, Color color, float radius)
        {
            GUI.DrawTexture(rect, BaseContent.WhiteTex, ScaleMode.StretchToFill, true, 0f, color, 0f, radius);
        }
    }
}
