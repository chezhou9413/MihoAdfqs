using HarmonyLib;
using Verse;

namespace MihoAdfqs.Phase2.Pawns
{
    //职责：让帝国美狐采用独立的一点一五体型数值。
    [HarmonyPatch(typeof(Pawn), "get_BodySize")]
    public static class Patch_ImperialSize
    {
        //职责：仅覆盖帝国美狐的体型，不改米莉拉和普通美狐。
        public static void Postfix(Pawn __instance, ref float __result)
        {
            if (EmpirePawnUtility.Imperial(__instance) && __instance.def.defName == "Alien_Miho") __result = 1.15f;
        }
    }
}
