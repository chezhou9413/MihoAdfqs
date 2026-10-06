using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI.Group;
using Verse.Sound;
using MihoAdfqs.Phase2.CombatAI;

namespace MihoAdfqs.Phase2.Orbital
{
    //空投内容直接安全落地，不经过原版撞击清场和延迟开舱。
    public sealed class Skyfaller_ImperialCombatDrop : Skyfaller
    {
        public Faction callingFaction;
        public Lord sourceLord;

        //保存派系与原队伍，呼叫者死亡不取消已发射的空投。
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref callingFaction, "callingFaction");
            if (Scribe.mode == LoadSaveMode.Saving && sourceLord != null && !Map.lordManager.lords.Contains(sourceLord)) sourceLord = null;
            Scribe_References.Look(ref sourceLord, "sourceLord");
        }

        //先确定所有落点，再释放内容；无法落地时明确取消而不清除障碍物。
        protected override void Impact()
        {
            hasImpacted = true;
            Map.GetComponent<MapComponent_ImperialCombat>().Threats.Refresh();
            var contents = innerContainer.ToList();
            var cells = new List<IntVec3>();
            var reserved = new HashSet<IntVec3>();
            foreach (Thing thing in contents)
            {
                if (!ImperialCombatDrop.TryLanding(Map, Position, thing.def.size, out IntVec3 cell, reserved,
                    ImperialCombatDrop.DeliverySearchRadius))
                {
                    ImperialCombatDrop.ReportBlocked(Map, Position, callingFaction);
                    innerContainer.ClearAndDestroyContents();
                    Destroy();
                    return;
                }
                cells.Add(cell);
                foreach (IntVec3 occupied in GenAdj.OccupiedRect(cell, Rot4.North, thing.def.size)) reserved.Add(occupied);
            }
            var soldiers = new List<Pawn>();
            for (int i = 0; i < contents.Count; i++)
            {
                Thing thing = contents[i];
                innerContainer.Remove(thing);
                GenSpawn.Spawn(thing, cells[i], Map, Rot4.North);
                if (thing is Pawn pawn) soldiers.Add(pawn);
            }
            if (soldiers.Count > 0)
            {
                if (sourceLord != null && Map.lordManager.lords.Contains(sourceLord)
                    && sourceLord.faction == callingFaction && soldiers.All(sourceLord.CanAddPawn))
                    sourceLord.AddPawns(soldiers);
                else LordMaker.MakeNewLord(callingFaction, new LordJob_OrbitalAssist(Position), Map, soldiers);
                Messages.Message(callingFaction.Name + "的" + soldiers.Count + "名轨道援军已抵达并加入战斗。",
                    new TargetInfo(cells[0], Map), DefDatabase<MessageTypeDef>.GetNamed("MihoPhase2_SilentSupport"));
            }
            def.skyfaller.impactSound.PlayOneShot(new TargetInfo(Position, Map));
            FleckMaker.ThrowDustPuff(Position.ToVector3Shifted(), Map, 2f);
            Destroy();
        }
    }
}
