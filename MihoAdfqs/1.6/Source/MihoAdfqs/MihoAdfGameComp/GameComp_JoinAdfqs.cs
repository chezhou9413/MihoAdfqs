using MihoAdfqs.MihoAdfDefRef;
using RimWorld;
using System.Linq;
using Verse;

namespace MihoAdfqs.MihoAdfGameComp
{
    public class GameComp_JoinAdfqs : GameComponent
    {
        public bool isTigger = false;
        public GameComp_JoinAdfqs(Game game) 
        {
        }

        public override void FinalizeInit()
        {
            base.FinalizeInit();
            EnsureMihoHatesEveryone();
        }
        public override void ExposeData()
        {
            Scribe_Values.Look(ref isTigger, "isTigger", false);
            base.ExposeData();
        }

        public override void GameComponentTick()
        {
            base.GameComponentTick();
            if (Find.TickManager.TicksGame % 2500 == 0)
            {
                CheckIdeoCount();
            }
            if (Find.TickManager.TicksGame % 60 == 0)
            {
                CheckAdfqsXenotype();
            }
        }
        private void CheckIdeoCount()
        {
            if (!ModsConfig.IdeologyActive || Faction.OfPlayer == null || Faction.OfPlayer.ideos == null)
            {
                return;
            }
            Ideo primaryIdeo = Faction.OfPlayer.ideos.PrimaryIdeo;
            if (primaryIdeo == null) return;
            int count = PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_FreeColonists
                        .Count(p => p.Ideo == primaryIdeo);
            Map map = Find.AnyPlayerHomeMap;
            if (count >= 3 && !isTigger && map != null)
            {
                isTigger = true;
                IncidentParms parms = StorytellerUtility.DefaultParmsNow(MihoDefRef.Incidents_MihoAdf.category, map);
                MihoDefRef.Incidents_MihoAdf.Worker.TryExecute(parms);
            }
        }

        private void EnsureMihoHatesEveryone()
        {
            Faction mihoFaction = Find.FactionManager.FirstFactionOfDef(MihoDefRef.MihoThirdEmpire);
            if (mihoFaction == null) return;
            foreach (Faction otherFaction in Find.FactionManager.AllFactionsListForReading)
            {
                if (otherFaction == mihoFaction) continue;
                if (otherFaction.IsPlayer) continue;
                if (otherFaction.defeated || otherFaction.Hidden) continue;
                FactionRelation relA = mihoFaction.RelationWith(otherFaction, true);
                relA.baseGoodwill = -100;
                relA.kind = FactionRelationKind.Hostile;
                FactionRelation relB = otherFaction.RelationWith(mihoFaction, true);
                relB.baseGoodwill = -100;
                relB.kind = FactionRelationKind.Hostile;
            }
        }

        private void CheckAdfqsXenotype()
        {
            // 确保 Biotech DLC 激活，并且相关的 Defs 已经加载
            if (!ModsConfig.BiotechActive || MihoDefRef.Miho_adfqs == null || MihoDefRef.Xeno_MihoThirdEmpire == null)
            {
                return;
            }

            // 遍历所有玩家的殖民者
            foreach (Pawn pawn in PawnsFinder.AllMapsWorldAndTemporary_Alive)
            {
                // 基础检查
                if (pawn == null || pawn.genes == null) continue;

                // 检查是否是目标 PawnKind
                if (pawn.kindDef == MihoDefRef.Miho_adfqs)
                {
                    // 如果当前基因型不匹配，则开始“清洗并重置”流程
                    if (pawn.genes.Xenotype != MihoDefRef.Xeno_MihoThirdEmpire)
                    {
                        // ==================================================
                        // 步骤 1: 清空现有基因
                        // ==================================================

                        // 必须倒序遍历 (Reverse Loop) 来删除，否则会报错或漏删
                        // 先删异种基因 (Xenogenes)
                        for (int i = pawn.genes.Xenogenes.Count - 1; i >= 0; i--)
                        {
                            pawn.genes.RemoveGene(pawn.genes.Xenogenes[i]);
                        }

                        // 再删内源基因 (Endogenes)
                        for (int i = pawn.genes.Endogenes.Count - 1; i >= 0; i--)
                        {
                            pawn.genes.RemoveGene(pawn.genes.Endogenes[i]);
                        }

                        // ==================================================
                        // 步骤 2: 设置基因型标签
                        // ==================================================
                        pawn.genes.SetXenotype(MihoDefRef.Xeno_MihoThirdEmpire);

                        // ==================================================
                        // 步骤 3: 手动添加新基因型的基因
                        // ==================================================
                        // 如果不做这一步，Pawn 会变成该基因型，但是是个没有任何能力的“白板”

                        if (MihoDefRef.Xeno_MihoThirdEmpire.genes != null)
                        {
                            foreach (GeneDef geneDef in MihoDefRef.Xeno_MihoThirdEmpire.genes)
                            {
                                // AddGene 方法会自动处理基因冲突和添加逻辑
                                // xenogene: false 表示添加为内源基因（这也是大多数种族基因的默认行为）
                                pawn.genes.AddGene(geneDef, xenogene: false);
                            }
                        }
                    }
                }
            }
        }
    }
}
