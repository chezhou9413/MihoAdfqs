using System.Runtime.CompilerServices;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace MihoAdfqs.Combat.Muzzles
{
    //同时限制粒子的连接端点与贴片尺寸，火焰不得越过枪口向后延伸。
    [HarmonyPatch(typeof(IncineratorProjectileMotion), nameof(IncineratorProjectileMotion.Tick))]
    public static class Patch_IncineratorMoteMotion
    {
        private static readonly ConditionalWeakTable<MoteDualAttached, object> Motes =
            new ConditionalWeakTable<MoteDualAttached, object>();
        private static readonly object Marker = new object();

        //粒子出生时两端都落在枪口，弱引用标记随粒子回收而释放。
        internal static void Register(IncineratorProjectileMotion motion, Map map)
        {
            Motes.Add(motion.mote, Marker);
            IntVec3 cell = motion.worldSource.ToIntVec3();
            Vector3 offset = motion.worldSource - cell.ToVector3Shifted();
            var target = new TargetInfo(cell, map);
            motion.mote.UpdateTargets(target, target, offset, offset);
            //原版图形有10格基础尺寸，出生帧尚未飞行时必须将整张贴片收为零。
            motion.mote.linearScale = new Vector3(0, 1, 0);
        }

        //原版本刻先增加粒子年龄，再向后绘制两格；尾巴最多覆盖已飞过的距离。
        public static void Prefix(ref IncineratorProjectileMotion __instance, out Vector4 __state)
        {
            Vector3 direction = __instance.moveVector;
            __state = new Vector4(direction.x, direction.y, direction.z, -1);
            if (!Motes.TryGetValue(__instance.mote, out _)) return;
            float progress = Mathf.Clamp01((float)(__instance.ticks + 1) / __instance.lifespanTicks);
            float travelled = Vector3.Distance(__instance.worldSource, __instance.worldTarget) * progress;
            __state.w = Mathf.Min(2f, travelled);
            __instance.moveVector *= __state.w * .5f;
        }

        //原版图形还会乘drawSize，将纵向尺寸换算为两端距离，宽度随飞行逐渐展开。
        public static void Postfix(ref IncineratorProjectileMotion __instance, Vector4 __state)
        {
            if (__state.w < 0) return;
            __instance.moveVector = new Vector3(__state.x, __state.y, __state.z);
            MoteDualAttached mote = __instance.mote;
            float width = Mathf.Min(__state.w, (.18f + __instance.Alpha * .85f) * mote.linearScale.x);
            Vector2 drawSize = mote.def.graphicData.drawSize;
            mote.linearScale = new Vector3(width / drawSize.x, 1, __state.w / drawSize.y);
        }
    }
}
