using UnityEngine;
using Verse;
using WeaponMuzzleFramework.Runtime;

namespace MihoAdfqs.Combat.Muzzles
{
    //原版火流与叠加火焰锥共用持枪姿态、实际枪口和墙体截断后的终点。
    internal static class IncineratorMuzzleUtility
    {
        //目标可能正在横向扫射，枪口角度始终采用原版武器实际使用的姿态角。
        public static bool TryGetRay(Verb_ArcSprayIncinerator verb, Vector3 target,
            out Vector3 origin, out Vector3 end)
        {
            origin = Vector3.zero;
            end = target.Yto0();
            if (!verb.CasterIsPawn || !MuzzleUtility.TryGetPosition(verb.Caster, verb.EquipmentSource,
                MuzzlePose.AimAngle(verb.CasterPawn, verb.CurrentTarget), out origin)) return false;
            origin = origin.Yto0();
            Map map = verb.Caster.Map;
            IntVec3 targetCell = end.ToIntVec3();
            IntVec3 hit = GenSight.LastPointOnLineOfSight(verb.Caster.Position, targetCell,
                cell => cell.InBounds(map) && cell.CanBeSeenOverFast(map), skipFirstCell: true);
            if (hit.IsValid && hit != targetCell) end = hit.ToVector3Shifted().Yto0();
            //贴近墙面时截断点可能退到枪口后方，此时不绘制反向火流。
            if (Vector3.Dot(end - origin, (target - verb.Caster.DrawPos).Yto0()) <= 0) end = origin;
            return true;
        }
    }
}
