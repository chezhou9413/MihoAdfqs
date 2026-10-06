using Verse;

namespace MihoAdfqs.Phase2.Orbital
{
    //职责：记录动员模式下一个战场的下一次舰炮支援时刻。
    public class OrbitalBattle : IExposable
    {
        public Map map;
        public int nextStrike;

        //职责：保存战场引用和连续轰炸的时间进度。
        public void ExposeData()
        {
            Scribe_References.Look(ref map, "map");
            Scribe_Values.Look(ref nextStrike, "nextStrike");
        }
    }
}
