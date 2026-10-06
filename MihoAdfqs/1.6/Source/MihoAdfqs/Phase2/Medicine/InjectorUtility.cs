using Verse;

namespace MihoAdfqs.Phase2.Medicine
{
    //职责：集中提供注射器持续状态对射击和意识的影响。
    public static class InjectorUtility
    {
        //职责：计算不同注射器同时生效时的射击耗时倍率，同名健康状态仅计一次。
        public static float ShotTime(Pawn pawn)
        {
            if (pawn == null) return 1f;
            float factor = 1f;
            if (Has(pawn, "MihoPhase2_ActivationInjectorEffect")) factor *= 0.5f;
            if (Has(pawn, "MihoPhase2_StimulantInjectorEffect")) factor *= 0.75f;
            return factor;
        }

        //职责：识别六小时或十二小时的强制清醒状态。
        public static bool MaintainsConsciousness(Pawn pawn) => Has(pawn, "MihoPhase2_AnalgesicInjectorEffect")
            || Has(pawn, "MihoPhase2_StimulantInjectorEffect") || Has(pawn, "MihoPhase2_EmergencyInjectorEffect");

        //职责：查找指定注射器持续状态。
        private static bool Has(Pawn pawn, string name) =>
            pawn.health.hediffSet.HasHediff(DefDatabase<HediffDef>.GetNamed(name));
    }
}
