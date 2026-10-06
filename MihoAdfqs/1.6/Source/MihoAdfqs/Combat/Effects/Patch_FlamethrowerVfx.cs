using HarmonyLib;
using Verse;

namespace MihoAdfqs.Combat.Effects
{
    //为本模组喷火器叠加流动火焰锥，保留原版燃烧与伤害。
    [HarmonyPatch(typeof(Verb_ArcSprayIncinerator), "TryCastShot")]
    public static class Patch_FlamethrowerVfx
    {
        //只在成功喷射时启动视觉，不改动其它模组的喷火器。
        public static void Postfix(Verb_ArcSprayIncinerator __instance, bool __result)
        {
            if (__result && __instance.EquipmentSource?.def.defName == "Weapon_BeamFlamethrower")
                __instance.Caster.Map.GetComponent<MapComponent_CombatVfx>().Flame(__instance);
        }
    }
}
