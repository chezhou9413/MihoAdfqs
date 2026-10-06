using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace MihoAdfqs.Phase2.Factions
{
    //职责：为超过一万点的敌对帝国袭击加入一至两名制式米莉拉战士。
    [HarmonyPatch(typeof(IncidentWorker_Raid), nameof(IncidentWorker_Raid.TryGenerateRaidInfo))]
    public static class Patch_ImperialRaidMilira
    {
        //职责：保留袭击策略修正之前的事件点数，保证一万点门槛使用原始袭击预算。
        public static void Prefix(IncidentParms parms, out float __state) => __state = parms.points > 0f
            ? parms.points : StorytellerUtility.DefaultThreatPointsNow(parms.target);

        //职责：在成功袭击生成后让米莉拉按同一到达方式入场，并纳入后续领队和通知处理。
        public static void Postfix(IncidentWorker_Raid __instance, IncidentParms parms, List<Pawn> pawns, bool debugTest, bool __result, float __state)
        {
            if (!Compatibility.OptionalMods.Milira || !__result || !(__instance is IncidentWorker_RaidEnemy) || __state <= 10000f || parms.faction?.def.defName != "MihoThirdEmpire"
                || !parms.faction.HostileTo(Faction.OfPlayer)) return;
            var additions = new List<Pawn>();
            int count = Rand.RangeInclusive(1, 2);
            for (int i = 0; i < count; i++)
                additions.Add(PawnGenerator.GeneratePawn(new PawnGenerationRequest(DefDatabase<PawnKindDef>.GetNamed("MihoPhase2_ImperialMiliraRaider"),
                    parms.faction, PawnGenerationContext.NonPlayer, parms.target.Tile, forceGenerateNewPawn: true, allowPregnant: false)));
            if (!debugTest) parms.raidArrivalMode.Worker.Arrive(additions, parms);
            pawns.AddRange(additions);
            parms.pawnCount = pawns.Count;
        }
    }
}
