using System.Collections.Generic;
using MihoAdfqs.Phase2.CombatAI;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace MihoAdfqs.Phase2.Orbital
{
    //战备空投在发射和落地时都检查占地，避免覆盖已有建筑。
    internal static class ImperialCombatDrop
    {
        //战场火力网最大危险半径超过三十格，投送时允许向周边六十格改降。
        internal const float DeliverySearchRadius = 60f;

        //炮塔仅使用帝国堡垒，增援兵永远不携带支援兵。
        public static List<Skyfaller> Deliver(OrbitalDelivery delivery)
        {
            delivery.map.GetComponent<MapComponent_ImperialCombat>().Threats.Refresh();
            if (!TryPlanLanding(delivery, DeliverySearchRadius))
            {
                ReportBlocked(delivery.map, delivery.cell, delivery.faction);
                return new List<Skyfaller>();
            }
            var contents = new List<Thing>();
            if (delivery.kind == "Soldiers")
            {
                PawnKindDef kind = DefDatabase<PawnKindDef>.GetNamed("MihoPhase2_OrbitalDropTrooper");
                for (int i = 0; i < 4; i++)
                    contents.Add(PawnGenerator.GeneratePawn(new PawnGenerationRequest(kind, delivery.faction,
                        forceGenerateNewPawn: true, mustBeCapableOfViolence: true)));
            }
            else
            {
                Thing turret = ThingMaker.MakeThing(FortressDef(delivery.kind));
                turret.SetFaction(delivery.faction);
                contents.Add(turret);
            }
            var flight = (Skyfaller_ImperialCombatDrop)SkyfallerMaker.MakeSkyfaller(
                DefDatabase<ThingDef>.GetNamed("MihoPhase2_CombatSupportIncoming"), contents);
            flight.callingFaction = delivery.faction;
            flight.sourceLord = delivery.sourceLord;
            foreach (Thing thing in contents)
                if (thing is Pawn pawn && pawn.IsWorldPawn()) Find.WorldPawns.RemovePawn(pawn);
            return new List<Skyfaller> { (Skyfaller)GenSpawn.Spawn(flight, delivery.cell, delivery.map) };
        }

        //下单与发射前都确认整组占地，记录落点供其他支援兵避开友军空投。
        internal static bool TryPlanLanding(OrbitalDelivery delivery, float searchRadius)
        {
            IntVec2 size = delivery.kind == "Soldiers" ? IntVec2.One : FortressDef(delivery.kind).size;
            int count = delivery.kind == "Soldiers" ? 4 : 1;
            var cells = new List<IntVec3>(count);
            var reserved = new HashSet<IntVec3>();
            IntVec3 center = delivery.cell;
            for (int i = 0; i < count; i++)
            {
                if (!TryLanding(delivery.map, center, size, out IntVec3 cell, reserved, searchRadius)) return false;
                cells.Add(cell);
                foreach (IntVec3 occupied in GenAdj.OccupiedRect(cell, Rot4.North, size)) reserved.Add(occupied);
                if (i == 0) center = cell;
            }
            delivery.cell = cells[0];
            delivery.landingCells = cells;
            return true;
        }

        //取消必须让玩家看到具体原因，沿用战备无提示音消息。
        internal static void ReportBlocked(Map map, IntVec3 cell, Faction faction)
        {
            string message = faction.Name + "的战备空投取消：目标周围60格内没有足够的安全空地，请避开建筑、火焰、毒气和轨道炮击区。";
            Messages.Message(message, new TargetInfo(cell, map),
                DefDatabase<MessageTypeDef>.GetNamed("MihoPhase2_SilentSupport"));
            Log.Warning("[MihoAdfqs] " + message + " 目标=" + cell);
        }

        //呼叫和实体创建共用炮型映射。
        internal static ThingDef FortressDef(string kind) => DefDatabase<ThingDef>.GetNamed(
            kind == "Fortress88" ? "MihoPhase2_CombatFortress88mm" : "MihoPhase2_CombatFortress30mm");

        //以给定落点为中心有序检查，不挤走人物或毁坏建筑。
        internal static bool TryLanding(Map map, IntVec3 center, IntVec2 size, out IntVec3 cell,
            HashSet<IntVec3> reserved = null, float searchRadius = 12f)
        {
            ImperialCombatThreats threats = map.GetComponent<MapComponent_ImperialCombat>().Threats;
            foreach (IntVec3 candidate in GenRadial.RadialCellsAround(center, searchRadius, true))
            {
                bool valid = true;
                foreach (IntVec3 occupied in GenAdj.OccupiedRect(candidate, Rot4.North, size))
                {
                    if (!occupied.InBounds(map) || !occupied.Standable(map) || occupied.Roofed(map)
                        || occupied.GetFirstPawn(map) != null || occupied.GetEdifice(map) != null
                        || (reserved != null && reserved.Contains(occupied))
                        || threats.OrbitalRisk(occupied) > 0 || threats.UnsafeTerrain(occupied))
                    { valid = false; break; }
                }
                if (!valid) continue;
                cell = candidate;
                return true;
            }
            cell = IntVec3.Invalid;
            return false;
        }
    }
}
