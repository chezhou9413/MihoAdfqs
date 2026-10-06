using System.Linq;
using HarmonyLib;
using RimWorld;

namespace MihoAdfqs.Phase2.Backgrounds
{
    //职责：让国防军士兵背景在同伴死亡的原版心情上额外减少三点。
    [HarmonyPatch(typeof(Thought_Memory), nameof(Thought_Memory.MoodOffset))]
    public static class Patch_BackgroundDeathMood
    {
        //职责：只调整当前生效的殖民者死亡记忆，不重复创建叠加记忆。
        public static void Postfix(Thought_Memory __instance, ref float __result)
        {
            if (__instance.def == ThoughtDefOf.KnowColonistDied && __result < 0f)
                __result -= BackgroundUtility.Effects(__instance.pawn).Sum(effect => effect.colleagueDeathPenalty);
        }
    }
}
