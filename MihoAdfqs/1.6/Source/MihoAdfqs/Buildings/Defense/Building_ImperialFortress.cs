using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using MihoAdfqs.Phase2.UI;
using Verse.AI;

namespace MihoAdfqs.Buildings.Defense
{
    //在同一座堡垒内切换炮台，管理共用钢铁库存、弹仓与自动装填。
    public class Building_ImperialFortress : Building_TurretGun
    {
        private int rounds;
        private int reloadTicks;
        public bool Heavy => gun.def.defName == "MihoPhase2_Gun_Fortress88mm";
        public int Capacity => Heavy ? 50 : 500;
        public bool HasRound => rounds > 0 && reloadTicks == 0;
        public int Rounds => rounds;
        public int ReloadTicks => reloadTicks;
        public float SteelStock => refuelableComp.Fuel;
        public string SwitchBlockReason => !Active ? "需要通电、开启开关并修好故障"
            : AttackVerb.state == VerbState.Bursting ? "请等待本轮射击结束"
            : SteelStock < 100 ? $"换装需要库存100钢铁，当前{SteelStock:F0}" : null;
        public override Material TurretTopMaterial => gun.Graphic.MatSingle;
        protected override bool CanSetForcedTarget => Faction == Faction.OfPlayer;

        //炮管绘制和射弹共用偏移后的旋转轴心。
        internal Vector3 TurretPivot => DrawPos + new Vector3(def.building.turretTopOffset.x, 0f,
            def.building.turretTopOffset.y);

        //瞄准从实际炮轴指向目标，消除底座偏移造成的角度误差。
        internal float TurretAimAngle => AttackVerb.AimAngleOverride ?? (CurrentTarget.IsValid
            ? (CurrentTarget.CenterVector3 - TurretPivot).AngleFlat() : top.CurRotation);

        //两张512像素炮管贴图的尖端分别位于纵坐标417和483。
        internal Vector3 MuzzlePosition => TurretPivot + (Vector3.forward
            * def.building.turretTopDrawSize * ((Heavy ? 483f : 417f) / 512f - .5f)).RotatedBy(TurretAimAngle);

        //长射程使用几何圆线，避免原版径向格表无法覆盖80或120格。
        public override void DrawExtraSelectionOverlays()
        {
            GenDraw.DrawInteractionCells(def, Position, Rotation);
            foreach (ThingComp comp in AllComps) comp.PostDrawExtraSelectionOverlays();
            Vector3 center = Position.ToVector3Shifted();
            center.y = AltitudeLayer.MetaOverlays.AltitudeFor();
            float range = AttackVerb.EffectiveRange;
            float minRange = AttackVerb.verbProps.EffectiveMinRange(allowAdjacentShot: true);
            if (minRange < range)
            {
                GenDraw.DrawCircleOutline(center, range);
                if (minRange > .1f) GenDraw.DrawCircleOutline(center, minRange);
            }
            if (burstWarmupTicksLeft > 0)
                GenDraw.DrawAimPie(this, CurrentTarget, (int)(burstWarmupTicksLeft * .5f), def.size.x * .5f);
            if (forcedTarget.IsValid && (!forcedTarget.HasThing || forcedTarget.Thing.Spawned))
            {
                Vector3 start = this.TrueCenter();
                Vector3 end = forcedTarget.HasThing ? forcedTarget.Thing.TrueCenter() : forcedTarget.Cell.ToVector3Shifted();
                start.y = end.y = center.y;
                GenDraw.DrawLineBetween(start, end, ForcedTargetLineMat);
            }
        }

        //通电且有库存时自动装填，装填期间停止射击。
        protected override void Tick()
        {
            if (Active && refuelableComp.Fuel >= 1 && rounds == 0)
            {
                if (reloadTicks == 0) reloadTicks = 300;
                if (--reloadTicks == 0) rounds = Mathf.Min(Capacity, Mathf.FloorToInt(refuelableComp.Fuel));
            }
            base.Tick();
        }

        //成功发射后扣除弹仓计数，钢铁由原版每发消耗规则统一扣除。
        public void ConsumeRound() => rounds--;

