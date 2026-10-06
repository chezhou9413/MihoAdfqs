using System.Linq;
using Verse;

namespace MihoAdfqs.Combat.Projectiles
{
    //在三十秒内每秒损伤所有仍存在的身体部件。
    public class Hediff_NaniteExposure : HediffWithComps
    {
        //以一秒为周期施加无视护甲的真实伤口，不使用物品腐坏伤害。
        public override void TickInterval(int delta)
        {
            base.TickInterval(delta);
            if (!pawn.IsHashIntervalTick(60, delta)) return;
            DamageDef naniteDamage = DefDatabase<DamageDef>.GetNamed("MihoPhase2_NaniteDamage");
            foreach (BodyPartRecord part in pawn.health.hediffSet.GetNotMissingParts().ToList())
            {
                if (pawn.Dead) break;
                var damage = new DamageInfo(naniteDamage, 2, hitPart: part);
                damage.SetIgnoreArmor(true);
                damage.SetAllowDamagePropagation(false);
                pawn.TakeDamage(damage);
            }
        }
    }
}
