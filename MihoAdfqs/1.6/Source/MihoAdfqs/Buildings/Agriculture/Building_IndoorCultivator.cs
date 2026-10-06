using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace MihoAdfqs.Buildings.Agriculture
{
    //维持培养机内的植物生长，并自动收割成熟作物。
    public class Building_IndoorCultivator : Building_PlantGrower
    {
        private Graphic interior;

        //内衬写入建筑高度的静态网格，避免地面杂物穿过种植床。
        public override void Print(SectionLayer layer)
        {
            if (interior == null)
                interior = GraphicDatabase.Get<Graphic_Single>(
                    "Building/MihoPhase2/IndoorCultivator/MihoPhase2_IndoorCultivatorInterior_east",
                    ShaderDatabase.Cutout, def.graphicData.drawSize, Color.white);
            Vector3 center = this.TrueCenter() + def.graphicData.DrawOffsetForRot(Rotation);
            center.y = def.Altitude;
            Printer_Plane.PrintPlane(layer, center, interior.drawSize, interior.MatSingle,
                topVerticesAltitudeBias: 0f);
            base.Print(layer);
        }

        //透明顶盖单独绘制，保持原贴图的窗框、控制面板和玻璃透明度。
        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            base.DrawAt(drawLoc, flip);
            Graphic.Draw(drawLoc + Altitudes.AltIncVect * 4f, Rotation, this);
        }

        //在供电正常时收获成熟作物，保留人工重新播种流程。
        public override void TickRare()
        {
            base.TickRare();
            if (!GetComp<CompPowerTrader>().PowerOn) return;
            CellRect growingArea = this.OccupiedRect();
            IntVec3 output = new IntVec3(growingArea.minX - 1, Position.y, Position.z);
            //砍树需要真实殖民者记录行为，机器只自动收获非树木作物。
            foreach (Plant plant in PlantsOnMe.Where(p => !p.def.plant.IsTree
                && p.Growth >= 1 && p.CanYieldNow()).ToList())
            {
                int count = plant.YieldNow();
                if (count <= 0) continue;
                Thing crop = ThingMaker.MakeThing(plant.def.plant.harvestedThingDef);
                crop.stackCount = count;
                //收获物放到培养区外，避免占用刚空出的播种位置。
                if (!GenPlace.TryPlaceThing(crop, output, Map, ThingPlaceMode.Near,
                    extraValidator: cell => !(cell.GetEdifice(Map) is Building_PlantGrower)))
                {
                    crop.Destroy();
                    continue;
                }
                //机器收获不追加殖民者的砍伐队列。
                plant.PlantCollected(null, PlantDestructionMode.Chop);
            }
        }
    }
}
