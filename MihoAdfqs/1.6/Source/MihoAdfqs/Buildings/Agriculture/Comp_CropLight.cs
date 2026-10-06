using System.Linq;
using RimWorld;
using Verse;

namespace MihoAdfqs.Buildings.Agriculture
{
    //职责：维护通电农用灯的照明范围，并在建筑状态变化时刷新光照。
    public class Comp_CropLight : ThingComp
    {
        private bool lastActive;
        public CompProperties_CropLight Props => (CompProperties_CropLight)props;
        public bool Active => parent.Spawned && parent.GetComp<CompPowerTrader>().PowerOn
            && FlickUtility.WantsToBeOn(parent) && parent.GetComp<CompBreakdownable>()?.BrokenDown != true;
        public CellRect Area => Props.occupiedOnly ? parent.OccupiedRect() : CellRect.CenteredOn(parent.Position, Props.width / 2);

        //职责：在建筑生成或读档后加入地图照明表。
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            lastActive = Active;
            parent.Map.GetComponent<MapComponent_CropLight>().Register(this);
        }

        //职责：建筑离图时清除对应光照。
        public override void PostDeSpawn(Map map, DestroyMode mode = DestroyMode.Vanish)
        {
            map.GetComponent<MapComponent_CropLight>().Remove(this);
            base.PostDeSpawn(map, mode);
        }

        //职责：跟随供电、开关与故障信号立即更新照明。
        public override void ReceiveCompSignal(string signal)
        {
            base.ReceiveCompSignal(signal);
            Refresh();
        }

        //职责：在稀有更新中同步没有发出组件信号的状态变化。
        public override void CompTickRare() => Refresh();

        //职责：仅在通电状态改变时重建光照，避免持续刷新整片地面。
        private void Refresh()
        {
            if (!parent.Spawned || lastActive == Active) return;
            lastActive = Active;
            parent.Map.GetComponent<MapComponent_CropLight>().RequestRefresh();
        }

        //职责：选中建筑时显示实际方形边界。
        public override void PostDrawExtraSelectionOverlays() => GenDraw.DrawFieldEdges(Area.ClipInsideMap(parent.Map).Cells.ToList());
    }
}
