using HarmonyLib;
using Verse;

namespace MihoAdfqs.Phase2.Pawns
{
    //为帝国人物选择启动时已建立的佩尔克体身体结构。
    [HarmonyPatch(typeof(Pawn), "get_RaceProps")]
    public static class Patch_ImperialBody
    {
        //直接读取兵种索引，实际种族已改变的人物保留当前属性。
        public static void Postfix(Pawn __instance, ref RaceProperties __result)
        {
            RaceProperties replacement = ImperialBodyRegistry.ForKind(__instance.kindDef);
            if (replacement != null && __result == __instance.kindDef.race.race) __result = replacement;
        }
    }
}
