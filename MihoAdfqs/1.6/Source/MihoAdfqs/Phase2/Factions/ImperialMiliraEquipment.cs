using MihoAdfqs.Phase2.Pawns;
using Verse;

namespace MihoAdfqs.Phase2.Factions
{
    //区分帝国米莉拉的精致常服与战斗装备。
    public static class ImperialMiliraEquipment
    {
        private static readonly string[] Dresses =
        {
            "Milira_DelicateDressA", "Milira_DelicateDressA_II", "Milira_DelicateDressB", "Milira_DelicateDressC",
            "Milira_DelicateDressD", "Milira_DelicateDressE", "Milira_DelicateDressF", "Milira_DelicateDressG"
        };
        private static readonly string[] Armor = { "Milira_ValkyrArmor", "Milira_ArtemisArmor", "Milira_DaedalusArmor" };
        private static readonly string[] Helmets = { "Milira_ValkyrHelmet", "Milira_ArtemisHelmet", "Milira_DaedalusHelmet" };

        //普通帝国米莉拉随机穿精致服装，不携带武器。
        public static void Civilian(Pawn pawn)
        {
            SpecialPawnEquipment.Dress(pawn, Dresses.RandomElement());
            pawn.equipment.DestroyAllEquipment();
        }

        //等概率选择三套盔甲之一，并配套头盔和四选一超新星武器。
        public static void Raider(Pawn pawn)
        {
            int index = Rand.Range(0, Armor.Length);
            SpecialPawnEquipment.Dress(pawn, Armor[index], Helmets[index]);
            SpecialPawnEquipment.ArmNova(pawn);
        }
    }
}
