using MihoAdfqs.MihoAdfDefRef;
using MihoAdfqs.MihoAdfWorldComponent;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MihoAdfqs.MihoAdfThoughtWorker
{
    public class ThoughtWorker_ExperiencedTribulation : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            // 1. 基础检查
            if (!p.IsColonist || p.Dead) return ThoughtState.Inactive;
            PawnKindDef targetKind = MihoDefRef.Miho_adfqs;
            if (p.kindDef != targetKind)
            {
                return ThoughtState.Inactive;
            }
            var tracker = Find.World?.GetComponent<MapComp_MihoRaidTracker>();
            if (tracker == null)
            {
                // 理论上 World 会自动实例化所有 WorldComponent 子类；如果这里为 null，多半是还没生成世界/还在加载阶段。
                return ThoughtState.Inactive;
            }
            int recentRaids = tracker.GetRaidsCountInLastDays(30);
            if (recentRaids >= 3)
            {
                return ThoughtState.ActiveAtStage(0);
            }
            return ThoughtState.Inactive;
        }
    }
}
