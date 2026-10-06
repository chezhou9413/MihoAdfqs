using HarmonyLib;
using RimWorld;
using Verse;

namespace MihoAdfqs.Phase2.Pawns
{
    //让提亚娜保留正在穿着的专属服装，手动换装仍由原版处理。
    [HarmonyPatch(typeof(JobGiver_OptimizeApparel), "TryGiveJob")]
    public static class Patch_TianaApparelOptimization
    {
        //自动换装前使用原版强制服装标记，避免策略排除或评分替换。
        public static void Prefix(Pawn pawn)
        {
            if (pawn.kindDef.defName != "MihoPhase2_Tiana" || pawn.outfits == null) return;
            foreach (Apparel apparel in pawn.apparel.WornApparel)
                if (apparel.def.defName == "MihoPhase2_TianaChant"
                    || apparel.def.defName == "MihoPhase2_TianaMaidDress")
                    pawn.outfits.forcedHandler.SetForced(apparel, true);
        }
    }
}
