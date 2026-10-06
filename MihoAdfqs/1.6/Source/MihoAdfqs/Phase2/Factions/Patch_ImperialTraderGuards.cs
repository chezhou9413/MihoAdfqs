using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace MihoAdfqs.Phase2.Factions
{
    //商队保留原版点数生成，并保证至少四名美狐护卫。
    [HarmonyPatch(typeof(PawnGroupKindWorker_Trader), "GenerateGuards")]
    public static class Patch_ImperialTraderGuards
    {
        //记录生成前的人数，避免把货物中的奴隶和驮兽算作护卫。
        public static void Prefix(List<Pawn> outPawns, out int __state) => __state = outPawns.Count;

        //补足低点数商队的护卫，再独立抽取一名米莉拉平民。
        public static void Postfix(PawnGroupMakerParms parms, List<Pawn> outPawns, int __state)
        {
            if (parms.faction?.def.defName != "MihoThirdEmpire") return;
            int guards = outPawns.Count - __state;
            for (int i = guards; i < 4; i++)
                outPawns.Add(Generate("MihoPhase2_DefenseSoldier", parms, true));
            if (Compatibility.OptionalMods.Milira && Rand.Chance(0.5f))
                outPawns.Add(Generate("MihoPhase2_ImperialMilira", parms, false));
        }

        //生成属于当前商队派系与意识形态的全新成年成员。
        private static Pawn Generate(string name, PawnGroupMakerParms parms, bool fighter) => PawnGenerator.GeneratePawn(new PawnGenerationRequest(
            DefDatabase<PawnKindDef>.GetNamed(name), parms.faction, PawnGenerationContext.NonPlayer, parms.tile,
            forceGenerateNewPawn: true, fixedIdeo: parms.ideo, allowPregnant: false, mustBeCapableOfViolence: fighter));
    }
}
