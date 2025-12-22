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
        APCEConstants.ModSettingsTabs categoryTab = APCEConstants.ModSettingsTabs.General_Settings;
        APCEConstants.BalanceTabs balanceTab = APCEConstants.BalanceTabs.Apparel;
        APCEConstants.BalanceWeaponTabs weaponTab = APCEConstants.BalanceWeaponTabs.Melee;

        string searchTerm = "";
        internal Vector2 leftScrollPosition = new Vector2();
        internal Vector2 rightScrollPosition = new Vector2();
        internal Def leftSelectedObject = null;
        internal Def rightSelectedObject = null;

        internal ModContentPack leftSelectedObject2 = null;
        internal ModContentPack rightSelectedObject2 = null;

        public Window_CustomizeMod(ModContentPack mod)
        {
            this.mod = mod;
            modData = APCESettings.modDataDict.TryGetValue(mod.PackageId);
            if (modData == null)
            {
                modData = new ModDataHolder(mod);
            }
            modData.isCustomized = true;
            doCloseButton = true;
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
            Text.Font = GameFont.Small;

            list.Gap(45);
            list.EnumSelector(ref categoryTab, "", "", "select settings page");

            if (categoryTab == APCEConstants.ModSettingsTabs.General_Settings)
            {
                list.Gap();
                list.Label("Weapon Stuff:");
                list.CheckboxLabeled("Limit Weapon Mass: ", ref modData.limitWeaponMass);
                list.CheckboxLabeled("Try to patch custom verbs on guns (safety not guaranteed): ", ref modData.patchCustomVerbs);
                list.Gap();
                list.Label("Apparel Stuff:");
                //list.CheckboxLabeled("Patch apparels from selected mods: ", ref APCESettings.patchApparels);
                //list.CheckboxLabeled("Patch headgear layers: ", ref APCESettings.patchHeadgearLayers);
                list.Gap();
                list.Label("Other Stuff:");
            }
            else if (categoryTab == APCEConstants.ModSettingsTabs.Balance_Control)
            {
                list.Label("Balance Control placeholder");
                list.EnumSelector(ref balanceTab, "", "", "select balance category");
                if (balanceTab == APCEConstants.BalanceTabs.Apparel)
                {
                    list.Gap();
                    Text.Font = GameFont.Medium;
                    list.Label("Armor value settings");
                    Text.Font = GameFont.Small;

                    list.Gap();
                    list.Label("These multipliers will apply to all apparels, and are then further multiplied by techlevel");
                    string apparelSharpMultBuffer = modData.apparelSharpMult.ToString();
                    list.TextFieldNumericLabeled("Armor sharp base multiplier (default: 10)", ref modData.apparelSharpMult, ref apparelSharpMultBuffer);
                    string apparelBluntMultBuffer = modData.apparelBluntMult.ToString();
                    list.TextFieldNumericLabeled("Armor blunt base multiplier (default: 40)", ref modData.apparelBluntMult, ref apparelBluntMultBuffer);
                    list.Gap();

                    string apparelTechMultAnimalBuffer = modData.apparelTechMultAnimal.ToString();
                    list.TextFieldNumericLabeled("Tech level 'Animal' apparel multiplier (default: 0.25)", ref modData.apparelTechMultAnimal, ref apparelTechMultAnimalBuffer);
                    string apparelTechMultNeolithicBuffer = modData.apparelTechMultNeolithic.ToString();
                    list.TextFieldNumericLabeled("Tech level 'Neolithic' apparel multiplier (default: 0.5)", ref modData.apparelTechMultNeolithic, ref apparelTechMultNeolithicBuffer);
                    string apparelTechMultMedievalBuffer = modData.apparelTechMultMedieval.ToString();
                    list.TextFieldNumericLabeled("Tech level 'Medieval' apparel multiplier (default: 0.75)", ref modData.apparelTechMultMedieval, ref apparelTechMultMedievalBuffer);
                    string apparelTechMultIndustrialBuffer = modData.apparelTechMultIndustrial.ToString();
                    list.TextFieldNumericLabeled("Tech level 'Industrial' apparel multiplier (default: 1.0)", ref modData.apparelTechMultIndustrial, ref apparelTechMultIndustrialBuffer);
                    string apparelTechMultSpacerBuffer = modData.apparelTechMultSpacer.ToString();
                    list.TextFieldNumericLabeled("Tech level 'Spacer' apparel multiplier (default: 2.0)", ref modData.apparelTechMultSpacer, ref apparelTechMultSpacerBuffer);
                    string apparelTechMultUltratechBuffer = modData.apparelTechMultUltratech.ToString();
                    list.TextFieldNumericLabeled("Tech level 'Ultratech' apparel multiplier (default: 3.0)", ref modData.apparelTechMultUltratech, ref apparelTechMultUltratechBuffer);
                    string apparelTechMultArchotechBuffer = modData.apparelTechMultArchotech.ToString();
                    list.TextFieldNumericLabeled("Tech level 'Archotech' apparel multiplier (default: 4.0)", ref modData.apparelTechMultArchotech, ref apparelTechMultArchotechBuffer);
                }
                if (balanceTab == APCEConstants.BalanceTabs.Weapons)
                {
                    list.EnumSelector(ref weaponTab, "", "", "select weapon category");
                    if (weaponTab == APCEConstants.BalanceWeaponTabs.Ranged)
                    {
                        list.Gap();
                        Text.Font = GameFont.Medium;
                        list.Label("Ranged weapon armor penetration settings");
                        Text.Font = GameFont.Small;
                        string gunSharpPenMultBuffer = modData.gunSharpPenMult.ToString();
                        list.TextFieldNumericLabeled("Ranged weapon sharp penetration multiplier (default: 10)", ref modData.gunSharpPenMult, ref gunSharpPenMultBuffer);

                        string gunBluntPenMultBuffer = modData.gunBluntPenMult.ToString();
                        list.TextFieldNumericLabeled("Ranged weapon blunt penetration multiplier (default: 40)", ref modData.gunBluntPenMult, ref gunBluntPenMultBuffer);

                        list.Gap();

                        string gunTechMultAnimalBuffer = modData.gunTechMultAnimal.ToString();
                        list.TextFieldNumericLabeled("Tech level 'Animal' ammo penetration multiplier (default: 0.5)", ref modData.gunTechMultAnimal, ref gunTechMultAnimalBuffer);

                        string gunTechMultNeolithicBuffer = modData.gunTechMultNeolithic.ToString();
                        list.TextFieldNumericLabeled("Tech level 'Neolithic' ammo penetration multiplier (default: 1.0)", ref modData.gunTechMultNeolithic, ref gunTechMultNeolithicBuffer);

                        string gunTechMultMedievalBuffer = modData.gunTechMultMedieval.ToString();
                        list.TextFieldNumericLabeled("Tech level 'Medieval' ammo penetration multiplier (default: 2.0)", ref modData.gunTechMultMedieval, ref gunTechMultMedievalBuffer);

                        string gunTechMultIndustrialBuffer = modData.gunTechMultIndustrial.ToString();
                        list.TextFieldNumericLabeled("Tech level 'Industrial' ammo penetration multiplier (default: 4.0)", ref modData.gunTechMultIndustrial, ref gunTechMultIndustrialBuffer);

                        string gunTechMultSpacerBuffer = modData.gunTechMultSpacer.ToString();
                        list.TextFieldNumericLabeled("Tech level 'Spacer' ammo penetration multiplier (default: 5.0)", ref modData.gunTechMultSpacer, ref gunTechMultSpacerBuffer);

                        string gunTechMultUltratechBuffer = modData.gunTechMultUltratech.ToString();
                        list.TextFieldNumericLabeled("Tech level 'Ultratech' ammo penetration multiplier (default: 6.0)", ref modData.gunTechMultUltratech, ref gunTechMultUltratechBuffer);

                        string gunTechMultArchotechBuffer = modData.gunTechMultArchotech.ToString();
                        list.TextFieldNumericLabeled("Tech level 'Archotech' ammo penetration multiplier (default: 8.0)", ref modData.gunTechMultArchotech, ref gunTechMultArchotechBuffer);

                        list.Gap();

                        list.CheckboxLabeled("Ranged Weapons Use Ammo", ref modData.gunsUseAmmo);

                        string maximumWeaponMassBuffer = modData.maximumWeaponMass.ToString();
                        list.TextFieldNumericLabeled("Maximum weapon mass (default 20.0)", ref modData.maximumWeaponMass, ref maximumWeaponMassBuffer);
                    }
                    if (weaponTab == APCEConstants.BalanceWeaponTabs.Melee)
                    {
                        list.Gap();
                        Text.Font = GameFont.Medium;
                        list.Label("Weapon tool (melee attacks) settings");
                        Text.Font = GameFont.Small;
                        string weaponToolPowerMultBuffer = modData.weaponToolPowerMult.ToString();
                        list.TextFieldNumericLabeled("Weapon tool power (damage) multiplier (default: 1)", ref modData.weaponToolPowerMult, ref weaponToolPowerMultBuffer);

                        string weaponToolSharpPenetrationBuffer = modData.weaponToolSharpPenetration.ToString();
                        list.TextFieldNumericLabeled("Weapon tool sharp penetration multiplier (default: 1)", ref modData.weaponToolSharpPenetration, ref weaponToolSharpPenetrationBuffer);

                        string weaponToolBluntPenetrationBuffer = modData.weaponToolBluntPenetration.ToString();
                        list.TextFieldNumericLabeled("Weapon tool blunt penetration multiplier (default: 4)", ref modData.weaponToolBluntPenetration, ref weaponToolBluntPenetrationBuffer);

                        list.Gap();

                        string weaponToolTechMultAnimalBuffer = modData.weaponToolTechMultAnimal.ToString();
                        list.TextFieldNumericLabeled("Tech level 'Animal' tool penetration multiplier (default: 1.0)", ref modData.weaponToolTechMultAnimal, ref weaponToolTechMultAnimalBuffer);

                        string weaponToolTechMultNeolithicBuffer = modData.weaponToolTechMultNeolithic.ToString();
                        list.TextFieldNumericLabeled("Tech level 'Neolithic' tool penetration multiplier (default: 1.0)", ref modData.weaponToolTechMultNeolithic, ref weaponToolTechMultNeolithicBuffer);

                        string weaponToolTechMultMedievalBuffer = modData.weaponToolTechMultMedieval.ToString();
                        list.TextFieldNumericLabeled("Tech level 'Medieval' tool penetration multiplier (default: 1.0)", ref modData.weaponToolTechMultMedieval, ref weaponToolTechMultMedievalBuffer);

                        string weaponToolTechMultIndustrialBuffer = modData.weaponToolTechMultIndustrial.ToString();
                        list.TextFieldNumericLabeled("Tech level 'Industrial' tool penetration multiplier (default: 2.0)", ref modData.weaponToolTechMultIndustrial, ref weaponToolTechMultIndustrialBuffer);

                        string weaponToolTechMultSpacerBuffer = modData.weaponToolTechMultSpacer.ToString();
                        list.TextFieldNumericLabeled("Tech level 'Spacer' tool penetration multiplier (default: 3.0)", ref modData.weaponToolTechMultSpacer, ref weaponToolTechMultSpacerBuffer);

                        string weaponToolTechMultUltratechBuffer = modData.weaponToolTechMultUltratech.ToString();
                        list.TextFieldNumericLabeled("Tech level 'Ultratech' tool penetration multiplier (default: 4.0)", ref modData.weaponToolTechMultUltratech, ref weaponToolTechMultUltratechBuffer);

                        string weaponToolTechMultArchotechBuffer = modData.weaponToolTechMultArchotech.ToString();
                        list.TextFieldNumericLabeled("Tech level 'Archotech' tool penetration multiplier (default: 6.0)", ref modData.weaponToolTechMultArchotech, ref weaponToolTechMultArchotechBuffer);
                    }
                }
                if (balanceTab == APCEConstants.BalanceTabs.Pawns)
                {
                    list.Gap();
                    Text.Font = GameFont.Medium;
                    list.Label("Pawn armor settings");
                    Text.Font = GameFont.Small;

                    string pawnArmorSharpMultBuffer = modData.pawnArmorSharpMult.ToString();
                    list.TextFieldNumericLabeled("Pawn sharp armor multiplier (default: 10)", ref modData.pawnArmorSharpMult, ref pawnArmorSharpMultBuffer);

                    string pawnArmorBluntMultBuffer = modData.pawnArmorBluntMult.ToString();
                    list.TextFieldNumericLabeled("Pawn blunt armor multiplier (default: 40)", ref modData.pawnArmorBluntMult, ref pawnArmorBluntMultBuffer);

                    list.Gap();
                    Text.Font = GameFont.Medium;
                    list.Label("Pawn tool (melee attack) settings");
                    Text.Font = GameFont.Small;

                    string pawnToolPowerMultBuffer = modData.pawnToolPowerMult.ToString();
                    list.TextFieldNumericLabeled("Pawn tool power (damage) multiplier (default: 1)", ref modData.pawnToolPowerMult, ref pawnToolPowerMultBuffer);

                    string pawnToolSharpPenetrationBuffer = modData.pawnToolSharpPenetration.ToString();
                    list.TextFieldNumericLabeled("Pawn tool sharp penetration multiplier (default: 10)", ref modData.pawnToolSharpPenetration, ref pawnToolSharpPenetrationBuffer);

                    string pawnToolBluntPenetrationBuffer = modData.pawnToolBluntPenetration.ToString();
                    list.TextFieldNumericLabeled("Pawn tool blunt penetration multiplier (default: 40)", ref modData.pawnToolBluntPenetration, ref pawnToolBluntPenetrationBuffer);

                    list.Gap();
                    Text.Font = GameFont.Medium;
                    list.Label("Pawn Kind settings");
                    Text.Font = GameFont.Small;

                    list.CheckboxLabeled("Patch PawnKinds with weapons to be allowed backpacks: ", ref modData.patchBackpacks);

                    string pawnKindMinMagsBuffer = modData.pawnKindMinMags.ToString();
                    list.TextFieldNumericLabeled("Pawn kind minimum ammo magazines when spawned (default: 2)", ref modData.pawnKindMinMags, ref pawnKindMinMagsBuffer);

                    string pawnKindMaxMagsBuffer = modData.pawnKindMaxMags.ToString();
                    list.TextFieldNumericLabeled("Pawn kind maximum ammo magazines when spawned (default: 5)", ref modData.pawnKindMaxMags, ref pawnKindMaxMagsBuffer);

                    list.Gap();
                    Text.Font = GameFont.Medium;
                    list.Label("Hediff settings");
                    Text.Font = GameFont.Small;

                    string hediffSharpMultBuffer = modData.hediffSharpMult.ToString();
                    list.TextFieldNumericLabeled("Hediff sharp armor multiplier (default: 10)", ref modData.hediffSharpMult, ref hediffSharpMultBuffer);

                    string hediffBluntMultBuffer = modData.hediffBluntMult.ToString();
                    list.TextFieldNumericLabeled("Hediff blunt armor multiplier (default: 40)", ref modData.hediffBluntMult, ref hediffBluntMultBuffer);

                    list.Gap();
                    Text.Font = GameFont.Medium;
                    list.Label("Gene settings");
                    Text.Font = GameFont.Small;

                    string geneArmorSharpMultBuffer = modData.geneArmorSharpMult.ToString();
                    list.TextFieldNumericLabeled("Gene sharp armor multiplier (default: 10)", ref modData.geneArmorSharpMult, ref geneArmorSharpMultBuffer);

                    string geneArmorBluntMultBuffer = modData.geneArmorBluntMult.ToString();
                    list.TextFieldNumericLabeled("Gene blunt armor multiplier (default: 10)", ref modData.geneArmorBluntMult, ref geneArmorBluntMultBuffer);
                }

                if (balanceTab == APCEConstants.BalanceTabs.Other)
                {
                    list.Gap();
                    Text.Font = GameFont.Medium;
                    list.Label("Vehicle settings");
                    Text.Font = GameFont.Small;

                    list.Gap();

                    string vehicleSharpMultBuffer = modData.vehicleSharpMult.ToString();
                    list.TextFieldNumericLabeled("Vehicle sharp armor multiplier (default: 15)", ref modData.vehicleSharpMult, ref vehicleSharpMultBuffer);

                    string vehicleBluntMultBuffer = modData.vehicleBluntMult.ToString();
                    list.TextFieldNumericLabeled("Vehicle blunt armor multiplier (default: 15)", ref modData.vehicleBluntMult, ref vehicleBluntMultBuffer);

                    string vehicleHealthMultBuffer = modData.vehicleHealthMult.ToString();
                    list.TextFieldNumericLabeled("Vehicle health multiplier (default: 3.0)", ref modData.vehicleHealthMult, ref vehicleHealthMultBuffer);
                }
            }
            else if (categoryTab == APCEConstants.ModSettingsTabs.Deflist)
            {
                Rect listRect = new Rect(0, 0, inRect.width, inRect.height * 0.75f);
                //list.ListControlMods(listRect, ref APCESettings.activeMods, ref APCESettings.modsToPatch, ref searchTerm, ref leftScrollPosition, ref rightScrollPosition,
                //        ref leftSelectedObject2, ref rightSelectedObject2, "Mods to patch", rectPCT: 1f);
                list.ListControlDefs(listRect, ref searchTerm, ref leftScrollPosition, ref rightScrollPosition,
                    ref leftSelectedObject, ref rightSelectedObject, "Defs to patch", rectPCT: 1f, modData);

                //Rect customizeButtonRect = new Rect(inRect.xMax + 10f, inRect.yMax - 40f, 100f, 30f);
                // Customize Mod button
                if (Widgets.ButtonText(rect: inRect.BottomPart(0.15f).BottomPart(0.5f).RightPart(0.3f).RightPart(0.5f), "Customize Def"))
                {
                    if (rightSelectedObject != null)
                    {
                        TryOpenDefWindow(rightSelectedObject, modData);
                    }
                }
            }

            if (Widgets.ButtonText(rect: inRect.BottomPart(0.15f).BottomPart(0.5f).LeftPart(0.3f).LeftPart(0.5f), "Export Patches"))
            {
                APCEPatchExport.ExportPatchesForMod(modData);
            }


            list.End();
        }

        public void TryOpenDefWindow(Def def, ModDataHolder modData)
        {
            DefDataHolder defDataHolder = TryGetDataHolder();
            if (defDataHolder == null)
            {
                Window_DefFailure failureWindow = new Window_DefFailure(def);
                Find.WindowStack.Add(failureWindow);
            }
            else
            {
                Window newWindow;
                if (defDataHolder is DefDataHolderAmmoSet)
                {
                    newWindow = new Window_CustomizeAmmoSet(defDataHolder);
                }
                else if (defDataHolder is DefDataHolderApparel)
                {
                    newWindow = new Window_CustomizeDefApparel(defDataHolder);
                }
                else if (defDataHolder is DefDataHolderBuilding_TurretGun)
                {
                    newWindow = new Window_CustomizeDefBuilding_TurretGun(defDataHolder);
                }
                else if (defDataHolder is DefDataHolderGene)
                {
                    newWindow = new Window_CustomizeDefGene(defDataHolder);
                }
                else if (defDataHolder is DefDataHolderHediff)
                {
                    newWindow = new Window_CustomizeDefHediff(defDataHolder);
                }
                else if (defDataHolder is DefDataHolderMeleeWeapon)
                {
                    newWindow = new Window_CustomizeDefMeleeWeapon(defDataHolder);
                }
                else if (defDataHolder is DefDataHolderMortarShell)
                {
                    newWindow = new Window_CustomizeMortarShell(defDataHolder);
                }
                else if (defDataHolder is DefDataHolderPawn)
                {
                    newWindow = new Window_CustomizeDefPawn(defDataHolder);
                }
                else if (defDataHolder is DefDataHolderPawnKind)
                {
                    newWindow = new Window_CustomizeDefPawnKind(defDataHolder);
                }
                else if (defDataHolder is DefDataHolderRangedWeapon)
                {
                    newWindow = new Window_CustomizeDefRangedWeapon(defDataHolder);
                }
                else if (APCESettings.defCustomizationWindowDictionary.TryGetValue(defDataHolder.GetType(), out Type windowType))
                {
                    newWindow = (Window)Activator.CreateInstance(windowType, defDataHolder);
                }
                else
                {
                    newWindow = new Window_DefFailure(def);
                }

                Find.WindowStack.Add(newWindow);
            }

            DefDataHolder TryGetDataHolder()
            {
            if (modData.defDict.TryGetValue(def, out DefDataHolder dataHolder))
            {
                return dataHolder;
            }
            else if (APCEController.TryGenerateDataHolderForDef(def))
            {
                modData.defDict.TryGetValue(def, out DefDataHolder ddh2);
                    ddh2.AutoCalculate();
                return ddh2;
            }
            else
            {
                //handle failure in the caller
                return default;
            }
        }
    }

        public override void PreOpen()
        {
            base.PreOpen();
            modData = APCESettings.modDataDict.TryGetValue(mod.PackageId);
        }

        public override void PreClose()
        {
            APCESaveLoad.SaveDataHolders();
            base.PreClose();
        }
    }
}
