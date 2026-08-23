using MihoAdfqs.MihoAdfComp;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace MihoAdfqs.MihoAdfJobGive
{
    public class JobGiver_RefuelSprayer : ThinkNode_JobGiver
    {
        protected override Job TryGiveJob(Pawn pawn)
        {
            if (pawn.Drafted) return null; // 如果需要在征召时自动装填，去掉这行，但通常征召时由玩家控制
            if (pawn.equipment?.Primary == null) return null;
            var comp = pawn.equipment.Primary.TryGetComp<ThingComp_FueledSprayer>();
            if (comp == null) return null;
            if (!comp.needsReload && comp.fuel > comp.Props.maxFuel * 0.5f) return null;
            ThingDef fuelDef = DefDatabase<ThingDef>.GetNamed(comp.Props.fuelDef);
            bool Validator(Thing t) => t.def == fuelDef && !ForbidUtility.IsForbidden(t, pawn) && pawn.CanReserveAndReach(t, PathEndMode.ClosestTouch, Danger.Deadly);
            Thing foundFuel = GenClosest.ClosestThingReachable(
                pawn.Position,
                pawn.Map,
                ThingRequest.ForDef(fuelDef),
                PathEndMode.ClosestTouch,
                TraverseParms.For(pawn),
                9999f,
                Validator
            );
            if (foundFuel != null)
            {
                JobDef jobDef = DefDatabase<JobDef>.GetNamed("Miho_RefuelSprayerJob");
                Job job = JobMaker.MakeJob(jobDef, foundFuel, pawn.equipment.Primary);
                job.count = 1; // 只需要拿一堆
                return job;
            }
            return null;
        }
    }
}
