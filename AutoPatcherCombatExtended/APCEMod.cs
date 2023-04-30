﻿using CombatExtended;
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
            return "Nuff's Autopatcher for CE";
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard list = new Listing_Standard();
            Text.Font = GameFont.Medium;
            list.Begin(inRect);

            if (Current.Game != null)
            {
                list.Label("SETTINGS MUST BE CHANGED FROM THE MAIN MENU.");
            }
            else
            {
                list.EnumSelector(ref APCESettings.settingsTabs, "", "", "select settings page");

                if (APCESettings.settingsTabs == APCEConstants.SettingsTabs.General_Settings)
                {
                    list.CheckboxLabeled("Patch weapons from selected mods", ref APCESettings.patchWeapons);
                    list.CheckboxLabeled("Try to patch custom verbs on guns (safety not guaranteed)", ref APCESettings.patchCustomVerbs);
                    list.CheckboxLabeled("Patch apparels from selected mods", ref APCESettings.patchApparels);
                    list.CheckboxLabeled("Patch headgear layers", ref APCESettings.patchHeadgearLayers);
                    list.CheckboxLabeled("Patch pawns from selected mods", ref APCESettings.patchPawns);
                    list.CheckboxLabeled("Patch hediffs from selected mods", ref APCESettings.patchHediffs);
                    list.CheckboxLabeled("Show patch logs", ref APCESettings.printLogs);
                    list.CheckboxLabeled("Enable Debug Mode (print errors)", ref APCESettings.printPatchErrors);
                }

                else if (APCESettings.settingsTabs == APCEConstants.SettingsTabs.Modlist)
                {
                    list.ListControl(inRect, ref APCESettings.activeMods, ref APCESettings.modsToPatch, ref Settings.searchTerm, ref Settings.leftScrollPosition, ref Settings.rightScrollPosition,
                        ref Settings.leftSelectedObject, ref Settings.rightSelectedObject, "Mods to patch", rectPCT: 0.85f);
                }

                else if (APCESettings.settingsTabs == APCEConstants.SettingsTabs.Balance_Control)
                {
                    list.Label("Balance Control placeholder");
                    list.EnumSelector(ref APCESettings.balanceTabs, "", "", "select balance category");
                    if (APCESettings.balanceTabs == APCEConstants.BalanceTabs.Apparel)
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
                    if (APCESettings.balanceTabs == APCEConstants.BalanceTabs.Weapons)
                    {
                        list.Gap();
                        Text.Font = GameFont.Medium;
                        list.Label("Ranged weapon armor penetration settings");
                        Text.Font = GameFont.Small;
                        list.TextFieldNumericLabeled("Ranged weapon sharp penetration multiplier (default: 10)", ref APCESettings.gunSharpPenMult, ref APCESettings.gunSharpPenMultBuffer);
                        list.TextFieldNumericLabeled("Ranged weapon blunt penetration multiplier (default: 40)", ref APCESettings.gunBluntPenMult, ref APCESettings.gunBluntPenMultBuffer);

                        list.Gap();
                        list.TextFieldNumericLabeled("Tech level 'Animal' ammo penetration multiplier (default: 0.5)", ref APCESettings.gunTechMultAnimal, ref APCESettings.gunTechMultAnimalBuffer);
                        list.TextFieldNumericLabeled("Tech level 'Neolithic' ammo penetration multiplier (default: 1.0)", ref APCESettings.gunTechMultNeolithic, ref APCESettings.gunTechMultNeolithicBuffer);
                        list.TextFieldNumericLabeled("Tech level 'Medieval' ammo penetration multiplier (default: 2.0)", ref APCESettings.gunTechMultMedieval, ref APCESettings.gunTechMultMedievalBuffer);
                        list.TextFieldNumericLabeled("Tech level 'Industrial' ammo penetration multiplier (default: 4.0)", ref APCESettings.gunTechMultIndustrial, ref APCESettings.gunTechMultIndustrialBuffer);
                        list.TextFieldNumericLabeled("Tech level 'Spacer' ammo penetration multiplier (default: 5.0)", ref APCESettings.gunTechMultSpacer, ref APCESettings.gunTechMultSpacerBuffer);
                        list.TextFieldNumericLabeled("Tech level 'Ultratech' ammo penetration multiplier (default: 6.0)", ref APCESettings.gunTechMultUltratech, ref APCESettings.gunTechMultUltratechBuffer);
                        list.TextFieldNumericLabeled("Tech level 'Archotech' ammo penetration multiplier (default: 8.0)", ref APCESettings.gunTechMultArchotech, ref APCESettings.gunTechMultArchotechBuffer);

                        list.Gap();
                        Text.Font = GameFont.Medium;
                        list.Label("Weapon tool (melee attacks) settings");
                        Text.Font = GameFont.Small;
                        list.TextFieldNumericLabeled("Weapon tool power (damage) multiplier (default: 1)", ref APCESettings.weaponToolPowerMult, ref APCESettings.weaponToolPowerMultBuffer);
                        list.TextFieldNumericLabeled("Weapon tool sharp penetration multiplier (default: 10)", ref APCESettings.weaponToolSharpPenetration, ref APCESettings.weaponToolSharpPenetrationBuffer);
                        list.TextFieldNumericLabeled("Weapon tool blunt penetration multiplier (default: 40)", ref APCESettings.weaponToolBluntPenetration, ref APCESettings.weaponToolBluntPenetrationBuffer);

                    }
                    if (APCESettings.balanceTabs == APCEConstants.BalanceTabs.Pawns)
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
                    if (APCESettings.balanceTabs == APCEConstants.BalanceTabs.Hediffs)
                    {
                        list.Gap();
                        Text.Font = GameFont.Medium;
                        list.Label("Hediff settings");
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

        public override void WriteSettings()
        {
            //DEBUG
            APCEController.RemoveListDuplicates(APCESettings.modsByPackageId);
            foreach (ModContentPack mod in APCESettings.modsToPatch)
            {
                if (APCESettings.modsAlreadyPatched.Add(mod))
                {
                    APCEController.PatchMod(mod);
                }
            }
            base.WriteSettings();
        }
    }
}
