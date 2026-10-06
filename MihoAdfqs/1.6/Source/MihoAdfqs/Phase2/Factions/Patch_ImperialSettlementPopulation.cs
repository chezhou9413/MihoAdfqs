using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace MihoAdfqs.Phase2.Factions
{
    //职责：按人口权重抽取帝国据点成员，避免战斗成本再度扭曲80／15／5种族比例。
    [HarmonyPatch(typeof(PawnGroupMakerUtility), nameof(PawnGroupMakerUtility.ChoosePawnGenOptionsByPoints))]
    public static class Patch_ImperialSettlementPopulation
    {
        //职责：仅替代帝国据点的人口抽签，其它派系及战斗队伍继续使用原版规则。
        public static bool Prefix(float pointsTotal, List<PawnGenOption> options, PawnGroupMakerParms groupParms,
            ref IEnumerable<PawnGenOptionWithXenotype> __result)
        {
            if (groupParms?.faction?.def.defName != "MihoThirdEmpire" || groupParms.groupKind != PawnGroupKindDefOf.Settlement) return true;
            __result = Select(pointsTotal, options, groupParms);
            return false;
        }

        //职责：按定义权重选人并扣除实际人口预算，保留事件种子可复现性。
        private static IEnumerable<PawnGenOptionWithXenotype> Select(float points, List<PawnGenOption> options, PawnGroupMakerParms parms)
        {
            var result = new List<PawnGenOptionWithXenotype>();
            if (parms.seed.HasValue) Rand.PushState(parms.seed.Value);
            try
            {
                while (points > 0f)
                {
                    PawnGenOption option = options.RandomElementByWeight(item => item.selectionWeight);
                    XenotypeDef xenotype = PawnGenerator.XenotypesAvailableFor(option.kind, parms.faction.def, parms.faction)
                        .RandomElementByWeight(pair => pair.Value).Key;
                    var selected = new PawnGenOptionWithXenotype(option, xenotype, option.selectionWeight);
                    if (selected.Cost <= 0f) throw new System.InvalidOperationException("帝国据点人物成本必须为正数：" + option.kind);
                    result.Add(selected);
                    points -= selected.Cost;
                }
            }
            finally { if (parms.seed.HasValue) Rand.PopState(); }
            return result;
        }
    }
}
