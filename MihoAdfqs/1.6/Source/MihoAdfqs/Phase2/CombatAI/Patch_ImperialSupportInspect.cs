using HarmonyLib;
using Verse;

namespace MihoAdfqs.Phase2.CombatAI
{
    //选中支援兵时显示实际战备额度与个人冷却。
    [HarmonyPatch(typeof(Pawn), nameof(Pawn.GetInspectString))]
    public static class Patch_ImperialSupportInspect
    {
        //仅为地图上的支援兵补充原版信息栏。
        public static void Postfix(Pawn __instance, ref string __result)
        {
            if (!__instance.Spawned || __instance.kindDef.defName != "MihoPhase2_StratagemSupportSoldier") return;
            ImperialCombatPawnState state = __instance.Map.GetComponent<MapComponent_ImperialCombat>().State(__instance);
            int ticks = System.Math.Max(0, state.nextCallTick - Find.TickManager.TicksGame);
            __result += $"\n战备：{state.charges}/3" + (ticks > 0 ? $" · 冷却{ticks / 60f:F1}秒" : "");
        }
    }
}
