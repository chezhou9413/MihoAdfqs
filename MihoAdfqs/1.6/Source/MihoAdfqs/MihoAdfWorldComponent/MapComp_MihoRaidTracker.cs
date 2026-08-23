using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MihoAdfqs.MihoAdfWorldComponent
{
    public class MapComp_MihoRaidTracker : WorldComponent
    {
        // 存储每次袭击发生的 tick 时间
        public List<int> raidTimestamps = new List<int>();

        public MapComp_MihoRaidTracker(World world) : base(world)
        {
        }

        // 提供一个静态方法方便从任何地方记录袭击
        public static void RegisterRaid()
        {
            var component = Find.World.GetComponent<MapComp_MihoRaidTracker>();
            if (component != null)
            {
                component.raidTimestamps.Add(Find.TickManager.TicksGame);
                int cutoff = Find.TickManager.TicksGame - (60 * 60000);
                component.raidTimestamps.RemoveAll(t => t < cutoff);
            }
        }

        // 核心功能：计算过去 days 天内发生了多少次袭击
        public int GetRaidsCountInLastDays(float days)
        {
            int ticksThreshold = (int)(days * 60000f);
            int cutoffTime = Find.TickManager.TicksGame - ticksThreshold;

            int count = 0;
            for (int i = 0; i < raidTimestamps.Count; i++)
            {
                if (raidTimestamps[i] >= cutoffTime)
                {
                    count++;
                }
            }
            return count;
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref raidTimestamps, "mihoRaidTimestamps", LookMode.Value);
            if (raidTimestamps == null) raidTimestamps = new List<int>();
        }
    }
}
