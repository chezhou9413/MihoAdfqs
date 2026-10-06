using RimWorld;
using Verse;
using Verse.AI.Group;

namespace MihoAdfqs.Phase2.Orbital
{
    //职责：让轨道步兵在殖民地防御四十八小时，随后自行离开地图。
    public class LordJob_OrbitalAssist : LordJob
    {
        private IntVec3 center;

        //职责：提供存档反序列化构造入口。
        public LordJob_OrbitalAssist() { }

        //职责：记录守卫时的集结位置。
        public LordJob_OrbitalAssist(IntVec3 center) { this.center = center; }

        //职责：建立搜敌防御与到期离开的状态转换。
        public override StateGraph CreateGraph()
        {
            var graph = new StateGraph();
            var defend = new LordToil_HuntEnemies(center);
            var leave = new LordToil_ExitMap();
            graph.AddToil(defend);
            graph.AddToil(leave);
            var expiry = new Transition(defend, leave);
            expiry.AddTrigger(new Trigger_TicksPassed(120000));
            expiry.AddPostAction(new TransitionAction_EndAllJobs());
            graph.AddTransition(expiry);
            return graph;
        }

        //职责：保存支援集结点。
        public override void ExposeData() => Scribe_Values.Look(ref center, "center");
    }
}
