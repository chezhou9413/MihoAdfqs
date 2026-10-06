using MihoAdfqs.Phase2.UI;
using MihoAdfqs.Combat.Weapons;
using UnityEngine;
using Verse;

namespace MihoAdfqs.Combat.UI
{
    //加载由SVG导出的各类弹药图标，保持暗红界面的配色。
    [StaticConstructorOnStartup]
    internal static class MagazineUiAssets
    {
        public static readonly Texture2D Round = ContentFinder<Texture2D>.Get("UI/MihoPhase2/Weapons/MagazineRound");
        private static readonly Texture2D Shell = ContentFinder<Texture2D>.Get("UI/MihoPhase2/Weapons/MagazineShell");
        private static readonly Texture2D Rocket = ContentFinder<Texture2D>.Get("UI/MihoPhase2/Weapons/MagazineRocket");
        private static readonly Texture2D Grenade = ContentFinder<Texture2D>.Get("UI/MihoPhase2/Weapons/MagazineGrenade");
        private static readonly Texture2D Shotgun = ContentFinder<Texture2D>.Get("UI/MihoPhase2/Weapons/MagazineShotgun");
        private static readonly Texture2D Laser = ContentFinder<Texture2D>.Get("UI/MihoPhase2/Weapons/MagazineLaser");
        public static readonly Texture2D Reload = ContentFinder<Texture2D>.Get("UI/MihoPhase2/Weapons/ReloadCycle");
        public static readonly Color Loaded = new Color(0.72f, 0.61f, 0.40f);
        public static readonly Color Empty = ImperialUiStyle.Edge;
        public static readonly Color Reloading = ImperialUiStyle.Accent;

        //每种弹药采用独立轮廓，主枪与下挂可分别配置。
        public static Texture2D Icon(MagazineWeaponType type)
        {
            switch (type)
            {
                case MagazineWeaponType.Shell: return Shell;
                case MagazineWeaponType.Rocket: return Rocket;
                case MagazineWeaponType.Grenade: return Grenade;
                case MagazineWeaponType.Shotgun: return Shotgun;
                case MagazineWeaponType.Laser: return Laser;
                default: return Round;
            }
        }

        //提示玩家当前枪管使用的弹药类型。
        public static string TypeLabel(MagazineWeaponType type)
        {
            switch (type)
            {
                case MagazineWeaponType.Shell: return "炮弹";
                case MagazineWeaponType.Rocket: return "火箭";
                case MagazineWeaponType.Grenade: return "榴弹／手雷";
                case MagazineWeaponType.Shotgun: return "霰弹";
                case MagazineWeaponType.Laser: return "激光能量";
                default: return "子弹";
            }
        }

        //激光补充能量，其余弹药沿用装填措辞。
        public static string ReloadLabel(MagazineWeaponType type) => type == MagazineWeaponType.Laser ? "充能" : "装弹";

        //能量格采用柔和的红色，实体弹药保留黄铜色。
        public static Color LoadedColor(MagazineWeaponType type) => type == MagazineWeaponType.Laser
            ? new Color(.72f, .40f, .34f) : Loaded;

        //在指定区域绘制完整或部分填充的弹药，裁剪保持原始长宽比例。
        public static void DrawRound(Rect rect, Texture2D icon, float fraction, Color color)
        {
            Color previous = GUI.color;
            GUI.color = Empty;
            GUI.DrawTexture(rect, icon);
            if (fraction > 0)
            {
                fraction = Mathf.Clamp01(fraction);
                GUI.color = color;
                Rect filled = new Rect(rect.x, rect.yMax - rect.height * fraction, rect.width, rect.height * fraction);
                GUI.DrawTextureWithTexCoords(filled, icon, new Rect(0, 0, 1, fraction));
            }
            GUI.color = previous;
        }
    }
}
