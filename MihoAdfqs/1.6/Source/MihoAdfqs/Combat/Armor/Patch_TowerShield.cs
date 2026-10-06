using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace MihoAdfqs.Combat.Armor
{
    //在身体伤害处理前依次结算帝国能量护盾与塔盾，并保留穿透伤害。
    [HarmonyPatch(typeof(ThingWithComps), nameof(ThingWithComps.PreApplyDamage))]
    public static class Patch_TowerShield
    {
        //衣物拦截接口按值传递伤害，这里直接修改主流程中的伤害引用。
        public static void Postfix(ThingWithComps __instance, ref DamageInfo dinfo, ref bool absorbed)
        {
            if (absorbed || !(__instance is Pawn pawn) || dinfo.IgnoreArmor) return;
            if (pawn.apparel != null)
            {
                foreach (Apparel apparel in pawn.apparel.WornApparel)
                {
                    CompImperialShield energyShield = apparel.GetComp<CompImperialShield>();
                    if (energyShield == null) continue;
                    energyShield.AbsorbDamage(ref dinfo, out absorbed);
                    if (absorbed) return;
                }
            }
            AbsorbWithTowerShield(pawn, ref dinfo, ref absorbed);
        }

        //塔盾抵挡正面半圆内的直射攻击，先判定护甲，再消耗抵消值和盾体耐久。
        private static void AbsorbWithTowerShield(Pawn pawn, ref DamageInfo dinfo, ref bool absorbed)
        {
            ThingWithComps shield = pawn.equipment?.Primary;
            CompArmorReserve reserve = shield?.GetComp<CompArmorReserve>();
            if (reserve?.Settings.towerShield != true || dinfo.Amount <= 0f || !dinfo.Def.isRanged
                || dinfo.Def.isExplosive || dinfo.Def.ignoreShields || dinfo.Weapon?.building?.IsMortar == true) return;
            //弹丸角度指向飞行方向，反转后才是攻击来源方向。
            if (Mathf.Abs(Mathf.DeltaAngle(pawn.Rotation.AsAngle, dinfo.Angle + 180f)) > 90f) return;

            float remainder = ApplyShieldArmor(shield, ref dinfo);
            remainder = reserve.Absorb(remainder);
            if (remainder > 0f)
            {
                float block = Mathf.Min(shield.HitPoints, remainder);
                shield.HitPoints -= Mathf.CeilToInt(block);
                remainder -= block;
                if (shield.HitPoints <= 0) shield.Destroy();
            }
            dinfo.SetAmount(remainder);
            absorbed = remainder <= 0f;
        }

        //沿用原版的穿甲、全额格挡和半伤判定，盾体损耗由后续耐久结算承担。
        private static float ApplyShieldArmor(ThingWithComps shield, ref DamageInfo dinfo)
        {
            if (dinfo.Def.armorCategory == null) return dinfo.Amount;
            float rating = Mathf.Max(0f, shield.GetStatValue(dinfo.Def.armorCategory.armorRatingStat) - dinfo.ArmorPenetrationInt);
            float roll = Rand.Value;
            if (roll < rating * 0.5f) return 0f;
            if (roll >= rating) return dinfo.Amount;
            if (dinfo.Def.armorCategory == DamageArmorCategoryDefOf.Sharp) dinfo.Def = DamageDefOf.Blunt;
            return GenMath.RoundRandom(dinfo.Amount * 0.5f);
        }
    }
}
