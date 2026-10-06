using System.Linq;
using MihoAdfqs.Combat.Armor;
using RimWorld;
using Verse;

namespace MihoAdfqs.Phase2.Materials
{
    //让帝国护盾的恢复属性与最终容量保持相同的充满时间。
    public class StatPart_ImperialShieldRecharge : StatPart
    {
        //容量已包含品质和材质倍率，避免恢复速度重复套用或遗漏倍率。
        public override void TransformValue(StatRequest req, ref float value)
        {
            CompProperties_Shield shield = ShieldProperties(req);
            if (shield != null)
                value = StatDefOf.EnergyShieldEnergyMax.Worker.GetValue(req) * 60f / shield.startingTicksToReset;
        }

        //在属性详情中说明充满时间，供玩家核对恢复规则。
        public override string ExplanationPart(StatRequest req)
        {
            CompProperties_Shield shield = ShieldProperties(req);
            return shield == null ? null : $"护盾充满时间：{shield.startingTicksToReset / 60f:F0}秒（按最终容量计算）";
        }

        //只处理帝国护盾组件，保留其他护盾的原始恢复属性。
        private static CompProperties_Shield ShieldProperties(StatRequest req)
        {
            return (req.BuildableDef as ThingDef)?.comps.OfType<CompProperties_Shield>()
                .FirstOrDefault(comp => comp.compClass == typeof(CompImperialShield));
        }
    }
}
