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
        static public Stopwatch stopwatchMaster = new Stopwatch();
        Stopwatch stopwatch = new Stopwatch();

        //used for logging
        int defsPatched = 0;
        int defsTotal = 0;
        int defsFailed = 0;
        List<string> failureList = new List<string>(); //will be logged after PatchString if defsFailed > 0
        List<Exception> errorList = new List<Exception>();
        ModContentPack currentMod;

        public APCEPatchLogger(ModContentPack mod)
        {
            currentMod = mod;
        }
        
        public void BeginPatch()
        {
            stopwatch.Start();
            if (APCESettings.printLogs)
            {
                Log.Message($"Attempting to patch defs from {currentMod.Name}");
            }
        }

        public void PatchSucceeded()
        {
            defsTotal++;
            defsPatched++;
        }

        public void PatchFailed(string defName, Exception ex)
        {
            defsTotal++;
            defsFailed++;
            failureList.Add(defName);
            errorList.Add(ex);
        }

        public void EndPatch()
        {
            stopwatch.Stop();
            if (APCESettings.printLogs)
            {
                Log.Message(EndPatchString());
                if (defsFailed != 0)
                {
                    for (int i = 0; i < failureList.Count; i++)
                    {
                        Log.Warning($"Failed to patch def: {failureList[i]}");
                        if (APCESettings.printPatchErrors)
                        {
                            Log.Warning(errorList[i].ToString());
                        }
                    }
                    
                }
            }
            stopwatch.Reset();
        }

        public string EndPatchString()
        {
            return String.Format($"Patching finished on {currentMod.Name}. Successfully patched {defsPatched} out of {defsTotal} defs.");
        }

    }
}