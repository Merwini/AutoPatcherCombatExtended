using RimWorld;
using Verse;
using System;
using System.Collections.Generic;

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

        internal static APCESettings.SettingsTabs settingsTabs = APCESettings.SettingsTabs.General_Settings;

        //General Settings
        internal bool printDebug = true;


        //Modlist Settings
        public static List<ModContentPack> modsToPatch = new List<ModContentPack>();


        //Balance Control Settings


        public override void ExposeData()
        {
            //General Settings
            Scribe_Values.Look(ref printDebug, "printDebug");


            //Modlist Settings
            Scribe_Collections.Look(ref modsToPatch, "modsToPatch", LookMode.Reference);


            //Balance Control Settings


            base.ExposeData();
        }
    }
}
