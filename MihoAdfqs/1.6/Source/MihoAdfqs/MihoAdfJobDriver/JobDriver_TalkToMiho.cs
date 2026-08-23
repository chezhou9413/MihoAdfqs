using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace MihoAdfqs.MihoAdfJobDriver
{
    public class JobDriver_TalkToMiho : JobDriver
    {
        // TargetA 将会是我们右键点击的目标 Pawn
        private Pawn TargetPawn => (Pawn)TargetThingA;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(TargetThingA, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            // 1. 如果目标消失或死了，任务失败
            this.FailOnDespawnedOrNull(TargetIndex.A);

            // 2. 走到目标面前 (Touch 表示贴身，InteractionCell 表示走到交互位)
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch)
                .FailOn(() => TargetPawn.Downed || TargetPawn.Dead);
            yield return Toils_General.Do(delegate
            {
                pawn.rotationTracker.FaceTarget(TargetPawn);
            });
            Toil talkToil = new Toil();
            talkToil.initAction = delegate
            {
                Pawn p = this.TargetPawn;

                // --- 1. 创建对话根节点 ---
                DiaNode rootNode = new DiaNode("嗯，我大概在附近观察了几天，殖民地的思想很符合我的见解，正好，如今，我正需要一段特殊经历的历练。如果可以的话，愿意让我加入这里吗？放心，这只是一个小小的请求，不接受，也不会影响什么。");
                // --- 选项 A: 允许加入 (变位殖民者) ---
                DiaOption optJoin = new DiaOption("允许加入")
                {
                    action = delegate
                    {
                        // 招募逻辑：设置阵营为玩家
                        if (p.Faction != Faction.OfPlayer)
                        {
                            p.SetFaction(Faction.OfPlayer, null);
                            if (ModsConfig.IdeologyActive && Faction.OfPlayer.ideos?.PrimaryIdeo != null)
                            {
                                Ideo playerIdeo = Faction.OfPlayer.ideos.PrimaryIdeo;
                                p.ideo.SetIdeo(playerIdeo);
                            }
                            Messages.Message($"{p.LabelShort} 已加入你的殖民地！", p, MessageTypeDefOf.PositiveEvent);
                        }
                    },
                    resolveTree = true // 点击后关闭窗口
                };

                // --- 选项 B: 驱逐 (离开地图) ---
                DiaOption optBanish = new DiaOption("驱逐")
                {
                    action = delegate
                    {
                        Messages.Message($"{p.LabelShort} 正在离开。", p, MessageTypeDefOf.NeutralEvent);

                        // 给予“离开地图”的 LordJob (群体AI)
                        // 这会让小人自动寻找最近的地图边缘离开
                        LordJob_ExitMapBest exitJob = new LordJob_ExitMapBest(LocomotionUrgency.Walk, true, true);
                        LordMaker.MakeNewLord(p.Faction, exitJob, p.Map, new List<Pawn> { p });
                    },
                    resolveTree = true
                };

                // --- 选项 C: 攻击 (变成敌对) ---
                DiaOption optAttack = new DiaOption("攻击")
                {
                    action = delegate
                    {
                        Faction enemyFaction = Faction.OfAncientsHostile; // 或者其他永远敌对的阵营
                        p.SetFaction(enemyFaction);

                        // 可选：让它立刻攻击现在的对话者
                        p.mindState.enemyTarget = pawn; // pawn 是当前执行任务的殖民者

                        Messages.Message($"{p.LabelShort} 变得充满敌意！", p, MessageTypeDefOf.ThreatSmall);
                    },
                    resolveTree = true
                };

                // --- 将选项加入节点 ---
                rootNode.options.Add(optJoin);
                rootNode.options.Add(optBanish);
                rootNode.options.Add(optAttack);
                Find.WindowStack.Add(new Dialog_NodeTree(rootNode, true, true, "与"+pawn.LabelShort+ "交谈"));
            };
            talkToil.defaultCompleteMode = ToilCompleteMode.Instant; 
            yield return talkToil;
        }
    }
}
