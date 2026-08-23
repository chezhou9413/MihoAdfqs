using MihoAdfqs.MihoAdfDefRef;
using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI.Group;

namespace MihoAdfqs.MihoAdfIncidents
{
    public class IncidentWorker_MihoAdfApproach : IncidentWorker
    {
        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            if (!RCellFinder.TryFindRandomPawnEntryCell(out IntVec3 spawnCell, map, CellFinder.EdgeRoadChance_Neutral))
            {
                return false; // 找不到路，事件触发失败
            }
            Faction faction = Find.FactionManager.FirstFactionOfDef(FactionDefOf.Ancients);
            if (faction == null) return false;
            PawnGenerationRequest request = new PawnGenerationRequest(
     kind: MihoDefRef.Miho_adfqs,            // 必须指定 Kind
     faction: faction,                       // 必须指定阵营
     context: PawnGenerationContext.NonPlayer,
     tile: -1,
     forceGenerateNewPawn: false,
     allowDead: false,
     allowDowned: false,
     canGeneratePawnRelations: true,
     mustBeCapableOfViolence: false,
     colonistRelationChanceFactor: 1f,
     forceAddFreeWarmLayerIfNeeded: false,
     allowGay: true,
     allowPregnant: true,
     allowFood: true
 );
            Pawn miho = PawnGenerator.GeneratePawn(request);
            GenSpawn.Spawn(miho, spawnCell, map);
            LordJob_VisitColony visitJob = new LordJob_VisitColony(faction, spawnCell, 60000 * 3);
            LordMaker.MakeNewLord(faction, visitJob, map, new List<Pawn> { miho });
            string label = "美狐来访";
            string text = "一位身着军服的美狐正在缓缓靠近你的殖民地？\n\n" +
                          "或许她并非美狐？对方的面孔洋溢着青春的神色，但谈话水平相当老练。\n\n" +
                          "她正想跟你的殖民者们谈谈加入殖民地的事情。\n" +
                          "（她将在殖民地停留 3 天，随后自行离开。你可以派人前去交谈。）";
            Find.LetterStack.ReceiveLetter(label, text, LetterDefOf.NeutralEvent, miho);
            return true;
        }
    }
}
