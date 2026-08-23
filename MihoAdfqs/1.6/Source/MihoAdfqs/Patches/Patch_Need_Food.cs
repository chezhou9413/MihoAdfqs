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
    [HarmonyPatch(typeof(Need_Food), "NeedInterval")]
    public static class Patch_Need_Food
    {
        public static bool Prefix(Need_Food __instance, Pawn ___pawn)
        {
            if (___pawn?.Faction?.def == MihoDefRef.MihoThirdEmpire)
            {
                __instance.CurLevel = __instance.MaxLevel;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Need_Mood), "NeedInterval")]
    public static class Patch_Need_Mood
    {
        [HarmonyPrefix]
        public static bool Prefix(Need_Mood __instance, ref Pawn ___pawn)
        {
            if (___pawn?.Faction?.def == MihoDefRef.MihoThirdEmpire)
            {
                __instance.CurLevel = 1f;
                return false;
            }
            return true;
        }
    }
}
