using HarmonyLib;
using Verse;

namespace MihoAdfqs.Phase2.Helmer
{
    //职责：避免恢复等待期的尸体销毁连带删除人物服装与随身物品。
    [HarmonyPatch(typeof(Corpse), nameof(Corpse.PostCorpseDestroy))]
    public static class Patch_HelmerCorpsePreservation
    {
        //职责：保留已经登记恢复的人物内部容器，其余尸体照常结算。
        public static bool Prefix(Pawn pawn) => Verse.Current.Game == null || !GameComponent_HelmerRecovery.Current.Contains(pawn);
    }
}
