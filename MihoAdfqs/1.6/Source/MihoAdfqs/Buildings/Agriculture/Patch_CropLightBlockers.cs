using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Verse;

namespace MihoAdfqs.Buildings.Agriculture
{
    //在灯源范围内遮光物变化后标记方形灯光需要更新。
    [HarmonyPatch]
    public static class Patch_CropLightBlockers
    {
        //定位原版新增和移除遮光物的通知。
        public static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.Method(typeof(GlowGrid), nameof(GlowGrid.LightBlockerAdded));
            yield return AccessTools.Method(typeof(GlowGrid), nameof(GlowGrid.LightBlockerRemoved));
        }

        //连续墙体变化只累计一次更新请求。
        public static void Postfix(Map ___map, IntVec3 cell) => ___map.GetComponent<MapComponent_CropLight>().BlockerChanged(cell);
    }
}
