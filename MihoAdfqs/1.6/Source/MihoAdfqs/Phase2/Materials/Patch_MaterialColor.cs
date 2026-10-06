using HarmonyLib;
using UnityEngine;
using Verse;

namespace MihoAdfqs.Phase2.Materials
{
    //职责：让聚乙烯建筑使用浅灰色，武器和服装沿用材料定义中的深灰色。
    [HarmonyPatch(typeof(BuildableDef), nameof(BuildableDef.GetColorForStuff))]
    public static class Patch_MaterialColor
    {
        //职责：在建造预览与实物绘制使用同一聚乙烯建筑颜色。
        public static void Postfix(BuildableDef __instance, ThingDef stuff, ref Color __result)
        {
            if (stuff?.defName == "MihoPhase2_Polyethylene" && __instance is ThingDef thing && thing.category == ThingCategory.Building)
                __result = new Color(0.7f, 0.7f, 0.7f);
        }
    }
}
