using RimWorld.Planet;
using Verse;
using Verse.AI.Group;

namespace MihoAdfqs.Phase2.Helmer
{
    //职责：记录死亡前的位置、武器、领队和远行队，以便原身份恢复。
    public class HelmerRecovery : IExposable
    {
        public Pawn pawn;
        public Map map;
        public IntVec3 cell;
        public ThingWithComps weapon;
        public Lord lord;
        public Caravan caravan;
        public PlanetTile tile = PlanetTile.Invalid;
        public bool drafted;
        public bool ready;
        public int dueTick;

        //职责：保存死亡待恢复数据，允许在恢复等待期正常存读档。
        public void ExposeData()
        {
            Scribe_References.Look(ref pawn, "pawn");
            Scribe_References.Look(ref map, "map");
            Scribe_Values.Look(ref cell, "cell");
            Scribe_References.Look(ref weapon, "weapon");
            Scribe_References.Look(ref lord, "lord");
            Scribe_References.Look(ref caravan, "caravan");
            Scribe_Values.Look(ref tile, "tile", PlanetTile.Invalid);
            Scribe_Values.Look(ref drafted, "drafted");
            Scribe_Values.Look(ref ready, "ready");
            Scribe_Values.Look(ref dueTick, "dueTick");
        }
    }
}
