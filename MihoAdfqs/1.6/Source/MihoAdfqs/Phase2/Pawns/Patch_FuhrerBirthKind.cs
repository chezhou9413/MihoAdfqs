using HarmonyLib;
using Verse;

namespace MihoAdfqs.Phase2.Pawns
{
    //元首的孩子采用普通美狐出生定义，避免跨种族配对改成其它种族。
    [HarmonyPatch(typeof(AlienRace.HarmonyPatches), nameof(AlienRace.HarmonyPatches.BirthOutcomeHelper))]
    public static class Patch_FuhrerBirthKind
    {
        //在HAR选出婴儿种类后，只指定元首孩子的美狐种族。
        public static void Postfix(Pawn mother, ref PawnKindDef __result)
        {
            if (EmpirePawnUtility.Fuhrer(mother))
                __result = DefDatabase<PawnKindDef>.GetNamed("Miho_PlayerColonistBorn");
        }
    }
}
