using HarmonyLib;
using MihoAdfqs.MihoAdfDefRef;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace MihoAdfqs.Health
{
    //类职责：在提亚娜灵感持续期间，将她制作的服装和武器固定为传奇品质。
    [HarmonyPatch(typeof(GenRecipe), nameof(GenRecipe.MakeRecipeProducts))]
    public static class Patch_TianaCrafting
    {
        //函数职责：用可延迟枚举的包装保留原版产物流程，并只改写服装和武器的品质。
        [HarmonyPostfix]
        public static void Postfix(Pawn worker, ref IEnumerable<Thing> __result)
        {
            if (worker?.health?.hediffSet?.GetFirstHediffOfDef(MihoDefRef.MihoPhase2_TianaInspiration, false) == null)
            {
                return;
            }

            __result = SetLegendaryQuality(__result);
        }

        //函数职责：逐个返回原版产物，并将其中带品质组件的服装或武器设为传奇。
        private static IEnumerable<Thing> SetLegendaryQuality(IEnumerable<Thing> products)
        {
            foreach (Thing product in products)
            {
                if (product?.def != null && (product.def.IsApparel || product.def.IsWeapon))
                {
                    product.TryGetComp<CompQuality>()?.SetQuality(QualityCategory.Legendary, null);
                }

                yield return product;
            }
        }
    }
}
