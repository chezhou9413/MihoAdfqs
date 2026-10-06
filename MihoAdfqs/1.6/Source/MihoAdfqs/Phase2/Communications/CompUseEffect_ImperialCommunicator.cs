using MihoAdfqs.Phase2.Orbital;
using RimWorld;
using Verse;

namespace MihoAdfqs.Phase2.Communications
{
    //职责：使用通讯器时进入帝国通讯，并验证当前殖民地与联络权限。
    public class CompUseEffect_ImperialCommunicator : CompUseEffect
    {
        //职责：从实际使用物品的小人所在地图开启对话。
        public override void DoEffect(Pawn usedBy) => Find.WindowStack.Add(new Dialog_ImperialCommunications(usedBy.Map));

        //职责：在元首告知配方且使用者处于玩家殖民地时允许通讯。
        public override AcceptanceReport CanBeUsedBy(Pawn p)
        {
            if (!GameComponent_OrbitalNetwork.Current.unlocked) return "需要元首加入殖民地并告知联络方式。";
            if (p.Faction != Faction.OfPlayer || p.Map == null || !p.Map.IsPlayerHome) return "请在玩家殖民地使用通讯器。";
            return true;
        }
    }
}
