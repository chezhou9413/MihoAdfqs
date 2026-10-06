using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace MihoAdfqs.Buildings.Medical
{
    //职责：预约并搬运药物、患者或尸体到医疗舱，抵达后完成转移。
    public class JobDriver_ServiceMedicalPod : JobDriver
    {
        private Building_MedicalPod Pod => (Building_MedicalPod)TargetB.Thing;

        //职责：同时预约搬运目标和医疗舱，避免多人并发装入。
        public override bool TryMakePreToilReservations(bool errorOnFailed) => pawn.Reserve(TargetA, job, 1, -1, null, errorOnFailed) && pawn.Reserve(TargetB, job, 1, -1, null, errorOnFailed);

        //职责：在目标或舱体失效时结束任务，搬运失败不消耗物品。
        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDestroyedOrNull(TargetIndex.A);
            this.FailOnDespawnedNullOrForbidden(TargetIndex.B);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch);
            yield return Toils_Haul.StartCarryThing(TargetIndex.A);
            yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.InteractionCell);
            yield return Toils_General.Wait(100, TargetIndex.B).WithProgressBarToilDelay(TargetIndex.B);
            Toil finish = ToilMaker.MakeToil("完成医疗舱搬运");
            finish.initAction = () =>
            {
                Thing carried = pawn.carryTracker.CarriedThing;
                if (carried.def.IsMedicine) Pod.AddMedicine(carried);
                else Pod.TryAcceptThing(carried);
            };
            finish.defaultCompleteMode = ToilCompleteMode.Instant;
            yield return finish;
        }
    }
}
