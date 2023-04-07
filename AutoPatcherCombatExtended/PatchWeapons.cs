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
        //TODO
        internal static void PatchWeapon(ThingDef def, APCEPatchLogger log)
        {
            try
            {
                #region Tools
                PatchAllTools(def);
                #endregion

                List<VerbProperties> newVerbsCE = new List<VerbProperties>();
                List<StatModifier> newStatBases = new List<StatModifier>();
                APCESettings.gunTypes gunType = new APCESettings.gunTypes();


                //TODO
                log.PatchSucceeded();
            }
            catch (Exception ex)
            {
                log.PatchFailed(def.defName, ex);
            }
        }
    }
}