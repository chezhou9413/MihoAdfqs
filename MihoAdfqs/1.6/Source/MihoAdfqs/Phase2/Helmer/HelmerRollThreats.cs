using System.Collections.Generic;
using HarmonyLib;
using MihoAdfqs.Combat.Projectiles;
using MihoAdfqs.Phase2.Orbital;
using MihoAdfqs.Phase2.CombatAI;
using RimWorld;
using UnityEngine;
using Verse;

namespace MihoAdfqs.Phase2.Helmer
{
    //收集一次附近的弹道、爆炸和近战威胁，供各个候选落点评分。
    internal sealed class HelmerRollThreats
    {
        private static readonly AccessTools.FieldRef<Projectile, Vector3> Destination =
            AccessTools.FieldRefAccess<Projectile, Vector3>("destination");
        private static readonly AccessTools.FieldRef<Projectile, int> ImpactTicks =
            AccessTools.FieldRefAccess<Projectile, int>("ticksToImpact");
        private static readonly AccessTools.FieldRef<Projectile, bool> Landed =
            AccessTools.FieldRefAccess<Projectile, bool>("landed");
        private static readonly AccessTools.FieldRef<Projectile_Explosive, int> DetonationTicks =
            AccessTools.FieldRefAccess<Projectile_Explosive, int>("ticksToDetonation");

        private readonly Pawn pawn;
        private readonly List<Threat> threats = new List<Threat>();

        //威胁使用平面线段或圆形、方形落区，不依赖显示用的弹道高度。
        private struct Threat
        {
            public Vector3 from, to;
            public float radius, weight;
            public bool blast, blockedByWalls;
            public CellRect? rectangle;
        }

        //只在翻滚可用时采样，射弹候选来自地图共用的空间索引。
        public HelmerRollThreats(Pawn pawn)
        {
            this.pawn = pawn;
            var projectiles = new HashSet<Projectile>();
            pawn.Map.GetComponent<MapComponent_ImperialCombat>().Threats.NearbyProjectiles(pawn.Position, projectiles);
            foreach (Projectile projectile in projectiles) AddProjectile(projectile);
            foreach (Thing thing in pawn.Map.listerThings.ThingsOfDef(DefDatabase<ThingDef>.GetNamed("MihoPhase2_OrbitalShell")))
            {
                var shell = (Skyfaller_ImperialShell)thing;
                if (shell.ticksToImpact > 45) continue;
                ImperialSupportOption option = ImperialSupportOption.Get(shell.supportKind);
                Vector3 center = shell.Position.ToVector3Shifted();
                if (shell.supportKind == "Bombard")
                    AddBlast(center, 0f, OrbitalSupport.StrikeArea(pawn.Map, shell.Position), false);
                else
                    AddBlast(center, option.blastRadius > 0 ? option.blastRadius : option.areaRadius, null, shell.supportKind != "Gas");
            }
            foreach (Pawn enemy in pawn.Map.mapPawns.AllPawnsSpawned)
            {
                if (enemy == pawn || enemy.Dead || enemy.Downed || !enemy.HostileTo(pawn)
                    || enemy.Position.DistanceToSquared(pawn.Position) > 144 || enemy.IsPsychologicallyInvisible()
                    || !GenSight.LineOfSight(pawn.Position, enemy.Position, pawn.Map)) continue;
                Verb weapon = enemy.equipment?.PrimaryEq?.PrimaryVerb;
                if (weapon != null && !weapon.verbProps.IsMeleeAttack && enemy.CurJob?.def != JobDefOf.AttackMelee) continue;
                Vector3 position = enemy.Position.ToVector3Shifted();
                threats.Add(new Threat { from = position, to = position, radius = 5f, weight = 18f, blockedByWalls = true });
            }
        }

