using Verse;

namespace MihoAdfqs.Phase2.CombatAI
{
    //地图持有人物引用，战备额度和冷却在翻滚、空投及读档期间保留。
    public sealed class ImperialCombatPawnState : IExposable
    {
        public Pawn pawn;
        public int charges = 3;
        public int nextCallTick;
        public int nextRollTick;

        //保存实际消耗和冷却终点，不在落地时重新初始化。
        public void ExposeData()
        {
            Scribe_References.Look(ref pawn, "pawn", saveDestroyedThings: true);
            Scribe_Values.Look(ref charges, "charges", 3);
            Scribe_Values.Look(ref nextCallTick, "nextCallTick");
            Scribe_Values.Look(ref nextRollTick, "nextRollTick");
        }
    }
}
