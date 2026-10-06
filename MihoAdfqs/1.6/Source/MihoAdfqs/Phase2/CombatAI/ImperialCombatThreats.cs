using System.Collections.Generic;
using HarmonyLib;
using MihoAdfqs.Combat.Projectiles;
using MihoAdfqs.Phase2.Orbital;
using RimWorld;
using UnityEngine;
using Verse;

namespace MihoAdfqs.Phase2.CombatAI
{
    //每张地图共用弹道快照，按十二格分区查询附近威胁。
    internal sealed class ImperialCombatThreats
    {
        private static readonly AccessTools.FieldRef<Projectile, Vector3> Destination =
            AccessTools.FieldRefAccess<Projectile, Vector3>("destination");
        private static readonly AccessTools.FieldRef<Projectile, int> ImpactTicks =
            AccessTools.FieldRefAccess<Projectile, int>("ticksToImpact");
        private static readonly AccessTools.FieldRef<Projectile, bool> Landed =
            AccessTools.FieldRefAccess<Projectile, bool>("landed");
        private static readonly AccessTools.FieldRef<Projectile_Explosive, int> DetonationTicks =
            AccessTools.FieldRefAccess<Projectile_Explosive, int>("ticksToDetonation");
        private readonly Map map;
        private readonly Dictionary<int, List<Threat>> buckets = new Dictionary<int, List<Threat>>();
        private readonly List<Threat> orbital = new List<Threat>();
        private int sampledTick = -9999;

        //圆形、方形和线段都在地面平面上判断，不使用射弹的视觉高度。
        private struct Threat
        {
            public Vector3 from, to;
            public float radius, weight;
            public CellRect? rect;
            public bool line, wallBlocked;
            public Thing launcher, intended;
            public ProjectileHitFlags flags;
            public Projectile projectile;
        }

        //组件构造时只记录地图，不访问尚未初始化的网格。
        public ImperialCombatThreats(Map map) { this.map = map; }

        //所有人物复用同一次投射物遍历和轨道队列采样。
        public void Refresh()
        {
            int tick = Find.TickManager.TicksGame;
            if (sampledTick == tick) return;
            sampledTick = tick;
            foreach (List<Threat> list in buckets.Values) list.Clear();
            orbital.Clear();
            foreach (Thing thing in map.listerThings.ThingsInGroup(ThingRequestGroup.Projectile))
                if (thing is Projectile projectile) AddProjectile(projectile);
            foreach (Thing thing in map.listerThings.ThingsOfDef(ThingDefOf.Explosion))
            {
                var explosion = (Explosion)thing;
                Add(new Threat { from = explosion.Position.ToVector3Shifted(), to = explosion.Position.ToVector3Shifted(),
                    radius = explosion.radius + .75f, weight = 60f, wallBlocked = true });
            }
            foreach (OrbitalDelivery delivery in GameComponent_OrbitalNetwork.Current.Deliveries)
            {
                if (delivery.map != map) continue;
                ImperialSupportOption option = ImperialSupportOption.Get(delivery.kind);
                if (!option.IsStrike) continue;
                if (!option.IsBarrage && delivery.dispatched && delivery.ArrivalTicks == 0) continue;
                Threat threat = new Threat { from = delivery.cell.ToVector3Shifted(), to = delivery.cell.ToVector3Shifted(), weight = 100f };
                if (delivery.kind == "Bombard") threat.rect = OrbitalSupport.StrikeArea(map, delivery.cell).ExpandedBy(2);
                else threat.radius = (option.IsBarrage ? option.areaRadius + option.blastRadius
                    : Mathf.Max(option.areaRadius, option.blastRadius)) + 2f;
                orbital.Add(threat);
                Add(threat);
            }
        }

        //预测未来三十六刻的弹道，并收录四十五刻内引爆的落区。
        private void AddProjectile(Projectile projectile)
        {
            bool landed = Landed(projectile);
            int ticks = landed && projectile is Projectile_Explosive explosive
                ? DetonationTicks(explosive) : ImpactTicks(projectile);
            Vector3 from = (landed ? projectile.Position.ToVector3Shifted() : projectile.ExactPosition).Yto0();
            Vector3 destination = landed ? from : Destination(projectile).Yto0();
            if (ticks <= 45)
            {
                AreaPayload payload = projectile.def.GetModExtension<AreaPayload>();
                var blast = new Threat { from = destination, to = destination, weight = 60f, projectile = projectile };
                if (payload != null)
                {
                    int width = payload.width, height = payload.height > 0 ? payload.height : width;
                    if (payload.orientToFlight && Mathf.Abs(destination.x - from.x) > Mathf.Abs(destination.z - from.z))
                    { int previous = width; width = height; height = previous; }
                    IntVec3 cell = destination.ToIntVec3();
                    blast.rect = new CellRect(cell.x - width / 2, cell.z - height / 2, width, height).ExpandedBy(1);
                    Add(blast);
                }
                else if (projectile.def.projectile.explosionRadius > 0)
                {
                    blast.radius = projectile.def.projectile.explosionRadius + .75f;
                    blast.wallBlocked = true;
                    Add(blast);
                }
            }
            if (landed || projectile.def.projectile.flyOverhead) return;
            Add(new Threat { from = from, to = Vector3.Lerp(from, destination, ticks > 0 ? Mathf.Min(1f, 36f / ticks) : 1f),
                radius = 1.25f, weight = 24f, line = true, wallBlocked = true, launcher = projectile.Launcher,
                intended = projectile.intendedTarget.Thing, flags = projectile.HitFlags, projectile = projectile });
        }

