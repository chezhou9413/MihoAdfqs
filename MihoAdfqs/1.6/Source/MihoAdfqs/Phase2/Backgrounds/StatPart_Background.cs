using System.Collections.Generic;
using RimWorld;
using Verse;

namespace MihoAdfqs.Phase2.Backgrounds
{
    //职责：将背景的非技能属性计入原版属性计算和信息页说明。
    public class StatPart_Background : StatPart
    {
        //职责：先合计加值再应用乘数，保留其它属性来源的原有结果。
        public override void TransformValue(StatRequest req, ref float val)
        {
            float offset = 0f;
            float factor = 1f;
            foreach (BackgroundEffects effect in BackgroundUtility.Effects(req.Thing as Pawn))
            {
                foreach (StatModifier item in effect.offsets) if (item.stat == parentStat) offset += item.value;
                foreach (StatModifier item in effect.factors) if (item.stat == parentStat) factor *= item.value;
            }
            val = (val + offset) * factor;
        }

        //职责：列出参与当前属性计算的背景加值和乘数。
        public override string ExplanationPart(StatRequest req)
        {
            Pawn pawn = req.Thing as Pawn;
            if (pawn?.story == null) return null;
            var lines = new List<string>();
            foreach (BackstoryDef story in pawn.story.AllBackstories)
            {
                BackgroundEffects effect = story?.GetModExtension<BackgroundEffects>();
                if (effect == null) continue;
                foreach (StatModifier item in effect.offsets) if (item.stat == parentStat) lines.Add(story.title + "：" + item.ValueToStringAsOffset);
                foreach (StatModifier item in effect.factors) if (item.stat == parentStat) lines.Add(story.title + "：×" + item.value.ToString("0.##"));
            }
            return lines.Count == 0 ? null : string.Join("\n", lines);
        }
    }
}
