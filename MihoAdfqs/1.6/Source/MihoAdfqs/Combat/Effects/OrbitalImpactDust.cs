using RimWorld;
using UnityEngine;
using Verse;

namespace MihoAdfqs.Combat.Effects
{
    //轨道炮弹落地时向外掀起灰尘和碎石，粒子由原版Fleck系统推进。
    internal static class OrbitalImpactDust
    {
        //按爆炸大小生成有限数量的尘团和抛物线碎石，不影响伤害结算。
        public static void Spawn(Map map, IntVec3 center, float radius)
        {
            if (map != Find.CurrentMap || !center.InBounds(map) || center.Fogged(map)) return;
            float strength = Mathf.Clamp(radius, 2f, 8f);
            int count = Mathf.Clamp(Mathf.RoundToInt(strength * 2.5f), 14, 24);
            Vector3 origin = center.ToVector3Shifted();
            FleckDef dustDef = DefDatabase<FleckDef>.GetNamed("MihoPhase2_OrbitalDust");
            FleckDef debrisDef = DefDatabase<FleckDef>.GetNamed("MihoPhase2_OrbitalDebris");
            float offset = Rand.Range(0f, 360f);
            for (int i = 0; i < count; i++)
            {
                float angle = offset + i * 360f / count + Rand.Range(-8f, 8f);
                Vector3 direction = Quaternion.AngleAxis(angle, Vector3.up) * Vector3.forward;
                Vector3 position = origin + direction * Rand.Range(.2f, .8f);
                if (!position.InBounds(map)) continue;
                FleckCreationData dust = FleckMaker.GetDataStatic(position, map, dustDef,
                    strength * Rand.Range(.28f, .46f));
                dust.instanceColor = new Color(.68f, .64f, .58f, .78f);
                dust.rotation = Rand.Range(0f, 360f);
                dust.rotationRate = Rand.Range(-45f, 45f);
                dust.velocityAngle = angle;
                dust.velocitySpeed = strength * Rand.Range(.55f, .9f);
                map.flecks.CreateFleck(dust);
                if (i % 2 != 0) continue;
                FleckCreationData debris = FleckMaker.GetDataStatic(position, map, debrisDef, Rand.Range(.25f, .55f));
                debris.instanceColor = new Color(.42f, .38f, .32f);
                debris.rotation = Rand.Range(0f, 360f);
                debris.rotationRate = Rand.Range(-480f, 480f);
                debris.velocityAngle = angle;
                debris.velocitySpeed = strength * Rand.Range(.8f, 1.25f);
                debris.airTimeLeft = .65f;
                map.flecks.CreateFleck(debris);
            }
        }
    }
}
