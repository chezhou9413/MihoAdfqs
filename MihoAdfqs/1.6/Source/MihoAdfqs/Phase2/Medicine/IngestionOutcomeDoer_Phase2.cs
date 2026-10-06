using RimWorld;
using Verse;

namespace MihoAdfqs.Phase2.Medicine
{
    //职责：执行二期注射剂的即时治疗、止血、肢体重建与同化行为。
    public class IngestionOutcomeDoer_Phase2 : IngestionOutcomeDoer
    {
        public bool heal;
        public bool restore;
        public bool convert;

        //职责：在正常注射或医疗施药完成后作用于受药者。
        protected override void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested, int ingestedCount)
        {
            if (convert)
            {
                RaceConversion.Convert(pawn, Compatibility.OptionalMods.Milira && Rand.Bool);
                return;
            }
            if (restore) MedicalUtility.RestoreMissing(pawn);
            MedicalUtility.Treat(pawn, heal ? float.MaxValue : 0);
        }
    }
}
