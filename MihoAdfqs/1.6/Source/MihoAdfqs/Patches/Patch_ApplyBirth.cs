using HarmonyLib;
using MihoAdfqs.MihoAdfDefRef;
using System;
using Verse;

namespace MihoAdfqs.Patches
{
    [HarmonyPatch(typeof(PawnGenerator), nameof(PawnGenerator.GeneratePawn), new Type[] { typeof(PawnGenerationRequest) })]
    public static class Patch_BirthKindFix
    {
        public static void Prefix(ref PawnGenerationRequest request)
        {
            // 检查是否为新生儿
            if (request.AllowedDevelopmentalStages == DevelopmentalStage.Newborn)
            {
                // 检查当前的 KindDef
                if (request.KindDef != null && request.KindDef == MihoDefRef.Miho_adfqs)
                {
                    // 替换为目标 KindDef
                    var targetKind = DefDatabase<PawnKindDef>.GetNamed("Miho_PlayerColonist", false);
                    if (targetKind != null)
                    {
                        request.KindDef = targetKind;
                    }
                }
            }
        }
    }
}
