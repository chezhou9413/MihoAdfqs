using HarmonyLib;
using Verse;

namespace MihoAdfqs.Combat.Weapons
{
    //装备动词每刻结算换弹，声音不依赖玩家选中武器。
    [HarmonyPatch(typeof(Verb), nameof(Verb.VerbTick))]
    internal static class Patch_MagazineReload
    {
        //主枪和下挂使用各自的计时器，仅在结束时播放一次。
        private static void Postfix(Verb __instance)
        {
            if (__instance is Verb_Magazine magazine) magazine.CompleteReload();
        }
    }
}
