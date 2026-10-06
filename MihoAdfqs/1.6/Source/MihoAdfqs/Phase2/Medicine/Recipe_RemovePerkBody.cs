using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace MihoAdfqs.Phase2.Medicine
{
    //职责：提供仅针对佩尔克体的摘除手术，并沿用原版手术成功率和医疗消耗。
    public class Recipe_RemovePerkBody : Recipe_RemoveBodyPart
    {
        protected override bool SpawnPartsWhenRemoved => false;

        //职责：只列出身体中尚未缺失的佩尔克体，不把其它健康器官加入手术选项。
        public override IEnumerable<BodyPartRecord> GetPartsToApplyOn(Pawn pawn, RecipeDef recipe) =>
            pawn.health.hediffSet.GetNotMissingParts().Where(p => p.def.defName == "MihoPhase2_PerkOrgan");

        //职责：直接标记器官缺失，让原版同时清除依附其上的成长效果，避免战斗限伤阻止手术。
        public override void DamagePart(Pawn pawn, BodyPartRecord part)
        {
            var missing = (Hediff_MissingPart)HediffMaker.MakeHediff(HediffDefOf.MissingBodyPart, pawn, part);
            missing.lastInjury = HediffDefOf.SurgicalCut;
            pawn.health.AddHediff(missing, part, new DamageInfo(DamageDefOf.SurgicalCut, 0, hitPart: part));
        }

        //职责：在医疗列表中显示专用手术名称。
        public override string GetLabelWhenUsedOn(Pawn pawn, BodyPartRecord part) => recipe.label;
    }
}
