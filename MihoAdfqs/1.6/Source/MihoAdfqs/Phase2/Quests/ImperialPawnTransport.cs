using RimWorld.Planet;
using Verse;
using Verse.AI.Group;

namespace MihoAdfqs.Phase2.Quests
{
    //职责：在任务运输前解除人物的地图、集群、商队及世界列表归属。
    public static class ImperialPawnTransport
    {
        //职责：将原人物交给运输容器，移除失去最后一名成员的商队。
        public static void Detach(Pawn pawn)
        {
            Caravan caravan = pawn.GetCaravan();
            pawn.GetLord()?.RemovePawn(pawn);
            if (pawn.Spawned) pawn.DeSpawn();
            else pawn.holdingOwner?.Remove(pawn);
            if (caravan != null && caravan.PawnsListForReading.Count == 0) caravan.Destroy();
            if (Find.WorldPawns.Contains(pawn)) Find.WorldPawns.RemovePawn(pawn);
        }
    }
}
