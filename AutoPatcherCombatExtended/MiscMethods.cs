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
        public void BasicException(Exception ex)
        {
            Log.Error(ex.ToString());
        }

        public static List<ModContentPack> GetActiveModsList()
        {
            //Log.Message("finding mods");
            List<ModContentPack> activeMods = new List<ModContentPack>(LoadedModManager.RunningMods.Where(mod => !mod.IsOfficialMod).OrderBy(mod => mod.Name).ToList());
            //Log.Message($"Found {activeMods.Count} mods");
            return activeMods;
        }

        public static List<ModContentPack> RebuildModsToPatch()
        {
            List<ModContentPack> modsToPatch = new List<ModContentPack>();
            foreach (ModContentPack mod in APCESettings.activeMods)
            {
                Log.Message($"{mod.Name} has {mod.AllDefs.Count()} defs");
                foreach (Def def in mod.AllDefs)
                {
                    Log.Message(def.defName);
                }
                if (APCESettings.modsByPackageId.Contains(mod.PackageId))
                {
                    modsToPatch.Add(mod);
                }
            }
            return modsToPatch;
        }

        public static void CleanModList(List<ModContentPack> modList)
        {
            foreach (ModContentPack mod in modList)
            {
                //I know these could be combined into one check, but it's easier this way
                if (mod == null)
                {
                    modList.Remove(mod);
                    return;
                }
                if (mod.AllDefs == null || mod.AllDefs.Count() == 0)
                {
                    Log.Message($"Mod named \"{mod.Name}\" has no defs to patch. Removing from the list.");
                    modList.Remove(mod);
                    return;
                }
            }
        }
    }

}