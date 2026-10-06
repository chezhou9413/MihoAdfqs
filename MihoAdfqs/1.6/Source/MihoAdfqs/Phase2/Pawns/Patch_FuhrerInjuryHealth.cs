using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace MihoAdfqs.Phase2.Pawns
{
    //限制秋霜与提亚娜的伤口，秋霜失血后仍保留虚弱效果。
    [HarmonyPatch(typeof(Hediff), nameof(Hediff.Severity), MethodType.Setter)]
    public static class Patch_FuhrerInjuryHealth
    {
        //失血停在致死阈值以下，伤口按同部件剩余生命值截断。
        public static void Prefix(Hediff __instance, ref float value)
        {
            if (__instance.pawn == null) return;
            if (__instance.def == HediffDefOf.BloodLoss)
            {
                if (EmpirePawnUtility.Fuhrer(__instance.pawn))
                    value = Mathf.Min(value, __instance.def.lethalSeverity * 0.99f);
                return;
            }
            if (!(__instance is Hediff_Injury) || __instance.Part == null
                || value <= __instance.Severity
                || !(EmpirePawnUtility.Fuhrer(__instance.pawn) || EmpirePawnUtility.Immortal(__instance.pawn))) return;
            value = Mathf.Min(value, InjuryLimit(__instance.pawn, __instance.Part, __instance));
        }

        //使用未取整的伤口合计，保证实际剩余生命值也不低于一。
        internal static float InjuryLimit(Pawn pawn, BodyPartRecord part, Hediff injury)
        {
            float remaining = part.def.GetMaxHealth(pawn) - 1f;
            var hediffs = pawn.health.hediffSet.hediffs;
            for (int i = 0; i < hediffs.Count; i++)
            {
                Hediff other = hediffs[i];
                if (other != injury && other.Part == part && other is Hediff_Injury)
                    remaining -= other.Severity;
            }
            return Mathf.Max(0f, remaining);
        }
    }
}
