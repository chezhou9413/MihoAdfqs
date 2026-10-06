using System.Collections.Generic;
using MihoAdfqs.Combat.Muzzles;
using MihoAdfqs.Combat.Weapons;
using UnityEngine;
using Verse;
using Verse.Sound;
using WeaponMuzzleFramework.Runtime;

namespace MihoAdfqs.Combat.Effects
{
    //在当前地图绘制短暂战斗效果，暂停时保持当前帧，离开地图不发起绘制。
    public sealed class MapComponent_CombatVfx : MapComponent
    {
        private readonly List<Flash> flashes = new List<Flash>();
        private readonly List<BeamLink> beams = new List<BeamLink>();
        private readonly List<Verb_ArcSprayIncinerator> flames = new List<Verb_ArcSprayIncinerator>();
        private readonly List<ShieldBurst> shieldBursts = new List<ShieldBurst>(16);

        //短暂视觉与声音不写入存档。
        public MapComponent_CombatVfx(Map map) : base(map) { }

        //破盾在原地抛出薄片、火花和冲击环，人物移动后碎片继续独立消散。
        public void ShieldBreak(Vector3 position, Color color, float size)
        {
            if (map != Find.CurrentMap || !position.InBounds(map) || position.ToIntVec3().Fogged(map)) return;
            Add(position, Color.Lerp(color, Color.white, .65f), size * .65f, 8, false, false);
            Add(position, color, size * 1.5f, 30, true, false);
            //整次破盾只保存一个记录，最多同时绘制16组、768片碎片。
            if (shieldBursts.Count == 16) shieldBursts.RemoveAt(0);
            shieldBursts.Add(new ShieldBurst { position = position, color = color, size = size,
                started = Find.TickManager.TicksGame, seed = Rand.Range(0f, Mathf.PI * 2) });
        }

        //记录命中位置，爆炸产生冲击环、向外抛出的尘土和火花。
        public void Impact(Vector3 position, CombatVisuals settings, bool explosive)
        {
            if (!position.InBounds(map) || position.ToIntVec3().Fogged(map)) return;
            if (settings.impactSound != null) settings.impactSound.PlayOneShot(new TargetInfo(position.ToIntVec3(), map));
            Add(position, settings.color, settings.impactScale, explosive ? 24 : 10, false, false);
            if (settings.energy)
            {
                //能量命中使用白热核心和短促涟漪，聚变爆炸再叠加外层冲击环。
                Add(position, new Color(1f, .96f, .9f, .85f), settings.impactScale * .45f, 6, false, false);
                Add(position, settings.color, settings.impactScale * 1.6f, 18, true, false);
            }
            if (explosive)
            {
                if (map == Find.CurrentMap) Find.CameraDriver.shaker.DoShake(Mathf.Min(.35f, settings.impactScale * .05f));
                Add(position, settings.color, settings.impactScale * 2.5f, 36, true, false);
                Add(position, settings.smokeColor, settings.impactScale * 1.7f, 130, false, true);
                float angle = Rand.Range(0f, Mathf.PI * 2);
                Color dust = settings.energy ? new Color(settings.color.r * .4f, settings.color.g * .4f, settings.color.b * .4f, .45f)
                    : new Color(.38f, .32f, .24f, .65f);
                for (int i = 0; i < 6; i++)
                {
                    Vector3 direction = new Vector3(Mathf.Cos(angle + i * Mathf.PI / 3), 0,
                        Mathf.Sin(angle + i * Mathf.PI / 3));
                    Add(position, dust, settings.impactScale * .65f,
                        55 + i * 7, false, true, direction * settings.impactScale * (1.1f + i * .08f));
                    Add(position, settings.color, settings.impactScale * .12f, 20 + i * 3,
                        false, false, direction * settings.impactScale * (1.4f + i * .1f));
                }
            }
            else if (settings.smoke) Add(position, settings.smokeColor, Mathf.Max(.6f, settings.impactScale * 1.5f),
                90, false, true);
        }

        //火焰伤害与循环声仍由原版动词处理，仅登记喷射锥的绘制。
        public void Flame(Verb_ArcSprayIncinerator verb)
        {
            if (!flames.Contains(verb)) flames.Add(verb);
        }

        //每次成功射击刷新同一根光束，连续射击期间不堆叠光束或循环声。
        public void Shot(Verb_Magazine verb, LocalTargetInfo target, CombatVisuals settings)
        {
            Vector3 start = Muzzle(verb, target.CenterVector3);
            Add(start, settings.color, settings.energy ? .5f : .65f, 4, false, false);
            if (!settings.beam) return;
            BeamLink link = beams.Find(item => item.verb == verb);
            if (link == null)
            {
                link = new BeamLink { verb = verb, settings = settings, seed = verb.Caster.thingIDNumber % 97 };
                if (settings.loopSound != null)
                    link.sound = settings.loopSound.TrySpawnSustainer(SoundInfo.InMap(verb.Caster, MaintenanceType.PerTick));
                beams.Add(link);
            }
            link.target = target;
            link.until = Find.TickManager.TicksGame + verb.NextShotTicks + 2;
        }

        //移动或改派任务时立即关闭这把武器的光束与持续声音。
        public void Stop(Verb_Magazine verb)
        {
            for (int i = beams.Count - 1; i >= 0; i--)
                if (beams[i].verb == verb)
                {
                    beams[i].sound?.End();
                    beams.RemoveAt(i);
                }
        }

