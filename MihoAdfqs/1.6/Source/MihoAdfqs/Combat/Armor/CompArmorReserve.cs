using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using MihoAdfqs.Phase2.UI;

namespace MihoAdfqs.Combat.Armor
{
    //职责：记录可消耗且只能用插板修复的护甲抵消值。
    public class CompArmorReserve : ThingComp
    {
        public float reserve;
        private int flashReadyTick;
        public CompProperties_ArmorReserve Settings => (CompProperties_ArmorReserve)props;

        //职责：制作或生成护甲时装满抵消值。
        public override void PostPostMake() { reserve = Settings.capacity; }

        //职责：优先扣除抵消值并返回穿透剩余伤害。
        public float Absorb(float damage)
        {
            float absorbed = Mathf.Min(reserve, damage);
            reserve -= absorbed;
            return damage - absorbed;
        }

        //职责：按一块插板二百五十点恢复抵消值。
        public void Repair() { reserve = Mathf.Min(Settings.capacity, reserve + 250); }

        //按装备名称分别显示抵消值，塔盾另有闪烁指令。
        public IEnumerable<Gizmo> Commands(Pawn wearer)
        {
            yield return new Gizmo_StatusProgress(parent.LabelCap + " · 抵消值", () => reserve / Settings.capacity,
                () => $"{reserve:F0} / {Settings.capacity:F0}",
                () => Settings.towerShield ? "塔盾减伤后消耗；不自动恢复，每块防弹插板补充250点。"
                    : "覆盖部位受击时先消耗抵消值，耗尽后损耗装备耐久；不自动恢复，每块防弹插板补充250点。");
            if (!Settings.towerShield) yield break;
            var flash = new Command_ImperialAction
            {
                defaultLabel = "塔盾闪烁", defaultDesc = "使面前2×3格内敌人眩晕5秒，冷却30秒。", icon = parent.def.uiIcon,
                action = () => Flash(wearer)
            };
            if (Find.TickManager.TicksGame < flashReadyTick) flash.Disable($"冷却{(flashReadyTick - Find.TickManager.TicksGame) / 60f:F1}秒");
            yield return flash;
        }

        //职责：按持盾者朝向选取前方两列三行的敌人并施加眩晕。
        private void Flash(Pawn wearer)
        {
            IntVec3 forward = wearer.Rotation.FacingCell;
            IntVec3 side = wearer.Rotation.Rotated(RotationDirection.Clockwise).FacingCell;
            for (int depth = 1; depth <= 3; depth++)
                for (int width = 0; width < 2; width++)
                {
                    IntVec3 cell = wearer.Position + forward * depth + side * width;
                    if (!cell.InBounds(wearer.Map)) continue;
                    foreach (Pawn target in cell.GetThingList(wearer.Map).OfType<Pawn>().Where(p => p.HostileTo(wearer)).ToList()) target.stances.stunner.StunFor(300, wearer);
                    FleckMaker.Static(cell, wearer.Map, FleckDefOf.ShotFlash);
                }
            flashReadyTick = Find.TickManager.TicksGame + 1800;
        }

        //职责：为已穿戴的护甲公开抵消值。
        public override IEnumerable<Gizmo> CompGetWornGizmosExtra() => Commands(((Apparel)parent).Wearer);

        //职责：显示地面护甲的剩余抵消值。
        public override string CompInspectStringExtra() => $"抵消值：{reserve:F0}/{Settings.capacity:F0}";

        //职责：保存抵消值及塔盾闪烁冷却。
        public override void PostExposeData()
        {
            Scribe_Values.Look(ref reserve, "armorReserve");
            Scribe_Values.Look(ref flashReadyTick, "flashReadyTick");
        }
    }
}
