using CombatExtended;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Reflection;
using Verse;
using UnityEngine;

namespace nuff.AutoPatcherCombatExtended
{
    [StaticConstructorOnStartup]
    public class AutoPatcherCombatExtended : Mod
    {
        APCESettings Settings;

        public AutoPatcherCombatExtended(ModContentPack content) : base(content)
        {
            this.Settings = GetSettings<APCESettings>();
        }

        public override string SettingsCategory()
        {
            return "Autopatcher for Combat Extended";
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();

            Text.Font = GameFont.Medium;
            listingStandard.Begin(inRect);
            listingStandard.EnumSelector(ref APCESettings.settingsTabs, "", "", "select settings page");

            if (APCESettings.settingsTabs == APCESettings.SettingsTabs.General_Settings)
            {
                listingStandard.CheckboxLabeled("Patch weapons from selected mods", ref APCESettings.patchWeapons);
                listingStandard.CheckboxLabeled("Patch apparels from selected mods", ref APCESettings.patchApparels);
                listingStandard.CheckboxLabeled("Patch pawns from selected mods", ref APCESettings.patchPawns);
                listingStandard.CheckboxLabeled("Patch hediffs from selected mods", ref APCESettings.patchHediffs);
                listingStandard.CheckboxLabeled("Enable Debug Mode", ref APCESettings.printDebug);
            }

            else if (APCESettings.settingsTabs == APCESettings.SettingsTabs.Modlist)
            {;
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
