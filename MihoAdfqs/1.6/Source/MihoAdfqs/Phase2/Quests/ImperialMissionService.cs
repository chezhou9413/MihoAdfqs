using System.Linq;
using RimWorld;
using Verse;

namespace MihoAdfqs.Phase2.Quests
{
    //职责：提供通讯委托的名称、接取入口和唯一活动任务检索。
    public static class ImperialMissionService
    {
        public static QuestPart_ImperialContract Active => Find.QuestManager.QuestsListForReading
            .Where(q => !q.Historical).SelectMany(q => q.PartsListForReading).OfType<QuestPart_ImperialContract>().FirstOrDefault();

        //职责：返回任务对外显示名称。
        public static string Name(string kind) => kind == "Social" ? "一场激烈的语言交锋" : kind == "Rescue" ? "拯救大兵瑞恩" : "回收空投";

        //职责：返回任务描述与完成条件，供通讯预览和任务列表共用。
        public static string Description(string kind)
        {
            if (kind == "Social") return "受卡黛尔所托，派出一名社交高于10的殖民者10天。穿梭机会来接人；人员回归后奖励工业级钢板200—440和20点数。";
            if (kind == "Rescue") return "小分队缺乏补给，多人受伤，附近有敌人。前往标记地点，消灭围困敌人并至少救出一名队员。奖励聚乙烯400—800、白银1000和40点数。";
            return "前往标记地点回收空投。物资的一半归殖民地，另一半通过通讯呼叫穿梭机交回。奖励工业级钢板200—500、黄金100—299和10点数。";
        }

        //职责：将委托注册为实际任务，借调任务要求已经选定符合条件的殖民者。
        public static void Accept(string kind, Map map, Pawn negotiator = null)
        {
            if (Active != null) return;
            if (kind == "Social" && (negotiator == null || negotiator.skills.GetSkill(SkillDefOf.Social).Level <= 10))
            {
                Messages.Message("需要一名社交高于10的殖民者。", MessageTypeDefOf.RejectInput, false);
                return;
            }
            Quest quest = Quest.MakeRaw();
            quest.root = DefDatabase<QuestScriptDef>.GetNamed("MihoPhase2_Contract");
            quest.name = Name(kind);
            quest.description = Description(kind);
            var contract = new QuestPart_ImperialContract { kind = kind, home = map.Parent,
                negotiator = negotiator, inSignalEnable = quest.InitiateSignal };
            quest.AddPart(contract);
            if (kind == "Social")
            {
                quest.AddPart(new QuestPart_ImperialLoan { inSignalEnable = contract.TransportTag + ".SentSatisfied",
                    returnMap = map.Parent, lendColonistsToFaction = Orbital.OrbitalSupport.Empire,
                    returnLentColonistsInTicks = 600000, outSignalsCompleted = { contract.TransportTag + ".LoanComplete" } });
            }
            quest.SetInitiallyAccepted();
            Find.QuestManager.Add(quest);
            Find.LetterStack.ReceiveLetter(quest.name, "帝国总部委托已建立。\n\n" + quest.description,
                LetterDefOf.PositiveEvent, null, Orbital.OrbitalSupport.Empire, quest);
        }
    }
}
