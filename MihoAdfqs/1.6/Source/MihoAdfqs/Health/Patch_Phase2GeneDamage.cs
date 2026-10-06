using HarmonyLib;
using MihoAdfqs.MihoAdfDefRef;
using Verse;

namespace MihoAdfqs.Health
{
    //在防护结算前应用四号实验基因的承伤倍率。
    [HarmonyPatch(typeof(Pawn), nameof(Pawn.PreApplyDamage))]
    public static class Patch_Phase2GeneDamage
    {
        //只处理基因倍率，佩尔克体限伤在最终伤口生成时结算。
        public static void Prefix(Pawn __instance, ref DamageInfo dinfo)
        {
            Gene gene = __instance.genes?.GetGene(MihoDefRef.Gene_MihoPhase2_Experiment4);
            if (gene != null && gene.Active) dinfo.SetAmount(dinfo.Amount * 0.15f);
        }
    }
}
