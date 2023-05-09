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
        internal static void PatchTurretBase(ThingDef turretBase, APCEPatchLogger log)
        {
            try
            {
                if (turretBase.fillPercent < 0.85)
                {
                    turretBase.fillPercent = 0.85f; //needs to be this to shoot over barricades and still be shot over
                }

                turretBase.thingClass = typeof(CombatExtended.Building_TurretGunCE);

                if (turretBase.comps != null)
                {
                    turretBase.comps.RemoveAll(comp => comp is CompProperties_Refuelable);
                }
                if (!(turretBase.weaponTags == null) && (turretBase.weaponTags.Any(str => str.IndexOf("Artillery", StringComparison.OrdinalIgnoreCase) >= 0)))
                {
                    turretBase.building.turretBurstCooldownTime = 2;
                    turretBase.building.spawnedConceptLearnOpportunity = CE_ConceptDefOf.CE_MortarDirectFire;
                    //turretBase.inspectorTabs = new List<Type>(); //TODO look into the necessity of this
                }
                else
                {
                    turretBase.building.turretBurstCooldownTime *= 0.5f; //TODO seems to generally be lowered, refine formula
                }

                if (!(turretBase.statBases == null))
                {
                    StatModifier aimingAccuracy = new StatModifier();
                    aimingAccuracy.stat = CE_StatDefOf.AimingAccuracy;
                    aimingAccuracy.value = 1f; //TODO formula

                    turretBase.statBases.Add(aimingAccuracy);
                }

                log.PatchSucceeded();
            }
            catch (Exception ex)
            {
                log.PatchFailed(turretBase.defName, ex);
            }
        }
    }
}