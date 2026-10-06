using HarmonyLib;
using UnityEngine;
using Verse;

namespace MihoAdfqs.Phase2.Pawns
{
    //在健康系统加入伤口前截断伤害，保留受保护部位最后一点生命值。
    [HarmonyPatch(typeof(Pawn_HealthTracker), nameof(Pawn_HealthTracker.AddHediff), new[] { typeof(Hediff), typeof(BodyPartRecord), typeof(DamageInfo?), typeof(DamageWorker.DamageResult) })]
    public static class Patch_PreserveBodyParts
    {
        //限制伤口严重度，并阻止直接加入缺失部件。
        public static bool Prefix(Pawn ___pawn, Hediff hediff, BodyPartRecord part, DamageInfo? dinfo)
        {
            BodyPartRecord affected = part ?? hediff.Part;
            //佩尔克体的自愿摘除不属于战斗损伤，允许专用手术建立缺失记录。
            if (affected?.def.defName == "MihoPhase2_PerkOrgan" && dinfo?.Def == RimWorld.DamageDefOf.SurgicalCut) return true;
            if (!EmpirePawnUtility.Preserve(___pawn, affected)) return true;
            if (hediff is Hediff_MissingPart) return false;
            if (!(hediff is Hediff_Injury) || affected == null) return true;
            float limit = EmpirePawnUtility.Fuhrer(___pawn) || EmpirePawnUtility.Immortal(___pawn)
                ? Patch_FuhrerInjuryHealth.InjuryLimit(___pawn, affected, hediff)
                : Mathf.Max(0, ___pawn.health.hediffSet.GetPartHealth(affected) - 1);
            hediff.Severity = Mathf.Min(hediff.Severity, limit);
            return hediff.Severity > 0;
        }
    }
}
