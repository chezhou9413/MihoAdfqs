using System.Collections.Generic;
using HarmonyLib;
using MihoAdfqs.MihoAdfDefRef;
using UnityEngine;
using Verse;

namespace MihoAdfqs.Health
{
    //在防护与承伤倍率结算后，对最终伤口执行佩尔克体每秒限伤。
    [HarmonyPatch(typeof(DamageWorker_AddInjury), "FinalizeAndAddInjury",
        new[] { typeof(Pawn), typeof(Hediff_Injury), typeof(DamageInfo), typeof(DamageWorker.DamageResult) })]
    public static class Patch_PerkInjuryDamage
    {
        private static readonly Dictionary<int, PerkDamageWindow> DamageWindows = new Dictionary<int, PerkDamageWindow>();
        private static int lastCleanupTick;

        //先预留本次伤口额度，避免伤口处理触发的嵌套伤害重复使用余额。
        public static bool Prefix(Pawn pawn, Hediff_Injury injury, ref float __result, out float __state)
        {
            __state = 0f;
            float limit = GetDamageLimit(pawn.health.hediffSet.GetFirstHediffOfDef(MihoDefRef.MihoPhase2_PerkBody));
            if (limit <= 0f) return true;
            int tick = Find.TickManager.TicksGame;
            CleanupExpiredWindows(tick);
            PerkDamageWindow window = GetCurrentWindow(pawn.thingIDNumber, tick);
            injury.Severity = Mathf.Min(injury.Severity, Mathf.Max(0f, limit - window.acceptedDamage));
            if (injury.Severity <= 0f)
            {
                __result = 0f;
                return false;
            }
            __state = injury.Severity;
            window.acceptedDamage += __state;
            return true;
        }

        //按原版最终报告的实际伤害计数，释放部位血量和即死保护未使用的额度。
        public static void Postfix(Pawn pawn, float __result, float __state)
        {
            if (__state <= 0f) return;
            DamageWindows[pawn.thingIDNumber].acceptedDamage += __result - __state;
        }

        //按成长阶段提供每秒三十、二十或十五点的实际伤害上限。
        private static float GetDamageLimit(Hediff perkBody)
        {
            if (perkBody == null || perkBody.Severity < 0.31f) return 0f;
            if (perkBody.Severity < 0.71f) return 30f;
            return perkBody.Severity < 1f ? 20f : 15f;
        }

        //从第一次受伤开始计时，经过六十刻后开启下一个伤害窗口。
        private static PerkDamageWindow GetCurrentWindow(int pawnId, int tick)
        {
            if (!DamageWindows.TryGetValue(pawnId, out PerkDamageWindow window))
            {
                window = new PerkDamageWindow { startTick = tick };
                DamageWindows.Add(pawnId, window);
            }
            if (tick < window.startTick || tick - window.startTick >= 60)
            {
                window.startTick = tick;
                window.acceptedDamage = 0f;
            }
            return window;
        }

        //移除长期没有受伤的记录。
        private static void CleanupExpiredWindows(int tick)
        {
            if (tick >= lastCleanupTick && tick - lastCleanupTick < 2500) return;
            lastCleanupTick = tick;
            var expired = new List<int>();
            foreach (KeyValuePair<int, PerkDamageWindow> pair in DamageWindows)
                if (tick < pair.Value.startTick || tick - pair.Value.startTick > 60000) expired.Add(pair.Key);
            foreach (int id in expired) DamageWindows.Remove(id);
        }
    }
}
