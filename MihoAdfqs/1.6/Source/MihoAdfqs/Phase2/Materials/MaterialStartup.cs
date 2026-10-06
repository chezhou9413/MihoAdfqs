using System.Collections.Generic;
using RimWorld;
using Verse;

namespace MihoAdfqs.Phase2.Materials
{
    //为材料耐久和护盾属性安装计算，并让工业钢板沿用钢铁颜色。
    [StaticConstructorOnStartup]
    public static class MaterialStartup
    {
        //保留原版属性部件，在材料定义完成加载后设置装备属性和钢板颜色。
        static MaterialStartup()
        {
            StatDef stat = StatDefOf.MaxHitPoints;
            if (stat.parts == null) stat.parts = new List<StatPart>();
            stat.parts.Add(new StatPart_MaterialDurability { parentStat = stat });
            AddBaseline(stat, null);
            AddBaseline(StatDefOf.ArmorRating_Sharp, StatDefOf.StuffPower_Armor_Sharp);
            AddBaseline(StatDefOf.ArmorRating_Blunt, StatDefOf.StuffPower_Armor_Blunt);
            AddBaseline(StatDefOf.ArmorRating_Heat, StatDefOf.StuffPower_Armor_Heat);
            StatDef shield = StatDefOf.EnergyShieldEnergyMax;
            if (shield.parts == null) shield.parts = new List<StatPart>();
            shield.parts.Add(new StatPart_ShieldMaterial { parentStat = shield });
            StatDef recharge = StatDefOf.EnergyShieldRechargeRate;
            if (recharge.parts == null) recharge.parts = new List<StatPart>();
            recharge.parts.Add(new StatPart_ImperialShieldRecharge { parentStat = recharge });
            DefDatabase<ThingDef>.GetNamed("MihoPhase2_IndustrialSteelPlate").stuffProps.color = ThingDefOf.Steel.stuffProps.color;
        }

        //职责：为指定属性附加装备基准换算，保留原有属性部件。
        private static void AddBaseline(StatDef stat, StatDef power)
        {
            if (stat.parts == null) stat.parts = new List<StatPart>();
            stat.parts.Add(new StatPart_ApparelBaseline { parentStat = stat, armorPower = power });
        }
    }
}
