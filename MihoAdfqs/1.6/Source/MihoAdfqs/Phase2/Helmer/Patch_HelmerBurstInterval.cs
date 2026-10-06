using System;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace MihoAdfqs.Phase2.Helmer
{
    //职责：在原版缓存读取后修正连发间隔，使姿态切换立即作用于后续弹丸。
    [HarmonyPatch]
    public static class Patch_HelmerBurstInterval
    {
        //职责：定位以游戏刻表示的连发间隔属性。
        public static MethodBase TargetMethod() => AccessTools.PropertyGetter(typeof(Verb), nameof(Verb.TicksBetweenBurstShots));

        //职责：将姿态倍率换算成至少一刻的实际射击间隔。
        public static void Postfix(Verb __instance, ref int __result)
        {
            if (__instance.verbProps.IsMeleeAttack || __instance.EquipmentSource == null) return;
            if (__instance is Combat.Weapons.Verb_Magazine magazine)
            {
                __result = magazine.NextShotTicks;
                return;
            }
            __result = Math.Max(1, Mathf.RoundToInt(__result * Patch_HelmerShootingTime.Factor(__instance)
                * Backgrounds.BackgroundUtility.ShotTime(__instance.CasterPawn)
                * Medicine.InjectorUtility.ShotTime(__instance.CasterPawn)
                * Combat.Weapons.WeaponTiming.ShotFactor(__instance.CasterPawn)));
        }
    }
}
