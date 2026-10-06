namespace MihoAdfqs.Combat.Weapons
{
    //保存一次真实扣弹的槽位和界面时间，避免Gizmo每帧重建丢失动画。
    public readonly struct MagazineShotVisual
    {
        public const float Duration = 0.28f;
        public readonly int roundIndex;
        public readonly float startedAt;

        //弹匣从右向左消耗，扣弹后的余量就是被消耗的槽位。
        public MagazineShotVisual(int roundIndex, float startedAt)
        {
            this.roundIndex = roundIndex;
            this.startedAt = startedAt;
        }
    }
}
