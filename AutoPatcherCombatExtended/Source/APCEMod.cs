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

        internal static Dictionary<string, Type> defFolderTypesDictionary = new Dictionary<string, Type>
        {
            {"DefDtaHolderAmmoSet", typeof(DefDataHolderAmmoSet) },
            {"DefDataHolderApparel", typeof(DefDataHolderApparel) },
            {"DefDataHolderBuilding_TurretGun", typeof(DefDataHolderBuilding_TurretGun) },
            {"DefDataHolderGene", typeof(DefDataHolderGene) },
            {"DefDataHolderHediff", typeof(DefDataHolderHediff) },
            {"DefDataHolderMeleeWeapon", typeof(DefDataHolderMeleeWeapon) },
            {"DefDataHolderPawn", typeof(DefDataHolderPawn) },
            {"DefDataHolderPawnKind", typeof(DefDataHolderPawnKind) },
            {"DefDataHolderRangedWeapon", typeof(DefDataHolderRangedWeapon) }
            //TODO stuff once implemented
        };

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
                APCESettings.shouldRunController = false;
            }
            else
            {
                ModDataHolder auto = APCESettings.modDataDict.TryGetValue("nuff.ceautopatcher");

                if (auto == null)
                {
                    auto = APCESettings.modDataDict.TryGetValue("nuff.ceautopatcher_steam");
                    APCEController.CreateAPCEModDataHolder();
                }
                if (auto == null)
                {
                    APCEController.CreateAPCEModDataHolder();
                }

                APCESettings.shouldRunController = true;

                list.EnumSelector(ref APCESettings.settingsTabs, "", "", "select settings page");

                if (APCESettings.settingsTabs == APCEConstants.SettingsTabs.General_Settings)
                {
                    list.Gap();
                    list.Label("Weapon Stuff:");
                    //list.CheckboxLabeled("Patch weapons from selected mods: ", ref APCESettings.patchWeapons);
                    list.CheckboxLabeled("Limit Weapon Mass: ", ref auto.limitWeaponMass);
                    list.CheckboxLabeled("Try to patch custom verbs on guns (safety not guaranteed): ", ref auto.patchCustomVerbs);
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

                    list.Gap(20f);
                    if (list.ButtonText("Mod Ignore List"))
                    {
                        Find.WindowStack.Add(new Window_IgnoreList());
                    }
                }

                else if (APCESettings.settingsTabs == APCEConstants.SettingsTabs.Modlist)
                {
                    Rect listRect = new Rect(0, 0, inRect.width, inRect.height * 0.85f);
                    //list.ListControl(listRect, ref APCESettings.activeMods, ref APCESettings.modsToPatch, ref Settings.searchTerm, ref Settings.leftScrollPosition, ref Settings.rightScrollPosition,
                    //    ref Settings.leftSelectedObject, ref Settings.rightSelectedObject, "Mods to patch", rectPCT: 1f);
                    list.ListControlMods(listRect, ref APCESettings.activeMods, ref APCESettings.modsToPatch, ref Settings.searchTerm, ref Settings.leftScrollPosition, ref Settings.rightScrollPosition,
                        ref Settings.leftSelectedObject, ref Settings.rightSelectedObject, "Mods to patch", rectPCT: 1f);

                    //Rect customizeButtonRect = new Rect(inRect.xMax + 10f, inRect.yMax - 40f, 100f, 30f);
                    // Customize Mod button
                    if (Widgets.ButtonText(rect: inRect.BottomPart(0.15f).TopPart(0.5f).RightPart(0.3f).RightPart(0.5f), "Customize"))
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
                        string apparelSharpMultBuffer = auto.apparelSharpMult.ToString();
                        list.TextFieldNumericLabeled("Armor sharp base multiplier (default: 10)", ref auto.apparelSharpMult, ref apparelSharpMultBuffer);
                        string apparelBluntMultBuffer = auto.apparelBluntMult.ToString();
                        list.TextFieldNumericLabeled("Armor blunt base multiplier (default: 40)", ref auto.apparelBluntMult, ref apparelBluntMultBuffer);
                        list.Gap();

                        string apparelTechMultAnimalBuffer = auto.apparelTechMultAnimal.ToString();
                        list.TextFieldNumericLabeled("Tech level 'Animal' apparel multiplier (default: 0.25)", ref auto.apparelTechMultAnimal, ref apparelTechMultAnimalBuffer);
                        string apparelTechMultNeolithicBuffer = auto.apparelTechMultNeolithic.ToString();
                        list.TextFieldNumericLabeled("Tech level 'Neolithic' apparel multiplier (default: 0.5)", ref auto.apparelTechMultNeolithic, ref apparelTechMultNeolithicBuffer);
                        string apparelTechMultMedievalBuffer = auto.apparelTechMultMedieval.ToString();
                        list.TextFieldNumericLabeled("Tech level 'Medieval' apparel multiplier (default: 0.75)", ref auto.apparelTechMultMedieval, ref apparelTechMultMedievalBuffer);
                        string apparelTechMultIndustrialBuffer = auto.apparelTechMultIndustrial.ToString();
                        list.TextFieldNumericLabeled("Tech level 'Industrial' apparel multiplier (default: 1.0)", ref auto.apparelTechMultIndustrial, ref apparelTechMultIndustrialBuffer);
                        string apparelTechMultSpacerBuffer = auto.apparelTechMultSpacer.ToString();
                        list.TextFieldNumericLabeled("Tech level 'Spacer' apparel multiplier (default: 2.0)", ref auto.apparelTechMultSpacer, ref apparelTechMultSpacerBuffer);
                        string apparelTechMultUltratechBuffer = auto.apparelTechMultUltratech.ToString();
                        list.TextFieldNumericLabeled("Tech level 'Ultratech' apparel multiplier (default: 3.0)", ref auto.apparelTechMultUltratech, ref apparelTechMultUltratechBuffer);
                        string apparelTechMultArchotechBuffer = auto.apparelTechMultArchotech.ToString();
                        list.TextFieldNumericLabeled("Tech level 'Archotech' apparel multiplier (default: 4.0)", ref auto.apparelTechMultArchotech, ref apparelTechMultArchotechBuffer);
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
                            string gunSharpPenMultBuffer = auto.gunSharpPenMult.ToString();
                            list.TextFieldNumericLabeled("Ranged weapon sharp penetration multiplier (default: 10)", ref auto.gunSharpPenMult, ref gunSharpPenMultBuffer);

                            string gunBluntPenMultBuffer = auto.gunBluntPenMult.ToString();
                            list.TextFieldNumericLabeled("Ranged weapon blunt penetration multiplier (default: 40)", ref auto.gunBluntPenMult, ref gunBluntPenMultBuffer);

                            list.Gap();

                            string gunTechMultAnimalBuffer = auto.gunTechMultAnimal.ToString();
                            list.TextFieldNumericLabeled("Tech level 'Animal' ammo penetration multiplier (default: 0.5)", ref auto.gunTechMultAnimal, ref gunTechMultAnimalBuffer);

                            string gunTechMultNeolithicBuffer = auto.gunTechMultNeolithic.ToString();
                            list.TextFieldNumericLabeled("Tech level 'Neolithic' ammo penetration multiplier (default: 1.0)", ref auto.gunTechMultNeolithic, ref gunTechMultNeolithicBuffer);

                            string gunTechMultMedievalBuffer = auto.gunTechMultMedieval.ToString();
                            list.TextFieldNumericLabeled("Tech level 'Medieval' ammo penetration multiplier (default: 2.0)", ref auto.gunTechMultMedieval, ref gunTechMultMedievalBuffer);

                            string gunTechMultIndustrialBuffer = auto.gunTechMultIndustrial.ToString();
                            list.TextFieldNumericLabeled("Tech level 'Industrial' ammo penetration multiplier (default: 4.0)", ref auto.gunTechMultIndustrial, ref gunTechMultIndustrialBuffer);

                            string gunTechMultSpacerBuffer = auto.gunTechMultSpacer.ToString();
                            list.TextFieldNumericLabeled("Tech level 'Spacer' ammo penetration multiplier (default: 5.0)", ref auto.gunTechMultSpacer, ref gunTechMultSpacerBuffer);

                            string gunTechMultUltratechBuffer = auto.gunTechMultUltratech.ToString();
                            list.TextFieldNumericLabeled("Tech level 'Ultratech' ammo penetration multiplier (default: 6.0)", ref auto.gunTechMultUltratech, ref gunTechMultUltratechBuffer);

                            string gunTechMultArchotechBuffer = auto.gunTechMultArchotech.ToString();
                            list.TextFieldNumericLabeled("Tech level 'Archotech' ammo penetration multiplier (default: 8.0)", ref auto.gunTechMultArchotech, ref gunTechMultArchotechBuffer);

                            list.Gap();

                            string maximumWeaponMassBuffer = auto.maximumWeaponMass.ToString();
                            list.TextFieldNumericLabeled("Maximum weapon mass (default 20.0)", ref auto.maximumWeaponMass, ref maximumWeaponMassBuffer);
                        }
                        if (APCESettings.balanceWeaponTabs == APCEConstants.BalanceWeaponTabs.Melee)
                        {
                            list.Gap();
                            Text.Font = GameFont.Medium;
                            list.Label("Weapon tool (melee attacks) settings");
                            Text.Font = GameFont.Small;
                            string weaponToolPowerMultBuffer = auto.weaponToolPowerMult.ToString();
                            list.TextFieldNumericLabeled("Weapon tool power (damage) multiplier (default: 1)", ref auto.weaponToolPowerMult, ref weaponToolPowerMultBuffer);

                            string weaponToolSharpPenetrationBuffer = auto.weaponToolSharpPenetration.ToString();
                            list.TextFieldNumericLabeled("Weapon tool sharp penetration multiplier (default: 1)", ref auto.weaponToolSharpPenetration, ref weaponToolSharpPenetrationBuffer);

                            string weaponToolBluntPenetrationBuffer = auto.weaponToolBluntPenetration.ToString();
                            list.TextFieldNumericLabeled("Weapon tool blunt penetration multiplier (default: 4)", ref auto.weaponToolBluntPenetration, ref weaponToolBluntPenetrationBuffer);

                            list.Gap();

                            string weaponToolTechMultAnimalBuffer = auto.weaponToolTechMultAnimal.ToString();
                            list.TextFieldNumericLabeled("Tech level 'Animal' tool penetration multiplier (default: 1.0)", ref auto.weaponToolTechMultAnimal, ref weaponToolTechMultAnimalBuffer);

                            string weaponToolTechMultNeolithicBuffer = auto.weaponToolTechMultNeolithic.ToString();
                            list.TextFieldNumericLabeled("Tech level 'Neolithic' tool penetration multiplier (default: 1.0)", ref auto.weaponToolTechMultNeolithic, ref weaponToolTechMultNeolithicBuffer);

                            string weaponToolTechMultMedievalBuffer = auto.weaponToolTechMultMedieval.ToString();
                            list.TextFieldNumericLabeled("Tech level 'Medieval' tool penetration multiplier (default: 1.0)", ref auto.weaponToolTechMultMedieval, ref weaponToolTechMultMedievalBuffer);

                            string weaponToolTechMultIndustrialBuffer = auto.weaponToolTechMultIndustrial.ToString();
                            list.TextFieldNumericLabeled("Tech level 'Industrial' tool penetration multiplier (default: 2.0)", ref auto.weaponToolTechMultIndustrial, ref weaponToolTechMultIndustrialBuffer);

                            string weaponToolTechMultSpacerBuffer = auto.weaponToolTechMultSpacer.ToString();
                            list.TextFieldNumericLabeled("Tech level 'Spacer' tool penetration multiplier (default: 3.0)", ref auto.weaponToolTechMultSpacer, ref weaponToolTechMultSpacerBuffer);

                            string weaponToolTechMultUltratechBuffer = auto.weaponToolTechMultUltratech.ToString();
                            list.TextFieldNumericLabeled("Tech level 'Ultratech' tool penetration multiplier (default: 4.0)", ref auto.weaponToolTechMultUltratech, ref weaponToolTechMultUltratechBuffer);

                            string weaponToolTechMultArchotechBuffer = auto.weaponToolTechMultArchotech.ToString();
                            list.TextFieldNumericLabeled("Tech level 'Archotech' tool penetration multiplier (default: 6.0)", ref auto.weaponToolTechMultArchotech, ref weaponToolTechMultArchotechBuffer);
                        }
                    }
                    if (APCESettings.balanceTabs == APCEConstants.BalanceTabs.Pawns)
                    {
                        list.Gap();
                        Text.Font = GameFont.Medium;
                        list.Label("Pawn armor settings");
                        Text.Font = GameFont.Small;

                        string pawnArmorSharpMultBuffer = auto.pawnArmorSharpMult.ToString();
                        list.TextFieldNumericLabeled("Pawn sharp armor multiplier (default: 10)", ref auto.pawnArmorSharpMult, ref pawnArmorSharpMultBuffer);

                        string pawnArmorBluntMultBuffer = auto.pawnArmorBluntMult.ToString();
                        list.TextFieldNumericLabeled("Pawn blunt armor multiplier (default: 40)", ref auto.pawnArmorBluntMult, ref pawnArmorBluntMultBuffer);

                        list.Gap();
                        Text.Font = GameFont.Medium;
                        list.Label("Pawn tool (melee attack) settings");
                        Text.Font = GameFont.Small;

                        string pawnToolPowerMultBuffer = auto.pawnToolPowerMult.ToString();
                        list.TextFieldNumericLabeled("Pawn tool power (damage) multiplier (default: 1)", ref auto.pawnToolPowerMult, ref pawnToolPowerMultBuffer);

                        string pawnToolSharpPenetrationBuffer = auto.pawnToolSharpPenetration.ToString();
                        list.TextFieldNumericLabeled("Pawn tool sharp penetration multiplier (default: 10)", ref auto.pawnToolSharpPenetration, ref pawnToolSharpPenetrationBuffer);

                        string pawnToolBluntPenetrationBuffer = auto.pawnToolBluntPenetration.ToString();
                        list.TextFieldNumericLabeled("Pawn tool blunt penetration multiplier (default: 40)", ref auto.pawnToolBluntPenetration, ref pawnToolBluntPenetrationBuffer);

                        list.Gap();
                        Text.Font = GameFont.Medium;
                        list.Label("Pawn Kind settings");
                        Text.Font = GameFont.Small;

                        list.CheckboxLabeled("Patch PawnKinds with weapons to be allowed backpacks: ", ref auto.patchBackpacks);

                        string pawnKindMinMagsBuffer = auto.pawnKindMinMags.ToString();
                        list.TextFieldNumericLabeled("Pawn kind minimum ammo magazines when spawned (default: 2)", ref auto.pawnKindMinMags, ref pawnKindMinMagsBuffer);

                        string pawnKindMaxMagsBuffer = auto.pawnKindMaxMags.ToString();
                        list.TextFieldNumericLabeled("Pawn kind maximum ammo magazines when spawned (default: 5)", ref auto.pawnKindMaxMags, ref pawnKindMaxMagsBuffer);

                        list.Gap();
                        Text.Font = GameFont.Medium;
                        list.Label("Hediff settings");
                        Text.Font = GameFont.Small;

                        string hediffSharpMultBuffer = auto.hediffSharpMult.ToString();
                        list.TextFieldNumericLabeled("Hediff sharp armor multiplier (default: 10)", ref auto.hediffSharpMult, ref hediffSharpMultBuffer);

                        string hediffBluntMultBuffer = auto.hediffBluntMult.ToString();
                        list.TextFieldNumericLabeled("Hediff blunt armor multiplier (default: 40)", ref auto.hediffBluntMult, ref hediffBluntMultBuffer);

                        list.Gap();
                        Text.Font = GameFont.Medium;
                        list.Label("Gene settings");
                        Text.Font = GameFont.Small;

                        string geneArmorSharpMultBuffer = auto.geneArmorSharpMult.ToString();
                        list.TextFieldNumericLabeled("Gene sharp armor multiplier (default: 10)", ref auto.geneArmorSharpMult, ref geneArmorSharpMultBuffer);

                        string geneArmorBluntMultBuffer = auto.geneArmorBluntMult.ToString();
                        list.TextFieldNumericLabeled("Gene blunt armor multiplier (default: 10)", ref auto.geneArmorBluntMult, ref geneArmorBluntMultBuffer);
                    }

                    if (APCESettings.balanceTabs == APCEConstants.BalanceTabs.Other)
                    {
                        list.Gap();
                        Text.Font = GameFont.Medium;
                        list.Label("Vehicle settings");
                        Text.Font = GameFont.Small;

                        list.Gap();

                        string vehicleSharpMultBuffer = auto.vehicleSharpMult.ToString();
                        list.TextFieldNumericLabeled("Vehicle sharp armor multiplier (default: 15)", ref auto.vehicleSharpMult, ref vehicleSharpMultBuffer);

                        string vehicleBluntMultBuffer = auto.vehicleBluntMult.ToString();
                        list.TextFieldNumericLabeled("Vehicle blunt armor multiplier (default: 15)", ref auto.vehicleBluntMult, ref vehicleBluntMultBuffer);

                        string vehicleHealthMultBuffer = auto.vehicleHealthMult.ToString();
                        list.TextFieldNumericLabeled("Vehicle health multiplier (default: 3.0)", ref auto.vehicleHealthMult, ref vehicleHealthMultBuffer);
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
            APCEController.RemoveListDuplicates(APCESettings.modsByPackageId);
            if (APCESettings.shouldRunController)
            {
                APCEController.APCEPatchController();
            }
            base.WriteSettings();
        }

        public static bool RegisterDefTypeFolder(string folderName, Type defType)
        {
            defFolderTypesDictionary.TryAdd(folderName, defType);
            if (defFolderTypesDictionary.TryGetValue(folderName, out Type value) && value == defType)
            {
                return true;
            }
            return false;
        }
    }
}