        //限制同一时刻的短暂效果数量，密集弹幕不会无限积累。
        private void Add(Vector3 position, Color color, float size, int duration, bool ring, bool smoke,
            Vector3 displacement = default(Vector3))
        {
            if (map != Find.CurrentMap) return;
            if (flashes.Count >= 320) flashes.RemoveAt(0);
            flashes.Add(new Flash { position = position, color = color, size = size, duration = duration,
                started = Find.TickManager.TicksGame, seed = Rand.Range(0f, 100f), ring = ring, smoke = smoke,
                displacement = displacement });
        }

        //射手死亡、停火、换弹或目标消失时结束激光和循环声。
        public override void MapComponentTick()
        {
            int tick = Find.TickManager.TicksGame;
            for (int i = shieldBursts.Count - 1; i >= 0; i--)
                if (tick - shieldBursts[i].started >= 54) shieldBursts.RemoveAt(i);
            flashes.RemoveAll(item => tick - item.started >= item.duration);
            flames.RemoveAll(verb => !verb.Bursting || !verb.Caster.Spawned || verb.Caster.Map != map || verb.CasterPawn?.Dead == true);
            for (int i = beams.Count - 1; i >= 0; i--)
            {
                BeamLink link = beams[i];
                if (tick > link.until || !link.verb.Caster.Spawned || link.verb.Caster.Map != map ||
                    !link.verb.Bursting || link.verb.ReloadTicksLeft > 0 || link.verb.CasterPawn?.Dead == true ||
                    link.target.HasThing && !link.target.Thing.Spawned)
                {
                    link.sound?.End();
                    beams.RemoveAt(i);
                }
                else link.sound?.Maintain();
            }
        }

        //只绘制玩家当前地图中未被战争迷雾遮挡的效果。
        public override void MapComponentDraw()
        {
            if (map != Find.CurrentMap) return;
            int tick = Find.TickManager.TicksGame;
            foreach (Flash flash in flashes)
            {
                float age = Mathf.Clamp01((float)(tick - flash.started) / flash.duration);
                float growth = flash.ring ? .2f + age * 1.1f : flash.smoke ? .5f + age * 1.3f : 1 + age * .3f;
                //尘土和火花先快速飞散再减速；烟雾继续缓慢漂移。
                Vector3 position = flash.position + flash.displacement * (age * (2 - age))
                    + (flash.smoke ? new Vector3(age * .35f, 0, age * .25f) : Vector3.zero);
                if (!position.InBounds(map) || position.ToIntVec3().Fogged(map)) continue;
                CombatVfxMaterials.Burst(position, flash.size * growth, flash.color, age, flash.seed, flash.ring, flash.smoke);
            }
            CellRect view = Find.CameraDriver.CurrentViewRect;
            for (int i = 0; i < shieldBursts.Count; i++)
            {
                ShieldBurst burst = shieldBursts[i];
                if (!view.ExpandedBy(Mathf.CeilToInt(burst.size * 1.8f)).Contains(burst.position.ToIntVec3())
                    || burst.position.ToIntVec3().Fogged(map)) continue;
                ShieldVfxMaterials.Shatter(burst.position, burst.size, burst.color,
                    Mathf.Clamp01((tick - burst.started) / 54f), burst.seed);
            }
            foreach (BeamLink link in beams)
            {
                Vector3 end = link.target.CenterVector3;
                Vector3 start = Muzzle(link.verb, end);
                if (!start.InBounds(map) || !end.InBounds(map) || start.ToIntVec3().Fogged(map) || end.ToIntVec3().Fogged(map)) continue;
                CombatVfxMaterials.Beam(start, end, link.settings.color, .24f, true);
                CombatVfxMaterials.Burst(end, .55f, link.settings.color, .1f, link.seed);
            }
            foreach (Verb_ArcSprayIncinerator verb in flames)
            {
                if (!IncineratorMuzzleUtility.TryGetRay(verb, verb.InterpolatedPosition,
                    out Vector3 start, out Vector3 end)) continue;
                if (!start.InBounds(map) || !end.InBounds(map) || start.ToIntVec3().Fogged(map) || end.ToIntVec3().Fogged(map)) continue;
                float width = Mathf.Min(2.4f, Vector3.Distance(start, end) * .22f);
                CombatVfxMaterials.Beam(start, end, new Color(1,.5f,.1f,.8f), width, false, true);
            }
        }

        //复用现有枪口坐标，炮塔等无持枪姿态的来源使用其自身绘制位置。
        private static Vector3 Muzzle(Verb verb, Vector3 end)
        {
            float angle = (end - verb.Caster.DrawPos).AngleFlat();
            return MuzzleUtility.TryGetPosition(verb.Caster, verb.EquipmentSource, angle, out Vector3 position)
                ? position : verb.Caster.DrawPos;
        }

        //地图卸载时清理由本地图持有的循环声。
        public override void MapRemoved()
        {
            foreach (BeamLink link in beams) link.sound?.End();
            beams.Clear(); flashes.Clear(); flames.Clear(); shieldBursts.Clear();
        }

        //一次闪光或烟团的生命周期。
        private sealed class Flash
        {
            public Vector3 position;
            public Vector3 displacement;
            public Color color;
            public float size, seed;
            public int started, duration;
            public bool ring, smoke;
        }

        //一次破盾的固定位置与时间，碎片轨迹只由Shader读取这些参数。
        private struct ShieldBurst
        {
            public Vector3 position;
            public Color color;
            public float size, seed;
            public int started;
        }

        //一把激光武器对应一条光束和一个持续声音实例。
        private sealed class BeamLink
        {
            public Verb_Magazine verb;
            public LocalTargetInfo target;
            public CombatVisuals settings;
            public int until;
            public float seed;
            public Sustainer sound;
        }
    }
}
