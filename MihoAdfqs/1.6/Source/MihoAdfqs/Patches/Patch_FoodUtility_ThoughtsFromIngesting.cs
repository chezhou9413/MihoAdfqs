using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace MihoAdfqs.MihoAdfHarmony
{
    [HarmonyPatch(typeof(FoodUtility), "ThoughtsFromIngesting")]
    public static class Patch_FoodUtility_ThoughtsFromIngesting
    {
        public static void Postfix(Pawn ingester, Thing foodSource, ThingDef foodDef, List<FoodUtility.ThoughtFromIngesting> __result)
        {
            // 1. 判断是否为目标角色：仅限玩家阵营的殖民者/俘虏等（避免动物/NPC 触发）
            if (ingester == null || ingester.Faction != Faction.OfPlayer) return;

            // --- 辅助函数：通过字符串获取原版 ThoughtDef ---
            // 这样做是因为 ThoughtDefOf 里没有这些定义，但游戏数据库里肯定有
            ThoughtDef GetDef(string name) => (name.NullOrEmpty() ? null : DefDatabase<ThoughtDef>.GetNamedSilentFail(name));

            // 辅助：按 ThoughtDef 移除/添加（1.6 返回的是 List<FoodUtility.ThoughtFromIngesting>，不是 List<ThoughtDef>）
            // 注意：不能在本地函数/lambda 中捕获 ref 参数 __result，所以这里全部用普通 for 循环实现。
            void RemoveThought(ThoughtDef td)
            {
                if (td == null || __result == null) return;
                for (int i = __result.Count - 1; i >= 0; i--)
                {
                    if (__result[i].thought == td)
                    {
                        __result.RemoveAt(i);
                    }
                }
            }

            void AddThought(ThoughtDef td)
            {
                if (td == null || __result == null) return;
                for (int i = 0; i < __result.Count; i++)
                {
                    if (__result[i].thought == td) return;
                }
                __result.Add(new FoodUtility.ThoughtFromIngesting { thought = td, fromPrecept = null });
            }

            // 提前获取原版 Def 用于对比和移除
            // 注意：在你当前 1.6 Data 里，“营养膏”的 ThoughtDef 是 AteNutrientPasteMeal（AteNutrientPaste 是 HistoryEventDef，不是 ThoughtDef）
            ThoughtDef originalPaste = GetDef("AteNutrientPasteMeal");
            ThoughtDef originalFine = GetDef("AteFineMeal");
            ThoughtDef originalLavish = GetDef("AteLavishMeal");
            ThoughtDef originalHumanDirect = GetDef("AteHumanlikeMeatDirect");
            ThoughtDef originalHumanIngredient = GetDef("AteHumanlikeMeatAsIngredient");
            ThoughtDef originalHumanDirectCannibal = GetDef("AteHumanlikeMeatDirectCannibal");
            ThoughtDef originalHumanIngredientCannibal = GetDef("AteHumanlikeMeatAsIngredientCannibal");

            if (foodDef == null || __result == null) return;

            // --- 场景 A: 吃了营养膏 ---
            if (foodDef == ThingDefOf.MealNutrientPaste)
            {
                RemoveThought(originalPaste);
                AddThought(GetDef("Miho_AteNutrientPaste_Disgust"));
            }

            // --- 场景 B: 吃了精致食物 ---
            else if (foodDef.defName == "MealFine" || foodDef.defName == "MealFine_Veg" || foodDef.defName == "MealFine_Meat")
            {
                RemoveThought(originalFine);
                AddThought(GetDef("Miho_AteFineMeal_Critique"));
            }

            // --- 场景 C: 吃了奢侈食物 ---
            else if (foodDef.defName == "MealLavish" || foodDef.defName == "MealLavish_Veg" || foodDef.defName == "MealLavish_Meat")
            {
                RemoveThought(originalLavish);
                AddThought(GetDef("Miho_AteLavishMeal_Praise"));
            }

            // --- 场景 D: 吃了人肉 ---
            // 替代 FoodUtility.IsHumanlikeMeat 的手动判断逻辑
            bool isHumanMeat = IsHumanMeat(foodDef);
            bool hasHumanMeatIngredient = (foodSource != null && IsHumanMeat(foodSource.def));

            // 有些食物本身不是肉，但成分表里有人肉 (CompIngredients)
            if (!hasHumanMeatIngredient && foodSource != null)
            {
                CompIngredients ingredients = foodSource.TryGetComp<CompIngredients>();
                if (ingredients != null)
                {
                    foreach (ThingDef ingredient in ingredients.ingredients)
                    {
                        if (IsHumanMeat(ingredient))
                        {
                            hasHumanMeatIngredient = true;
                            break;
                        }
                    }
                }
            }

            if (isHumanMeat || hasHumanMeatIngredient)
            {
                // 移除原版所有可能的人肉相关想法
                RemoveThought(originalHumanDirect);
                RemoveThought(originalHumanIngredient);
                RemoveThought(originalHumanDirectCannibal);
                RemoveThought(originalHumanIngredientCannibal);

                // 添加自定义想法 (防止重复添加)
                AddThought(GetDef("Miho_AteHumanMeat_Acceptance"));
            }
        }

        // --- 手动实现判断人肉的逻辑 ---
        private static bool IsHumanMeat(ThingDef def)
        {
            // 逻辑：必须是肉类 且 来源种族是类人生物
            return def != null &&
                   def.IsMeat &&
                   def.ingestible != null &&
                   def.ingestible.sourceDef != null &&
                   def.ingestible.sourceDef.race != null &&
                   def.ingestible.sourceDef.race.Humanlike;
        }
    }
}