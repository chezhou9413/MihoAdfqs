using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI.Group;

namespace MihoAdfqs.Phase2.Orbital
{
    //记录调度时间与实际飞行物，使地图标记持续到落地。
    public class OrbitalDelivery : IExposable
    {
        public Map map;
        public IntVec3 cell;
        public string kind;
        public int dueTick;
        public int refundPoints;
        public int orderedTick;
        public int soldierCount = 4;
        public int shotsFired;
        public bool dispatched;
        public bool includeCaptain;
        public Faction faction;
        public Pawn caller;
        public Lord sourceLord;
        public bool npcCall;
        public List<IntVec3> landingCells;
        public List<Skyfaller> flights = new List<Skyfaller>();
        public bool BarrageActive => ImperialSupportOption.Get(kind).IsBarrage && Find.TickManager.TicksGame >= dueTick;
        public int BarrageTicksLeft => System.Math.Max(0, dueTick + ImperialSupportOption.Get(kind).durationTicks - Find.TickManager.TicksGame);

        //实际进入空降阶段后，使用飞行物的剩余刻计算到达时间。
        public int ArrivalTicks
        {
            get
            {
                int remaining = 0;
                ImperialSupportOption option = ImperialSupportOption.Get(kind);
                if (option.IsBarrage) return System.Math.Max(0, dueTick - Find.TickManager.TicksGame);
                if (dispatched)
                {
                    foreach (Skyfaller flight in flights)
                        if (flight?.Spawned == true) remaining = System.Math.Max(remaining, flight.ticksToImpact);
                    return remaining;
                }
                return System.Math.Max(0, dueTick - Find.TickManager.TicksGame) + (option.IsStrike ? 0 : 120);
            }
        }

        //保存已付款指令与尚未落地的飞行物引用。
        public void ExposeData()
        {
            Scribe_References.Look(ref map, "map");
            Scribe_Values.Look(ref cell, "cell");
            Scribe_Values.Look(ref kind, "kind");
            Scribe_Values.Look(ref dueTick, "dueTick");
            Scribe_Values.Look(ref refundPoints, "refundPoints");
            Scribe_Values.Look(ref orderedTick, "orderedTick");
            Scribe_Values.Look(ref soldierCount, "soldierCount", 4);
            Scribe_Values.Look(ref shotsFired, "shotsFired");
            Scribe_Values.Look(ref dispatched, "dispatched");
            Scribe_Values.Look(ref includeCaptain, "includeCaptain");
            Scribe_References.Look(ref faction, "faction");
            Scribe_References.Look(ref caller, "caller", saveDestroyedThings: true);
            if (Scribe.mode == LoadSaveMode.Saving && sourceLord != null
                && (map == null || !map.lordManager.lords.Contains(sourceLord))) sourceLord = null;
            Scribe_References.Look(ref sourceLord, "sourceLord");
            Scribe_Values.Look(ref npcCall, "npcCall");
            Scribe_Collections.Look(ref landingCells, "landingCells", LookMode.Value);
            Scribe_Collections.Look(ref flights, "flights", LookMode.Reference);
        }
    }
}
