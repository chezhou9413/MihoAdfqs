using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace MihoAdfqs.Buildings.Production
{
    //职责：接收人工装料后的生产批次，通电加工完成后释放标准品质成品。
    public class Building_AutomatedProcessor : Building_WorkTable, IThingHolder
    {
        private ThingOwner products;
        private float remainingTicks;
        private float totalTicks;
        private Pawn worker;
        public bool Busy => products.Count > 0;
        public bool IsBarrel => def.defName == "MihoPhase2_ImperialBeerBarrel";
        public string PauseReason => GetComp<CompBreakdownable>()?.BrokenDown == true ? "设备故障，请安排维修"
            : IsBarrel ? (AmbientTemperature < 29 || AmbientTemperature > 31 ? $"环境{AmbientTemperature:F1}℃，需要29～31℃" : null)
            : GetComp<CompPowerTrader>().PowerOn && FlickUtility.WantsToBeOn(this) ? null : "未通电或开关已关闭";

        //职责：建立保存生产中成品的内部容器。
        public Building_AutomatedProcessor() { products = new ThingOwner<Thing>(this); }

        //职责：接收唯一生产批次，按2400工作量一天换算自动加工耗时。
        public void BeginBatch(IEnumerable<Thing> batch, float work, Pawn maker)
        {
            if (Busy) throw new System.InvalidOperationException("自动设备已有生产批次");
            foreach (Thing product in batch) products.TryAdd(product);
            totalTicks = remainingTicks = IsBarrel ? 1800000 : Mathf.Max(1, work * 25f);
            worker = maker;
        }

        //职责：供电和设备状态正常时推进生产；酒桶只在约三十度环境发酵。
        protected override void Tick()
        {
            base.Tick();
            if (!Busy || GetComp<CompBreakdownable>()?.BrokenDown == true) return;
            if (IsBarrel)
            {
                if (AmbientTemperature < 29 || AmbientTemperature > 31) return;
            }
            else if (!GetComp<CompPowerTrader>().PowerOn || !FlickUtility.WantsToBeOn(this)) return;
            if (--remainingTicks > 0) return;
            var completed = new List<Thing>();
            foreach (Thing product in products)
            {
                product.TryGetComp<CompQuality>()?.SetQuality(QualityCategory.Normal, ArtGenerationContext.Colony);
                completed.Add(product);
            }
            products.TryDropAll(InteractionCell, Map, ThingPlaceMode.Near);
            if (worker != null) Find.QuestManager.Notify_ThingsProduced(worker, completed);
            worker = null;
        }

        //选中建筑即可查看批次进度、暂停原因和使用入口。
        public override string GetInspectString() => base.GetInspectString()
            + (Busy ? $"\n自动加工：{1f - remainingTicks / totalTicks:P0}（剩余{remainingTicks / 60000f:F2}天）"
                + (PauseReason == null ? "" : "\n加工暂停：" + PauseReason) : "\n等待人工装料：在账单页安排订单（100工作量）")
            + (IsBarrel ? "\n发酵温度：29～31℃，累计30天" : "");

        //职责：返回内部生产容器供存档与持有物遍历使用。
        public ThingOwner GetDirectlyHeldThings() => products;

        //职责：登记容器内部仍在生产的物品持有者。
        public void GetChildHolders(List<IThingHolder> outChildren) => ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, products);

        //职责：保存批次成品、操作人员与剩余加工时间。
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look(ref products, "products", this);
            Scribe_Values.Look(ref remainingTicks, "remainingTicks");
            Scribe_Values.Look(ref totalTicks, "totalTicks");
            Scribe_References.Look(ref worker, "worker");
        }

        //职责：设备销毁时销毁未加工完成的内容，避免提前获得成品。
        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            products.ClearAndDestroyContents();
            base.Destroy(mode);
        }
    }
}
