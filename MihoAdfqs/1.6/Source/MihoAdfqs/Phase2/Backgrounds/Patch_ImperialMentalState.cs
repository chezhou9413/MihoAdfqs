using HarmonyLib;
using System.Linq;
using MihoAdfqs.Phase2.Helmer;
using Verse;
using Verse.AI;

namespace MihoAdfqs.Phase2.Backgrounds
{
    //职责：执行45号基因和背景的精神免疫，并为指定背景选择宠物猫屠宰行为。
    [HarmonyPatch(typeof(MentalStateHandler), nameof(MentalStateHandler.TryStartMentalState))]
    public static class Patch_ImperialMentalState
    {
        //职责：在精神状态创建前执行免疫，或为心情诱发的崩溃选择背景对应行为。
        public static bool Prefix(Pawn ___pawn, ref MentalStateDef stateDef, bool causedByMood, ref bool __result)
        {
            if (Gene_Helmer.Get(___pawn) == null && !BackgroundUtility.MentalImmune(___pawn))
            {
                if (causedByMood && BackgroundUtility.Effects(___pawn).Any(effect => effect.catSlaughter) && CatSlaughterUtility.FindCat(___pawn) != null)
                    stateDef = DefDatabase<MentalStateDef>.GetNamed("Slaughterer");
                return true;
            }
            __result = false;
            return false;
        }
    }
}
