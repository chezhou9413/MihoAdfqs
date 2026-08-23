using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Reflection;
using Verse;
using MihoAdfqs.MihoAdfDefRef;

namespace MihoAdfqs.Patches
{
    /// <summary>
    /// 让特殊kind的小人永远不会死亡
    /// 拦截多个死亡相关的方法，确保无论如何都不会死
    /// </summary>
    
    /// <summary>
    /// 拦截ShouldBeDead方法，让特殊kind永远不应该死亡
    /// </summary>
    [HarmonyPatch(typeof(Pawn_HealthTracker), "ShouldBeDead")]
    public static class Patch_ShouldBeDead
    {
        private static FieldInfo pawnField = null;

        [HarmonyPostfix]
        public static void Postfix(Pawn_HealthTracker __instance, ref bool __result)
        {
            // 使用反射获取私有pawn字段
            if (pawnField == null)
            {
                pawnField = typeof(Pawn_HealthTracker).GetField("pawn", BindingFlags.NonPublic | BindingFlags.Instance);
            }

            if (pawnField == null) return;

            // 获取Pawn对象
            Pawn pawn = pawnField.GetValue(__instance) as Pawn;
            
            // 检查是否为特殊kind
            if (pawn?.kindDef == MihoDefRef.Miho_adfqs)
            {
                // 强制返回false，永远不应该死亡
                __result = false;
            }
        }
    }

    /// <summary>
    /// 拦截Kill方法，阻止特殊kind被杀死
    /// </summary>
    [HarmonyPatch(typeof(Pawn), "Kill")]
    public static class Patch_Pawn_Kill
    {
        [HarmonyPrefix]
        public static bool Prefix(Pawn __instance, DamageInfo? dinfo, Hediff exactCulprit = null)
        {
            // 检查是否为特殊kind
            if (__instance?.kindDef == MihoDefRef.Miho_adfqs)
            {
                // 阻止死亡，但是如果受伤严重就让其倒地
                if (!__instance.Downed && __instance.health.ShouldBeDowned())
                {
                    // 使用反射调用私有方法MakeDowned
                    MethodInfo makeDowned = typeof(Pawn_HealthTracker).GetMethod(
                        "MakeDowned", 
                        BindingFlags.NonPublic | BindingFlags.Instance
                    );
                    
                    if (makeDowned != null)
                    {
                        makeDowned.Invoke(__instance.health, new object[] { dinfo, exactCulprit });
                    }
                }
                
                // 治疗致命伤害，确保不会真的死
                HealLethalInjuries(__instance);
                
                // 显示提示
                if (__instance.Spawned)
                {
                    MoteMaker.ThrowText(__instance.DrawPos, __instance.Map, "不死之身！", 8f);
                }
                
                // 阻止原版Kill方法执行
                return false;
            }
            
            // 其他小人正常死亡
            return true;
        }

        /// <summary>
        /// 治疗致命伤害，防止死亡
        /// </summary>
        private static void HealLethalInjuries(Pawn pawn)
        {
            if (pawn?.health?.hediffSet == null) return;

            // 获取所有会导致死亡的hediff
            List<Hediff> hediffsToRemove = new List<Hediff>();
            
            foreach (Hediff hediff in pawn.health.hediffSet.hediffs)
            {
                // 检查是否会导致立即死亡
                if (hediff.CauseDeathNow())
                {
                    hediffsToRemove.Add(hediff);
                }
                
                // 检查是否是致命的缺失部位（如大脑、心脏等）
                if (hediff is Hediff_MissingPart missingPart)
                {
                    BodyPartRecord part = missingPart.Part;
                    if (part != null && IsVitalPart(pawn, part))
                    {
                        hediffsToRemove.Add(hediff);
                    }
                }
            }

            // 移除致命hediff
            foreach (Hediff hediff in hediffsToRemove)
            {
                pawn.health.RemoveHediff(hediff);
            }

            // 确保核心部位效率不为0
            if (pawn.health.hediffSet != null && pawn.RaceProps?.body?.corePart != null)
            {
                float coreEfficiency = PawnCapacityUtility.CalculatePartEfficiency(
                    pawn.health.hediffSet, 
                    pawn.RaceProps.body.corePart
                );
                
                if (coreEfficiency <= 0.0001f)
                {
                    // 移除所有影响核心部位的hediff
                    List<Hediff> coreHediffs = new List<Hediff>();
                    foreach (Hediff hediff in pawn.health.hediffSet.hediffs)
                    {
                        if (hediff.Part == pawn.RaceProps.body.corePart)
                        {
                            coreHediffs.Add(hediff);
                        }
                    }
                    
                    foreach (Hediff hediff in coreHediffs)
                    {
                        pawn.health.RemoveHediff(hediff);
                    }
                }
            }

            // 恢复致命能力
            RestoreVitalCapacities(pawn);
        }

