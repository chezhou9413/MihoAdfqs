using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace MihoAdfqs.Phase2.Backgrounds
{
    //职责：统一读取幼年和成年背景效果，避免各玩法重复解释背景名称。
    public static class BackgroundUtility
    {
        //职责：枚举人物当前实际拥有的背景扩展。
        public static IEnumerable<BackgroundEffects> Effects(Pawn pawn)
        {
            if (pawn?.story == null) yield break;
            foreach (BackstoryDef story in pawn.story.AllBackstories)
            {
                BackgroundEffects effect = story?.GetModExtension<BackgroundEffects>();
                if (effect != null) yield return effect;
            }
        }

        //职责：判断背景是否禁止精神异常。
        public static bool MentalImmune(Pawn pawn) => Effects(pawn).Any(effect => effect.mentalImmune);

        //职责：叠乘射击耗时，并把指挥光环最多计算一次。
        public static float ShotTime(Pawn pawn)
        {
            float value = 1f;
            foreach (BackgroundEffects effect in Effects(pawn)) value *= effect.shotTime;
            if (pawn?.health?.hediffSet.HasHediff(DefDatabase<HediffDef>.GetNamed("MihoPhase2_CommandAura")) == true) value *= 0.5f;
            return value;
        }

        //职责：叠乘各背景对瞄准耗时的影响。
        public static float AimTime(Pawn pawn)
        {
            float value = 1f;
            foreach (BackgroundEffects effect in Effects(pawn)) value *= effect.aimTime;
            return value;
        }
    }
}
