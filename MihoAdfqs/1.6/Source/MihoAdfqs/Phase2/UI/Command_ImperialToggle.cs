using UnityEngine;
using Verse;

namespace MihoAdfqs.Phase2.UI
{
    //保留原版开关状态和交互，使用帝国红色指令外观。
    public class Command_ImperialToggle : Command_Toggle
    {
        public override Texture2D BGTexture => ImperialUiStyle.CommandBackground;
        public override Texture2D BGTextureShrunk => ImperialUiStyle.CommandBackground;

        //在功能图标旁绘制铁十字，开关勾选仍由原版处理。
        public override void DrawIcon(Rect rect, Material buttonMat, GizmoRenderParms parms)
        {
            base.DrawIcon(rect, buttonMat, parms);
            ImperialUiStyle.Mark(new Rect(rect.xMax - 19, rect.y + 4, 15, 15));
        }
    }
}
