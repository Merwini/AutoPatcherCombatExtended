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

        //to be used by the four patch methods
        //0 : string, type of def being patched by that method
        //1 : float, stopwatch time elapsed for that patch operation
        //2 : integer, defs of that type found
        //3 : integer, defs successfully patched
        //4 : integer, defs unsuccessfully patched
        //used for logging
        int defsPatched = 0; //2
        int defsTotal = 0; //3
        int defsFailed = 0; //4
        StringBuilder failureList = new StringBuilder(); //will be logged after PatchString if defsFailed > 0
        ModContentPack currentMod;

        APCEPatchLogger(ModContentPack mod)
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
            return String.Format($"Patching finished on {currentMod.Name}. Successfully patched {defsFailed} out of {defsTotal} defs.");
        }

    }
}