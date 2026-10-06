using System.Linq;
using RimWorld;
using Verse;

namespace MihoAdfqs.Phase2.Backgrounds
{
    //职责：在背景产生固定心情影响或离室超过十小时后激活背景心情。
    public class ThoughtWorker_BackgroundMood : ThoughtWorker
    {
        //职责：判断当前人物是否存在实际非零背景心情变化。
        protected override ThoughtState CurrentStateInternal(Pawn p) => Mood(p) != 0;

        //职责：合计固定背景心情，并计算死宅背景连续离室的额外惩罚。
        public static int Mood(Pawn pawn)
        {
            int result = BackgroundUtility.Effects(pawn).Sum(effect => effect.mood);
            if (pawn.GetComp<Comp_BackgroundExposure>()?.outdoorTicks > 25000) result -= 10;
            return result;
        }
    }
}
