﻿using CombatExtended;
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

        private static Dictionary<string, Type> defFolderTypesDictionary; //TODO initialize this with the core def types and their folders

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
            foreach (string dataHolderFolder in Directory.EnumerateDirectories(DataHoldersPath))
            {
                try
                {
                    LoadModDataHolder(dataHolderFolder);

                    //each type of dataholder will have its own folder and use its own loader method, just in case any need custom logic
                    //will need Delegates for mod def types

                    //TODO call LoadDefDataHolderFolder for each def type in the dictionary


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

        internal static void LoadModDataHolder(string folder)
        {
            string[] mdha = Directory.GetFiles(folder, "ModDataHolder.xml");
            if (mdha.Length != 0)
            {
                ModDataHolder mdh = new ModDataHolder();
                Scribe.loader.InitLoading(mdha[0]);
                mdh.ExposeData();
            }
        }

        internal static bool LoadDefDataHolderFolder(string folderPath, Type defType)
        {
            if (!Directory.Exists(folderPath))
            {
                return false;
            }

            foreach (string defDataHolderFile in Directory.EnumerateFiles(folderPath, "*.xml"))
            {
                LoadDefDataHolderFile(defDataHolderFile);
            }

            return true;
        }

        internal static void LoadDefDataHolderFile(string file)
        {

        }

        public static bool RegisterDefTypeFolder(string folderName, Type defType)
        {
            defFolderTypesDictionary.TryAdd(folderName, defType);
            if (defFolderTypesDictionary.TryGetValue(folderName, out Type value) && value == defType)
            {
                return true;
            }
            return false;
        }
    }
}