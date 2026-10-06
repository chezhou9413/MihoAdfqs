using System.Linq;
using AlienRace;
using RimWorld;
using Verse;

namespace MihoAdfqs.Phase2.Medicine
{
    //在保留人物经历和身份的同时重建目标种族允许的外观。
    public static class ConversionAppearance
    {
        //按目标种族的HAR集合重建头型、颜色和样式，清除旧种族的待换造型。
        public static void Refresh(Pawn pawn)
        {
            var race = (ThingDef_AlienRace)pawn.def;
            AlienPartGenerator parts = race.alienRace.generalSettings.alienPartGenerator;
            AlienPartGenerator.AlienComp comp = pawn.GetComp<AlienPartGenerator.AlienComp>();
            pawn.story.SkinColorBase = comp.GetChannel("skin").first;
            pawn.story.HairColor = comp.GetChannel("hair").first;
            pawn.story.bodyType = AlienRace.HarmonyPatches.CheckBodyType(pawn, PawnGenerator.GetBodyTypeFor(pawn));
            if (!pawn.story.TryGetRandomHeadFromSet(parts.HeadTypes.Where(h => h.gender == Gender.None || h.gender == pawn.gender)))
                throw new System.InvalidOperationException("目标种族没有适用于转换人物的头型：" + pawn.def.defName);
            pawn.story.hairDef = PawnStyleItemChooser.RandomHairFor(pawn);
            pawn.style.beardDef = PawnStyleItemChooser.RandomBeardFor(pawn);
            if (ModsConfig.IdeologyActive)
            {
                pawn.style.FaceTattoo = PawnStyleItemChooser.RandomTattooFor(pawn, TattooType.Face);
                pawn.style.BodyTattoo = PawnStyleItemChooser.RandomTattooFor(pawn, TattooType.Body);
            }
            else pawn.style.SetupTattoos_NoIdeology();
            pawn.style.nextHairColor = null;
            pawn.style.Notify_StyleItemChanged();
        }
    }
}
