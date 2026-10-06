using MihoAdfqs.Combat.Effects;
using RimWorld;
using UnityEngine;
using Verse;

namespace MihoAdfqs.Phase2.Orbital
{
    //实体轨道炮弹使用原版Skyfaller飞行、保存和落地计时。
    public sealed class Skyfaller_ImperialShell : Skyfaller
    {
        public const int FlightTicks = 30;
        public string supportKind = "Bombard";
        public Pawn caller;
        public Faction faction;

        //保存弹种，让飞行中的毒气、EMP与火力网炮弹读档后正确结算。
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref supportKind, "supportKind", "Bombard");
            Scribe_References.Look(ref caller, "caller", saveDestroyedThings: true);
            Scribe_References.Look(ref faction, "callingFaction");
        }

        //长拖尾带明显的白色内芯，外层保留弹种颜色并向尾端收细淡出。
        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            if (hasImpacted) return;
            base.DrawAt(drawLoc, flip);
            Vector3 direction = (Position.ToVector3Shifted() - drawLoc).Yto0().normalized;
            if (direction.sqrMagnitude > .01f)
            {
                Vector3 tail = drawLoc - direction * 36f;
                CombatVfxMaterials.Beam(tail, drawLoc,
                    OrbitalStrike.TrailColor(supportKind), 1.6f, true, trail: true);
                CombatVfxMaterials.Beam(tail, drawLoc,
                    new Color(.98f, .98f, 1f, .88f), .55f, false, trail: true);
            }
        }

        //弹着只结算一次，根据弹种执行爆炸、点燃、毒气或EMP。
        protected override void Impact()
        {
            hasImpacted = true;
            Map map = Map;
            IntVec3 center = Position;
            //呼叫者已死亡时仍保留炮弹派系作为伤害来源。
            //Skyfaller不是可认领实体，来源派系只用于伤害归属。
            factionInt = faction;
            Thing instigator = caller != null && !caller.Destroyed && caller.Faction == faction ? (Thing)caller : this;
            if (supportKind == "Bombard") OrbitalSupport.Bombard(map, center, instigator);
            else OrbitalStrike.Impact(supportKind, map, center, instigator);
        }

        //原版爆炸逐格传播，保留来源实体到结算结束以支持期间存读档。
        protected override void Tick()
        {
            if (!hasImpacted) { base.Tick(); return; }
            foreach (Thing thing in Map.listerThings.ThingsOfDef(ThingDefOf.Explosion))
                if (((Explosion)thing).instigator == this) return;
            Destroy();
        }
    }
}
