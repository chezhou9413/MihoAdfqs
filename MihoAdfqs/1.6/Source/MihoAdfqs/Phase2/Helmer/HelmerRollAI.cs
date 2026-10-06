using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace MihoAdfqs.Phase2.Helmer
{
    //翻滚先避险，再考虑射程和掩体；不干涉玩家的移动与交互指令。
    internal static class HelmerRollAI
    {
        //紧急检查允许打断瞄准和连发，冷却期间不收集威胁。
        public static bool TryEvade(Gene_Helmer gene)
        {
            if (!CanAct(gene)) return false;
            var threats = new HelmerRollThreats(gene.pawn);
            float currentRisk = threats.Risk(gene.pawn.Position);
            if (currentRisk < 4f) return false;
            return TryChoose(gene, threats, null, null, IntVec3.Invalid, currentRisk, true);
        }

        //普通走位要求落点明显改善射击条件，不为小幅移动消耗翻滚。
        public static bool TryReposition(Gene_Helmer gene, Verb weapon, Pawn target, IntVec3 desired)
        {
            if (!CanAct(gene) || gene.pawn.Position.DistanceTo(desired) < 3f) return false;
            return TryChoose(gene, new HelmerRollThreats(gene.pawn), weapon, target, desired, 0f, false);
        }

        //保持自动控制开关、手动移动和停止自由射击的优先级。
        private static bool CanAct(Gene_Helmer gene)
        {
            Pawn pawn = gene.pawn;
            if (!gene.Active || !gene.automatic || !pawn.Spawned || pawn.Downed || pawn.InMentalState
                || Find.TickManager.TicksGame < gene.nextRollTick) return false;
            if (pawn.IsColonistPlayerControlled && !pawn.Drafted) return false;
            if (pawn.Drafted && pawn.drafter?.FireAtWill == false) return false;
            Job job = pawn.CurJob;
            return job?.playerForced != true || job.def == JobDefOf.AttackStatic || job.def == JobDefOf.AttackMelee;
        }

        //固定检查十六个方向、两档距离，候选数量不随地图尺寸增长。
        private static bool TryChoose(Gene_Helmer gene, HelmerRollThreats threats, Verb weapon, Pawn target,
            IntVec3 desired, float currentRisk, bool emergency)
        {
            Pawn pawn = gene.pawn;
            IntVec3 best = IntVec3.Invalid;
            float bestScore = float.MinValue;
            float previousTactical = emergency ? 0f : TacticalScore(pawn, weapon, target, pawn.Position, desired);
            for (int direction = 0; direction < 16; direction++)
            {
                Vector3 offset = Quaternion.AngleAxis(direction * 22.5f, Vector3.up) * Vector3.forward;
                for (int length = 3; length <= 6; length += 3)
                {
                    IntVec3 requested = (pawn.Position.ToVector3Shifted() + offset * length).ToIntVec3();
                    if (!HelmerRoll.TryFindDestination(pawn, requested, out IntVec3 cell)
                        || pawn.Position.DistanceTo(cell) < (emergency ? 2f : 3f) || threats.HasFire(cell)) continue;
                    Pawn occupant = cell.GetFirstPawn(pawn.Map);
                    if (occupant != null && occupant != pawn) continue;
                    float risk = threats.Risk(cell);
                    float score;
                    if (emergency)
                    {
                        //逃离危险须有实质改善；宁可保留冷却，也不翻入更密集的火力。
                        if (risk > 1f && risk > currentRisk * .65f) continue;
                        score = -risk * 10f - pawn.Position.DistanceTo(cell) * .1f;
                    }
                    else
                    {
                        if (risk > 1f || !weapon.CanHitTargetFrom(cell, target)) continue;
                        score = TacticalScore(pawn, weapon, target, cell, desired);
                        if (score < previousTactical + 3f) continue;
                    }
                    if (score <= bestScore) continue;
                    best = cell;
                    bestScore = score;
                }
            }
            return best.IsValid && HelmerRoll.TryRoll(gene, best);
        }

        //优先接近预选射击位、利用掩体，并留出与目标的近战间隔。
        private static float TacticalScore(Pawn pawn, Verb weapon, Pawn target, IntVec3 cell, IntVec3 desired)
        {
            float distance = cell.DistanceTo(target.Position);
            float cover = CoverUtility.CalculateOverallBlockChance(new LocalTargetInfo(cell), target.Position, pawn.Map);
            float score = -cell.DistanceTo(desired) + cover * 8f;
            if (distance < Mathf.Max(4f, weapon.verbProps.minRange)) score -= 20f;
            return score;
        }
    }
}
