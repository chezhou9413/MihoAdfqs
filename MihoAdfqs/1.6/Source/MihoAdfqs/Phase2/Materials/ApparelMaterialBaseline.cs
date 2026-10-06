using Verse;

namespace MihoAdfqs.Phase2.Materials
{
    //职责：声明装备需求数值对应的基准材料，便于不同材质按相对系数换算。
    public class ApparelMaterialBaseline : DefModExtension
    {
        public ThingDef material;
    }
}
