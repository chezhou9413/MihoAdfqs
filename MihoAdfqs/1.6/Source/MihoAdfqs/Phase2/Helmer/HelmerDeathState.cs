namespace MihoAdfqs.Phase2.Helmer
{
    //职责：区分真正首次死亡和重复调用，并携带45号人物的恢复登记。
    public class HelmerDeathState
    {
        public bool wasAlive;
        public HelmerRecovery recovery;
    }
}
