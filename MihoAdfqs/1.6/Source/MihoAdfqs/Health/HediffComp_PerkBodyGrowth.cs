using System.Collections.Generic;
using RimWorld;
using Verse;

namespace MihoAdfqs.Health
{
    //类职责：为佩尔克体配置按成长阶段递减的每日增长速度。
    public class HediffCompProperties_PerkBodyGrowth : HediffCompProperties
    {
        //职责：指定佩尔克体的成长组件。
        public HediffCompProperties_PerkBodyGrowth()
        {
            compClass = typeof(HediffComp_PerkBodyGrowth);
        }
    }

    //类职责：根据佩尔克体当前进度持续推进其成长，并在完全体停止增长。
    public class HediffComp_PerkBodyGrowth : HediffComp
    {
        private double progress = -1;
        private float lastApplied = -1;

        //职责：用双精度累计极小的成长量，避免后期逐刻增量低于浮点分辨率而停止成长。
        public override void CompPostTickInterval(ref float severityAdjustment, int delta)
        {
            float severity = parent.Severity;
            if (progress < 0 || lastApplied != severity) progress = severity;
            progress = System.Math.Min(1d, progress + DailyGrowth(severity) * delta / 60000d);
            lastApplied = (float)progress;
            severityAdjustment += lastApplied - severity;
        }

        //职责：按当前器官阶段提供每日生长比例。
        private static double DailyGrowth(float severity) => severity < 0.31f ? 0.005 : severity < 0.71f ? 0.002 : severity < 1f ? 0.001 : 0;

        //职责：为选中的玩家人物显示实际成长百分比与当前每日增长速度。
        public override IEnumerable<Gizmo> CompGetGizmos()
        {
            if (Pawn.Faction == Faction.OfPlayer && Find.Selector.SingleSelectedThing == Pawn)
                yield return new Phase2.UI.Gizmo_StatusProgress("佩尔克体", () => parent.Severity,
                    () => parent.Severity.ToString("P2"), () => parent.CurStage.label + " · 每日增长" + DailyGrowth(parent.Severity).ToString("P1"));
        }

        //职责：保存未达到单精度显示阈值的成长余量。
        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look(ref progress, "preciseGrowth", -1d);
            Scribe_Values.Look(ref lastApplied, "growthLastApplied", -1f);
        }
    }
}
