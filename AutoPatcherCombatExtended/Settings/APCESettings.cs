using RimWorld;
using Verse;
using System;
using System.Collections.Generic;
using UnityEngine;

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
        internal static bool printDebug = true;


        //Modlist Settings
        public static List<ModContentPack> activeMods = new List<ModContentPack>(); //will not be saved. will be gotten at startup
        public static List<ModContentPack> modsToPatch = new List<ModContentPack>();

        public string searchTerm = "";
        public Vector2 leftScrollPosition = new Vector2();
        public Vector2 rightScrollPosition = new Vector2();
        public ModContentPack leftSelectedObject = null;
        public ModContentPack rightSelectedObject = null;


        //Balance Control Settings


        public override void ExposeData()
        {
            //General Settings
            Scribe_Values.Look(ref printDebug, "printDebug");


            //Modlist Settings
            Scribe_Collections.Look(ref modsToPatch, "modsToPatch", LookMode.Value);


            //Balance Control Settings


            base.ExposeData();
        }
    }
}
