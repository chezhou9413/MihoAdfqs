using RimWorld;
using Verse;

namespace MihoAdfqs.MihoAdfDefRef
{
    //类职责：集中提供代码需要访问的本模组定义引用。
    [DefOf]
    public static class MihoDefRef
    {
        public static PawnKindDef Miho_adfqs;
        public static JobDef Miho_TalkToPawn;
        public static JobDef Miho_Brainwash;
        public static IncidentDef Incidents_MihoAdf;
        public static PawnKindDef Miho_SS;
        public static ThoughtDef Thought_BrainwashHappy;
        public static JobDef Miho_ToMyCareerFoPawn;
        public static XenotypeDef Xeno_MihoThirdEmpire;
        public static XenotypeDef Xeno_MihoPhase2_Fuhrer;
        public static FactionDef MihoThirdEmpire;
        public static PawnKindDef MihoPhase2_LeaderGuard;
        public static PawnKindDef MihoPhase2_LeaderGuardCaptain;
        public static GeneDef Gene_MihoPhase2_Experiment4;
        public static HediffDef MihoPhase2_PerkBody;
        public static HediffDef MihoPhase2_TianaInspiration;
        public static TraderKindDef MihoPhase2_Trader_SoapRecovery;
        public static TraderKindDef MihoPhase2_Trader_CropAssociation;
        public static TraderKindDef MihoPhase2_Trader_InterriverCommerce;

        //函数职责：确保游戏装载定义时初始化全部静态引用。
        static MihoDefRef()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(MihoDefRef));
        }
    }
}
