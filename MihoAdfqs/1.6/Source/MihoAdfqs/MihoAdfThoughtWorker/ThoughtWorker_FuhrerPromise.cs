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
    public class ThoughtWorker_FuhrerPromise : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            if (!p.IsColonist)
            {
                return ThoughtState.Inactive;
            }
            PawnKindDef targetKind = MihoDefRef.Miho_adfqs;
            if (p.kindDef == targetKind) 
            {
                return ThoughtState.Inactive; 
            }
            if (targetKind == null) return ThoughtState.Inactive;
            List<Pawn> playerPawns = PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_FreeColonists;

            bool fuhrerExists = false;

            for (int i = 0; i < playerPawns.Count; i++)
            {
                Pawn member = playerPawns[i];
                if (member.kindDef == targetKind && !member.Dead)
                {
                    fuhrerExists = true;
                    break; // 只要找到一个元首就行，跳出循环
                }
            }
            if (fuhrerExists)
            {
                return ThoughtState.ActiveAtStage(0);
            }
            return ThoughtState.Inactive;
        }
    }
}
