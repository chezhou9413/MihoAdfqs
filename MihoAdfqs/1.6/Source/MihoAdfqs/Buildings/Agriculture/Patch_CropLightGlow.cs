using HarmonyLib;
using Unity.Collections;
using UnityEngine;
using Verse;

namespace MihoAdfqs.Buildings.Agriculture
{
    //在原版光照计算完成后写入农用照明，避免拦截每一次格子光照查询。
    [HarmonyPatch(typeof(GlowGrid), nameof(GlowGrid.GlowGridUpdate_First))]
    public static class Patch_CropLightGlow
    {
        //记录本次是否会重算光照数组；原版结束后会清除脏标记。
        public static void Prefix(Map ___map, ref bool ___anyDirtyCell, bool ___hasRunInitially, out bool __state)
        {
            ___map.GetComponent<MapComponent_CropLight>().FlushPending();
            __state = ___anyDirtyCell || !___hasRunInitially;
        }

        //原版两个Job均已Complete，覆盖两套结果后供后续读取直接使用。
        public static void Postfix(Map ___map, NativeArray<Color32> ___accumulatedGlow,
            NativeArray<Color32> ___accumulatedGlowNoCavePlants, bool __state)
        {
            if (!__state || !___accumulatedGlow.IsCreated || !___accumulatedGlowNoCavePlants.IsCreated) return;
            Color32 full = new Color32(255, 255, 255, 1);
            foreach (int index in ___map.GetComponent<MapComponent_CropLight>().LitIndices)
            {
                ___accumulatedGlow[index] = full;
                ___accumulatedGlowNoCavePlants[index] = full;
            }
        }
    }
}
