using System.Collections.Generic;
using MihoAdfqs.Phase2.UI;
using RimWorld;
using Verse;

namespace MihoAdfqs.Buildings.UI
{
    //为医疗舱提供原版界面未覆盖的治疗与手术入口。
    public class Comp_ImperialBuildingControls : ThingComp
    {
        //仅在选中已建成的玩家建筑时提供管理按钮。
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (parent.Faction != Faction.OfPlayer) yield break;
            yield return new Command_ImperialAction
            {
                defaultLabel = "医疗舱管理",
                defaultDesc = "安排药物搬运、患者入舱，查看治疗进度并选择部件重建、复活或种族转换。",
                icon = parent.def.uiIcon,
                action = () => Find.WindowStack.Add(new Dialog_ImperialBuilding((Building)parent))
            };
        }
    }
}
