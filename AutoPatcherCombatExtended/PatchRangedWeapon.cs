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
        internal static void PatchRangedWeapon(ThingDef def, APCEPatchLogger log)
        {
            try
            {

                APCEConstants.gunKinds gunKind = DetermineGunKind(def);

                if (APCESettings.printLogs)
                {
                    Log.Message($"APCE thinks that {def.label} is a gun of kind: {gunKind}");
                }
                if (!(gunKind == APCEConstants.gunKinds.Mortar))
                {

                    def.statBases = PatchStatBases(def, gunKind);

                    PatchAllTools(def);

                    //ShootBeams can end here, no comps or verb patching needed
                    if (gunKind == APCEConstants.gunKinds.BeamGun)
                    {
                        return;
                    }

                    PatchAllVerbs(def);

                    if (!(gunKind == APCEConstants.gunKinds.Grenade))
                    {
                        AddCompProperties_AmmoUser(def, gunKind);
                        AddCompProperties_FireModes(def, gunKind);

                        VerbPropertiesCE vpce = def.Verbs[0] as VerbPropertiesCE;
                        vpce.recoilAmount = def.statBases.GetStatValueFromList(CE_StatDefOf.Recoil, 1.25f);

                        
                        if (!(gunKind == APCEConstants.gunKinds.Turret) && !(gunKind == APCEConstants.gunKinds.MachineGun))
                        {
                            vpce.recoilPattern = RecoilPattern.Regular;
                        }
                        else
                        {
                            vpce.recoilPattern = RecoilPattern.Mounted;
                        }
                    }
                    else
                    {
                        PatchGrenade(def);
                    }
                }
                else
                {
                    PatchMortar(def);
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