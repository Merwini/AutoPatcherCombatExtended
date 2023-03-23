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
    public partial class AutoPatcherCombatExtended : Mod
    {
        public void BasicException(Exception ex)
        {
            Logger.Error(ex.ToString());
        }

        public void UpdateModList(ThingDef td)
        {
            if (!td.modContentPack.PackageId.Contains("ludeon")
                && !modsNeedingPatches.Contains(td.modContentPack))
            {
                modsNeedingPatches.Add(td.modContentPack);
            }
        }
    }

}