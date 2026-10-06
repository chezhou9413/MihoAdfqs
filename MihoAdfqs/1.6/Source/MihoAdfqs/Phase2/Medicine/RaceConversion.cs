using System.Linq;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace MihoAdfqs.Phase2.Medicine
{
    //替换同一人物的肉体种族，保留身份、关系、记忆、技能和所属派系。
    public static class RaceConversion
    {
        //重新初始化种族组件和身体，并保留原Pawn对象供既有关系引用。
        public static void Convert(Pawn pawn, bool milira)
        {
            PawnKindDef kind = DefDatabase<PawnKindDef>.GetNamed(milira ? "MihoPhase2_ImperialMilira" : "MihoPhase2_ImperialCivilian");
            Map map = pawn.Map;
            IntVec3 position = pawn.Position;
            Rot4 rotation = pawn.Rotation;
            if (pawn.Spawned) pawn.DeSpawn();
            //基因移除回调必须仍使用原种族的身体，随后再统一重建肉体。
            foreach (Gene gene in pawn.genes.GenesListForReading.ToList()) pawn.genes.RemoveGene(gene);
            pawn.kindDef = kind;
            pawn.def = kind.race;
            //固定性别先于身体和外观初始化，米莉拉按种族模板转为女性。
            if (kind.fixedGender.HasValue) pawn.gender = kind.fixedGender.Value;
            //HAR初始化会读取故事中的肤色和发色，先清除旧种族外观再生成目标种族颜色。
            pawn.story.skinColorOverride = kind.skinColorOverride;
            pawn.story.SkinColorBase = Color.clear;
            pawn.story.HairColor = Color.clear;
            pawn.InitializeComps();
            foreach (ThingComp comp in pawn.AllComps) comp.PostPostMake();
            pawn.health.Reset();
            pawn.health.forceDowned = false;
            //寿命阶段和肉体攻击都依赖种族，不能保留旧种族的缓存及动词。
            AccessTools.Field(typeof(Pawn_AgeTracker), "cachedLifeStageIndex").SetValue(pawn.ageTracker, -1);
            pawn.verbTracker = new VerbTracker(pawn);
            pawn.meleeVerbs = new Pawn_MeleeVerbs(pawn);
            pawn.genes.SetXenotype(DefDatabase<XenotypeDef>.GetNamed(milira ? "Xeno_MihoPhase2_ImperialMilira" : "Xeno_MihoPhase2_ImperialMiho"));
            //转换不经过人物生成器，需要同步模板中的飞行等种族能力。
            if (!kind.abilities.NullOrEmpty())
                foreach (AbilityDef ability in kind.abilities) pawn.abilities.GainAbility(ability);
            pawn.needs.AddOrRemoveNeedsAsAppropriate();
            ConversionAppearance.Refresh(pawn);
            ConversionApparel.Refresh(pawn);
            pawn.Notify_DisabledWorkTypesChanged();
            pawn.Drawer.renderer.SetAllGraphicsDirty();
            if (map != null) GenSpawn.Spawn(pawn, position, map, rotation);
            pawn.needs.mood?.thoughts.memories.TryGainMemory(DefDatabase<ThoughtDef>.GetNamed(Rand.Bool ? "MihoPhase2_ConversionPositive" : "MihoPhase2_ConversionNegative"));
        }
    }
}
