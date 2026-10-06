using HarmonyLib;
using System.Linq;
using RimWorld;
using Verse;

namespace MihoAdfqs.Phase2.Backgrounds
{
    //职责：为背景指定的老兵与烹饪教师生成明确的既往伤病。
    [HarmonyPatch(typeof(PawnGenerator), "GenerateNewPawnInternal")]
    public static class Patch_BackgroundScars
    {
        //职责：把老枪伤设为永久伤疤，并为有心脏病背景的人物保留动脉阻塞。
        public static void Postfix(Pawn __result)
        {
            if (__result == null) return;
            foreach (BackgroundEffects effect in BackgroundUtility.Effects(__result))
            {
                if (effect.oldGunshot)
                {
                    Hediff injury = HediffMaker.MakeHediff(DefDatabase<HediffDef>.GetNamed("Gunshot"), __result, __result.RaceProps.body.corePart);
                    injury.Severity = 3f;
                    injury.TryGetComp<HediffComp_GetsPermanent>().IsPermanent = true;
                    __result.health.AddHediff(injury);
                }
                if (effect.heartDisease)
                {
                    BodyPartRecord heart = __result.health.hediffSet.GetNotMissingParts().First(part => part.def == BodyPartDefOf.Heart);
                    Hediff hediff = __result.health.AddHediff(DefDatabase<HediffDef>.GetNamed("HeartArteryBlockage"), heart);
                    hediff.Severity = 0.2f;
                }
            }
        }
    }
}
