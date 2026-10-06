using System.Collections.Generic;
using HarmonyLib;
using Verse;

namespace MihoAdfqs.Combat.Weapons
{
    //强制攻击只显示当前枪管，避免相同武器的两个按钮合并后绑定未选动词。
    [HarmonyPatch(typeof(VerbTracker), nameof(VerbTracker.GetVerbsCommands))]
    public static class Patch_SelectedWeaponCommands
    {
        //保留原版权限和范围，换弹期间也允许下达目标，实际发射由动词检查弹药。
        public static IEnumerable<Command> Postfix(IEnumerable<Command> __result)
        {
            foreach (Command command in __result)
            {
                if (command is Command_VerbTarget target && target.verb is Verb_Magazine magazine)
                {
                    if (!magazine.Selected) continue;
                    target.requiresAvailableVerb = false;
                }
                yield return command;
            }
        }
    }
}
