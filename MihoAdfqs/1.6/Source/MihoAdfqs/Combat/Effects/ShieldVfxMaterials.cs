using System;
using ChezhouLib.ObjectPool;
using ChezhouLib.Startup;
using UnityEngine;
using Verse;

namespace MihoAdfqs.Combat.Effects
{
    //球壳与碎片共享材质和属性块，连续受击时不创建材质或场景物体。
    [StaticConstructorOnStartup]
    internal static class ShieldVfxMaterials
    {
        private static Material material;
        private static MaterialPropertyBlock properties;

        //通过现有资源包获取护盾材质，缺失时明确报告资源名。
        private static void Initialize()
        {
            if (material != null) return;
            InitiaUnityShaderLord.EnsureInitialized();
            material = ClMaterialPool.GetByDefName("MihoCombat_ShieldMaterial");
            if (material == null) throw new InvalidOperationException("[MihoAdfqs] 护盾材质未加载：MihoCombat_ShieldMaterial");
            properties = new MaterialPropertyBlock();
        }

        //共享薄片球壳只提交一次绘制，受击与充能服从游戏时间。
        public static void Shell(Vector3 center, float size, Color color, Vector4 state, Vector4 events, Vector4[] hits)
        {
            Initialize();
            properties.Clear();
            properties.SetColor("_Color", color);
            properties.SetFloat("_Phase", Find.TickManager.TicksGame / 60f);
            properties.SetVector("_State", state);
            properties.SetVector("_Event", events);
            properties.SetVectorArray("_Hits", hits);
            center.y = AltitudeLayer.MoteOverhead.AltitudeFor();
            Graphics.DrawMesh(ShieldVfxMeshes.Shell, Matrix4x4.TRS(center, Quaternion.identity,
                Vector3.one * size), material, 0, null, 0, properties);
        }

        //整组碎片共享一次绘制，GPU按各片种子计算三维翻转与飞散。
        public static void Shatter(Vector3 center, float size, Color color, float age, float seed)
        {
            Initialize();
            properties.Clear();
            properties.SetColor("_Color", color);
            properties.SetFloat("_Fragment", 1);
            properties.SetFloat("_Age", age);
            properties.SetVector("_Event", new Vector4(0, 0, seed, 0));
            center.y = AltitudeLayer.MoteOverhead.AltitudeFor();
            Graphics.DrawMesh(ShieldVfxMeshes.Shards, Matrix4x4.TRS(center, Quaternion.identity,
                Vector3.one * size), material, 0, null, 0, properties);
        }
    }
}
