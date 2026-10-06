using System.Collections.Generic;
using System.Reflection;
using FacialAnimation;
using HarmonyLib;
using Verse;

namespace MihoAdfqs.FA
{
    //让提亚娜使用与专属贴图匹配的帝国表情，不改变其他米莉拉的动画。
    [HarmonyPatch]
    public static class Patch_TianaFaceAnimations
    {
        //定位FA按工作建立动画表的方法。
        public static MethodBase TargetMethod()
        {
            return AccessTools.Method(AccessTools.TypeByName("FacialAnimation.FAHelper"), "CreateAnimationDict");
        }

        //按当前工作绑定待机、眨眼、情绪、战斗、进食和休息表情。
        [HarmonyAfter("ChezhouLib.lib.fa")]
        public static void Postfix(Pawn pawn, int initialTick, Dictionary<string, List<FaceAnimation>> animationDict)
        {
            if (pawn.kindDef.defName != "MihoPhase2_Tiana") return;
            var animations = new List<FaceAnimation>();
            foreach (FaceAnimationDef def in DefDatabase<FaceAnimationDef>.AllDefsListForReading)
            {
                if (def.defName.StartsWith("MihoPhase2_Face_"))
                    animations.Add(new FaceAnimation(def, initialTick));
            }
            foreach (KeyValuePair<string, List<FaceAnimation>> entry in animationDict)
            {
                entry.Value.Clear();
                foreach (FaceAnimation animation in animations)
                {
                    if (animation.animationDef.IsSame("ConstantJob", "Alien_Miho") ||
                        animation.animationDef.IsSame(entry.Key, "Alien_Miho"))
                        entry.Value.Add(animation);
                }
                entry.Value.Sort((a, b) => a.animationDef.priority.CompareTo(b.animationDef.priority));
            }
        }
    }
}
