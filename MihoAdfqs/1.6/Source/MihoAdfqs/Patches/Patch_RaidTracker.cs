using HarmonyLib;
using MihoAdfqs.MihoAdfWorldComponent;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MihoAdfqs.Patches
{
    // 统计所有袭击：凡是继承 IncidentWorker_Raid 的事件（RaidEnemy/RaidFriendly/机械族等）只要成功执行就计数。
    [HarmonyPatch(typeof(IncidentWorker_Raid), "TryExecuteWorker")]
    public static class Patch_RaidTracker
    {
        // Postfix 表示在原版函数执行完毕后运行
        public static void Postfix(bool __result)
        {
            if (__result)
            {
                MapComp_MihoRaidTracker.RegisterRaid();
            }
        }
    }
}
