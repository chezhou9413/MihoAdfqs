namespace MihoAdfqs.Health
{
    //记录当前一秒内实际承受的伤害，以及正在生成伤口的预留额度。
    internal sealed class PerkDamageWindow
    {
        public int startTick;
        public float acceptedDamage;
    }
}
