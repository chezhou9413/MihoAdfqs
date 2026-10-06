using MihoAdfqs.MihoAdfDefRef;
using MihoAdfqs.Phase2.Campaign;
using MihoAdfqs.Phase2.UI;
using MihoAdfqs.Phase2.Orbital;
using RimWorld;
using System;
using System.Collections.Generic;
using Verse;
using Verse.AI.Group;

namespace MihoAdfqs.MihoAdfqsVerb
{
    //类职责：在玩家确认队长选项后空投四名元首亲卫及可选亲卫队长。
    public class Verb_RequestSSReinforcements : Verb_CastAbility
    {
        public override bool Targetable => true;
        public override bool MultiSelect => true;

        //函数职责：捕获空投目标并弹出是否让亲卫队长参战的确认窗口。
        protected override bool TryCastShot()
        {
            Map map = CasterPawn?.Map;
            if (map == null)
            {
                return false;
            }

            IntVec3 targetCell = currentTarget.Cell;
            if (GameComponent_ImperialCampaign.Current.captainJoined)
            {
                TryDeployReinforcements(map, targetCell, false);
                return false;
            }
            Action withCaptain = delegate { TryDeployReinforcements(map, targetCell, true); };
            Action withoutCaptain = delegate { TryDeployReinforcements(map, targetCell, false); };
            var dialog = new Dialog_ImperialConfirmation("召唤元首亲卫",
                "注意，亲卫队长会显著影响前期乃至后期的战斗强度。是否让亲卫队长一同支援？",
                withCaptain,
                withoutCaptain);
            Find.WindowStack.Add(dialog);

            //选择完成后才生成小人并进入冷却，当前施放流程先停止。
            return false;
        }

        //函数职责：验证定义与派系后生成亲卫队并执行空投。
        private void TryDeployReinforcements(Map map, IntVec3 targetCell, bool includeCaptain)
        {
            includeCaptain &= !GameComponent_ImperialCampaign.Current.captainJoined;
            PawnKindDef guardKind = MihoDefRef.MihoPhase2_LeaderGuard;
            PawnKindDef captainKind = MihoDefRef.MihoPhase2_LeaderGuardCaptain;
            Faction faction = Find.FactionManager.FirstFactionOfDef(MihoDefRef.MihoThirdEmpire);
            if (guardKind == null || includeCaptain && captainKind == null)
            {
                Messages.Message("元首亲卫定义未正确加载。", DefDatabase<MessageTypeDef>.GetNamed("MihoPhase2_SilentSupport"), false);
                return;
            }

            if (faction == null)
            {
                Messages.Message("第三帝国派系当前不存在，无法响应呼叫。", DefDatabase<MessageTypeDef>.GetNamed("MihoPhase2_SilentSupport"), false);
                return;
            }

            GameComponent_OrbitalNetwork.Current.QueueGuards(CasterPawn, targetCell, includeCaptain);
            StartAbilityCooldown();
        }

        //已确认的亲卫支援在投送时生成，沿用原亲卫队伍与集结任务。
        internal static List<Thing> GenerateGuards(Map map, IntVec3 targetCell, bool includeCaptain)
        {
            includeCaptain &= !GameComponent_ImperialCampaign.Current.captainJoined;
            Faction faction = Find.FactionManager.FirstFactionOfDef(MihoDefRef.MihoThirdEmpire);
            List<Pawn> generatedPawns = new List<Pawn>();
            for (int i = 0; i < 4; i++)
            {
                generatedPawns.Add(GeneratePawn(MihoDefRef.MihoPhase2_LeaderGuard, faction));
            }

            if (includeCaptain)
            {
                generatedPawns.Add(GeneratePawn(MihoDefRef.MihoPhase2_LeaderGuardCaptain, faction));
            }

            List<Thing> thingsToDrop = new List<Thing>();
            for (int i = 0; i < generatedPawns.Count; i++)
            {
                thingsToDrop.Add(generatedPawns[i]);
            }

            LordJob_AssistColony lordJob = new LordJob_AssistColony(faction, targetCell);
            LordMaker.MakeNewLord(faction, lordJob, map, generatedPawns);
            return thingsToDrop;
        }

        //函数职责：按统一参数生成一名可参加支援战斗的非玩家小人。
        private static Pawn GeneratePawn(PawnKindDef pawnKind, Faction faction)
        {
            return PawnGenerator.GeneratePawn(new PawnGenerationRequest(
                pawnKind,
                faction,
                PawnGenerationContext.NonPlayer,
                -1,
                false,
                false,
                false,
                false,
                true,
                1f,
                false,
                true,
                true,
                true,
                true));
        }

        //函数职责：在支援成功落地后启动技能定义中的冷却时间。
        private void StartAbilityCooldown()
        {
            if (Ability == null)
            {
                return;
            }

            int cooldownTicks = Ability.def.cooldownTicksRange.RandomInRange;
            if (cooldownTicks > 0)
            {
                Ability.StartCooldown(cooldownTicks);
            }
        }
    }
}
