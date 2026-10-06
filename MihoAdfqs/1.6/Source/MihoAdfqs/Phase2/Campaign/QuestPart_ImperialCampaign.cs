using System.Collections.Generic;
using System.Linq;
using MihoAdfqs.Phase2.Orbital;
using MihoAdfqs.Phase2.Quests;
using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using Verse;

namespace MihoAdfqs.Phase2.Campaign
{
    //职责：执行三条主线的地图目标、穿梭机加入与胜利奖励。
    public class QuestPart_ImperialCampaign : QuestPartActivable
    {
        public string kind;
        public MapParent home;
        private List<Site> sites = new List<Site>();
        private List<bool> cleared = new List<bool>();
        private TransportShip ship;
        private Pawn rewardPawn;
        private bool battleStarted;
        private bool rewarded;
        public override string DescriptionPart => kind == "Destiny" ? "已消灭据点：" + cleared.Count(x => x) + "/3" : null;
        public override IEnumerable<GlobalTargetInfo> QuestLookTargets => sites.Where(s => s != null && !s.Destroyed).Select(s => new GlobalTargetInfo(s));

        //职责：接取后生成所需地图或安排提亚娜的穿梭机。
        protected override void Enable(SignalArgs receivedArgs)
        {
            base.Enable(receivedArgs);
            Map destination = home?.Map ?? Find.AnyPlayerHomeMap;
            if (destination == null) { quest.End(QuestEndOutcome.Fail); return; }
            home = destination.Parent;
            if (kind == "Familiarity") { SendPawn("MihoPhase2_Tiana"); return; }
            int count = kind == "Destiny" ? 3 : 1;
            for (int i = 0; i < count; i++)
            {
                Site site = kind == "Destiny" ? ImperialMissionSites.Create("Rescue", home.Map) : CampaignBattleUtility.CreateOutpost(home.Map);
                if (site == null) { quest.End(QuestEndOutcome.Fail); return; }
                sites.Add(site);
                cleared.Add(false);
            }
        }

        //职责：以实际角色而非复制品作为加入奖励，乘穿梭机抵达殖民地。
        private void SendPawn(string kindDef)
        {
            Map map = home?.Map ?? Find.AnyPlayerHomeMap;
            if (map == null) { quest.End(QuestEndOutcome.Fail); return; }
            rewardPawn = PawnsFinder.AllMapsWorldAndTemporary_Alive.FirstOrDefault(p => p.kindDef.defName == kindDef)
                ?? PawnGenerator.GeneratePawn(new PawnGenerationRequest(DefDatabase<PawnKindDef>.GetNamed(kindDef),
                    Faction.OfPlayer, forceGenerateNewPawn: true));
            ImperialPawnTransport.Detach(rewardPawn);
            rewardPawn.SetFaction(Faction.OfPlayer);
            Thing shuttle = ThingMaker.MakeThing(ThingDefOf.Shuttle);
            shuttle.SetFaction(Faction.OfPlayer);
            ship = TransportShipMaker.MakeTransportShip(TransportShipDefOf.Ship_Shuttle, new[] { rewardPawn }, shuttle);
            ship.ArriveAt(IntVec3.Invalid, map.Parent);
            ship.AddJobs(ShipJobDefOf.Unload, ShipJobDefOf.FlyAway);
        }

        //职责：在实际战场清除或奖励人物落地后完成任务。
        public override void QuestPartTick()
        {
            if (Find.TickManager.TicksGame % 60 != 0 || rewarded) return;
            if (rewardPawn != null)
            {
                if (rewardPawn.Dead || rewardPawn.Destroyed || (!rewardPawn.Spawned && ship?.shipThing?.Destroyed == true))
                { quest.End(QuestEndOutcome.Fail); return; }
                if (!rewardPawn.Spawned || rewardPawn.Faction != Faction.OfPlayer) return;
                if (kind == "Destiny") GameComponent_ImperialCampaign.Current.captainJoined = true;
                Finish();
                return;
            }
            for (int i = 0; i < sites.Count; i++)
            {
                if (cleared[i]) continue;
                if (sites[i] == null || sites[i].Destroyed) { quest.End(QuestEndOutcome.Fail); return; }
                Map map = sites[i]?.Map;
                if (map == null || !map.mapPawns.FreeColonistsSpawned.Any()) continue;
                if (kind == "Vanguard" && !battleStarted)
                {
                    CampaignBattleUtility.Attack(map);
                    battleStarted = true;
                }
                if (!GenHostility.AnyHostileActiveThreatToPlayer(map, true, true)) cleared[i] = true;
            }
            if (sites.Count == 0 || cleared.Any(x => !x)) return;
            if (kind == "Destiny") SendPawn("MihoPhase2_LeaderGuardCaptain");
            else
            {
                Map map = home?.Map ?? Find.AnyPlayerHomeMap;
                if (map == null) return;
                OrbitalSupport.DropGoods(map, new[] { ThingMaker.MakeThing(DefDatabase<ResearchProjectDef>.GetNamed("MihoPhase2_Research_Nova").Techprint) });
                Finish();
            }
        }

        //职责：设置结算标记并结束任务。
        private void Finish() { rewarded = true; quest.End(QuestEndOutcome.Success); }

        //职责：保持奖励人物在运输过程中的世界引用。
        public override bool QuestPartReserves(Pawn p) => p == rewardPawn;

        //职责：在奖励人物落地前保留穿梭机引用。
        public override bool QuestPartReserves(TransportShip value) => value == ship && !rewarded;

        //职责：取消任务时安全卸下奖励人物，清理未进入的任务地点。
        public override void Cleanup()
        {
            ImperialShuttle.Cleanup(ship, true);
            foreach (Site site in sites.Where(s => s != null && !s.Destroyed && !s.HasMap)) Find.WorldObjects.Remove(site);
            base.Cleanup();
        }

        //职责：保存主线地图、胜利计数及特殊人物交付状态。
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref kind, "kind");
            Scribe_References.Look(ref home, "home");
            Scribe_Collections.Look(ref sites, "sites", LookMode.Reference);
            Scribe_Collections.Look(ref cleared, "cleared", LookMode.Value);
            Scribe_References.Look(ref ship, "ship");
            Scribe_References.Look(ref rewardPawn, "rewardPawn");
            Scribe_Values.Look(ref battleStarted, "battleStarted");
            Scribe_Values.Look(ref rewarded, "rewarded");
        }
    }
}