        //预测未来半秒多的飞行路段，并识别落地等待引爆的手雷。
        private void AddProjectile(Projectile projectile)
        {
            bool landed = Landed(projectile);
            int ticks = landed && projectile is Projectile_Explosive explosive ? DetonationTicks(explosive) : ImpactTicks(projectile);
            Vector3 from = (landed ? projectile.Position.ToVector3Shifted() : projectile.ExactPosition).Yto0();
            Vector3 destination = Destination(projectile).Yto0();
            if (landed) destination = from;
            if (ticks <= 45)
            {
                AreaPayload payload = projectile.def.GetModExtension<AreaPayload>();
                if (payload != null)
                {
                    int width = payload.width, height = payload.height > 0 ? payload.height : width;
                    if (payload.orientToFlight && Mathf.Abs(destination.x - from.x) > Mathf.Abs(destination.z - from.z))
                    { int previous = width; width = height; height = previous; }
                    IntVec3 cell = destination.ToIntVec3();
                    AddBlast(destination, 0f, new CellRect(cell.x - width / 2, cell.z - height / 2, width, height), false);
                }
                else if (projectile.def.projectile.explosionRadius > 0f)
                    AddBlast(destination, projectile.def.projectile.explosionRadius, null, true);
            }
            if (landed || projectile.Launcher == pawn || projectile.def.projectile.flyOverhead) return;
            ProjectileHitFlags flags = projectile.HitFlags;
            if ((flags & ProjectileHitFlags.NonTargetPawns) == 0
                && !((flags & ProjectileHitFlags.IntendedTarget) != 0 && projectile.intendedTarget.Thing == pawn)) return;
            Vector3 to = Vector3.Lerp(from, destination, ticks > 0 ? Mathf.Min(1f, 36f / ticks) : 1f);
            if (DistanceToSegment(pawn.Position.ToVector3Shifted(), from, to) > 8f) return;
            threats.Add(new Threat { from = from, to = to, radius = 1.25f, weight = 24f, blockedByWalls = true });
        }

        //仅保留翻滚可达范围附近的爆炸，不为远处战斗构造评分项。
        private void AddBlast(Vector3 center, float radius, CellRect? rectangle, bool blockedByWalls)
        {
            float reach = rectangle.HasValue ? Mathf.Max(rectangle.Value.Width, rectangle.Value.Height) : radius;
            if ((center - pawn.Position.ToVector3Shifted()).sqrMagnitude > (reach + 7f) * (reach + 7f)) return;
            threats.Add(new Threat { from = center, to = center, radius = radius + .75f, weight = 40f,
                blast = true, blockedByWalls = blockedByWalls, rectangle = rectangle });
        }

        //风险累计考虑交叉火力；火焰格始终禁止作为自动翻滚落点。
        public float Risk(IntVec3 cell)
        {
            if (HasFire(cell)) return 1000f;
            float risk = pawn.Map.gasGrid.DensityAt(cell, GasType.ToxGas) / 255f * 20f;
            Vector3 position = cell.ToVector3Shifted();
            foreach (Threat threat in threats)
            {
                float distance = threat.blast ? (position - threat.to).magnitude : DistanceToSegment(position, threat.from, threat.to);
                if (threat.rectangle.HasValue)
                {
                    if (!threat.rectangle.Value.Contains(cell)) continue;
                }
                else if (distance >= threat.radius) continue;
                IntVec3 source = threat.from.ToIntVec3();
                if (threat.blockedByWalls && source.InBounds(pawn.Map) && !GenSight.LineOfSight(source, cell, pawn.Map)) continue;
                risk += threat.rectangle.HasValue ? threat.weight : threat.weight * (1f - distance / threat.radius);
            }
            return risk;
        }

        //读取落点格上的真实火焰，不扫描整张地图的火焰列表。
        public bool HasFire(IntVec3 cell)
        {
            foreach (Thing thing in cell.GetThingList(pawn.Map))
                if (thing is Fire) return true;
            return false;
        }

        //计算平面线段距离，排除已经飞过或朝其它方向飞行的子弹。
        private static float DistanceToSegment(Vector3 point, Vector3 from, Vector3 to)
        {
            Vector3 direction = to - from;
            float fraction = direction.sqrMagnitude > .001f ? Mathf.Clamp01(Vector3.Dot(point - from, direction) / direction.sqrMagnitude) : 0f;
            return (point - (from + direction * fraction)).magnitude;
        }
    }
}
