using System.Linq;
using RimWorld;
using Verse;

namespace MihoAdfqs.Phase2.Backgrounds
{
    //职责：维护指挥官周围十五格内友军的高效指挥效果。
    public class MapComponent_CommandAura : MapComponent
    {
        //职责：绑定光环所属地图。
        public MapComponent_CommandAura(Map map) : base(map) { }

        //职责：每秒刷新一次友军光环，失去指挥来源后由短时效果自动移除。
        public override void MapComponentTick()
        {
            if (Find.TickManager.TicksGame % 60 != 0) return;
            var commanders = map.mapPawns.AllPawnsSpawned.Where(pawn => !pawn.Dead && !pawn.Downed && pawn.Faction != null
                && BackgroundUtility.Effects(pawn).Any(effect => effect.commander)).ToList();
            if (commanders.Count == 0) return;
            HediffDef aura = DefDatabase<HediffDef>.GetNamed("MihoPhase2_CommandAura");
            foreach (Pawn pawn in map.mapPawns.AllPawnsSpawned)
            {
                if (pawn.Dead || pawn.Faction == null || !commanders.Any(source => source.Position.DistanceToSquared(pawn.Position) <= 225
                    && (source.Faction == pawn.Faction || source.Faction.RelationKindWith(pawn.Faction) == FactionRelationKind.Ally))) continue;
                Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(aura) ?? pawn.health.AddHediff(aura);
                hediff.TryGetComp<HediffComp_Disappears>().ticksToDisappear = 120;
            }
        }
    }
}
