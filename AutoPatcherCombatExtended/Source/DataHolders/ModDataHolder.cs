﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace nuff.AutoPatcherCombatExtended
{
    public class ModDataHolder : IExposable
    {
        public ModContentPack mod;

        public string packageId;
        public bool isCustomized = false;

        //saved as strings so they don't have issues if the mods updates and defs are added/removed/renamed
        public List<string> defsByName;
        public List<string> defsToPatch;
        public List<string> defsToIgnore; //need to save this so we can notify if new defs are added

        //toggles //todo remove
        public bool patchWeapons = true;
        public bool patchCustomVerbs = false;
        public bool limitWeaponMass = false;
        public bool patchApparels = true;
        public bool patchPawns = true;
        public bool patchPawnKinds = true;
        public bool patchGenes = true;
        public bool patchHediffs = true;
        public bool patchHeadgearLayers = true;
        public bool patchVehicles = true;

        //apparel values
        public float apparelSharpMult = 10f;
        public float apparelBluntMult = 40f;
        public float apparelTechMultAnimal = 0.25f;
        public float apparelTechMultNeolithic = 0.5f;
        public float apparelTechMultMedieval = 0.75f;
        public float apparelTechMultIndustrial = 1f;
        public float apparelTechMultSpacer = 2f;
        public float apparelTechMultUltratech = 3f;
        public float apparelTechMultArchotech = 4f; 
        
        public float advancedArmorCarryWeight = 80f;
        public float advancedArmorCarryBulk = 10f;
        public float advancedArmorShootingAccuracy = 0.2f;

        //weapon settings
        public float gunSharpPenMult = 10f;
        public float gunBluntPenMult = 40f;
        public float gunTechMultAnimal = 0.5f;
        public float gunTechMultNeolithic = 1f;
        public float gunTechMultMedieval = 2f;
        public float gunTechMultIndustrial = 4;
        public float gunTechMultSpacer = 5;
        public float gunTechMultUltratech = 6;
        public float gunTechMultArchotech = 8;

        public float weaponToolPowerMult = 1f;
        public float weaponToolSharpPenetration = 1f;
        public float weaponToolBluntPenetration = 4f;
        public float weaponToolTechMultAnimal = 1f;
        public float weaponToolTechMultNeolithic = 1f;
        public float weaponToolTechMultMedieval = 1f;
        public float weaponToolTechMultIndustrial = 2f;
        public float weaponToolTechMultSpacer = 3f;
        public float weaponToolTechMultUltratech = 4f;
        public float weaponToolTechMultArchotech = 6f;

        public float maximumWeaponMass = 20f;

        //pawn settings
        public float pawnArmorSharpMult = 10;
        public float pawnArmorBluntMult = 40;

        public float pawnToolPowerMult = 1f;
        public float pawnToolSharpPenetration = 10f;
        public float pawnToolBluntPenetration = 40f;

        public float pawnKindMinMags = 2f;
        public float pawnKindMaxMags = 5f;

        public bool patchBackpacks = true;

        public float geneArmorSharpMult = 10;
        public float geneArmorBluntMult = 10;

        //hediff settings
        public float hediffSharpMult = 10;
        public float hediffBluntMult = 40;

        //other
        public float vehicleSharpMult = 15;
        public float vehicleBluntMult = 15;
        public float vehicleHealthMult = 3;
        public void Reset()
        {
            //TODO reset values to those of nuff.ceautopatcher
        }

        public void ExposeData()
        {
            //only bother to save data if the user has changed values. If not, just recalculate during patching
            if (Scribe.mode == LoadSaveMode.LoadingVars
                || (Scribe.mode == LoadSaveMode.Saving && isCustomized == true))
            {
                Scribe_Values.Look(ref packageId, "packageId");
                Scribe_Values.Look(ref isCustomized, "isCustomized");

                //toggles
                Scribe_Values.Look(ref patchWeapons, "patchWeapons", true);
                Scribe_Values.Look(ref patchCustomVerbs, "patchCustomVerbs", false);
                Scribe_Values.Look(ref limitWeaponMass, "limitWeaponMass", false);
                Scribe_Values.Look(ref patchApparels, "patchApparels", true);
                Scribe_Values.Look(ref patchPawns, "patchPawns", true);
                Scribe_Values.Look(ref patchPawnKinds, "patchPawnKinds", true);
                Scribe_Values.Look(ref patchGenes, "patchGenes", true);
                Scribe_Values.Look(ref patchHediffs, "patchHediffs", true);
                Scribe_Values.Look(ref patchHeadgearLayers, "patchHeadgearLayers", true);
                Scribe_Values.Look(ref patchVehicles, "patchVehicles", true);

                // Apparel values
                Scribe_Values.Look(ref apparelSharpMult, "apparelSharpMult", 10);
                Scribe_Values.Look(ref apparelBluntMult, "apparelBluntMult", 40);
                Scribe_Values.Look(ref apparelTechMultAnimal, "apparelTechMultAnimal", 0.25f);
                Scribe_Values.Look(ref apparelTechMultNeolithic, "apparelTechMultNeolithic", 0.5f);
                Scribe_Values.Look(ref apparelTechMultMedieval, "apparelTechMultMedieval", 0.75f);
                Scribe_Values.Look(ref apparelTechMultIndustrial, "apparelTechMultIndustrial", 1f);
                Scribe_Values.Look(ref apparelTechMultSpacer, "apparelTechMultSpacer", 2f);
                Scribe_Values.Look(ref apparelTechMultUltratech, "apparelTechMultUltratech", 3f);
                Scribe_Values.Look(ref apparelTechMultArchotech, "apparelTechMultArchotech", 4f);

                Scribe_Values.Look(ref advancedArmorCarryWeight, "advancedArmorCarryWeight", 80f);
                Scribe_Values.Look(ref advancedArmorCarryBulk, "advancedArmorCarryBulk", 10f);
                Scribe_Values.Look(ref advancedArmorShootingAccuracy, "advancedArmorShootingAccuracy", 0.2f);

                // Weapon settings
                Scribe_Values.Look(ref gunSharpPenMult, "gunSharpPenMult", 10f);
                Scribe_Values.Look(ref gunBluntPenMult, "gunBluntPenMult", 40f);
                Scribe_Values.Look(ref gunTechMultAnimal, "gunTechMultAnimal", 0.5f);
                Scribe_Values.Look(ref gunTechMultNeolithic, "gunTechMultNeolithic", 1f);
                Scribe_Values.Look(ref gunTechMultMedieval, "gunTechMultMedieval", 2f);
                Scribe_Values.Look(ref gunTechMultIndustrial, "gunTechMultIndustrial", 4f);
                Scribe_Values.Look(ref gunTechMultSpacer, "gunTechMultSpacer", 5f);
                Scribe_Values.Look(ref gunTechMultUltratech, "gunTechMultUltratech", 6f);
                Scribe_Values.Look(ref gunTechMultArchotech, "gunTechMultArchotech", 8f);

                Scribe_Values.Look(ref weaponToolPowerMult, "weaponToolPowerMult", 1f);
                Scribe_Values.Look(ref weaponToolSharpPenetration, "weaponToolSharpPenetration", 1f);
                Scribe_Values.Look(ref weaponToolBluntPenetration, "weaponToolBluntPenetration", 4f);
                Scribe_Values.Look(ref weaponToolTechMultAnimal, "weaponToolTechMultAnimal", 1f);
                Scribe_Values.Look(ref weaponToolTechMultNeolithic, "weaponToolTechMultNeolithic", 1f);
                Scribe_Values.Look(ref weaponToolTechMultMedieval, "weaponToolTechMultMedieval", 1f);
                Scribe_Values.Look(ref weaponToolTechMultIndustrial, "weaponToolTechMultIndustrial", 2f);
                Scribe_Values.Look(ref weaponToolTechMultSpacer, "weaponToolTechMultSpacer", 3f);
                Scribe_Values.Look(ref weaponToolTechMultUltratech, "weaponToolTechMultUltratech", 4f);
                Scribe_Values.Look(ref weaponToolTechMultArchotech, "weaponToolTechMultArchotech", 6f);

                Scribe_Values.Look(ref maximumWeaponMass, "maximumWeaponMass", 20f);

                // Pawn settings
                Scribe_Values.Look(ref pawnArmorSharpMult, "pawnArmorSharpMult", 10f);
                Scribe_Values.Look(ref pawnArmorBluntMult, "pawnArmorBluntMult", 40f);

                Scribe_Values.Look(ref pawnToolPowerMult, "pawnToolPowerMult", 1f);
                Scribe_Values.Look(ref pawnToolSharpPenetration, "pawnToolSharpPenetration", 10f);
                Scribe_Values.Look(ref pawnToolBluntPenetration, "pawnToolBluntPenetration", 40f);

                Scribe_Values.Look(ref pawnKindMinMags, "pawnKindMinMags", 2f);
                Scribe_Values.Look(ref pawnKindMaxMags, "pawnKindMaxMags", 5f);

                Scribe_Values.Look(ref patchBackpacks, "patchBackpacks", true);

                Scribe_Values.Look(ref geneArmorSharpMult, "geneArmorSharpMult", 10f);
                Scribe_Values.Look(ref geneArmorBluntMult, "geneArmorBluntMult", 40f);

                // Hediff settings
                Scribe_Values.Look(ref hediffSharpMult, "hediffSharpMult", 10f);
                Scribe_Values.Look(ref hediffBluntMult, "hediffBluntMult", 40f);

                // Other
                Scribe_Values.Look(ref vehicleSharpMult, "vehicleSharpMult", 15f);
                Scribe_Values.Look(ref vehicleBluntMult, "vehicleBluntMult", 15f);
                Scribe_Values.Look(ref vehicleHealthMult, "vehicleHealthMult", 3f);
            }
            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                APCESettings.modDataDict.Add(packageId, this);
                mod = LoadedModManager.RunningMods.First(m => m.PackageId == packageId);
            }
        }
    }
}
