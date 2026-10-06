using HarmonyLib;
using Verse;

namespace MihoAdfqs.Phase2.Pawns
{
    //职责：让第三帝国人物按照兵种配装，阻止意识形态在生成末尾替换制服和头盔。
    [HarmonyPatch(typeof(PawnGenerator), "GenerateOrRedressPawnInternal")]
    public static class Patch_ImperialApparelGeneration
    {
        //职责：对帝国专属兵种的新生成人物和重新配装人物关闭意识形态赠衣。
        public static void Prefix(ref PawnGenerationRequest request)
        {
            if (request.KindDef.defaultFactionDef?.defName == "MihoThirdEmpire")
                request.ForceNoIdeoGear = true;
        }
    }
}
