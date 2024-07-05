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
        public static void LogDefCause(Def def)
        {
            if (APCESettings.printLogs)
            {
                Log.Message($"Mod {def.modContentPack.Name} was suggested to patch due to def {def.defName}.");
            }
        }
    }
}