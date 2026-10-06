using System;
using System.Linq;
using MihoAdfqs.Phase2.Communications;
using MihoAdfqs.Phase2.UI;
using RimWorld;
using UnityEngine;
using Verse;

namespace MihoAdfqs.Phase2.Orbital
{
    //帝国支援目录与正在执行的指令共用独立作战面板。
    public sealed class Dialog_ImperialSupport : Window
    {
        private readonly Map map;
        private Vector2 scroll;
        private bool showQueue;
        private GameComponent_OrbitalNetwork Network => GameComponent_OrbitalNetwork.Current;
        public override Vector2 InitialSize => new Vector2(Mathf.Min(1080, Verse.UI.screenWidth - 36),
            Mathf.Min(780, Verse.UI.screenHeight - 36));
        protected override float Margin => 0;

        //支援面板不暂停战场，选择目标时关闭面板释放地图输入。
        public Dialog_ImperialSupport(Map map)
        {
            this.map = map;
            doWindowBackground = false; doCloseX = false; doCloseButton = false;
            absorbInputAroundWindow = true; closeOnAccept = false;
        }

        //保留标题、页签和底栏后测量内容区域。
        public override void DoWindowContents(Rect inRect)
        {
            GameFont font = Text.Font;
            TextAnchor anchor = Text.Anchor;
            bool wrap = Text.WordWrap;
            Color color = GUI.color;
            Action action = null;
            try
            {
                GUI.color = Color.white; Text.Font = GameFont.Small; Text.WordWrap = true;
                Text.Anchor = TextAnchor.UpperLeft;
                ImperialSupportGui.DrawShell(inRect);
                Rect area = inRect.ContractedBy(26);
                float small = Text.LineHeightOf(GameFont.Small);
                float header = Text.LineHeightOf(GameFont.Medium) + small + 20;
                ImperialUiStyle.Mark(new Rect(area.x, area.y + 3, 48, 48));
                Text.Font = GameFont.Medium;
                HeadquartersGui.Label(new Rect(area.x + 64, area.y, area.width - 64, Text.LineHeight + 6),
                    "第三帝国 / 支援作战面板", ImperialSupportGui.Ink);
                Text.Font = GameFont.Small;
                HeadquartersGui.Label(new Rect(area.x + 64, area.y + header - small - 8, area.width - 64, small + 8),
                    "帝国点数 " + Network.PointsLabel + "    ·    倒计时随游戏速度推进", ImperialSupportGui.Muted);
                float buttonHeight = Mathf.Max(44, small + 16);
                Rect tabs = new Rect(area.x, area.y + header + 10, area.width, buttonHeight);
                float tabWidth = (tabs.width - 12) / 2;
                if (ImperialSupportGui.Command(new Rect(tabs.x,tabs.y,tabWidth,buttonHeight), "支援目录", showQueue ? 0 : 1))
                    action = () => { showQueue = false; scroll = Vector2.zero; };
                int pending = Network.Deliveries.Count(delivery => delivery.map == map);
                if (ImperialSupportGui.Command(new Rect(tabs.x+tabWidth+12,tabs.y,tabWidth,buttonHeight),
                    "执行队列 / " + pending, showQueue ? 1 : 0))
                    action = () => { showQueue = true; scroll = Vector2.zero; };
                Rect footer = new Rect(area.x, area.yMax - buttonHeight, area.width, buttonHeight);
                Rect body = new Rect(area.x, tabs.yMax + 14, area.width, footer.y - tabs.yMax - 28);
                Action chosen = showQueue ? DrawQueue(body) : DrawCatalog(body);
                if (chosen != null) action = chosen;
                //底栏只保留一个返回入口，悬停反馈限定在按钮内。
                float backWidth = Mathf.Min(footer.width, Mathf.Max(200, Text.CalcSize("返回总部联络").x + 48));
                Rect back = new Rect(footer.xMax - backWidth, footer.y, backWidth, footer.height);
                if (ImperialSupportGui.Command(back, "返回总部联络", Mouse.IsOver(back) ? 1 : 0))
                    action = () => { Close(); Find.WindowStack.Add(new Dialog_ImperialCommunications(map)); };
            }
            finally { Text.Font = font; Text.Anchor = anchor; Text.WordWrap = wrap; GUI.color = color; }
            action?.Invoke();
        }

