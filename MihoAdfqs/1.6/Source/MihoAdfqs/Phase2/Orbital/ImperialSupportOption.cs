using UnityEngine;
using Verse;

namespace MihoAdfqs.Phase2.Orbital
{
    //支援面板中的现有指令，图标与价格集中在同一处。
    [StaticConstructorOnStartup]
    internal sealed class ImperialSupportOption
    {
        public readonly string kind, label, description;
        public readonly int price, delay;
        public readonly float areaRadius, blastRadius;
        public readonly int damage, durationTicks, intervalTicks;
        public readonly Texture2D icon, markerIcon;
        public bool IsStrike => kind == "Bombard" || areaRadius > 0;
        public bool IsBarrage => durationTicks > 0;

        //读取对应的剪影图标。
        private ImperialSupportOption(string kind, string label, string description, int price, int delay,
            float areaRadius = 0, float blastRadius = 0, int damage = 0, int intervalTicks = 0, string iconKind = null)
        {
            this.kind = kind; this.label = label; this.description = description;
            this.price = price; this.delay = delay;
            this.areaRadius = areaRadius; this.blastRadius = blastRadius; this.damage = damage;
            this.intervalTicks = intervalTicks;
            durationTicks = intervalTicks > 0 ? 1200 : 0;
            icon = ContentFinder<Texture2D>.Get("UI/MihoPhase2/Support/" + (iconKind ?? kind));
            markerIcon = ContentFinder<Texture2D>.Get("UI/MihoPhase2/Support/Markers/" + (iconKind ?? kind));
        }

        //范围按半径存储，面板按玩家确认的直径显示。
        public static readonly ImperialSupportOption[] All =
        {
            new ImperialSupportOption("Bombard", "轨道精准打击", "单发 · 6×6区域 · 3秒弹着\n5000爆炸伤害，友军也会受伤。", 40, 180),
            new ImperialSupportOption("Barrage120", "轨道120mm火力网", "7秒到达 · 持续20秒 · 每秒1发\n散布直径30格；每发300伤害，爆炸直径10格。", 100, 420, 15, 5, 300, 60),
            new ImperialSupportOption("Napalm", "轨道凝固汽油弹火力网", "7秒到达 · 持续20秒 · 每2秒1发\n散布直径50格；每发100伤害，铺油点燃直径16格。", 150, 420, 25, 8, 100, 120),
            new ImperialSupportOption("Barrage380", "轨道380mm火力网", "7秒到达 · 持续20秒 · 每2秒1发\n散布直径50格；每发500伤害，爆炸直径16格。", 200, 420, 25, 8, 500, 120),
            new ImperialSupportOption("Gas", "轨道毒气打击", "3秒到达 · 单发 · 毒气直径20格\n毒素积累并造成恐慌，按原版规则扩散消散。", 80, 180, 10),
            new ImperialSupportOption("EMP", "轨道EMP打击", "3秒到达 · 单发 · EMP直径20格\n原版EMP强度，对可受EMP影响的单位生效。", 60, 180, 10),
            new ImperialSupportOption("Soldiers", "轨道士兵", "1游戏小时后开始空降\n3名空降兵和1名战备支援兵，协助防御48游戏小时。", 20, 2500),
            new ImperialSupportOption("Medicine", "药物补给", "立即投送 · 空降约2秒\n50份医药和10份闪耀医药。", 10, 0),
            new ImperialSupportOption("Blueprint", "新星蓝图", "立即投送 · 空降约2秒\n1份N3新星研究蓝图。", 200, 0),
            new ImperialSupportOption("Guards", "召唤亲卫", "元首技能 · 冷却3游戏日\n4名亲卫，可选择队长赫尔默参战。", 0, 0),
            new ImperialSupportOption("Mobilize", "永久动员", "每次袭击增援9名空降兵和1名战备支援兵\n战斗每30秒舰炮支援，元首心情−10。", 0, 0)
        };

        //NPC堡垒使用现有动员图标，不添加玩家购买入口。
        private static readonly ImperialSupportOption Fortress30 = new ImperialSupportOption("Fortress30", "空投30毫米帝国堡垒",
            "7秒到达 · 自供能 · 500发 · 存在120秒", 0, 300, iconKind: "Mobilize");
        private static readonly ImperialSupportOption Fortress88 = new ImperialSupportOption("Fortress88", "空投88毫米帝国堡垒",
            "7秒到达 · 自供能 · 500发 · 存在120秒", 0, 300, iconKind: "Mobilize");

        //将交付类别转换成面板和地图标记使用的名称。
        public static string Label(string kind)
        {
            return Get(kind).label;
        }

        //调度、地图指示器与伤害结算使用同一份支援参数。
        public static ImperialSupportOption Get(string kind)
        {
            if (kind == "Fortress30") return Fortress30;
            if (kind == "Fortress88") return Fortress88;
            foreach (ImperialSupportOption option in All) if (option.kind == kind) return option;
            throw new System.InvalidOperationException("未知帝国支援：" + kind);
        }
    }
}
