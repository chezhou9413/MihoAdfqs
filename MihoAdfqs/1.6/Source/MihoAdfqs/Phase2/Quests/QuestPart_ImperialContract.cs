using System.Collections.Generic;
using System.Linq;
using MihoAdfqs.Phase2.Orbital;
using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using Verse;

namespace MihoAdfqs.Phase2.Quests
{
    //职责：跟踪帝国委托的地点、穿梭机交接、人员救援和唯一奖励结算。
    public class QuestPart_ImperialContract : QuestPartActivable
    {
        public string kind;
        public MapParent home;
        public Pawn negotiator;
        private Site site;
        private TransportShip ship;
        private List<Pawn> squad = new List<Pawn>();
        private bool visited;
        private bool squadSpawned;
        private bool dispatched;
        private bool rewardPending;
        private bool paid;
        private bool cancelling;
        private int finishAfter;
        public string TransportTag => "Quest" + quest.id + ".ImperialTransport";
        public bool CanSubmit => kind == "Recovery" && visited && ship == null;

        public override string DescriptionPart => kind == "Recovery"
            ? "交回：聚乙烯200、工业级钢板100、医药20。请在通讯器中提交任务并装载穿梭机。"
            : kind == "Rescue" ? "消灭包围小分队的敌人后，存活队员将撤回殖民地。" : "穿梭机起飞后开始计算10天借调时间。";

        public override IEnumerable<GlobalTargetInfo> QuestLookTargets
        {
            get { if (site != null && !site.Destroyed) yield return site; if (ship?.shipThing != null && !ship.shipThing.Destroyed) yield return ship.shipThing; }
        }

        //职责：接取任务后安排借调穿梭机或创建地图目标。
        protected override void Enable(SignalArgs receivedArgs)
        {
            base.Enable(receivedArgs);
            Map destination = home?.Map ?? Find.AnyPlayerHomeMap;
            if (destination == null) { quest.End(QuestEndOutcome.Fail); return; }
            home = destination.Parent;
            if (kind == "Social")
            {
                ship = ImperialShuttle.Receive(home.Map, TransportTag, pawn: negotiator);
                quest.PartsListForReading.OfType<QuestPart_ImperialLoan>().Single().shuttle = ship.shipThing;
            }
            else
            {
                site = ImperialMissionSites.Create(kind, home.Map);
                if (site == null)
                {
                    Messages.Message("没有适合本次委托的地点，任务已取消。", MessageTypeDefOf.RejectInput, false);
                    quest.End(QuestEndOutcome.Fail);
                }
            }
        }

        //职责：只在已到达回收地点后呼叫接收货物的穿梭机。
        public void Submit(Map map)
        {
            if (!CanSubmit)
            {
                Messages.Message("任务尚未满足提交条件，或穿梭机已经在途中。", MessageTypeDefOf.RejectInput, false);
                return;
            }
            home = map.Parent;
            ship = ImperialShuttle.Receive(map, TransportTag, ImperialMissionSites.ReturnCargo());
        }

        //职责：更换任务时先标记取消，阻止人员归还信号触发奖励。
        public void Cancel()
        {
            cancelling = true;
            quest.End(QuestEndOutcome.Fail, false);
        }

        //职责：只接受本任务穿梭机的完整提交和借调结束信号。
        public override void Notify_QuestSignalReceived(Signal signal)
        {
            base.Notify_QuestSignalReceived(signal);
            if (signal.tag == TransportTag + ".SentSatisfied")
            {
                dispatched = true;
                if (kind == "Recovery") ScheduleReward();
            }
            if (signal.tag == TransportTag + ".LoanComplete" && !cancelling) ScheduleReward();
        }

        //职责：延后结算到穿梭机发射流程之外，避免任务清理打断发射中的载荷转移。
        private void ScheduleReward()
        {
            rewardPending = true;
            finishAfter = Find.TickManager.TicksGame + 600;
        }

        //职责：跟踪任务地图进入、战场胜负和可结算的奖励。
        public override void QuestPartTick()
        {
            if (Find.TickManager.TicksGame % 60 != 0 || cancelling || paid) return;
            if (ship?.shipThing?.Destroyed == true && !dispatched) { quest.End(QuestEndOutcome.Fail); return; }
            if (negotiator != null && negotiator.Dead) { quest.End(QuestEndOutcome.Fail); return; }
            if (!rewardPending && kind != "Social" && (site == null || site.Destroyed) && !(kind == "Recovery" && visited))
            { quest.End(QuestEndOutcome.Fail); return; }
            if (!rewardPending && squadSpawned && squad.All(p => p.Dead || p.Destroyed))
            { quest.End(QuestEndOutcome.Fail); return; }
            Map map = site?.Map;
            if (map != null && map.mapPawns.FreeColonistsSpawned.Any())
            {
                visited = true;
                if (kind == "Rescue") UpdateRescue(map);
            }
            if (rewardPending && Find.TickManager.TicksGame >= finishAfter)
            {
                Map destination = home?.Map ?? Find.AnyPlayerHomeMap;
                if (destination == null) return;
                paid = true;
                ImperialMissionRewards.Grant(kind, destination);
                quest.End(QuestEndOutcome.Success);
            }
        }

        //职责：生成被困队员，敌军败退且仍有队员存活时执行撤离。
        private void UpdateRescue(Map map)
        {
            if (!squadSpawned) { squad = ImperialMissionSites.SpawnSquad(map); squadSpawned = true; }
            if (rewardPending) return;
            if (squad.All(p => p.Dead)) { quest.End(QuestEndOutcome.Fail); return; }
            if (map.mapPawns.AllPawnsSpawned.Any(p => !p.Downed && !p.Dead && p.HostileTo(Faction.OfPlayer))) return;
            Map destination = home?.Map ?? Find.AnyPlayerHomeMap;
            if (destination == null) return;
            var survivors = squad.Where(p => !p.Dead && !p.Destroyed).ToList();
            foreach (Pawn pawn in survivors)
            {
                ImperialPawnTransport.Detach(pawn);
            }
            OrbitalSupport.DropGoods(destination, survivors);
            ScheduleReward();
        }

        //职责：保持借调人员和活动穿梭机不被世界清理机制回收。
        public override bool QuestPartReserves(Pawn p) => p == negotiator || squad.Contains(p);

        //职责：为尚未结束的穿梭机登记任务引用。
        public override bool QuestPartReserves(TransportShip value) => value == ship;

        //职责：任务清理时归还尚未交出的载荷，并移除没有活动地图的委托地点。
        public override void Cleanup()
        {
            cancelling = true;
            ImperialShuttle.Cleanup(ship, !dispatched);
            if (site != null && !site.HasMap && !site.Destroyed) Find.WorldObjects.Remove(site);
            base.Cleanup();
        }

        //职责：保存委托地点、交接状态、队员与防重复结算标志。
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref kind, "kind");
            Scribe_References.Look(ref home, "home");
            Scribe_References.Look(ref negotiator, "negotiator");
            Scribe_References.Look(ref site, "site");
            Scribe_References.Look(ref ship, "ship");
            Scribe_Collections.Look(ref squad, "squad", LookMode.Reference);
            Scribe_Values.Look(ref visited, "visited");
            Scribe_Values.Look(ref squadSpawned, "squadSpawned");
            Scribe_Values.Look(ref dispatched, "dispatched");
            Scribe_Values.Look(ref rewardPending, "rewardPending");
            Scribe_Values.Look(ref paid, "paid");
            Scribe_Values.Look(ref cancelling, "cancelling");
            Scribe_Values.Look(ref finishAfter, "finishAfter");
        }
    }
}
