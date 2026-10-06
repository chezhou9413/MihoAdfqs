using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace MihoAdfqs.Phase2.Backgrounds
{
    //职责：为爱猫者精神崩溃寻找可到达的本派系宠物猫。
    public static class CatSlaughterUtility
    {
        //职责：只把家养猫作为候选，排除其它动物和敌对猫。
        public static Pawn FindCat(Pawn pawn) => pawn.Spawned ? pawn.Map.mapPawns.AllPawnsSpawned
            .Where(cat => cat.def.defName == "Cat" && cat.Faction == pawn.Faction && !cat.Dead && !cat.IsBurning()
                && !cat.InAggroMentalState && pawn.CanReserveAndReach(cat, PathEndMode.Touch, Danger.Deadly))
            .OrderBy(cat => cat.Position.DistanceToSquared(pawn.Position)).FirstOrDefault() : null;
    }
}
