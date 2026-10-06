using System.Collections.Generic;
using Verse;

namespace MihoAdfqs.Buildings.Defense
{
    //临时战备堡垒使用随舱弹药和独立撤收时间。
    public sealed class Building_CombatFortress : Building_ImperialFortress
    {
        private int withdrawTick;

        //首次落地时装填首仓，读档保留原库存和撤收时间。
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            if (respawningAfterLoad) return;
            LoadDropMagazine();
            withdrawTick = Find.TickManager.TicksGame + 7200;
        }

        //到期直接撤收，不产生拆解材料或爆炸。
        protected override void Tick()
        {
            if (Find.TickManager.TicksGame >= withdrawTick) { Destroy(DestroyMode.Vanish); return; }
            base.Tick();
        }

        //战备堡垒不提供换炮、搬迁或玩家操作按钮。
        public override IEnumerable<Gizmo> GetGizmos() { yield break; }

        //列出总弹药与剩余驻留秒数。
        public override string GetInspectString() => base.GetInspectString()
            + $"\n撤收：{System.Math.Max(0, withdrawTick - Find.TickManager.TicksGame) / 60f:F1}秒";

        //保存已经开始的驻留计时。
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref withdrawTick, "combatWithdrawTick");
        }
    }
}
