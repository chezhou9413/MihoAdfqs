using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace MihoAdfqs.Phase2.Rendering
{
    //职责：按照二期提供的男女贴图加载服装，避免原版强制拼接不存在的体型后缀。
    [HarmonyPatch(typeof(ApparelGraphicRecordGetter), nameof(ApparelGraphicRecordGetter.TryGetGraphicApparel))]
    public static class Patch_ApparelGraphics
    {
        //职责：二期服装使用自身标准方向图，有女性版本时按身体类型选用。
        [HarmonyPriority(Priority.First)]
        public static bool Prefix(Apparel apparel, BodyTypeDef bodyType, bool forStatue, ref ApparelGraphicRecord rec, ref bool __result)
        {
            if (!apparel.def.defName.StartsWith("MihoPhase2_") || string.IsNullOrEmpty(apparel.WornGraphicPath)) return true;
            string path = apparel.WornGraphicPath;
            if ((bodyType == BodyTypeDefOf.Female || apparel.Wearer?.gender == Gender.Female) && ContentFinder<Texture2D>.Get(path + "_Female_south", false) != null) path += "_Female";
            Graphic graphic = GraphicDatabase.Get<Graphic_Multi>(path, ShaderDatabase.Cutout, apparel.def.graphicData.drawSize, forStatue ? apparel.DrawColor : Color.white);
            rec = new ApparelGraphicRecord(graphic, apparel);
            __result = true;
            return false;
        }
    }
}
