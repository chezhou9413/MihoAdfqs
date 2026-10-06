using System.Collections.Generic;
using MihoAdfqs.Phase2.Helmer;
using RimWorld;
using Verse;

namespace MihoAdfqs.Phase2.CombatAI
{
    //统一调度军队战备和避险，不向所有种族添加逐刻人物组件。
    public sealed class MapComponent_ImperialCombat : MapComponent
    {
        private static readonly HashSet<string> MilitaryKinds = new HashSet<string>
        {
            "MihoPhase2_DefenseSoldier", "MihoPhase2_DefenseNCO", "MihoPhase2_SpecialForcesSoldier",
            "MihoPhase2_OrbitalDropTrooper", "MihoPhase2_LeaderGuard", "MihoPhase2_LeaderGuardCaptain",
            "MihoPhase2_StratagemSupportSoldier"
        };
        private List<ImperialCombatPawnState> states = new List<ImperialCombatPawnState>();
        private readonly Dictionary<Pawn, ImperialCombatPawnState> byPawn = new Dictionary<Pawn, ImperialCombatPawnState>();
        private readonly List<Pawn> army = new List<Pawn>();
        private Dictionary<Faction, int> nextFactionCall = new Dictionary<Faction, int>();
        private List<Faction> savedFactions;
        private List<int> savedTicks;
        internal readonly ImperialCombatThreats Threats;

        //威胁快照仅属于当前地图。
        public MapComponent_ImperialCombat(Map map) : base(map) { Threats = new ImperialCombatThreats(map); }

        //普通军事兵种与拥有赫尔默基因的人物都可进入轨道避险流程。
        internal static bool Military(Pawn pawn) => MilitaryKinds.Contains(pawn.kindDef.defName);

        //首次出现人物时创建一次状态，翻滚落地不重置额度。
        internal ImperialCombatPawnState State(Pawn pawn)
        {
            if (!byPawn.TryGetValue(pawn, out ImperialCombatPawnState state))
            {
                state = new ImperialCombatPawnState { pawn = pawn };
                states.Add(state); byPawn.Add(pawn, state);
            }
            return state;
        }

        //同一派系的支援兵共用十秒间隔，敌对测试派系各自计时。
        internal bool FactionReady(Faction faction, int tick) => !nextFactionCall.TryGetValue(faction, out int next) || tick >= next;
        internal void Called(Faction faction, int tick) { nextFactionCall[faction] = tick + 600; }

        //六刻一次快照；战备决策按人物散列分散到每秒。
        public override void MapComponentTick()
        {
            int tick = Find.TickManager.TicksGame;
            if (tick % 6 != 0) return;
            army.Clear();
            var spawned = map.mapPawns.AllPawnsSpawned;
            for (int i = 0; i < spawned.Count; i++)
                if (Military(spawned[i]) || Gene_Helmer.Get(spawned[i]) != null) army.Add(spawned[i]);
            if (army.Count == 0) return;
            Threats.Refresh();
            foreach (Pawn pawn in army)
            {
                if (!pawn.Spawned || pawn.Dead || pawn.Downed || pawn.InMentalState) continue;
                ImperialCombatPawnState state = State(pawn);
                if (ImperialEvasion.Update(pawn, state, this)) continue;
                if (pawn.kindDef.defName == "MihoPhase2_StratagemSupportSoldier"
                    && (tick / 6 + pawn.thingIDNumber) % 10 == 0)
                    ImperialSupportAI.Update(pawn, state, this);
            }
            if (tick % 600 != 0) return;
            for (int i = states.Count - 1; i >= 0; i--)
            {
                ImperialCombatPawnState state = states[i];
                if (state.pawn != null && !state.pawn.Discarded) continue;
                states.RemoveAt(i);
                if (state.pawn != null) byPawn.Remove(state.pawn);
            }
        }

        //保存人物额度、翻滚冷却和派系节流；索引和威胁快照不写入存档。
        public override void ExposeData()
        {
            Scribe_Collections.Look(ref states, "imperialCombatStates", LookMode.Deep);
            Scribe_Collections.Look(ref nextFactionCall, "nextFactionCall", LookMode.Reference, LookMode.Value,
                ref savedFactions, ref savedTicks);
            if (Scribe.mode != LoadSaveMode.PostLoadInit) return;
            byPawn.Clear();
            states.RemoveAll(s => s.pawn == null || s.pawn.Discarded);
            foreach (ImperialCombatPawnState state in states) byPawn.Add(state.pawn, state);
        }
    }
}
