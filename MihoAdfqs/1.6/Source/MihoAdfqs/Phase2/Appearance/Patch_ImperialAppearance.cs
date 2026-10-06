using System.Collections.Generic;
using System.Linq;
using AlienRace;
using HarmonyLib;
using Verse;

namespace MihoAdfqs.Phase2.Appearance
{
    //为特殊人物保留姿态和可见方向，贴图由角色规则组提供。
    [HarmonyPatch(typeof(AlienPartGenerator.AlienComp), nameof(AlienPartGenerator.AlienComp.CompRenderNodes))]
    public static class Patch_ImperialAppearance
    {
        //职责：在HAR节点建成后只调整目标人物的节点列表，不修改共享种族定义。
        //先替换专用外观，再由ChezhouLib根据当前装备移除需要隐藏的附加层。
        [HarmonyBefore("ChezhouLib.lib.har")]
        public static void Postfix(AlienPartGenerator.AlienComp __instance, ref List<PawnRenderNode> __result)
        {
            Pawn pawn = (Pawn)__instance.parent;
            if (__result == null || (pawn.def.defName != "Alien_Miho" && pawn.kindDef.defName != "MihoPhase2_Tiana")) return;
            foreach (AlienPawnRenderNode_BodyAddon node in __result.OfType<AlienPawnRenderNode_BodyAddon>())
                if (node.props.addon.Name == "MihoHead") node.Props.workerClass = typeof(PawnRenderNodeWorker_StaticMihoFace);
            bool fuhrer = pawn.kindDef.defName == "Miho_adfqs";
            bool captain = pawn.kindDef.defName == "MihoPhase2_LeaderGuardCaptain";
            bool tiana = pawn.kindDef.defName == "MihoPhase2_Tiana";
            if (!fuhrer && !captain && !tiana) return;
            var templates = ((ThingDef_AlienRace)pawn.def).alienRace.generalSettings.alienPartGenerator.bodyAddons;
            if (fuhrer || captain)
            {
                string root = captain ? "AlienRace/adfqs/MihoPhase2/HelmerZog/" : "AlienRace/adfqs/";
                Replace(pawn, __instance, __result, templates, "MihoEarRight", root + (captain ? "Ear/MihoPhase2_HelmerZogEar" : "Ear/ear"));
                Replace(pawn, __instance, __result, templates, "MihoEarLeft", root + (captain ? "EarBack/MihoPhase2_HelmerZogEarBack" : "Earback/earback"), captain ? new List<Rot4> { Rot4.East, Rot4.West } : null);
                if (captain)
                {
                    Replace(pawn, __instance, __result, templates, "MihoHairFront", root + "Hair/MihoPhase2_HelmerZogHair");
                    Remove(__result, "MihoHairBack");
                    Replace(pawn, __instance, __result, templates, "MihoTail", root + "Tail/MihoPhase2_HelmerZogTail");
                    Remove(__result, "MihoTailMoving");
                }
            }
            if (tiana)
            {
                foreach (AlienPawnRenderNode_BodyAddon node in __result.OfType<AlienPawnRenderNode_BodyAddon>())
                {
                    if (node.props.addon.Name != "milira hair BG") continue;
                    node.Props.workerClass = typeof(PawnRenderNodeWorker_ImperialAddon);
                    node.Props.visibleFacing = new List<Rot4> { Rot4.South };
                }
                Remove(__result, "milira ahoge");
            }
        }

        //职责：删除目标附加层的所有站立及躺卧节点，只加入一个完整姿态节点。
        private static void Replace(Pawn pawn, AlienPartGenerator.AlienComp comp, List<PawnRenderNode> nodes,
            List<AlienPartGenerator.BodyAddon> templates, string name, string path, List<Rot4> facings = null)
        {
            AlienPartGenerator.BodyAddon template = templates.First(addon => addon.Name == name);
            Remove(nodes, name);
            nodes.Add(ImperialAddonFactory.Create(pawn, comp, template, path, facings));
        }

        //职责：从当前人物的节点集合移除指定名称的所有原始附加层。
        private static void Remove(List<PawnRenderNode> nodes, string name) => nodes.RemoveAll(node => node is AlienPawnRenderNode_BodyAddon addon && addon.props.addon.Name == name);
    }
}
