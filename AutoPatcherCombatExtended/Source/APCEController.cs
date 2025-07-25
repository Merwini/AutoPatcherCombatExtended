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
            APCESettings.activeMods = GetActiveModsList();
            APCESettings.modsToPatch = RebuildModsToPatch();
            InjectedDefHasher.PrepareReflection();

            ModContentPack thisMod = LoadedModManager.runningMods.First(mod => mod.PackageId.Contains("nuff.ceautopatcher"));

            CompatibilityPatches compat = new CompatibilityPatches(thisMod);
            compat.PatchMods();

            APCEHarmonyPatches harmony = new APCEHarmonyPatches();

            APCESaveLoad saveLoad = new APCESaveLoad();
            APCESaveLoad.LoadDataHolders();

            //if a nuff.ceautopatcher ModDataHolder wasn't loaded, make one. essential for autocalcs and changing settings
            if (!APCESettings.modDataDict.ContainsKey("nuff.ceautopatcher"))
            {
                CreateAPCEModDataHolder();
            }

            Log.Message("APCE Controller constructed");
        }

        internal static void CreateAPCEModDataHolder()
        {
            ModContentPack thisMod = LoadedModManager.runningMods.First(mod => mod.PackageId.Contains("nuff.ceautopatcher"));
            ModDataHolder apceDefaults = new ModDataHolder(thisMod);
            Log.Message("APCE Controller created default ModDataHolder");
        }

        public static void APCEPatchController()
        {
            foreach (ModContentPack mod in APCESettings.modsToPatch)
            {
                GenerateDataHolderForMod(mod);
            }

            foreach (var holder in APCESettings.modDataDict)
            {
                if (APCESettings.modsToPatch.Contains(holder.Value.mod))
                {
                    try
                    {
                        //note this is the ModDataHolder. It then decided which DefDataHolders to patch based on its defDict
                        holder.Value.GenerateDefDataHolders();
                        holder.Value.ReCalc();
                        holder.Value.PrePatch();
                        holder.Value.Patch();
                        holder.Value.PostPatch();
                        holder.Value.RegisterDelayedHolders();
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"Exception while trying to run patches for {holder.Value.mod.Name}. Exception is: \n{ex}");
                    }
                }
            }

            AmmoInjector.Inject();
            AmmoInjector.AddRemoveCaliberFromGunRecipes();
        }

        public static void GenerateDataHolderForMod(ModContentPack mod)
        {
            //Don't generate a ModDataHolder if it already generated during SaveLoad
            if (!APCESettings.modDataDict.ContainsKey(mod.PackageId))
            {
                ModDataHolder mdh = new ModDataHolder(mod);
            }
        }

        public static bool TryGenerateDataHolderForDef(Def def)
        {
            try
            {
                if (HandleDelegatedDefTypesGenerate(def))
                {
                    return true;
                }
                else if (def is ThingDef td)
                {
                    if (td.IsApparel)
                    {
                        DataHolderUtils.GenerateDefDataHolder(def, APCEConstants.DefTypes.Apparel);
                        return true;
                    }
                    else if (td.IsWeapon)
                    {
                        if (td.IsRangedWeapon
                            && (!typeof(Verb_CastAbility).IsAssignableFrom(td.Verbs[0].verbClass))
                            && (!typeof(Verb_CastBase).IsAssignableFrom(td.Verbs[0].verbClass)))
                        {
                            DataHolderUtils.GenerateDefDataHolder(def, APCEConstants.DefTypes.RangedWeapon);
                            return true;
                        }
                        else //if (td.IsMeleeWeapon)
                        {
                            DataHolderUtils.GenerateDefDataHolder(def, APCEConstants.DefTypes.MeleeWeapon);
                            return true;
                        }
                    }
                    else if (typeof(Pawn).IsAssignableFrom(td.thingClass))
                    {
                        DataHolderUtils.GenerateDefDataHolder(def, APCEConstants.DefTypes.Pawn);
                        return true;
                    }
                    else if (typeof(Building_TurretGun).IsAssignableFrom(td.thingClass))
                    {
                        DataHolderUtils.GenerateDefDataHolder(def, APCEConstants.DefTypes.Building_TurretGun);
                        return true;
                    }
                    else if ((td.thingCategories != null) && td.thingCategories.Contains(APCEDefOf.MortarShells))
                    {
                        DataHolderUtils.GenerateDefDataHolder(def, APCEConstants.DefTypes.MortarShell);
                        return true;
                    }
                }
                else if (def is HediffDef hd)
                {
                    DataHolderUtils.GenerateDefDataHolder(def, APCEConstants.DefTypes.Hediff);
                    return true;
                }
                else if (def is PawnKindDef pkd)
                {
                    DataHolderUtils.GenerateDefDataHolder(def, APCEConstants.DefTypes.PawnKind);
                    return true;
                }
                else if (ModLister.BiotechInstalled
                    && def is GeneDef gene)
                {
                    DataHolderUtils.GenerateDefDataHolder(def, APCEConstants.DefTypes.Gene);
                    return true;
                }
                else
                {
                    HandleUnknownDefGenerate(def);
                }
                return false;
            }
            catch (Exception ex)
            {
                Log.Warning($"Exception while trying to generate DefDataHolder for def {def.defName} from mod {def.modContentPack.Name}. Exception: \n" + ex.ToString());
                return false;
            }
        }

        public static bool HandleDelegatedDefTypesGenerate(Def def)
        {
            Type defType = def.GetType();
            if (APCESettings.typeHandlerDictionaryGenerate.TryGetValue(defType, out var handler))
            {
                handler.DynamicInvoke(def);
                return true;
            }
            return false;
        }

        public static bool DefIsDelegatedType(Def def)
        {
            Type defType = def.GetType();
            if (APCESettings.typeHandlerDictionaryCheck.ContainsKey(defType))
            {
                return true;
            }
            return false;
        }

        public static APCEConstants.NeedsPatch HandleDelegatedDefTypesCheck(Def def)
        {
            Type defType = def.GetType();

            if (APCESettings.typeHandlerDictionaryCheck.TryGetValue(defType, out var handler))
            {
                return handler.Invoke(def);
            }
            else
            {
                return APCEConstants.NeedsPatch.ignore;
            }
        }

        public static void HandleUnknownDefGenerate(Def def)
        {
            //TODO remove? most unhandled def types are not in need of patching
            //Log.Warning($"Unable to generate DefDataHolder for {def.defName} from mod {def.modContentPack.Name}. Type {def.GetType()} is unrecognized.");
            return;
        }

        public static APCEConstants.NeedsPatch HandleUnknownDefCheck(Def def)
        {
            //TODO remove?
            //Log.Warning($"Unable to check if def {def.defName} from mod {def.modContentPack.Name} needs patching. Type {def.GetType()} is unrecognized.");
            return APCEConstants.NeedsPatch.ignore;
        }

        public static void FindModsNeedingPatched()
        {
            //make a list of all mods with defs detected as needing patching
            List<ModContentPack> modsNeedingPatched = new List<ModContentPack>();
            for (int i = 0; i < APCESettings.activeMods.Count; i++)
            {
                if (APCESettings.activeMods[i].Name != "Combat Extended" && APCESettings.activeMods[i].AllDefs.Count() != 0
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
            //start as true because switching to false is more final
            bool needsPatched = true;
            List<Def> defsNeedingPatched = new List<Def>();
            List<Def> defsNotNeedingPatched = new List<Def>();

            if (mod.AllDefs == null || mod.AllDefs.Count() == 0)
                return false;

            //iteration does not break when a 'no' is found, so that it can still checks partially-patched mods
            //won't recommend them, but will be useful for finding patches that need updates
            foreach (Def def in mod.AllDefs)
            {
                APCEConstants.NeedsPatch defNeedsPatched = CheckIfDefNeedsPatched(def);
                if (defNeedsPatched == APCEConstants.NeedsPatch.yes)
                {
                    defsNeedingPatched.Add(def);
                }
                else if (defNeedsPatched == APCEConstants.NeedsPatch.no)
                {
                    defsNotNeedingPatched.Add(def);
                    needsPatched = false;
                }
                //if unsure, added to either list
            }

            if (defsNeedingPatched.Count != 0)
            {
                APCELogUtility.LogDefsCause(defsNeedingPatched);
            }

            if (needsPatched && defsNeedingPatched.Count == 0)
            {
                needsPatched = false;
            }

            if (!needsPatched && defsNeedingPatched.Count != 0)
            {
                APCELogUtility.LogDefsCauseNotSuggested(defsNotNeedingPatched);
            }

            return needsPatched;
        }

        public static APCEConstants.NeedsPatch CheckIfDefNeedsPatched(Def def)
        {
            APCEConstants.NeedsPatch needsPatched = APCEConstants.NeedsPatch.ignore;
            try
            {
                if (DefIsDelegatedType(def))
                {
                    needsPatched = HandleDelegatedDefTypesCheck(def);
                }
                else if (def is ThingDef thingDef)
                {
                    if (thingDef.IsApparel)
                    {
                        if (thingDef.statBases.Any(sm => sm.stat == CE_StatDefOf.Bulk)
                        || thingDef.statBases.Any(sm => sm.stat == CE_StatDefOf.WornBulk))
                        {
                            needsPatched = APCEConstants.NeedsPatch.no;
                        }
                        else
                        {
                            needsPatched = APCEConstants.NeedsPatch.yes;
                        }
                    }
                    else if (thingDef.IsWeapon)
                    {
                        if (thingDef.IsRangedWeapon)
                        {
                            if (thingDef.Verbs[0].verbClass.ToString().Contains("CE")
                            || thingDef.statBases.Any(sm => sm.stat == CE_StatDefOf.Bulk)
                            || (!thingDef.comps.NullOrEmpty() && thingDef.comps.Any(comp => comp is CompProperties_AmmoUser)))
                            {
                                needsPatched = APCEConstants.NeedsPatch.no;
                            }
                            else
                            {
                                needsPatched = APCEConstants.NeedsPatch.yes;
                            }
                        }
                        else if (thingDef.IsMeleeWeapon)
                        {
                            if (thingDef.tools.NullOrEmpty() || thingDef.tools.Any(tool => tool is ToolCE))
                            {
                                needsPatched = APCEConstants.NeedsPatch.no;
                            }
                            else
                            {
                                needsPatched = APCEConstants.NeedsPatch.yes;
                            }
                        }
                        //else default/unsure
                    }
                    else if (typeof(Pawn).IsAssignableFrom(thingDef.thingClass))
                    {
                        //can't be sure the Pawn is patched, could be inheriting tools from a patched base
                        if (!thingDef.tools.NullOrEmpty() && thingDef.tools.Any(tool => tool is ToolCE))
                        {
                            needsPatched = APCEConstants.NeedsPatch.unsure;
                        }
                        if (!thingDef.tools.NullOrEmpty() && !thingDef.tools.Any(tool => tool is ToolCE))
                        {
                            needsPatched = APCEConstants.NeedsPatch.yes;
                        }
                    }
                    else if (typeof(Building_TurretGun).IsAssignableFrom(thingDef.thingClass) && thingDef.fillPercent < 0.85f)
                    {
                        needsPatched = APCEConstants.NeedsPatch.yes;
                    }
                    //is the AmmoDef check necessary?
                    else if (!(thingDef is AmmoDef) && thingDef.projectileWhenLoaded != null)
                        {
                            needsPatched = APCEConstants.NeedsPatch.yes;
                        }
                }
                else if (def is HediffDef hd)
                {
                    if (!hd.comps.NullOrEmpty())
                    {
                        HediffCompProperties_VerbGiver hcp_vg = (HediffCompProperties_VerbGiver)hd.comps.FirstOrDefault(c => c is HediffCompProperties_VerbGiver);
                        if (hcp_vg != null && !hcp_vg.tools.NullOrEmpty() && hcp_vg.tools.Any(tool => tool is ToolCE))
                        {
                            needsPatched = APCEConstants.NeedsPatch.no;
                        }
                        else if (hcp_vg != null && !hcp_vg.tools.NullOrEmpty() && !hcp_vg.tools.Any(tool => tool is ToolCE))
                        {
                            needsPatched = APCEConstants.NeedsPatch.yes;
                        }
                    }
                    else if (hd.stages != null && hd.stages.Any(stage => stage.statOffsets != null && stage.statOffsets.Any(stat =>
                           stat.stat == StatDefOf.ArmorRating_Sharp ||
                           stat.stat == StatDefOf.ArmorRating_Blunt ||
                           stat.stat == StatDefOf.ArmorRating_Heat)))
                    {
                        needsPatched = APCEConstants.NeedsPatch.unsure;
                    }
                    //implicit else ignore
                }
                else if (ModLister.BiotechInstalled && def is GeneDef gene)
                {
                    if (gene.statOffsets != null && gene.statOffsets.Any(stat =>
                           stat.stat == StatDefOf.ArmorRating_Sharp ||
                           stat.stat == StatDefOf.ArmorRating_Blunt ||
                           stat.stat == StatDefOf.ArmorRating_Heat))
                    {
                        needsPatched = APCEConstants.NeedsPatch.unsure;
                    }
                }
                else if (def is PawnKindDef pkd)
                {
                    if (pkd.race.race.intelligence != Intelligence.Animal)
                    {
                        if (!pkd.modExtensions.NullOrEmpty() && pkd.modExtensions.Any(ext => ext is LoadoutPropertiesExtension))
                        {
                            needsPatched = APCEConstants.NeedsPatch.no;
                        }
                        else
                        {
                            needsPatched = APCEConstants.NeedsPatch.unsure;
                        }
                    }
                }
                //else default/unsure
                else
                {
                    needsPatched = HandleUnknownDefCheck(def);
                }
            }
            catch (Exception ex)
            {
                Log.Warning($"Exception when checking if def {def.defName} from mod {def.modContentPack.Name} needs patching. Exception is: \n" + ex.ToString());
            }

            if (needsPatched != APCEConstants.NeedsPatch.ignore)
            {
                APCESettings.patchableDefs[def] = needsPatched;
            }
            return needsPatched;
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
