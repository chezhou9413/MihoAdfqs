using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace MihoAdfqs.Combat.Weapons
{
    //职责：按文档给出的实际距离节点计算武器命中系数，保留射手与掩体的原版结算。
    [HarmonyPatch(typeof(VerbProperties), nameof(VerbProperties.GetHitChanceFactor))]
    public static class Patch_MagazineAccuracy
    {
        //职责：使用独立距离曲线，并通过装备属性保留品质及其它精度修正。
        public static bool Prefix(VerbProperties __instance, Thing equipment, float dist, ref float __result)
        {
            if (!(__instance is VerbProperties_Magazine settings) || settings.accuracyByDistance == null) return true;
            float factor = equipment == null ? 1f : equipment.GetStatValue(StatDefOf.AccuracyTouch)
                / equipment.def.GetStatValueAbstract(StatDefOf.AccuracyTouch);
            __result = Mathf.Clamp(settings.accuracyByDistance.Evaluate(dist) * factor, 0.01f, 1f);
            return false;
        }
    }
}
