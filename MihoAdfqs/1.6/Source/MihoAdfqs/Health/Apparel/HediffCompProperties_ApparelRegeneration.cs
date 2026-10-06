using Verse;

namespace MihoAdfqs.Health
{
    //职责：配置轨道装备每秒对单个部位的恢复量。
    public class HediffCompProperties_ApparelRegeneration : HediffCompProperties
    {
        public float healPerBodyPartPerSecond = 2f;

        //职责：关联包扎与再生组件类型。
        public HediffCompProperties_ApparelRegeneration() => compClass = typeof(HediffComp_ApparelRegeneration);
    }
}
