using System.Linq;
using Verse;

namespace MihoAdfqs.Genes
{
    //在基因存在期间为携带者维持XML指定的健康状态。
    public class Gene_GrantsHediff : Gene
    {
        //在基因加入后创建关联健康状态。
        public override void PostAdd()
        {
            base.PostAdd();
            EnsureHediff();
        }

        //生成过程可能重置健康状态，结束后补齐仍有效基因授予的部件状态。
        public void EnsureHediff()
        {
            if (!Active) return;
            GeneHediffExtension extension = def.GetModExtension<GeneHediffExtension>();
            if (extension?.hediffDef == null || pawn?.health == null)
            {
                return;
            }

            Hediff existing = pawn.health.hediffSet.GetFirstHediffOfDef(extension.hediffDef, false);
            if (existing != null)
            {
                existing.Severity = System.Math.Max(existing.Severity, extension.initialSeverity);
                return;
            }

            BodyPartRecord part = pawn.RaceProps.body.AllParts.FirstOrDefault(p => p.def.defName == "MihoPhase2_PerkOrgan");
            if (part == null || pawn.health.hediffSet.PartIsMissing(part)) return;
            Hediff granted = HediffMaker.MakeHediff(extension.hediffDef, pawn, part);
            granted.Severity = extension.initialSeverity;
            pawn.health.AddHediff(granted);
        }

        //在最后一个同类授予基因移除后清理其关联健康状态。
        public override void PostRemove()
        {
            GeneHediffExtension extension = def.GetModExtension<GeneHediffExtension>();
            if (extension?.hediffDef != null && pawn?.health != null && !HasAnotherGrant(extension.hediffDef))
            {
                Hediff existing = pawn.health.hediffSet.GetFirstHediffOfDef(extension.hediffDef, false);
                if (existing != null)
                {
                    pawn.health.RemoveHediff(existing);
                }
            }

            base.PostRemove();
        }

        //判断当前小人是否还有其他基因维持同一个健康状态。
        private bool HasAnotherGrant(HediffDef hediffDef)
        {
            return pawn.genes.GenesListForReading.Any(gene =>
                gene != this &&
                gene.def.GetModExtension<GeneHediffExtension>()?.hediffDef == hediffDef);
        }
    }
}
