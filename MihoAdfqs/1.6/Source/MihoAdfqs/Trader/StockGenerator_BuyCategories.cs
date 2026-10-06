using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using Verse;

namespace MihoAdfqs.Trader
{
    //类职责：让商人只收购 XML 列出的物品分类及其全部子分类。
    public class StockGenerator_BuyCategories : StockGenerator
    {
        public List<ThingCategoryDef> categories = new List<ThingCategoryDef>();

        //函数职责：保持该生成器只负责收购，不创建任何货物。
        public override IEnumerable<Thing> GenerateThings(PlanetTile forTile, Faction faction = null)
        {
            yield break;
        }

        //函数职责：判断物品是否属于任一允许收购的分类。
        public override bool HandlesThingDef(ThingDef thingDef)
        {
            if (thingDef == null || thingDef.tradeability == Tradeability.None)
            {
                return false;
            }

            for (int i = 0; i < categories.Count; i++)
            {
                if (categories[i] != null && thingDef.IsWithinCategory(categories[i]))
                {
                    return true;
                }
            }

            return false;
        }

        //函数职责：把命中的分类物品标记为仅可卖给商人。
        public override Tradeability TradeabilityFor(ThingDef thingDef)
        {
            return HandlesThingDef(thingDef) ? Tradeability.Sellable : Tradeability.None;
        }

        //函数职责：在定义校验阶段报告空分类配置。
        public override IEnumerable<string> ConfigErrors(TraderKindDef parentDef)
        {
            foreach (string error in base.ConfigErrors(parentDef))
            {
                yield return error;
            }

            if (categories == null || categories.Count == 0)
            {
                yield return parentDef.defName + " 的分类收购生成器没有配置 categories。";
            }
        }
    }
}
