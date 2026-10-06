using HarmonyLib;
using RimWorld;
using Verse;

namespace MihoAdfqs.Phase2.Medicine
{
    //职责：在所有疾病、失血与药物容量修正结算后维持存活患者百分之百意识。
    [HarmonyPatch(typeof(PawnCapacityUtility), nameof(PawnCapacityUtility.CalculateCapacityLevel))]
    public static class Patch_InjectorConsciousness
    {
        //职责：覆盖意识最终值，避免其它容量上限或乘数抵消注射器效果。
        [HarmonyPriority(Priority.Last)]
        public static void Postfix(HediffSet diffSet, PawnCapacityDef capacity, ref float __result)
        {
            if (capacity == PawnCapacityDefOf.Consciousness && !diffSet.pawn.Dead
                && InjectorUtility.MaintainsConsciousness(diffSet.pawn)) __result = 1f;
        }
    }
}
