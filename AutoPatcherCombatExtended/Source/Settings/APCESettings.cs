using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace nuff.AutoPatcherCombatExtended
{
    public class APCESettings : ModSettings
    {
        public static APCEConstants.SettingsTabs settingsTabs = APCEConstants.SettingsTabs.General_Settings;
        public static APCEConstants.BalanceTabs balanceTabs = APCEConstants.BalanceTabs.Apparel;
        public static APCEConstants.BalanceWeaponTabs balanceWeaponTabs = APCEConstants.BalanceWeaponTabs.Ranged;

        public static APCEConstants.ModSettingsTabs modSettingsTabs = APCEConstants.ModSettingsTabs.General_Settings;

        //Modlist Settings
        public static List<ModContentPack> activeMods = new List<ModContentPack>(); //will not be saved. will be gotten at startup
        public static List<ModContentPack> modsToPatch = new List<ModContentPack>(); //will also not be saved, but instead a saved list of PackageIDs will be used to rebuild this list at startup
        public static List<ModContentPack> modsToRecommend = new List<ModContentPack>();
        public static Dictionary<ModContentPack, bool> modsToRecommendDict = new Dictionary<ModContentPack, bool>();
        public static List<string> modsByPackageId = new List<string>(); //this is the list that will be used to rebuild the modsToPatch list on startup
        public static ModContentPack thisMod;
        public static HashSet<ModContentPack> modsAlreadyPatched = new HashSet<ModContentPack>(); //set of patched mods, to keep track so added mods can be patched when closing the settings window
        public static bool suggestionWindowOpened = false;

        public string searchTerm = "";
        public Vector2 leftScrollPosition = new Vector2();
        public Vector2 rightScrollPosition = new Vector2();
        public ModContentPack leftSelectedObject = null;
        public ModContentPack rightSelectedObject = null;

        public static Dictionary<string, ModDataHolder> modDataDict = new Dictionary<string, ModDataHolder>(); //ModDataHolders stored here. Not saved, instead ModDataHolders will register themselves as they are loaded
        public static Dictionary<string, DefDataHolder> defDataDict = new Dictionary<string, DefDataHolder>(); //DefDataHolders stored here. Not saved, instead DefDataHolders will register themselves as they are loaded
        public static Dictionary<string, ThingCategoryDef> modAmmoThingCategoryDict = new Dictionary<string, ThingCategoryDef>(); //not saved, populated if/when ammos are made for that mod

        //General Settings
        public static bool patchWeapons = true;
        public static bool patchCustomVerbs = false;
        public static bool limitWeaponMass = false;
        public static bool patchApparels = true;
        public static bool patchPawns = true;
        public static bool patchPawnKinds = true;
        public static bool patchGenes = true;
        public static bool patchHediffs = true;
        public static bool patchHeadgearLayers = true;
        public static bool patchVehicles = true;
        public static bool printLogs = false;
        public static bool printPatchErrors = false;

        //Balance Control Settings

        //armor settings
        public static float apparelSharpMult = 10;    public static string apparelSharpMultBuffer = "10";
        public static float apparelBluntMult = 40;    public static string apparelBluntMultBuffer = "40";
        public static float armorTechMultAnimal = 0.25f;  public static string armorTechMultAnimalBuffer = "0.25";
        public static float armorTechMultNeolithic = 0.5f;    public static string armorTechMultNeolithicBuffer = "0.5";
        public static float armorTechMultMedieval = 0.75f;    public static string armorTechMultMedievalBuffer = "0.75";
        public static float armorTechMultIndustrial = 1;  public static string armorTechMultIndustrialBuffer = "1";
        public static float armorTechMultSpacer = 2;  public static string armorTechMultSpacerBuffer = "2";
        public static float armorTechMultUltratech = 3;   public static string armorTechMultUltratechBuffer = "3";
        public static float armorTechMultArchotech = 4;   public static string armorTechMultArchotechBuffer = "4";

        public static float skinBulkAdd = 1f;
        public static float skinWulkAdd = 0.5f;
        public static float slinBulkMult = 1f;
        public static float skinWulkMult = 1f;

        public static float midBulkAdd = 5f;
        public static float midWulkAdd = 3f;
        public static float midBulkMult = 1f;
        public static float midWulkMult = 1f;

        public static float shellBulkAdd = 7.5f;
        public static float shellWulkAdd = 2.5f;
        public static float shellBulkMult = 20f;
        public static float shellWulkMult = 5f;

        public static float advancedArmorCarryWeight = 80f;
        public static float advancedArmorCarryBulk = 10f;
        public static float advancedArmorShootingAccuracy = 0.2f; 

        //weapon settings
        public static float gunSharpPenMult = 10f; public static string gunSharpPenMultBuffer = "10";
        public static float gunBluntPenMult = 40f; public static string gunBluntPenMultBuffer = "40";
        public static float gunTechMultAnimal = 0.5f; public static string gunTechMultAnimalBuffer = "0.5";
        public static float gunTechMultNeolithic = 1f; public static string gunTechMultNeolithicBuffer = "1.0";
        public static float gunTechMultMedieval = 2f; public static string gunTechMultMedievalBuffer = "2.0";
        public static float gunTechMultIndustrial = 4; public static string gunTechMultIndustrialBuffer = "4.0";
        public static float gunTechMultSpacer = 5; public static string gunTechMultSpacerBuffer = "5.0";
        public static float gunTechMultUltratech = 6; public static string gunTechMultUltratechBuffer = "6.0";
        public static float gunTechMultArchotech = 8; public static string gunTechMultArchotechBuffer = "8.0";

        public static float weaponToolPowerMult = 1f; public static string weaponToolPowerMultBuffer = "1.0";
        public static float weaponToolSharpPenetration = 1f; public static string weaponToolSharpPenetrationBuffer = "1.0";
        public static float weaponToolBluntPenetration = 4f; public static string weaponToolBluntPenetrationBuffer = "4.0";
        public static float weaponToolTechMultAnimal = 1f; public static string weaponToolTechMultAnimalBuffer = "1.0";
        public static float weaponToolTechMultNeolithic = 1f; public static string weaponToolTechMultNeolithicBuffer = "1.0";
        public static float weaponToolTechMultMedieval = 1f; public static string weaponToolTechMultMedievalBuffer = "1.0";
        public static float weaponToolTechMultIndustrial = 2f; public static string weaponToolTechMultIndustrialBuffer = "2.0";
        public static float weaponToolTechMultSpacer = 3f; public static string weaponToolTechMultSpacerBuffer = "3.0";
        public static float weaponToolTechMultUltratech = 4f; public static string weaponToolTechMultUltratechBuffer = "4.0";
        public static float weaponToolTechMultArchotech = 6f; public static string weaponToolTechMultArchotechBuffer = "6.0";

        public static float maximumWeaponMass = 20f; public static string maximumWeaponMassBuffer = "20.0";


        //pawn settings
        public static float pawnArmorSharpMult = 10; public static string pawnArmorSharpMultBuffer = "10";
        public static float pawnArmorBluntMult = 40; public static string pawnArmorBluntMultBuffer = "40";

        public static float pawnToolPowerMult = 1f; public static string pawnToolPowerMultBuffer = "1";
        public static float pawnToolSharpPenetration = 10f; public static string pawnToolSharpPenetrationBuffer = "10";
        public static float pawnToolBluntPenetration = 40f; public static string pawnToolBluntPenetrationBuffer = "40";

        public static float pawnKindMinMags = 2f; public static string pawnKindMinMagsBuffer = "2";
        public static float pawnKindMaxMags = 5f; public static string pawnKindMaxMagsBuffer = "5";

        public static bool patchBackpacks = true;
        public static bool patchCarryBulk = true;

        public static float geneArmorSharpMult = 10; public static string geneArmorSharpMultBuffer = "10";
        public static float geneArmorBluntMult = 10; public static string geneArmorBluntMultBuffer = "10";

        //hediff settings
        public static float hediffSharpMult = 10; public static string hediffSharpMultBuffer = "10";
        public static float hediffBluntMult = 40; public static string hediffBluntMultBuffer = "40";

        //other
        public static float vehicleSharpMult = 15; public static string vehicleSharpMultBuffer = "15";
        public static float vehicleBluntMult = 15; public static string vehicleBluntMultBuffer = "15";
        public static float vehicleHealthMult = 3; public static string vehicleHealthMultBuffer = "3.0";

        public override void ExposeData()
        {
            //General Settings
            Scribe_Values.Look(ref printLogs, "printDebug", false);
            Scribe_Values.Look(ref printPatchErrors, "printPatchErrors", false);
            Scribe_Values.Look(ref patchWeapons, "patchWeapons", true);
            Scribe_Values.Look(ref patchCustomVerbs, "patchCustomVerbs", false);
            Scribe_Values.Look(ref limitWeaponMass, "limitWeaponMass", false);
            Scribe_Values.Look(ref patchApparels, "patchApparels", true);
            Scribe_Values.Look(ref patchHeadgearLayers, "patchHeadgearLayers", true);
            Scribe_Values.Look(ref patchPawns, "patchPawns", true);
            Scribe_Values.Look(ref patchPawnKinds, "patchPawnKinds", true);
            Scribe_Values.Look(ref patchGenes, "patchGenes", true);
            Scribe_Values.Look(ref patchHediffs, "patchHediffs", true);
            Scribe_Values.Look(ref patchVehicles, "patchVehicles", true);

            //Modlist Settings
            Scribe_Collections.Look(ref modsByPackageId, "modsByPackageId", LookMode.Value);


            //Balance Control Settings
            //armor
            Scribe_Values.Look(ref apparelSharpMult, "apparelSharpMult", 10);   Scribe_Values.Look(ref apparelSharpMultBuffer, "apparelSharpMultBuffer", "10");
            Scribe_Values.Look(ref apparelBluntMult, "apparelBluntMult", 40);   Scribe_Values.Look(ref apparelBluntMultBuffer, "apparelBluntMultBuffer", "40");
            Scribe_Values.Look(ref armorTechMultAnimal, "armorTechMultAnimal", 0.25f); Scribe_Values.Look(ref armorTechMultAnimalBuffer, "armorTechMultAnimalBuffer", "0.25");
            Scribe_Values.Look(ref armorTechMultNeolithic, "armorTechMultNeolithic", 0.5f); Scribe_Values.Look(ref armorTechMultNeolithicBuffer, "armorTechMultNeolithicBuffer", "0.5");
            Scribe_Values.Look(ref armorTechMultMedieval, "armorTechMultMedieval", 0.75f); Scribe_Values.Look(ref armorTechMultMedievalBuffer, "armorTechMultMedievalBuffer", "0.75");
            Scribe_Values.Look(ref armorTechMultIndustrial, "armorTechMultIndustrial", 1f); Scribe_Values.Look(ref armorTechMultIndustrialBuffer, "armorTechMultIndustrialBuffer", "1.0");
            Scribe_Values.Look(ref armorTechMultSpacer, "armorTechMultSpacer", 2f); Scribe_Values.Look(ref armorTechMultSpacerBuffer, "armorTechMultSpacerBuffer", "2.0");
            Scribe_Values.Look(ref armorTechMultUltratech, "armorTechMultUltratech", 3f); Scribe_Values.Look(ref armorTechMultUltratechBuffer, "armorTechMultUltratechBuffer", "3.0");
            Scribe_Values.Look(ref armorTechMultArchotech, "armorTechMultArchotech", 4f); Scribe_Values.Look(ref armorTechMultArchotechBuffer, "armorTechMultArchotechBuffer", "4.0");

            //hediff
            Scribe_Values.Look(ref hediffSharpMult, "hediffSharpMult", 10); Scribe_Values.Look(ref hediffSharpMultBuffer, "hediffSharpMultBuffer", "10");
            Scribe_Values.Look(ref hediffBluntMult, "hediffBluntMult", 40); Scribe_Values.Look(ref hediffBluntMultBuffer, "hediffBluntMultBuffer", "40");

            //pawn
            Scribe_Values.Look(ref pawnArmorSharpMult, "pawnArmorSharpMult", 10); Scribe_Values.Look(ref pawnArmorSharpMultBuffer, "pawnArmorSharpMultBuffer", "10");
            Scribe_Values.Look(ref pawnArmorBluntMult, "pawnArmorBluntMult", 40); Scribe_Values.Look(ref pawnArmorBluntMultBuffer, "pawnArmorBluntMultBuffer", "40");
            Scribe_Values.Look(ref pawnToolPowerMult, "pawnToolPowerMult", 1); Scribe_Values.Look(ref pawnToolPowerMultBuffer, "pawnToolPowerMultBuffer", "1");
            Scribe_Values.Look(ref pawnToolSharpPenetration, "pawnToolSharpPenetration", 10); Scribe_Values.Look(ref pawnToolPowerMultBuffer, "pawnToolSharpPenetrationBuffer", "10");
            Scribe_Values.Look(ref pawnToolPowerMult, "pawnToolBluntPenetration", 40); Scribe_Values.Look(ref pawnToolPowerMultBuffer, "pawnToolBluntPenetrationBuffer", "40");

            Scribe_Values.Look(ref pawnKindMinMags, "pawnKindMinMags", 2); Scribe_Values.Look(ref pawnKindMinMagsBuffer, "pawnKindMinMagsBuffer", "2");
            Scribe_Values.Look(ref pawnKindMaxMags, "pawnKindMaxMags", 5); Scribe_Values.Look(ref pawnKindMaxMagsBuffer, "pawnKindMaxMagsBuffer", "5");

            Scribe_Values.Look(ref patchBackpacks, "patchBackpacks", true);

            Scribe_Values.Look(ref geneArmorSharpMult, "geneArmorSharpMult", 10); Scribe_Values.Look(ref geneArmorSharpMultBuffer, "geneArmorSharpMultBuffer", "10");
            Scribe_Values.Look(ref geneArmorBluntMult, "geneArmorBluntMult", 40); Scribe_Values.Look(ref geneArmorBluntMultBuffer, "geneArmorBluntMultBuffer", "40");

            //weapon
            Scribe_Values.Look(ref gunSharpPenMult, "gunSharpPenMult", 10); Scribe_Values.Look(ref gunSharpPenMultBuffer, "gunSharpPenMultBuffer", "10");
            Scribe_Values.Look(ref gunBluntPenMult, "gunBluntPenMult", 40); Scribe_Values.Look(ref gunBluntPenMultBuffer, "gunBluntPenMultBuffer", "40");
            Scribe_Values.Look(ref gunTechMultAnimal, "gunTechMultAnimal", 0.5f); Scribe_Values.Look(ref gunTechMultAnimalBuffer, "gunTechMultAnimalBuffer", "0.5");
            Scribe_Values.Look(ref gunTechMultNeolithic, "gunTechMultNeolithic", 1f); Scribe_Values.Look(ref gunTechMultNeolithicBuffer, "gunTechMultNeolithicBuffer", "1.0");
            Scribe_Values.Look(ref gunTechMultMedieval, "gunTechMultMedieval", 2f); Scribe_Values.Look(ref gunTechMultMedievalBuffer, "gunTechMultMedievalBuffer", "2.0");
            Scribe_Values.Look(ref gunTechMultIndustrial, "gunTechMultIndustrial", 4f); Scribe_Values.Look(ref gunTechMultIndustrialBuffer, "gunTechMultIndustrialBuffer", "4.0");
            Scribe_Values.Look(ref gunTechMultSpacer, "gunTechMultSpacer", 5f); Scribe_Values.Look(ref gunTechMultSpacerBuffer, "gunTechMultSpacerBuffer", "5.0");
            Scribe_Values.Look(ref gunTechMultUltratech, "gunTechMultUltratech", 6f); Scribe_Values.Look(ref gunTechMultUltratechBuffer, "gunTechMultUltratechBuffer", "6.0");
            Scribe_Values.Look(ref gunTechMultArchotech, "gunTechMultArchotech", 8f); Scribe_Values.Look(ref gunTechMultArchotechBuffer, "gunTechMultArchotechBuffer", "8.0");

            Scribe_Values.Look(ref weaponToolPowerMult, "weaponToolPowerMult", 1); Scribe_Values.Look(ref weaponToolPowerMultBuffer, "weaponToolPowerMultBuffer", "1");
            Scribe_Values.Look(ref weaponToolSharpPenetration, "weaponToolSharpPenetration", 1); Scribe_Values.Look(ref weaponToolSharpPenetrationBuffer, "weaponToolSharpPenetrationBuffer", "1");
            Scribe_Values.Look(ref weaponToolBluntPenetration, "weaponToolBluntPenetration", 4); Scribe_Values.Look(ref weaponToolBluntPenetrationBuffer, "weaponToolBluntPenetrationBuffer", "4");
            Scribe_Values.Look(ref weaponToolTechMultAnimal, "weaponToolTechMultAnimal", 1f); Scribe_Values.Look(ref weaponToolTechMultAnimalBuffer, "weaponToolTechMultAnimalBuffer", "1.0");
            Scribe_Values.Look(ref weaponToolTechMultNeolithic, "weaponToolTechMultNeolithic", 1f); Scribe_Values.Look(ref weaponToolTechMultNeolithicBuffer, "weaponToolTechMultNeolithicBuffer", "1.0");
            Scribe_Values.Look(ref weaponToolTechMultMedieval, "weaponToolTechMultMedieval", 1f); Scribe_Values.Look(ref weaponToolTechMultMedievalBuffer, "weaponToolTechMultMedievalBuffer", "1.0");
            Scribe_Values.Look(ref weaponToolTechMultIndustrial, "weaponToolTechMultIndustrial", 2f);  Scribe_Values.Look(ref weaponToolTechMultIndustrialBuffer, "weaponToolTechMultIndustrialBuffer", "2.0");
            Scribe_Values.Look(ref weaponToolTechMultSpacer, "weaponToolTechMultSpacer", 3f); Scribe_Values.Look(ref weaponToolTechMultSpacerBuffer, "weaponToolTechMultSpacerBuffer", "3.0");
            Scribe_Values.Look(ref weaponToolTechMultUltratech, "weaponToolTechMultUltratech", 4f); Scribe_Values.Look(ref weaponToolTechMultUltratechBuffer, "weaponToolTechMultUltratechBuffer", "4.0");
            Scribe_Values.Look(ref weaponToolTechMultArchotech, "weaponToolTechMultArchotech", 6f); Scribe_Values.Look(ref weaponToolTechMultArchotechBuffer, "weaponToolTechMultArchotechBuffer", "6.0");
            Scribe_Values.Look(ref maximumWeaponMass, "maximumWeaponMass", 20f); Scribe_Values.Look(ref maximumWeaponMassBuffer, "maximumWeaponMassBuffer", "20.0");

            //other
            Scribe_Values.Look(ref vehicleSharpMult, "vehicleSharpMult", 15f);  Scribe_Values.Look(ref vehicleSharpMultBuffer, "vehicleSharpMultBuffer", "15.0");
            Scribe_Values.Look(ref vehicleBluntMult, "vehicleBluntMult", 15f); Scribe_Values.Look(ref vehicleBluntMultBuffer, "vehicleBluntMultBuffer", "15.0");
            Scribe_Values.Look(ref vehicleHealthMult, "vehicleHealthMult", 3f); Scribe_Values.Look(ref vehicleHealthMultBuffer, "vehicleHealthMultBuffer", "3.0");

            base.ExposeData();
        }
    }
}
