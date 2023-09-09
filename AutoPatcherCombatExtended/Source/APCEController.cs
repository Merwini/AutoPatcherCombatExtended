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
    public static partial class APCEController
    {
        static APCEController()
        {
            Log.Message("APCE Controller constructed");
            APCESettings.activeMods = GetActiveModsList();
            APCESettings.modsToPatch = RebuildModsToPatch();
            InjectedDefHasher.PrepareReflection();
            CompatibilityPatches compat = new CompatibilityPatches();
            compat.PatchMods();
            APCEHarmonyPatches harmony = new APCEHarmonyPatches();

            //defaults should be rebuilt on startup
            ModDataHolder apceDefaults = new ModDataHolder();
            apceDefaults.packageId = "nuff.apcedefaults";
            APCESettings.modDataDict.Add(apceDefaults.packageId, apceDefaults);

            APCEPatchController();
        }

        public static void APCEPatchController()
        {
            if (APCESettings.printLogs)
            {
                APCEPatchLogger.stopwatchMaster.Start();
            }
            //CleanModList(APCESettings.modsToPatch);
            if (!APCESettings.patchWeapons)
            {
                DisableGenericAmmos();
            }
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
                DetermineDefType(def, log);
            }
            log.EndPatch();
        }

        public static void DetermineDefType(Def def, APCEPatchLogger log)
        {
            if (def is ThingDef td)
            {
                if (td.IsApparel
                    && APCESettings.patchApparels)
                {
                    PatchApparel(td, log);
                    return;
                }
                else if (td.IsWeapon
                    && APCESettings.patchWeapons)
                {
                    if (td.IsRangedWeapon
                        && (!typeof(Verb_CastAbility).IsAssignableFrom(td.Verbs[0].verbClass))
                        && (!typeof(Verb_CastBase).IsAssignableFrom(td.Verbs[0].verbClass)))
                    {
                        PatchRangedWeapon(td, log);
                        return;
                    }
                    else //if (td.IsMeleeWeapon)
                    {
                        PatchMeleeWeapon(td, log);
                        return;
                    }
                }
                else if (typeof(Pawn).IsAssignableFrom(td.thingClass))
                {
                    PatchPawn(td, log);
                    return;
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
                return;
            }
            else if (def is PawnKindDef pkd
                && APCESettings.patchPawnKinds)
            {
                PatchPawnKind(pkd, log);
                return;
            }
            else if (ModLister.BiotechInstalled
                && def is GeneDef gene
                && APCESettings.patchGenes)
            {
                PatchGene(gene, log);
                return;
            }
            else
            {
                HandleUnknownDef(def, log);
            }
        }
    }
}
