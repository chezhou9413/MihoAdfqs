using Verse;

namespace MihoAdfqs.Health
{
    //职责：为装备自动止血状态指定执行组件。
    public class HediffCompProperties_ApparelAutoTend : HediffCompProperties
    {
        //职责：关联自动包扎组件类型。
        public HediffCompProperties_ApparelAutoTend() => compClass = typeof(HediffComp_ApparelAutoTend);
    }
}
