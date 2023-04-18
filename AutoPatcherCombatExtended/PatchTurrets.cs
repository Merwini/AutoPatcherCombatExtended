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
                    turretBase.fillPercent = 0.85f;
                }

                turretBase.thingClass = typeof(CombatExtended.Building_TurretGunCE);

                if (turretBase.comps != null)
                {
                    turretBase.comps.RemoveAll(comp => comp.GetType() == typeof(CompProperties_Refuelable));
                }
                if (!(turretBase.weaponTags == null) && (turretBase.weaponTags.Any(str => str.IndexOf("Artillery", StringComparison.OrdinalIgnoreCase) >= 0)))
                {
                    turretBase.building.turretBurstCooldownTime = 2;
                    turretBase.building.spawnedConceptLearnOpportunity = CE_ConceptDefOf.CE_MortarDirectFire;
                    //turretBase.inspectorTabs = new List<Type>(); //TODO I dunno, but some patches remove this? Not sure what it does.
                }

                Log.Warning(turretBase.defName); //DEBUG

                log.PatchSucceeded();
            }
            catch (Exception ex)
            {
                log.PatchFailed(turretBase.defName, ex);
            }
        }
    }
}