using HarmonyLib;
using MihoAdfqs.MihoAdfDefRef;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MihoAdfqs.Patches
{
    [HarmonyPatch(typeof(LordJob_Joinable_MarriageCeremony), "WeddingSucceeded")]
    public static class Patch_WeddingSucceeded
    {
        public static void Postfix(LordJob_Joinable_MarriageCeremony __instance)
        {
            if (__instance.lord == null || __instance.lord.ownedPawns == null) return;
            List<Pawn> participants = __instance.lord.ownedPawns;
            ThoughtDef vanillaThought = DefDatabase<ThoughtDef>.GetNamed("AttendedWedding");
            ThoughtDef mihoThought = DefDatabase<ThoughtDef>.GetNamed("Thoughts_WitnessedMarriageBlessing", false);
            if (mihoThought == null) return;

            for (int i = 0; i < participants.Count; i++)
            {
                Pawn p = participants[i];
                if (p.Destroyed || p.Dead || p.needs?.mood?.thoughts?.memories == null) continue;
                bool isMiho = p.kindDef == MihoDefRef.Miho_adfqs;
                if (!isMiho) continue;
                MemoryThoughtHandler memories = p.needs.mood.thoughts.memories;
                if (memories.GetFirstMemoryOfDef(vanillaThought) != null)
                {
                    memories.RemoveMemoriesOfDef(vanillaThought);
                    memories.TryGainMemory(mihoThought);
                }
            }
        }
    }
}
