using HarmonyLib;
using Verse;

namespace MihoAdfqs.Phase2.Pawns
{
    //各部件保留一滴血后，累计伤口仍可能超过原版致命伤害阈值。
    [HarmonyPatch(typeof(Pawn_HealthTracker), nameof(Pawn_HealthTracker.ShouldBeDeadFromLethalDamageThreshold))]
    public static class Patch_FuhrerLethalDamage
    {
        //元首不因伤口合计死亡，保留疾病死亡与正常受伤、倒地和治疗流程。
        public static bool Prefix(Pawn ___pawn, ref bool __result)
        {
            if (!EmpirePawnUtility.Fuhrer(___pawn)) return true;
            __result = false;
            return false;
        }
    }
}
