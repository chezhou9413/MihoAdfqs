using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace MihoAdfqs.Combat.Projectiles
{
    //在精确的方形区域内结算弹头伤害，避免以半径代替文档的长宽。
    public class Projectile_AreaPayload : Projectile
    {
        //命中时为每个实体结算一次伤害或毒剂状态，拦截后不释放载荷。
        protected override void Impact(Thing hitThing, bool blockedByShield = false)
        {
            if (!blockedByShield)
            {
                AreaPayload payload = def.GetModExtension<AreaPayload>();
                var visual = def.GetModExtension<MihoAdfqs.Combat.Effects.CombatVisuals>();
                if (visual != null)
                    Map.GetComponent<MihoAdfqs.Combat.Effects.MapComponent_CombatVfx>().Impact(ExactPosition, visual,
                        !payload.orientToFlight && payload.hediff == null);
                int width = payload.width;
                int height = payload.height > 0 ? payload.height : width;
                //横向霰弹扇面垂直于飞行主方向，在落点覆盖三格宽的一排。
                if (payload.orientToFlight && Mathf.Abs(Position.x - origin.x) > Mathf.Abs(Position.z - origin.z))
                {
                    int previous = width;
                    width = height;
                    height = previous;
                }
                var rect = new CellRect(Position.x - width / 2, Position.z - height / 2, width, height);
                var center = CellRect.CenteredOn(Position, payload.centerWidth / 2);
                var targets = new HashSet<Thing>(rect.ClipInsideMap(Map).Cells.SelectMany(c => c.GetThingList(Map)));
                if (!payload.orientToFlight && payload.hediff == null && (visual == null || !visual.energy))
                    FleckMaker.Static(Position, Map, FleckDefOf.ExplosionFlash, payload.width);
                foreach (Thing thing in targets.Where(t => t != this && !t.Destroyed).ToList())
                {
                    if (payload.hediff != null)
                    {
                        if (thing is Pawn pawn && !pawn.Dead) pawn.health.AddHediff(payload.hediff);
                        continue;
                    }
                    float damage = payload.centerWidth > 0 && center.Contains(thing.Position) ? payload.centerDamage : DamageAmount;
                    if (payload.wallDamage > 0 && thing.def.IsWall) damage = payload.wallDamage;
                    thing.TakeDamage(new DamageInfo(DamageDef, damage, ArmorPenetration, instigator: launcher, weapon: equipmentDef));
                }
            }
            Destroy();
        }
    }
}
