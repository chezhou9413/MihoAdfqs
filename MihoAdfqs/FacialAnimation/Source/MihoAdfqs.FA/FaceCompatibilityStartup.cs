using FacialAnimation;
using HarmonyLib;
using MihoAdfqs.Phase2.Appearance;
using Verse;

namespace MihoAdfqs.FA
{
    //连接动态脸显示设置与专属角色动画。
    [StaticConstructorOnStartup]
    public static class FaceCompatibilityStartup
    {
        //同步动态脸设置并注册提亚娜的动画补丁。
        static FaceCompatibilityStartup()
        {
            AppearanceHooks.UseAnimatedFace = IsEnabled;
            new Harmony("MihoAdfqs.FA").PatchAll();
        }

        //依据FA设置判断人物是否应该显示动态表情。
        private static bool IsEnabled(Pawn pawn)
        {
            if (pawn.gender == Gender.Female && !FacialAnimationMod.Settings.EnableFemaleDrawing) return false;
            if (pawn.gender == Gender.Male && !FacialAnimationMod.Settings.EnableMaleDrawing) return false;
            if (pawn.gender == Gender.None && !FacialAnimationMod.Settings.EnableNeitherDrawing) return false;
            return FacialAnimationMod.Settings.ShouldDrawRaceXenoType(pawn);
        }
    }
}
