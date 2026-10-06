using UnityEngine;
using Verse;

namespace MihoAdfqs.Phase2.UI
{
    //沿用原版指令交互，配以红底和角落铁十字。
    public class Command_ImperialAction : Command_Action
    {
        public override Texture2D BGTexture => ImperialUiStyle.CommandBackground;
        public override Texture2D BGTextureShrunk => ImperialUiStyle.CommandBackground;

        //保留功能图标，在右上角补充阵营标志。
        public override void DrawIcon(Rect rect, Material buttonMat, GizmoRenderParms parms)
        {
            base.DrawIcon(rect, buttonMat, parms);
            ImperialUiStyle.Mark(new Rect(rect.xMax - 19, rect.y + 4, 15, 15));
        }
    }
}
