using HarmonyLib;
using RimWorld;
using Verse;

namespace MihoAdfqs.Phase2.Pawns
{
    //使四号和45号实验人物不会被其它人物占格阻挡。
    [StaticConstructorOnStartup]
    [HarmonyPatch(typeof(PawnUtility), nameof(PawnUtility.ShouldCollideWithPawns))]
    public static class Patch_ImperialCollision
    {
        private static readonly ThingDef Suit = DefDatabase<ThingDef>.GetNamed("MihoPhase2_HelmerZogSuit");
        private static readonly ThingDef Casual = DefDatabase<ThingDef>.GetNamed("MihoPhase2_HelmerZogCasual");

        //使用缓存基因与服装Def引用，避免寻路时重复解析名称和分配枚举器。
        public static void Postfix(Pawn p, ref bool __result)
        {
            //原版寻路允许传入空人物；已经无需碰撞时不再读取人物组件。
            if (!__result || p == null) return;
            if (ImperialGeneLookup.For(p).IgnoresPawnCollision) { __result = false; return; }
            var apparel = p.apparel?.WornApparel;
            if (apparel == null) return;
            for (int i = 0; i < apparel.Count; i++)
                if (apparel[i].def == Suit || apparel[i].def == Casual) { __result = false; return; }
        }
    }
}
