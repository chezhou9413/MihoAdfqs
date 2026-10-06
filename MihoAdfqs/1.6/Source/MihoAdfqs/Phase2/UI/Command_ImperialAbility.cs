using RimWorld;
using UnityEngine;
using Verse;

namespace MihoAdfqs.Phase2.UI
{
    //保留技能冷却和目标选择，统一模组技能按钮的外观。
    public class Command_ImperialAbility : Command_Ability
    {
        //由AbilityDef的gizmoClass实例化，传入当前技能和小人。
        public Command_ImperialAbility(Ability ability, Pawn pawn) : base(ability, pawn) { }

        public override Texture2D BGTexture => ImperialUiStyle.CommandBackground;
        public override Texture2D BGTextureShrunk => ImperialUiStyle.CommandBackground;

        //角落标志与原有技能图标同时显示。
        public override void DrawIcon(Rect rect, Material buttonMat, GizmoRenderParms parms)
        {
            base.DrawIcon(rect, buttonMat, parms);
            ImperialUiStyle.Mark(new Rect(rect.xMax - 19, rect.y + 4, 15, 15));
        }
    }
}
