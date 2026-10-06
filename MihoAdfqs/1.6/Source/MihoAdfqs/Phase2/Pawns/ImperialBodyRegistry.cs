using System.Collections.Generic;
using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Verse;

namespace MihoAdfqs.Phase2.Pawns
{
    //构造帝国独立身体记录，加入佩尔克体并保持原种族身体不变。
    [StaticConstructorOnStartup]
    public static class ImperialBodyRegistry
    {
        private static readonly Dictionary<RaceProperties, RaceProperties> Bodies = new Dictionary<RaceProperties, RaceProperties>();
        private static readonly MethodInfo Clone = AccessTools.Method(typeof(object), "MemberwiseClone");
        private static readonly RaceProperties[] ByKind = new RaceProperties[DefDatabase<PawnKindDef>.AllDefsListForReading.Count];

        //在读档前建立兵种索引，人物属性查询不再解析名称或建立身体树。
        static ImperialBodyRegistry()
        {
            foreach (PawnKindDef kind in DefDatabase<PawnKindDef>.AllDefsListForReading)
                if (kind.race != null && (kind.defaultFactionDef?.defName == "MihoThirdEmpire" ||
                    kind.defName.StartsWith("MihoPhase2_", StringComparison.Ordinal) || kind.defName == "Miho_adfqs"))
                    ByKind[kind.index] = Get(kind.race.race);
        }

        //按稳定的Def索引读取身体，未登记的兵种保持原种族属性。
        public static RaceProperties ForKind(PawnKindDef kind) =>
            kind != null && kind.index < ByKind.Length ? ByKind[kind.index] : null;

        //按原身体建立稳定名称的独立定义，供手术和存档定位部件。
        public static RaceProperties Get(RaceProperties original)
        {
            if (Bodies.TryGetValue(original, out RaceProperties existing)) return existing;
            var props = (RaceProperties)Clone.Invoke(original, null);
            var body = new BodyDef { defName = "MihoPhase2_" + original.body.defName, label = original.body.label };
            BodyDef registered = DefDatabase<BodyDef>.GetNamedSilentFail(body.defName);
            if (registered == null)
            {
                body.corePart = CopyPart(original.body.corePart, body, null);
                BodyPartRecord head = Flatten(body.corePart).First(p => p.def.defName == "Head");
                head.parts.Add(new BodyPartRecord { body = body, parent = head, def = DefDatabase<BodyPartDef>.GetNamed("MihoPhase2_PerkOrgan"), depth = BodyPartDepth.Inside, height = head.height, coverage = 0.01f });
                body.ResolveReferences();
                DefDatabase<BodyDef>.Add(body);
            }
            props.body = registered ?? body;
            Compatibility.ImperialRaceProperties.Register(props, original);
            Bodies.Add(original, props);
            return props;
        }
        //复制身体树并重建父子引用。
        private static BodyPartRecord CopyPart(BodyPartRecord source, BodyDef body, BodyPartRecord parent)
        {
            var part = (BodyPartRecord)Clone.Invoke(source, null);
            part.body = body; part.parent = parent;
            part.parts = source.parts.Select(p => CopyPart(p, body, part)).ToList();
            return part;
        }
        //遍历尚未建立缓存的身体树。
        private static IEnumerable<BodyPartRecord> Flatten(BodyPartRecord part)
        {
            yield return part;
            foreach (BodyPartRecord child in part.parts)
                foreach (BodyPartRecord descendant in Flatten(child)) yield return descendant;
        }
    }
}
