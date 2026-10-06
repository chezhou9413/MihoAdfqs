using System.Linq;
using Verse;

namespace MihoAdfqs.Phase2.Backgrounds
{
    //职责：记录死宅背景人物连续离开室内的游戏时间。
    public class Comp_BackgroundExposure : ThingComp
    {
        public int outdoorTicks;
        private int elapsed;

        //职责：每秒检查所在房间，回到室内立即重置连续离室计时。
        public override void CompTickInterval(int delta)
        {
            elapsed += delta;
            if (elapsed < 60) return;
            Pawn pawn = (Pawn)parent;
            if (!BackgroundUtility.Effects(pawn).Any(effect => effect.homebody)) outdoorTicks = 0;
            else if (pawn.Spawned && pawn.GetRoom()?.PsychologicallyOutdoors == false) outdoorTicks = 0;
            else outdoorTicks = System.Math.Min(25001, outdoorTicks + elapsed);
            elapsed = 0;
        }

        //职责：保存离室时间和未结算的秒内计时。
        public override void PostExposeData()
        {
            Scribe_Values.Look(ref outdoorTicks, "backgroundOutdoorTicks");
            Scribe_Values.Look(ref elapsed, "backgroundExposureElapsed");
        }
    }
}
