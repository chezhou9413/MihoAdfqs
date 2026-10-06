using System;
using System.Collections.Generic;
using System.Linq;
using LudeonTK;
using MihoAdfqs.Phase2.Pawns;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI.Group;

namespace MihoAdfqs.Phase2.DebugTools
{
    //在独立空地图上生成第三帝国与原版帝国部队，供玩家观察大规模战斗。
    public static class DebugActions_ImperialBattle
    {
        private static readonly string[] NovaWeapons =
        {
            "MihoPhase2_SupernovaHeavyLauncher", "MihoPhase2_SupernovaLaserRifle",
            "MihoPhase2_SupernovaEMPrecisionRifle", "MihoPhase2_SupernovaEMMarksmanRifle"
        };

        //入口只在已进入存档后提供，不复用当前地图或已有特殊人物。
        [DebugAction("美狐第三帝国", "新建空地图：第三帝国精英60人对原版帝国180人", actionType = DebugActionType.Action,
            allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void CreateBattlefield()
        {
            if (!ModsConfig.RoyaltyActive)
            {
                Messages.Message("原版帝国战斗测试场需要启用皇权（Royalty）DLC。", MessageTypeDefOf.RejectInput, false);
                return;
            }
            PawnKindDef soldier = DefDatabase<PawnKindDef>.GetNamed("MihoPhase2_DefenseSoldier");
            //避开世界据点、地块变异和生物群系额外生成步骤，保证场地为空。
            if (!Find.WorldGrid.Surface.Tiles.Where(tile => tile.PrimaryBiome.canBuildBase
                    && tile.PrimaryBiome.extraGenSteps.Count == 0 && tile.Mutators.Count == 0
                    && !Find.WorldObjects.AnyWorldObjectAt(tile.tile) && TileFinder.IsValidTileForNewSettlement(tile.tile)
                    && Find.World.tileTemperatures.SeasonAndOutdoorTemperatureAcceptableFor(tile.tile, soldier.race))
                .TryRandomElement(out Tile location))
            {
                Messages.Message("没有找到可用于帝国战斗测试的空闲地块。", MessageTypeDefOf.RejectInput, false);
                return;
            }
            PlanetTile tileToUse = location.tile;
            Find.TickManager.Pause();
            //地图和人物生成均在主线程的长事件内完成。
            LongEventHandler.QueueLongEvent(() => GenerateBattle(tileToUse), "GeneratingMap", false, exception =>
            {
                Log.Error("[MihoAdfqs] 帝国战斗测试场创建失败：\n" + exception);
                Messages.Message("帝国战斗测试场创建失败，原因已记录到日志。", MessageTypeDefOf.RejectInput, false);
            });
        }

        //创建地图、双方阵列及搜敌任务，完成后将视角切到战场。
        private static void GenerateBattle(PlanetTile tile)
        {
            Faction eliteFaction = MakeTestFaction(DefDatabase<FactionDef>.GetNamed("MihoThirdEmpire"), "第三帝国精英测试队", .1f);
            Faction enemyFaction = MakeTestFaction(FactionDefOf.Empire, "原版帝国士兵测试队", .9f);
            SetRelation(eliteFaction, Faction.OfPlayer, FactionRelationKind.Ally, 100);
            SetRelation(enemyFaction, Faction.OfPlayer, FactionRelationKind.Hostile, -100);
            SetRelation(eliteFaction, enemyFaction, FactionRelationKind.Hostile, -100);

            var parent = (MapParent)WorldObjectMaker.MakeWorldObject(
                DefDatabase<WorldObjectDef>.GetNamed("MihoPhase2_DebugBattlefield"));
            parent.Tile = tile;
            parent.SetFaction(eliteFaction);
            Find.WorldObjects.Add(parent);
            Map map = MapGenerator.GenerateMap(new IntVec3(250, 1, 250), parent,
                DefDatabase<MapGeneratorDef>.GetNamed("MihoPhase2_DebugBattlefieldGenerator"));
            map.fogGrid.ClearAllFog();

            List<Pawn> elites = SpawnArmy(map, eliteFaction, true);
            List<Pawn> enemies = SpawnArmy(map, enemyFaction, false);
            LordMaker.MakeNewLord(eliteFaction, new LordJob_ImperialDebugBattle(map.Center), map, elites);
            LordMaker.MakeNewLord(enemyFaction, new LordJob_ImperialDebugBattle(map.Center), map, enemies);
            string report = "帝国战斗测试场已创建：250×250空白地图。精英60人（队长1、亲卫19、空降兵20、特战兵14、支援兵6）"
                + "对原版帝国180人（按原版常规战斗兵种权重生成，无战备支援兵），双方自动搜敌，第三帝国支援兵呼叫战备。";
            Log.Message("[MihoAdfqs] " + report);
            LongEventHandler.ExecuteWhenFinished(() =>
            {
                Current.Game.CurrentMap = map;
                CameraJumper.TryJump(map.Center, map);
                Find.TickManager.CurTimeSpeed = TimeSpeed.Normal;
                Messages.Message(report, MessageTypeDefOf.PositiveEvent, false);
            });
        }

        //使用完整的原版派系生成流程，隐藏测试派系并禁止其参与常规世界事件。
        private static Faction MakeTestFaction(FactionDef definition, string name, float color)
        {
            Faction faction = FactionGenerator.NewGeneratedFaction(new FactionGeneratorParms(
                definition, hidden: true));
            faction.Name = name;
            faction.temporary = true;
            faction.colorFromSpectrum = color;
            Find.FactionManager.Add(faction);
            return faction;
        }

        //只设置新测试派系的关系，双向好感与敌对标志保持一致。
        private static void SetRelation(Faction first, Faction second, FactionRelationKind kind, int goodwill)
        {
            first.SetRelation(new FactionRelation { other = second, kind = kind, baseGoodwill = goodwill });
            second.RelationWith(first).baseGoodwill = goodwill;
        }

        //按制式兵种生成独立人物，展开为相对的两支阵列。
        private static List<Pawn> SpawnArmy(Map map, Faction faction, bool elite)
        {
            int count = elite ? 60 : 180;
            int columns = elite ? 6 : 12;
            int rows = count / columns;
            var pawns = new List<Pawn>(count);
            //原版帝国沿用常规战斗池及权重，不混入第三帝国兵种或装备。
            List<PawnGenOption> empireOptions = elite ? null
                : faction.def.pawnGroupMakers.First(g => g.kindDef == PawnGroupKindDefOf.Combat).options;
            for (int index = 0; index < count; index++)
            {
                PawnKindDef kind;
                if (elite)
                {
                    string kindName = index == 0 ? "MihoPhase2_LeaderGuardCaptain" : index < 20 ? "MihoPhase2_LeaderGuard"
                        : index < 40 ? "MihoPhase2_OrbitalDropTrooper" : index < 54 ? "MihoPhase2_SpecialForcesSoldier"
                        : "MihoPhase2_StratagemSupportSoldier";
                    kind = DefDatabase<PawnKindDef>.GetNamed(kindName);
                }
                else kind = empireOptions.RandomElementByWeight(option => option.selectionWeight).kind;
                var request = new PawnGenerationRequest(kind, faction, tile: map.Tile,
                    forceGenerateNewPawn: true, canGeneratePawnRelations: false, mustBeCapableOfViolence: true);
                Pawn pawn = PawnGenerator.GeneratePawn(request);
                if (pawn == null) throw new InvalidOperationException("测试部队人物生成失败：" + kind.defName);
                //确保亲卫与空降兵覆盖四种超新星武器，队长保留自己的制式武器。
                if (elite && index > 0 && index < 40) SpecialPawnEquipment.Arm(pawn, NovaWeapons[index % NovaWeapons.Length]);
                int column = index % columns;
                int row = index / columns;
                var cell = new IntVec3(elite ? 80 + column * 3 : 170 - column * 3,
                    0, map.Center.z - (rows - 1) * 3 / 2 + row * 3);
                GenSpawn.Spawn(pawn, cell, map, elite ? Rot4.East : Rot4.West);
                pawns.Add(pawn);
            }
            return pawns;
        }
    }
}
