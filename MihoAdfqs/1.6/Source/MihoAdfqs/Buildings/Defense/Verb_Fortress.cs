using RimWorld;
using Verse;

namespace MihoAdfqs.Buildings.Defense
{
    //使堡垒射击同时受弹仓和钢铁库存约束。
    public class Verb_Fortress : Verb_Shoot
    {
        //禁止30毫米炮手动瞄准或持续攻击已经升空的人物。
        public override bool ValidateTarget(LocalTargetInfo target, bool showMessages = true) => GroundOrHeavy(target) && base.ValidateTarget(target, showMessages);

        //按当前炮型检查目标是否位于空中。
        private bool GroundOrHeavy(LocalTargetInfo target) => ((Building_ImperialFortress)caster).Heavy
            || !(target.Thing is Pawn pawn && pawn.Flying || target.Thing is RimWorld.PawnFlyer);

        //空仓或装填期间拒绝射击，原版继续校验库存与目标。
        public override bool Available() => ((Building_ImperialFortress)caster).HasRound && base.Available();

        //实际发射成功后减少弹仓，并在炮管尖端产生火光。
        protected override bool TryCastShot()
        {
            if (!GroundOrHeavy(currentTarget)) return false;
            if (!base.TryCastShot()) return false;
            var fortress = (Building_ImperialFortress)caster;
            fortress.ConsumeRound();
            FleckMaker.Static(fortress.MuzzlePosition, caster.Map, FleckDefOf.ShotFlash, 1.3f);
            return true;
        }
    }
}
