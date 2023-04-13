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
            return "Autopatcher for CE";
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard list = new Listing_Standard();

            Text.Font = GameFont.Medium;
            list.Begin(inRect);
            list.EnumSelector(ref APCESettings.settingsTabs, "", "", "select settings page");

            if (APCESettings.settingsTabs == APCESettings.SettingsTabs.General_Settings)
            {
                list.CheckboxLabeled("Patch weapons from selected mods", ref APCESettings.patchWeapons);
                list.CheckboxLabeled("Patch apparels from selected mods", ref APCESettings.patchApparels);
                list.CheckboxLabeled("Patch pawns from selected mods", ref APCESettings.patchPawns);
                list.CheckboxLabeled("Patch hediffs from selected mods", ref APCESettings.patchHediffs);
                list.CheckboxLabeled("Show patch logs", ref APCESettings.printLogs);
                list.CheckboxLabeled("Enable Debug Mode (print errors)", ref APCESettings.printPatchErrors);
            }

            else if (APCESettings.settingsTabs == APCESettings.SettingsTabs.Modlist)
            {
                list.ListControl(inRect, ref APCESettings.activeMods, ref APCESettings.modsToPatch, ref Settings.searchTerm, ref Settings.leftScrollPosition, ref Settings.rightScrollPosition,
                    ref Settings.leftSelectedObject, ref Settings.rightSelectedObject, "Mods to patch", rectPCT: 0.85f);
            }

            else if (APCESettings.settingsTabs == APCESettings.SettingsTabs.Balance_Control)
            {
                list.Label("Balance Control placeholder");
                list.EnumSelector(ref APCESettings.balanceTabs, "", "", "select balance category");
                if (APCESettings.balanceTabs == APCESettings.BalanceTabs.Apparel)
                {
                    list.Gap();
                    Text.Font = GameFont.Medium;
                    list.Label("Armor value settings");
                    Text.Font = GameFont.Small;

                    list.Gap();
                    list.Label("These multipliers will apply to all apparels, and are then further multiplied by techlevel");
                    list.TextFieldNumericLabeled("Armor sharp base multiplier (default: 10)", ref APCESettings.apparelSharpMult, ref APCESettings.apparelSharpMultBuffer);
                    list.TextFieldNumericLabeled("Armor blunt base multiplier (default: 40)", ref APCESettings.apparelBluntMult, ref APCESettings.apparelBluntMultBuffer);
                    list.Gap();
                    
                    list.TextFieldNumericLabeled("Tech level 'Animal' armor multiplier (default: 0.25)", ref APCESettings.armorTechMultAnimal, ref APCESettings.armorTechMultAnimalBuffer);
                    list.TextFieldNumericLabeled("Tech level 'Neolithic' armor multiplier (default: 0.5)", ref APCESettings.armorTechMultNeolithic, ref APCESettings.armorTechMultNeolithicBuffer);
                    list.TextFieldNumericLabeled("Tech level 'Medieval' armor multiplier (default: 0.75)", ref APCESettings.armorTechMultMedieval, ref APCESettings.armorTechMultMedievalBuffer);
                    list.TextFieldNumericLabeled("Tech level 'Industrial' armor multiplier (default: 1.0)", ref APCESettings.armorTechMultIndustrial, ref APCESettings.armorTechMultIndustrialBuffer);
                    list.TextFieldNumericLabeled("Tech level 'Spacer' armor multiplier (default: 2.0)", ref APCESettings.armorTechMultSpacer, ref APCESettings.armorTechMultSpacerBuffer);
                    list.TextFieldNumericLabeled("Tech level 'Ultratech' armor multiplier (default: 3.0)", ref APCESettings.armorTechMultUltratech, ref APCESettings.armorTechMultUltratechBuffer);
                    list.TextFieldNumericLabeled("Tech level 'Archotech' armor multiplier (default: 4.0)", ref APCESettings.armorTechMultArchotech, ref APCESettings.armorTechMultArchotechBuffer);
                    
                    list.NewColumn();
                    Text.Font = GameFont.Medium;
                    //list.Label("Bulk value settings");
                    Text.Font = GameFont.Small;
                }
                if (APCESettings.balanceTabs == APCESettings.BalanceTabs.Weapons)
                {
                    list.Gap();
                    Text.Font = GameFont.Medium;
                    list.Label("Weapon tool (melee attacks) settings");
                    Text.Font = GameFont.Small;
                    list.TextFieldNumericLabeled("Weapon tool power (damage) multiplier (default: 1)", ref APCESettings.weaponToolPowerMult, ref APCESettings.weaponToolPowerMultBuffer);
                    list.TextFieldNumericLabeled("Weapon tool sharp penetration multiplier (default: 10)", ref APCESettings.weaponToolSharpPenetration, ref APCESettings.weaponToolSharpPenetrationBuffer);
                    list.TextFieldNumericLabeled("Weapon tool blunt penetration multiplier (default: 40)", ref APCESettings.weaponToolBluntPenetration, ref APCESettings.weaponToolBluntPenetrationBuffer);

                    //list.Gap();
                    //Text.Font = GameFont.Medium;
                    //list.Label("Weapon tool (melee attacks) settings");
                    //Text.Font = GameFont.Small;
                }
                if (APCESettings.balanceTabs == APCESettings.BalanceTabs.Pawns)
                {
                    list.Gap();
                    Text.Font = GameFont.Medium;
                    list.Label("Pawn armor settings");
                    Text.Font = GameFont.Small;
                    list.TextFieldNumericLabeled("Pawn sharp armor multiplier (default: 10)", ref APCESettings.pawnArmorSharpMult, ref APCESettings.pawnArmorSharpMultBuffer);
                    list.TextFieldNumericLabeled("Pawn blunt armor multiplier (default: 40)", ref APCESettings.pawnArmorBluntMult, ref APCESettings.pawnArmorBluntMultBuffer);

                    list.Gap();
                    Text.Font = GameFont.Medium;
                    list.Label("Pawn tool (melee attack) settings");
                    Text.Font = GameFont.Small;
                    list.TextFieldNumericLabeled("Pawn tool power (damage) multiplier (default: 1)", ref APCESettings.pawnToolPowerMult, ref APCESettings.pawnToolPowerMultBuffer);
                    list.TextFieldNumericLabeled("Pawn tool sharp penetration multiplier (default: 10)", ref APCESettings.pawnToolSharpPenetration, ref APCESettings.pawnToolSharpPenetrationBuffer);
                    list.TextFieldNumericLabeled("Pawn tool blunt penetration multiplier (default: 40)", ref APCESettings.pawnToolBluntPenetration, ref APCESettings.pawnToolBluntPenetrationBuffer);
                }
                if (APCESettings.balanceTabs == APCESettings.BalanceTabs.Hediffs)
                {
                    list.Gap();
                    Text.Font = GameFont.Medium;
                    list.Label("Hediff settings (really just armor values from Hediffs)");
                    Text.Font = GameFont.Small;

                    list.Gap();
                    list.TextFieldNumericLabeled("Hediff sharp armor multiplier (default: 10)", ref APCESettings.hediffSharpMult, ref APCESettings.hediffSharpMultBuffer);
                    list.TextFieldNumericLabeled("Hediff blunt armor multiplier (default: 40)", ref APCESettings.hediffBluntMult, ref APCESettings.hediffBluntMultBuffer);
                }
            }

            else
            {
                throw new Exception("The legendary FOURTH TAB of the settings window has been found.");
            }

            list.End();
        }
    }
}
