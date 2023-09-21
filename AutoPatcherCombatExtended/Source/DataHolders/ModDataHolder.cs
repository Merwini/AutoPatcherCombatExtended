using System;
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
        internal ModContentPack mod;

        internal string packageId;
        internal bool isCustomized = false;

        //toggles
        internal bool patchWeapons = true;
        internal bool patchCustomVerbs = false;
        internal bool limitWeaponMass = false;
        internal bool patchApparels = true;
        internal bool patchPawns = true;
        internal bool patchPawnKinds = true;
        internal bool patchGenes = true;
        internal bool patchHediffs = true;
        internal bool patchHeadgearLayers = true;
        internal bool patchVehicles = true;

        //apparel values
        internal float apparelSharpMult = 10f;
        internal float apparelBluntMult = 40f;
        internal float apparelTechMultAnimal = 0.25f;
        internal float apparelTechMultNeolithic = 0.5f;
        internal float apparelTechMultMedieval = 0.75f;
        internal float apparelTechMultIndustrial = 1f;
        internal float apparelTechMultSpacer = 2f;
        internal float apparelTechMultUltratech = 3f;
        internal float apparelTechMultArchotech = 4f; 
        
        internal float advancedArmorCarryWeight = 80f;
        internal float advancedArmorCarryBulk = 10f;
        internal float advancedArmorShootingAccuracy = 0.2f;

        //weapon settings
        internal float gunSharpPenMult = 10f;
        internal float gunBluntPenMult = 40f;
        internal float gunTechMultAnimal = 0.5f;
        internal float gunTechMultNeolithic = 1f;
        internal float gunTechMultMedieval = 2f;
        internal float gunTechMultIndustrial = 4;
        internal float gunTechMultSpacer = 5;
        internal float gunTechMultUltratech = 6;
        internal float gunTechMultArchotech = 8;

        internal float weaponToolPowerMult = 1f;
        internal float weaponToolSharpPenetration = 1f;
        internal float weaponToolBluntPenetration = 4f;
        internal float weaponToolTechMultAnimal = 1f;
        internal float weaponToolTechMultNeolithic = 1f;
        internal float weaponToolTechMultMedieval = 1f;
        internal float weaponToolTechMultIndustrial = 2f;
        internal float weaponToolTechMultSpacer = 3f;
        internal float weaponToolTechMultUltratech = 4f;
        internal float weaponToolTechMultArchotech = 6f;

        internal float maximumWeaponMass = 20f;

        //pawn settings
        internal float pawnArmorSharpMult = 10;
        internal float pawnArmorBluntMult = 40;

        internal float pawnToolPowerMult = 1f;
        internal float pawnToolSharpPenetration = 10f;
        internal float pawnToolBluntPenetration = 40f;

        internal float pawnKindMinMags = 2f;
        internal float pawnKindMaxMags = 5f;

        internal bool patchBackpacks = true;

        internal float geneArmorSharpMult = 10;
        internal float geneArmorBluntMult = 10;

        //hediff settings
        internal float hediffSharpMult = 10;
        internal float hediffBluntMult = 40;

        //other
        internal float vehicleSharpMult = 15;
        internal float vehicleBluntMult = 15;
        internal float vehicleHealthMult = 3;
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
