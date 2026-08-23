using MihoAdfqs.MihoAdfDefRef;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MihoAdfqs.MihoAdfThoughtWorker
{
    public class ThoughtWorker_NotLeaderOrMoralGuide : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            if (!p.IsColonist || p.IsSlave || p.Dead)
            {
                return ThoughtState.Inactive;
            }
            PawnKindDef targetKind = MihoDefRef.Miho_adfqs;
            if (p.kindDef != targetKind)
            {
                return ThoughtState.Inactive;
            }
            if (!ModsConfig.IdeologyActive || p.Ideo == null)
            {
                return ThoughtState.Inactive;
            }
            Precept_Role currentRole = p.Ideo.GetRole(p);
            if (currentRole == null)
            {
                return ThoughtState.ActiveAtStage(0);
            }
            if (currentRole.def == PreceptDefOf.IdeoRole_Leader)
            {
                return ThoughtState.Inactive;
            }
            if (currentRole.def == PreceptDefOf.IdeoRole_Moralist)
            {
                return ThoughtState.Inactive;
            }
            return ThoughtState.ActiveAtStage(0);
        }
    }
}
