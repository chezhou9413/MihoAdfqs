using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace MihoAdfqs.Phase2.CombatAI
{
    //撤离期间暂停队伍的常驻攻击决策，撤退和手动指令仍按原版执行。
    [HarmonyPatch(typeof(ThinkNode_DutyConstant), nameof(ThinkNode_DutyConstant.TryIssueJobPackage))]
    public static class Patch_ImperialEvadeDuty
    {
        //危险解除后由撤离任务结束，原队伍无需改变职责。
        public static bool Prefix(Pawn pawn, ref ThinkResult __result)
        {
            if (pawn.CurJob?.def.defName != "MihoPhase2_EvadeFire" || !ImperialEvasion.CanAct(pawn)) return true;
            __result = ThinkResult.NoJob;
            return false;
        }
    }
}
