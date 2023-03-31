using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace nuff.AutoPatcherCombatExtended
{
    public class APCESettings : ModSettings
    {
        public enum SettingsTabs
        {
            General_Settings,
            Modlist,
            Balance_Control,
        }

        internal static SettingsTabs settingsTabs = SettingsTabs.General_Settings;

        //General Settings
        internal static bool patchWeapons = true;
        internal static bool patchApparels = true;
        internal static bool patchPawns = true;
        internal static bool patchHediffs = true;
        internal static bool printLogs = false;
        internal static bool printPatchErrors = false;


        //Modlist Settings
        public static List<ModContentPack> activeMods = new List<ModContentPack>(); //will not be saved. will be gotten at startup
        public static List<ModContentPack> modsToPatch = new List<ModContentPack>(); //will also not be saved, but instead a saved list of PackageIDs will be used to rebuild this list at startup
        public static List<string> modsByPackageId = new List<string>(); //this is the list that will be used to rebuild the modsToPatch list on startup


        public string searchTerm = "";
        public Vector2 leftScrollPosition = new Vector2();
        public Vector2 rightScrollPosition = new Vector2();
        public ModContentPack leftSelectedObject = null;
        public ModContentPack rightSelectedObject = null;


        //Balance Control Settings


        public override void ExposeData()
        {
            //General Settings
            Scribe_Values.Look(ref printLogs, "printDebug");
            Scribe_Values.Look(ref printPatchErrors, "printPatchErrors");
            Scribe_Values.Look(ref patchWeapons, "patchWeapons");
            Scribe_Values.Look(ref patchApparels, "patchApparels");
            Scribe_Values.Look(ref patchPawns, "patchPawns");
            Scribe_Values.Look(ref patchHediffs, "patchHediffs");

            //Modlist Settings
            Scribe_Collections.Look(ref modsByPackageId, "modsByPackageId", LookMode.Value);


            //Balance Control Settings


            base.ExposeData();
        }
    }
}
