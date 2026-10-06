using System.Reflection;
using HarmonyLib;
using Verse;

namespace MihoAdfqs.Phase2.Helmer
{
    //职责：在瞄准耗时中叠乘血液风暴和背景倍率，不改变共享武器定义。
    [HarmonyPatch]
    public static class Patch_HelmerShootingTime
    {
        //职责：确定浮点数瞄准时长的原版访问入口。
        public static MethodBase TargetMethod() => AccessTools.PropertyGetter(typeof(Verb), nameof(Verb.WarmupTime));

        //职责：为远程武器叠乘姿态与背景的瞄准耗时。
        public static void Postfix(Verb __instance, ref float __result)
        {
            if (__instance.verbProps.IsMeleeAttack || __instance.EquipmentSource == null) return;
            __result *= Factor(__instance) * Backgrounds.BackgroundUtility.AimTime(__instance.CasterPawn)
                * Combat.Weapons.WeaponTiming.AimFactor(__instance.CasterPawn);
        }

        //职责：区分远程武器和近战动作并返回姿态对应的耗时倍率。
        public static float Factor(Verb verb)
        {
            Gene_Helmer gene = Gene_Helmer.Get(verb.CasterPawn);
            return gene == null || verb.verbProps.IsMeleeAttack || verb.EquipmentSource == null ? 1f : gene.storm ? 0.75f : 1.5f;
        }
    }
}
