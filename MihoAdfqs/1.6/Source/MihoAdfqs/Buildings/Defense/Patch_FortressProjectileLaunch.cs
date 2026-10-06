using HarmonyLib;
using UnityEngine;
using Verse;

namespace MihoAdfqs.Buildings.Defense
{
    //堡垒保留原版命中、掩体和偏射计算，只校正炮弹的发射位置。
    [HarmonyPatch(typeof(Projectile), nameof(Projectile.Launch), new[] { typeof(Thing), typeof(Vector3),
        typeof(LocalTargetInfo), typeof(LocalTargetInfo), typeof(ProjectileHitFlags), typeof(bool),
        typeof(Thing), typeof(ThingDef) })]
    public static class Patch_FortressProjectileLaunch
    {
        //当前炮型的实体射弹与绘制炮口使用同一坐标。
        public static void Prefix(Thing launcher, ref Vector3 origin, Thing equipment)
        {
            if (launcher is Building_ImperialFortress fortress && equipment == fortress.gun)
                origin = fortress.MuzzlePosition;
        }
    }
}
