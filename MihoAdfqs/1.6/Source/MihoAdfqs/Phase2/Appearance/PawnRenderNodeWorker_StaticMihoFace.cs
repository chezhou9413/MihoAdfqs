using AlienRace;
using Verse;

namespace MihoAdfqs.Phase2.Appearance
{
    //职责：在动态表情启用时隐藏原始静态脸，关闭表情时恢复原始显示。
    public class PawnRenderNodeWorker_StaticMihoFace : AlienPawnRenderNodeWorker_BodyAddon
    {
        //职责：提供渲染工作器的公共构造入口。
        public PawnRenderNodeWorker_StaticMihoFace() { }

        //职责：只对该静态脸节点应用动态表情显示条件。
        public override bool CanDrawNow(PawnRenderNode node, PawnDrawParms parms) => !AppearanceHooks.UseAnimatedFace(parms.pawn) && base.CanDrawNow(node, parms);
    }
}
