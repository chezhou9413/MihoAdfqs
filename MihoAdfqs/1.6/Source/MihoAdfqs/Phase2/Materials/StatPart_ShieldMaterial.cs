using System.Linq;
using RimWorld;
using Verse;

namespace MihoAdfqs.Phase2.Materials
{
    //职责：使帝国护盾的实际容量、信息页和能量条共同使用装备材料耐久倍率。
    public class StatPart_ShieldMaterial : StatPart
    {
        //职责：按照当前护盾材质放大最大能量。
        public override void TransformValue(StatRequest req, ref float value) => value *= Factor(req);

        //职责：在护盾容量信息中列出材料来源。
        public override string ExplanationPart(StatRequest req) => Factor(req) == 1f ? null : "护盾材料耐久：×" + Factor(req).ToString("0.###");

        //职责：仅对帝国护盾应用装备耐久系数，其它护盾维持原版容量。
        private static float Factor(StatRequest req)
        {
            if (!(req.BuildableDef is ThingDef def) || !def.comps.Any(c => c.compClass == typeof(Combat.Armor.CompImperialShield))) return 1f;
            switch (req.StuffDef?.defName)
            {
                case "MihoPhase2_IndustrialSteelPlate": return 4f;
                case "MihoPhase2_Polyethylene": return 2.5f;
                default: return req.StuffDef?.stuffProps.statFactors.GetStatFactorFromList(StatDefOf.MaxHitPoints) ?? 1f;
            }
        }
    }
}
