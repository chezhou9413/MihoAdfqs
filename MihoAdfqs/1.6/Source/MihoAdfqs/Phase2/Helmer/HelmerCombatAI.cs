using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace MihoAdfqs.Phase2.Helmer
{
    //选择目标、切换远射与近攻姿态，并驱动射击、保持距离和战斗翻滚。
    public static class HelmerCombatAI
    {
        //依据当前目标与武器射程执行一次低频战斗决策。
        public static void Update(Gene_Helmer gene)
        {
            Pawn pawn = gene.pawn;
            Verb weapon = pawn.equipment?.PrimaryEq?.PrimaryVerb;
            if (weapon == null || weapon.verbProps.IsMeleeAttack) { gene.storm = false; return; }
            if (pawn.Drafted && pawn.drafter?.FireAtWill == false) { gene.storm = false; return; }
            //保留玩家指定的攻击对象，搜索仅限可见且仍有战斗能力的敌人。
            Pawn target = pawn.CurJob?.targetA.Thing as Pawn;
            if (!ValidEnemy(pawn, target))
            {
                target = null;
                float range = weapon.verbProps.range * 2f;
                float bestDistance = range * range;
                var pawns = pawn.Map.mapPawns.AllPawnsSpawned;
                for (int i = 0; i < pawns.Count; i++)
                {
                    Pawn candidate = pawns[i];
                    int candidateDistance = candidate.Position.DistanceToSquared(pawn.Position);
                    if (candidateDistance > bestDistance || target != null && candidateDistance == bestDistance
                        || !ValidEnemy(pawn, candidate)) continue;
                    target = candidate;
                    bestDistance = candidateDistance;
                }
            }
            if (target == null) { gene.storm = false; return; }
            //手动移动和交互任务保留控制权，自动逻辑只接管等待及攻击任务。
            Job current = pawn.CurJob;
            if (current?.def.defName == "MihoPhase2_EvadeFire") return;
            if (current?.playerForced == true && current.def != JobDefOf.AttackStatic && current.def != JobDefOf.AttackMelee) return;
            if (weapon.WarmingUp || weapon.Bursting) return;
            float distance = pawn.Position.DistanceTo(target.Position);
            //进出阈值不同，避免在同一距离附近反复改变姿态。
            gene.storm = gene.storm ? distance < weapon.verbProps.range * 0.65f : distance < weapon.verbProps.range * 0.4f;
            float desired = weapon.EffectiveRange * (gene.storm ? 0.75f : 0.85f);
            var request = new CastPositionRequest
            {
                caster = pawn, target = target, verb = weapon,
                maxRangeFromTarget = desired, maxRangeFromCaster = weapon.verbProps.range * 2f,
                wantCoverFromTarget = !gene.storm,
                validator = cell => gene.storm || cell.DistanceTo(target.Position) >= desired * 0.8f
            };
            if (CastPositionFinder.TryFindCastPosition(request, out IntVec3 position) && position != pawn.Position)
            {
                if (HelmerRollAI.TryReposition(gene, weapon, target, position)) return;
                if (current?.def == JobDefOf.Goto && current.targetA.Cell.DistanceTo(position) < 3f) return;
                pawn.jobs.StartJob(JobMaker.MakeJob(JobDefOf.Goto, position), JobCondition.InterruptForced);
                return;
            }
            if (!weapon.CanHitTarget(target)) return;
            if (current?.def == JobDefOf.AttackStatic && current.targetA.Thing == target) return;
            Job attack = JobMaker.MakeJob(JobDefOf.AttackStatic, target);
            attack.endIfCantShootTargetFromCurPos = true;
            pawn.jobs.StartJob(attack, JobCondition.InterruptForced);
        }

        //排除倒地、不可见以及非敌对人物。
        private static bool ValidEnemy(Pawn pawn, Pawn target) => target != null && target.Spawned && !target.Dead && !target.Downed
            && target.Map == pawn.Map && target.HostileTo(pawn) && !target.Position.Fogged(pawn.Map) && !target.IsPsychologicallyInvisible();
    }
}
