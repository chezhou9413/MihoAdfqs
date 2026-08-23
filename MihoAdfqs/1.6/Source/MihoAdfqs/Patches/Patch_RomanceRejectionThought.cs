using HarmonyLib;
using RimWorld;
using Verse;
using System;
using MihoAdfqs.MihoAdfDefRef; 

namespace MihoAdfqs.MihoAdfHarmony
{
    [HarmonyPatch(typeof(Pawn_HealthTracker), "AddHediff")]
    public static class Patch_Pregnancy_AddHediff
    {
        [HarmonyPatch(new Type[] { typeof(Hediff), typeof(BodyPartRecord), typeof(DamageInfo?), typeof(DamageWorker.DamageResult) })]
        public static void Postfix(Pawn ___pawn, Hediff hediff)
        {
            if (___pawn == null || ___pawn.kindDef != MihoDefRef.Miho_adfqs) return;
            if (hediff == null) return;
            if (hediff.def == HediffDefOf.Pregnant)
            {
                if (___pawn.needs?.mood?.thoughts?.memories != null)
                {
                    ThoughtDef customDef = DefDatabase<ThoughtDef>.GetNamed("Thought_PregnantRealization", false);
                    if (customDef != null)
                    {
                        ___pawn.needs.mood.thoughts.memories.TryGainMemory(customDef);
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(MemoryThoughtHandler), "TryGainMemory", new Type[] { typeof(ThoughtDef), typeof(Pawn), typeof(Precept) })]
    public static class Patch_MemoryThoughtHandler_GlobalSwap
    {
        public static bool Prefix(MemoryThoughtHandler __instance, ref ThoughtDef def)
        {
            // 获取当前的小人
            Pawn p = __instance.pawn;
            if (p == null || p.kindDef != MihoDefRef.Miho_adfqs) return true;
            if (def == ThoughtDefOf.GotMarried)
            {
                ThoughtDef customDef = DefDatabase<ThoughtDef>.GetNamed("Thoughts_GotMarriedIdeal", false);
                if (customDef != null) def = customDef;
            }
            else if (def == ThoughtDefOf.GotSomeLovin)
            {
                ThoughtDef customDef = DefDatabase<ThoughtDef>.GetNamed("Thoughts_GotSomeLovin", false);
                if (customDef != null) def = customDef;
            }
            else if (def == ThoughtDefOf.FailedRomanceAttemptOnMe)
            {
                ThoughtDef customDef = DefDatabase<ThoughtDef>.GetNamed("Miho_RejectedCourtship_Patriotism", false);
                if (customDef != null) def = customDef;
            }
            else if (ModsConfig.BiotechActive && def == ThoughtDefOf.BabyBorn)
            {
                ThoughtDef customDef = DefDatabase<ThoughtDef>.GetNamed("Thoughts_BabyBornIdeal", false);
                if (customDef != null) def = customDef;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Pawn_RelationsTracker), "AddDirectRelation")]
    public static class Patch_Relation_Acceptance
    {
        public static void Postfix(PawnRelationDef def, Pawn otherPawn, Pawn ___pawn)
        {
            if (___pawn == null || ___pawn.kindDef != MihoDefRef.Miho_adfqs) return;
            if (___pawn.needs?.mood?.thoughts?.memories == null) return;
            if (def == PawnRelationDefOf.Lover || def == PawnRelationDefOf.Fiance)
            {
                ThoughtDef customDef = DefDatabase<ThoughtDef>.GetNamed("Miho_AcceptedCourtship_DelayIdeal", false);
                if (customDef != null)
                {
                    if (___pawn.needs.mood.thoughts.memories.GetFirstMemoryOfDef(customDef) == null)
                    {
                        ___pawn.needs.mood.thoughts.memories.TryGainMemory(customDef, otherPawn);
                    }
                }
            }
        }
    }
}