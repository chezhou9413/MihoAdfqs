using System.Collections.Generic;
using System.Linq;
using MihoAdfqs.Buildings.Medical;
using MihoAdfqs.Phase2.Communications;
using MihoAdfqs.Phase2.UI;
using RimWorld;
using UnityEngine;
using Verse;

namespace MihoAdfqs.Buildings.UI
{
    //集中显示医疗舱的患者、药物搬运和手术操作。
    public sealed class Dialog_ImperialBuilding : Window
    {
        internal readonly Building Building;
        internal Pawn Operator;
        private Vector2 scroll;
        private float contentHeight = 900;
        public override Vector2 InitialSize => new Vector2(Mathf.Min(720, Verse.UI.screenWidth - 40),
            Mathf.Min(720, Verse.UI.screenHeight - 40));
        protected override float Margin => 0;

        //保留右键选定的操作者，直接打开时选择能行动的殖民者。
        public Dialog_ImperialBuilding(Building building, Pawn operatorPawn = null)
        {
            Building = building;
            Operator = operatorPawn ?? building.Map.mapPawns.FreeColonists.FirstOrDefault(p => p.Spawned && !p.Downed && !p.Dead);
            doWindowBackground = false; doCloseX = false; doCloseButton = false;
            absorbInputAroundWindow = true; closeOnAccept = false;
        }

        //建筑离图后关闭管理窗口，避免继续操作已经失效的目标。
        public override void WindowUpdate()
        {
            base.WindowUpdate();
            if (!Building.Spawned || Building.Faction != Faction.OfPlayer) Close();
        }

        //为标题和底部关闭按钮预留空间，正文按实际高度滚动。
        public override void DoWindowContents(Rect inRect)
        {
            GameFont font = Text.Font; TextAnchor anchor = Text.Anchor;
            bool wrap = Text.WordWrap; Color color = GUI.color;
            try
            {
                GUI.color = Color.white; Text.Font = GameFont.Small; Text.WordWrap = true;
                HeadquartersGui.DrawShell(inRect);
                Rect area = inRect.ContractedBy(28);
                float titleHeight = Mathf.Max(64, Text.CalcHeight(Building.LabelCap, area.width - 86) + 12);
                Widgets.DrawTextureFitted(new Rect(area.x, area.y, 64, 64), Building.def.uiIcon, 1);
                HeadquartersGui.Label(new Rect(area.x + 82, area.y, area.width - 82, titleHeight), Building.LabelCap, ImperialUiStyle.Ink);
                float footer = Mathf.Max(44, Text.LineHeight + 16);
                Rect viewport = new Rect(area.x, area.y + titleHeight + 12, area.width, area.height - titleHeight - footer - 24);
                ImperialUiStyle.Panel(viewport);
                viewport = viewport.ContractedBy(12);
                Rect content = new Rect(0, 0, viewport.width - 20, Mathf.Max(viewport.height, contentHeight));
                Widgets.BeginScrollView(viewport, ref scroll, content);
                try
                {
                    var listing = new BuildingControlListing(content.width);
                    listing.Label("状态：" + OperatingState());
                    MedicalPodControls.Draw(listing, (Building_MedicalPod)Building, this);
                    contentHeight = listing.Height;
                }
                finally { Widgets.EndScrollView(); }
                Rect close = new Rect(area.x, area.yMax - footer, area.width, footer);
                if (HeadquartersGui.Command(close, "关闭面板", Mouse.IsOver(close) ? 1 : 0, true)) Close();
            }
            finally { Text.Font = font; Text.Anchor = anchor; Text.WordWrap = wrap; GUI.color = color; }
        }

        //按故障、电力和开关状态解释设备当前是否能工作。
        private string OperatingState()
        {
            if (Building.GetComp<CompBreakdownable>()?.BrokenDown == true) return "设备故障，需要维修";
            if (!FlickUtility.WantsToBeOn(Building)) return "开关关闭，等待开启";
            CompPowerTrader power = Building.GetComp<CompPowerTrader>();
            return power != null && !power.PowerOn ? "未通电，请接通电网" : "可用";
        }

        //选择实际执行搬运和入舱指令的殖民者。
        internal void DrawOperator(BuildingControlListing listing)
        {
            if (listing.Button("操作者：" + (Operator?.LabelShort ?? "未选择") + " · 点击更换"))
            {
                var options = new List<FloatMenuOption>();
                foreach (Pawn pawn in Building.Map.mapPawns.FreeColonists)
                {
                    Pawn selected = pawn;
                    bool usable = pawn.Spawned && !pawn.Downed && !pawn.Dead;
                    options.Add(new FloatMenuOption(pawn.LabelShort + (usable ? "" : "（无法行动）"),
                        usable ? (System.Action)(() => Operator = selected) : null));
                }
                if (options.Count == 0) options.Add(new FloatMenuOption("本地图没有可用殖民者", null));
                Find.WindowStack.Add(new FloatMenu(options));
            }
        }

    }
}
