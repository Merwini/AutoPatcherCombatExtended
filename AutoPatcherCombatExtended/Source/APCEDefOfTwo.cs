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

    [DefOf]
    public static class APCEDefOfTwo
    {
        //TODO maybe find a better way to do this. Literally just need this reference, but APCEDefOf already has a 'Blunt'. Stupid design to allow defs of different types to have the same defName
        #pragma warning disable CS0649

        public static DamageArmorCategoryDef Blunt;
    }
}