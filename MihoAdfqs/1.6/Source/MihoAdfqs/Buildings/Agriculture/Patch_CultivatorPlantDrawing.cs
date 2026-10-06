using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace MihoAdfqs.Buildings.Agriculture
{
    //将培养机内的原版植物网格收拢到中央玻璃窗口，保留实际种植位置和产量。
    [HarmonyPatch(typeof(Plant), nameof(Plant.Print))]
    public static class Patch_CultivatorPlantDrawing
    {
        //记录本株植物开始绘制时已有的网格顶点数量。
        public struct PrintState
        {
            public Building_IndoorCultivator cultivator;
            public int[] vertexCounts;
        }

        //只在重建地图网格时记录相关植物的写入起点，普通植物不分配状态数组。
        public static void Prefix(Plant __instance, SectionLayer layer, out PrintState __state)
        {
            __state = default;
            if (!__instance.Spawned || !(__instance.Position.GetEdifice(__instance.Map) is Building_IndoorCultivator cultivator)) return;
            __state.cultivator = cultivator;
            __state.vertexCounts = new int[layer.subMeshes.Count];
            for (int i = 0; i < layer.subMeshes.Count; i++)
                __state.vertexCounts[i] = layer.subMeshes[i].verts.Count;
        }

        //只调整本株植物刚写入的顶点，继续使用原版分丛、成长、风摆及雪层。
        public static void Postfix(Plant __instance, SectionLayer layer, PrintState __state)
        {
            if (__state.cultivator == null) return;
            Building_IndoorCultivator cultivator = __state.cultivator;
            Vector3 source = __instance.TrueCenter();
            Vector3 center = cultivator.TrueCenter();
            Vector3 window = center + cultivator.def.graphicData.DrawOffsetForRot(cultivator.Rotation)
                + new Vector3(-0.08f, 0f, 0.53f);
            Vector3 destination = window + new Vector3((source.x - center.x) * 0.4f, 0f,
                (source.z - center.z) * 0.3f);
            float altitude = cultivator.def.Altitude + Altitudes.AltInc;
            for (int i = 0; i < layer.subMeshes.Count; i++)
            {
                LayerSubMesh mesh = layer.subMeshes[i];
                int first = i < __state.vertexCounts.Length ? __state.vertexCounts[i] : 0;
                for (int j = first; j < mesh.verts.Count; j++)
                {
                    Vector3 vertex = mesh.verts[j];
                    vertex.x = destination.x + (vertex.x - source.x) * 0.4f;
                    vertex.z = destination.z + (vertex.z - source.z) * 0.4f;
                    vertex.y = altitude + (vertex.y - __instance.def.Altitude) * 0.1f;
                    mesh.verts[j] = vertex;
                }
            }
        }
    }
}
