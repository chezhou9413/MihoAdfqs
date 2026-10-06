using HarmonyLib;
using Verse;

namespace MihoAdfqs.Phase2.Pawns
{
    //一滴血的脑部仍会因疼痛失去意识，外部呼吸通路也可能失去效率。
    [HarmonyPatch(typeof(Pawn_HealthTracker), nameof(Pawn_HealthTracker.ShouldBeDeadFromRequiredCapacity))]
    public static class Patch_FuhrerRequiredCapacity
    {
        //元首保留能力损伤与倒地，不因生命维持能力归零直接死亡。
        public static bool Prefix(Pawn ___pawn, ref PawnCapacityDef __result)
        {
            if (!EmpirePawnUtility.Fuhrer(___pawn)) return true;
            __result = null;
            return false;
        }
    }
}
