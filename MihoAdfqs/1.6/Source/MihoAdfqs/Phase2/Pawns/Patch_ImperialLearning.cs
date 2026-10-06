using HarmonyLib;
using RimWorld;
using Verse;

namespace MihoAdfqs.Phase2.Pawns
{
    //职责：将帝国美狐的基础学习倍率设为百分之百，再叠加基因和成长效果。
    [HarmonyPatch(typeof(StatWorker), "GetBaseValueFor")]
    public static class Patch_ImperialLearning
    {
        //职责：校正种族基础学习值，不影响后续属性修正。
        public static void Postfix(StatRequest request, StatDef ___stat, ref float __result)
        {
            if (___stat == StatDefOf.GlobalLearningFactor && request.Thing is Pawn pawn && EmpirePawnUtility.Imperial(pawn) && pawn.def.defName == "Alien_Miho") __result = 1f;
        }
    }
}
