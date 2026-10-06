using HarmonyLib;
using RimWorld;
using Verse;

namespace MihoAdfqs.Phase2.Orbital
{
    //职责：敌军袭击成功产生后触发已经开启的永久动员支援。
    [HarmonyPatch(typeof(IncidentWorker_RaidEnemy), "TryExecuteWorker")]
    public static class Patch_OrbitalRaidSupport
    {
        //职责：排除未成功生成的袭击，使用袭击实际地图安排支援。
        public static void Postfix(IncidentParms parms, bool __result)
        {
            if (__result) GameComponent_OrbitalNetwork.Current.OnRaid(parms.target as Map);
        }
    }
}
