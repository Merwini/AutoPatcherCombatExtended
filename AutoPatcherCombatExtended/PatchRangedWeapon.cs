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
        internal static void PatchRangedWeapon(ThingDef def, APCEPatchLogger log)
        {
            try
            {

                APCESettings.gunKinds gunKind = DetermineGunKind(def);

                if (APCESettings.printLogs)
                {
                    Log.Message($"APCE thinks that {def.label} is a gun of kind: {gunKind}");
                }
                log.PatchSucceeded();

                PatchAllTools(def);
                List<StatModifier> newStatBases = PatchStatBases(def, gunKind);

                PatchAllVerbs(def);
                def.statBases = newStatBases;
                if (gunKind != APCESettings.gunKinds.Grenade)
                {
                    AddCompsAmmoUser(def, gunKind);
                    AddCompsFireModes(def, gunKind);
                }
                else
                {
                    PatchGrenade(def);
                }
            }
            catch (Exception ex)
            {
                log.PatchFailed(def.defName, ex);
            }
        }
    }
}