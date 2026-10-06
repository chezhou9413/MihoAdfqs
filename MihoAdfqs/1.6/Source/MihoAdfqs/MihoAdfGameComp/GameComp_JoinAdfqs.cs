using MihoAdfqs.MihoAdfDefRef;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace MihoAdfqs.MihoAdfGameComp
{
    //类职责：维护元首事件、第三帝国外交、元首基因和加入后的定期商队。
    public class GameComp_JoinAdfqs : GameComponent
    {
        private const int MaintenanceIntervalTicks = 2500;
        private const int EmpireCaravanIntervalTicks = 420000;

        public bool isTigger;
        private int nextEmpireCaravanTick;

        //函数职责：创建全局游戏组件并保留游戏对象构造签名。
        public GameComp_JoinAdfqs(Game game)
        {
        }

        //函数职责：在存档完成初始化后立即校正外交与元首状态。
        public override void FinalizeInit()
        {
            base.FinalizeInit();
            EnsureEmpireForeignRelations();
            MaintainJoinedFuhrerState();
        }

        //函数职责：保存事件触发标志和下一支定期商队的到达时刻。
        public override void ExposeData()
        {
            Scribe_Values.Look(ref isTigger, "isTigger", false);
            Scribe_Values.Look(ref nextEmpireCaravanTick, "nextEmpireCaravanTick", 0);
            base.ExposeData();
        }

        //函数职责：以低频维护二期全局规则，避免逐秒遍历世界小人。
        public override void GameComponentTick()
        {
            base.GameComponentTick();
            if (Find.TickManager.TicksGame % MaintenanceIntervalTicks != 0)
            {
                return;
            }

            CheckIdeoCount();
            EnsureEmpireForeignRelations();
            MaintainJoinedFuhrerState();
        }

        //函数职责：在主流文化人数达到阈值时触发一次元首接触事件。
        private void CheckIdeoCount()
        {
            if (!ModsConfig.IdeologyActive || Faction.OfPlayer?.ideos == null)
            {
                return;
            }

            Ideo primaryIdeo = Faction.OfPlayer.ideos.PrimaryIdeo;
            Map map = Find.AnyPlayerHomeMap;
            if (primaryIdeo == null || map == null || isTigger)
            {
                return;
            }

            int count = PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_FreeColonists.Count(pawn => pawn.Ideo == primaryIdeo);
            if (count < 3)
            {
                return;
            }

            isTigger = true;
            IncidentParms parms = StorytellerUtility.DefaultParmsNow(MihoDefRef.Incidents_MihoAdf.category, map);
            MihoDefRef.Incidents_MihoAdf.Worker.TryExecute(parms);
        }

        //函数职责：让第三帝国敌视非美狐势力，同时保持所有美狐势力至少中立。
        private void EnsureEmpireForeignRelations()
        {
            Faction empire = Find.FactionManager?.FirstFactionOfDef(MihoDefRef.MihoThirdEmpire);
            if (empire == null)
            {
                return;
            }

            foreach (Faction other in Find.FactionManager.AllFactionsListForReading)
            {
                if (other == empire || other.IsPlayer || other.defeated || other.Hidden)
                {
                    continue;
                }

                if (IsMihoFaction(other.def))
                {
                    if (empire.HostileTo(other))
                    {
                        SetMutualRelation(empire, other, FactionRelationKind.Neutral, 0);
                    }
                }
                else
                {
                    SetMutualRelation(empire, other, FactionRelationKind.Hostile, -100);
                }
            }
        }

        //函数职责：识别由美狐本体及美狐扩展提供的派系定义。
        private static bool IsMihoFaction(FactionDef factionDef)
        {
            if (factionDef == null)
            {
                return false;
            }

            string packageId = factionDef.modContentPack?.PackageId ?? string.Empty;
            return factionDef == MihoDefRef.MihoThirdEmpire ||
                   factionDef.defName.IndexOf("Miho", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   packageId.IndexOf("miho", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        //函数职责：在两个派系的双向关系记录中写入相同的关系与好感。
        private static void SetMutualRelation(Faction first, Faction second, FactionRelationKind kind, int goodwill)
        {
            FactionRelation firstRelation = first.RelationWith(second, true);
            firstRelation.kind = kind;
            firstRelation.baseGoodwill = goodwill;

            FactionRelation secondRelation = second.RelationWith(first, true);
            secondRelation.kind = kind;
            secondRelation.baseGoodwill = goodwill;
        }

        //函数职责：为已加入玩家的元首维护专属异种类型、永久同盟和七日商队。
        private void MaintainJoinedFuhrerState()
        {
            if (!ModsConfig.BiotechActive || MihoDefRef.Miho_adfqs == null)
            {
                return;
            }

            Pawn fuhrer = PawnsFinder.AllMapsWorldAndTemporary_Alive.FirstOrDefault(pawn => pawn.kindDef == MihoDefRef.Miho_adfqs);
            if (fuhrer == null)
            {
                return;
            }

            EnsureFuhrerXenotype(fuhrer);
            if (fuhrer.Faction != Faction.OfPlayer)
            {
                nextEmpireCaravanTick = 0;
                return;
            }

            Faction empire = Find.FactionManager.FirstFactionOfDef(MihoDefRef.MihoThirdEmpire);
            if (empire != null)
            {
                SetMutualRelation(empire, Faction.OfPlayer, FactionRelationKind.Ally, 100);
            }

            MaintainEmpireCaravanSchedule(empire);
        }

        //函数职责：在不清除既有基因的前提下补齐元首专属异种类型所需基因。
        private static void EnsureFuhrerXenotype(Pawn fuhrer)
        {
            XenotypeDef xenotype = MihoDefRef.Xeno_MihoPhase2_Fuhrer;
            if (fuhrer?.genes == null || xenotype == null)
            {
                return;
            }

            if (fuhrer.genes.Xenotype != xenotype)
            {
                fuhrer.genes.SetXenotype(xenotype);
            }

            if (xenotype.genes == null)
            {
                return;
            }

            foreach (GeneDef geneDef in xenotype.genes)
            {
                if (fuhrer.genes.GetGene(geneDef) == null)
                {
                    fuhrer.genes.AddGene(geneDef, false);
                }
            }
        }

        //函数职责：在元首加入后每七天尝试派遣一支随机类型的第三帝国商队。
        private void MaintainEmpireCaravanSchedule(Faction empire)
        {
            int currentTick = Find.TickManager.TicksGame;
            if (nextEmpireCaravanTick <= 0)
            {
                nextEmpireCaravanTick = currentTick + EmpireCaravanIntervalTicks;
                return;
            }

            if (currentTick < nextEmpireCaravanTick || empire == null)
            {
                return;
            }

            Map map = Find.AnyPlayerHomeMap;
            if (map == null || !TrySendEmpireCaravan(map, empire))
            {
                nextEmpireCaravanTick = currentTick + MaintenanceIntervalTicks;
                return;
            }

            nextEmpireCaravanTick = currentTick + EmpireCaravanIntervalTicks;
        }

        //函数职责：使用三个二期商人定义之一执行原版商队到达事件。
        private static bool TrySendEmpireCaravan(Map map, Faction empire)
        {
            List<TraderKindDef> traderKinds = new List<TraderKindDef>
            {
                MihoDefRef.MihoPhase2_Trader_SoapRecovery,
                MihoDefRef.MihoPhase2_Trader_CropAssociation,
                MihoDefRef.MihoPhase2_Trader_InterriverCommerce
            };
            traderKinds.RemoveAll(traderKind => traderKind == null);
            if (traderKinds.Count == 0)
            {
                return false;
            }

            IncidentParms parms = StorytellerUtility.DefaultParmsNow(IncidentDefOf.TraderCaravanArrival.category, map);
            parms.faction = empire;
            parms.traderKind = traderKinds.RandomElement();
            return IncidentDefOf.TraderCaravanArrival.Worker.TryExecute(parms);
        }
    }
}
