using System.Linq;
using RimWorld;
using Verse;

namespace MihoAdfqs.Phase2.Pawns
{
    //职责：为首次生成的制式单位提供完整穿戴清单及四种超新星主武器。
    public static class SpecialPawnEquipment
    {
        private static readonly string[] NovaWeapons =
        {
            "MihoPhase2_SupernovaHeavyLauncher", "MihoPhase2_SupernovaLaserRifle",
            "MihoPhase2_SupernovaEMPrecisionRifle", "MihoPhase2_SupernovaEMMarksmanRifle"
        };

        //职责：清理随机服装，按内层到外层顺序穿戴所需服装。
        public static void Dress(Pawn pawn, params string[] clothing)
        {
            foreach (Apparel apparel in pawn.apparel.WornApparel.ToList()) apparel.Destroy();
            foreach (string name in clothing) pawn.apparel.Wear((Apparel)Make(pawn, name), false);
        }

        //职责：用指定主武器替换人物生成器选出的随机武器。
        public static void Arm(Pawn pawn, string weapon)
        {
            pawn.equipment.DestroyAllEquipment();
            pawn.equipment.AddEquipment((ThingWithComps)Make(pawn, weapon));
        }

        //职责：从需求指定的四种超新星枪械中等概率选取制式主武器。
        public static void ArmNova(Pawn pawn) => Arm(pawn, NovaWeapons.RandomElement());

        //职责：以定义默认材料及兵种品质生成装备。
        private static Thing Make(Pawn pawn, string name)
        {
            ThingDef def = DefDatabase<ThingDef>.GetNamed(name);
            Thing result = ThingMaker.MakeThing(def, GenStuff.DefaultStuffFor(def));
            result.TryGetComp<CompQuality>()?.SetQuality(pawn.kindDef.itemQuality, ArtGenerationContext.Outsider);
            return result;
        }
    }
}
