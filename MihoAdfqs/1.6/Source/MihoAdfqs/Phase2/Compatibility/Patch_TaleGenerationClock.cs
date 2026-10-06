using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

namespace MihoAdfqs.Phase2.Compatibility
{
    //职责：让故事记录在开局配置和正式游戏中均使用对应阶段的绝对时间。
    [HarmonyPatch(typeof(TaleFactory), nameof(TaleFactory.MakeRawTale))]
    public static class Patch_TaleGenerationClock
    {
        //职责：将故事工厂直接读取游戏时钟的调用替换为原版支持开局阶段的时间入口。
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var tickGetter = AccessTools.PropertyGetter(typeof(TickManager), nameof(TickManager.TicksAbs));
            var generationTickGetter = AccessTools.PropertyGetter(typeof(GenTicks), nameof(GenTicks.TicksAbs));
            int replacements = 0;
            foreach (CodeInstruction instruction in instructions)
            {
                if (!instruction.Calls(tickGetter))
                {
                    yield return instruction;
                    continue;
                }
                //移除实例属性调用原本消耗的时钟对象，再压入静态时间属性的结果。
                yield return new CodeInstruction(OpCodes.Pop).MoveLabelsFrom(instruction).MoveBlocksFrom(instruction);
                yield return new CodeInstruction(OpCodes.Call, generationTickGetter);
                replacements++;
            }
            if (replacements != 1)
                throw new InvalidOperationException($"故事工厂绝对时间调用数量异常：{replacements}，预期为1。");
        }
    }
}
