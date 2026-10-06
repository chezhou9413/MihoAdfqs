using System.Collections.Generic;
using System.Linq;
using MihoAdfqs.Phase2.Orbital;
using RimWorld;
using UnityEngine;
using Verse;

namespace MihoAdfqs.Phase2.CombatAI
{
    //根据局部敌情选择战备，成功登记后再消耗人物额度。
    internal static class ImperialSupportAI
    {
        private static readonly string[] StrikeKinds = { "Bombard", "Barrage120", "Barrage380", "Napalm", "Gas", "EMP" };
        private static readonly string[] FortressKinds = { "Fortress30", "Fortress88" };

        //每个候选同时记录战备类别、落点和战术权重。
        private struct Choice
        {
            public string kind;
            public IntVec3 cell;
            public float weight;
        }

        //同一落点的敌人数和单位类型供六种战备共用。
        private struct TargetGroup
        {
            public Thing target;
            public int count;
            public bool machine, human, hasMachines, hasHumans;
        }

        //只对第三帝国能行动的NPC生效，玩家余额与人物战备额度独立。
        internal static void Update(Pawn pawn, ImperialCombatPawnState state, MapComponent_ImperialCombat component)
        {
            int tick = Find.TickManager.TicksGame;
            if (!ImperialEvasion.CanAct(pawn) || pawn.Faction.def.defName != "MihoThirdEmpire"
                || pawn.Faction.IsPlayer || state.charges <= 0 || tick < state.nextCallTick
                || !component.FactionReady(pawn.Faction, tick)) return;
            Map map = pawn.Map;
            var enemies = new List<Thing>();
            foreach (var candidate in map.attackTargetsCache.GetPotentialTargetsFor(pawn))
            {
                Thing target = candidate.Thing;
                if (!target.Spawned || target.Position.DistanceToSquared(pawn.Position) > 3600
                    || !(target is Pawn || target is Building_TurretGun)) continue;
                if (target is Pawn enemy && (enemy.Dead || enemy.Downed || enemy.IsPsychologicallyInvisible())) continue;
                if (!target.HostileTo(pawn) || target.Position.Fogged(map) || candidate.ThreatDisabled(pawn)
                    || !GenSight.LineOfSight(pawn.Position, target.Position, map)) continue;
                enemies.Add(target);
            }
            if (enemies.Count == 0) return;
            //翻滚期间的人物仍计入安全检查，避免暂时离开人物列表导致误判。
            var friends = map.mapPawns.AllPawnsSpawned.Where(p => !p.Dead && !p.HostileTo(pawn)).Cast<Thing>().ToList();
            friends.AddRange(map.listerThings.ThingsInGroup(ThingRequestGroup.ThingHolder).OfType<PawnFlyer>()
                .Where(f => f.FlyingPawn != null && !f.FlyingPawn.HostileTo(pawn)).Cast<Thing>());
            friends.AddRange(map.listerThings.ThingsInGroup(ThingRequestGroup.BuildingArtificial)
                .Where(t => t is Building_TurretGun && !t.HostileTo(pawn)));
            var choices = new List<Choice>();
            foreach (TargetGroup group in GroupTargets(enemies).OrderByDescending(g => g.count).Take(8))
            {
                Thing target = group.target;
                foreach (string kind in StrikeKinds)
                {
                    ImperialSupportOption option = ImperialSupportOption.Get(kind);
                    int clustered = group.count;
                    bool machines = group.hasMachines;
                    if (kind == "EMP" && !machines) continue;
                    if ((kind == "Gas" || kind == "Napalm") && !group.hasHumans) continue;
                    if (option.IsBarrage && clustered < 3) continue;
                    if (!SafeStrike(map, target.Position, kind, friends, pawn.Faction)) continue;
                    choices.Add(new Choice { kind = kind, cell = target.Position,
                        weight = kind == "EMP" && machines ? 3f : option.IsBarrage ? 2f : 1f });
                }
            }
            int allies = map.mapPawns.AllPawnsSpawned.Count(p => !p.Dead && !p.Downed && p.Faction == pawn.Faction
                && MilitaryNear(p, pawn.Position));
            Thing nearest = enemies[0];
            int nearestDistance = nearest.Position.DistanceToSquared(pawn.Position);
            for (int i = 1; i < enemies.Count; i++)
            {
                int distance = enemies[i].Position.DistanceToSquared(pawn.Position);
                if (distance >= nearestDistance) continue;
                nearest = enemies[i]; nearestDistance = distance;
            }
            Vector3 away = (pawn.Position - nearest.Position).ToVector3().normalized;
            IntVec3 rear = (pawn.Position.ToVector3Shifted() + away * 10f).ToIntVec3();
            if (allies <= enemies.Count && ImperialCombatDrop.TryLanding(map, rear, IntVec2.One, out IntVec3 soldierCell)
                && component.Threats.OrbitalRisk(soldierCell) == 0)
                choices.Add(new Choice { kind = "Soldiers", cell = soldierCell, weight = 3f });
            bool fortressNearby = map.listerThings.ThingsInGroup(ThingRequestGroup.BuildingArtificial)
                .Any(t => t is Building_TurretGun && t.Faction == pawn.Faction && t.Position.DistanceToSquared(pawn.Position) < 576);
            if (!fortressNearby)
            {
                foreach (string kind in FortressKinds)
                    if (ImperialCombatDrop.TryLanding(map, rear, ImperialCombatDrop.FortressDef(kind).size, out IntVec3 cell)
                        && component.Threats.OrbitalRisk(cell) == 0
                        && GenSight.LineOfSight(cell, nearest.Position, map, GenAdj.OccupiedRect(cell, Rot4.North, new IntVec2(4, 4)),
                            nearest.OccupiedRect()))
                        choices.Add(new Choice { kind = kind, cell = cell, weight = enemies.Count >= 3 ? 3f : 1f });
            }
            if (choices.Count == 0) return;
            Choice chosen = choices.RandomElementByWeight(c => c.weight);
            if (!GameComponent_OrbitalNetwork.Current.QueueCombatSupport(pawn, chosen.kind, chosen.cell)) return;
            state.charges--;
            state.nextCallTick = tick + 1800;
            component.Called(pawn.Faction, tick);
        }