        //按窗口宽度选择一列或两列，卡片高度由中文说明测量得到。
        private Action DrawCatalog(Rect rect)
        {
            int columns = rect.width >= 720 ? 2 : 1;
            float width = (rect.width - 18 - (columns-1)*14) / columns;
            float line = Text.LineHeightOf(GameFont.Small);
            float[] heights = ImperialSupportOption.All.Select(option =>
                Mathf.Max(108, line*2 + Text.CalcHeight(option.description,Mathf.Max(60,width-124)) + 32) +
                Mathf.Max(44,line+16) + line + 34).ToArray();
            float total = 0;
            for (int i=0;i<heights.Length;i+=columns)
                total += heights.Skip(i).Take(columns).Max() + 14;
            Widgets.BeginScrollView(rect,ref scroll,new Rect(0,0,rect.width-18,Mathf.Max(rect.height,total)));
            Action selected = null;
            try
            {
                float y = 0;
                for (int i=0;i<heights.Length;i+=columns)
                {
                    float height = heights.Skip(i).Take(columns).Max();
                    for (int j=0;j<columns && i+j<heights.Length;j++)
                    {
                        ImperialSupportOption option = ImperialSupportOption.All[i+j];
                        Rect card = new Rect(j*(width+14),y,width,height);
                        ImperialSupportGui.Panel(card);
                        ImperialSupportGui.Icon(new Rect(card.x+16,card.y+20,76,76),option.icon);
                        Rect text = new Rect(card.x+108,card.y+12,card.width-124,line+8);
                        HeadquartersGui.Label(text,option.label,ImperialSupportGui.Ink);
                        HeadquartersGui.Label(new Rect(text.x,text.yMax,text.width,line+6),
                            option.price>0 ? option.price+" 帝国点数" : option.kind=="Guards" ? "元首技能" : "永久战略选项",ImperialSupportGui.Muted);
                        Rect desc = new Rect(text.x,text.yMax+line+12,text.width,
                            Text.CalcHeight(option.description,text.width));
                        GUI.color = ImperialSupportGui.Ink; Widgets.Label(desc,option.description); GUI.color=Color.white;
                        string reason = DisabledReason(option);
                        float buttonHeight = Mathf.Max(44,line+16);
                        Rect button = new Rect(card.x+12,card.yMax-buttonHeight-12,card.width-24,buttonHeight);
                        Rect status = new Rect(card.x+12,button.y-line-10,card.width-24,line+8);
                        Text.WordWrap = false;
                        HeadquartersGui.Label(status,(reason ?? "可调用 · 选择落点后下达").Truncate(status.width),
                            reason == null ? ImperialUiStyle.Signal : ImperialUiStyle.Warning);
                        Text.WordWrap = true;
                        TooltipHandler.TipRegion(status,reason ?? (option.IsBarrage
                            ? "圆圈表示炮弹落点散布区，边缘爆炸可能伤及圈外单位；所有支援均可能伤及友军。"
                            : option.IsStrike ? "显示打击范围；所有支援均可能伤及友军。" : "空投落点会按附近地形调整。"));
                        bool pressed = ImperialSupportGui.Command(button,reason == null ? "下达支援" : "暂不可调用",
                            Mouse.IsOver(button)?1:0,disabled:reason != null);
                        if (pressed && reason==null) selected = () => SelectSupport(option);
                    }
                    y += height + 14;
                }
            }
            finally { Widgets.EndScrollView(); }
            return selected;
        }

