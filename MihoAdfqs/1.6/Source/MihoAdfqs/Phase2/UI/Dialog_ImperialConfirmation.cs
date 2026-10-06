using System;
using MihoAdfqs.Phase2.Communications;
using UnityEngine;
using Verse;

namespace MihoAdfqs.Phase2.UI
{
    //以红色铁十字窗口确认亲卫支援，长说明可滚动查看。
    public sealed class Dialog_ImperialConfirmation : Window
    {
        private readonly string title;
        private readonly string message;
        private readonly Action accept;
        private readonly Action decline;
        private readonly string acceptLabel, declineLabel;
        private Vector2 scroll;
        public override Vector2 InitialSize => new Vector2(Mathf.Min(620, Verse.UI.screenWidth - 36),
            Mathf.Min(330, Verse.UI.screenHeight - 36));
        protected override float Margin => 0;

        //使用自绘底板并暂停游戏，确认后才执行对应空投。
        public Dialog_ImperialConfirmation(string title, string message, Action accept, Action decline,
            string acceptLabel = "是 · 队长一同支援", string declineLabel = "否 · 仅四名亲卫")
        {
            this.title = title; this.message = message; this.accept = accept; this.decline = decline;
            this.acceptLabel = acceptLabel; this.declineLabel = declineLabel;
            doWindowBackground = false; doCloseX = false; doCloseButton = false;
            forcePause = true; absorbInputAroundWindow = true;
            closeOnAccept = false; closeOnCancel = false;
        }

        //先保留标题和底部按钮，再布局说明区。
        public override void DoWindowContents(Rect inRect)
        {
            GameFont font = Text.Font;
            TextAnchor anchor = Text.Anchor;
            bool wrap = Text.WordWrap;
            Color color = GUI.color;
            int choice = 0;
            try
            {
                GUI.color = Color.white;
                HeadquartersGui.DrawShell(inRect);
                Rect area = inRect.ContractedBy(28);
                Text.Font = GameFont.Medium; Text.WordWrap = false;
                float headerHeight = Mathf.Max(40, Text.LineHeight + 8);
                ImperialUiStyle.Mark(new Rect(area.x, area.y, 36, 36));
                HeadquartersGui.Label(new Rect(area.x + 48, area.y, area.width - 48, headerHeight), title, ImperialUiStyle.Ink);
                Text.Font = GameFont.Small;
                float buttonHeight = Mathf.Max(42, Text.LineHeight + 12);
                Rect body = new Rect(area.x, area.y + headerHeight + 12, area.width,
                    area.height - headerHeight - buttonHeight - 24);
                ImperialUiStyle.Panel(body);
                Rect viewport = body.ContractedBy(12);
                Text.WordWrap = true; Text.Anchor = TextAnchor.UpperLeft;
                Rect content = new Rect(0, 0, viewport.width - 18, 0);
                content.height = Mathf.Max(viewport.height, Text.CalcHeight(message, content.width));
                Widgets.BeginScrollView(viewport, ref scroll, content);
                GUI.color = ImperialUiStyle.Ink;
                Widgets.Label(content, message);
                Widgets.EndScrollView();
                GUI.color = Color.white; Text.WordWrap = false;
                float width = (area.width - 12) / 2;
                Rect yes = new Rect(area.x, area.yMax - buttonHeight, width, buttonHeight);
                Rect no = new Rect(yes.xMax + 12, yes.y, width, buttonHeight);
                if (HeadquartersGui.Command(yes, acceptLabel, Mouse.IsOver(yes) ? 1 : 0)) choice = 1;
                if (HeadquartersGui.Command(no, declineLabel, Mouse.IsOver(no) ? 1 : 0, true)) choice = -1;
            }
            finally { Text.Font = font; Text.Anchor = anchor; Text.WordWrap = wrap; GUI.color = color; }
            if (choice != 0) Choose(choice > 0);
        }

        //回车执行确认选项，退出键执行第二个选项。
        public override void OnAcceptKeyPressed() { Event.current.Use(); Choose(true); }
        public override void OnCancelKeyPressed() { Event.current.Use(); Choose(false); }

        //先关闭窗口，再执行已确认的支援操作。
        private void Choose(bool withCaptain)
        {
            Close();
            (withCaptain ? accept : decline)();
        }
    }
}