        //赫尔默复用地图快照，只取翻滚范围附近的射弹，再按原有规则评分。
        public void NearbyProjectiles(IntVec3 center, HashSet<Projectile> result)
        {
            if (Find.TickManager.TicksGame - sampledTick >= 6) Refresh();
            int columns = (map.Size.x + 11) / 12;
            int minX = Mathf.Max(0, center.x - 8) / 12, maxX = Mathf.Min(map.Size.x - 1, center.x + 8) / 12;
            int minZ = Mathf.Max(0, center.z - 8) / 12, maxZ = Mathf.Min(map.Size.z - 1, center.z + 8) / 12;
            for (int z = minZ; z <= maxZ; z++)
                for (int x = minX; x <= maxX; x++)
                    if (buckets.TryGetValue(x + z * columns, out List<Threat> nearby))
                        for (int i = 0; i < nearby.Count; i++)
                        {
                            Projectile shot = nearby[i].projectile;
                            if (shot != null && shot.Spawned && shot.Map == map) result.Add(shot);
                        }
        }

        //每个威胁只登记到所覆盖的分区，查询不扫描整张地图。
        private void Add(Threat threat)
        {
            int minX, minZ, maxX, maxZ;
            if (threat.rect.HasValue)
            {
                CellRect rect = threat.rect.Value;
                minX = rect.minX; maxX = rect.maxX; minZ = rect.minZ; maxZ = rect.maxZ;
            }
            else
            {
                minX = Mathf.FloorToInt(Mathf.Min(threat.from.x, threat.to.x) - threat.radius);
                maxX = Mathf.CeilToInt(Mathf.Max(threat.from.x, threat.to.x) + threat.radius);
                minZ = Mathf.FloorToInt(Mathf.Min(threat.from.z, threat.to.z) - threat.radius);
                maxZ = Mathf.CeilToInt(Mathf.Max(threat.from.z, threat.to.z) + threat.radius);
            }
            minX = Mathf.Max(0, minX) / 12; maxX = Mathf.Min(map.Size.x - 1, maxX) / 12;
            minZ = Mathf.Max(0, minZ) / 12; maxZ = Mathf.Min(map.Size.z - 1, maxZ) / 12;
            for (int x = minX; x <= maxX; x++)
                for (int z = minZ; z <= maxZ; z++)
                {
                    int key = x + z * ((map.Size.x + 11) / 12);
                    if (!buckets.TryGetValue(key, out List<Threat> list)) buckets.Add(key, list = new List<Threat>());
                    list.Add(threat);
                }
        }

        //火焰和毒气不作为安全落点。
        public bool UnsafeTerrain(IntVec3 cell)
        {
            if (map.gasGrid.DensityAt(cell, GasType.ToxGas) > 0) return true;
            foreach (Thing thing in cell.GetThingList(map)) if (thing is Fire) return true;
            return false;
        }

        //圆形危险区的中心风险更高，离开方形区域前仍保持高风险。
        private float Weight(Threat threat, IntVec3 cell)
        {
            if (threat.rect.HasValue) return threat.rect.Value.Contains(cell) ? threat.weight : 0f;
            Vector3 position = cell.ToVector3Shifted().Yto0();
            Vector3 from = threat.from.Yto0(), to = threat.to.Yto0();
            Vector3 segment = to - from;
            float fraction = threat.line && segment.sqrMagnitude > .001f
                ? Mathf.Clamp01(Vector3.Dot(position - from, segment) / segment.sqrMagnitude) : 1f;
            float distanceSquared = (position - (from + segment * fraction)).sqrMagnitude;
            if (distanceSquared >= threat.radius * threat.radius) return 0f;
            return threat.weight * (1f - Mathf.Sqrt(distanceSquared) / threat.radius) + 5f;
        }

        //轨道避险不依赖敌友关系或墙体视线。
        public float OrbitalRisk(IntVec3 cell)
        {
            float risk = 0;
            foreach (Threat threat in orbital) risk += Weight(threat, cell);
            return risk;
        }

        //真实友军伤害和目标命中标志共同决定射弹是否威胁该人物。
        public float Risk(Pawn pawn, IntVec3 cell)
        {
            if (!cell.InBounds(map) || UnsafeTerrain(cell)) return 1000f;
            int key = cell.x / 12 + cell.z / 12 * ((map.Size.x + 11) / 12);
            if (!buckets.TryGetValue(key, out List<Threat> list)) return 0f;
            float risk = 0;
            foreach (Threat threat in list)
            {
                if (threat.line && (threat.launcher == pawn || ((threat.flags & ProjectileHitFlags.NonTargetPawns) == 0
                    && !((threat.flags & ProjectileHitFlags.IntendedTarget) != 0 && threat.intended == pawn)))) continue;
                float weight = Weight(threat, cell);
                if (weight <= 0) continue;
                IntVec3 source = threat.from.ToIntVec3();
                if (threat.wallBlocked && source.InBounds(map) && !GenSight.LineOfSight(source, cell, map)) continue;
                risk += weight;
            }
            return risk;
        }
    }
}
