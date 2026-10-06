using MihoAdfqs.Combat.Weapons;
using UnityEngine;
using Verse;

namespace MihoAdfqs.Combat.UI
{
    //每个正在装弹的枪管缓存文字与测量结果，进度条仍每帧读取实时进度。
    internal sealed class ReloadOverlayText
    {
        private readonly string label;
        private int nextTextTick = -1;
        private int reloadEndTick;
        private float lineHeight;
        private float uiScale;
        internal string Status { get; private set; }
        internal float Width { get; private set; }

        //枪管类型和主副标签在动词存续期间保持不变。
        internal ReloadOverlayText(Verb_Magazine verb)
        {
            label = (((VerbProperties_Magazine)verb.verbProps).secondary ? "下挂" : "")
                + MagazineUiAssets.ReloadLabel(verb.WeaponType);
        }

        //文字每六刻刷新，新一轮换弹或字体缩放变化时立即重新测量。
        internal void Update(int tick, int remaining, float progress)
        {
            int end = tick + remaining;
            float height = Text.LineHeight;
            float scale = Prefs.UIScale;
            if (tick < nextTextTick && end == reloadEndTick && height == lineHeight && scale == uiScale) return;
            Status = $"{label} {progress:P0}  {remaining / 60f:F1}s";
            Width = Mathf.Max(146, Text.CalcSize(Status).x + 34);
            nextTextTick = tick + 6;
            reloadEndTick = end;
            lineHeight = height;
            uiScale = scale;
        }
    }
}
