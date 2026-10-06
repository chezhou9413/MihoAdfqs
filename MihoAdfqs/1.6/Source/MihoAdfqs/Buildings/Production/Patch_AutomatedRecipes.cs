using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace MihoAdfqs.Buildings.Production
{
    //职责：限制每台自动设备同时仅有一批生产任务。
    [HarmonyPatch(typeof(Building_WorkTable), nameof(Building_WorkTable.CurrentlyUsableForBills))]
    public static class Patch_AutomatedAvailability
    {
        //职责：已有批次时拒绝接取下一张账单。
        public static void Postfix(Building_WorkTable __instance, ref bool __result)
        {
            if (__instance is Building_AutomatedProcessor processor && processor.Busy) __result = false;
        }
    }

    //职责：把自动设备的人工阶段限定为一百工作量。
    [HarmonyPatch(typeof(Bill), nameof(Bill.GetWorkAmount))]
    public static class Patch_AutomatedSetupWork
    {
        //职责：仅改变自动设备账单的人工工作量，不改变共享配方。
        public static void Postfix(Bill __instance, ref float __result)
        {
            if (__instance.billStack?.billGiver is Building_AutomatedProcessor) __result = 100;
        }
    }

    //职责：将原版已消耗材料的产物保存在自动设备内部，等加工完成再产出。
    [HarmonyPatch(typeof(GenRecipe), nameof(GenRecipe.MakeRecipeProducts))]
    public static class Patch_AutomatedProducts
    {
        //职责：延迟暴露成品，保留原版材料消耗和账单次数结算。
        public static IEnumerable<Thing> Postfix(IEnumerable<Thing> __result, RecipeDef recipeDef, Pawn worker, Thing dominantIngredient, IBillGiver billGiver)
        {
            if (billGiver is Building_AutomatedProcessor processor)
            {
                processor.BeginBatch(__result, recipeDef.WorkAmountTotal(dominantIngredient), worker);
                yield break;
            }
            foreach (Thing product in __result) yield return product;
        }
    }
}
