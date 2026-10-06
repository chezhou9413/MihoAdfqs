using RimWorld;
using Verse;

namespace MihoAdfqs.Phase2.Materials
{
    //职责：使装备以需求声明的布料或钢铁为基准，换用材料时按护甲与耐久系数计算。
    public class StatPart_ApparelBaseline : StatPart
    {
        public StatDef armorPower;

        //职责：在品质和原版材料计算之外施加基准换算。
        public override void TransformValue(StatRequest req, ref float value) => value *= Factor(req);

        //职责：在信息页说明当前装备的基准与材料换算倍率。
        public override string ExplanationPart(StatRequest req) => Factor(req) == 1f ? null : "基准材料换算：×" + Factor(req).ToString("0.###");

        //职责：消除基准材料的重复耐久乘算，并按材料护甲能力相对换算防护。
        private float Factor(StatRequest req)
        {
            ThingDef baseline = req.BuildableDef?.GetModExtension<ApparelMaterialBaseline>()?.material;
            if (baseline == null || req.StuffDef == null) return 1f;
            if (armorPower == null) return 1f / baseline.stuffProps.statFactors.GetStatFactorFromList(StatDefOf.MaxHitPoints);
            if (baseline == req.StuffDef) return 1f;
            float basePower = baseline.GetStatValueAbstract(armorPower);
            //布料没有钝器材料值时，使用独立基础防护，其它材料按自身钝器系数换算。
            return req.StuffDef.GetStatValueAbstract(armorPower) / (basePower > 0f ? basePower : 1f);
        }
    }
}
