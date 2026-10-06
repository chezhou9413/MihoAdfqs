using System.Linq;
using Verse;

namespace MihoAdfqs.Health
{
    //职责：穿戴医疗装备时包扎流血状态，脱下全部来源装备后移除效果。
    public class HediffComp_ApparelAutoTend : HediffComp
    {
        public override bool CompShouldRemove => !MedicalApparelUtility.HasSource(parent);

        //职责：每秒为全部可包扎的流血伤口和缺肢创口提供满质量包扎。
        public override void CompPostTick(ref float severityAdjustment)
        {
            if (!Pawn.IsHashIntervalTick(60) || CompShouldRemove) return;
            foreach (Hediff wound in Pawn.health.hediffSet.hediffs.Where(h => h.BleedRate > 0 && h.TendableNow()).ToList())
                wound.Tended(1f, 1f, 1);
        }
    }
}