        /// <summary>
        /// 检查是否是致命部位
        /// </summary>
        private static bool IsVitalPart(Pawn pawn, BodyPartRecord part)
        {
            if (part == null) return false;

            // 检查是否是大脑、心脏等致命部位
            if (part.def.tags != null)
            {
                if (part.def.tags.Contains(BodyPartTagDefOf.ConsciousnessSource) ||
                    part.def.tags.Contains(BodyPartTagDefOf.BreathingSource) ||
                    part.def.tags.Contains(BodyPartTagDefOf.BloodPumpingSource))
                {
                    return true;
                }
            }

            // 检查是否是核心部位
            if (part == pawn.RaceProps?.body?.corePart)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 恢复致命能力（意识、呼吸、血液循环等）
        /// </summary>
        private static void RestoreVitalCapacities(Pawn pawn)
        {
            if (pawn?.health?.capacities == null) return;

            // 检查致命能力
            List<PawnCapacityDef> allCapacities = DefDatabase<PawnCapacityDef>.AllDefsListForReading;
            
            foreach (PawnCapacityDef capacity in allCapacities)
            {
                bool isLethal = pawn.RaceProps.IsFlesh ? capacity.lethalFlesh : capacity.lethalMechanoids;
                
                if (isLethal && !pawn.health.capacities.CapableOf(capacity))
                {
                    // 找到影响这个能力的所有hediff并移除
                    List<Hediff> affectingHediffs = new List<Hediff>();
                    
                    foreach (Hediff hediff in pawn.health.hediffSet.hediffs)
                    {
                        // 检查这个hediff是否影响该能力
                        if (hediff.Part != null && AffectsCapacity(hediff.Part, capacity))
                        {
                            affectingHediffs.Add(hediff);
                        }
                    }

                    // 移除部分影响该能力的hediff，直到能力恢复
                    foreach (Hediff hediff in affectingHediffs)
                    {
                        pawn.health.RemoveHediff(hediff);
                        
                        // 重新检查能力是否恢复
                        if (pawn.health.capacities.CapableOf(capacity))
                        {
                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 检查身体部位是否影响某个能力
        /// </summary>
        private static bool AffectsCapacity(BodyPartRecord part, PawnCapacityDef capacity)
        {
            if (part?.def?.tags == null) return false;

            // 根据能力类型检查
            if (capacity == PawnCapacityDefOf.Consciousness)
            {
                return part.def.tags.Contains(BodyPartTagDefOf.ConsciousnessSource);
            }
            else if (capacity == PawnCapacityDefOf.BloodPumping)
            {
                return part.def.tags.Contains(BodyPartTagDefOf.BloodPumpingSource);
            }
            else if (capacity == PawnCapacityDefOf.Breathing)
            {
                return part.def.tags.Contains(BodyPartTagDefOf.BreathingSource);
            }

            return false;
        }
    }

    /// <summary>
    /// 拦截CheckForStateChange，在检查状态变化时保护特殊kind
    /// </summary>
    [HarmonyPatch(typeof(Pawn_HealthTracker), "CheckForStateChange")]
    public static class Patch_CheckForStateChange
    {
        private static FieldInfo pawnField = null;

        [HarmonyPrefix]
        public static void Prefix(Pawn_HealthTracker __instance)
        {
            // 使用反射获取私有pawn字段
            if (pawnField == null)
            {
                pawnField = typeof(Pawn_HealthTracker).GetField("pawn", BindingFlags.NonPublic | BindingFlags.Instance);
            }

            if (pawnField == null) return;

            Pawn pawn = pawnField.GetValue(__instance) as Pawn;
            
            // 如果是特殊kind且即将死亡，先治疗
            if (pawn?.kindDef == MihoDefRef.Miho_adfqs)
            {
                // 检查是否有致命伤害
                if (__instance.hediffSet != null)
                {
                    List<Hediff> lethalHediffs = new List<Hediff>();
                    
                    foreach (Hediff hediff in __instance.hediffSet.hediffs)
                    {
                        if (hediff.CauseDeathNow())
                        {
                            lethalHediffs.Add(hediff);
                        }
                    }

                    // 移除致命hediff
                    foreach (Hediff hediff in lethalHediffs)
                    {
                        __instance.RemoveHediff(hediff);
                    }
                }
            }
        }
    }

    /// <summary>
    /// 拦截ShouldBeDeadFromRequiredCapacity，让特殊kind不会因为失去致命能力而死亡
    /// </summary>
    [HarmonyPatch(typeof(Pawn_HealthTracker), "ShouldBeDeadFromRequiredCapacity")]
    public static class Patch_ShouldBeDeadFromRequiredCapacity
    {
        private static FieldInfo pawnField = null;

        [HarmonyPostfix]
        public static void Postfix(Pawn_HealthTracker __instance, ref PawnCapacityDef __result)
        {
            // 使用反射获取私有pawn字段
            if (pawnField == null)
            {
                pawnField = typeof(Pawn_HealthTracker).GetField("pawn", BindingFlags.NonPublic | BindingFlags.Instance);
            }

            if (pawnField == null) return;

            Pawn pawn = pawnField.GetValue(__instance) as Pawn;
            
            // 如果是特殊kind，永远不会因为失去能力而死亡
            if (pawn?.kindDef == MihoDefRef.Miho_adfqs)
            {
                __result = null;
            }
        }
    }

    /// <summary>
    /// 拦截ShouldBeDeadFromLethalDamageThreshold，让特殊kind不会因为伤害过高而死亡
    /// </summary>
    [HarmonyPatch(typeof(Pawn_HealthTracker), "ShouldBeDeadFromLethalDamageThreshold")]
    public static class Patch_ShouldBeDeadFromLethalDamageThreshold
    {
        private static FieldInfo pawnField = null;

        [HarmonyPostfix]
        public static void Postfix(Pawn_HealthTracker __instance, ref bool __result)
        {
            // 使用反射获取私有pawn字段
            if (pawnField == null)
            {
                pawnField = typeof(Pawn_HealthTracker).GetField("pawn", BindingFlags.NonPublic | BindingFlags.Instance);
            }

            if (pawnField == null) return;

            Pawn pawn = pawnField.GetValue(__instance) as Pawn;
            
            // 如果是特殊kind，永远不会因为伤害阈值而死亡
            if (pawn?.kindDef == MihoDefRef.Miho_adfqs)
            {
                __result = false;
            }
        }
    }
}

