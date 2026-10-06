using HarmonyLib;
using Verse;
using Verse.AI;

namespace MihoAdfqs.Buildings.Agriculture
{
    //清理播种障碍物时，禁止把它搬回任一培养机的种植格。
    [HarmonyPatch(typeof(HaulAIUtility), "HaulablePlaceValidator")]
    public static class Patch_CultivatorHaulAside
    {
        //其余落点仍由原版检查通行、预约、火焰和搬运阻挡。
        public static bool Prefix(Thing haulable, Pawn worker, IntVec3 c, ref bool __result)
        {
            if (haulable.def.BlocksPlanting() && c.GetEdifice(worker.Map) is Building_IndoorCultivator)
            {
                __result = false;
                return false;
            }
            return true;
        }
    }
}
