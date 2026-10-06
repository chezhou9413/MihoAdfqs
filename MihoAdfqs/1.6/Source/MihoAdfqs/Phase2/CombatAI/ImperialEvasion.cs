using System.Collections.Generic;
using System.Linq;
using MihoAdfqs.Phase2.Helmer;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace MihoAdfqs.Phase2.CombatAI
{
    //紧急翻滚与步行撤离共用危险判断，并保留手动控制权。
    internal static class ImperialEvasion
    {
        private static readonly int[] WalkDistances = { 6, 12, 24, 40, 60 };
        //NPC撤退任务不被重新拉回战斗；玩家移动与交互优先。
        internal static bool CanAct(Pawn pawn)
        {
            if (pawn.Faction == null || pawn.Downed || pawn.InMentalState || pawn.IsPrisoner
                || !pawn.Awake() || pawn.stances.stunner.Stunned) return false;
            if (pawn.Faction.IsPlayer && (!pawn.Drafted || pawn.drafter.FireAtWill == false)) return false;
            Job job = pawn.CurJob;
            DutyDef duty = pawn.mindState.duty?.def;
            if (job?.exitMapOnArrival == true || pawn.GetLord()?.CurLordToil is LordToil_ExitMap
                || pawn.GetLord()?.CurLordToil is LordToil_PanicFlee || duty == DutyDefOf.ExitMapBest
                || duty == DutyDefOf.ExitMapBestAndDefendSelf || duty == DutyDefOf.ExitMapRandom
                || duty == DutyDefOf.ExitMapNearDutyTarget) return false;
            return !pawn.Faction.IsPlayer || job?.playerForced != true || job.def == JobDefOf.AttackStatic || job.def == JobDefOf.AttackMelee;
        }

        //风险解除后由任务恢复队列；危险持续时不重新生成相同撤离任务。
        internal static bool Update(Pawn pawn, ImperialCombatPawnState state, MapComponent_ImperialCombat component)
        {
            if (!CanAct(pawn)) return false;
            Gene_Helmer helmer = Gene_Helmer.Get(pawn);
            if (helmer != null && !helmer.automatic) return false;
            float orbital = component.Threats.OrbitalRisk(pawn.Position);
            //已经位于圈外时拦住通向炮击区的自动移动，等待预警解除。
            if (orbital <= 0 && pawn.pather.Moving && pawn.CurJob?.def.defName != "MihoPhase2_EvadeFire"
                && component.Threats.OrbitalRisk(pawn.pather.nextCell) > 0)
            {
                Job hold = JobMaker.MakeJob(DefDatabase<JobDef>.GetNamed("MihoPhase2_EvadeFire"), pawn.Position, pawn.pather.nextCell);
                pawn.jobs.StartJob(hold, JobCondition.InterruptForced, resumeCurJobAfterwards: true);
                return true;
            }
            //赫尔默的射弹翻滚仍由原基因驱动，组件只补足预警阶段的轨道撤离。
            if (helmer != null && orbital <= 0) return pawn.CurJob?.def.defName == "MihoPhase2_EvadeFire";
            float risk = component.Threats.Risk(pawn, pawn.Position);
            if (risk < 4f) return pawn.CurJob?.def.defName == "MihoPhase2_EvadeFire";
            int nextRoll = helmer != null ? helmer.nextRollTick : state.nextRollTick;
            if (Find.TickManager.TicksGame >= nextRoll && TryRoll(pawn, state, component, helmer, risk, orbital > 0)) return true;
            Job current = pawn.CurJob;
            if (current?.def.defName == "MihoPhase2_EvadeFire"
                && component.Threats.Risk(pawn, current.targetA.Cell) < 1f) return true;
            if (!TryWalkDestination(pawn, component.Threats, out IntVec3 destination)) return false;
            Job retreat = JobMaker.MakeJob(DefDatabase<JobDef>.GetNamed("MihoPhase2_EvadeFire"), destination, pawn.Position);
            pawn.jobs.StartJob(retreat, JobCondition.InterruptForced,
                resumeCurJobAfterwards: current?.def.defName != "MihoPhase2_EvadeFire");
            return true;
        }

        //固定检查十六方向两档距离，不穿墙且不落到火焰、毒气或人物所在格。
        private static bool TryRoll(Pawn pawn, ImperialCombatPawnState state, MapComponent_ImperialCombat component,
            Gene_Helmer helmer, float currentRisk, bool orbital)
        {
            IntVec3 best = IntVec3.Invalid;
            float bestScore = float.MaxValue;
            for (int direction = 0; direction < 16; direction++)
            {
                Vector3 offset = Quaternion.AngleAxis(direction * 22.5f, Vector3.up) * Vector3.forward;
                for (int length = 3; length <= 6; length += 3)
                {
                    IntVec3 requested = (pawn.Position.ToVector3Shifted() + offset * length).ToIntVec3();
                    if (!HelmerRoll.TryFindDestination(pawn, requested, out IntVec3 cell) || pawn.Position.DistanceToSquared(cell) < 4
                        || cell.GetFirstPawn(pawn.Map) != null || ReservedByOther(pawn, cell) || component.Threats.UnsafeTerrain(cell)) continue;
                    if (orbital && component.Threats.OrbitalRisk(cell) > 0) continue;
                    float risk = component.Threats.Risk(pawn, cell);
                    if (risk > 1f && risk > currentRisk * .65f) continue;
                    float score = risk * 10f + pawn.Position.DistanceTo(cell) * .1f;
                    if (score >= bestScore) continue;
                    best = cell; bestScore = score;
                }
            }
            if (!best.IsValid) return false;
            if (helmer != null) return HelmerRoll.TryRoll(helmer, best);
            IntVec3 start = pawn.Position;
            Map map = pawn.Map;
            var flyer = PawnFlyer.MakeFlyer(DefDatabase<ThingDef>.GetNamed("MihoPhase2_ImperialRoll"), pawn, best, null, null);
            GenSpawn.Spawn(flyer, start, map);
            state.nextRollTick = Find.TickManager.TicksGame + 600;
            return true;
        }

        //较大火力网需要步行到外围，先按距离排序再核对可达性。
        private static bool TryWalkDestination(Pawn pawn, ImperialCombatThreats threats, out IntVec3 destination)
        {
            var candidates = new HashSet<IntVec3>();
            for (int direction = 0; direction < 16; direction++)
            {
                Vector3 offset = Quaternion.AngleAxis(direction * 22.5f, Vector3.up) * Vector3.forward;
                foreach (int length in WalkDistances)
                {
                    IntVec3 cell = (pawn.Position.ToVector3Shifted() + offset * length).ToIntVec3();
                    if (!cell.InBounds(pawn.Map) || !cell.Standable(pawn.Map) || cell.GetFirstPawn(pawn.Map) != null
                        || ReservedByOther(pawn, cell) || threats.Risk(pawn, cell) > 1f) continue;
                    candidates.Add(cell);
                }
            }
            foreach (IntVec3 cell in candidates.OrderBy(c => c.DistanceToSquared(pawn.Position)).Take(8))
                if (pawn.CanReach(cell, PathEndMode.OnCell, Danger.Deadly)) { destination = cell; return true; }
            destination = IntVec3.Invalid;
            return false;
        }

        //遵守已有撤离终点预留，减少军队集中移动时的互相阻挡。
        private static bool ReservedByOther(Pawn pawn, IntVec3 cell) => pawn.Map.pawnDestinationReservationManager
            .IsReserved(cell, out Pawn claimant) && claimant != pawn;
    }
}
