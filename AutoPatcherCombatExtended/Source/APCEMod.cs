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
        Mod CEMod;
        Settings CESettings;

        public AutoPatcherCombatExtended(ModContentPack content) : base(content)
        {
            this.Settings = GetSettings<APCESettings>();
            APCESettings.thisMod = this;
            try
            {
                Type loaderType = Type.GetType("CombatExtended.Loader.Loader, 0CombatExtendedLoader");
                if (loaderType != null)
                {
                    CEMod = LoadedModManager.GetMod(loaderType);
                }
            }
            catch (TypeLoadException ex)
            {
                Log.Message("Type CombatExtended.Loader.Loader not found. Are you running the dev build?");
            }
            if (CEMod == null)
            {
                Type controllerType = Type.GetType("CombatExtended.Controller, CombatExtended");
                if (controllerType != null)
                {
                    CEMod = LoadedModManager.GetMod(controllerType);
                }
            }
            //CEMod = LoadedModManager.GetMod(typeof(CombatExtended.Controller));
            if (CEMod != null)
            {
                CESettings = CEMod.GetSettings<Settings>();
                AdjustCESettings();
            }
            else
            {
                Log.Error("Unable to find Combat Extended settings and adjust them. Good luck.");
            }
            
        }

        public override string SettingsCategory()
        {
            return "Nuff's Auto-Patcher for CE";
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
                    list.Gap();
                    list.Label("Weapon Stuff:");
                    //list.CheckboxLabeled("Patch weapons from selected mods: ", ref APCESettings.patchWeapons);
                    list.CheckboxLabeled("Limit Weapon Mass: ", ref APCESettings.limitWeaponMass);
                    list.CheckboxLabeled("Try to patch custom verbs on guns (safety not guaranteed): ", ref APCESettings.patchCustomVerbs);
                    list.Gap();
                    list.Label("Apparel Stuff:");
                    //list.CheckboxLabeled("Patch apparels from selected mods: ", ref APCESettings.patchApparels);
                    //list.CheckboxLabeled("Patch headgear layers: ", ref APCESettings.patchHeadgearLayers);
                    list.Gap();
                    list.Label("Other Stuff:");
                    //list.CheckboxLabeled("Patch pawns from selected mods: ", ref APCESettings.patchPawns);
                    //list.CheckboxLabeled("Patch PawnKinds from selected mods: ", ref APCESettings.patchPawnKinds);
                    //list.CheckboxLabeled("Patch Genes from selected mods: ", ref APCESettings.patchGenes);
                    //list.CheckboxLabeled("Patch hediffs from selected mods: ", ref APCESettings.patchHediffs);
                    //list.CheckboxLabeled("Patch vehicles from selected mods: ", ref APCESettings.patchVehicles);
                    list.Gap();
                    list.CheckboxLabeled("Show patch logs: ", ref APCESettings.printLogs);
                    list.CheckboxLabeled("Enable Debug Mode (print errors): ", ref APCESettings.printPatchErrors);
                    list.CheckboxLabeled("Stop checking mod after first unpatched def is found: ", ref APCESettings.stopAfterOneDefCheckFails);
                }

                else if (APCESettings.settingsTabs == APCEConstants.SettingsTabs.Modlist)
                {
                    Rect listRect = new Rect(0, 0, inRect.width, inRect.height * 0.85f);
                    list.ListControl(listRect, ref APCESettings.activeMods, ref APCESettings.modsToPatch, ref Settings.searchTerm, ref Settings.leftScrollPosition, ref Settings.rightScrollPosition,
                        ref Settings.leftSelectedObject, ref Settings.rightSelectedObject, "Mods to patch", rectPCT: 1f);


                    //Rect customizeButtonRect = new Rect(inRect.xMax + 10f, inRect.yMax - 40f, 100f, 30f);
                    // Customize Mod button
                    if (Widgets.ButtonText(rect: inRect.BottomPart(0.15f).TopPart(0.5f).RightPart(0.5f).LeftPart(0.3f), "Customize"))
                    {
                        if (Settings.rightSelectedObject != null)
                        {
                            Window_CustomizeMod window = new Window_CustomizeMod(Settings.rightSelectedObject);
                            Find.WindowStack.Add(window);
                        }
                    }
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
                    }
                    if (APCESettings.balanceTabs == APCEConstants.BalanceTabs.Weapons)
                    {
                        list.EnumSelector(ref APCESettings.balanceWeaponTabs, "", "", "select weapon category");
                        if (APCESettings.balanceWeaponTabs == APCEConstants.BalanceWeaponTabs.Ranged)
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
                            list.TextFieldNumericLabeled("Maximum weapon mass (default 20.0)", ref APCESettings.maximumWeaponMass, ref APCESettings.maximumWeaponMassBuffer);
                        }
                        if (APCESettings.balanceWeaponTabs == APCEConstants.BalanceWeaponTabs.Melee)
                        {
                            list.Gap();
                            Text.Font = GameFont.Medium;
                            list.Label("Weapon tool (melee attacks) settings");
                            Text.Font = GameFont.Small;
                            list.TextFieldNumericLabeled("Weapon tool power (damage) multiplier (default: 1)", ref APCESettings.weaponToolPowerMult, ref APCESettings.weaponToolPowerMultBuffer);
                            list.TextFieldNumericLabeled("Weapon tool sharp penetration multiplier (default: 1)", ref APCESettings.weaponToolSharpPenetration, ref APCESettings.weaponToolSharpPenetrationBuffer);
                            list.TextFieldNumericLabeled("Weapon tool blunt penetration multiplier (default: 4)", ref APCESettings.weaponToolBluntPenetration, ref APCESettings.weaponToolBluntPenetrationBuffer);

                            list.Gap();
                            list.TextFieldNumericLabeled("Tech level 'Animal' tool penetration multiplier (default: 1.0)", ref APCESettings.weaponToolTechMultAnimal, ref APCESettings.weaponToolTechMultAnimalBuffer);
                            list.TextFieldNumericLabeled("Tech level 'Neolithic' tool penetration multiplier (default: 1.0)", ref APCESettings.weaponToolTechMultNeolithic, ref APCESettings.weaponToolTechMultNeolithicBuffer);
                            list.TextFieldNumericLabeled("Tech level 'Medieval' tool penetration multiplier (default: 1.0)", ref APCESettings.weaponToolTechMultMedieval, ref APCESettings.weaponToolTechMultMedievalBuffer);
                            list.TextFieldNumericLabeled("Tech level 'Industrial' tool penetration multiplier (default: 2.0)", ref APCESettings.weaponToolTechMultIndustrial, ref APCESettings.weaponToolTechMultIndustrialBuffer);
                            list.TextFieldNumericLabeled("Tech level 'Spacer' tool penetration multiplier (default: 3.0)", ref APCESettings.weaponToolTechMultSpacer, ref APCESettings.weaponToolTechMultSpacerBuffer);
                            list.TextFieldNumericLabeled("Tech level 'Ultratech' tool penetration multiplier (default: 4.0)", ref APCESettings.weaponToolTechMultUltratech, ref APCESettings.weaponToolTechMultUltratechBuffer);
                            list.TextFieldNumericLabeled("Tech level 'Archotech' tool penetration multiplier (default: 6.0)", ref APCESettings.weaponToolTechMultArchotech, ref APCESettings.weaponToolTechMultArchotechBuffer);
                        }
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

                        list.Gap();
                        Text.Font = GameFont.Medium;
                        list.Label("Pawn Kind settings");
                        Text.Font = GameFont.Small;
                        list.CheckboxLabeled("Patch PawnKinds with weapons to be allowed backpacks: ", ref APCESettings.patchBackpacks);
                        list.TextFieldNumericLabeled("Pawn kind minimum ammo magazines when spawned (default: 2)", ref APCESettings.pawnKindMinMags, ref APCESettings.pawnKindMinMagsBuffer);
                        list.TextFieldNumericLabeled("Pawn kind maximum ammo magazines when spawned (default: 5)", ref APCESettings.pawnKindMaxMags, ref APCESettings.pawnKindMaxMagsBuffer);

                        list.Gap();
                        Text.Font = GameFont.Medium;
                        list.Label("Hediff settings");
                        Text.Font = GameFont.Small;
                        list.TextFieldNumericLabeled("Hediff sharp armor multiplier (default: 10)", ref APCESettings.hediffSharpMult, ref APCESettings.hediffSharpMultBuffer);
                        list.TextFieldNumericLabeled("Hediff blunt armor multiplier (default: 40)", ref APCESettings.hediffBluntMult, ref APCESettings.hediffBluntMultBuffer);

                        list.Gap();
                        Text.Font = GameFont.Medium;
                        list.Label("Gene settings");
                        Text.Font = GameFont.Small;
                        list.TextFieldNumericLabeled("Gene sharp armor multiplier (default: 10)", ref APCESettings.geneArmorSharpMult, ref APCESettings.geneArmorSharpMultBuffer);
                        list.TextFieldNumericLabeled("Gene blunt armor multiplier (default: 10)", ref APCESettings.geneArmorBluntMult, ref APCESettings.geneArmorBluntMultBuffer);
                    }
                    if (APCESettings.balanceTabs == APCEConstants.BalanceTabs.Other)
                    {
                        list.Gap();
                        Text.Font = GameFont.Medium;
                        list.Label("Vehicle settings");
                        Text.Font = GameFont.Small;

                        list.Gap();
                        list.TextFieldNumericLabeled("Vehicle sharp armor multiplier (default: 15)", ref APCESettings.vehicleSharpMult, ref APCESettings.vehicleSharpMultBuffer);
                        list.TextFieldNumericLabeled("Vehicle blunt armor multiplier (default: 15)", ref APCESettings.vehicleBluntMult, ref APCESettings.vehicleBluntMultBuffer);
                        list.TextFieldNumericLabeled("Vehicle health multiplier (default: 3.0)", ref APCESettings.vehicleHealthMult, ref APCESettings.vehicleHealthMultBuffer);
                    }
                }

                else
                {
                    throw new Exception("The legendary FOURTH TAB of the settings window has been found.");
                }

                list.End();
            }
        }

        public void AdjustCESettings()
        {
            if (CESettings == null)
                return;

            string[] autopatcherFieldNames = {
            "enableApparelAutopatcher",
            "enableWeaponAutopatcher",
            "enableRaceAutopatcher",
            "enablePawnKindAutopatcher",
            "enableWeaponToughnessAutopatcher"
            };

            Type settingsType = typeof(Settings);

            foreach (string fieldName in autopatcherFieldNames)
            {
                FieldInfo field = settingsType.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
                if (field != null && field.FieldType == typeof(bool))
                {
                    field.SetValue(CESettings, false);
                }
            }
        }

        public override void WriteSettings()
        {
            //DEBUG
            base.WriteSettings();
            APCEController.RemoveListDuplicates(APCESettings.modsByPackageId);
            foreach (ModContentPack mod in APCESettings.modsToPatch)
            {
                if (APCESettings.modsAlreadyPatched.Add(mod))
                {
                    APCEController.GenerateDataHoldersForMod(mod);
                    foreach (Def def in mod.AllDefs)
                    {
                        if (APCESettings.defDataDict.TryGetValue(def, out var holder))
                        {
                            holder.Patch();
                        }
                    }
                }
            }
        }
    }
}
