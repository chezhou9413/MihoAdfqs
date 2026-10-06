using HarmonyLib;
using RimWorld;
using Verse;

namespace MihoAdfqs.Buildings.Defense
{
    //战备堡垒只有随舱弹药，自动工作和手动补给都不可增加库存。
    [HarmonyPatch(typeof(RefuelWorkGiverUtility), nameof(RefuelWorkGiverUtility.CanRefuel))]
    public static class Patch_CombatFortressRefuel
    {
        //普通建筑沿用原版补给判断。
        public static bool Prefix(Thing t, ref bool __result)
        {
            if (!(t is Building_CombatFortress)) return true;
            __result = false;
            return false;
        }
    }
}
