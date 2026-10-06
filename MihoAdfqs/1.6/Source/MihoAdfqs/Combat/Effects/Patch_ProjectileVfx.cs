using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace MihoAdfqs.Combat.Effects
{
    //按弹丸配置绘制实体弹头、曳光和持续激光。
    [HarmonyPatch(typeof(Projectile), "DrawAt")]
    public static class Patch_ProjectileCombatDraw
    {
        //弹体与能量核心保留各自贴图，拖尾裁到枪口之后并避开弹体。
        public static bool Prefix(Projectile __instance, Vector3 drawLoc, Vector3 ___origin)
        {
            CombatVisuals settings = __instance.def.GetModExtension<CombatVisuals>();
            if (settings == null) return true;
            if (settings.beam) return false;
            if (settings.tracerWidth <= 0) return true;
            Vector3 direction = __instance.ExactRotation * Vector3.forward;
            float travelled = Mathf.Max(0f, Vector3.Dot((drawLoc - ___origin).Yto0(), direction));
            float length = Mathf.Min(settings.tracerLength, travelled);
            if (settings.physicalProjectile)
            {
                if (__instance.def.projectile.arcHeightFactor > 0f) return true;
                if (settings.energy)
                {
                    CombatVfxMaterials.Beam(drawLoc - direction * length, drawLoc, settings.color,
                        settings.tracerWidth, true, trail: true);
                    float pulse = .85f + .15f * Mathf.Sin(Find.TickManager.TicksGame * .3f + __instance.thingIDNumber);
                    Color halo = settings.color;
                    halo.a *= .45f;
                    CombatVfxMaterials.Burst(drawLoc, settings.tracerWidth * 4f * pulse, halo, .1f,
                        __instance.thingIDNumber % 97);
                }
                else if (settings.rocketExhaust)
                {
                    float offset = __instance.def.graphicData.drawSize.y * .35f;
                    Vector3 nozzle = drawLoc - direction * offset;
                    float exhaustLength = Mathf.Min(settings.tracerLength, Mathf.Max(0f, travelled - offset));
                    Color exhaust = settings.color;
                    exhaust.a *= .45f;
                    CombatVfxMaterials.Beam(nozzle - direction * Mathf.Min(1.3f, exhaustLength), nozzle, exhaust,
                        settings.tracerWidth, true, trail: true);
                    //尾焰保持短促，后方用透明烟团，避免整条火箭轨迹看起来像激光。
                    if (exhaustLength > 1f)
                        for (int i = 1; i <= 3; i++)
                            CombatVfxMaterials.Burst(nozzle - direction * (exhaustLength * i / 4f),
                                Mathf.Min(.4f + i * .18f, exhaustLength * .45f), new Color(.28f, .25f, .21f, .5f),
                                i * .12f, __instance.thingIDNumber + i * 13, isSmoke: true);
                }
                else
                    CombatVfxMaterials.Beam(drawLoc - direction * length, drawLoc,
                        new Color(.55f, .52f, .48f, .16f), settings.tracerWidth, false, trail: true);
                return true;
            }
            CombatVfxMaterials.Beam(drawLoc - direction * length, drawLoc, settings.color, settings.tracerWidth, settings.energy);
            return false;
        }
    }

    //普通子弹命中保留原版伤害，只追加火花。
    [HarmonyPatch(typeof(Bullet), "Impact")]
    public static class Patch_BulletCombatImpact
    {
        //连续激光由光束绘制接触效果，不叠加隐藏弹丸在随机格内落点产生的闪光。
        public static void Prefix(Bullet __instance, bool blockedByShield)
        {
            CombatVisuals settings = __instance.def.GetModExtension<CombatVisuals>();
            if (settings == null || blockedByShield || settings.beam) return;
            __instance.Map.GetComponent<MapComponent_CombatVfx>().Impact(__instance.ExactPosition, settings,
                __instance.DamageDef == DamageDefOf.Bomb);
        }
    }

    //圆形爆炸仍由原版结算，额外绘制冲击环和烟尘。
    [HarmonyPatch(typeof(Projectile_Explosive), "Explode")]
    public static class Patch_ExplosiveCombatImpact
    {
        //原版已经播放soundExplode，因此此处不再叠加相同爆炸声音。
        public static void Prefix(Projectile_Explosive __instance)
        {
            CombatVisuals settings = __instance.def.GetModExtension<CombatVisuals>();
            if (settings == null) return;
            __instance.Map.GetComponent<MapComponent_CombatVfx>().Impact(__instance.ExactPosition, settings, true);
        }
    }
}
