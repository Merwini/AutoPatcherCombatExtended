using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace nuff.AutoPatcherCombatExtended
{
    public class APCESettings : ModSettings
    {
        internal static APCEConstants.SettingsTabs settingsTabs = APCEConstants.SettingsTabs.General_Settings;
        internal static APCEConstants.BalanceTabs balanceTabs = APCEConstants.BalanceTabs.Apparel;

        //General Settings
        internal static bool patchWeapons = true;
        internal static bool patchCustomVerbs = false;
        internal static bool patchApparels = true;
        internal static bool patchPawns = true;
        internal static bool patchPawnKinds = true;
        internal static bool patchHediffs = true;
        internal static bool patchHeadgearLayers = true;
        internal static bool printLogs = false;
        internal static bool printPatchErrors = false;


        //Modlist Settings
        internal static List<ModContentPack> activeMods = new List<ModContentPack>(); //will not be saved. will be gotten at startup
        internal static List<ModContentPack> modsToPatch = new List<ModContentPack>(); //will also not be saved, but instead a saved list of PackageIDs will be used to rebuild this list at startup
        internal static List<string> modsByPackageId = new List<string>(); //this is the list that will be used to rebuild the modsToPatch list on startup
        internal static ModContentPack thisMod;
        internal static HashSet<ModContentPack> modsAlreadyPatched = new HashSet<ModContentPack>(); //set of patched mods, to keep track so added mods can be patched when closing the settings window

        internal string searchTerm = "";
        internal Vector2 leftScrollPosition = new Vector2();
        internal Vector2 rightScrollPosition = new Vector2();
        internal ModContentPack leftSelectedObject = null;
        internal ModContentPack rightSelectedObject = null;


        //Balance Control Settings

        //armor settings
        internal static float apparelSharpMult = 10;    internal static string apparelSharpMultBuffer = "10";
        internal static float apparelBluntMult = 40;    internal static string apparelBluntMultBuffer = "40";
        internal static float armorTechMultAnimal = 0.25f;  internal static string armorTechMultAnimalBuffer = "0.25";
        internal static float armorTechMultNeolithic = 0.5f;    internal static string armorTechMultNeolithicBuffer = "0.5";
        internal static float armorTechMultMedieval = 0.75f;    internal static string armorTechMultMedievalBuffer = "0.75";
        internal static float armorTechMultIndustrial = 1;  internal static string armorTechMultIndustrialBuffer = "1";
        internal static float armorTechMultSpacer = 2;  internal static string armorTechMultSpacerBuffer = "2";
        internal static float armorTechMultUltratech = 3;   internal static string armorTechMultUltratechBuffer = "3";
        internal static float armorTechMultArchotech = 4;   internal static string armorTechMultArchotechBuffer = "4";

        internal static float skinBulkAdd = 1f;
        internal static float skinWulkAdd = 0.5f;
        internal static float slinBulkMult = 1f;
        internal static float skinWulkMult = 1f;

        internal static float midBulkAdd = 5f;
        internal static float midWulkAdd = 3f;
        internal static float midBulkMult = 1f;
        internal static float midWulkMult = 1f;

        internal static float shellBulkAdd = 7.5f;
        internal static float shellWulkAdd = 2.5f;
        internal static float shellBulkMult = 20f;
        internal static float shellWulkMult = 5f;

        internal static float advancedArmorCarryWeight = 80f;
        internal static float advancedArmorCarryBulk = 10f;
        internal static float advancedArmorShootingAccuracy = 0.2f; 

        internal static int testInt = 5;

        //weapon settings
        internal static float gunSharpPenMult = 10f; internal static string gunSharpPenMultBuffer = "10";
        internal static float gunBluntPenMult = 40f; internal static string gunBluntPenMultBuffer = "40";
        internal static float gunTechMultAnimal = 0.5f; internal static string gunTechMultAnimalBuffer = "0.5";
        internal static float gunTechMultNeolithic = 1f; internal static string gunTechMultNeolithicBuffer = "1.0";
        internal static float gunTechMultMedieval = 2f; internal static string gunTechMultMedievalBuffer = "2.0";
        internal static float gunTechMultIndustrial = 4; internal static string gunTechMultIndustrialBuffer = "4";
        internal static float gunTechMultSpacer = 5; internal static string gunTechMultSpacerBuffer = "5";
        internal static float gunTechMultUltratech = 6; internal static string gunTechMultUltratechBuffer = "6";
        internal static float gunTechMultArchotech = 8; internal static string gunTechMultArchotechBuffer = "8";

        internal static float weaponToolPowerMult = 1f; internal static string weaponToolPowerMultBuffer = "1";
        internal static float weaponToolSharpPenetration = 10f; internal static string weaponToolSharpPenetrationBuffer = "10";
        internal static float weaponToolBluntPenetration = 40f; internal static string weaponToolBluntPenetrationBuffer = "40";


        //pawn settings
        internal static float pawnArmorSharpMult = 10; internal static string pawnArmorSharpMultBuffer = "10";
        internal static float pawnArmorBluntMult = 40; internal static string pawnArmorBluntMultBuffer = "40";

        internal static float pawnToolPowerMult = 1f; internal static string pawnToolPowerMultBuffer = "1";
        internal static float pawnToolSharpPenetration = 10f; internal static string pawnToolSharpPenetrationBuffer = "10";
        internal static float pawnToolBluntPenetration = 40f; internal static string pawnToolBluntPenetrationBuffer = "40";

        internal static float pawnKindMinMags = 2f; internal static string pawnKindMinMagsBuffer = "2";
        internal static float pawnKindMaxMags = 5f; internal static string pawnKindMaxMagsBuffer = "5";

        internal static bool patchBackpacks = true;

        //hediff settings
        internal static float hediffSharpMult = 10; internal static string hediffSharpMultBuffer = "10";
        internal static float hediffBluntMult = 40; internal static string hediffBluntMultBuffer = "40";

        public override void ExposeData()
        {
            //General Settings
            Scribe_Values.Look(ref printLogs, "printDebug", false);
            Scribe_Values.Look(ref printPatchErrors, "printPatchErrors", false);
            Scribe_Values.Look(ref patchWeapons, "patchWeapons", true);
            Scribe_Values.Look(ref patchCustomVerbs, "patchCustomVerbs", false);
            Scribe_Values.Look(ref patchApparels, "patchApparels", true);
            Scribe_Values.Look(ref patchHeadgearLayers, "patchHeadgearLayers", true);
            Scribe_Values.Look(ref patchPawns, "patchPawns", true);
            Scribe_Values.Look(ref patchPawnKinds, "patchPawnKinds", true);
            Scribe_Values.Look(ref patchHediffs, "patchHediffs", true);

            //Modlist Settings
            Scribe_Collections.Look(ref modsByPackageId, "modsByPackageId", LookMode.Value);


            //Balance Control Settings
            //armor
            Scribe_Values.Look(ref apparelSharpMult, "apparelSharpMult", 10);   Scribe_Values.Look(ref apparelSharpMultBuffer, "apparelSharpMultBuffer", "10");
            Scribe_Values.Look(ref apparelBluntMult, "apparelBluntMult", 40);   Scribe_Values.Look(ref apparelBluntMultBuffer, "apparelBluntMultBuffer", "40");
            Scribe_Values.Look(ref armorTechMultAnimal, "armorTechMultAnimal", 0.25f); Scribe_Values.Look(ref armorTechMultAnimalBuffer, "armorTechMultAnimalBuffer", "0.25");
            Scribe_Values.Look(ref armorTechMultNeolithic, "armorTechMultNeolithic", 0.5f); Scribe_Values.Look(ref armorTechMultNeolithicBuffer, "armorTechMultNeolithicBuffer", "0.5");
            Scribe_Values.Look(ref armorTechMultMedieval, "armorTechMultMedieval", 0.75f); Scribe_Values.Look(ref armorTechMultMedievalBuffer, "armorTechMultMedievalBuffer", "0.75");
            Scribe_Values.Look(ref armorTechMultIndustrial, "armorTechMultIndustrial", 1f); Scribe_Values.Look(ref armorTechMultIndustrialBuffer, "armorTechMultIndustrialBuffer", "1");
            Scribe_Values.Look(ref armorTechMultSpacer, "armorTechMultSpacer", 2f); Scribe_Values.Look(ref armorTechMultSpacerBuffer, "armorTechMultSpacerBuffer", "2");
            Scribe_Values.Look(ref armorTechMultUltratech, "armorTechMultUltratech", 3f); Scribe_Values.Look(ref armorTechMultUltratechBuffer, "armorTechMultUltratechBuffer", "3");
            Scribe_Values.Look(ref armorTechMultArchotech, "armorTechMultArchotech", 4f); Scribe_Values.Look(ref armorTechMultArchotechBuffer, "armorTechMultArchotechBuffer", "4");

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

            //weapon
            Scribe_Values.Look(ref gunSharpPenMult, "gunSharpPenMult", 10); Scribe_Values.Look(ref gunSharpPenMultBuffer, "gunSharpPenMultBuffer", "10");
            Scribe_Values.Look(ref gunBluntPenMult, "gunBluntPenMult", 40); Scribe_Values.Look(ref gunBluntPenMultBuffer, "gunBluntPenMultBuffer", "40");

            base.ExposeData();
        }
    }
}
