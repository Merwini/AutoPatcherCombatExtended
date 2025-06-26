using CombatExtended;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.IO;
using Verse;

namespace nuff.AutoPatcherCombatExtended
{
    public class APCESaveLoad
    {
        private static string FolderPath;
        private static string DataHoldersPath;
        private static string PatchesPath;

        public APCESaveLoad()
        {
            FolderPath = Path.Combine(GenFilePaths.SaveDataFolderPath, "NuffsAutoPatcher");
            DataHoldersPath = Path.Combine(FolderPath, "DataHolders");
            PatchesPath = Path.Combine(FolderPath, "ExportedPatches");

            if (!Directory.Exists(FolderPath))
            {
                Directory.CreateDirectory(FolderPath);
            }
            if (!Directory.Exists(DataHoldersPath))
            {
                Directory.CreateDirectory(DataHoldersPath);
            }
            if (!Directory.Exists(PatchesPath))
            {
                Directory.CreateDirectory(PatchesPath);
            }
        }

        internal static bool LoadDataHolders()
        {
            HashSet<string> activeModPackageIds = ModsConfig.ActiveModsInLoadOrder
                .Select(mod => mod.PackageId)
                .ToHashSet();


            foreach (string modDataFolder in Directory.EnumerateDirectories(DataHoldersPath))
            {
                string folderName = new DirectoryInfo(modDataFolder).Name;
                if (!activeModPackageIds.Contains(folderName))
                {
                    Log.Message($"Skipping {folderName} as it does not match any active mod packageId.");
                    continue;
                }

                try
                {
                    LoadModDataHolder(modDataFolder);

                    LoadDefDataHolders(modDataFolder);
                }
                catch (Exception ex)
                {
                    Log.Error("Error while loading DataHolders: \n" + ex.ToString());
                }
            }


            return false;
        }

        internal static void LoadModDataHolder(string modDataFolder)
        {
            string[] mdha = Directory.GetFiles(modDataFolder, "ModDataHolder.xml");
            if (mdha.Length != 0)
            {
                try
                {
                    ModDataHolder mdh = new ModDataHolder();
                    Scribe.loader.InitLoading(mdha[0]);
                    mdh.ExposeData();
                    mdh.GetModContentPack();
                    mdh.RegisterSelfInDict();
                }
                catch (Exception ex)
                {
                    Log.Error(ex.ToString());
                }
                finally
                {
                    Scribe.loader.FinalizeLoading();
                }
            }
        }

        //TODO rewrite this. No more individual folders for DDH types. Will instead store the type as a node and read it
        internal static bool LoadDefDataHolders(string modDataFolder)
        {
            string defDataHoldersFolderPath = Path.Combine(modDataFolder, "DefDataHolders");
            foreach (string defDataHolderFile in Directory.EnumerateFiles(defDataHoldersFolderPath, "*.xml"))
            {
                LoadDefDataHolderFile(defDataHolderFile);
            }

            return true;
        }

