using HarmonyLib;
using RimWorld;
using Verse;

namespace MihoAdfqs.Phase2.Factions
{
    //首领固定背景必须保留兵种要求的能力，避免生成人物时反复拒绝同一个背景。
    [HarmonyPatch(typeof(PawnBioAndNameGenerator), "TryGetRandomUnusedSolidBioFor")]
    public static class Patch_FactionLeaderBioWorkTags
    {
        //HAR的背景选择会覆盖原版筛选结果，此处补回首领的工作能力约束。
        [HarmonyAfter("rimworld.erdelf.alien_race.main")]
        public static void Postfix(PawnKindDef kind, ref PawnBio result, ref bool __result)
        {
            if (!__result || !kind.factionLeader || kind.requiredWorkTags == WorkTags.None) return;
            WorkTags disabled = (result.childhood?.workDisables ?? WorkTags.None)
                | (result.adulthood?.workDisables ?? WorkTags.None);
            WorkTags conflict = disabled & kind.requiredWorkTags;
            if (conflict == WorkTags.None) return;
            Log.WarningOnce($"[MihoAdfqs] 首领{kind.defName}的固定背景{result.name}禁用必需能力{conflict}，"
                + "已拒绝该背景，继续原版随机背景流程。", 20074100 + kind.shortHash);
            //原版会生成随机背景，HAR在填充随机背景时清理固定背景的装备与伤病引用。
            __result = false;
            result = null;
        }
    }
}
