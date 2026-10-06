using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using Verse;

namespace MihoAdfqs.Trader
{
    //类职责：让指定商人收购所有原本允许交易的物品，但不额外生成出售库存。
    public class StockGenerator_BuyAllTradeable : StockGenerator
    {
        //函数职责：保持该生成器只负责收购，不创建任何货物。
        public override IEnumerable<Thing> GenerateThings(PlanetTile forTile, Faction faction = null)
        {
            yield break;
        }

        //函数职责：接受所有具有原生交易许可和有效市场价值的物品定义。
        public override bool HandlesThingDef(ThingDef thingDef)
        {
            return thingDef != null && thingDef.tradeability != Tradeability.None && thingDef.BaseMarketValue > 0f;
        }

        //函数职责：把命中的物品标记为仅可卖给商人。
        public override Tradeability TradeabilityFor(ThingDef thingDef)
        {
            return HandlesThingDef(thingDef) ? Tradeability.Sellable : Tradeability.None;
        }
    }
}
