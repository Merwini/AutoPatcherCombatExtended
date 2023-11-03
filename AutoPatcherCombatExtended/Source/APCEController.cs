using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using Verse.AI;

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

            //Search for mods that need patching
            //

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
                SortAndPatchDef(def, log);
            }
            log.EndPatch();
        }

        public static void SortAndPatchDef(Def def, APCEPatchLogger log)
        {
            //vehicles checked by a Harmony prefix applied by APCEVF PatchVehicle
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

        public static void FindModsNeedingPatched()
        {
            List<ModContentPack> modsNeedingPatched = new List<ModContentPack>();
            for (int i = 0; i < APCESettings.activeMods.Count; i++)
            {
                if (!APCESettings.modsToPatch.Contains(APCESettings.activeMods[i])
                    && APCESettings.activeMods[i].AllDefs.Count() != 0
                    && CheckIfModNeedsPatched(APCESettings.activeMods[i]))
                {
                    modsNeedingPatched.Add(APCESettings.activeMods[i]);
                }
            }
        }

        public static bool CheckIfModNeedsPatched(ModContentPack mod)
        { 
            foreach (Def def in mod.AllDefs)
            {
                if (CheckIfDefNeedsPatched(def))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool CheckIfDefNeedsPatched(Def def)
        {
            //TODO check for vehicle defs is added by Harmony prefix

            if (def is ThingDef thingDef)
            {
                //check if is apparel
                    //check if apparel has no bulk / wornbulk
                //check if is weapon
                    //check if is ranged and has compammouser
                    //check if is melee and has melee statBases - check for all of them

            }

            return false;
        }
    }
}
