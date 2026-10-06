using Verse;

namespace MihoAdfqs.Combat.Projectiles
{
    //每个部位保留一处活动侵蚀伤口，持续伤害累加到原有伤口。
    public sealed class Hediff_NaniteInjury : Hediff_Injury
    {
        //新侵蚀会重新破坏包扎，不能因为自动包扎而每秒另建一处伤口。
        public override bool TryMergeWith(Hediff other)
        {
            if (!(other is Hediff_NaniteInjury) || other.def != def || other.Part != Part
                || this.IsPermanent() || other.IsPermanent()) return false;
            Severity += other.Severity;
            ageTicks = 0;
            HediffComp_TendDuration tending = this.TryGetComp<HediffComp_TendDuration>();
            if (tending != null) tending.tendTicksLeft = 0;
            foreach (HediffComp comp in comps) comp.CompPostMerged(other);
            return true;
        }
    }
}
