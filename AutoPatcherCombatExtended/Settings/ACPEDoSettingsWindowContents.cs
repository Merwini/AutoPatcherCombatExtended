using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace nuff.AutoPatcherCombatExtended
{
    public partial class AutoPatcherCombatExtended : Mod
    {
        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            APCESettings.SettingsTabs settingsTabs = APCESettings.SettingsTabs.General_Settings;

            Text.Font = GameFont.Medium;
            listingStandard.EnumSelector(ref settingsTabs, "", "", "select settings page");

            if (settingsTabs == APCESettings.SettingsTabs.General_Settings)
            {

            }

            else if (settingsTabs == APCESettings.SettingsTabs.Modlist)
            {

            }

            else if (settingsTabs == APCESettings.SettingsTabs.Balance_Control)
            {

            }

            else
            {
                throw new Exception("The legendary FOURTH TAB of the settings window has been found.");
            }
        }
    }
}