        internal static void LoadDefDataHolderFile(string filePath)
        {
            DefDataHolder ddh = null;
            try
            {
                Scribe.loader.InitLoading(filePath);

                string defType = null;
                Scribe_Values.Look(ref defType, "DefDataHolderType");

                if (defType != null)
                {
                    Type type = Type.GetType(defType);
                    if (type != null && typeof(DefDataHolder).IsAssignableFrom(type))
                    {
                        ddh = (DefDataHolder)Activator.CreateInstance(type);
                        ddh.ExposeData();
                    }
                    else
                    {
                        Log.Error($"Unknown or invalid DefDataHolder type: {defType}");
                    }
                }
                else
                {
                    string defName = null;
                    Scribe_Values.Look(ref defName, "defName");
                    Log.Error($"Missing DefDataHolderType in saved data of DefDataHolder for: {defName}.");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to load DefDataHolder from {filePath}: {ex}");
            }
            finally
            {
                Scribe.loader.FinalizeLoading();
            }
        }

        internal static void SaveDataHolders()
        {
            foreach (var entry in APCESettings.modDataDict)
            {
                ModDataHolder mdh = entry.Value;
                if (!mdh.isCustomized)
                {
                    //TODO remove this message, only here for debugging
                    Log.Message($"Content from mod {mdh.mod.Name} is not customized, skipping save");
                    continue;
                }

                string folderPath = MakeFolderForMod(mdh);
                if (folderPath == null)
                {
                    Log.Warning($"Failed to make folder to save data for mod {mdh.mod.Name}");
                    continue;
                }

                if (!SaveModDataHolder(mdh, folderPath))
                {
                    Log.Warning($"Failed to save ModDataHolder for mod {mdh.mod.Name}");
                }

                List<Def> failedDefs = SaveDefDataHolders(mdh, folderPath);
                if (failedDefs.Count != 0)
                {
                    StringBuilder failMessage = new StringBuilder($"Failed to save DefDataHolders for the following defs from {mdh.mod.Name}");
                    foreach (Def def in failedDefs)
                    {
                        failMessage.Append($"\n{def.defName}");
                    }
                    Log.Warning(failMessage.ToString());
                }
            }

            /* input: ModDataHolder for the mod to save
               output: string for the path of the mod's folder. Null string if folder failed to make */
            string MakeFolderForMod(ModDataHolder mdh)
            {
                return MakeFolders();

                string MakeFolders()
                {
                    string modFolderPath = Path.Combine(DataHoldersPath, mdh.packageId);
                    string defFolderPath = Path.Combine(modFolderPath, "DefDataHolders");

                    try
                    {
                        if (!Directory.Exists(modFolderPath))
                        {
                            Directory.CreateDirectory(modFolderPath);
                        }
                        if (!Directory.Exists(defFolderPath))
                        {
                            Directory.CreateDirectory(defFolderPath);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"Error while making folders to save data holders for mod {mdh.mod.Name}: \n {ex.ToString()}");
                    }

                    if (Directory.Exists(modFolderPath) && Directory.Exists(defFolderPath))
                    {
                        return modFolderPath;
                    }
                    else
                    {
                        return null;
                    }
                }
            }

            /* input: ModDataHolder to save, string for the save folder path
               output: bool indicating success or failure */
            bool SaveModDataHolder(ModDataHolder mdh, string folderPath)
            {
                string filePath = Path.Combine(folderPath, "ModDataHolder.xml");
                try
                {
                    Scribe.saver.InitSaving(filePath, "ModDataHolder");
                    mdh.ExposeData();
                }
                catch (Exception ex)
                {
                    Log.Error($"Failed to save ModDataHolder for mod {mdh.mod.Name} \n {ex.ToString()}");
                }
                finally
                {
                    Scribe.saver.FinalizeSaving();
                }

                return File.Exists(filePath);
            }

            /* input: ModDataHolder for the mod with defs to save, string for the save folder path
               output: list of defs with DataHolders that failed to save */
            List<Def> SaveDefDataHolders(ModDataHolder mdh, string folderPath)
            {
                List<Def> failedDefs = new List<Def>();
                string defFolderPath = Path.Combine(folderPath, "DefDataHolders");

                foreach (var entry in mdh.defDict)
                {
                    DefDataHolder ddh = entry.Value;
                    if (!ddh.isCustomized)
                    {
                        continue;
                    }
                    string filePath = Path.Combine(defFolderPath, $"{entry.Key.defName}.xml");

                    try
                    {
                        Scribe.saver.InitSaving(filePath, "DefDataHolder");
                        string defType = ddh.GetType().AssemblyQualifiedName;
                        Scribe_Values.Look(ref defType, "DefDataHolderType");
                        ddh.ExposeData();
                    }
                    catch (Exception ex)
                    {
                        failedDefs.Add(entry.Key);
                        Log.Error($"Failed to save DefDataHolder for def {ddh.defName} from mod {mdh.mod.Name} \n {ex.ToString()}");
                    }
                    finally
                    {
                        Scribe.saver.FinalizeSaving();
                    }

                    if (!File.Exists(filePath) && !failedDefs.Contains(entry.Key))
                    {
                        failedDefs.Add(entry.Key);
                    }
                }

                return failedDefs;
            }
        }

        
    }
}