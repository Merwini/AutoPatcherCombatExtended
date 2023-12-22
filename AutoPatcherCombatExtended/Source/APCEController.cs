﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using Verse.AI;
using CombatExtended;

namespace nuff.AutoPatcherCombatExtended
{
    [StaticConstructorOnStartup]
    public static class APCEController
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

            FindModsNeedingPatched();
        }

        public static void APCEPatchController()
        {
            //if (APCESettings.printLogs)
            //{
            //    APCEPatchLogger.stopwatchMaster.Start();
            //}
            foreach (ModContentPack mod in APCESettings.modsToPatch)
            {
                GenerateDataHoldersForMod(mod);
            }

            Log.Warning("defDataDict has number of entries: " + APCESettings.defDataDict.Count.ToString());

            foreach (var holder in APCESettings.defDataDict)
            {
                try
                {
                    holder.Value.Patch();
                }
                catch (Exception ex)
                {
                    Log.Warning($"Failed to patch def {holder.Value.defName} due to exception: \n" + ex.ToString());
                }
                //if (APCESettings.printLogs)
                //{
                //    APCEPatchLogger.stopwatchMaster.Stop();
                //    Log.Message($"Auto-patcher for Combat Extended finished in {APCEPatchLogger.stopwatchMaster.ElapsedMilliseconds / 1000f} seconds.");
                //}
            }
        }

        public static void GenerateDataHoldersForMod(ModContentPack mod)
        {
            APCEPatchLogger log = new APCEPatchLogger(mod);
            //log.BeginPatch(); //TODO redo logging
            foreach (Def def in mod.AllDefs)
            {
                TryGenerateDataHolderForDef(def);
            }
            //log.EndPatch();
        }

        public static void TryGenerateDataHolderForDef(Def def)
        {
            try
            {
                if (def is ThingDef td)
                {
                    if (td.IsApparel)
                    {
                        DataHolderUtils.GenerateDefDataHolder(def, APCEConstants.DefTypes.Apparel);
                        return;
                    }
                    else if (td.IsWeapon)
                    {
                        if (td.IsRangedWeapon
                            && (!typeof(Verb_CastAbility).IsAssignableFrom(td.Verbs[0].verbClass))
                            && (!typeof(Verb_CastBase).IsAssignableFrom(td.Verbs[0].verbClass)))
                        {
                            DataHolderUtils.GenerateDefDataHolder(def, APCEConstants.DefTypes.RangedWeapon);
                            return;
                        }
                        else //if (td.IsMeleeWeapon)
                        {
                            DataHolderUtils.GenerateDefDataHolder(def, APCEConstants.DefTypes.MeleeWeapon);
                            return;
                        }
                    }
                    else if (typeof(Pawn).IsAssignableFrom(td.thingClass))
                    {
                        DataHolderUtils.GenerateDefDataHolder(def, APCEConstants.DefTypes.Pawn);
                        return;
                    }
                    else if (typeof(Building_TurretGun).IsAssignableFrom(td.thingClass))
                    {
                        DataHolderUtils.GenerateDefDataHolder(def, APCEConstants.DefTypes.Building_TurretGun);
                    }
                    else if ((td.thingCategories != null) && td.thingCategories.Contains(APCEDefOf.MortarShells))
                    {
                        //PatchMortarShell(td, log); TODO mortarshell
                    }
                }
                else if (def is HediffDef hd)
                {
                    DataHolderUtils.GenerateDefDataHolder(def, APCEConstants.DefTypes.Hediff);
                    return;
                }
                else if (def is PawnKindDef pkd)
                {
                    DataHolderUtils.GenerateDefDataHolder(def, APCEConstants.DefTypes.PawnKind);
                    return;
                }
                else if (ModLister.BiotechInstalled
                    && def is GeneDef gene)
                {
                    DataHolderUtils.GenerateDefDataHolder(def, APCEConstants.DefTypes.Gene);
                    return;
                }
                else
                {
                    HandleUnknownDefGenerate(def);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }
        }

        public static void HandleUnknownDefGenerate(Def def)
        {
            Type defType = def.GetType();
            if (APCESettings.typeHandlerDictionaryGenerate.TryGetValue(defType, out var handler))
            {
                handler.DynamicInvoke(def);
            }
            else
            {
                //log.PatchFailed(def.defName, new Exception("Auto-patcher unable to patch def {def.defName} with unrecognized type {defType.ToString()}")); //TODO logging
            }
            //TODO pass the def to the value delegate
            return;
        }

        public static bool HandleUnknownDefCheck(Def def)
        {
            Type defType = def.GetType();

            if (APCESettings.typeHandlerDictionaryCheck.TryGetValue(defType, out var handler))
            {
                return handler.Invoke(def);
            }
            else
            {
                return false;
            }
        }

        public static void FindModsNeedingPatched()
        {
            //make a list of all mods with defs detected as needing patching
            List<ModContentPack> modsNeedingPatched = new List<ModContentPack>();
            for (int i = 0; i < APCESettings.activeMods.Count; i++)
            {
                if (APCESettings.activeMods[i].AllDefs.Count() != 0
                    && CheckIfModNeedsPatched(APCESettings.activeMods[i]))
                {
                    modsNeedingPatched.Add(APCESettings.activeMods[i]);
                }
            }

            //compare modsNeedingPatched list to mods currently selected to patch, add any missing to a list to recommend to the player
            APCESettings.modsToRecommendAdd = new List<ModContentPack>();
            foreach (ModContentPack mod in modsNeedingPatched)
            {
                if (!APCESettings.modsToPatch.Contains(mod))
                {
                    APCESettings.modsToRecommendAdd.Add(mod);
                }
            }

            //compare modsToRecommendAdd to modsNeedingPatched, add any extras to a list to recommend removing, for cases where a patch comes out and the player still has the mod selected to patch
            APCESettings.modsToRecommendRemove = new List<ModContentPack>();
            foreach (ModContentPack mod in APCESettings.modsToPatch)
            {
                if (!modsNeedingPatched.Contains(mod))
                {
                    APCESettings.modsToRecommendRemove.Add(mod);
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
                if (thingDef.IsApparel
                    && !thingDef.statBases.Any(sm => sm.stat == CE_StatDefOf.Bulk)
                    && !thingDef.statBases.Any(sm => sm.stat == CE_StatDefOf.WornBulk))
                {
                    return true;
                }
                else if (thingDef.IsWeapon)
                {
                    if (thingDef.IsRangedWeapon
                        && !thingDef.Verbs[0].verbClass.ToString().Contains("CE")
                        //&& !thingDef.comps.Any(comp => comp is CompProperties_AmmoUser)
                        && !thingDef.statBases.Any(sm => sm.stat == CE_StatDefOf.Bulk))
                    {
                        return true;
                    }
                    else if (thingDef.IsMeleeWeapon
                        && !thingDef.tools.Any(tool => tool is ToolCE))
                    {
                        return true;
                    }
                }
                else if (typeof(Pawn).IsAssignableFrom(thingDef.thingClass)
                    && !thingDef.tools.Any(tool => tool is ToolCE))
                {
                    return true;
                }
            }
            else if (def is HediffDef hd
                && (!hd.comps.NullOrEmpty() && hd.comps.Any(comp => comp is HediffCompProperties_VerbGiver hcp_vg && !hcp_vg.tools.Any(tool => tool is ToolCE))))
            {
                return true;
            }
            /*
            else if (ModLister.BiotechInstalled && def is GeneDef gene)
            {
                //TODO not a great way to see if a gene mod needs to be patched other than really low armor values?
            }
            */
            else if (def is PawnKindDef pkd
                && (pkd.race.race.intelligence != Intelligence.Animal && !pkd.modExtensions.NullOrEmpty() && !pkd.modExtensions.Any(ext => ext is LoadoutPropertiesExtension)))
            {
                return true;
            }
            else
            {
                return HandleUnknownDefCheck(def);
            }

            return false;
        }

        public static List<ModContentPack> GetActiveModsList()
        {
            List<ModContentPack> activeMods = new List<ModContentPack>(LoadedModManager.RunningMods.Where(mod => !mod.IsOfficialMod
                                                                                                              && !(mod.AllDefs.Count() == 0)
                                                                                                              && !(mod.PackageId == "ceteam.combatextended")
                                                                                                              && !(mod.PackageId == "nuff.ceautopatcher")
                                                                                                              ).OrderBy(mod => mod.Name).ToList());
            return activeMods;
        }

        public static List<ModContentPack> RebuildModsToPatch()
        {
            Dictionary<string, ModContentPack> modDict = new Dictionary<string, ModContentPack>();
            List<ModContentPack> modsToPatch = new List<ModContentPack>();

            foreach (ModContentPack mod in APCESettings.activeMods)
            {
                modDict[mod.PackageId] = mod;
            }

            for (int i = APCESettings.modsByPackageId.Count - 1; i >= 0; i--)
            {
                string packageId = APCESettings.modsByPackageId[i];
                if (modDict.TryGetValue(packageId, out ModContentPack mod) && mod != null)
                {
                    modsToPatch.Add(mod);
                }
                else
                {
                    APCESettings.modsByPackageId.RemoveAt(i);
                }
            }
            APCESettings.thisMod = modDict.TryGetValue("nuff.ceautopatcher");

            return modsToPatch;
        }
        public static void RemoveListDuplicates(List<string> list)
        {
            HashSet<string> uniqueItems = new HashSet<string>();
            for (int i = list.Count - 1; i >= 0; i--)
            {
                if (!uniqueItems.Add(list[i]))
                {
                    list.RemoveAt(i);
                }
            }
        }
    }
}
