using System;
using ChezhouLib.ObjectPool;
using ChezhouLib.Startup;
using UnityEngine;
using Verse;

namespace MihoAdfqs.Combat.Effects
{
    //复用ChezhouLib材质模板，每次绘制只更新属性块。
    [StaticConstructorOnStartup]
    internal static class CombatVfxMaterials
    {
        private static Material beam;
        private static Material burst;
        private static Material smoke;
        private static MaterialPropertyBlock properties;

        //首次真正需要绘制时，确保Shader与材质已经加载。
        private static void Initialize()
        {
            if (beam != null) return;
            InitiaUnityShaderLord.EnsureInitialized();
            beam = Required("MihoCombat_BeamMaterial");
            burst = Required("MihoCombat_BurstMaterial");
            smoke = Required("MihoCombat_SmokeMaterial");
            properties = new MaterialPropertyBlock();
        }

        //缺少资源时明确报告错误，不以原版Shader静默替代。
        private static Material Required(string name)
        {
            Material material = ClMaterialPool.GetByDefName(name);
            if (material == null) throw new InvalidOperationException("[MihoAdfqs] 战斗Shader材质未加载：" + name);
            return material;
        }

        //沿前进方向绘制光束、曳光或末端渐隐的炮弹拖尾。
        public static void Beam(Vector3 start, Vector3 end, Color color, float width, bool energy, bool flame = false, bool trail = false)
        {
            Vector3 direction = (end - start).Yto0();
            if (direction.sqrMagnitude < .0001f) return;
            Initialize();
            properties.Clear();
            properties.SetColor("_Color", color);
            properties.SetFloat("_Phase", Find.TickManager.TicksGame / 60f);
            properties.SetFloat("_Energy", energy ? 1 : 0);
            properties.SetFloat("_Flame", flame ? 1 : 0);
            properties.SetFloat("_Trail", trail ? 1 : 0);
            Vector3 center = (start + end) * .5f;
            center.y = AltitudeLayer.MoteOverhead.AltitudeFor();
            Graphics.DrawMesh(MeshPool.plane10, Matrix4x4.TRS(center, Quaternion.LookRotation(direction),
                new Vector3(width, 1, direction.magnitude)), beam, 0, null, 0, properties);
        }

        //闪光、冲击环和烟团均使用共享网格，不创建临时GameObject。
        public static void Burst(Vector3 position, float size, Color color, float age, float seed, bool ring = false, bool isSmoke = false)
        {
            Initialize();
            properties.Clear();
            properties.SetColor("_Color", color);
            properties.SetFloat("_Age", age);
            properties.SetFloat("_Seed", seed);
            properties.SetFloat("_Ring", ring ? 1 : 0);
            position.y = (isSmoke ? AltitudeLayer.MoteLow : AltitudeLayer.MoteOverhead).AltitudeFor();
            Graphics.DrawMesh(MeshPool.plane10, Matrix4x4.TRS(position, Quaternion.Euler(0, seed * 57, 0),
                new Vector3(size, 1, size)), isSmoke ? smoke : burst, 0, null, 0, properties);
        }
    }
}
