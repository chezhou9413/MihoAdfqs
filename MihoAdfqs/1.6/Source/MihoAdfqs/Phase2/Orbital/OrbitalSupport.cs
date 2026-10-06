using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI.Group;

namespace MihoAdfqs.Phase2.Orbital
{
    //职责：执行轨道货物、驻留援军和精确方形舰炮伤害。
    public static class OrbitalSupport
    {
        public static Faction Empire => Find.FactionManager.FirstFactionOfDef(DefDatabase<FactionDef>.GetNamed("MihoThirdEmpire"));

        //按已付款的交付类别发放物品、士兵或单发打击。
        public static List<Skyfaller> Deliver(OrbitalDelivery delivery)
        {
            if (ImperialSupportOption.Get(delivery.kind).IsStrike)
                return new List<Skyfaller> { OrbitalStrike.Fire(delivery) };
            if (delivery.npcCall) return ImperialCombatDrop.Deliver(delivery);
            IEnumerable<Thing> contents;
            switch (delivery.kind)
            {
                case "Blueprint":
                    contents = new[] { ThingMaker.MakeThing(DefDatabase<ResearchProjectDef>.GetNamed("MihoPhase2_Research_Nova").Techprint) };
                    break;
                case "Medicine":
                    contents = new[] { Stack("MedicineIndustrial", 50), Stack("MedicineUltratech", 10) };
                    break;
                case "Soldiers": contents = GenerateSoldiers(delivery.map, delivery.soldierCount, delivery.cell, delivery.faction); break;
                case "Guards": contents = MihoAdfqsVerb.Verb_RequestSSReinforcements.GenerateGuards(delivery.map, delivery.cell, delivery.includeCaptain); break;
                default: throw new System.InvalidOperationException("未知轨道交付：" + delivery.kind);
            }
            return DropSupport(delivery.map, delivery.cell, Pack(contents));
        }

        //使用实际空投舱记录到达时间，落点在玩家指定位置附近选择。
        private static List<Skyfaller> DropSupport(Map map, IntVec3 center, IEnumerable<Thing> contents)
        {
            var flights = new List<Skyfaller>();
            foreach (Thing item in contents)
            {
                if (!DropCellFinder.TryFindDropSpotNear(center, map, out IntVec3 cell, false, true))
                    throw new System.InvalidOperationException("帝国支援无法在指定区域找到有效空降格：" + center);
                var info = new ActiveTransporterInfo { openDelay = 110, leaveSlag = false };
                info.innerContainer.TryAdd(item);
                var pod = (ActiveTransporter)ThingMaker.MakeThing(ThingDefOf.ActiveDropPod);
                pod.Contents = info;
                flights.Add(SkyfallerMaker.SpawnSkyfaller(DefDatabase<ThingDef>.GetNamed("MihoPhase2_SupportPodIncoming"), pod, cell, map));
                if (item is Pawn pawn && pawn.IsWorldPawn()) Find.WorldPawns.RemovePawn(pawn);
            }
            return flights;
        }

        //职责：创建指定数量的物资堆叠。
        public static Thing Stack(string defName, int count)
        {
            Thing result = ThingMaker.MakeThing(DefDatabase<ThingDef>.GetNamed(defName));
            result.stackCount = count;
            return result;
        }

        //职责：在殖民地贸易空投位置附近投放奖励物资。
        public static void DropGoods(Map map, IEnumerable<Thing> things)
        {
            DropPodUtility.DropThingsNear(DropCellFinder.TradeDropSpot(map), map, Pack(things), 110, false, false, true, false);
        }

        //职责：按定义的堆叠上限分装奖励，避免大额物资以超限单堆落地。
        public static List<Thing> Pack(IEnumerable<Thing> things)
        {
            var result = new List<Thing>();
            foreach (Thing thing in things)
            {
                while (thing.stackCount > thing.def.stackLimit) result.Add(thing.SplitOff(thing.def.stackLimit));
                result.Add(thing);
            }
            return result;
        }

        //生成制式轨道步兵并指定两天的援助任务。
        private static List<Thing> GenerateSoldiers(Map map, int count, IntVec3 center, Faction faction)
        {
            PawnKindDef kind = DefDatabase<PawnKindDef>.GetNamed("MihoPhase2_OrbitalDropTrooper");
            var soldiers = new List<Pawn>();
            for (int i = 0; i < count; i++)
                soldiers.Add(PawnGenerator.GeneratePawn(new PawnGenerationRequest(i == 0
                    ? DefDatabase<PawnKindDef>.GetNamed("MihoPhase2_StratagemSupportSoldier") : kind, faction, PawnGenerationContext.NonPlayer,
                    forceGenerateNewPawn: true)));
            LordMaker.MakeNewLord(faction, new LordJob_OrbitalAssist(center), map, soldiers);
            return soldiers.Cast<Thing>().ToList();
        }

        //职责：返回以选定格为中心锚点的六乘六打击区域。
        public static CellRect StrikeArea(Map map, IntVec3 center) =>
            new CellRect(center.x - 3, center.z - 3, 6, 6).ClipInsideMap(map);

        //落地掀起烟尘，每个实体仅受一次伤害，避免大型建筑按占地重复受伤。
        public static void Bombard(Map map, IntVec3 center, Thing instigator = null)
        {
            var targets = new HashSet<Thing>(StrikeArea(map, center).Cells.SelectMany(c => c.GetThingList(map)));
            FleckMaker.Static(center, map, FleckDefOf.ExplosionFlash, 6f);
            map.GetComponent<Combat.Effects.MapComponent_CombatVfx>().Impact(center.ToVector3Shifted(),
                new Combat.Effects.CombatVisuals { color = new UnityEngine.Color(1,.42f,.08f), impactScale = 4,
                    impactSound = DefDatabase<SoundDef>.GetNamed("MihoCombat_OrbitalBlast") }, true);
            Combat.Effects.OrbitalImpactDust.Spawn(map, center, 4f);
            foreach (Thing target in targets.Where(t => !t.Destroyed).ToList())
                target.TakeDamage(new DamageInfo(DamageDefOf.Bomb, 5000, 100, instigator: instigator));
        }
    }
}
