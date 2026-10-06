using RimWorld;
using Verse;
using MihoAdfqs.Phase2.Orbital;

namespace MihoAdfqs.Phase2.Quests
{
    //职责：集中结算三类委托的实物奖励和帝国点数。
    public static class ImperialMissionRewards
    {
        //职责：在已完成任务的目标殖民地空投对应奖励。
        public static void Grant(string kind, Map map)
        {
            switch (kind)
            {
                case "Social":
                    OrbitalSupport.DropGoods(map, new[] { OrbitalSupport.Stack("MihoPhase2_IndustrialSteelPlate", Rand.RangeInclusive(200, 440)) });
                    GameComponent_OrbitalNetwork.Current.AwardPoints(20);
                    break;
                case "Rescue":
                    OrbitalSupport.DropGoods(map, new[] { OrbitalSupport.Stack("MihoPhase2_Polyethylene", Rand.RangeInclusive(400, 800)), OrbitalSupport.Stack("Silver", 1000) });
                    GameComponent_OrbitalNetwork.Current.AwardPoints(40);
                    break;
                case "Recovery":
                    OrbitalSupport.DropGoods(map, new[] { OrbitalSupport.Stack("MihoPhase2_IndustrialSteelPlate", Rand.RangeInclusive(200, 500)), OrbitalSupport.Stack("Gold", Rand.RangeInclusive(100, 299)) });
                    GameComponent_OrbitalNetwork.Current.AwardPoints(10);
                    break;
                default: throw new System.InvalidOperationException("未知帝国委托奖励：" + kind);
            }
        }
    }
}
