using HarmonyLib;
using MihoAdfqs.MihoAdfDefRef;
using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

namespace MihoAdfqs.MihoAdfHarmony
{
    [HarmonyPatch(typeof(PawnDiedOrDownedThoughtsUtility), "TryGiveThoughts", new Type[] { typeof(Pawn), typeof(DamageInfo?), typeof(PawnDiedOrDownedThoughtsKind) })]
    public static class Patch_ColonistDeathThoughts
    {
        // 缓存 Def，不用每次都查数据库
        private static ThoughtDef customDef;
        private static ThoughtDef vanillaDef;

        public static void Postfix(Pawn victim, PawnDiedOrDownedThoughtsKind thoughtsKind)
        {
            if (thoughtsKind != PawnDiedOrDownedThoughtsKind.Died) return;
            if (!victim.IsColonist || victim.IsSlave) return;
            if (customDef == null) customDef = DefDatabase<ThoughtDef>.GetNamed("Thoughts_ColonistDiedRespect", false);
            if (vanillaDef == null) vanillaDef = ThoughtDefOf.KnowColonistDied;
            if (customDef == null) return;
            List<Pawn> allColonists = PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_FreeColonists;
            for (int i = 0; i < allColonists.Count; i++)
            {
                Pawn observer = allColonists[i];
                if (observer == victim) continue;
                if (observer.kindDef != MihoDefRef.Miho_adfqs) continue;
                if (observer.needs?.mood?.thoughts?.memories == null) continue;
                var memories = observer.needs.mood.thoughts.memories;
                List<Thought_Memory> memoryList = memories.Memories;
                Thought_Memory targetMemory = null;

                for (int j = 0; j < memoryList.Count; j++)
                {
                    if (memoryList[j].def == vanillaDef && memoryList[j].age < 2)
                    {
                        targetMemory = memoryList[j];
                        break;
                    }
                }

                if (targetMemory != null)
                {
                    memories.RemoveMemory(targetMemory);
                    memories.TryGainMemory(customDef);
                }
            }
        }
    }
}

