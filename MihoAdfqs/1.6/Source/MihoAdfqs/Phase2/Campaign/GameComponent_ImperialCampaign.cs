using MihoAdfqs.Phase2.Orbital;
using RimWorld;
using Verse;

namespace MihoAdfqs.Phase2.Campaign
{
    //职责：按元首到达时间推进熟识和命运，并提供无主线前置的前卫任务。
    public class GameComponent_ImperialCampaign : GameComponent
    {
        private int arrivalTick = -1;
        private Quest familiarity;
        private Quest destiny;
        private Quest vanguard;
        public bool captainJoined;
        public static GameComponent_ImperialCampaign Current => Verse.Current.Game.GetComponent<GameComponent_ImperialCampaign>();

        //职责：提供游戏全局组件构造入口。
        public GameComponent_ImperialCampaign(Game game) { }

        //职责：低频检查任务前置，并保证每条主线只发布一次。
        public override void GameComponentTick()
        {
            int tick = Find.TickManager.TicksGame;
            if (tick % 60 != 0) return;
            Map home = Find.AnyPlayerHomeMap;
            if (home == null) return;
            if (vanguard == null) vanguard = Offer("Vanguard", "前卫", Compatibility.OptionalMods.Milira
                ? "前往第三帝国前哨，协助抵御智人种与米莉拉的联合进攻。奖励：新星蓝图1份。"
                : "前往第三帝国前哨，协助抵御敌对部队的进攻。奖励：新星蓝图1份。", home);
            if (!GameComponent_OrbitalNetwork.Current.unlocked) return;
            if (arrivalTick < 0) arrivalTick = tick;
            if (Compatibility.OptionalMods.Milira && familiarity == null && tick - arrivalTick >= 420000)
                familiarity = Offer("Familiarity", "熟识", "一名名叫提亚娜的米莉拉将乘穿梭机前来，希望与元首一起留在殖民地。奖励：提亚娜加入。", home);
            //未启用米莉拉时，队长主线按元首到达时间解锁，不等待提亚娜任务。
            if (destiny == null && (Compatibility.OptionalMods.Milira
                ? familiarity?.State == QuestState.EndedSuccess : tick - arrivalTick >= 420000))
                destiny = Offer("Destiny", "命运", "消灭三个指定据点，证明殖民地的能力。奖励：亲卫队长赫尔默·佐格加入。", home);
        }

        //职责：建立可在原版任务列表接取的主线，并发送带任务关联的信件。
        private static Quest Offer(string kind, string name, string description, Map map)
        {
            Quest quest = Quest.MakeRaw();
            quest.root = DefDatabase<QuestScriptDef>.GetNamed("MihoPhase2_Contract");
            quest.name = name;
            quest.description = description;
            quest.acceptanceExpireTick = kind == "Familiarity" ? Find.TickManager.TicksGame + 599940000 : -1;
            quest.AddPart(new QuestPart_ImperialCampaign { kind = kind, home = map.Parent, inSignalEnable = quest.InitiateSignal });
            Find.QuestManager.Add(quest);
            Find.LetterStack.ReceiveLetter(name, description, LetterDefOf.PositiveEvent, null, OrbitalSupport.Empire, quest);
            return quest;
        }

        //职责：保存到达时刻、主线引用与队长加入状态。
        public override void ExposeData()
        {
            Scribe_Values.Look(ref arrivalTick, "arrivalTick", -1);
            Scribe_References.Look(ref familiarity, "familiarity");
            Scribe_References.Look(ref destiny, "destiny");
            Scribe_References.Look(ref vanguard, "vanguard");
            Scribe_Values.Look(ref captainJoined, "captainJoined");
        }
    }
}
