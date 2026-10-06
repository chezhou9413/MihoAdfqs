using HarmonyLib;
using Verse;

namespace MihoAdfqs.Combat.Armor
{
    //在原版逐件结算覆盖部位的护甲时，先消耗该装备的抵消值。
    [HarmonyPatch(typeof(ArmorUtility), "ApplyArmor")]
    public static class Patch_ArmorReserve
    {
        //抵消值耗尽前不损耗耐久，未吸收的伤害继续使用原版护甲判定。
        public static bool Prefix(Thing armorThing, ref float damAmount, out bool metalArmor)
        {
            metalArmor = false;
            CompArmorReserve reserve = (armorThing as ThingWithComps)?.GetComp<CompArmorReserve>();
            if (reserve == null || reserve.reserve <= 0f || damAmount <= 0f) return true;
            damAmount = reserve.Absorb(damAmount);
            if (damAmount > 0f) return true;
            metalArmor = armorThing.def.apparel.useDeflectMetalEffect || armorThing.Stuff?.IsMetal == true;
            return false;
        }
    }
}
