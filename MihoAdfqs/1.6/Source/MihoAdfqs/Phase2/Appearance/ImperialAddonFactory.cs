using System.Collections.Generic;
using AlienRace;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace MihoAdfqs.Phase2.Appearance
{
    //职责：以原种族的对齐参数建立独立附加层，绕开原耳朵的变体索引和姿态条件。
    [StaticConstructorOnStartup]
    public static class ImperialAddonFactory
    {
        private static readonly Dictionary<string, Graphic> Graphics = new Dictionary<string, Graphic>();
        private static readonly AccessTools.FieldRef<AlienPartGenerator.BodyAddon, string> AddonName =
            AccessTools.FieldRefAccess<AlienPartGenerator.BodyAddon, string>("name");

        //职责：在渲染树构建时创建与人物生命周期一致的独立节点，复用已加载图形。
        public static PawnRenderNode Create(Pawn pawn, AlienPartGenerator.AlienComp comp, AlienPartGenerator.BodyAddon template,
            string path, List<Rot4> facings = null)
        {
            if (!Graphics.TryGetValue(path, out Graphic graphic))
            {
                graphic = GraphicDatabase.Get<Graphic_Multi>(path, ShaderDatabase.Cutout, Vector2.one, Color.white);
                Graphics.Add(path, graphic);
            }
            var addon = new AlienPartGenerator.BodyAddon
            {
                path = path, alignWithHead = template.alignWithHead, inFrontOfBody = template.inFrontOfBody,
                layerInvert = template.layerInvert, scaleWithPawnDrawsize = template.scaleWithPawnDrawsize,
                drawSize = template.drawSize, drawSizePortrait = template.drawSizePortrait,
                offsets = template.offsets, defaultOffsets = template.defaultOffsets,
                femaleOffsets = template.femaleOffsets, angle = template.angle, useSkipFlags = template.useSkipFlags,
                userCustomizable = false, colorOverrideOne = Color.white
            };
            //保留原附加层名称，让服装的左右耳和头发隐藏规则能够识别专用人物外观。
            AddonName(addon) = template.Name;
            var props = new AlienPawnRenderNodeProperties_BodyAddon
            {
                addon = addon, addonIndex = -1, alienComp = comp, graphic = graphic,
                parentTagDef = addon.alignWithHead ? PawnRenderNodeTagDefOf.Head : PawnRenderNodeTagDefOf.Body,
                workerClass = typeof(PawnRenderNodeWorker_ImperialAddon), nodeClass = typeof(AlienPawnRenderNode_BodyAddon),
                debugLabel = path, visibleFacing = facings
            };
            var node = new AlienPawnRenderNode_BodyAddon(pawn, props, pawn.Drawer.renderer.renderTree);
            props.node = node;
            return node;
        }
    }
}
