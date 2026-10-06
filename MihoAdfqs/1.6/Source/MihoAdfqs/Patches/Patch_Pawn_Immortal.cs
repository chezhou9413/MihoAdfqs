using HarmonyLib;
using MihoAdfqs.Phase2.Pawns;
using Verse;

namespace MihoAdfqs.Patches
{
    //阻止提亚娜进入死亡状态，保留普通疾病与倒地流程。
    [HarmonyPatch(typeof(Pawn), nameof(Pawn.Kill))]
    public static class Patch_Pawn_Immortal
    {
        //拦截最初标准基因携带者的死亡结算。
        public static bool Prefix(Pawn __instance) => !EmpirePawnUtility.Immortal(__instance);
    }
}
