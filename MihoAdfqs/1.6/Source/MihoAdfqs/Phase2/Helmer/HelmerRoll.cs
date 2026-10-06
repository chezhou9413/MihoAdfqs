using RimWorld;
using UnityEngine;
using Verse;

namespace MihoAdfqs.Phase2.Helmer
{
    //校验翻滚路线、转移人物到移动实体并开始冷却。
    public static class HelmerRoll
    {
        //沿目标方向寻找最多六格的连续可通行落点，成功移动时才消耗冷却。
        public static bool TryRoll(Gene_Helmer gene, IntVec3 target)
        {
            Pawn pawn = gene.pawn;
            if (!pawn.Spawned || pawn.Downed || !gene.Active || Find.TickManager.TicksGame < gene.nextRollTick) return false;
            if (!TryFindDestination(pawn, target, out IntVec3 destination)) return false;
            Map map = pawn.Map;
            IntVec3 start = pawn.Position;
            PawnFlyer flyer = PawnFlyer.MakeFlyer(DefDatabase<ThingDef>.GetNamed("MihoPhase2_HelmerRoll"), pawn, destination, null, null);
            GenSpawn.Spawn(flyer, start, map);
            gene.nextRollTick = Find.TickManager.TicksGame + 1200;
            return true;
        }

        //预选落点和实际翻滚复用同一套路线规则。
        internal static bool TryFindDestination(Pawn pawn, IntVec3 target, out IntVec3 destination)
        {
            Vector3 direction = (target - pawn.Position).ToVector3().normalized;
            float length = Mathf.Min(6f, pawn.Position.DistanceTo(target));
            destination = pawn.Position;
            //半格采样防止斜向翻滚越过墙角；关闭的门和障碍物终止路线。
            for (float step = 0.5f; step <= length; step += 0.5f)
            {
                IntVec3 cell = (pawn.Position.ToVector3Shifted() + direction * step).ToIntVec3();
                if (!cell.InBounds(pawn.Map) || !cell.Standable(pawn.Map) || !GenSight.LineOfSight(pawn.Position, cell, pawn.Map)) break;
                if (pawn.Position.DistanceToSquared(cell) > 36) break;
                if (!cell.Fogged(pawn.Map)) destination = cell;
            }
            return destination != pawn.Position;
        }
    }
}
