using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace MihoAdfqs.Combat.Muzzles
{
    //把本模组喷火器的火流接到枪口库，保留原版伤害结算和现有燃料补丁。
    [HarmonyPatch(typeof(Verb_ArcSprayIncinerator), "TryCastShot")]
    public static class Patch_IncineratorMuzzle
    {
        //仅替换火流入队入口，传入发射动词以查询当前装备的枪口。
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo add = AccessTools.Method(typeof(IncineratorSpray), nameof(IncineratorSpray.Add));
            MethodInfo replacement = AccessTools.Method(typeof(Patch_IncineratorMuzzle), nameof(AddAtMuzzle));
            int matches = 0;
            foreach (CodeInstruction instruction in instructions)
            {
                if (!instruction.Calls(add))
                {
                    yield return instruction;
                    continue;
                }
                //保留原调用入口的分支和异常块元数据，再补充动词参数。
                var loadVerb = new CodeInstruction(OpCodes.Ldarg_0);
                loadVerb.MoveLabelsFrom(instruction);
                loadVerb.MoveBlocksFrom(instruction);
                yield return loadVerb;
                yield return new CodeInstruction(OpCodes.Call, replacement);
                matches++;
            }
            if (matches != 1) throw new InvalidOperationException("喷火枪口接入失败：原版火流入队入口数量不为一。");
        }

        //按真实枪口重算火流方向与飞行时长，并让视觉终点服从原版墙体截断。
        private static void AddAtMuzzle(IncineratorSpray spray, IncineratorProjectileMotion motion,
            Verb_ArcSprayIncinerator verb)
        {
            if (verb.EquipmentSource?.TryGetComp<MihoAdfComp.ThingComp_FueledSprayer>() != null
                && IncineratorMuzzleUtility.TryGetRay(verb, motion.worldTarget, out Vector3 origin, out Vector3 end))
            {
                motion.targetDest = end.ToIntVec3();
                motion.worldTarget = end;
                motion.worldSource = origin;
                Vector3 delta = motion.worldTarget - motion.worldSource;
                motion.moveVector = delta.normalized;
                motion.lifespanTicks = Mathf.Max(1,
                    Mathf.FloorToInt(delta.magnitude * Verb_ArcSprayIncinerator.DistanceToLifetimeScalar));
                Patch_IncineratorMoteMotion.Register(motion, verb.Caster.Map);
            }
            spray.Add(motion);
        }
    }
}
