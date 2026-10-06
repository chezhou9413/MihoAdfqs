using HarmonyLib;
using System.Reflection;
using Verse;
using UnityEngine;
using WeaponMuzzleFramework.Runtime;

namespace MihoAdfqs
{
    //加载结束后注册补丁，避免提前触发 HAR 的静态初始化。
    public class MihoAdfqsMain : Mod
    {
        //加载过程中只安排注册，补丁在 Def 就绪后的主线程安装。
        public MihoAdfqsMain(ModContentPack content) : base(content)
        {
            Harmony harmony = new Harmony("chezhou.kind.mihoadfqs");
            LongEventHandler.ExecuteWhenFinished(() => harmony.PatchAll(Assembly.GetExecutingAssembly()));
        }

        //职责：提供本模组的设置分类名称。
        public override string SettingsCategory() => Content.Name;

        //职责：在模组设置中提供共享枪口编辑器入口。
        public override void DoSettingsWindowContents(Rect inRect)
        {
            MuzzleEditor.DrawSettingsButton(inRect);
        }
    }
}
