using AlienRace;
using HarmonyLib;
using Verse;

namespace MihoAdfqs.Phase2.Compatibility
{
    //职责：让HAR识别帝国身体属性所属的原种族，保留该种族的饮食和行为限制。
    [HarmonyPatch(typeof(CachedData), nameof(CachedData.GetRaceFromRaceProps))]
    public static class Patch_HARRaceIdentity
    {
        //职责：仅在HAR反查种族身份时传入原始属性，不替换人物的佩尔克体身体结构。
        public static void Prefix(ref RaceProperties props) => props = ImperialRaceProperties.OriginalFor(props);
    }
}
