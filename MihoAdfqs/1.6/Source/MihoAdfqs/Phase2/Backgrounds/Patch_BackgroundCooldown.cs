using HarmonyLib;
using Verse;

namespace MihoAdfqs.Phase2.Backgrounds
{
    //职责：让远程武器的轮次冷却与背景、指挥及血液风暴射击计时一致。
    [HarmonyPatch(typeof(VerbProperties), nameof(VerbProperties.AdjustedCooldown), new[] { typeof(Verb), typeof(Pawn) })]
    public static class Patch_BackgroundCooldown
    {
        //职责：只缩放远程武器冷却，不把射击加成应用到近战动作。
        public static void Postfix(Verb ownerVerb, Pawn attacker, ref float __result)
        {
            if (!ownerVerb.verbProps.IsMeleeAttack && ownerVerb.EquipmentSource != null)
                __result *= BackgroundUtility.ShotTime(attacker) * Helmer.Patch_HelmerShootingTime.Factor(ownerVerb)
                    * Medicine.InjectorUtility.ShotTime(attacker) * Combat.Weapons.WeaponTiming.ShotFactor(attacker);
        }
    }
}
