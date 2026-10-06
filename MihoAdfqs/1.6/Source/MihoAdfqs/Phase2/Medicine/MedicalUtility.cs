using System.Linq;
using Verse;

namespace MihoAdfqs.Phase2.Medicine
{
    //职责：供注射器、医疗舱与装备共享伤口处理和部件恢复操作。
    public static class MedicalUtility
    {
        //职责：包扎所有可治疗伤口，按部件分别分配本次恢复量。
        public static void Treat(Pawn pawn, float healingPerPart)
        {
            //缺肢创口也会流血，必须与普通损伤一并包扎。
            foreach (Hediff wound in pawn.health.hediffSet.hediffs.Where(h => h.BleedRate > 0 && h.TendableNow()).ToList())
                wound.Tended(1f, 1f, 1);
            foreach (var group in pawn.health.hediffSet.hediffs.OfType<Hediff_Injury>().ToList().GroupBy(h => h.Part))
            {
                float left = healingPerPart;
                foreach (Hediff_Injury injury in group)
                {
                    if (injury.TendableNow()) injury.Tended(1f, 1f, 1);
                    float amount = System.Math.Min(left, injury.Severity);
                    if (amount > 0) injury.Heal(amount);
                    left -= amount;
                }
            }
        }

        //职责：恢复最高层缺失部件，子部件由原版恢复流程重建。
        public static void RestoreMissing(Pawn pawn)
        {
            foreach (Hediff_MissingPart missing in pawn.health.hediffSet.GetMissingPartsCommonAncestors().ToList())
                pawn.health.RestorePart(missing.Part);
        }
    }
}
