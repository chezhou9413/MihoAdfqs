using AlienRace;
using Verse;

namespace MihoAdfqs.Phase2.Appearance
{
    //职责：为独立人物附加层限制有效朝向，并随头部缺失与服装跳过标志隐藏。
    public class PawnRenderNodeWorker_ImperialAddon : AlienPawnRenderNodeWorker_BodyAddon
    {
        //职责：提供渲染工作器的公共构造入口。
        public PawnRenderNodeWorker_ImperialAddon() { }

        //职责：排除没有对应贴图的朝向，保留HAR旋转、缩放与隐身处理。
        public override bool CanDrawNow(PawnRenderNode node, PawnDrawParms parms)
        {
            if (AddonFromNode(node).alignWithHead && parms.flags.FlagSet(PawnRenderFlags.HeadStump)) return false;
            if (node.Props.visibleFacing != null && !node.Props.visibleFacing.Contains(parms.facing)) return false;
            return base.CanDrawNow(node, parms);
        }
    }
}
