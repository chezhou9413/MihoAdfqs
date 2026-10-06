using RimWorld;
using Verse;

namespace MihoAdfqs.Phase2.Pawns
{
    //识别帝国人物和专属实验基因，限定规则的作用对象。
    public static class EmpirePawnUtility
    {
        //检查指定基因是否生效。
        public static bool HasGene(Pawn pawn, string name) => pawn?.genes?.GetGene(DefDatabase<GeneDef>.GetNamed(name))?.Active == true;
        //复用已建立的兵种索引，避免体型等属性查询反复比较名称前缀。
        public static bool Imperial(Pawn pawn) => pawn != null && ImperialBodyRegistry.ForKind(pawn.kindDef) != null;
        //识别45号实验对象。
        public static bool Helmer(Pawn pawn) => ImperialGeneLookup.For(pawn).IsHelmer;
        //识别使用元首部件保护规则的人物。
        public static bool Fuhrer(Pawn pawn) => pawn?.kindDef?.defName == "Miho_adfqs" || ImperialGeneLookup.For(pawn).IsFuhrer;
        //识别仍保留不可死亡规则的提亚娜。
        public static bool Immortal(Pawn pawn) => ImperialGeneLookup.For(pawn).IsImmortal;
        //职责：识别需要保留一滴血的身体部件。
        public static bool Preserve(Pawn pawn, BodyPartRecord part)
        {
            if (Fuhrer(pawn) || Immortal(pawn)) return true;
            if (!Helmer(pawn) || part == null) return false;
            for (BodyPartRecord current = part; current != null; current = current.parent)
                if (current.def == BodyPartDefOf.Arm || current.def == BodyPartDefOf.Leg) return true;
            return false;
        }
    }
}
