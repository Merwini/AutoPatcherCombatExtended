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
            FormatDefsCauseList(defs);

            if (APCESettings.printLogs)
            {
                StringBuilder causeString = new StringBuilder("");
                causeString.AppendLine($"Mod {defs[0].modContentPack.Name} was suggested to patch due to defs: ");
                APCESettings.modUnpatchedDefsDict.TryGetValue(defs[0].modContentPack, out string str);

                causeString.Append(str);


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

        public static void FormatDefsCauseList(List<Def> defs)
        {
            StringBuilder sb = new StringBuilder();

            foreach (Def def in defs)
            {
                sb.AppendLine($"\nlabel:{def.label}   defName:{def.defName}   type:{def.GetType()}");
            }

            APCESettings.modUnpatchedDefsDict[defs[0].modContentPack] = sb.ToString();
        }
    }
}