using System.Collections.Generic;
using MihoAdfqs.Phase2.Pawns;
using RimWorld;
using UnityEngine;
using Verse;
using MihoAdfqs.Phase2.UI;

namespace MihoAdfqs.Phase2.Helmer
{
    //保存赫尔默的战斗姿态和翻滚冷却，并提供玩家可见的技能入口。
    public class Gene_Helmer : Gene
    {
        public bool storm;
        public bool automatic = true;
        public int nextRollTick;

        //取得目标当前生效的45号实验基因。
        public static Gene_Helmer Get(Pawn pawn) => ImperialGeneLookup.For(pawn).ActiveHelmer;

        //避险每六刻检查，战斗走位每半秒检查，不干涉未征召殖民者的工作。
        public override void TickInterval(int delta)
        {
            base.TickInterval(delta);
            if (!Active || !pawn.Spawned) return;
            if (pawn.CurJob?.def.defName == "MihoPhase2_EvadeFire") return;
            if (pawn.Downed || !automatic || (pawn.IsColonistPlayerControlled && !pawn.Drafted)) { storm = false; return; }
            if (pawn.IsHashIntervalTick(6, delta) && HelmerRollAI.TryEvade(this)) return;
            if (!pawn.IsHashIntervalTick(30, delta)) return;
            HelmerCombatAI.Update(this);
        }

        //显示战斗姿态，并允许玩家关闭自动移动和手动选择翻滚方向。
        public override IEnumerable<Gizmo> GetGizmos()
        {
            //原版基因基类不提供指令集合，只生成本基因负责的玩家指令。
            if (!Active || pawn.Faction?.IsPlayer != true) yield break;
            yield return new Command_ImperialToggle
            {
                defaultLabel = storm ? "血液风暴：近攻" : "血液风暴：远射",
                defaultDesc = "自动战斗开启时，根据距离切换姿态并保持射程。关闭可手动控制移动。远射：射程×2、命中100%、射击耗时×150%；近攻：射程×0.5、45号承伤×50%、换弹耗时×50%、射击耗时×75%，击杀恢复一个受伤部位。",
                icon = ContentFinder<Texture2D>.Get("UI/MihoPhase2/Abilities/MihoPhase2_HelmerZogBloodStorm_" + (storm ? "Active" : "Inactive")),
                isActive = () => automatic,
                toggleAction = () => { automatic = !automatic; if (!automatic) storm = false; }
            };
            var roll = new Command_ImperialAction
            {
                defaultLabel = nextRollTick > Find.TickManager.TicksGame ? $"翻滚 {(nextRollTick - Find.TickManager.TicksGame) / 60f:F1}秒" : "翻滚",
                defaultDesc = "向选定方向翻滚最多6格，途中免疫伤害。冷却20秒，不穿越墙体。自动战斗开启时优先躲避来袭弹道、爆炸和近身敌人，选择安全落点；普通走位优先掩体与有效射程。手动移动期间不自动翻滚。",
                icon = ContentFinder<Texture2D>.Get("UI/MihoPhase2/Abilities/MihoPhase2_HelmerZogRoll"),
                action = () => Find.Targeter.BeginTargeting(new TargetingParameters { canTargetLocations = true, canTargetPawns = true, canTargetBuildings = false },
                    target => HelmerRoll.TryRoll(this, target.Cell), null,
                    target => pawn.Spawned && pawn.Map == Find.CurrentMap && target.Cell.InBounds(pawn.Map) && target.Cell != pawn.Position)
            };
            if (!pawn.Spawned || pawn.Downed || nextRollTick > Find.TickManager.TicksGame) roll.Disable("需要能够行动且翻滚已冷却");
            yield return roll;
        }

        //保存战斗控制选项、当前姿态和翻滚冷却终点。
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref storm, "bloodStorm");
            Scribe_Values.Look(ref automatic, "automaticCombat", true);
            Scribe_Values.Look(ref nextRollTick, "nextRollTick");
        }
    }
}
