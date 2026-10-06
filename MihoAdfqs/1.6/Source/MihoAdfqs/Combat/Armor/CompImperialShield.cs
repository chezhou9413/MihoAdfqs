using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace MihoAdfqs.Combat.Armor
{
    //提供全方向护盾，容量不足时传递剩余伤害，停止受击后恢复能量。
    public class CompImperialShield : CompShield
    {
        private const int RechargeDelay = 600;
        private int lastHitTick = -RechargeDelay;
        private readonly ImperialShieldVisuals visuals = new ImperialShieldVisuals();
        private CompProperties_ImperialShield Settings => (CompProperties_ImperialShield)props;
        private float Capacity => parent.GetStatValue(StatDefOf.EnergyShieldEnergyMax);
        private float RechargeRate => parent.GetStatValue(StatDefOf.EnergyShieldRechargeRate);

        //生成时充满能量。
        public override void PostPostMake() { base.PostPostMake(); energy = Capacity; }

        //显示实际可吸收的伤害点数和当前恢复状态。
        public override IEnumerable<Gizmo> CompGetWornGizmosExtra()
        {
            foreach (Gizmo gizmo in base.CompGetWornGizmosExtra())
                if (!(gizmo is Gizmo_EnergyShieldStatus)) yield return gizmo;
            if (PawnOwner?.Faction == Faction.OfPlayer && Find.Selector.SingleSelectedThing == PawnOwner)
                yield return new Phase2.UI.Gizmo_StatusProgress(parent.LabelCap, () => energy / Capacity,
                    () => $"{energy / Props.energyLossPerDamage:F0} / {Capacity / Props.energyLossPerDamage:F0}", StatusText);
        }

        //破盾后保持原有重置时间，未破盾时停止受击十秒后逐渐恢复。
        public override void CompTick()
        {
            if (PawnOwner == null) return;
            if (ticksToReset > 0)
            {
                //只让原版处理重置倒计时及重启反馈，常规恢复统一读取属性。
                base.CompTick();
                if (ticksToReset <= 0)
                {
                    energy = Capacity;
                    visuals.Reform();
                }
                return;
            }
            if (ShieldState == ShieldState.Active && Find.TickManager.TicksGame - lastHitTick >= RechargeDelay)
            {
                float capacity = Capacity;
                if (energy >= capacity) return;
                energy = Mathf.Min(capacity, energy + RechargeRate / 60f);
                if (energy >= capacity) visuals.Reform();
            }
        }

        //实际拦截由主伤害流程调用，避免衣物的按值接口丢失穿透伤害。
        public override void PostPreApplyDamage(ref DamageInfo dinfo, out bool absorbed) { absorbed = false; }

        //拦截伤害并把受击位置交给曲面动画，EMP保持停机而不拦截后续效果。
        public void AbsorbDamage(ref DamageInfo dinfo, out bool absorbed)
        {
            absorbed = false;
            if (ShieldState != ShieldState.Active || PawnOwner == null) return;
            bool emp = dinfo.Def == DamageDefOf.EMP;
            if (!emp && (dinfo.Amount <= 0f || dinfo.Def.ignoreShields || !dinfo.Def.ExternalViolenceFor(PawnOwner))) return;
            lastHitTick = Find.TickManager.TicksGame;
            float blocked = Mathf.Min(dinfo.Amount, energy / Props.energyLossPerDamage);
            float remainder = Mathf.Max(0f, dinfo.Amount - energy / Props.energyLossPerDamage);
            energy = emp ? 0 : Mathf.Max(0, energy - dinfo.Amount * Props.energyLossPerDamage);
            bool broken = energy <= 0;
            if (broken) ticksToReset = Props.startingTicksToReset;
            KeepDisplaying();
            visuals.Hit(PawnOwner, dinfo, blocked, broken, Settings);
            if (PawnOwner.Spawned)
            {
                SoundDef sound = broken ? DefDatabase<SoundDef>.GetNamed("EnergyShield_Broken") : SoundDefOf.EnergyShield_AbsorbDamage;
                sound.PlayOneShot(new TargetInfo(PawnOwner.Position, PawnOwner.Map));
            }
            if (emp) return;
            dinfo.SetAmount(remainder);
            absorbed = remainder <= 0f;
        }

        //替换原版圆形贴片，绘制随人物移动的能量罩和恢复过程。
        public override void CompDrawWornExtras()
        {
            Pawn pawn = PawnOwner;
            if (pawn == null || ShieldState == ShieldState.Disabled) return;
            float capacity = Capacity;
            bool charging = ticksToReset <= 0 && energy < capacity
                && Find.TickManager.TicksGame - lastHitTick >= RechargeDelay;
            visuals.Draw(pawn, Settings, energy / capacity, charging, ticksToReset, ShouldDisplay);
        }

        //完整恢复规则保留在悬浮提示中。
        private string StatusText()
        {
            if (ticksToReset > 0) return $"破盾重置：剩余{ticksToReset / 60f:F1}秒";
            if (energy >= Capacity) return "全方向护盾；超额伤害穿透";
            int wait = RechargeDelay - (Find.TickManager.TicksGame - lastHitTick);
            return wait > 0 ? $"停止受击{wait / 60f:F1}秒后开始恢复"
                : $"恢复{RechargeRate / Props.energyLossPerDamage:F2}点/秒；从空充满需{Props.startingTicksToReset / 60f:F0}秒";
        }

        //保存最近受击时间，读档后继续计算恢复延迟。
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref lastHitTick, "lastHitTick", -RechargeDelay);
        }
    }
}
