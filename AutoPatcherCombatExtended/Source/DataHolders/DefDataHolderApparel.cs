using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using CombatExtended;

namespace nuff.AutoPatcherCombatExtended
{
    class DefDataHolderApparel : DefDataHolder
    {

        DefDataHolderApparel(ThingDef def)
        {
            this.def = def;
            defName = def.defName;
            parentModPackageId = def.modContentPack.PackageId;
            modData = APCESettings.modDataDict.TryGetValue(def.modContentPack.PackageId);
            if (modData == null)
            {
                modData = APCESettings.modDataDict.TryGetValue("nuff.ceautopatcher");
            }
            GetOriginalData();
            CalculateTechMult();
            CalculateBulk();

            //if !isCustomized autocalc
        }

        internal ThingDef def;

        //unsaved values taken from unpatched def
        float original_ArmorRatingSharp;
        float original_ArmorRatingBlunt;
        float original_ArmorRatingHeat;
        float original_Mass;
        float original_CarryWeight;
        float original_ShootingAccuracyPawn;
        float original_MaxHitPoints;

        //unsaved calculated values
        float apparelTechMult;
        float bulk;
        float wornBulk;
        bool isHeadgear;

        //saved modified values
        float modified_ArmorRatingSharp;
        float modified_ArmorRatingBlunt;
        float modified_ArmorRatingHeat;
        float modified_Mass;
        float modified_MaxHitPoints;
        float modified_Bulk;
        float modified_WornBulk;
        float modified_CarryWeight;
        float modified_CarryBulk;
        float modified_SmokeSensitivity;
        float modified_NightVisionEfficiency;
        float modified_ShootingAccuracyPawn;

        //these are not worth letting the player customize. they can just change the bulk and worn bulk directly instead
        float skinBulkAdd = 1f;
        float skinWulkAdd = 0.5f;
        float slinBulkMult = 1f;
        float skinWulkMult = 1f;
        float midBulkAdd = 5f;
        float midWulkAdd = 3f;
        float midBulkMult = 1f;
        float midWulkMult = 1f;
        float shellBulkAdd = 7.5f;
        float shellWulkAdd = 2.5f;
        float shellBulkMult = 20f;
        float shellWulkMult = 5f;

        public override void GetOriginalData()
        {
            original_ArmorRatingSharp = def.statBases.GetStatValueFromList(StatDefOf.ArmorRating_Sharp, 0);
            original_ArmorRatingBlunt = def.statBases.GetStatValueFromList(StatDefOf.ArmorRating_Blunt, 0);
            original_ArmorRatingHeat = def.statBases.GetStatValueFromList(StatDefOf.ArmorRating_Heat, 0);
            original_Mass = def.statBases.GetStatValueFromList(StatDefOf.Mass, 0);
            original_MaxHitPoints = def.statBases.GetStatValueFromList(StatDefOf.MaxHitPoints, 0);
            original_CarryWeight = def.equippedStatOffsets.GetStatValueFromList(CE_StatDefOf.CarryWeight, 0);
            original_ShootingAccuracyPawn = def.equippedStatOffsets.GetStatValueFromList(StatDefOf.ShootingAccuracyPawn, 0);
            //todo
        }

        public override void AutoCalculate()
        {
            modified_ArmorRatingSharp = original_ArmorRatingSharp * modData.apparelSharpMult * apparelTechMult;;
            modified_ArmorRatingBlunt = original_ArmorRatingBlunt * modData.apparelBluntMult * apparelTechMult;


            //todo
        }

        public override void Patch()
        {
            //todo
        }

        public override void PrepExport()
        {
            //todo
        }

        public override void Export()
        {
            //todo
        }

        public override void ExposeData()
        {
            base.ExposeData();
            //todo
        }

        public void CalculateTechMult()
        {
            float techMult = 1f;
            switch (def.techLevel)
            {
                case TechLevel.Animal:
                    techMult *= modData.apparelTechMultAnimal;
                    break;
                case TechLevel.Neolithic:
                    techMult *= modData.apparelTechMultNeolithic;
                    break;
                case TechLevel.Medieval:
                    techMult *= modData.apparelTechMultMedieval;
                    break;
                case TechLevel.Industrial:
                    techMult *= modData.apparelTechMultIndustrial;
                    break;
                case TechLevel.Spacer:
                    techMult *= modData.apparelTechMultSpacer;
                    break;
                case TechLevel.Ultra:
                    techMult *= modData.apparelTechMultUltratech;
                    break;
                case TechLevel.Archotech:
                    techMult *= modData.apparelTechMultArchotech;
                    break;
                default:
                    break;
            }

            apparelTechMult = techMult;
        }

        public void CalculateBulk()
        {
            bool isSkin = false;
            bool isMid = false;
            bool isShell = false;
            bool coversTorso = false;
            bool coversLegs = false;
            bool isArmor = false;
            float newBulk = 0;
            float newWornBulk = 0;

            //rewrite notes:
            //flak vest 5 bulk, 3 wulk
            //flak pants 4 bulk, 2.5 wulk
            //jackets, parkas, flak jacket, etc are about 5-7.5, bulk 1-3 wulk
            //whole-body armor 80-100, bulk 12-15 wulk

            for (int i = 0; i < def.apparel.layers.Count; i++)
            {
                //string matching is sloppy but helps with compatibility for custom alien race layers
                if (def.apparel.layers[i] == ApparelLayerDefOf.OnSkin || def.apparel.layers[i].ToString().ToUpper().Contains("SKIN") || def.apparel.layers[i] == CE_ApparelLayerDefOf.StrappedHead)
                {
                    isSkin = true;
                }
                if (def.apparel.layers[i] == ApparelLayerDefOf.Middle || def.apparel.layers[i].ToString().ToUpper().Contains("MID") || def.apparel.layers[i] == ApparelLayerDefOf.Overhead)
                {
                    isMid = true;
                    if (def.apparel.layers[i] == ApparelLayerDefOf.Overhead)
                    {
                        isHeadgear = true;
                    }
                }
                if (def.apparel.layers[i] == ApparelLayerDefOf.Shell || def.apparel.layers[i].ToString().ToUpper().Contains("SHELL") || def.apparel.layers[i].ToString().ToUpper().Contains("OUTER"))
                {
                    isShell = true;
                }
            }
            if (def.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.Torso))
            {
                coversTorso = true;
            }
            if (def.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.Legs))
            {
                coversLegs = true;
            }
            //probably still need a more elegant method to decide if an apparel should be counted as armor, but this is better than the old method of checking if mass > 2
            if (def.thingCategories.Contains(ThingCategoryDefOf.ApparelArmor)
                || def.thingCategories.Contains(ThingCategoryDefOf.ArmorHeadgear)
                || def.tradeTags.Any(tag => tag.ToLower().Contains("armor")))
            {
                isArmor = true;
            }

            //todo - use the above bools to calc the bulk and wulk
        }

    }
}
