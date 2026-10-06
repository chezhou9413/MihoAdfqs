using RimWorld;
using Verse;

namespace MihoAdfqs.Phase2.Materials
{
    //职责：区分工业钢板和聚乙烯用于建筑与装备时的耐久倍率。
    public class StatPart_MaterialDurability : StatPart
    {
        //职责：将原材料的建筑耐久倍率换算为装备耐久倍率。
        public override void TransformValue(StatRequest req, ref float value) => value *= Factor(req);

        //职责：在属性说明中列出装备与建筑的倍率差异。
        public override string ExplanationPart(StatRequest req) => Factor(req) == 1f ? null :
            "装备材料耐久：" + (req.StuffDef.defName == "MihoPhase2_IndustrialSteelPlate" ? "×4（建筑×4.3）" : "×2.5（建筑×2）");

        //职责：只对使用指定材料制造的武器和衣物进行修正。
        private static float Factor(StatRequest req)
        {
            if (!(req.BuildableDef is ThingDef item) || !item.IsApparel && !item.IsWeapon) return 1f;
            switch (req.StuffDef?.defName)
            {
                case "MihoPhase2_IndustrialSteelPlate": return 4f / 4.3f;
                case "MihoPhase2_Polyethylene": return 2.5f / 2f;
                default: return 1f;
            }
        }
    }
}
