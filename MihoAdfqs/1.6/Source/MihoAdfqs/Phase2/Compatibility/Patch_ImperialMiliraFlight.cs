using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace MihoAdfqs.Phase2.Compatibility
{
    //为带佩尔克体的米莉拉身体视图保留原种族的四向扑翼动画。
    [HarmonyPatch]
    public static class Patch_ImperialMiliraFlight
    {
        //只在米莉拉启用时接入游戏的飞行动画选择入口。
        public static bool Prepare() => OptionalMods.Milira;

        //使用当前游戏的飞行接口，主程序集无需引用米莉拉程序集。
        public static MethodBase TargetMethod() => AccessTools.Method(typeof(Pawn_FlightTracker), "GetBestFlyAnimation");

        //原米莉拉补丁按身体名称判断，帝国身体视图需要按原始身体判断。
        public static void Postfix(Pawn pawn, Rot4? facingOverride, ref AnimationDef __result)
        {
            if (pawn == null) return;
            RaceProperties properties = pawn.RaceProps;
            RaceProperties original = ImperialRaceProperties.OriginalFor(properties);
            if (original == properties || original.body.defName != "Milira_Body") return;
            Rot4 facing = facingOverride ?? pawn.Rotation;
            string direction = facing == Rot4.North ? "North" : facing == Rot4.East ? "East" :
                facing == Rot4.West ? "West" : "South";
            __result = DefDatabase<AnimationDef>.GetNamed("Milira_Fly" + direction);
        }
    }
}
