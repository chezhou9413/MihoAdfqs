using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace MihoAdfqs.Phase2.Orbital
{
    //仅为帝国毒气区中的可中毒单位触发原版恐慌逃跑。
    public sealed class MapComponent_ImperialGasPanic : MapComponent
    {
        private List<IntVec3> clouds = new List<IntVec3>();
        private List<Pawn> exposed = new List<Pawn>();

        //气体本身由原版GasGrid保存和消散，此处只记录恐慌来源。
        public MapComponent_ImperialGasPanic(Map map) : base(map) { }

        //弹着后立即检查范围内单位，之后继续检查新进入的单位。
        public void AddCloud(IntVec3 center)
        {
            if (!clouds.Contains(center)) clouds.Add(center);
            CheckExposure();
        }

        //每秒检查一次，不持续扫描全地图格子。
        public override void MapComponentTick()
        {
            if (Find.TickManager.TicksGame % 60 == 0 && clouds.Count > 0) CheckExposure();
        }

        //同一次暴露只触发一次，离开毒气后结束本组件触发的恐慌。
        private void CheckExposure()
        {
            clouds.RemoveAll(center => !GenRadial.RadialCellsAround(center, 10f, true)
                .Any(cell => cell.InBounds(map) && map.gasGrid.DensityAt(cell, GasType.ToxGas) > 0));
            foreach (Pawn pawn in exposed.ToList())
            {
                if (pawn == null) { exposed.Remove(pawn); continue; }
                if (pawn.Spawned && pawn.Map == map && InCloud(pawn)) continue;
                if (!pawn.Dead && pawn.MentalStateDef == MentalStateDefOf.PanicFlee
                    && pawn.MentalState.sourceFaction == OrbitalSupport.Empire)
                    pawn.MentalState.RecoverFromState();
                exposed.Remove(pawn);
            }
            foreach (Pawn pawn in map.mapPawns.AllPawnsSpawned.ToList())
            {
                if (pawn.Dead || pawn.Downed || pawn.InMentalState || exposed.Contains(pawn) || !pawn.RaceProps.IsFlesh
                    || pawn.GetStatValue(StatDefOf.ToxicResistance) >= 1f
                    || pawn.GetStatValue(StatDefOf.ToxicEnvironmentResistance) >= 1f || !InCloud(pawn)) continue;
                if (pawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.PanicFlee,
                    "帝国恐慌毒气", forced: true, forceWake: true, causedByDamage: true))
                {
                    pawn.MentalState.forceRecoverAfterTicks = 600;
                    pawn.MentalState.sourceFaction = OrbitalSupport.Empire;
                    exposed.Add(pawn);
                }
            }
        }

        //必须实际接触毒气，墙后或毒气已消散的格子不触发恐慌。
        private bool InCloud(Pawn pawn) => map.gasGrid.DensityAt(pawn.Position, GasType.ToxGas) > 0
            && clouds.Any(center => center.DistanceToSquared(pawn.Position) <= 100);

        //保存尚未消散的打击区和已受惊单位，防止读档重复触发。
        public override void ExposeData()
        {
            Scribe_Collections.Look(ref clouds, "imperialGasClouds", LookMode.Value);
            Scribe_Collections.Look(ref exposed, "imperialGasExposed", LookMode.Reference);
        }
    }
}
