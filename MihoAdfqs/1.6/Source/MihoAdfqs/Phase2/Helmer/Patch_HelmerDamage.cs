using HarmonyLib;
using Verse;

namespace MihoAdfqs.Phase2.Helmer
{
    //职责：处理翻滚免伤和血液风暴对45号基因承伤倍率的替换。
    [HarmonyPatch(typeof(Pawn), nameof(Pawn.PreApplyDamage))]
    public static class Patch_HelmerDamage
    {
        //职责：先拦截翻滚伤害，再将静态75%承伤换算为近攻50%；其它基因倍率照常叠乘。
        [HarmonyPriority(Priority.First)]
        public static bool Prefix(Pawn __instance, ref DamageInfo dinfo, ref bool absorbed)
        {
            if (__instance.ParentHolder is PawnFlyer_HelmerRoll)
            {
                dinfo.SetAmount(0f);
                absorbed = true;
                return false;
            }
            if (Gene_Helmer.Get(__instance)?.storm == true) dinfo.SetAmount(dinfo.Amount * (0.5f / 0.75f));
            return true;
        }
    }
}
