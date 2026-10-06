using RimWorld;
using Verse;
using Verse.AI;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;

namespace MihoAdfqs.Phase2.Helmer
{
    //承载地面翻滚期间的人物，并同步人物旋转动画与原任务控制权。
    public class PawnFlyer_HelmerRoll : PawnFlyer
    {
        private static readonly AccessTools.FieldRef<PawnFlyer, JobQueue> CapturedJobs =
            AccessTools.FieldRefAccess<PawnFlyer, JobQueue>("jobQueue");
        private List<bool> originalForced = new List<bool>();

        //使翻滚保持半秒，并在创建或读档后挂接对应动画。
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            if (!respawningAfterLoad) originalForced = CapturedJobs(this).Select(j => j.job.playerForced).ToList();
            ticksFlightTime = 30;
            FlyingPawn.Drawer.renderer.SetAnimation(DefDatabase<AnimationDef>.GetNamed("MihoPhase2_HelmerRollAnimation"));
            FlyingPawn.Drawer.renderer.renderTree.animationStartTick = Find.TickManager.TicksGame - ticksFlying;
        }

        //原版恢复队列会标记为手动命令，落地后还原每项原有的控制标记。
        protected override void RespawnPawn()
        {
            FlyingPawn.Drawer.renderer.SetAnimation(null);
            List<Job> jobs = CapturedJobs(this).Select(j => j.job).ToList();
            base.RespawnPawn();
            for (int i = 0; i < jobs.Count; i++) jobs[i].playerForced = originalForced[i];
        }

        //读档不把原来的自动任务转换成玩家手动任务。
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref originalForced, "originalForced", LookMode.Value);
        }
    }
}
