using MihoAdfqs.Utility;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace MihoAdfqs.MihoAdfJobDriver
{
    public class JobDriver_Brainwash : JobDriver
    {
        private const int DurationTicks = 300; 
        private Pawn TargetPawn => (Pawn)TargetA.Thing;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(TargetA, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedOrNull(TargetIndex.A);
            this.FailOn(() => !TargetPawn.RaceProps.Humanlike);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
            // 创建一个通用的等待Toil，用于读条
            Toil channelToil = Toils_General.Wait(DurationTicks, TargetIndex.A)
                .WithProgressBarToilDelay(TargetIndex.A, false, -0.5f);

            // 设置失败条件
            channelToil.FailOnDespawnedOrNull(TargetIndex.A);
            channelToil.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);

            // 在读条开始时，让目标也进入等待状态并锁定
            channelToil.initAction = delegate
            {
                Pawn target = TargetPawn;
                if (target != null && !target.Dead && !target.Downed)
                {
                    // 强制目标等待，并面向施法者
                    Job waitJob = JobMaker.MakeJob(JobDefOf.Wait_MaintainPosture);
                    waitJob.expiryInterval = DurationTicks; // 持续时间和读条一样
                    waitJob.playerForced = true; // 玩家强制，防止被其他任务打断
                    target.jobs.StartJob(waitJob, JobCondition.InterruptForced, null, true, true, null, null, true);
                    target.pather.StopDead(); // 停止移动
                    target.rotationTracker.FaceTarget(pawn); // 面向施法者
                }
            };

            // 在读条期间，施法者也面向目标
            channelToil.tickAction = delegate
            {
                pawn.rotationTracker.FaceTarget(TargetA);
            };

            // 读条结束后，应用洗脑效果
            channelToil.AddFinishAction(delegate
            {
                Pawn target = TargetPawn;
                if (target != null)
                {
                    BrainwashUtility.BrainwashPawn(target);
                    Messages.Message($"{target.LabelShort} 已被洗脑！", target, MessageTypeDefOf.PositiveEvent, true);
                    if (job.ability != null)
                    {                        
                        job.ability.StartCooldown(job.ability.def.cooldownTicksRange.RandomInRange);
                    }
                }
            });

            // 将带有读条和效果的Toil返回
            yield return channelToil;
        }
    }
}
