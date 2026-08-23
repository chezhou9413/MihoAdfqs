using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using Verse;

namespace MihoAdfqs.Trader
{
    public class StockGenerator_Slaves_ExcludeRace : StockGenerator_Slaves
    {
        public ThingDef excludedRaceDef;
        public override IEnumerable<Thing> GenerateThings(PlanetTile forTile, Faction faction = null)
        { 
            foreach (Thing t in base.GenerateThings(forTile, faction))
            {
                Pawn pawn = t as Pawn;
                if (pawn != null && pawn.def == excludedRaceDef)
                {
                    pawn.Destroy();
                    PawnKindDef replacementKind = PawnKindDefOf.Slave;
                    PawnGenerationRequest request = new PawnGenerationRequest(
                        kind: replacementKind,
                        faction: null, // <--- 重点：暂时不给派系，避免被种族锁死
                        context: PawnGenerationContext.NonPlayer,
                        tile: -1,
                        forceGenerateNewPawn: true,
                        allowDead: false,
                        allowDowned: false,
                        canGeneratePawnRelations: true,
                        mustBeCapableOfViolence: false,
                        colonistRelationChanceFactor: 1f,
                        forceAddFreeWarmLayerIfNeeded: false,
                        allowGay: true,
                        allowPregnant: false
                    );

                    Pawn replacementPawn = PawnGenerator.GeneratePawn(request);
                    if (replacementPawn.Faction != faction)
                    {
                        replacementPawn.SetFaction(faction);
                    }
                    if (replacementPawn.def != excludedRaceDef)
                    {
                        yield return replacementPawn;
                    }
                    // --- 修正重点结束 ---
                }
                else
                {
                    yield return t;
                }
            }
        }
    }
}
