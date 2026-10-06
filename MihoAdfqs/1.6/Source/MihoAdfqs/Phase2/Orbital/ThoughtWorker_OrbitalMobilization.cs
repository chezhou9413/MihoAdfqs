using RimWorld;
using Verse;

namespace MihoAdfqs.Phase2.Orbital
{
    //职责：启用永久动员后让元首持续获得挑战性不足的心情惩罚。
    public class ThoughtWorker_OrbitalMobilization : ThoughtWorker
    {
        //职责：仅对元首本人应用网络动员状态。
        protected override ThoughtState CurrentStateInternal(Pawn p) =>
            p.kindDef.defName == "Miho_adfqs" && GameComponent_OrbitalNetwork.Current.mobilized;
    }
}
