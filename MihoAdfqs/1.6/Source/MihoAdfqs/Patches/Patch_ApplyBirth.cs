using HarmonyLib;
using MihoAdfqs.MihoAdfDefRef;
using System;
using Verse;

namespace MihoAdfqs.Patches
{
    //新生儿请求使用美狐原版出生定义，避开元首的年龄和身份设置。
    [HarmonyPatch(typeof(PawnGenerator), nameof(PawnGenerator.GeneratePawn), new Type[] { typeof(PawnGenerationRequest) })]
    public static class Patch_BirthKindFix
    {
        //只替换新生儿的种类，保留原版零岁、遗传基因与亲子关系请求。
        public static void Prefix(ref PawnGenerationRequest request)
        {
            if (request.AllowedDevelopmentalStages.Newborn() && request.KindDef == MihoDefRef.Miho_adfqs)
                request.KindDef = DefDatabase<PawnKindDef>.GetNamed("Miho_PlayerColonistBorn");
        }
    }
}
