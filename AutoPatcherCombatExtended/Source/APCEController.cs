using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using Verse.AI;
using Vehicles;

namespace nuff.AutoPatcherCombatExtended
{
    [StaticConstructorOnStartup]
    public partial class APCEController
    {
        static CompatibilityPatches compat = new CompatibilityPatches();
        static bool vehiclesInstalled = false;

        static APCEController()
        {
            Log.Message("APCE Controller constructed");
            APCESettings.activeMods = GetActiveModsList();
            APCESettings.modsToPatch = RebuildModsToPatch();
            if (ModLister.HasActiveModWithName("Vehicle Framework"))
            {
                vehiclesInstalled = true;
            }
            compat.PatchMods();
            APCEPatchController();
        }

        public static void APCEPatchController()
        {
            if (APCESettings.printLogs)
            {
                APCEPatchLogger.stopwatchMaster.Start();
            }
            //CleanModList(APCESettings.modsToPatch);
            InjectedDefHasher.PrepareReflection();
            foreach (ModContentPack mod in APCESettings.modsToPatch)
            {
                if (APCESettings.modsAlreadyPatched.Add(mod))
                {
                    PatchMod(mod);
                }
            }
            if (APCESettings.printLogs)
            {
                APCEPatchLogger.stopwatchMaster.Stop();
                Log.Message($"Auto-patcher for Combat Extended finished in {APCEPatchLogger.stopwatchMaster.ElapsedMilliseconds / 1000f} seconds.");
            }
        }

        public static void PatchMod(ModContentPack mod)
        {
            APCEPatchLogger log = new APCEPatchLogger(mod);
            log.BeginPatch();
            foreach (Def def in mod.AllDefs)
            {
                if (def is ThingDef td)
                {
                    if (td.IsApparel)
                    {
                        if (APCESettings.patchApparels)
                        {
                            PatchApparel(td, log);
                            continue;
                        }
                    }
                    else if (td.IsWeapon)
                    {
                        if (APCESettings.patchWeapons)
                        {
                            if (td.IsRangedWeapon
                                && (!typeof(Verb_CastAbility).IsAssignableFrom(td.Verbs[0].verbClass))
                                && (!typeof(Verb_CastBase).IsAssignableFrom(td.Verbs[0].verbClass)))
                            {
                                PatchRangedWeapon(td, log);
                                continue;
                            }
                            else //if (td.IsMeleeWeapon)
                            {
                                PatchMeleeWeapon(td, log);
                                continue;
                            }
                        }
                        else
                        {
                            DisableGenericAmmos();
                        }
                    }
                    else if (typeof(Pawn).IsAssignableFrom(td.thingClass))
                    {
                        PatchPawn(td, log);
                        continue;
                    }
                    else if (typeof(Building_TurretGun).IsAssignableFrom(td.thingClass))
                    {
                        PatchTurretBase(td, log);
                    }
                    else if ((td.thingCategories != null) && td.thingCategories.Contains(APCEDefOf.MortarShells))
                    {
                        PatchMortarShell(td, log);
                    }
                }
                else if (def is HediffDef hd
                    && APCESettings.patchHediffs)
                {
                    PatchHediff(hd, log);
                    continue;
                }
                else if (def is PawnKindDef pkd
                    && APCESettings.patchPawnKinds)
                {
                    PatchPawnKind(pkd, log);
                    continue;
                }
                else if (ModLister.BiotechInstalled
                    && def is GeneDef gene
                    && APCESettings.patchGenes)
                {
                    PatchGene(gene, log);
                    continue;
                }
                else if (vehiclesInstalled
                    && def is VehicleDef vd)
                {
                    PatchVehicle(vd, log);
                    continue;
                }
            }
            log.EndPatch();
        }
    }


}
