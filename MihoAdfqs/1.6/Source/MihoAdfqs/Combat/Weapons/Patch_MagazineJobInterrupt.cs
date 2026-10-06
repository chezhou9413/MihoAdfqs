using HarmonyLib;
using Verse;
using Verse.AI;

namespace MihoAdfqs.Combat.Weapons
{
    //任务已经确定开始后结束旧武器连射，排队的指令不会提前停火。
    [HarmonyPatch(typeof(Pawn_JobTracker), nameof(Pawn_JobTracker.StartJob))]
    public static class Patch_MagazineJobInterrupt
    {
        //原版只取消瞄准姿态，整匣连射还会继续续上冷却，因此必须重置动词。
        public static void Prefix(Pawn ___pawn, Job newJob, Job ___curJob)
        {
            if (newJob == null || newJob == ___curJob) return;
            CompEquippable equipment = ___pawn.equipment?.PrimaryEq;
            if (equipment == null) return;
            var verbs = equipment.AllVerbs;
            for (int i = 0; i < verbs.Count; i++)
                if (verbs[i] is Verb_Magazine magazine) magazine.StopFiring();
        }
    }
}
