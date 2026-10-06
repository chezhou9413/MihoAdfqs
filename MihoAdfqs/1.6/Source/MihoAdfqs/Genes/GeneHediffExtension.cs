using Verse;

namespace MihoAdfqs.Genes
{
    //类职责：声明基因授予健康状态时需要读取的配置数据。
    public class GeneHediffExtension : DefModExtension
    {
        public HediffDef hediffDef;
        public float initialSeverity = 0.15f;
    }
}
