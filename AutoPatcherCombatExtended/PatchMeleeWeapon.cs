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
    partial class APCEController
    {
        internal static void PatchMeleeWeapon(ThingDef def, APCEPatchLogger log)
        {
            PatchAllTools(def);

            //TODO add secondary stats like crits and dodge
        }
    }
}