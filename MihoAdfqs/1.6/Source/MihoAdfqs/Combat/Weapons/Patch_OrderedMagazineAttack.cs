using HarmonyLib;
using RimWorld;
using Verse;

namespace MihoAdfqs.Combat.Weapons
{
    //远程攻击等待当前枪管装填，不因暂时不可发射改用近战。
    [HarmonyPatch(typeof(Pawn), nameof(Pawn.TryGetAttackVerb))]
    public static class Patch_OrderedMagazineAttack
    {
        //玩家指令保留当前枪管；敌人和友军换弹时也继续使用远程攻击流程。
        public static void Postfix(Pawn __instance, bool allowManualCastWeapons, ref Verb __result)
        {
            if (!(__instance.equipment?.PrimaryEq?.PrimaryVerb is Verb_Magazine magazine) || !magazine.Selected) return;
            bool ordered = __instance.CurJob?.def == JobDefOf.AttackStatic && __instance.CurJob.playerForced;
            if (!ordered && magazine.ReloadTicksLeft <= 0) return;
            if (!ordered && magazine.verbProps.onlyManualCast && !allowManualCastWeapons
                && (__instance.CurJob == null || __instance.CurJob.def == JobDefOf.Wait_Combat)) return;
            __result = magazine;
        }
    }
}
