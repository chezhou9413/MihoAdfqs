using FacialAnimation;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace MihoAdfqs.FA
{
    //让提亚娜的眼睛初始化与基因刷新始终使用专属贴图。
    [HarmonyPatch(typeof(EyeballControllerComp))]
    public static class Patch_TianaEyeType
    {
        private static PawnKindDef tiana;
        private static EyeballTypeDef eyes;

        //专属面部定义仅在米莉拉动态表情兼容目录加载时存在。
        public static bool Prepare()
        {
            if (!ModsConfig.IsActive("Ancot.MiliraRace")
                || !ModsConfig.IsActive("Ancot.MiliraRaceFacialAnimation")) return false;
            tiana = DefDatabase<PawnKindDef>.GetNamed("MihoPhase2_Tiana");
            eyes = DefDatabase<EyeballTypeDef>.GetNamed("MihoPhase2_Tiana_Eyeball");
            return true;
        }

        //在首次初始化与渲染树重建前绑定专属眼睛。
        [HarmonyPrefix]
        [HarmonyPatch(nameof(EyeballControllerComp.InitializeIfNeed))]
        public static void InitializePrefix(EyeballControllerComp __instance)
        {
            BindEyes(__instance);
        }

        //提亚娜跳过普通米莉拉的随机眼睛选择及异色瞳抽取。
        [HarmonyPrefix]
        [HarmonyPatch("SetRandomFaceType")]
        public static bool RandomizePrefix(EyeballControllerComp __instance)
        {
            return !BindEyes(__instance);
        }

        //贴图已包含瞳色，只在类型或乘色变化时请求纹理刷新。
        private static bool BindEyes(EyeballControllerComp controller)
        {
            Pawn pawn = (Pawn)controller.parent;
            if (pawn.kindDef != tiana) return false;
            if (controller.FaceType == eyes && controller.FaceColor == Color.white
                && controller.FaceSecondColor == Color.white) return true;
            controller.FaceType = eyes;
            controller.FaceColor = Color.white;
            controller.FaceSecondColor = Color.white;
            controller.SetDirty();
            return true;
        }
    }
}
