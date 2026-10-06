using System.Collections.Generic;
using RimWorld;
using RimWorld.QuestGen;
using Verse;

namespace MihoAdfqs.Phase2.Quests
{
    //职责：创建使用原版装载界面的帝国任务穿梭机。
    public static class ImperialShuttle
    {
        //职责：取消时让仍在下降中的穿梭机先完成落地，再卸货离开。
        public static void Cleanup(TransportShip ship, bool unload)
        {
            if (ship == null || ship.Disposed) return;
            if (ship.curJob is ShipJob_Arrive && !ship.ShipExistsAndIsSpawned)
            {
                ship.SetNextJob(ShipJobMaker.MakeShipJob(ShipJobDefOf.FlyAway));
                if (unload) ship.SetNextJob(ShipJobMaker.MakeShipJob(ShipJobDefOf.Unload));
                return;
            }
            SendTransportShipAwayUtility.SendTransportShipAway(ship, unload);
        }

        //职责：指定必需货物或人物，落地后等待玩家装载并发送。
        public static TransportShip Receive(Map map, string signalTag, IEnumerable<ThingDefCount> items = null, Pawn pawn = null)
        {
            Thing shuttle = ThingMaker.MakeThing(ThingDefOf.Shuttle);
            shuttle.SetFaction(Faction.OfPlayer);
            shuttle.questTags = new List<string> { signalTag };
            CompShuttle comp = shuttle.TryGetComp<CompShuttle>();
            if (items != null) comp.requiredItems.AddRange(items);
            if (pawn != null) comp.requiredPawns.Add(pawn);
            comp.acceptColonists = pawn != null;
            comp.onlyAcceptColonists = pawn != null;
            comp.maxColonistCount = pawn == null ? 0 : 1;
            TransportShip ship = TransportShipMaker.MakeTransportShip(TransportShipDefOf.Ship_Shuttle, null, shuttle);
            ship.ArriveAt(IntVec3.Invalid, map.Parent);
            ship.AddJob(ShipJobDefOf.WaitForever);
            return ship;
        }
    }
}
