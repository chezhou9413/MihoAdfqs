using System;
using System.Collections.Generic;
using System.Linq;
using MihoAdfqs.Phase2.Orbital;
using UnityEngine;
using Verse;

namespace MihoAdfqs.Phase2.Communications
{
    //显示总部联络台，保留原有对话、委托和兑换流程。
    public partial class Dialog_ImperialCommunications : Window
    {
        private readonly Map map;
        private readonly List<CommunicationOption> options = new List<CommunicationOption>();
        private string dialogue;
        private Vector2 scroll;
        private float openedAt;
        private float pageChangedAt;
        private float lastDrawAt;
        private float[] optionHover = Array.Empty<float>();
        private float closeHover;
        private GameComponent_OrbitalNetwork Network => GameComponent_OrbitalNetwork.Current;
        public override Vector2 InitialSize => new Vector2(Mathf.Min(1040, Verse.UI.screenWidth - 36),
            Mathf.Min(730, Verse.UI.screenHeight - 36));
        protected override float Margin => 0;

        //使用独立外壳和关闭按钮，保留暂停及取消键行为。
        public Dialog_ImperialCommunications(Map map)
        {
            this.map = map;
            doWindowBackground = false;
            doCloseButton = false;
            forcePause = true;
            absorbInputAroundWindow = true;
            closeOnAccept = false;
            if (Network.introductionStep < 8) ShowIntroduction(); else ShowMain();
        }

        //在窗口真正打开时开始接通动画。
        public override void PreOpen()
        {
            base.PreOpen();
            openedAt = pageChangedAt = lastDrawAt = Time.realtimeSinceStartup;
        }

        //替换对话并重置滚动、选项响应和页面渐显。
        private void Page(string text, params CommunicationOption[] choices)
        {
            dialogue = text;
            options.Clear();
            options.AddRange(choices);
            scroll = Vector2.zero;
            optionHover = new float[choices.Length];
            pageChangedAt = Time.realtimeSinceStartup;
        }

        //按字体和窗口空间布局，在绘制结束后执行选项行为。
        public override void DoWindowContents(Rect inRect)
        {
            GameFont font = Text.Font;
            TextAnchor anchor = Text.Anchor;
            bool wrap = Text.WordWrap;
            Color color = GUI.color;
            Action selected = null;
            try
            {
                Text.Font = GameFont.Small;
                Text.Anchor = TextAnchor.UpperLeft;
                Text.WordWrap = true;
                GUI.color = Color.white;
                float time = Time.realtimeSinceStartup;
                float delta = Mathf.Min(time - lastDrawAt, 0.05f);
                if (Event.current.type == EventType.Repaint) lastDrawAt = time;
                HeadquartersGui.DrawShell(inRect);
                Rect inner = inRect.ContractedBy(28);
                float headerHeight = Text.LineHeightOf(GameFont.Medium) + Text.LineHeightOf(GameFont.Small) + 28;
                DrawHeader(new Rect(inner.x, inner.y, inner.width, headerHeight), time);
                float footerHeight = Mathf.Max(46, Text.LineHeightOf(GameFont.Small) + 20);
                Rect footer = new Rect(inner.x, inner.yMax - footerHeight, inner.width, footerHeight);
                Rect body = new Rect(inner.x, inner.y + headerHeight + 16, inner.width,
                    footer.y - inner.y - headerHeight - 32);
                float sideWidth = Mathf.Min(232, body.width * 0.28f);
                //较窄或较矮的屏幕收起仪表栏，将空间交给对话和选项。
                bool showInstruments = body.width >= 650 && body.height >= 360;
                if (showInstruments) DrawInstruments(new Rect(body.x, body.y, sideWidth, body.height), time);
                float gap = showInstruments ? sideWidth + 18 : 0;
                Rect content = new Rect(body.x + gap, body.y, body.width - gap, body.height);
                selected = DrawConversation(content, time, delta);
                if (DrawFooter(footer, delta)) selected = () => Close();
            }
            finally { Text.Font = font; Text.Anchor = anchor; Text.WordWrap = wrap; GUI.color = color; }
            selected?.Invoke();
        }

        //测量可滚动的对话与指令，避免长任务说明挤压选项。
        private Action DrawConversation(Rect rect, float time, float delta)
        {
            HeadquartersGui.Panel(rect);
            Rect area = rect.ContractedBy(20);
            float titleHeight = Text.LineHeightOf(GameFont.Small) + 10;
            HeadquartersGui.Label(new Rect(area.x, area.y, area.width, titleHeight),
                "卡黛尔  /  总部联络官", HeadquartersGui.Accent);
            float ruleY = area.y + titleHeight + 4;
            Widgets.DrawBoxSolid(new Rect(area.x, ruleY, area.width, 1), HeadquartersGui.Edge);
            Rect outer = new Rect(area.x, ruleY + 16, area.width, area.yMax - ruleY - 16);
            float width = outer.width - 18;
            float textHeight = Text.CalcHeight(dialogue, width);
            float[] heights = options.Select(o => Mathf.Max(48,
                Text.CalcHeight(o.label, width - 50) + 20)).ToArray();
            float choicesY = textHeight + 32;
            Rect view = new Rect(0, 0, width, Mathf.Max(outer.height,
                choicesY + heights.Sum() + 10 * options.Count));
            Action selected = null;
            Widgets.BeginScrollView(outer, ref scroll, view);
            try
            {
                float enter = Mathf.SmoothStep(0, 1, Mathf.Clamp01((time - pageChangedAt) / 0.24f));
                GUI.color = new Color(HeadquartersGui.Ink.r, HeadquartersGui.Ink.g,
                    HeadquartersGui.Ink.b, 0.4f + enter * 0.6f);
                Widgets.Label(new Rect(0, (1 - enter) * 6, width, textHeight), dialogue);
                GUI.color = Color.white;
                for (int i = 0; i < options.Count; i++)
                {
                    Rect button = new Rect(0, choicesY, width, heights[i]);
                    HeadquartersGui.UpdateHover(button, ref optionHover[i], delta);
                    if (HeadquartersGui.Command(button, options[i].label, optionHover[i])) selected = options[i].action;
                    choicesY += heights[i] + 10;
                }
            }
            finally { Widgets.EndScrollView(); }
            return selected;
        }
    }
}
