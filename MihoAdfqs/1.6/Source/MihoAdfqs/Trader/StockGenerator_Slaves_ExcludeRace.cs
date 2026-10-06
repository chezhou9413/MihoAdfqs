using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using Verse;

namespace MihoAdfqs.Trader
{
    //生成不包含指定种族的奴隶商品。
    public class StockGenerator_Slaves_ExcludeRace : StockGenerator_Slaves
    {
        public ThingDef excludedRaceDef;
        //专门的奴隶商队保证携带奴隶，不受商队派系随机意识形态的禁奴检查影响。
        public override IEnumerable<Thing> GenerateThings(PlanetTile forTile, Faction faction = null)
        { 
            foreach (Thing t in base.GenerateThings(forTile, null))
            {
                Pawn pawn = t as Pawn;
                if (pawn != null && pawn.def == excludedRaceDef)
                {
                    pawn.Destroy();
                    //原版Slave会被HAR随机替换种族，改用明确的智人成员避免再次生成美狐。
                    PawnKindDef replacementKind = PawnKindDefOf.Villager;
                    PawnGenerationRequest request = new PawnGenerationRequest(
                        kind: replacementKind,
                        faction: null,
                        context: PawnGenerationContext.NonPlayer,
                        tile: forTile,
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
                    if (faction != null && replacementPawn.Faction != faction)
                    {
                        replacementPawn.SetFaction(faction);
                    }
                    yield return replacementPawn;
                }
                else
                {
                    yield return t;
                }
            }
        }
    }
}
