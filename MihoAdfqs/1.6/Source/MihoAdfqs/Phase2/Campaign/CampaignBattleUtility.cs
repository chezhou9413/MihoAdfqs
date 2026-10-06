using System.Collections.Generic;
using System.Linq;
using MihoAdfqs.Phase2.Orbital;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace MihoAdfqs.Phase2.Campaign
{
    //职责：生成主线前哨以及由智人种与米莉拉组成的联合攻击部队。
    public static class CampaignBattleUtility
    {
        //职责：在附近建立友军据点，避免普通匪营的到访敌对惩罚。
        public static Site CreateOutpost(Map home)
        {
            if (!TileFinder.TryFindNewSiteTile(out PlanetTile tile, home.Tile)) return null;
            Site site = SiteMaker.MakeSite(DefDatabase<SitePartDef>.GetNamed("MihoPhase2_ImperialOutpost"), tile,
                OrbitalSupport.Empire, false, StorytellerUtility.DefaultSiteThreatPointsNow());
            Find.WorldObjects.Add(site);
            return site;
        }

        //职责：从地图边缘生成数量随任务威胁增长的混合进攻队伍。
        public static void Attack(Map map)
        {
            Faction enemy = Find.FactionManager.AllFactionsVisible.Where(f => f.def.humanlikeFaction &&
                f.HostileTo(Faction.OfPlayer) && f.HostileTo(OrbitalSupport.Empire) && !f.defeated).RandomElement();
            int count = Mathf.Clamp(Mathf.RoundToInt(StorytellerUtility.DefaultSiteThreatPointsNow() / 150), 20, 50);
            PawnKindDef human = PawnKindDefOf.Pirate;
            PawnKindDef milira = Compatibility.OptionalMods.Milira ? DefDatabase<PawnKindDef>.GetNamed("Milira_YoungWarrior") : null;
            var attackers = new List<Pawn>();
            for (int i = 0; i < count; i++)
            {
                Pawn pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(milira != null && i % 2 != 0 ? milira : human,
                    enemy, forceGenerateNewPawn: true));
                IntVec3 cell = CellFinder.RandomEdgeCell(map);
                GenSpawn.Spawn(pawn, CellFinder.RandomClosewalkCellNear(cell, map, 12), map);
                attackers.Add(pawn);
            }
            LordMaker.MakeNewLord(enemy, new LordJob_AssaultColony(enemy, false, false, canSteal: false), map, attackers);
        }
    }
}
