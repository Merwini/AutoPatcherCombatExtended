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
            //TODO more
        }

        public static List<ModContentPack> GetActiveModsList()
        {
            List<ModContentPack> activeMods = new List<ModContentPack>(LoadedModManager.RunningMods.Where(mod => !mod.IsOfficialMod).OrderBy(mod => mod.Name).ToList());
            return activeMods;
        }

        public static List<ModContentPack> RebuildModsToPatch()
        {
            List<ModContentPack> modsToPatch = new List<ModContentPack>();
            foreach (ModContentPack mod in APCESettings.activeMods)
            {
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
                    Log.Warning($"Mod named \"{mod.Name}\" has no defs to patch. Removing from the list.");
                    modList.Remove(mod);
                    return;
                }
            }
        }

        public static ToolCE PatchTool(Tool tool)
        {
            ToolCE newToolCE = new ToolCE();
            newToolCE.label = tool.label;
            newToolCE.capacities = tool.capacities;
            newToolCE.power = tool.power * APCESettings.pawnToolPowerMult;
            newToolCE.cooldownTime = tool.cooldownTime;
            newToolCE.linkedBodyPartsGroup = tool.linkedBodyPartsGroup;
            if (tool.armorPenetration >= 0)
            {
                newToolCE.armorPenetrationSharp = tool.armorPenetration * APCESettings.pawnToolSharpPenetration;
                newToolCE.armorPenetrationBlunt = tool.armorPenetration * APCESettings.pawnToolBluntPenetration;
            }
            return newToolCE;
        }
    }
}