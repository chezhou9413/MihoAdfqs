using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MihoAdfqs.MihoAdfDefRef
{
    [DefOf]
    public static class MihoDefRef
    {
        public static PawnKindDef Miho_adfqs;
        public static JobDef Miho_TalkToPawn;
        public static JobDef Miho_Brainwash;
        public static IncidentDef Incidents_MihoAdf;
        public static PawnKindDef Miho_SS;
        public static ThoughtDef Thought_BrainwashHappy;
        public static JobDef Miho_ToMyCareerFoPawn;
        public static XenotypeDef Xeno_MihoThirdEmpire;
        public static FactionDef MihoThirdEmpire;
        static MihoDefRef()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(MihoDefRef));
        }
    }
}
