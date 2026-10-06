using MihoAdfqs.Combat.Weapons;
using MihoAdfqs.Phase2.UI;
using UnityEngine;
using Verse;

namespace MihoAdfqs.Combat.UI
{
    //按武器类型显示单排弹药或能量格，提供消耗动效和补充入口。
    public sealed class Gizmo_MagazineProgress : Gizmo
    {
        private readonly Verb_Magazine verb;

        //主枪和下挂绑定各自的动词，弹药及动画不会混用。
        public Gizmo_MagazineProgress(Verb_Magazine verb)
        {
            this.verb = verb;
        }

        //按弹匣容量调整宽度，并遵循Gizmo网格提供的可用空间。
        public override float GetWidth(float maxWidth) => Mathf.Min(maxWidth,
            Mathf.Clamp(60 + Mathf.Min(verb.AmmoCapacity, 32) * 9, 210, 350));

        //显示武器、精确余量与子弹单排，并恢复共享界面状态。
        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
        {
            GameFont font = Text.Font;
            TextAnchor anchor = Text.Anchor;
            bool wrap = Text.WordWrap;
            Color color = GUI.color;
            try
            {
                GUI.color = Color.white;
                Text.Font = GameFont.Small;
                Text.Anchor = TextAnchor.MiddleLeft;
                Text.WordWrap = false;
                var rect = new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), Height);
                ImperialUiStyle.Panel(rect);
                float lineHeight = Text.LineHeight + 2f;
                string title = verb.verbProps.label ?? verb.EquipmentSource.LabelCap;
                int count = verb.AmmoCount;
                int remaining = verb.ReloadTicksLeft;
                bool reloading = remaining > 0;
                bool laser = verb.WeaponType == MagazineWeaponType.Laser;
                string reloadLabel = MagazineUiAssets.ReloadLabel(verb.WeaponType);
                string value = reloading ? $"{reloadLabel} {verb.ReloadProgress:P0}" : $"{count}/{verb.AmmoCapacity}";
                float valueWidth = Text.CalcSize(value).x + 10;
                ImperialUiStyle.Mark(new Rect(rect.x + 7, rect.y + 7, 18, 18));
                GUI.color = ImperialUiStyle.Ink;
                var titleRect = new Rect(rect.x + 31, rect.y + 4, rect.width - valueWidth - 45, lineHeight);
                Widgets.Label(titleRect, title.Truncate(titleRect.width));
                Text.Anchor = TextAnchor.MiddleRight;
                GUI.color = reloading ? MagazineUiAssets.Reloading : MagazineUiAssets.LoadedColor(verb.WeaponType);
                Widgets.Label(new Rect(rect.xMax - valueWidth - 8, titleRect.y, valueWidth, lineHeight), value);
                GUI.color = Color.white;
                Rect row = new Rect(rect.x + 8, titleRect.yMax + 3, rect.width - 16,
                    rect.yMax - titleRect.yMax - 10);
                int roundsPerIcon = DrawRounds(row, count, reloading);
                string unit = laser ? "次射击能量" : "发";
                string grouping = $"\n每个图标代表至多{roundsPerIcon}{unit}，部分填充表示该组余量。";
                bool canReload = verb.CanReload;
                string actionHint = !verb.PlayerCanControl ? "仅查看弹药状态；该单位当前不能接受玩家的武器操作。"
                    : canReload ? $"点击提前{reloadLabel}。"
                    : reloading ? $"正在{reloadLabel}，完成后恢复射击。"
                    : count == verb.AmmoCapacity ? (laser ? "能量已满。" : "弹匣已满。") : $"射击结束后可手动{reloadLabel}。";
                TooltipHandler.TipRegion(rect, $"{verb.CasterPawn?.LabelShortCap} · {verb.EquipmentSource.LabelCap}\n{title}\n类型：{MagazineUiAssets.TypeLabel(verb.WeaponType)}\n剩余{(laser ? "能量" : "弹药")}：{count} / {verb.AmmoCapacity}{grouping}\n{actionHint}");
                if (Mouse.IsOver(rect))
                {
                    if (canReload) Widgets.DrawHighlight(rect);
                    if (canReload && Event.current.button == 0 && Widgets.ButtonInvisible(rect))
                        return new GizmoResult(GizmoState.Interacted, Event.current);
                    return new GizmoResult(GizmoState.Mouseover);
                }
                return new GizmoResult(GizmoState.Clear);
            }
            finally
            {
                Text.Font = font;
                Text.Anchor = anchor;
                Text.WordWrap = wrap;
                GUI.color = color;
            }
        }

        //保持单排且不缩成不可辨认的细线，大弹匣用部分填充的分组图标。
        private int DrawRounds(Rect area, int count, bool reloading)
        {
            Texture2D texture = MagazineUiAssets.Icon(verb.WeaponType);
            bool laser = verb.WeaponType == MagazineWeaponType.Laser;
            float minStep = verb.WeaponType == MagazineWeaponType.Grenade ? 20 : laser ? 10 :
                verb.WeaponType == MagazineWeaponType.Bullet ? 10 : 14;
            int maxIcons = Mathf.Max(1, Mathf.FloorToInt(area.width / minStep));
            int perIcon = Mathf.CeilToInt((float)verb.AmmoCapacity / maxIcons);
            int icons = Mathf.CeilToInt((float)verb.AmmoCapacity / perIcon);
            float step = Mathf.Min(laser ? 16 : verb.WeaponType == MagazineWeaponType.Bullet ? 14 : 30, area.width / icons);
            float aspect = (float)texture.width / texture.height;
            float iconHeight = Mathf.Min(36, area.height - 5);
            float iconWidth = Mathf.Min(iconHeight * aspect, step - 3);
            iconHeight = iconWidth / aspect;
            float start = area.center.x - step * icons / 2;
            for (int i = 0; i < icons; i++)
            {
                int groupCapacity = Mathf.Min(perIcon, verb.AmmoCapacity - i * perIcon);
                float fraction = Mathf.Clamp01((float)(count - i * perIcon) / groupCapacity);
                Rect icon = new Rect(start + step * i + (step - iconWidth) / 2,
                    area.center.y - iconHeight / 2, iconWidth, iconHeight);
                MagazineUiAssets.DrawRound(icon, texture, fraction, MagazineUiAssets.LoadedColor(verb.WeaponType));
            }
            if (Event.current.type == EventType.Repaint)
            {
                float time = Time.realtimeSinceStartup;
                foreach (MagazineShotVisual shot in verb.ShotVisuals)
                {
                    float t = (time - shot.startedAt) / MagazineShotVisual.Duration;
                    if (t < 0 || t >= 1) continue;
                    int slot = shot.roundIndex / perIcon;
                    Rect eject = new Rect(start + step * slot + (step - iconWidth) / 2 + (laser ? 0 : 9 * t),
                        area.center.y - iconHeight / 2 - (laser ? 0 : Mathf.Sin(t * Mathf.PI / 2) * 9),
                        iconWidth, iconHeight);
                    if (laser) eject = eject.ExpandedBy(2 * t);
                    Color pulse = MagazineUiAssets.LoadedColor(verb.WeaponType);
                    pulse.a = 1 - t;
                    GUI.color = pulse;
                    GUI.DrawTexture(eject, texture);
                }
                GUI.color = Color.white;
            }
            if (reloading)
            {
                Rect rail = new Rect(area.x, area.yMax - 2, area.width, 2);
                Widgets.DrawBoxSolid(rail, MagazineUiAssets.Empty);
                Widgets.DrawBoxSolid(new Rect(rail.x, rail.y, rail.width * verb.ReloadProgress, rail.height),
                    MagazineUiAssets.Reloading);
            }
            return perIcon;
        }

        //通过动词重新检查控制权与手动装弹条件。
        public override void ProcessInput(Event ev)
        {
            verb.RequestReload();
        }
    }
}
