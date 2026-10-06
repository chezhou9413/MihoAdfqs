using HarmonyLib;
using MihoAdfqs.Phase2.Orbital;
using Verse;

namespace MihoAdfqs.Phase2.Communications
{
    //职责：让通讯器制作配方在元首加入并建立联系后开放。
    [HarmonyPatch(typeof(RecipeDef), nameof(RecipeDef.AvailableNow), MethodType.Getter)]
    public static class Patch_CommunicatorRecipe
    {
        //职责：仅对通讯器配方追加网络解锁条件。
        public static void Postfix(RecipeDef __instance, ref bool __result)
        {
            if (__instance.defName == "Make_MihoPhase2_OrbitalCommunicator")
                __result &= GameComponent_OrbitalNetwork.Current.unlocked;
        }
    }
}
