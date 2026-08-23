using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MihoAdfqs
{
    public class MihoAdfqsMain:Mod
    {
        public MihoAdfqsMain(ModContentPack content) : base(content)
        {
            var harmony = new Harmony("chezhou.kind.mihoadfqs");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}
