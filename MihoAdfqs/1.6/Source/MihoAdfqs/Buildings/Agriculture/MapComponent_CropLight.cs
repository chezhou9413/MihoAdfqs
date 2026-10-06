using System.Collections.Generic;
using RimWorld;
using Verse;

namespace MihoAdfqs.Buildings.Agriculture
{
    //按地图格缓存农用灯覆盖，供植物生长与地面视觉共享同一照明结果。
    public class MapComponent_CropLight : MapComponent
    {
        private readonly HashSet<Comp_CropLight> sources = new HashSet<Comp_CropLight>();
        private HashSet<int> lit = new HashSet<int>();
        private HashSet<int> next = new HashSet<int>();
        private bool dirty;

        //绑定照明缓存所属地图。
        public MapComponent_CropLight(Map map) : base(map) { }

        //登记生成的照明建筑。
        public void Register(Comp_CropLight source) { sources.Add(source); RequestRefresh(); }

        //移除离图建筑并保留其它重叠灯源。
        public void Remove(Comp_CropLight source) { sources.Remove(source); RequestRefresh(); }

        //只在原版光照重算时遍历已覆盖格子。
        public HashSet<int> LitIndices => lit;

        //同一轮建造、读档和供电变化合并到原版光照更新前处理。
        public void RequestRefresh() { if (sources.Count > 0 || lit.Count > 0) dirty = true; }

        //覆盖范围之外的遮挡变化不会影响灯源视线。
        public void BlockerChanged(IntVec3 cell)
        {
            if (dirty) return;
            foreach (Comp_CropLight source in sources)
                if (!source.Props.occupiedOnly && source.Area.Contains(cell)) { dirty = true; return; }
        }

        //在原版Job启动前重算覆盖，只标脏新增或移除的格子。
        public void FlushPending()
        {
            if (!dirty) return;
            dirty = false;
            next.Clear();
            foreach (Comp_CropLight source in sources)
            {
                if (!source.Active) continue;
                foreach (IntVec3 cell in source.Area.ClipInsideMap(map))
                    if (source.Props.occupiedOnly || GenSight.LineOfSight(source.parent.Position, cell, map, skipFirstCell: true))
                        next.Add(map.cellIndices.CellToIndex(cell));
            }
            HashSet<int> changed = lit;
            lit = next;
            changed.SymmetricExceptWith(next);
            foreach (int index in changed) map.glowGrid.DirtyCell(map.cellIndices.IndexToCell(index));
            next = changed;
            next.Clear();
        }
    }
}
