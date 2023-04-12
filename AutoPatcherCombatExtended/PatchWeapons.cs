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


                APCESettings.gunKinds gunKind = DetermineGunKind(def);
                List<StatModifier> newStatBases = PatchStatBases(def, gunKind);



                PatchAllVerbs(def);
                def.statBases = newStatBases;
                AddCompsAmmoUser(def, gunKind);
                AddCompsFireModes(def, gunKind);


                if (APCESettings.printLogs)
                {
                    Log.Message($"APCE thinks that {def.label} is a gun of kind: {gunKind}");
                }
                log.PatchSucceeded();
            }
            catch (Exception ex)
            {
                log.PatchFailed(def.defName, ex);
            }
        }
    }
}