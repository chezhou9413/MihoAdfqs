using System.Collections.Generic;
using System.Linq;
using MihoAdfqs.Phase2.Orbital;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI.Group;

namespace MihoAdfqs.Phase2.Quests
{
    //职责：创建委托目标据点和回收物资，并生成被围困的小分队。
    public static class ImperialMissionSites
    {
        //职责：在殖民地附近创建可远征的委托地点。
        public static Site Create(string kind, Map home)
        {
            if (!TileFinder.TryFindNewSiteTile(out PlanetTile tile, home.Tile)) return null;
            bool recovery = kind == "Recovery";
            SitePartDef def = DefDatabase<SitePartDef>.GetNamed(recovery ? "ItemStash" : "BanditCamp");
            Faction faction = recovery ? null : Find.FactionManager.AllFactionsVisible
                .Where(f => f.HostileTo(Faction.OfPlayer) && f.HostileTo(OrbitalSupport.Empire)
                    && f.def.humanlikeFaction && !f.defeated).RandomElement();
            Site site = SiteMaker.MakeSite(def, tile, faction, threatPoints: StorytellerUtility.DefaultSiteThreatPointsNow());
            if (recovery)
            {
                SitePart part = site.parts[0];
                part.things = new ThingOwner<Thing>(part, false);
                part.things.TryAddRangeOrTransfer(OrbitalSupport.Pack(new[] { OrbitalSupport.Stack("MihoPhase2_Polyethylene", 400),
                    OrbitalSupport.Stack("MihoPhase2_IndustrialSteelPlate", 200), OrbitalSupport.Stack("MedicineIndustrial", 40) }));
            }
            Find.WorldObjects.Add(site);
            return site;
        }

        //职责：定义交回空投物资的一半，另半由玩家保留。
        public static List<ThingDefCount> ReturnCargo() => new List<ThingDefCount>
        {
            new ThingDefCount(DefDatabase<ThingDef>.GetNamed("MihoPhase2_Polyethylene"), 200),
            new ThingDefCount(DefDatabase<ThingDef>.GetNamed("MihoPhase2_IndustrialSteelPlate"), 100),
            new ThingDefCount(ThingDefOf.MedicineIndustrial, 20)
        };

        //职责：在远征队入图时放置四名帝国士兵，其中三名带有致倒地的非流血伤势。
        public static List<Pawn> SpawnSquad(Map map)
        {
            var result = new List<Pawn>();
            for (int i = 0; i < 4; i++)
            {
                Pawn pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(
                    DefDatabase<PawnKindDef>.GetNamed("MihoPhase2_DefenseSoldier"), OrbitalSupport.Empire,
                    forceGenerateNewPawn: true));
                GenSpawn.Spawn(pawn, CellFinder.RandomClosewalkCellNear(map.Center, map, 8), map);
                if (i < 3) HealthUtility.DamageUntilDowned(pawn, false);
                pawn.needs.food.CurLevelPercentage = 0.2f;
                result.Add(pawn);
            }
            LordMaker.MakeNewLord(OrbitalSupport.Empire,
                new LordJob_DefendPoint(map.Center, 8, 16, addFleeToil: false), map, result);
            return result;
        }
    }
}
