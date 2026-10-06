using UnityEngine;
using MihoAdfqs.Phase2.UI;
using Verse;

namespace MihoAdfqs.Phase2.Communications
{
    //绘制铁十字总部标识和暂停时仍播放的联络动效。
    public partial class Dialog_ImperialCommunications
    {
        //以平面铁十字标明总部，给接通状态留出独立位置。
        private void DrawHeader(Rect rect, float time)
        {
            ImperialUiStyle.Mark(new Rect(rect.x, rect.y + 4, 60, 60));
            float textX = rect.x + 78;
            float statusWidth = rect.width >= 650 ? 150 : 0;
            float titleWidth = rect.width - 78 - statusWidth;
            Text.Font = rect.width >= 650 ? GameFont.Medium : GameFont.Small;
            float titleHeight = Text.LineHeight;
            HeadquartersGui.Label(new Rect(textX, rect.y, titleWidth, titleHeight + 4),
                "帝国总部 · 联络台", HeadquartersGui.Ink);
            Text.Font = GameFont.Small;
            HeadquartersGui.Label(new Rect(textX, rect.y + titleHeight + 8, titleWidth, Text.LineHeight + 4),
                "殖民地专线 / 轨道司令部", HeadquartersGui.Muted);
            if (statusWidth > 0)
            {
                float pulse = 0.48f + Mathf.Sin(time * 1.4f) * 0.04f;
                float statusX = rect.xMax - statusWidth;
                Widgets.DrawBoxSolid(new Rect(statusX, rect.y + 15, 6, 6),
                    new Color(HeadquartersGui.Signal.r, HeadquartersGui.Signal.g, HeadquartersGui.Signal.b, pulse));
                HeadquartersGui.Label(new Rect(statusX + 16, rect.y + 4, statusWidth - 16, Text.LineHeight + 8),
                    time - openedAt < 0.65f ? "正在接通" : "专线已接通", HeadquartersGui.Signal);
            }
            Widgets.DrawBoxSolid(new Rect(rect.x, rect.yMax - 3, rect.width, 1), HeadquartersGui.Edge);
            float connect = Mathf.SmoothStep(0, 1, Mathf.Clamp01((time - openedAt) / 0.65f));
            Widgets.DrawBoxSolid(new Rect(rect.x, rect.yMax - 3, rect.width * connect, 2), HeadquartersGui.Accent);
        }

        //把铁十字作为唯一主体图形，下方显示实际点数和部队状态。
        private void DrawInstruments(Rect rect, float time)
        {
            HeadquartersGui.Panel(rect);
            Rect inner = rect.ContractedBy(16);
            float row = Text.LineHeightOf(GameFont.Small) + 8;
            HeadquartersGui.Label(new Rect(inner.x, inner.y, inner.width, row), "轨道司令部", HeadquartersGui.Accent);
            float emblemSize = Mathf.Min(inner.width, rect.height * 0.36f);
            Rect emblem = new Rect(inner.center.x - emblemSize / 2, inner.y + row + 6, emblemSize, emblemSize);
            float enter = Mathf.SmoothStep(0, 1, Mathf.Clamp01((time - openedAt) / 0.45f));
            GUI.color = new Color(ImperialUiStyle.IconTint.r, ImperialUiStyle.IconTint.g,
                ImperialUiStyle.IconTint.b, 0.5f + enter * 0.5f);
            Widgets.DrawTextureFitted(emblem, HeadquartersGui.Cross, 0.94f + enter * 0.06f);
            GUI.color = Color.white;
            float y = emblem.yMax + 14;
            Widgets.DrawBoxSolid(new Rect(inner.x, y, inner.width, 1), HeadquartersGui.Edge);
            y += 10;
            HeadquartersGui.Label(new Rect(inner.x, y, inner.width, row), "可用帝国点数", HeadquartersGui.Muted);
            y += row;
            Text.Font = GameFont.Medium;
            HeadquartersGui.Label(new Rect(inner.x, y, inner.width, Text.LineHeight + 8),
                Network.PointsLabel, HeadquartersGui.Ink);
            y += Text.LineHeight + 14;
            Text.Font = GameFont.Small;
            HeadquartersGui.Label(new Rect(inner.x, y, inner.width, row),
                Network.mobilized ? "部队状态：永久集结" : "部队状态：待命", HeadquartersGui.Muted);
            Rect signal = new Rect(inner.x, inner.yMax - 46, inner.width, 46);
            if (signal.y >= y + row + 10) DrawSignal(signal, time);
        }

        //用轻量折线表现无线电载波，按真实时间推进，不依赖游戏刻。
        private static void DrawSignal(Rect rect, float time)
        {
            Widgets.DrawBoxSolid(rect, ImperialUiStyle.DeepRed);
            for (int i = 1; i < 4; i++)
                Widgets.DrawBoxSolid(new Rect(rect.x, rect.y + rect.height * i / 4, rect.width, 1),
                    new Color(HeadquartersGui.Edge.r, HeadquartersGui.Edge.g, HeadquartersGui.Edge.b, 0.18f));
            if (Event.current.type != EventType.Repaint) return;
            Vector2 previous = new Vector2(rect.x + 4, rect.center.y);
            const int samples = 48;
            for (int i = 1; i <= samples; i++)
            {
                float phase = i * 0.53f - time * 2;
                float amplitude = 4 + 6 * Mathf.Pow(Mathf.Sin(i * 0.09f + time * 0.7f), 2);
                Vector2 next = new Vector2(rect.x + 4 + (rect.width - 8) * i / samples,
                    rect.center.y + Mathf.Sin(phase) * amplitude);
                Widgets.DrawLine(previous, next, new Color(HeadquartersGui.Accent.r,
                    HeadquartersGui.Accent.g, HeadquartersGui.Accent.b, 0.55f), 1);
                previous = next;
            }
        }

        //将结束通讯固定在底部，避免滚动后找不到关闭入口。
        private bool DrawFooter(Rect rect, float delta)
        {
            float buttonWidth = 156;
            Rect button = new Rect(rect.xMax - buttonWidth, rect.y, buttonWidth, rect.height);
            HeadquartersGui.UpdateHover(button, ref closeHover, delta);
            HeadquartersGui.Label(new Rect(rect.x, rect.y, rect.width - buttonWidth - 18, rect.height),
                "总部值守中  /  卡黛尔", HeadquartersGui.Muted);
            return HeadquartersGui.Command(button, "结束通讯", closeHover, true);
        }
    }
}
