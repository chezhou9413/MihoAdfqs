using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MihoAdfqs.Phase2.Helmer;
using Verse;

namespace MihoAdfqs.Phase2.Pawns
{
    //按原版基因列表实例保存引用，列表替换时自然失效，不缓存基因激活状态。
    [StaticConstructorOnStartup]
    internal static class ImperialGeneLookup
    {
        private static readonly GeneDef FirstStandard = DefDatabase<GeneDef>.GetNamed("Gene_MihoPhase2_FirstStandard");
        private static readonly GeneDef Experiment45 = DefDatabase<GeneDef>.GetNamed("Gene_MihoPhase2_Experiment45");
        private static readonly GeneDef Experiment4 = DefDatabase<GeneDef>.GetNamed("Gene_MihoPhase2_Experiment4");
        private static readonly GeneDef Fuhrer = DefDatabase<GeneDef>.GetNamed("Gene_MihoThirdEmpireFuhrer");
        private static readonly ConditionalWeakTable<List<Gene>, Entry> Cache = new ConditionalWeakTable<List<Gene>, Entry>();
        private static readonly ConditionalWeakTable<List<Gene>, Entry>.CreateValueCallback CreateEntry = Build;
        private static readonly Entry Empty = new Entry(null, null, null, null, Array.Empty<Gene_Helmer>());

        //原版增删基因及读档会重建GenesListForReading，旧列表随人物回收。
        internal static Entry For(Pawn pawn)
        {
            return pawn?.genes == null ? Empty : Cache.GetValue(pawn.genes.GenesListForReading, CreateEntry);
        }

        //只在列表首次出现时扫描，保持原版GetGene的首个匹配顺序。
        private static Entry Build(List<Gene> genes)
        {
            Gene firstStandard = null;
            Gene experiment45 = null;
            Gene experiment4 = null;
            Gene fuhrer = null;
            List<Gene_Helmer> helmers = null;
            for (int i = 0; i < genes.Count; i++)
            {
                Gene gene = genes[i];
                if (firstStandard == null && gene.def == FirstStandard) firstStandard = gene;
                if (experiment45 == null && gene.def == Experiment45) experiment45 = gene;
                if (experiment4 == null && gene.def == Experiment4) experiment4 = gene;
                if (fuhrer == null && gene.def == Fuhrer) fuhrer = gene;
                if (gene is Gene_Helmer helmer)
                {
                    if (helmers == null) helmers = new List<Gene_Helmer>();
                    helmers.Add(helmer);
                }
            }
            return firstStandard == null && experiment45 == null && experiment4 == null && fuhrer == null && helmers == null ? Empty
                : new Entry(firstStandard, experiment45, experiment4, fuhrer, helmers?.ToArray() ?? Array.Empty<Gene_Helmer>());
        }

        //保存只读引用；覆盖、年龄和变异导致的Active变化仍在查询当刻生效。
        internal sealed class Entry
        {
            private readonly Gene firstStandard;
            private readonly Gene experiment45;
            private readonly Gene experiment4;
            private readonly Gene fuhrer;
            private readonly Gene_Helmer[] helmers;

            //同一列表的部位血量与射程查询共用这组引用。
            internal Entry(Gene firstStandard, Gene experiment45, Gene experiment4, Gene fuhrer, Gene_Helmer[] helmers)
            {
                this.firstStandard = firstStandard;
                this.experiment45 = experiment45;
                this.experiment4 = experiment4;
                this.fuhrer = fuhrer;
                this.helmers = helmers;
            }

            internal float BodyHealthFactor => (firstStandard?.Active == true ? 2f : 1f)
                * (experiment45?.Active == true ? 3f : 1f);
            internal bool IsHelmer => experiment45?.Active == true;
            internal bool IsFuhrer => fuhrer?.Active == true;
            internal bool IsImmortal => firstStandard?.Active == true;
            internal bool IgnoresPawnCollision => experiment4?.Active == true || IsHelmer;

            //保留原有“首个生效的赫尔默基因”规则，姿态直接读取基因实例。
            internal Gene_Helmer ActiveHelmer
            {
                get
                {
                    for (int i = 0; i < helmers.Length; i++)
                        if (helmers[i].Active) return helmers[i];
                    return null;
                }
            }
        }
    }
}
