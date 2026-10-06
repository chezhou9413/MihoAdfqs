using RimWorld;
using UnityEngine;
using Verse;

namespace MihoAdfqs.Buildings.Defense
{
    //堡垒使用自定义开火类，放置预览直接读取其主武器参数。
    public class PlaceWorker_ImperialFortress : PlaceWorker
    {
        //检查蓝图时显示射程，不依赖原版对开火类的精确匹配。
        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot,
            Map map, Thing thingToIgnore = null, Thing thing = null)
        {
            VerbProperties verb = ((ThingDef)checkingDef).building.turretGunDef.Verbs[0];
            Vector3 center = loc.ToVector3Shifted();
            center.y = AltitudeLayer.MetaOverlays.AltitudeFor();
            if (verb.range > 0f) GenDraw.DrawCircleOutline(center, verb.range);
            if (verb.minRange > 0f) GenDraw.DrawCircleOutline(center, verb.minRange);
            return true;
        }

        //炮管贴图朝画布下方，预览使用与实物一致的朝向和底座轴心。
        public override void DrawGhost(ThingDef def, IntVec3 loc, Rot4 rot, Color ghostCol, Thing thing = null)
        {
            if (def.useBlueprintGraphicAsGhost) return;
            float size = def.building.turretTopDrawSize;
            Graphic graphic = GraphicDatabase.Get<Graphic_Single>(def.building.turretGunDef.graphicData.texPath,
                ShaderDatabase.Cutout, new Vector2(size, size), Color.white);
            Vector2 offset = def.building.turretTopOffset;
            Vector3 center = GenThing.TrueCenter(loc, rot, def.Size, AltitudeLayer.MetaOverlays.AltitudeFor());
            GhostUtility.GhostGraphicFor(graphic, def, ghostCol).DrawFromDef(
                center + new Vector3(offset.x, 0f, offset.y), rot, def, 180f);
        }
    }
}
