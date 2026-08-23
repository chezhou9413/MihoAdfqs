using HarmonyLib;
using MihoAdfqs.MihoAdfComp;
using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

namespace MihoAdfqs.Patches
{
    [HarmonyPatch(typeof(FloatMenuMakerMap), "GetOptions")]
    public static class Patch_FloatMenu_Reload
    {
        // Postfix: 在原版生成完菜单后，往返回列表 (__result) 里追加我们的选项
        // 注意参数要和原版 GetOptions 对应：selectedPawns, clickPos
        public static void Postfix(List<Pawn> selectedPawns, Vector3 clickPos, ref List<FloatMenuOption> __result)
        {
            // 1. 安全检查
            if (selectedPawns == null || __result == null) return;

            Map map = Find.CurrentMap;
            if (map == null) return;
            if (!clickPos.InBounds(map)) return;

            // 2. 获取点击位置的物品列表
            IntVec3 c = IntVec3.FromVector3(clickPos);
            var thingList = c.GetThingList(map);

            // 3. 遍历每一个被选中的小人（新版特性：多选支持）
            foreach (Pawn pawn in selectedPawns)
            {
                // 过滤：必须是玩家派系，且是非倒地/非精神崩溃状态（可根据需要调整）
                if (pawn.Faction != Faction.OfPlayer) continue;
                if (!pawn.RaceProps.Humanlike) continue;
                if (pawn.Downed || pawn.Dead) continue;

                // 检查装备
                var eq = pawn.equipment?.Primary;
                if (eq == null) continue;

                // 检查是否有我们的燃料组件
                var comp = eq.TryGetComp<MihoAdfqs.MihoAdfComp.ThingComp_FueledSprayer>();
                if (comp == null) continue;

                // 遍历地上的物品，看是不是燃料
                foreach (Thing t in thingList)
                {
                    if (t.def.defName == comp.Props.fuelDef)
                    {
                        // 找到了燃料，检查是否可达
                        if (!pawn.CanReach(t, PathEndMode.ClosestTouch, Danger.Deadly))
                        {
                            __result.Add(new FloatMenuOption("CannotReach".Translate(), null));
                            continue;
                        }

                        // 生成菜单标签
                        string label = "Reload " + eq.LabelShort + " with " + t.Label;
                        if (comp.fuel < comp.Props.maxFuel)
                        {
                            // 添加选项
                            __result.Add(new FloatMenuOption(label, delegate
                            {
                                // --- 点击后的逻辑 ---

                                // 确保 JobDef 名称正确
                                JobDef jobDef = DefDatabase<JobDef>.GetNamed("Miho_RefuelSprayerJob");

                                // 创建任务：TargetA=燃料，TargetB=武器
                                Job job = JobMaker.MakeJob(jobDef, t, eq);
                                job.count = 1;

                                // 强制小人执行任务
                                pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
                            }));
                        }
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(Pawn), "GetGizmos")]
    public static class Patch_Pawn_GetGizmos_FuelBar
    {
        public static IEnumerable<Gizmo> Postfix(IEnumerable<Gizmo> __result, Pawn __instance)
        {
            foreach (var g in __result) yield return g;

            if (__instance.Faction != Faction.OfPlayer) yield break;
            if (!Find.Selector.IsSelected(__instance)) yield break;

            ThingWithComps weapon = __instance.equipment?.Primary;
            if (weapon == null) yield break;

            var fuelComp = weapon.GetComp<MihoAdfqs.MihoAdfComp.ThingComp_FueledSprayer>();
            if (fuelComp == null) yield break;

            yield return new Gizmo_PawnFuelBar(__instance, weapon, fuelComp);
        }

        private class Gizmo_PawnFuelBar : Gizmo
        {
            private readonly Pawn pawn;
            private readonly ThingWithComps weapon;
            private readonly MihoAdfqs.MihoAdfComp.ThingComp_FueledSprayer comp;

            private static readonly Texture2D BarTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.95f, 0.75f, 0.15f));
            private static readonly Texture2D BarBGTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.20f, 0.20f, 0.20f));

            public Gizmo_PawnFuelBar(Pawn pawn, ThingWithComps weapon, MihoAdfqs.MihoAdfComp.ThingComp_FueledSprayer comp)
            {
                this.pawn = pawn;
                this.weapon = weapon;
                this.comp = comp;
                Order = -200f;
            }

            public override float GetWidth(float maxWidth) => 160f;

            public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
            {
                Rect rect = new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), 75f);

                // 绘制背景
                Widgets.DrawWindowBackground(rect);

                // 绘制标题
                Rect labelRect = rect.ContractedBy(6f);
                labelRect.height = 22f;
                string title = weapon.LabelCap;
                Text.Anchor = TextAnchor.MiddleLeft;
                Widgets.Label(labelRect, title);
                Text.Anchor = TextAnchor.UpperLeft;

                // 绘制燃料条
                Rect barRect = rect.ContractedBy(6f);
                barRect.yMin += 26f;
                barRect.height = 18f;

                float pct = Mathf.Clamp01(comp.FuelPercent);
                Widgets.FillableBar(barRect, pct, BarTex, BarBGTex, doBorder: true);

                // 绘制燃料数值文字
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(barRect, $"燃料 {comp.fuel}/{comp.Props.maxFuel}");
                Text.Anchor = TextAnchor.UpperLeft;

                // --- 修改点：移除了 ButtonInvisible 的点击判定逻辑 ---

                // 修改提示信息，去掉"点击装填"的说明
                TooltipHandler.TipRegion(rect, "燃料不足将无法开火。\n殖民者会在非征召状态下自动寻找燃料装填。");

                // 返回 Clear 状态，表示没有任何交互发生
                return new GizmoResult(GizmoState.Clear);
            }
        }
    }
}