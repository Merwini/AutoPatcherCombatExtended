using CombatExtended;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using Verse;

namespace nuff.AutoPatcherCombatExtended
{
    public static class APCELogUtility
    {
        public static void LogDefsCause(List<Def> defs)
        {
            if (APCESettings.printLogs)
            {
                StringBuilder causeString = new StringBuilder("");
                causeString.Append($"Mod {defs[0].modContentPack.Name} was suggested to patch due to defs: ");
                foreach (Def def in defs)
                {
                    causeString.Append($"\n{def.defName}");
                }
                Log.Message(causeString);
            }
        }
    }
}