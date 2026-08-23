using MihoAdfqs.MihoAdfDefRef;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI.Group;

namespace MihoAdfqs.MihoAdfqsVerb
{
    public class Verb_RequestSSReinforcements : Verb_CastAbility
    {
        public override bool Targetable => true;
        public override bool MultiSelect => true;

        protected override bool TryCastShot()
        {
            Map map = this.CasterPawn.Map;
            IntVec3 targetCell = this.currentTarget.Cell;
            PawnKindDef pawnKind = MihoDefRef.Miho_SS;
            int pawnCount = 5;
            Faction faction = Find.FactionManager.FirstFactionOfDef(MihoDefRef.MihoThirdEmpire);
            if (faction == null)
            {
                faction = Find.FactionManager.RandomAlliedFaction();
            }
            if (faction == null)
            {
                Messages.Message("没有可用的盟友派系来响应呼叫！", MessageTypeDefOf.RejectInput, false);
                return false;
            }
            List<Thing> pawnsToDrop = new List<Thing>();
            List<Pawn> generatedPawns = new List<Pawn>();

            for (int i = 0; i < pawnCount; i++)
            {
                Pawn p = PawnGenerator.GeneratePawn(new PawnGenerationRequest(
                    pawnKind,
                    faction,
                    PawnGenerationContext.NonPlayer,
                    -1,
                    false,
                    false,
                    false,
                    false,
                    true,
                    1f,
                    false,
                    true,
                    true,
                    true,
                    true));
                pawnsToDrop.Add(p);
                generatedPawns.Add(p);
            }
            LordJob_AssistColony lordJob = new LordJob_AssistColony(faction, targetCell);
            LordMaker.MakeNewLord(faction, lordJob, map, generatedPawns);
            DropPodUtility.DropThingsNear(targetCell, map, pawnsToDrop, 110, false, false, true, true);
            if (this.Ability != null)
            {
                int cd = this.Ability.def.cooldownTicksRange.RandomInRange;
                if (cd > 0)
                    this.Ability.StartCooldown(cd);
            }
            return true;
        }
    }
}