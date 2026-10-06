using Verse;

namespace MihoAdfqs.Phase2.DebugTools
{
    //只铺设平坦地面，不生成山体、建筑、植物或动物。
    public sealed class GenStep_ImperialDebugField : GenStep
    {
        public override int SeedPart => 19370426;

        //在地图完成路径与区域初始化前，铺满不可长草的混凝土地面。
        public override void Generate(Map map, GenStepParams parms)
        {
            TerrainDef floor = DefDatabase<TerrainDef>.GetNamed("Concrete");
            foreach (IntVec3 cell in map.AllCells) map.terrainGrid.SetTerrain(cell, floor);
            MapGenerator.PlayerStartSpot = map.Center;
        }
    }
}
