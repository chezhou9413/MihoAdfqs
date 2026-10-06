using System.Linq;
using HarmonyLib;
using MihoAdfqs.Genes;
using Verse;

namespace MihoAdfqs.Phase2.Medicine
{
    //职责：在医疗舱或肢体重建药剂恢复佩尔克体后，重新建立器官的初始成长效果。
    [HarmonyPatch(typeof(Pawn_HealthTracker), nameof(Pawn_HealthTracker.RestorePart))]
    public static class Patch_RestorePerkBody
    {
        //职责：只在恢复范围含佩尔克体时调用有效基因，未恢复的缺失器官不授予效果。
        public static void Postfix(Pawn ___pawn, BodyPartRecord part)
        {
            BodyPartRecord organ = ___pawn.RaceProps.body.AllParts.FirstOrDefault(p => p.def.defName == "MihoPhase2_PerkOrgan");
            for (BodyPartRecord current = organ; current != null; current = current.parent)
            {
                if (current != part) continue;
                foreach (Gene_GrantsHediff gene in ___pawn.genes.GenesListForReading.OfType<Gene_GrantsHediff>())
                    if (gene.Active) gene.PostAdd();
                return;
            }
        }
    }
}
