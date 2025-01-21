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
    [StaticConstructorOnStartup]
    public static class APCESaveLoad
    {
        private static string FolderPath;
        private static string DataHoldersPath;
        private static string PatchesPath;

        static APCESaveLoad()
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

            LoadDataHolders();
        }

        internal static bool LoadDataHolders()
        {
            foreach (string modDataFolder in Directory.EnumerateDirectories(DataHoldersPath))
            {
                try
                {
                    LoadModDataHolder(modDataFolder);

                    LoadDefDataHolders(modDataFolder);
                }
                catch (Exception ex)
                {
                    Log.Error("Error while loading DataHolders: \n" + ex.ToString());
                }
                finally
                {

                }
            }


            return false;
        }

        internal static void LoadModDataHolder(string modDataFolder)
        {
            string[] mdha = Directory.GetFiles(modDataFolder, "ModDataHolder.xml");
            if (mdha.Length != 0)
            {
                ModDataHolder mdh = new ModDataHolder();
                Scribe.loader.InitLoading(mdha[0]);
                mdh.ExposeData();
            }
        }

        internal static bool LoadDefDataHolders(string modDataFolder)
        {
            foreach (var entry in AutoPatcherCombatExtended.defFolderTypesDictionary)
            {
                LoadDefDataHolderFolder(Path.Combine(modDataFolder, entry.Key), entry.Value);
            }

            return true;
        }

        internal static bool LoadDefDataHolderFolder(string folderPath, Type defType)
        {
            if (!Directory.Exists(folderPath))
            {
                return false;
            }

            foreach (string defDataHolderFile in Directory.EnumerateFiles(folderPath, "*.xml"))
            {
                LoadDefDataHolderFile(defDataHolderFile, defType);
            }

            return true;
        }

        internal static void LoadDefDataHolderFile(string filePath, Type defType)
        {
            if (filePath == null || !File.Exists(filePath))
            {
                Log.Error("Error: tried to load DefDataHolders, but either file path or Def Type was null");
                return;
            }

            if (defType == null || !typeof(DefDataHolder).IsAssignableFrom(defType))
            {
                Log.Error("Error: tried to load DefDataHolders, but either Def Type was null, or was not a valid Type");
            }

            Object obj = Activator.CreateInstance(defType);
            if (obj is DefDataHolder defDataHolder)
            {
                Scribe.loader.InitLoading(filePath);
                defDataHolder.ExposeData();
            }
        }
    }
}