        //执行队列显示发射、飞行阶段和实际到达倒计时。
        private Action DrawQueue(Rect rect)
        {
            OrbitalDelivery[] deliveries = Network.Deliveries.Where(delivery => delivery.map==map).ToArray();
            float line=Text.LineHeightOf(GameFont.Small);
            float height=line*3+70;
            Widgets.BeginScrollView(rect,ref scroll,new Rect(0,0,rect.width-18,Mathf.Max(rect.height,deliveries.Length*(height+12))));
            try
            {
                if (deliveries.Length==0)
                    HeadquartersGui.Label(new Rect(12,12,rect.width-42,line*2+12),"当前地图没有正在到达的支援。",ImperialSupportGui.Ink);
                for (int i=0;i<deliveries.Length;i++)
                {
                    OrbitalDelivery delivery=deliveries[i];
                    Rect row=new Rect(0,i*(height+12),rect.width-18,height);
                    ImperialSupportGui.Panel(row);
                    HeadquartersGui.Label(new Rect(16,row.y+8,row.width-32,line+8),
                        ImperialSupportOption.Label(delivery.kind)+"  /  "+(delivery.BarrageActive ? "剩余 "+MapComponent_ImperialSupportMarkers.Countdown(delivery.BarrageTicksLeft)
                            : "T−"+MapComponent_ImperialSupportMarkers.Countdown(delivery.ArrivalTicks)),ImperialSupportGui.Ink);
                    HeadquartersGui.Label(new Rect(16,row.y+line+18,row.width-32,line+8),
                        (delivery.BarrageActive ? "持续炮击 · 已发射 "+delivery.shotsFired+"发" : delivery.dispatched ? "飞行中" : "等待发射")
                            +"  ·  落点 "+delivery.cell.x+", "+delivery.cell.z,ImperialSupportGui.Muted);
                    ImperialSupportOption option = ImperialSupportOption.Get(delivery.kind);
                    string explanation=delivery.kind=="Bombard" ? "红色区域：6×6，友军会受到伤害。"
                        : option.IsStrike ? "范围直径 "+(option.areaRadius*2)+"格，友军也会受影响。" : "蓝色圆圈：预定空降区，舱旁小圈为实际落点。";
                    Rect description=new Rect(16,row.y+line*2+28,row.width-32,line+8);
                    Text.WordWrap=false;
                    HeadquartersGui.Label(description,explanation.Truncate(description.width),ImperialSupportGui.Muted);
                    TooltipHandler.TipRegion(description,explanation);
                    Text.WordWrap=true;
                    int totalTicks=delivery.dueTick-delivery.orderedTick+(option.IsStrike ? 0 : 120);
                    float progress=delivery.BarrageActive ? 1f-(float)delivery.BarrageTicksLeft/option.durationTicks
                        : 1f-(float)delivery.ArrivalTicks/Mathf.Max(1,totalTicks);
                    Widgets.FillableBar(new Rect(16,row.yMax-17,row.width-32,7),Mathf.Clamp01(progress),ImperialSupportGui.BarFill,ImperialSupportGui.BarEmpty,false);
                }
            }
            finally { Widgets.EndScrollView(); }
            return null;
        }

        //寻找本地图具有召唤技能的元首，保留原技能可用性判断。
        private Ability GuardAbility() => map.mapPawns.FreeColonists
            .Select(pawn => pawn.abilities?.GetAbility(DefDatabase<AbilityDef>.GetNamed("Ability_RequestSSReinforcements")))
            .FirstOrDefault(ability => ability!=null);

        //在按钮与下单时使用同一条件，防止余额或技能冷却变化后误调用。
        private string DisabledReason(ImperialSupportOption option)
        {
            if (option.kind=="Gas" && !ModsConfig.BiotechActive) return "毒气打击需要Biotech前置";
            if (option.kind=="Mobilize") return Network.mobilized ? "永久动员已开启" : null;
            if (option.kind=="Guards")
            {
                Ability ability=GuardAbility();
                if (ability==null) return "本地图没有可召唤亲卫的元首";
                return ability.GizmoDisabled(out string reason) ? reason : null;
            }
            return Network.CanAfford(option.price) ? null : "帝国点数不足";
        }

        //战略动员需要确认；战术支援进入地图选点，取消不扣点。
        private void SelectSupport(ImperialSupportOption option)
        {
            string reason=DisabledReason(option);
            if (reason!=null) { Messages.Message(reason,DefDatabase<MessageTypeDef>.GetNamed("MihoPhase2_SilentSupport"),false); return; }
            if (option.kind=="Mobilize")
            {
                Find.WindowStack.Add(new Dialog_ImperialConfirmation("开启永久动员",option.description+"\n\n确认后永久开启此状态。",
                    () => Network.mobilized=true,() => { },"确认 · 永久动员","取消"));
                return;
            }
            Ability ability=option.kind=="Guards" ? GuardAbility() : null;
            Close();
            Find.Targeter.BeginTargeting(new TargetingParameters { canTargetLocations=true,canTargetPawns=false,canTargetBuildings=false },
                target =>
                {
                    if (ability!=null) ability.QueueCastingJob(target,LocalTargetInfo.Invalid);
                    else Network.Purchase(option.kind,option.price,map,target.Cell,option.delay);
                },
                target =>
                {
                    if (option.kind=="Bombard") GenDraw.DrawFieldEdges(OrbitalSupport.StrikeArea(map,target.Cell).Cells.ToList(),Color.red);
                    else if (option.IsStrike) GenDraw.DrawRadiusRing(target.Cell,option.areaRadius,OrbitalStrike.TrailColor(option.kind));
                    else
                    {
                        GenDraw.DrawRadiusRing(target.Cell,5f,new Color(.35f,.85f,1));
                        if (ability!=null) GenDraw.DrawRadiusRing(ability.pawn.Position,ability.verb.verbProps.range);
                    }
                },
                target => Find.CurrentMap==map && target.Cell.InBounds(map) && !target.Cell.Fogged(map) &&
                    (ability==null || ability.verb.CanHitTarget(target)));
        }
    }
}
