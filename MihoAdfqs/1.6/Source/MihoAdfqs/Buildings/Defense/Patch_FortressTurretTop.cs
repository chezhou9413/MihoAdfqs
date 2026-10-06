using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace MihoAdfqs.Buildings.Defense
{
    //堡垒炮管的原始绘图方向与原版炮管不同。
    [HarmonyPatch(typeof(TurretTop), nameof(TurretTop.DrawTurret))]
    public static class Patch_FortressTurretTop
    {
        //保持原版瞄准和后坐力，只校正堡垒炮管的绘图角度。
        public static bool Prefix(Building_Turret ___parentTurret, TurretTop __instance,
            Vector3 drawLoc, Vector3 recoilDrawOffset, float recoilAngleOffset)
        {
            if (!(___parentTurret is Building_ImperialFortress fortress)) return true;
            Vector2 offset = fortress.def.building.turretTopOffset;
            Vector3 center = drawLoc + Altitudes.AltIncVect
                + new Vector3(offset.x, 0f, offset.y).RotatedBy(recoilAngleOffset) + recoilDrawOffset;
            float angle = fortress.TurretAimAngle;
            float size = fortress.def.building.turretTopDrawSize;
            Graphics.DrawMesh(MeshPool.plane10, Matrix4x4.TRS(center, (angle + 180f).ToQuat(),
                new Vector3(size, 1f, size)), fortress.TurretTopMaterial, 0);
            return false;
        }
    }
}
