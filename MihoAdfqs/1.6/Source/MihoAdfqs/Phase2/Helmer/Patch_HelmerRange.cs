using HarmonyLib;
using Verse;

namespace MihoAdfqs.Phase2.Helmer
{
    //使45号基因的远程武器射程随血液风暴姿态变化。
    [HarmonyPatch(typeof(VerbProperties), nameof(VerbProperties.AdjustedRange))]
    public static class Patch_HelmerRange
    {
        //先排除非人物、近战和非装备动词，再按当前姿态应用倍率。
        public static void Postfix(VerbProperties __instance, Verb ownerVerb, Thing attacker, ref float __result)
        {
            if (!(attacker is Pawn pawn) || pawn.genes == null || __instance.IsMeleeAttack
                || ownerVerb?.EquipmentSource == null) return;
            Gene_Helmer gene = Gene_Helmer.Get(pawn);
            if (gene != null) __result *= gene.storm ? 0.5f : 2f;
        }
    }
}
