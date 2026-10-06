using MihoAdfqs.MihoAdfComp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace MihoAdfqs.MihoAdfJobDriver
{
    //为喷火器搬运燃料并播放装填开始与完成音效。
    public class JobDriver_RefuelSprayer : JobDriver
    {

        //预留本次装填需要的燃料。
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
        }

        //靠近燃料后完成装填，失败或中断不会播放完成声音。
        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch)
                .FailOnDespawnedNullOrForbidden(TargetIndex.A);
            Toil reloadToil = Toils_General.Wait(120);
            reloadToil.AddPreInitAction(() => DefDatabase<SoundDef>.GetNamed("MihoCombat_Reload_Weapon_BeamFlamethrower")
                .PlayOneShot(new TargetInfo(pawn.Position, pawn.Map)));
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
                        DefDatabase<SoundDef>.GetNamed("MihoCombat_Reload_Weapon_BeamFlamethrower_Complete")
                            .PlayOneShot(new TargetInfo(pawn.Position, pawn.Map));
                    }
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };
        }
    }
}
