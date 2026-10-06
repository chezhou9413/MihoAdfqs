using System;
using System.Linq;
using LudeonTK;
using RimWorld;
using Verse;

namespace MihoAdfqs.Phase2.DebugTools
{
    //批量生成本模组的玩家人物与物品样本，并解锁帝国科技。
    public static class DebugActions_ModSamples
    {
        //在点击位置周围生成每种人物和物品各一个，同时完成本模组研究。
        [DebugAction("美狐第三帝国", "生成全部玩家Kind与物品并解锁帝国科技", actionType = DebugActionType.ToolMap,
            allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void SpawnAllSamples()
        {
            Map map = Find.CurrentMap;
            IntVec3 center = Verse.UI.MouseCell();
            ModContentPack content = LoadedModManager.GetMod<MihoAdfqsMain>().Content;
            var kinds = DefDatabase<PawnKindDef>.AllDefsListForReading
                .Where(def => def.modContentPack == content && def.RaceProps.Humanlike)
                .OrderBy(def => def.defName).ToList();
            var items = DefDatabase<ThingDef>.AllDefsListForReading
                .Where(def => def.modContentPack == content && def.category == ThingCategory.Item)
                .OrderBy(def => def.defName).ToList();
            var cells = DebugSpawnLayout.FindCells(map, center, kinds.Count + items.Count);
            if (cells.Count < kinds.Count + items.Count)
            {
                Messages.Message($"附近空地不足：需要{kinds.Count + items.Count}格，找到{cells.Count}格，请点击更开阔的位置。",
                    MessageTypeDefOf.RejectInput, false);
                return;
            }

            Find.TickManager.Pause();
            int researchCount = UnlockImperialResearch(content);
            int nextCell = 0;
            int pawnCount = 0;
            int itemCount = 0;
            int failures = 0;
            foreach (PawnKindDef kind in kinds)
            {
                IntVec3 cell = cells[nextCell++];
                try
                {
                    SpawnPawn(kind, map, cell);
                    pawnCount++;
                }
                catch (Exception exception)
                {
                    //每个定义独立记录异常，让其余样本仍可生成并暴露各自的问题。
                    failures++;
                    Log.Error($"[MihoAdfqs] 测试人物生成失败：{kind.defName}\n{exception}");
                }
            }
            foreach (ThingDef def in items)
            {
                IntVec3 cell = cells[nextCell++];
                try
                {
                    SpawnItem(def, map, cell);
                    itemCount++;
                }
                catch (Exception exception)
                {
                    failures++;
                    Log.Error($"[MihoAdfqs] 测试物品生成失败：{def.defName}\n{exception}");
                }
            }
            string report = $"帝国科技已全部解锁（本次完成{researchCount}项）；测试样本：玩家人物{pawnCount}/{kinds.Count}，物品{itemCount}/{items.Count}，失败{failures}。";
            Log.Message("[MihoAdfqs] " + report);
            Messages.Message(report, failures == 0 ? MessageTypeDefOf.PositiveEvent : MessageTypeDefOf.RejectInput, false);
        }

        //原版完成接口会补齐前置研究和科技蓝图要求，并应用研究效果。
        private static int UnlockImperialResearch(ModContentPack content)
        {
            var projects = DefDatabase<ResearchProjectDef>.AllDefsListForReading
                .Where(def => def.modContentPack == content && !def.IsFinished)
                .OrderBy(def => def.defName).ToList();
            foreach (ResearchProjectDef project in projects)
                if (!project.IsFinished)
                    Find.ResearchManager.FinishProject(project, doCompletionDialog: false, doCompletionLetter: false);
            return projects.Count;
        }

        //按兵种的正常装备和基因规则生成全新玩家人物，不复用已有世界人物。
        private static void SpawnPawn(PawnKindDef kind, Map map, IntVec3 cell)
        {
            var request = new PawnGenerationRequest(kind, Faction.OfPlayer, tile: map.Tile,
                forceGenerateNewPawn: true, canGeneratePawnRelations: false);
            Pawn pawn = PawnGenerator.GeneratePawn(request);
            if (pawn == null) throw new InvalidOperationException("人物生成器未返回人物。");
            GenSpawn.Spawn(pawn, cell, map, Rot4.South);
        }

        //使用合法默认材料创建标准品质物品，直接放到独立地面格。
        private static void SpawnItem(ThingDef def, Map map, IntVec3 cell)
        {
            Thing item = ThingMaker.MakeThing(def, GenStuff.DefaultStuffFor(def));
            item.TryGetComp<CompQuality>()?.SetQuality(QualityCategory.Normal, ArtGenerationContext.Outsider);
            if (!GenPlace.TryPlaceThing(item, cell, map, ThingPlaceMode.Direct))
                throw new InvalidOperationException($"物品无法放置到{cell}。");
            item.SetForbidden(false, false);
        }
    }
}
