using CombatExtended;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Reflection;
using Verse;

namespace nuff.AutoPatcherCombatExtended
{
    public class APCEPatchLogger
    {
        static internal Stopwatch stopwatchMaster = new Stopwatch();
        Stopwatch stopwatch = new Stopwatch();

        //used for logging
        int defsPatched = 0;
        int defsTotal = 0;
        int defsFailed = 0;
        StringBuilder failureList = new StringBuilder(); //will be logged after PatchString if defsFailed > 0
        ModContentPack currentMod;

        internal APCEPatchLogger(ModContentPack mod)
        {
            currentMod = mod;
        }
        
        internal void BeginPatch()
        {
            stopwatch.Start();
            Log.Message($"Attempting to patch defs from {currentMod.Name}");
        }

        internal void EndPatch(string defCat)
        {
            stopwatch.Stop();
            Log.Message(EndPatchString());
            if (defsFailed != 0)
            {
                Log.Error($"Failed to patch the following {defsFailed} defs: \n {failureList}");
            }
            stopwatch.Reset();
        }

        internal string EndPatchString()
        {
            return String.Format($"Patching finished on {currentMod.Name}. Successfully patched {defsPatched} out of {defsTotal} defs.");
        }

    }
}