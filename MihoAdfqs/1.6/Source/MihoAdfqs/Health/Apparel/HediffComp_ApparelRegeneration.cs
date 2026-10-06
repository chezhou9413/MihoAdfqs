using MihoAdfqs.Phase2.Medicine;
using Verse;

namespace MihoAdfqs.Health
{
    //职责：轨道装备同时提供包扎和每个部位的定量恢复，停止穿戴后移除状态。
    public class HediffComp_ApparelRegeneration : HediffComp
    {
        public override bool CompShouldRemove => !MedicalApparelUtility.HasSource(parent);

        //职责：每秒在同一部位的多处伤口之间分配恢复量，避免剩余治疗量被丢弃。
        public override void CompPostTick(ref float severityAdjustment)
        {
            if (!Pawn.IsHashIntervalTick(60) || CompShouldRemove) return;
            MedicalUtility.Treat(Pawn, ((HediffCompProperties_ApparelRegeneration)props).healPerBodyPartPerSecond);
        }
    }
}
