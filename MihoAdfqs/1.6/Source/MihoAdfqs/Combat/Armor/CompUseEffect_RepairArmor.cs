using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace MihoAdfqs.Combat.Armor
{
    //职责：使用插板后让操作者选择需要补充抵消值的装备。
    public class CompUseEffect_RepairArmor : CompUseEffect
    {
        //职责：列出操作者穿戴和持有的可修复装备，选定后消耗一块插板。
        public override void DoEffect(Pawn usedBy)
        {
            var options = new List<FloatMenuOption>();
            IEnumerable<ThingWithComps> items = usedBy.apparel.WornApparel.Cast<ThingWithComps>().Concat(usedBy.equipment.AllEquipmentListForReading);
            foreach (ThingWithComps item in items)
            {
                CompArmorReserve reserve = item.GetComp<CompArmorReserve>();
                if (reserve == null || reserve.reserve >= reserve.Settings.capacity) continue;
                options.Add(new FloatMenuOption($"修复{item.LabelCap}（{reserve.reserve:F0}/{reserve.Settings.capacity}）", () =>
                {
                    if (parent.Destroyed) return;
                    reserve.Repair();
                    parent.SplitOff(1).Destroy();
                }));
            }
            if (options.Count == 0) Messages.Message("没有需要修复抵消值的装备。", usedBy, MessageTypeDefOf.RejectInput, false);
            else Find.WindowStack.Add(new FloatMenu(options));
        }
    }
}