        //每对敌人的距离只计算一次，同时累计双方邻域，保持原来的十格统计范围。
        private static TargetGroup[] GroupTargets(List<Thing> enemies)
        {
            var groups = new TargetGroup[enemies.Count];
            for (int i = 0; i < enemies.Count; i++)
            {
                Pawn pawn = enemies[i] as Pawn;
                bool machine = pawn != null && pawn.RaceProps.IsMechanoid;
                bool human = pawn != null && pawn.RaceProps.Humanlike;
                groups[i] = new TargetGroup { target = enemies[i], count = 1, machine = machine, human = human,
                    hasMachines = machine, hasHumans = human };
            }
            for (int i = 0; i < groups.Length; i++)
                for (int j = i + 1; j < groups.Length; j++)
                {
                    if (enemies[i].Position.DistanceToSquared(enemies[j].Position) > 100) continue;
                    groups[i].count++; groups[j].count++;
                    groups[i].hasMachines |= groups[j].machine; groups[j].hasMachines |= groups[i].machine;
                    groups[i].hasHumans |= groups[j].human; groups[j].hasHumans |= groups[i].human;
                }
            return groups;
        }

        //比较局部兵力时不把平民或远处援军算入当前交战队伍。
        private static bool MilitaryNear(Pawn pawn, IntVec3 center) => pawn.kindDef.isFighter
            && pawn.Position.DistanceToSquared(center) <= 3600;

        //整个散布区连同爆炸半径都要避开非敌对单位和已登记的友军空投占地。
        private static bool SafeStrike(Map map, IntVec3 cell, string kind, List<Thing> friends, Faction faction)
        {
            ImperialSupportOption option = ImperialSupportOption.Get(kind);
            float radius = (option.IsBarrage ? option.areaRadius + option.blastRadius
                : Mathf.Max(option.areaRadius, option.blastRadius)) + 2f;
            CellRect rectangle = OrbitalSupport.StrikeArea(map, cell).ExpandedBy(2);
            foreach (Thing friend in friends)
            {
                foreach (IntVec3 occupied in friend.OccupiedRect())
                    if (kind == "Bombard" ? rectangle.Contains(occupied) : occupied.DistanceToSquared(cell) <= radius * radius)
                        return false;
                if (friend is PawnFlyer flyer)
                {
                    IntVec3 destination = flyer.DestinationPos.ToIntVec3();
                    if (kind == "Bombard" ? rectangle.Contains(destination) : destination.DistanceToSquared(cell) <= radius * radius)
                        return false;
                }
            }
            //空中和等待发射的援军尚未出现在地图人物列表，但落点同样需要保护。
            foreach (OrbitalDelivery delivery in GameComponent_OrbitalNetwork.Current.Deliveries)
            {
                if (delivery.map != map || delivery.landingCells == null || delivery.faction.HostileTo(faction)) continue;
                IntVec2 size = delivery.kind == "Soldiers" ? IntVec2.One : ImperialCombatDrop.FortressDef(delivery.kind).size;
                foreach (IntVec3 landing in delivery.landingCells)
                    foreach (IntVec3 occupied in GenAdj.OccupiedRect(landing, Rot4.North, size))
                        if (kind == "Bombard" ? rectangle.Contains(occupied) : occupied.DistanceToSquared(cell) <= radius * radius)
                            return false;
            }
            return true;
        }
    }
}
