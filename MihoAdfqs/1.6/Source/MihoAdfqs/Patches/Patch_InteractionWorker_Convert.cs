using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Verse;
using MihoAdfqs.MihoAdfDefRef;

namespace MihoAdfqs.Patches
{
    /// <summary>
    /// 拦截教化交互，让特殊kind的小人教化必定成功
    /// </summary>
    [HarmonyPatch(typeof(InteractionWorker_ConvertIdeoAttempt), "Interacted")]
    public static class Patch_InteractionWorker_Convert_Interacted
    {
        /// <summary>
        /// 前置方法：检查发起者是否为特殊kind，如果是则强制教化成功
        /// </summary>
        [HarmonyPrefix]
        public static bool Prefix(
            Pawn initiator, 
            Pawn recipient, 
            List<RulePackDef> extraSentencePacks, 
            out string letterText, 
            out string letterLabel, 
            out LetterDef letterDef, 
            out LookTargets lookTargets)
        {
            // 初始化输出参数
            letterLabel = null;
            letterText = null;
            letterDef = null;
            lookTargets = null;

            // 检查发起者是否为特殊kind（Miho_adfqs）
            if (initiator?.kindDef != MihoDefRef.Miho_adfqs)
            {
                // 不是特殊kind，执行原版逻辑
                return true;
            }

            // 是特殊kind，强制教化成功
            if (recipient?.ideo == null || initiator?.Ideo == null)
            {
                return true; // 数据异常，执行原版逻辑
            }

            // 记录原始信仰和角色
            Ideo oldIdeo = recipient.Ideo;
            Precept_Role role = oldIdeo?.GetRole(recipient);

            // 使用反射设置私有的certaintyInt字段为0，强制转换成功
            FieldInfo certaintyField = typeof(Pawn_IdeoTracker).GetField("certaintyInt", BindingFlags.NonPublic | BindingFlags.Instance);
            if (certaintyField != null)
            {
                certaintyField.SetValue(recipient.ideo, 0f);
            }
            
            // 调用原版的转换方法，传入足够大的确定性降低值确保成功
            // 由于我们已经将certaintyInt设为0，这里传入任何正值都会成功
            bool conversionSuccess = recipient.ideo.IdeoConversionAttempt(10f, initiator.Ideo, applyCertaintyFactor: false);

            if (conversionSuccess)
            {
                // 生成成功消息
                if (PawnUtility.ShouldSendNotificationAbout(initiator) || PawnUtility.ShouldSendNotificationAbout(recipient))
                {
                    letterLabel = "LetterLabelConvertIdeoAttempt_Success".Translate();
                    letterText = "LetterConvertIdeoAttempt_Success".Translate(
                        initiator.Named("INITIATOR"), 
                        recipient.Named("RECIPIENT"), 
                        initiator.Ideo.Named("IDEO"), 
                        oldIdeo.Named("OLDIDEO")
                    ).Resolve();
                    letterDef = LetterDefOf.PositiveEvent;
                    lookTargets = new LookTargets(initiator, recipient);

                    // 如果失去了角色，添加额外信息
                    if (role != null)
                    {
                        letterText = letterText + "\n\n" + "LetterRoleLostLetterIdeoChangedPostfix".Translate(
                            recipient.Named("PAWN"), 
                            role.Named("ROLE"), 
                            oldIdeo.Named("OLDIDEO")
                        ).Resolve();
                    }
                }

                // 添加成功的对话规则包
                extraSentencePacks.Add(RulePackDefOf.Sentence_ConvertIdeoAttemptSuccess);

                // 显示特殊提示（可选）
                if (recipient.Spawned)
                {
                    MoteMaker.ThrowText(recipient.DrawPos, recipient.Map, "教化成功！", 8f);
                }
            }

            // 跳过原版方法
            return false;
        }
    }

    /// <summary>
    /// 可选：拦截CertaintyReduction方法，让特殊kind的教化力度更强
    /// </summary>
    [HarmonyPatch(typeof(InteractionWorker_ConvertIdeoAttempt), "CertaintyReduction")]
    public static class Patch_CertaintyReduction
    {
        /// <summary>
        /// 后置方法：如果发起者是特殊kind，返回超大的确定性降低值
        /// </summary>
        [HarmonyPostfix]
        public static void Postfix(Pawn initiator, Pawn recipient, ref float __result)
        {
            // 检查发起者是否为特殊kind
            if (initiator?.kindDef == MihoDefRef.Miho_adfqs)
            {
                // 返回一个足够大的值，确保能够完全降低目标的确定性
                __result = 10f; // 远大于1.0，确保必定成功
            }
        }
    }
}
