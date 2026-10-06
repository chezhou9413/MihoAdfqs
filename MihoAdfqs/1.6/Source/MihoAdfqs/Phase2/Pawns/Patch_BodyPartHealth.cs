using HarmonyLib;
using RimWorld;
using Verse;

namespace MihoAdfqs.Phase2.Pawns
{
    //应用帝国身体和特殊基因的部位生命值倍率。
    [StaticConstructorOnStartup]
    [HarmonyPatch(typeof(BodyPartDef), nameof(BodyPartDef.GetMaxHealth))]
    public static class Patch_BodyPartHealth
    {
        private static readonly BodyPartDef PerkOrgan = DefDatabase<BodyPartDef>.GetNamed("MihoPhase2_PerkOrgan");
        private static readonly ThingDef MihoRace = DefDatabase<ThingDef>.GetNamed("Alien_Miho");

        //兵种与基因引用均已索引，热点查询只比较引用并读取当前激活状态。
        public static void Postfix(BodyPartDef __instance, Pawn pawn, ref float __result)
        {
            if (__instance == PerkOrgan) { __result = 40; return; }
            if (pawn.def == MihoRace && ImperialBodyRegistry.ForKind(pawn.kindDef) != null)
                __result *= __instance == BodyPartDefOf.Head || __instance == BodyPartDefOf.Torso ? 3 : 2;
            if (pawn.genes != null) __result *= ImperialGeneLookup.For(pawn).BodyHealthFactor;
        }
    }
}
