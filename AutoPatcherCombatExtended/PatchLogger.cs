﻿using CombatExtended;
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
    public partial class AutoPatcherCombatExtended : Mod
    {
        Stopwatch stopwatch = new Stopwatch();
        Stopwatch stopwatchMaster = new Stopwatch();

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

        private void BeginPatch(string defCat)
        {
            if (Settings.printDebug)
            {
                stopwatch.Start();
                Log.Message($"Attempting to patch {defCat} Defs");
            }

        }

        private void EndPatch(string defCat)
        {
            if (Settings.printDebug)
            {
                stopwatch.Stop();
                Log.Message(EndPatchString(defCat));
                if (defsFailed != 0)
                {
                    Log.Error($"Failed to patch the following {defCat} defs: \n {failureList}");
                    failureList.Clear();
                }

                stopwatch.Reset();
                defsPatched = 0;
                defsTotal = 0;
                defsFailed = 0;
            }
        }

        private string EndPatchString(string defCat)
        {
            return String.Format("Patched defs of type {0} in {1:F4} seconds. {2} patched out of {3}, {4} failed.", defCat, stopwatch.ElapsedMilliseconds / 1000f, defsPatched, defsTotal, defsFailed);
        }

    }
}