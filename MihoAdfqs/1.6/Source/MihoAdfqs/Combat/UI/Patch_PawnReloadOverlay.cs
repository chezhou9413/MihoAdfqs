using HarmonyLib;
using System.Runtime.CompilerServices;
using MihoAdfqs.Combat.Weapons;
using MihoAdfqs.Phase2.UI;
using UnityEngine;
using Verse;

namespace MihoAdfqs.Combat.UI
{
    //在可见小人头顶显示每个正在装弹的枪管进度。
    [HarmonyPatch(typeof(Pawn), nameof(Pawn.DrawGUIOverlay))]
    public static class Patch_PawnReloadOverlay
    {
        private static readonly ConditionalWeakTable<Verb_Magazine, ReloadOverlayText> TextCache =
            new ConditionalWeakTable<Verb_Magazine, ReloadOverlayText>();
        private static readonly ConditionalWeakTable<Verb_Magazine, ReloadOverlayText>.CreateValueCallback CreateText =
            verb => new ReloadOverlayText(verb);

        //沿用原版世界坐标投影与UI缩放，不要求小人被选中。
        public static void Postfix(Pawn __instance)
        {
            if (Event.current.type != EventType.Repaint) return;
            Pawn pawn = __instance;
            if (!pawn.Spawned || pawn.Dead || pawn.Map != Find.CurrentMap) return;
            var equipment = pawn.equipment?.PrimaryEq;
            if (equipment == null) return;
            var verbs = equipment.AllVerbs;
            int tick = Find.TickManager.TicksGame;
            int firstReloading = -1;
            for (int i = 0; i < verbs.Count; i++)
                if (verbs[i] is Verb_Magazine candidate && candidate.TryGetReloadDisplay(tick, out _, out _))
                {
                    firstReloading = i;
                    break;
                }
            //没有装弹时不访问字体、屏幕投影或GUI状态。
            if (firstReloading < 0 || pawn.Position.Fogged(pawn.Map)) return;
            GameFont font = Text.Font;
            TextAnchor anchor = Text.Anchor;
            bool wrap = Text.WordWrap;
            Color color = GUI.color;
            try
            {
                Text.Font = GameFont.Tiny;
                Text.Anchor = TextAnchor.MiddleLeft;
                Text.WordWrap = false;
                GUI.color = Color.white;
                Vector2 position = GenMapUI.LabelDrawPosFor(pawn, 0.65f);
                float bottom = position.y - 10;
                float lineHeight = Text.LineHeight;
                for (int i = firstReloading; i < verbs.Count; i++)
                {
                    if (!(verbs[i] is Verb_Magazine verb) || !verb.TryGetReloadDisplay(tick, out int remaining, out float progress)) continue;
                    ReloadOverlayText text = TextCache.GetValue(verb, CreateText);
                    text.Update(tick, remaining, progress);
                    float height = lineHeight + 16;
                    float width = text.Width;
                    Rect panel = new Rect(position.x - width / 2, bottom - height, width, height);
                    ImperialUiStyle.Panel(panel);
                    GUI.color = MagazineUiAssets.LoadedColor(verb.WeaponType);
                    Widgets.DrawTextureFitted(new Rect(panel.x + 6, panel.y + 4, 18, lineHeight),
                        MagazineUiAssets.Icon(verb.WeaponType), 1f);
                    GUI.color = ImperialUiStyle.Ink;
                    Widgets.Label(new Rect(panel.x + 30, panel.y + 2, panel.width - 34, lineHeight + 4), text.Status);
                    Rect bar = new Rect(panel.x + 5, panel.yMax - 8, panel.width - 10, 3);
                    Widgets.DrawBoxSolid(bar, MagazineUiAssets.Empty);
                    Widgets.DrawBoxSolid(new Rect(bar.x, bar.y, bar.width * progress, bar.height), MagazineUiAssets.Reloading);
                    bottom = panel.y - 4;
                }
            }
            finally
            {
                Text.Font = font;
                Text.Anchor = anchor;
                Text.WordWrap = wrap;
                GUI.color = color;
            }
        }
    }
}
