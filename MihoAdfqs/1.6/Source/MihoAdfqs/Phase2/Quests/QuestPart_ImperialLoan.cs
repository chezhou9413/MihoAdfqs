using RimWorld;
using Verse;

namespace MihoAdfqs.Phase2.Quests
{
    //职责：沿用原版离图借调管理，并在取消任务时归还已出发的殖民者。
    public class QuestPart_ImperialLoan : QuestPart_LendColonistsToFaction
    {
        //职责：清理未到期的借调，避免取消或更换任务后人员长期留在世界列表。
        public override void Cleanup()
        {
            if (State == QuestPartState.Enabled && LentColonistsListForReading.Count > 0)
                base.Complete(default(SignalArgs));
            base.Cleanup();
        }
    }
}
