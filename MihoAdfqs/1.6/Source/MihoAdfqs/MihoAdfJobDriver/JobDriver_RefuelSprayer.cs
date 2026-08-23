using MihoAdfqs.MihoAdfComp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace MihoAdfqs.MihoAdfJobDriver
{
   public class JobDriver_RefuelSprayer : JobDriver
    {

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            // 预定燃料，防止别人同时也来拿
            return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch)
                .FailOnDespawnedNullOrForbidden(TargetIndex.A);
            Toil reloadToil = Toils_General.Wait(120);
            reloadToil.WithProgressBarToilDelay(TargetIndex.A);
            reloadToil.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            reloadToil.FailOnCannotTouch(TargetIndex.A, PathEndMode.ClosestTouch);
            yield return reloadToil;
            yield return new Toil
            {
                initAction = delegate
                {
                    Thing fuelItem = job.targetA.Thing;
                    Thing weapon = job.targetB.Thing;

                    var comp = weapon?.TryGetComp<ThingComp_FueledSprayer>();
                    if (comp != null && fuelItem != null)
                    {
                        comp.RefuelFromItem(fuelItem);
                    }
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };
        }
    }
}
