using HarmonyLib;
using Verse;

namespace MihoAdfqs.Phase2.Helmer
{
    //职责：在死亡结算到复活的短暂间隔保留人物，避免尸体销毁导致永久清除。
    [HarmonyPatch(typeof(Pawn), nameof(Pawn.Discard))]
    public static class Patch_HelmerDiscard
    {
        //职责：仅阻止已有明确恢复记录的人物被永久丢弃。
        public static bool Prefix(Pawn __instance) => Verse.Current.Game == null || !GameComponent_HelmerRecovery.Current.Contains(__instance);
    }
}
