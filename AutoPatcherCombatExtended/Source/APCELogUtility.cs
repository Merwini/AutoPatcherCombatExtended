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
                causeString.Append($"Mod {defs[0].modContentPack.Name} has the following defs that appear to need patching: ");
                foreach (Def def in defs)
                {
                    causeString.Append($"\n{def.defName}");
                }
                Log.Message(causeString.ToString());
            }
        }

        public static void LogDefsCauseNotSuggested(List<Def> defs)
        {
            if (APCESettings.printLogs)
            {
                StringBuilder causeString = new StringBuilder("");
                causeString.Append($"Mod {defs[0].modContentPack.Name} has some defs that need patching, but was not suggested due to the following defs that appear already patched: ");
                foreach (Def def in defs)
                {
                    causeString.Append($"\n{def.defName}");
                }
                Log.Message(causeString.ToString());
            }
        }
    }
}