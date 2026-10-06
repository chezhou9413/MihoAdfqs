using System.Linq;
using MihoAdfqs.Combat.Effects;
using RimWorld;
using UnityEngine;
using Verse;

namespace MihoAdfqs.Phase2.Orbital
{
    //发射实体炮弹并按当前支援参数结算圆形打击。
    internal static class OrbitalStrike
    {
        //火力网落点在散布圆内均匀随机，地图边缘只选择有效格。
        public static Skyfaller Fire(OrbitalDelivery delivery)
        {
            ImperialSupportOption option = ImperialSupportOption.Get(delivery.kind);
            IntVec3 cell = option.IsBarrage
                ? GenRadial.RadialCellsAround(delivery.cell, option.areaRadius, true)
                    .Where(c => c.InBounds(delivery.map)).RandomElement()
                : delivery.cell;
            var shell = (Skyfaller_ImperialShell)SkyfallerMaker.MakeSkyfaller(
                DefDatabase<ThingDef>.GetNamed("MihoPhase2_OrbitalShell"));
            shell.supportKind = delivery.kind;
            shell.caller = delivery.caller;
            shell.faction = delivery.faction;
            return (Skyfaller)GenSpawn.Spawn(shell, cell, delivery.map);
        }

        //用拖尾颜色区分高爆、汽油、毒气与EMP弹。
        public static Color TrailColor(string kind)
        {
            if (kind == "Gas") return new Color(.55f, .85f, .28f, .5f);
            if (kind == "EMP") return new Color(.35f, .68f, 1f, .5f);
            return new Color(1f, .58f, .24f, .55f);
        }

        //爆炸伤害不随距离衰减；汽油弹在范围内铺油并点燃可燃物与单位。
        public static void Impact(string kind, Map map, IntVec3 center, Thing instigator = null)
        {
            ImperialSupportOption option = ImperialSupportOption.Get(kind);
            bool napalm = kind == "Napalm";
            bool gas = kind == "Gas";
            bool emp = kind == "EMP";
            float radius = option.blastRadius > 0 ? option.blastRadius : option.areaRadius;
            map.GetComponent<MapComponent_CombatVfx>().Impact(center.ToVector3Shifted(),
                new CombatVisuals { color = TrailColor(kind), impactScale = emp || gas ? 3 : radius,
                    impactSound = DefDatabase<SoundDef>.GetNamed("MihoSupport_" + kind) }, true);
            if (!gas && !emp) OrbitalImpactDust.Spawn(map, center, radius);
            if (gas)
            {
                foreach (IntVec3 cell in GenRadial.RadialCellsAround(center, radius, true).Where(c => c.InBounds(map)))
                    map.gasGrid.AddGas(cell, GasType.ToxGas, 255, canOverflow: false);
                map.GetComponent<MapComponent_ImperialGasPanic>().AddCloud(center);
                return;
            }
            DamageDef damage = emp ? DamageDefOf.EMP : napalm ? DamageDefOf.Flame : DamageDefOf.Bomb;
            GenExplosion.DoExplosion(center, map, radius, damage, instigator,
                damAmount: emp ? DamageDefOf.EMP.defaultDamage : option.damage,
                chanceToStartFire: napalm ? 1f : 0f, damageFalloff: false, doSoundEffects: false);
            if (!napalm) return;
            foreach (IntVec3 cell in GenRadial.RadialCellsAround(center, radius, true).Where(c => c.InBounds(map)))
            {
                //原版燃油让裸土和石地也能承载火焰，水面仍按地形规则灭火。
                if (cell.Walkable(map) && !cell.GetTerrain(map).extinguishesFire)
                    FilthMaker.TryMakeFilth(cell, map, ThingDefOf.Filth_Fuel, shouldPropagate: false);
                FireUtility.TryStartFireIn(cell, map, 1.5f, instigator);
                foreach (Pawn pawn in cell.GetThingList(map).OfType<Pawn>().ToList())
                    if (!pawn.Dead) pawn.TryAttachFire(1.5f, instigator);
                //已有火焰同样增大，不让爆炸先生成的小火阻止强火势。
                foreach (Fire fire in cell.GetThingList(map).OfType<Fire>())
                    fire.fireSize = Mathf.Max(fire.fireSize, 1.5f);
            }
        }
    }
}
