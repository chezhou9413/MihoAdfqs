using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace MihoAdfqs.Phase2.CombatAI
{
    //撤离后等原位置的轨道预警解除，防止战斗AI反复返回炮击区。
    public sealed class JobDriver_ImperialEvade : JobDriver
    {
        private int holdUntil;

        //读档后保留最短停留时间。
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref holdUntil, "holdUntil");
        }
        //预留撤离终点，避免多人选到同一个目的格。
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            pawn.Map.pawnDestinationReservationManager.Reserve(pawn, job, job.targetA.Cell);
            return true;
        }

        //走到安全位置后原地还击，持续火力网结束再恢复原任务。
        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.OnCell);
            Toil hold = Toils_General.Wait(60);
            hold.defaultCompleteMode = ToilCompleteMode.Never;
            hold.initAction += () => holdUntil = Find.TickManager.TicksGame + 60;
            hold.tickAction = () =>
            {
                if (!ImperialEvasion.CanAct(pawn) || (Find.TickManager.TicksGame >= holdUntil
                    && Map.GetComponent<MapComponent_ImperialCombat>().Threats.OrbitalRisk(job.targetB.Cell) <= 0))
                {
                    ReadyForNextToil();
                    return;
                }
                if (pawn.IsHashIntervalTick(6)) TryDefendInPlace();
            };
            yield return hold;
        }

        //沿用原版自由射击的目标筛选，只还击当前位置能攻击的敌人。
        private void TryDefendInPlace()
        {
            if (pawn.stances.FullBodyBusy || pawn.IsCarryingPawn() || pawn.WorkTagIsDisabled(WorkTags.Violent)
                || (!pawn.IsPlayerControlled && pawn.IsPsychologicallyInvisible())) return;
            if (pawn.kindDef.canMeleeAttack)
            {
                for (int i = 0; i < GenAdj.AdjacentCellsAndInside.Length; i++)
                {
                    IntVec3 cell = pawn.Position + GenAdj.AdjacentCellsAndInside[i];
                    if (!cell.InBounds(Map)) continue;
                    List<Thing> things = cell.GetThingList(Map);
                    for (int j = 0; j < things.Count; j++)
                    {
                        if (!(things[j] is Pawn enemy) || enemy.ThreatDisabled(pawn) || !pawn.HostileTo(enemy)
                            || pawn.ThreatDisabledBecauseNonAggressiveRoamer(enemy)
                            || !GenHostility.IsActiveThreatTo(enemy, pawn.Faction, false)) continue;
                        CompActivity activity = enemy.GetComp<CompActivity>();
                        if (activity != null && !activity.IsActive) continue;
                        pawn.meleeVerbs.TryMeleeAttack(enemy);
                        return;
                    }
                }
            }
            if (!job.canUseRangedWeapon || pawn.drafter?.FireAtWill == false) return;
            Verb verb = pawn.CurrentEffectiveVerb;
            if (verb == null || verb.verbProps.IsMeleeAttack) return;
            TargetScanFlags flags = TargetScanFlags.NeedLOSToAll | TargetScanFlags.NeedThreat | TargetScanFlags.NeedAutoTargetable;
            if (verb.IsIncendiary_Ranged()) flags |= TargetScanFlags.NeedNonBurning;
            IAttackTarget target = AttackTargetFinder.BestShootTargetFromCurrentPosition(pawn, flags);
            if (target != null) pawn.TryStartAttack(target.Thing);
        }
    }
}
