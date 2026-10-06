using System.Collections.Generic;
using RimWorld;
using Verse;

namespace MihoAdfqs.Phase2.Backgrounds
{
    //职责：只为实际被二期背景引用的属性安装计算部件。
    [StaticConstructorOnStartup]
    public static class BackgroundStartup
    {
        //职责：收集属性引用，并保留原有属性部件顺序与功能。
        static BackgroundStartup()
        {
            var stats = new HashSet<StatDef>();
            foreach (BackstoryDef story in DefDatabase<BackstoryDef>.AllDefsListForReading)
            {
                BackgroundEffects effect = story.GetModExtension<BackgroundEffects>();
                if (effect == null) continue;
                foreach (StatModifier item in effect.offsets) stats.Add(item.stat);
                foreach (StatModifier item in effect.factors) stats.Add(item.stat);
            }
            foreach (StatDef stat in stats)
            {
                if (stat.parts == null) stat.parts = new List<StatPart>();
                stat.parts.Add(new StatPart_Background { parentStat = stat });
            }
        }
    }
}
