using System.Linq;
using RimWorld;
using Verse;

namespace MihoAdfqs.Phase2.Medicine
{
    //职责：种族转换后重新应用服装的穿戴健康效果，并保存不再适穿的物品。
    public static class ConversionApparel
    {
        //职责：重走脱穿回调，避免健康重置后医疗装备效果消失；不适穿衣物放入人物库存。
        public static void Refresh(Pawn pawn)
        {
            foreach (Apparel apparel in pawn.apparel.WornApparel.ToList())
            {
                bool locked = pawn.apparel.IsLocked(apparel);
                pawn.apparel.Remove(apparel);
                if (ApparelUtility.HasPartsToWear(pawn, apparel.def) && apparel.PawnCanWear(pawn, ignoreGender: true))
                    pawn.apparel.Wear(apparel, false, locked);
                else if (!pawn.inventory.innerContainer.TryAdd(apparel))
                    throw new System.InvalidOperationException("无法保存种族转换后的衣物：" + apparel);
            }
        }
    }
}
