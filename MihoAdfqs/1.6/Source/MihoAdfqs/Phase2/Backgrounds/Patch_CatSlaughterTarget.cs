using System.Linq;
using HarmonyLib;
using Verse;
using Verse.AI;

namespace MihoAdfqs.Phase2.Backgrounds
{
    //职责：把爱猫者背景的屠宰型崩溃目标限制在宠物猫。
    [HarmonyPatch(typeof(SlaughtererMentalStateUtility), nameof(SlaughtererMentalStateUtility.FindAnimal))]
    public static class Patch_CatSlaughterTarget
    {
        //职责：替代目标选择以免在猫不存在时转而伤害其它动物。
        public static bool Prefix(Pawn pawn, ref Pawn __result)
        {
            if (!BackgroundUtility.Effects(pawn).Any(effect => effect.catSlaughter)) return true;
            __result = CatSlaughterUtility.FindCat(pawn);
            return false;
        }
    }
}
