using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.Sound;
using Verse.AI.Group;

namespace MihoAdfqs.Phase2.Orbital
{
    //维护通讯进度、帝国点数、交付队列和永久动员战场。
    public class GameComponent_OrbitalNetwork : GameComponent
    {
        public int points;
        public int introductionStep;
        public bool mobilized;
        public bool unlocked;
        private List<OrbitalDelivery> deliveries = new List<OrbitalDelivery>();
        private List<OrbitalBattle> battles = new List<OrbitalBattle>();
        public IReadOnlyList<OrbitalDelivery> Deliveries => deliveries;
        public static GameComponent_OrbitalNetwork Current => Verse.Current.Game.GetComponent<GameComponent_OrbitalNetwork>();

        public string PointsLabel => DebugSettings.godMode ? "∞" : points.ToString("N0");

        //上帝模式可无限调用支援，普通模式按实际余额判断。
        public bool CanAfford(int cost) => DebugSettings.godMode || points >= cost;

        //职责：提供游戏组件构造入口。
        public GameComponent_OrbitalNetwork(Game game) { }

        //职责：保存帝国网络的全部跨地图状态。
        public override void ExposeData()
        {
            Scribe_Values.Look(ref points, "points");
            Scribe_Values.Look(ref introductionStep, "introductionStep");
            Scribe_Values.Look(ref mobilized, "mobilized");
            Scribe_Values.Look(ref unlocked, "unlocked");
            Scribe_Collections.Look(ref deliveries, "deliveries", LookMode.Deep);
            Scribe_Collections.Look(ref battles, "battles", LookMode.Deep);
        }

        //有效下单后扣费并播放战备呼叫声，取消选点不触发声音。
        public bool Purchase(string kind, int cost, Map map, IntVec3 cell, int delay)
        {
            if (map == null || !Find.Maps.Contains(map) || !cell.InBounds(map) || !CanAfford(cost))
            {
                Messages.Message("点数不足或目标地图已不可用。", DefDatabase<MessageTypeDef>.GetNamed("MihoPhase2_SilentSupport"), false);
                return false;
            }
            int charged = DebugSettings.godMode ? 0 : cost;
            points -= charged;
            var delivery = new OrbitalDelivery { kind = kind, refundPoints = charged, map = map, cell = cell, orderedTick = Find.TickManager.TicksGame,
                dueTick = Find.TickManager.TicksGame + delay, faction = OrbitalSupport.Empire };
            deliveries.Add(delivery);
            DefDatabase<SoundDef>.GetNamed("MihoSupport_CallIn").PlayOneShotOnCamera();
            Messages.Message(ImperialSupportOption.Label(kind) + "已下达，地图图标显示到达进度。",
                new TargetInfo(cell, map), DefDatabase<MessageTypeDef>.GetNamed("MihoPhase2_SilentSupport"));
            return true;
        }

        //亲卫技能确认后登记空降，保留原技能冷却和队长选项。
        public void QueueGuards(Pawn caller, IntVec3 cell, bool includeCaptain)
        {
            deliveries.Add(new OrbitalDelivery { kind = "Guards", map = caller.Map, cell = cell,
                includeCaptain = includeCaptain, faction = OrbitalSupport.Empire, caller = caller, orderedTick = Find.TickManager.TicksGame,
                dueTick = Find.TickManager.TicksGame });
            DefDatabase<SoundDef>.GetNamed("MihoSupport_CallIn").PlayOneShotOnCamera();
        }

        //永久动员的自动舰炮也经过可见入射阶段。
        private void QueueMobilizedStrike(Map map, IntVec3 cell)
        {
            deliveries.Add(new OrbitalDelivery { kind = "Bombard", map = map, cell = cell, faction = OrbitalSupport.Empire,
                orderedTick = Find.TickManager.TicksGame, dueTick = Find.TickManager.TicksGame + 180 });
        }

        //先确认整组空投占地再登记，成功下单后由呼叫方扣次数；第七秒落地。
        internal bool QueueCombatSupport(Pawn caller, string kind, IntVec3 cell)
        {
            int tick = Find.TickManager.TicksGame;
            ImperialSupportOption option = ImperialSupportOption.Get(kind);
            var delivery = new OrbitalDelivery { kind = kind, map = caller.Map, cell = cell,
                faction = caller.Faction, caller = caller, sourceLord = caller.GetLord(), npcCall = true,
                orderedTick = tick, dueTick = tick + (option.IsStrike ? option.delay : 300) };
            if (!option.IsStrike && !ImperialCombatDrop.TryPlanLanding(delivery, 12f)) return false;
            deliveries.Add(delivery);
            DefDatabase<SoundDef>.GetNamed("MihoSupport_CallIn").PlayOneShot(new TargetInfo(delivery.cell, caller.Map));
            return true;
        }

