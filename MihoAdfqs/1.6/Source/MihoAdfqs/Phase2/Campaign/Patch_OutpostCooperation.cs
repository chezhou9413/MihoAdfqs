using System.Linq;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace MihoAdfqs.Phase2.Campaign
{
    //职责：让前卫据点内的帝国守军与玩家共同作战，维持地图外原有外交关系。
    [HarmonyPatch]
    public static class Patch_OutpostCooperation
    {
        //职责：登记实体目标和派系目标两种原版敌对判定入口。
        private static System.Collections.Generic.IEnumerable<System.Reflection.MethodBase> TargetMethods()
        {
            yield return AccessTools.Method(typeof(GenHostility), "HostileTo", new[] { typeof(Thing), typeof(Thing) });
            yield return AccessTools.Method(typeof(GenHostility), "HostileTo", new[] { typeof(Thing), typeof(Faction) });
        }

        //职责：仅在指定前哨地图排除玩家与帝国之间的常规派系敌对。
        private static void Postfix(object[] __args, ref bool __result)
        {
            if (!__result) return;
            Thing first = (Thing)__args[0];
            Thing second = __args[1] as Thing;
            //生成世界和人物关系时没有前哨地图，直接保留原版敌对结果。
            Map map = first.MapHeld;
            if (map == null || (second != null && second.MapHeld != map)) return;
            if (!(map.Parent is Site site) || !site.parts.Any(p => p.def.defName == "MihoPhase2_ImperialOutpost")) return;
            //依据双方已有派系识别玩家与帝国，不依赖全局玩家派系的初始化时机。
            Faction firstFaction = first.Faction;
            Faction other = second?.Faction ?? __args[1] as Faction;
            if (!((firstFaction?.IsPlayer == true && other?.def.defName == "MihoThirdEmpire")
                || (firstFaction?.def.defName == "MihoThirdEmpire" && other?.IsPlayer == true))) return;
            //精神状态的强制敌对仍由原版处理，避免影响暴力崩溃。
            if (first is Pawn a && a.MentalState != null) return;
            if (second is Pawn b && b.MentalState != null) return;
            __result = false;
        }
    }
}
