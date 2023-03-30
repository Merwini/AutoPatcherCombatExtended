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

            Text.Font = GameFont.Medium;
            listingStandard.Begin(inRect);
            listingStandard.EnumSelector(ref APCESettings.settingsTabs, "", "", "select settings page");

            if (APCESettings.settingsTabs == APCESettings.SettingsTabs.General_Settings)
            {
                listingStandard.CheckboxLabeled("Enable Debug Mode", ref APCESettings.printDebug);
            }

            else if (APCESettings.settingsTabs == APCESettings.SettingsTabs.Modlist)
            {
                listingStandard.Label("modlist placeholder");
                listingStandard.ListControl(inRect, ref APCESettings.activeMods, ref APCESettings.modsToPatch, ref Settings.searchTerm, ref Settings.leftScrollPosition, ref Settings.rightScrollPosition,
                    ref Settings.leftSelectedObject, ref Settings.rightSelectedObject, "Mods to patch", rectPCT: 0.85f);
            }

            else if (APCESettings.settingsTabs == APCESettings.SettingsTabs.Balance_Control)
            {
                listingStandard.Label("Balance Control placeholder");
            }

            else
            {
                throw new Exception("The legendary FOURTH TAB of the settings window has been found.");
            }

            listingStandard.End();
        }
    }
}
