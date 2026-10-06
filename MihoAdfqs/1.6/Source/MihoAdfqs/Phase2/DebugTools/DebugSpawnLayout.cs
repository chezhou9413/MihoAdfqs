using System.Collections.Generic;
using System.Linq;
using Verse;

namespace MihoAdfqs.Phase2.DebugTools
{
    //职责：为批量生成的测试样本分配独立空格，避免覆盖地图上的人物、建筑和物品。
    internal static class DebugSpawnLayout
    {
        //职责：从点击位置向外收集已揭开的可站立空格，按行排列供样本依次占用。
        public static List<IntVec3> FindCells(Map map, IntVec3 center, int count)
        {
            return GenRadial.RadialCellsAround(center, 30f, true)
                .Where(cell => cell.InBounds(map) && !cell.Fogged(map) && cell.Standable(map)
                    && cell.GetEdifice(map) == null && cell.GetFirstPawn(map) == null
                    && cell.GetFirstItem(map) == null)
                .Take(count)
                .OrderByDescending(cell => cell.z).ThenBy(cell => cell.x)
                .ToList();
        }
    }
}
