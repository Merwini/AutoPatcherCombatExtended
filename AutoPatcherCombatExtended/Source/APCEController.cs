using System;
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
            APCESettings.modsToPatch = RebuildModsToPatchFromSaved();
            InjectedDefHasher.PrepareReflection();

            ModContentPack thisMod = LoadedModManager.runningMods.First(mod => mod.PackageId.Contains("nuff.ceautopatcher"));

            CompatibilityPatches compat = new CompatibilityPatches(thisMod);
            compat.PatchMods();

            APCESaveLoad saveLoad = new APCESaveLoad();
            APCESaveLoad.LoadDataHolders();

            //if a nuff.ceautopatcher ModDataHolder wasn't loaded, make one. essential for autocalcs and changing settings
            if (!APCESettings.modDataDict.ContainsKey("nuff.ceautopatcher") && !APCESettings.modDataDict.ContainsKey("nuff.ceautopatcher_steam"))
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

        // This is called by WriteSettings of the Mod class, because it needs to run every time settings are changed.
        public static void APCEPatchController()
        {
            foreach (ModContentPack mod in APCESettings.modsToPatch)
            {
                DataHolderUtils.GenerateDataHolderForMod(mod);
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

        public static List<ModContentPack> GetActiveModsList()
        {
            List<ModContentPack> activeMods = LoadedModManager.RunningMods.Where(mod => !mod.IsOfficialMod
                                                                                        && !(mod.AllDefs.Count() == 0)
                                                                                        && !(mod.PackageId == "ceteam.combatextended")
                                                                                        && !(mod.PackageId == "ceteam.combatextended_steam")
                                                                                        && !(mod.PackageId == "nuff.ceautopatcher")
                                                                                        && !(mod.PackageId == "nuff.ceautopatcher_steam")
                                                                                        ).OrderBy(mod => mod.Name).ToList();
            return activeMods;
        }

        public static List<ModContentPack> RebuildModsToPatchFromSaved()
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
