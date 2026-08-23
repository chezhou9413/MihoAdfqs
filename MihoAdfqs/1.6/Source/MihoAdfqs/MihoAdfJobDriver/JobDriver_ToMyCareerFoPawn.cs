using MihoAdfqs.ThingClass;
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
   public class JobDriver_ToMyCareerFoPawn : JobDriver
    {
        protected Thing Item => job.targetA.Thing;
        protected Pawn Victim => (Pawn)job.targetB.Thing;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(Item, job, 1, -1, null, errorOnFailed) &&
                   pawn.Reserve(Victim, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            // 1. 全局失效检查
            this.FailOnDespawnedOrNull(TargetIndex.B);
            this.FailOn(() => Victim.Dead);

            // 2. 走向物品 (此时物品在地上)
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch).FailOnDespawnedOrNull(TargetIndex.A);

            // 3. 拿起物品
            Toil pickupToil = new Toil();
            pickupToil.initAction = () =>
            {
                if (Item != null && pawn.carryTracker.CarriedThing == null)
                {
                    pawn.carryTracker.TryStartCarry(Item, 1);
                }
            };
            pickupToil.defaultCompleteMode = ToilCompleteMode.Instant;
            yield return pickupToil;

            // 4. 手动处理走向目标的过程
            Toil gotoVictim = new Toil();
            gotoVictim.initAction = () =>
            {
                // 即使 Item 已经不在地面上(Spawned=false)，我们也强制小人走向目标 Pawn
                pawn.pather.StartPath(Victim, PathEndMode.Touch);
            };
            gotoVictim.tickAction = () =>
            {
                // 如果小人走到了目标旁边，进入下一步
                if (pawn.Position.AdjacentTo8WayOrInside(Victim.Position))
                {
                    ReadyForNextToil();
                }
                // 如果小人没在走，也没到地点（可能被卡住了），则尝试重新开始路径
                else if (!pawn.pather.Moving)
                {
                    pawn.pather.StartPath(Victim, PathEndMode.Touch);
                }
            };
            // 使用 Never 替代 Incomplete，表示必须手动调用 ReadyForNextToil()
            gotoVictim.defaultCompleteMode = ToilCompleteMode.Never;
            gotoVictim.AddFailCondition(() => pawn.carryTracker.CarriedThing == null);
            yield return gotoVictim;

            // 5. 读条阶段
            Toil waitToil = Toils_General.Wait(300);
            waitToil.WithProgressBarToilDelay(TargetIndex.B);

            waitToil.tickAction = () =>
            {
                // 维持物品在手里
                if (pawn.carryTracker.CarriedThing == null) { this.EndJobWith(JobCondition.Incompletable); }

                // 强制目标原地站立
                if (Victim.CurJobDef != JobDefOf.Wait_MaintainPosture && !Victim.Dead)
                {
                    Victim.jobs.StartJob(JobMaker.MakeJob(JobDefOf.Wait_MaintainPosture), JobCondition.InterruptForced);
                }
            };
            waitToil.AddFinishAction(() =>
            {
                OnInteractionFinished(pawn, Victim);
            });
            yield return waitToil;
            Toil dropToil = new Toil();
            dropToil.initAction = () =>
            {
                if (pawn.carryTracker.CarriedThing != null)
                {
                    pawn.carryTracker.TryDropCarriedThing(pawn.Position, ThingPlaceMode.Near, out _);
                }
            };
            dropToil.defaultCompleteMode = ToilCompleteMode.Instant;
            yield return dropToil;
        }

        private void OnInteractionFinished(Pawn actor, Pawn target)
        {
            Messages.Message($"{actor.LabelShort} 完成了对 {target.LabelShort} 的洗脑！", MessageTypeDefOf.PositiveEvent);
            BrainwashUtility.BrainwashPawn(target);
            if (Item is myCareerThing myItem)
            {
                myItem.lastUsedTick = Find.TickManager.TicksGame;
            }
            if (target.CurJobDef == JobDefOf.Wait_MaintainPosture)
            {
                target.jobs.EndCurrentJob(JobCondition.InterruptForced);
            }
        }
    }
}
