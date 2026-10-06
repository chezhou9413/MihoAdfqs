using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI.Group;

namespace MihoAdfqs.Phase2.Helmer
{
    //职责：登记赫尔默的非永久死亡、自爆及血液风暴击杀治疗。
    [HarmonyPatch(typeof(Pawn), nameof(Pawn.Kill))]
    public static class Patch_HelmerDeath
    {
        //职责：在原版死亡流程卸下武器和移除队伍前保存恢复所需的身份关联。
        public static void Prefix(Pawn __instance, out HelmerDeathState __state)
        {
            __state = new HelmerDeathState { wasAlive = !__instance.Dead };
            if (__instance.Dead || Gene_Helmer.Get(__instance) == null || GameComponent_HelmerRecovery.Current.Contains(__instance)) return;
            Caravan caravan = __instance.GetCaravan();
            __state.recovery = new HelmerRecovery
            {
                pawn = __instance, map = __instance.MapHeld, cell = __instance.PositionHeld,
                weapon = __instance.equipment?.Primary, lord = __instance.GetLord(), caravan = caravan,
                tile = caravan?.Tile ?? PlanetTile.Invalid, drafted = __instance.Drafted
            };
            GameComponent_HelmerRecovery.Current.Register(__state.recovery);
        }

        //职责：完成实际死亡后触发爆炸，并在下一刻恢复；敌方真实死亡触发攻击者的部位治疗。
        public static void Postfix(Pawn __instance, DamageInfo? dinfo, HelmerDeathState __state)
        {
            if (__state?.wasAlive != true) return;
            HelmerRecovery recovery = __state.recovery;
            if (recovery != null)
            {
                recovery.ready = true;
                recovery.dueTick = Find.TickManager.TicksGame + 1;
                if (__instance.Dead && recovery.map != null)
                {
                    var ignored = new List<Thing> { __instance };
                    if (__instance.Corpse != null) ignored.Add(__instance.Corpse);
                    if (recovery.weapon != null) ignored.Add(recovery.weapon);
                    GenExplosion.DoExplosion(recovery.cell, recovery.map, 3.9f, DamageDefOf.Bomb, __instance,
                        100, 1.5f, ignoredThings: ignored);
                }
            }
            Pawn attacker = dinfo?.Instigator as Pawn;
            if (!__instance.Dead || attacker == null || attacker == __instance || attacker.Dead || !__instance.HostileTo(attacker)
                || Gene_Helmer.Get(attacker)?.storm != true) return;
            var injuries = attacker.health.hediffSet.hediffs.OfType<Hediff_Injury>().Where(h => h.Part != null).ToList();
            if (injuries.Count == 0) return;
            BodyPartRecord part = injuries.Select(h => h.Part).Distinct().RandomElement();
            foreach (Hediff_Injury injury in injuries.Where(h => h.Part == part)) injury.Heal(injury.Severity);
        }
    }
}
