using Verse;

namespace MihoAdfqs.Phase2.Compatibility
{
    //职责：集中提供可选种族模组的启用状态，控制依赖该种族的生成入口。
    public static class OptionalMods
    {
        //职责：与米莉拉条件加载目录使用同一个包标识判断功能是否可用。
        public static bool Milira => ModsConfig.IsActive("ancot.milirarace");
    }
}
