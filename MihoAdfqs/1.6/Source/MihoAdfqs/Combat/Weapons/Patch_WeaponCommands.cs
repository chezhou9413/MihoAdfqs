using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Verse;
using MihoAdfqs.Phase2.UI;

namespace MihoAdfqs.Combat.Weapons
{
    //让原版攻击AI使用当前选中的主枪或下挂。
    [HarmonyPatch(typeof(VerbTracker), "get_PrimaryVerb")]
    public static class Patch_SelectedWeaponVerb
    {
        //直接遍历现有动词，避免高频主武器读取分配LINQ枚举器。
        public static void Postfix(VerbTracker __instance, ref Verb __result)
        {
            List<Verb> verbs = __instance.AllVerbs;
            for (int i = 0; i < verbs.Count; i++)
                if (verbs[i] is Verb_Magazine selected && selected.Selected)
                {
                    __result = selected;
                    return;
                }
        }
    }

    //将弹药状态和玩家可用的武器控制按钮加入已装备武器的命令列表。
    [HarmonyPatch(typeof(CompEquippable), nameof(CompEquippable.CompGetEquippedGizmosExtra))]
    public static class Patch_WeaponCommands
    {
        //保留已有命令，非玩家控制的持枪单位只显示弹药状态。
        public static IEnumerable<Gizmo> Postfix(IEnumerable<Gizmo> __result, CompEquippable __instance)
        {
            foreach (Gizmo gizmo in __result) yield return gizmo;
            var armor = __instance.parent.GetComp<MihoAdfqs.Combat.Armor.CompArmorReserve>();
            if (armor != null && __instance.PrimaryVerb.CasterPawn != null)
                foreach (Gizmo gizmo in armor.Commands(__instance.PrimaryVerb.CasterPawn)) yield return gizmo;
            var verbs = __instance.AllVerbs.OfType<Verb_Magazine>().ToList();
            foreach (Verb_Magazine verb in verbs.Where(v => v.Selected))
                foreach (Gizmo gizmo in verb.GetCommands()) yield return gizmo;
            if (verbs.Count < 2 || !verbs[0].PlayerCanControl) yield break;
            var command = new Command_ImperialAction
            {
                defaultLabel = "切换主枪／下挂", defaultDesc = "两个枪管分别保存弹药。",
                icon = __instance.parent.def.uiIcon,
                action = () => Verb_Magazine.Select(verbs.First(v => !v.Selected))
            };
            if (verbs.Any(v => v.Bursting || v.WarmingUp)) command.Disable("射击结束后切换");
            yield return command;
        }
    }
}
