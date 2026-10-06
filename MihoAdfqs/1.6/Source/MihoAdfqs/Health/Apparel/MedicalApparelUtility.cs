using RimWorld;
using Verse;

namespace MihoAdfqs.Health
{
    //检查共享医疗状态是否仍有实际穿戴装备作为来源。
    public static class MedicalApparelUtility
    {
        //高频移除检查直接遍历已有列表，不分配闭包和嵌套枚举器。
        public static bool HasSource(Hediff hediff)
        {
            var apparel = hediff.pawn.apparel?.WornApparel;
            if (apparel == null) return false;
            for (int i = 0; i < apparel.Count; i++)
            {
                var comps = apparel[i].def.comps;
                for (int j = 0; j < comps.Count; j++)
                    if (comps[j] is CompProperties_CauseHediff_Apparel source && source.hediff == hediff.def) return true;
            }
            return false;
        }
    }
}
