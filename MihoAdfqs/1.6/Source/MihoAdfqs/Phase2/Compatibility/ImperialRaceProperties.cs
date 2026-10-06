using System.Collections.Generic;
using Verse;

namespace MihoAdfqs.Phase2.Compatibility
{
    //职责：记录帝国身体属性视图对应的原始种族属性，供种族身份查询使用。
    public static class ImperialRaceProperties
    {
        private static readonly Dictionary<RaceProperties, RaceProperties> Originals = new Dictionary<RaceProperties, RaceProperties>();

        //职责：在身体视图建立时登记来源，不修改共享种族定义。
        public static void Register(RaceProperties view, RaceProperties original) => Originals.Add(view, original);

        //职责：将已登记的帝国视图还原为原始属性，其余种族保持自身属性。
        public static RaceProperties OriginalFor(RaceProperties properties) =>
            Originals.TryGetValue(properties, out RaceProperties original) ? original : properties;
    }
}