        //战备空投的首个弹仓在落地时已完成装填。
        internal void LoadDropMagazine() { rounds = Capacity; reloadTicks = 0; }

        //使用当前炮型的冷却，避免读取建造时默认炮型的参数。
        protected override float BurstCooldownTime() => Heavy ? 4f : 0.75f;

        //沿用原版敌我判定和目标评分，视线检查忽略自身占地。
        public override LocalTargetInfo TryFindNewTarget() => (Thing)AttackTargetFinder.BestShootTargetFromCurrentPosition(this,
            TargetScanFlags.NeedThreat | TargetScanFlags.NeedAutoTargetable, IsValidAutomaticTarget);

        //完整实体底座不遮挡自身炮台；墙体、其他建筑和致盲烟雾仍阻止索敌。
        private bool IsValidAutomaticTarget(Thing target)
        {
            if (target is Pawn pawn && ((pawn.IsPrisoner && Faction == Faction.OfPlayer)
                || GenAI.MachinesLike(Faction, pawn) || (!Heavy && pawn.Flying))) return false;
            if (!Heavy && target is PawnFlyer) return false;
            if (!AttackVerb.CanHitTargetFrom(Position, target)) return false;
            if (!ClearOfBlindSmoke(Position) || !ClearOfBlindSmoke(target.Position)) return false;
            return GenSight.LineOfSight(Position, target.Position, Map, this.OccupiedRect(),
                target.OccupiedRect(), ClearOfBlindSmoke);
        }

        //索敌路径上的致盲烟雾使用原版气体类型。
        private bool ClearOfBlindSmoke(IntVec3 cell) => !cell.AnyGas(Map, GasType.BlindSmoke);

        //显示切换按钮，切换费用从堡垒已装入的钢铁中支付。
        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo gizmo in base.GetGizmos()) yield return gizmo;
            if (Faction != Faction.OfPlayer) yield break;
            var command = new Command_ImperialAction
            {
                defaultLabel = Heavy ? "切换30毫米速射炮" : "切换88毫米高爆炮",
                defaultDesc = "消耗库存中的100钢铁切换炮台。30毫米弹仓500发，88毫米弹仓50发；钢铁总库存500，每发消耗1钢铁。空仓自动装填5秒，断电暂停。",
                icon = def.uiIcon,
                action = SwitchGun
            };
            if (SwitchBlockReason != null) command.Disable(SwitchBlockReason);
            yield return command;
        }

        //保留建筑身份与耐久，换装另一门炮并重新建立发射回调。
        public void SwitchGun()
        {
            if (!Active || AttackVerb.state == VerbState.Bursting || refuelableComp.Fuel < 100) return;
            ThingDef next = DefDatabase<ThingDef>.GetNamed(Heavy ? "MihoPhase2_Gun_Fortress30mm" : "MihoPhase2_Gun_Fortress88mm");
            refuelableComp.ConsumeFuel(100);
            gun.Destroy();
            gun = ThingMaker.MakeThing(next);
            foreach (Verb verb in GunCompEq.AllVerbs)
            {
                verb.caster = this;
                verb.castCompleteCallback = BurstComplete;
            }
            forcedTarget = LocalTargetInfo.Invalid;
            currentTargetInt = LocalTargetInfo.Invalid;
            burstWarmupTicksLeft = 0;
            burstCooldownTicksLeft = 0;
            rounds = 0;
            reloadTicks = 0;
        }

        //列出实际炮型、剩余弹数和装填时间。
        public override string GetInspectString() => base.GetInspectString() + "\n炮型：" + (Heavy ? "88毫米高爆炮" : "30毫米速射炮")
            + $"\n弹仓：{rounds}/{Capacity}" + (reloadTicks > 0 ? $"\n装填：{reloadTicks / 60f:F1}秒" : "");

        //保存弹仓和装填倒计时，当前炮型由原版保存的炮身对象恢复。
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref rounds, "fortressRounds");
            Scribe_Values.Look(ref reloadTicks, "fortressReloadTicks");
        }
    }
}
