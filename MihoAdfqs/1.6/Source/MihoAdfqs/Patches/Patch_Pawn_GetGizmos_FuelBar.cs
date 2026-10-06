using HarmonyLib;
using MihoAdfqs.MihoAdfComp;
using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using MihoAdfqs.Phase2.UI;
using Verse;
using Verse.AI;

namespace MihoAdfqs.Patches
{
    //职责：为携带燃料武器的殖民者添加手动装填菜单。
    [HarmonyPatch(typeof(FloatMenuMakerMap), "GetOptions")]
    public static class Patch_FloatMenu_Reload
    {
        //职责：在原版菜单中追加能够到达的燃料装填选项。
        public static void Postfix(List<Pawn> selectedPawns, Vector3 clickPos, ref List<FloatMenuOption> __result)
        {
            //1. 安全检查
            if (selectedPawns == null || __result == null) return;

            Map map = Find.CurrentMap;
            if (map == null) return;
            if (!clickPos.InBounds(map)) return;

            //2. 获取点击位置的物品列表
            IntVec3 c = IntVec3.FromVector3(clickPos);
            var thingList = c.GetThingList(map);

                //遍历每一个被选中的小人，支持多选装填。
            foreach (Pawn pawn in selectedPawns)
            {
                //过滤：必须是玩家派系，且是非倒地/非精神崩溃状态（可根据需要调整）
                if (pawn.Faction != Faction.OfPlayer) continue;
                if (!pawn.RaceProps.Humanlike) continue;
                if (pawn.Downed || pawn.Dead) continue;

                //检查装备
                var eq = pawn.equipment?.Primary;
                if (eq == null) continue;

                //检查是否有我们的燃料组件
                var comp = eq.TryGetComp<MihoAdfqs.MihoAdfComp.ThingComp_FueledSprayer>();
                if (comp == null) continue;

                //遍历地上的物品，看是不是燃料
                foreach (Thing t in thingList)
                {
                    if (t.def.defName == comp.Props.fuelDef)
                    {
                        //找到了燃料，检查是否可达
                        if (!pawn.CanReach(t, PathEndMode.ClosestTouch, Danger.Deadly))
                        {
                            __result.Add(new FloatMenuOption("CannotReach".Translate(), null));
                            continue;
                        }

                        //生成菜单标签
                        string label = "Reload " + eq.LabelShort + " with " + t.Label;
                        if (comp.fuel < comp.Props.maxFuel)
                        {
                            //添加选项
                            __result.Add(new FloatMenuOption(label, delegate
                            {
                                //执行装填菜单对应的任务。

                                //确保 JobDef 名称正确
                                JobDef jobDef = DefDatabase<JobDef>.GetNamed("Miho_RefuelSprayerJob");

                                //创建任务：TargetA=燃料，TargetB=武器
                                Job job = JobMaker.MakeJob(jobDef, t, eq);
                                job.count = 1;

                                //强制小人执行任务
                                pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
                            }));
                        }
                    }
                }
            }
        }
    }

    //职责：在选中殖民者时提供当前武器的燃料状态栏。
    [HarmonyPatch(typeof(Pawn), "GetGizmos")]
    public static class Patch_Pawn_GetGizmos_FuelBar
    {
        //职责：保留原版指令，仅为携带燃料组件武器的玩家人物附加状态栏。
        public static IEnumerable<Gizmo> Postfix(IEnumerable<Gizmo> __result, Pawn __instance)
        {
            foreach (var g in __result) yield return g;

            if (__instance.Faction != Faction.OfPlayer) yield break;
            if (!Find.Selector.IsSelected(__instance)) yield break;

            ThingWithComps weapon = __instance.equipment?.Primary;
            if (weapon == null) yield break;

            var fuelComp = weapon.GetComp<MihoAdfqs.MihoAdfComp.ThingComp_FueledSprayer>();
            if (fuelComp == null) yield break;

            yield return new Gizmo_StatusProgress(weapon.LabelCap, () => fuelComp.FuelPercent,
                () => $"燃料 {fuelComp.fuel}/{fuelComp.Props.maxFuel}",
                () => "燃料不足将无法开火，非征召时自动寻找燃料装填。") { Order = -200f };
        }

    }
}
