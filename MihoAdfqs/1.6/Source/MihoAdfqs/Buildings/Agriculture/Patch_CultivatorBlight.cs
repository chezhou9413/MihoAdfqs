using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace MihoAdfqs.Buildings.Agriculture
{
    //职责：阻止枯萎病进入培养机内部的植物。
    [HarmonyPatch(typeof(Plant), nameof(Plant.CropBlighted))]
    public static class Patch_CultivatorBlight
    {
        //职责：仅拦截位于培养机占地内的植物感染。
        public static bool Prefix(Plant __instance) => !__instance.Spawned || !__instance.Position.GetThingList(__instance.Map).Any(t => t is Building_IndoorCultivator);
    }
}
