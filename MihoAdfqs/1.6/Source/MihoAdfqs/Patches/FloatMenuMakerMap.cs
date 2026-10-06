using HarmonyLib;
using MihoAdfqs.MihoAdfDefRef;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;

namespace MihoAdfqs.Patches
{

    //在主线程加载问号图标，为可招募的米元首绘制提示。
    [StaticConstructorOnStartup]
    [HarmonyPatch(typeof(Pawn), "DrawGUIOverlay")]
    public static class Patch_Pawn_DrawGUIOverlay
    {
        private static readonly Texture2D QuestionMarkTex = ContentFinder<Texture2D>.Get("UI/Overlays/QuestionMark", true);

        //仅在当前地图的可见元首头顶显示招募提示。
        public static void Postfix(Pawn __instance)
        {
            if (__instance.kindDef != MihoDefRef.Miho_adfqs) return;
            if (!__instance.Spawned || __instance.Map != Find.CurrentMap || __instance.Position.Fogged(__instance.Map))
                return;
            if (__instance.Faction != Faction.OfPlayer && !__instance.HostileTo(Faction.OfPlayer))
            {
                float pixelsPerCell = Find.CameraDriver.CellSizePixels;
                float worldScale = 0.5f;
                float iconSize = pixelsPerCell * worldScale;
                Vector2 screenPos = GenMapUI.LabelDrawPosFor(__instance, 0.4f);

                // 向右偏移 0.4 个格子，向上偏移 0.4 个格子
                float xOffset = 0.4f * pixelsPerCell;
                float yOffset = 0.4f * pixelsPerCell;

                screenPos.x += xOffset;
                screenPos.y -= yOffset;
                Rect iconRect = new Rect(screenPos.x, screenPos.y, iconSize, iconSize);

                GUI.color = Color.yellow;
                GUI.DrawTexture(iconRect, QuestionMarkTex);
                GUI.color = Color.white;
            }
        }
    }

    [HarmonyPatch(typeof(FloatMenuMakerMap), "GetOptions")]
    public static class Patch_FloatMenuMakerMap_GetOptions
    {
        public static void Postfix(List<Pawn> selectedPawns, Vector3 clickPos, List<FloatMenuOption> __result)
        {
            if (__result == null || selectedPawns == null || selectedPawns.Count == 0) return;
            Pawn pawn = selectedPawns[0];
            if (pawn.Downed || pawn.Map == null) return;
            IntVec3 clickCell = IntVec3.FromVector3(clickPos);
            foreach (var target in clickCell.GetThingList(pawn.Map))
            {
                if (target is Pawn targetPawn && targetPawn != pawn)
                {
                    if (targetPawn.kindDef == MihoDefRef.Miho_adfqs && targetPawn.Faction != Faction.OfPlayer && !targetPawn.HostileTo(Faction.OfPlayer)) // 确保这里是正确的引用
                    {
                        string label = "交谈"; // 菜单显示的文字
                        Action action = delegate
                        {
                            Job job = JobMaker.MakeJob(MihoDefRef.Miho_TalkToPawn, targetPawn);
                            pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
                        };
                        FloatMenuOption option = new FloatMenuOption(label, action, MenuOptionPriority.Default, null, targetPawn);
                        if (pawn.CanReach(targetPawn, PathEndMode.Touch, Danger.Deadly) == false)
                        {
                            option.Disabled = true;
                            option.Label += " (无法到达)";
                        }
                        __result.Add(option);
                    }
                }
            }
        }
    }
}
