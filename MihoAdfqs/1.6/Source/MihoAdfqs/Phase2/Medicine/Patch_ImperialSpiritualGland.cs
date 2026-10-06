using HarmonyLib;
using Verse;

namespace MihoAdfqs.Phase2.Medicine
{
    //职责：阻止帝国美狐获得原种族的灵腺状态及其药物缺乏惩罚。
    [HarmonyPatch(typeof(Pawn_HealthTracker), nameof(Pawn_HealthTracker.AddHediff), new[] { typeof(Hediff), typeof(BodyPartRecord), typeof(DamageInfo?), typeof(DamageWorker.DamageResult) })]
    public static class Patch_ImperialSpiritualGland
    {
        //职责：拦截背景、茶饮和种族组件授予的灵腺；普通美狐仍沿用原种族规则。
        public static bool Prefix(Pawn ___pawn, Hediff hediff) =>
            !(Pawns.EmpirePawnUtility.Imperial(___pawn) && ___pawn.def.defName == "Alien_Miho"
              && hediff.def.defName == "Miho_Elegans_Hediff");
    }
}
