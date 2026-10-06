using RimWorld;

namespace MihoAdfqs.Phase2.Backgrounds
{
    //职责：显示随人物背景和离室计时变化的心情值。
    public class Thought_BackgroundMood : Thought_Situational
    {
        //职责：返回背景心情的当前实际数值。
        public override float MoodOffset() => ThoughtUtility.ThoughtNullified(pawn, def) ? 0f : ThoughtWorker_BackgroundMood.Mood(pawn);
    }
}