        //职责：在任务实际结算后发放帝国点数。
        public void AwardPoints(int amount) => points += amount;

        //职责：为成功发生的敌军袭击派遣十名空降兵，并开始每三十秒舰炮支援。
        public void OnRaid(Map map)
        {
            if (!mobilized || map == null || !map.IsPlayerHome) return;
            deliveries.Add(new OrbitalDelivery { kind = "Soldiers", map = map, cell = DropCellFinder.TradeDropSpot(map), faction = OrbitalSupport.Empire,
                orderedTick = Find.TickManager.TicksGame, dueTick = Find.TickManager.TicksGame, soldierCount = 10 });
            if (!battles.Any(b => b.map == map))
                battles.Add(new OrbitalBattle { map = map, nextStrike = Find.TickManager.TicksGame + 1800 });
        }

        //每刻推进交付，每秒检查永久动员战场中的敌军。
        public override void GameComponentTick()
        {
            int tick = Find.TickManager.TicksGame;
            foreach (OrbitalDelivery delivery in deliveries.ToList()) TickDelivery(delivery, tick);
            if (tick % 60 != 0) return;
            if (!unlocked && PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_FreeColonists
                .Any(p => p.kindDef.defName == "Miho_adfqs")) unlocked = true;
            foreach (OrbitalBattle battle in battles.ToList())
            {
                if (battle.map == null || !Find.Maps.Contains(battle.map)) { battles.Remove(battle); continue; }
                List<Pawn> enemies = battle.map.mapPawns.AllPawnsSpawned
                    .Where(p => !p.Dead && p.HostileTo(Faction.OfPlayer)).ToList();
                if (enemies.Count == 0) { battles.Remove(battle); continue; }
                if (tick < battle.nextStrike) continue;
                QueueMobilizedStrike(battle.map, enemies.RandomElement().Position);
                battle.nextStrike = tick + 1800;
            }
        }

        //发射与弹着使用游戏刻，暂停和读档后仍与实际飞行物同步。
        private void TickDelivery(OrbitalDelivery delivery, int tick)
        {
            if (delivery.map == null || !Find.Maps.Contains(delivery.map))
            {
                if (!delivery.dispatched) points += delivery.refundPoints;
                Messages.Message("轨道交付取消：目标地图已不存在。" + (!delivery.dispatched ? "已退还未投送支援的点数。" : ""),
                    DefDatabase<MessageTypeDef>.GetNamed("MihoPhase2_SilentSupport"), false);
                deliveries.Remove(delivery);
            }
            else if (delivery.dispatched)
            {
                ImperialSupportOption option = ImperialSupportOption.Get(delivery.kind);
                if (option.IsBarrage) TickBarrage(delivery, tick, option);
                else if (!delivery.flights.Any(flight => flight?.Spawned == true)) deliveries.Remove(delivery);
            }
            else if (tick >= delivery.dueTick - (ImperialSupportOption.Get(delivery.kind).IsStrike ? Skyfaller_ImperialShell.FlightTicks : 0))
            {
                ImperialSupportOption option = ImperialSupportOption.Get(delivery.kind);
                if (option.IsBarrage)
                {
                    delivery.dispatched = true;
                    TickBarrage(delivery, tick, option);
                }
                else
                {
                    delivery.flights = OrbitalSupport.Deliver(delivery);
                    //空投被安全检查取消时结束此单，不能把空列表登记为已发射。
                    if (delivery.flights.Count == 0) deliveries.Remove(delivery);
                    else delivery.dispatched = true;
                }
            }
        }

        //第一发在到达时间弹着，随后按间隔逐发入射，持续阶段不重复扣费。
        private void TickBarrage(OrbitalDelivery delivery, int tick, ImperialSupportOption option)
        {
            delivery.flights.RemoveAll(flight => flight == null || !flight.Spawned);
            int shotCount = option.durationTicks / option.intervalTicks;
            int launchTick = delivery.dueTick - Skyfaller_ImperialShell.FlightTicks + delivery.shotsFired * option.intervalTicks;
            if (delivery.shotsFired < shotCount && tick >= launchTick)
            {
                delivery.flights.Add(OrbitalStrike.Fire(delivery));
                delivery.shotsFired++;
            }
            if (tick >= delivery.dueTick + option.durationTicks && delivery.flights.Count == 0)
                deliveries.Remove(delivery);
        }
    }
}
