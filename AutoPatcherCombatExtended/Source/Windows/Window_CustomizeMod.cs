using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using UnityEngine;

namespace nuff.AutoPatcherCombatExtended
{
    class Window_CustomizeMod : Window
    {
        ModContentPack mod;
        ModDataHolder modData;

        public Window_CustomizeMod(ModContentPack mod)
        {
            this.mod = mod;
        }

        public override Vector2 InitialSize
        {
            get
            {
                return new Vector2(900, 700);
            }
        }

        public override void DoWindowContents(Rect inRect)
        {
            Listing_Standard list = new Listing_Standard();
            list.Begin(inRect);
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(0f, 0f, inRect.width - 150f - 17f, 35f), mod.Name);
            list.Gap(45);
            Text.Font = GameFont.Small;

            list.Label("COMING SOON! (hit escape to exit I haven't made a button yet)");

            list.EnumSelector(ref APCESettings.modSettingsTabs, "", "", "select settings page");

            if (APCESettings.modSettingsTabs == APCEConstants.ModSettingsTabs.General_Settings)
            {

            }
            else if (APCESettings.modSettingsTabs == APCEConstants.ModSettingsTabs.Balance_Control)
            {

            }
            else if (APCESettings.modSettingsTabs == APCEConstants.ModSettingsTabs.Deflist)
            {

            }

            list.End();
        }

        public override void PreOpen()
        {
            base.PreOpen();
            modData = APCESettings.modDataDict.TryGetValue(mod.PackageId);
        }
    }
}
