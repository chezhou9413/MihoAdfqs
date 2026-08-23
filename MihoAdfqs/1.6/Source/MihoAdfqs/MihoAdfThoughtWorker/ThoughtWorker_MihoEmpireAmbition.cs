using MihoAdfqs.MihoAdfDefRef;
using RimWorld;
using Verse;

namespace MihoAdfqs.MihoAdfThoughtWorker
{
    public class ThoughtWorker_MihoEmpireAmbition : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            if (!p.IsColonist || p.Dead)
            {
                return ThoughtState.Inactive;
            }
            if (p.kindDef != MihoDefRef.Miho_adfqs)
            {
                return ThoughtState.Inactive;
            }
            if (p.Map == null)
            {
                return ThoughtState.Inactive;
            }
            int mihoCount = 0;
            var colonists = p.Map.mapPawns.FreeColonists;
            for (int i = 0; i < colonists.Count; i++)
            {
                Pawn c = colonists[i];
                if (c.Dead) continue;
                if (c.def.defName == "Alien_Miho")
                {
                    mihoCount++;
                }
            }
            if (mihoCount > 5)
            {
                return ThoughtState.ActiveAtStage(0);
            }

            return ThoughtState.Inactive;
        }
    }
}
