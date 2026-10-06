using System.Linq;
using MihoAdfqs.Phase2.Quests;
using RimWorld;
using Verse;

namespace MihoAdfqs.Phase2.Communications
{
    //职责：展示随机任务、选择借调殖民者，以及提交和更换活动委托。
    public partial class Dialog_ImperialCommunications
    {
        //职责：有活动任务时显示提交与更换，否则展示随机委托。
        private void ShowMissions()
        {
            QuestPart_ImperialContract contract = ImperialMissionService.Active;
            if (contract == null) { ShowOffer(); return; }
            Page(contract.quest.name + "\n\n" + contract.quest.description,
                new CommunicationOption("提交任务", () =>
                {
                    if (!contract.CanSubmit) { contract.Submit(map); return; }
                    Page("这么快吗？看来是我低估了元首的眼光……咳咳，接下来，会有一艘穿梭机来到你们那里，接收对应的货物。",
                        new CommunicationOption("呼叫接收穿梭机", () => { contract.Submit(map); Close(); }),
                        new CommunicationOption("返回", ShowMissions));
                }),
                new CommunicationOption("呼叫卡黛尔", () => Page("任务很困难吗？诶？那我去换一个任务？嗯！\n\n更换会取消当前委托，借调中的殖民者将提前归还，当前任务不发奖励。",
                    new CommunicationOption("更换任务", () => { contract.Cancel(); ShowOffer(); }),
                    new CommunicationOption("返回", ShowMissions))),
                new CommunicationOption("返回", ShowMain));
        }

        //职责：随机展示一种委托及其完整奖励说明。
        private void ShowOffer()
        {
            string kind = new[] { "Social", "Rescue", "Recovery" }.RandomElement();
            string introduction = kind == "Recovery" ? "嗯，帮我们回收落在一个地方的空投物资，你可以拿其中的一半。至于剩下的，可就不准拿了哦！"
                : kind == "Rescue" ? "我们有一支小分队不剩什么补给了，基本都是伤员，附近还有一堆令人恶心的土著。拜托啦！"
                : "喂？你们有什么社交高的人员吗？这个……你不需要知晓具体原因，同不同意就是了！";
            Page(introduction + "\n\n" + ImperialMissionService.Description(kind),
                new CommunicationOption("接取", () =>
                {
                    if (kind == "Social") ChooseNegotiator();
                    else AcceptMission(kind, null);
                }), new CommunicationOption("返回", ShowMain));
        }

        //职责：只列出当前地图上社交严格大于十且可借调的自由殖民者。
        private void ChooseNegotiator()
        {
            var candidates = map.mapPawns.FreeColonistsSpawned.Where(p => !p.Downed && !p.InMentalState &&
                p.skills.GetSkill(SkillDefOf.Social).Level > 10 && !Find.QuestManager.IsReservedByAnyQuest(p)).ToList();
            if (candidates.Count == 0)
            {
                Messages.Message("当前没有社交高于10且能够出发的殖民者。", MessageTypeDefOf.RejectInput, false);
                return;
            }
            Find.WindowStack.Add(new FloatMenu(candidates.Select(p => new FloatMenuOption(
                p.LabelShort + "（社交" + p.skills.GetSkill(SkillDefOf.Social).Level + "）", () => AcceptMission("Social", p))).ToList()));
        }

        //职责：生成实际委托后显示结束通讯的确认文本。
        private void AcceptMission(string kind, Pawn pawn)
        {
            ImperialMissionService.Accept(kind, map, pawn);
            Page("那么好，等你完成后，我们再接着通讯吧！\n\n任务已加入任务列表。",
                new CommunicationOption("通讯挂断", () => Close()));
        }
    }
}
