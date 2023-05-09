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
    public class CompProperties_ReplaceMe : CompProperties
    {
        public CompProperties_ReplaceMe()
        {
            compClass = typeof(CompReplaceMe);
        }

        public ThingDef thingToSpawn;
    }
}