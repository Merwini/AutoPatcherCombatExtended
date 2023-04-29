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
        internal static bool patchHediffs = true;
        internal static bool patchHeadgearLayers = true;
        internal static bool printLogs = false;
        internal static bool printPatchErrors = false;


        //Modlist Settings
        internal static List<ModContentPack> activeMods = new List<ModContentPack>(); //will not be saved. will be gotten at startup
        internal static List<ModContentPack> modsToPatch = new List<ModContentPack>(); //will also not be saved, but instead a saved list of PackageIDs will be used to rebuild this list at startup
        internal static List<string> modsByPackageId = new List<string>(); //this is the list that will be used to rebuild the modsToPatch list on startup
        internal static ModContentPack thisMod;
        internal static HashSet<ModContentPack> modsAlreadyPatched = new HashSet<ModContentPack>();

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

        internal static float advancedArmorCarryWeight = 80f; //TODO customize
        internal static float advancedArmorCarryBulk = 10f; //TODO customize
        internal static float advancedArmorShootingAccuracy = 0.2f; //TODO customize

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

        //hediff settings
        internal static float hediffSharpMult = 10; internal static string hediffSharpMultBuffer = "10";
        internal static float hediffBluntMult = 40; internal static string hediffBluntMultBuffer = "40";

        public override void ExposeData()
        {
            //General Settings
            Scribe_Values.Look(ref printLogs, "printDebug");
            Scribe_Values.Look(ref printPatchErrors, "printPatchErrors");
            Scribe_Values.Look(ref patchWeapons, "patchWeapons");
            Scribe_Values.Look(ref patchCustomVerbs, "patchCustomVerbs");
            Scribe_Values.Look(ref patchApparels, "patchApparels");
            Scribe_Values.Look(ref patchHeadgearLayers, "patchHeadgearLayers");
            Scribe_Values.Look(ref patchPawns, "patchPawns");
            Scribe_Values.Look(ref patchHediffs, "patchHediffs");

            //Modlist Settings
            Scribe_Collections.Look(ref modsByPackageId, "modsByPackageId", LookMode.Value);


            //Balance Control Settings
            //armor
            Scribe_Values.Look(ref apparelSharpMult, "apparelSharpMult");   Scribe_Values.Look(ref apparelSharpMultBuffer, "apparelSharpMultBuffer");
            Scribe_Values.Look(ref apparelBluntMult, "apparelBluntMult");   Scribe_Values.Look(ref apparelBluntMultBuffer, "apparelBluntMultBuffer");
            Scribe_Values.Look(ref armorTechMultAnimal, "armorTechMultAnimal"); Scribe_Values.Look(ref armorTechMultAnimalBuffer, "armorTechMultAnimalBuffer");
            Scribe_Values.Look(ref armorTechMultNeolithic, "armorTechMultNeolithic"); Scribe_Values.Look(ref armorTechMultNeolithicBuffer, "armorTechMultNeolithicBuffer");
            Scribe_Values.Look(ref armorTechMultMedieval, "armorTechMultMedieval"); Scribe_Values.Look(ref armorTechMultMedievalBuffer, "armorTechMultMedievalBuffer");
            Scribe_Values.Look(ref armorTechMultIndustrial, "armorTechMultIndustrial"); Scribe_Values.Look(ref armorTechMultIndustrialBuffer, "armorTechMultIndustrialBuffer");
            Scribe_Values.Look(ref armorTechMultSpacer, "armorTechMultSpacer"); Scribe_Values.Look(ref armorTechMultSpacerBuffer, "armorTechMultSpacerBuffer");
            Scribe_Values.Look(ref armorTechMultUltratech, "armorTechMultUltratech"); Scribe_Values.Look(ref armorTechMultUltratechBuffer, "armorTechMultUltratechBuffer");
            Scribe_Values.Look(ref armorTechMultArchotech, "armorTechMultArchotech"); Scribe_Values.Look(ref armorTechMultArchotechBuffer, "armorTechMultArchotechBuffer");

            //hediff
            Scribe_Values.Look(ref hediffSharpMult, "hediffSharpMult"); Scribe_Values.Look(ref hediffSharpMultBuffer, "hediffSharpMultBuffer");
            Scribe_Values.Look(ref hediffBluntMult, "hediffBluntMult"); Scribe_Values.Look(ref hediffBluntMultBuffer, "hediffBluntMultBuffer");

            //pawn
            Scribe_Values.Look(ref pawnArmorSharpMult, "pawnArmorSharpMult"); Scribe_Values.Look(ref pawnArmorSharpMultBuffer, "pawnArmorSharpMultBuffer");
            Scribe_Values.Look(ref pawnArmorBluntMult, "pawnArmorBluntMult"); Scribe_Values.Look(ref pawnArmorBluntMultBuffer, "pawnArmorBluntMultBuffer");
            Scribe_Values.Look(ref pawnToolPowerMult, "pawnToolPowerMult"); Scribe_Values.Look(ref pawnToolPowerMultBuffer, "pawnToolPowerMultBuffer");
            Scribe_Values.Look(ref pawnToolSharpPenetration, "pawnToolSharpPenetration"); Scribe_Values.Look(ref pawnToolPowerMultBuffer, "pawnToolSharpPenetrationBuffer");
            Scribe_Values.Look(ref pawnToolPowerMult, "pawnToolBluntPenetration"); Scribe_Values.Look(ref pawnToolPowerMultBuffer, "pawnToolBluntPenetrationBuffer");

            //weapon
            Scribe_Values.Look(ref gunSharpPenMult, "gunSharpPenMult"); Scribe_Values.Look(ref gunSharpPenMultBuffer, "gunSharpPenMultBuffer");
            Scribe_Values.Look(ref gunBluntPenMult, "gunBluntPenMult"); Scribe_Values.Look(ref gunBluntPenMultBuffer, "gunBluntPenMultBuffer");

            base.ExposeData();
        }
    }
}
