using RimWorld;
using Verse;

namespace MihoAdfqs.Combat.Weapons
{
    //职责：统一计算装备提供的换弹倍率，保持乘算关系。
    public static class WeaponTiming
    {
        //职责：叠乘夜视仪瞄准时间；空降装备的瞄准倍率由其健康状态提供。
        public static float AimFactor(Pawn pawn) => ShootingFactor(pawn, false);

        //职责：叠乘夜视仪和空降装备的连发间隔与轮次冷却。
        public static float ShotFactor(Pawn pawn) => ShootingFactor(pawn, true);

        //职责：按装备实际穿戴情况计算射击计时，避免把百分比写成属性减值。
        private static float ShootingFactor(Pawn pawn, bool includeOrbital)
        {
            float factor = 1f;
            if (pawn?.apparel == null) return factor;
            foreach (Apparel item in pawn.apparel.WornApparel)
                switch (item.def.defName)
                {
                    case "MihoPhase2_PSOStandardNVGHelmet": factor *= 0.5f; break;
                    case "MihoPhase2_PSOSpecialNVGHelmet": factor *= 0.4f; break;
                    case "MihoPhase2_SupernovaDropHelmet":
                    case "MihoPhase2_SupernovaDropArmor": if (includeOrbital) factor *= 0.25f; break;
                }
            return factor;
        }

        //职责：叠乘夜视仪及空降头盔的换弹效果。
        public static float ReloadFactor(Pawn pawn)
        {
            float factor = Phase2.Helmer.Gene_Helmer.Get(pawn)?.storm == true ? 0.5f : 1f;
            if (pawn?.apparel == null) return factor;
            foreach (Apparel item in pawn.apparel.WornApparel)
            {
                switch (item.def.defName)
                {
                    case "MihoPhase2_PSOStandardNVGHelmet": factor *= 0.5f; break;
                    case "MihoPhase2_PSOSpecialNVGHelmet": factor *= 0.4f; break;
                    case "MihoPhase2_SupernovaDropHelmet": factor *= 0.5f; break;
                }
            }
            return factor;
        }
    }
}
