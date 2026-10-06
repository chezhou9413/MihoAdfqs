using RimWorld;
using Verse;
using Verse.AI.Group;

namespace MihoAdfqs.Phase2.DebugTools
{
    //让测试部队持续搜敌交战，不使用普通袭击的抢劫或限时撤退逻辑。
    public sealed class LordJob_ImperialDebugBattle : LordJob
    {
        private IntVec3 center;

        //提供存档读取入口。
        public LordJob_ImperialDebugBattle() { }

        //记录战场中心，作为暂无敌人时的集结位置。
        public LordJob_ImperialDebugBattle(IntVec3 center) { this.center = center; }

        //复用原版独立搜敌职责，让射击、走位与换弹仍走正常战斗逻辑。
        public override StateGraph CreateGraph()
        {
            var graph = new StateGraph();
            graph.AddToil(new LordToil_HuntEnemies(center));
            return graph;
        }

        //保存战场中心，使读取测试场存档后继续交战。
        public override void ExposeData() => Scribe_Values.Look(ref center, "center");
    }
}
