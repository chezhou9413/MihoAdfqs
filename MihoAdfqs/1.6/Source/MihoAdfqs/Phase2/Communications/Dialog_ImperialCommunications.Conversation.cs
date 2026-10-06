namespace MihoAdfqs.Phase2.Communications
{
    //职责：管理首次通讯的逐步对话、永久动员选择与主菜单。
    public partial class Dialog_ImperialCommunications
    {
        //职责：根据存档中的进度恢复初次联络对话。
        private void ShowIntroduction()
        {
            switch (Network.introductionStep)
            {
                case 0:
                    Page("……有人吗？", new CommunicationOption("没有", () => Intro(1)),
                        new CommunicationOption("默不作声", () => Close()));
                    break;
                case 1:
                    Page("……哈，真好笑，我是秘书卡黛尔。请问，元首那边有什么需要帮助的吗？",
                        new CommunicationOption("我需要重新集结部队！", () => Intro(2)),
                        new CommunicationOption("只是问问", () => Intro(3)));
                    break;
                case 2:
                    Page("你确定吗？元首她……真的不在意吗？虽然说，这样的话，她老人家很快就会回去就是了。\n\n集结后，每次敌军来袭将有10名轨道空降兵支援，战斗中每30秒进行舰炮轰炸；元首永久心情−10。",
                        new CommunicationOption("确定，重新集结部队！", () =>
                        {
                            Network.mobilized = true;
                            Network.introductionStep = 8;
                            ShowMain();
                        }), new CommunicationOption("只是问问", () => Intro(3)));
                    break;
                case 3:
                    Page("哦？那看来很正常了，所以，你需要什么？不对……帮我们一点小忙，你也可以拿到点数。根据持有的点数，帝国将会提供不同层次的援助。\n\n我好像说得有点多？呃，你现在能弄明白吗？",
                        new CommunicationOption("……", () => Intro(4)));
                    break;
                case 4:
                    Page("好吧，简单来说，除了某些简单任务提供的点数之外，一些需要在这颗星球稳步推行的任务会是你主要的奖励。",
                        new CommunicationOption("还有什么？", () => Intro(5)));
                    break;
                default:
                    Page("这个嘛……暂时没有，等元首她老人家的身体，也是你们殖民地的那位，什么时候回去了，什么时候内容会逐渐扩充。\n\n然后……",
                        new CommunicationOption("电磁干扰……对方挂断了通讯", () => { Network.introductionStep = 8; Close(); }));
                    break;
            }
        }

        //职责：保存初始对话节点并切换页面。
        private void Intro(int step) { Network.introductionStep = step; ShowIntroduction(); }

        //职责：展示后续通讯的四个主入口。
        private void ShowMain()
        {
            Page("你好，这里依旧是卡黛尔。现在，需要什么呢？",
                new CommunicationOption("任务", ShowMissions),
                new CommunicationOption("帝国支援 · 作战面板", ShowExchange),
                new CommunicationOption("想要看见你的相貌", () => PhaseThree("这个嘛，你要老实等到三期啦，嘻嘻！")),
                new CommunicationOption("聊一些奇怪的话题", () => PhaseThree("这个，也要等到三期啦，嘻嘻！")));
        }

        //职责：保留原文指定的三期对话提示。
        private void PhaseThree(string text) => Page(text, new CommunicationOption("结束通讯", () => Close()));
    }
}
