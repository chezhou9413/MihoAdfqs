using HarmonyLib;
using RimWorld;
using Verse;

namespace MihoAdfqs.Phase2.Pawns
{
    //职责：在新人物生成前指定特殊人物年龄，生成后设定身份及制式装备。
    [HarmonyPatch(typeof(PawnGenerator), "GenerateNewPawnInternal")]
    public static class Patch_SpecialPawnGeneration
    {
        //职责：将特殊人物的固定年龄直接交给生成请求，避免在连续年龄曲线中反复抽取单点。
        public static void Prefix(ref PawnGenerationRequest request)
        {
            int age;
            switch (request.KindDef.defName)
            {
                case "Miho_adfqs": age = 362; break;
                case "MihoPhase2_Tiana": age = 233; break;
                case "MihoPhase2_LeaderGuardCaptain": age = 74; break;
                default: return;
            }
            request.FixedBiologicalAge = age;
            request.FixedChronologicalAge = age;
        }

        //职责：按人物种类设置首次生成时的年龄、技能和装备。
        public static void Postfix(Pawn __result)
        {
            Pawn pawn = __result;
            if (pawn == null) return;
            if (ImperialBodyRegistry.ForKind(pawn.kindDef) != null && pawn.genes != null)
                foreach (Gene gene in pawn.genes.GenesListForReading)
                    if (gene is Genes.Gene_GrantsHediff grant) grant.EnsureHediff();
            switch (pawn.kindDef.defName)
            {
                case "MihoPhase2_ImperialMilira":
                    Factions.ImperialMiliraEquipment.Civilian(pawn);
                    break;
                case "MihoPhase2_ImperialMiliraRaider":
                    Factions.ImperialMiliraEquipment.Raider(pawn);
                    break;
                case "Miho_adfqs":
                    SetAge(pawn, 362);
                    break;
                case "MihoPhase2_Tiana":
                    SetAge(pawn, 233);
                    pawn.Name = new NameSingle("提亚娜");
                    foreach (SkillRecord skill in pawn.skills.skills)
                    {
                        skill.Level = skill.def == SkillDefOf.Crafting ? 12 :
                            skill.def == SkillDefOf.Intellectual ? 6 : skill.def == SkillDefOf.Cooking ? 15 : 3;
                        skill.xpSinceLastLevel = 0;
                    }
                    SpecialPawnEquipment.Dress(pawn, "MihoPhase2_TianaChant");
                    Patch_TianaApparelOptimization.Prefix(pawn);
                    pawn.equipment.DestroyAllEquipment();
                    break;
                case "MihoPhase2_LeaderGuardCaptain":
                    SetAge(pawn, 74);
                    pawn.Name = new NameTriple("赫尔默", "赫尔默", "佐格");
                    pawn.skills.GetSkill(SkillDefOf.Shooting).Level = 20;
                    pawn.skills.GetSkill(SkillDefOf.Melee).Level = 20;
                    pawn.skills.GetSkill(SkillDefOf.Medicine).Level = 20;
                    SpecialPawnEquipment.Dress(pawn, "Miho_Apparel_Under_Industrial",
                        "MihoPhase2_DemiHeavyArmorAlpha", "MihoPhase2_PSOSpecialNVGHelmet");
                    SpecialPawnEquipment.Arm(pawn, "MihoPhase2_SupernovaEMMarksmanRifle");
                    break;
                case "MihoPhase2_LeaderGuard":
                    SpecialPawnEquipment.Dress(pawn, "Miho_Apparel_Under_Industrial",
                        "MihoPhase2_DemiHeavyArmor", "MihoPhase2_PSOStandardNVGHelmet");
                    SpecialPawnEquipment.ArmNova(pawn);
                    break;
                case "MihoPhase2_OrbitalDropTrooper":
                    SpecialPawnEquipment.Dress(pawn, "Miho_Apparel_Under_Industrial",
                        "MihoPhase2_SupernovaDropArmor", "MihoPhase2_SupernovaDropHelmet");
                    SpecialPawnEquipment.ArmNova(pawn);
                    break;
            }
        }

        //职责：同步生理年龄与实际年龄，避免把指定年龄解释为冷冻年限。
        private static void SetAge(Pawn pawn, int years)
        {
            pawn.ageTracker.AgeBiologicalTicks = years * 3600000L;
            pawn.ageTracker.AgeChronologicalTicks = years * 3600000L;
        }
    }
}
