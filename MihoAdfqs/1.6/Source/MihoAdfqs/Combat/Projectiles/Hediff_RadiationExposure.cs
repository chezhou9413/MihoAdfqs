using RimWorld;
using Verse;

namespace MihoAdfqs.Combat.Projectiles
{
    //职责：让沾染辐射尘埃的人员在状态持续期间每秒累积核辐射毒性。
    public class Hediff_RadiationExposure : HediffWithComps
    {
        //职责：直接增加毒性积累，护甲不参与结算，持续时间由状态定义控制。
        public override void TickInterval(int delta)
        {
            base.TickInterval(delta);
            if (pawn.Dead || !pawn.IsHashIntervalTick(60, delta)) return;
            HealthUtility.AdjustSeverity(pawn, HediffDefOf.ToxicBuildup, 0.01f);
        }
    }
}
