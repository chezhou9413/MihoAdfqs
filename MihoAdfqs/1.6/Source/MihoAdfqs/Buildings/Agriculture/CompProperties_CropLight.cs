using Verse;

namespace MihoAdfqs.Buildings.Agriculture
{
    //职责：配置农用灯的方形覆盖或培养机自身占地照明。
    public class CompProperties_CropLight : CompProperties
    {
        public int width = 13;
        public bool occupiedOnly;

        //职责：指定执行农作物照明的建筑组件。
        public CompProperties_CropLight() => compClass = typeof(Comp_CropLight);
    }
}
