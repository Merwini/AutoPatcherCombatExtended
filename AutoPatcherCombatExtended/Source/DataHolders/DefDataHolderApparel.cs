﻿using System;
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
        //TODO: headgear layers
        DefDataHolderApparel(ThingDef def)
        {
            this.def = def;
            defName = def.defName;
            parentModPackageId = def.modContentPack.PackageId;
            modData = DataHolderUtil.ReturnModDataOrDefault(def);

            GetOriginalData();
            AutoCalculate();
        }

        internal ThingDef def;

        //should I make this a list or dictionary or something?
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
        bool isSkin = false;
        bool isMid = false;
        bool isShell = false;
        bool coversTorso = false;
        bool coversLegs = false;
        bool isArmor = false;

        //saved modified values
        //vanilla bloc
        float modified_ArmorRatingSharp;
        float modified_ArmorRatingBlunt;
        float modified_ArmorRatingHeat;
        float modified_Mass;
        float modified_MaxHitPoints;
        float modified_ShootingAccuracyPawn;

        //CE bloc
        float modified_Bulk;
        float modified_WornBulk;
        float modified_CarryWeight;
        float modified_CarryBulk;
        float modified_SmokeSensitivity;
        float modified_NightVisionEfficiency;

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

            CalculateTechMult();
            CheckWhatCovers();
        }

        public override void AutoCalculate()
        {
            modified_ArmorRatingSharp = original_ArmorRatingSharp * modData.apparelSharpMult * apparelTechMult;;
            modified_ArmorRatingBlunt = original_ArmorRatingBlunt * modData.apparelBluntMult * apparelTechMult;
            CalculateBulk();
            CalculateStatMods();
        }

        public override void Patch()
        {
            //check for null def in case this object was loaded without the def present e.g. from the mod source not being active
            if (def != null)
            {
                DataHolderUtil.AddOrChangeStat(def.statBases, StatDefOf.ArmorRating_Sharp, modified_ArmorRatingSharp);
                DataHolderUtil.AddOrChangeStat(def.statBases, StatDefOf.ArmorRating_Blunt, modified_ArmorRatingBlunt);
                DataHolderUtil.AddOrChangeStat(def.statBases, StatDefOf.ArmorRating_Heat, modified_ArmorRatingHeat);
                DataHolderUtil.AddOrChangeStat(def.statBases, StatDefOf.Mass, modified_Mass);
                DataHolderUtil.AddOrChangeStat(def.statBases, StatDefOf.MaxHitPoints, modified_MaxHitPoints);
                DataHolderUtil.AddOrChangeStat(def.equippedStatOffsets, StatDefOf.ShootingAccuracyPawn, modified_ShootingAccuracyPawn);

                DataHolderUtil.AddOrChangeStat(def.statBases, CE_StatDefOf.Bulk, modified_Bulk);
                DataHolderUtil.AddOrChangeStat(def.statBases, CE_StatDefOf.WornBulk, modified_WornBulk);
                DataHolderUtil.AddOrChangeStat(def.equippedStatOffsets, CE_StatDefOf.CarryWeight, modified_CarryWeight);
                DataHolderUtil.AddOrChangeStat(def.equippedStatOffsets, CE_StatDefOf.CarryBulk, modified_CarryBulk);
                DataHolderUtil.AddOrChangeStat(def.equippedStatOffsets, CE_StatDefOf.SmokeSensitivity, modified_SmokeSensitivity);
                DataHolderUtil.AddOrChangeStat(def.equippedStatOffsets, CE_StatDefOf.NightVisionEfficiency, modified_NightVisionEfficiency);
            }
        }

        public override StringBuilder PrepExport()
        {
            //todo
            return null;
        }

        public override void Export()
        {
            //todo
        }

        public override void ExposeData()
        {
            base.ExposeData();
            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                def = DefDatabase<ThingDef>.GetNamed(defName, false);
                if (def != null)
                {
                    GetOriginalData();
                }
            }
            Scribe_Values.Look(ref modified_ArmorRatingSharp, "modified_ArmorRatingSharp", 0);
            Scribe_Values.Look(ref modified_ArmorRatingBlunt, "modified_ArmorRatingBlunt", 0f);
            Scribe_Values.Look(ref modified_ArmorRatingHeat, "modified_ArmorRatingHeat", 0f);
            Scribe_Values.Look(ref modified_Mass, "modified_Mass", 0f);
            Scribe_Values.Look(ref modified_MaxHitPoints, "modified_MaxHitPoints", 0f);
            Scribe_Values.Look(ref modified_ShootingAccuracyPawn, "modified_ShootingAccuracyPawn", 0f);
            Scribe_Values.Look(ref modified_Bulk, "modified_Bulk", 0f);
            Scribe_Values.Look(ref modified_WornBulk, "modified_WornBulk", 0f);
            Scribe_Values.Look(ref modified_CarryWeight, "modified_CarryWeight", 0f);
            Scribe_Values.Look(ref modified_CarryBulk, "modified_CarryBulk", 0f);
            Scribe_Values.Look(ref modified_SmokeSensitivity, "modified_SmokeSensitivity", 0f);
            Scribe_Values.Look(ref modified_NightVisionEfficiency, "modified_NightVisionEfficiency", 0f);
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

        public void CheckWhatCovers()
        {
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
            if ((def.thingCategories != null && (def.thingCategories.Contains(ThingCategoryDefOf.ApparelArmor) || def.thingCategories.Contains(ThingCategoryDefOf.ArmorHeadgear)))
                || (def.tradeTags != null && def.tradeTags.Any(tag => tag.ToLower().Contains("armor"))))
            {
                isArmor = true;
            }
        }

        public void CalculateBulk()
        {            
            //rewrite notes:
            //flak vest 5 bulk, 3 wulk
            //flak pants 4 bulk, 2.5 wulk
            //jackets, parkas, flak jacket, etc are about 5-7.5, bulk 1-3 wulk
            //whole-body armor 80-100, bulk 12-15 wulk
            float newBulk = 0;
            float newWornBulk = 0;
            if (isArmor)
            {
                if (isHeadgear)
                {
                    newBulk = 5;
                    newWornBulk = 3;
                }
                else
                {
                    if (coversLegs)
                    {
                        newBulk += 5;
                        newWornBulk += 3;
                    }
                    if (coversTorso)
                    {
                        newBulk += 5;
                        newWornBulk += 3;
                    }
                    if (isMid && isShell)
                    {
                        newBulk *= 8;
                        newWornBulk *= 2;
                    }
                }
            }

            modified_Bulk = newBulk;
            modified_WornBulk = newWornBulk;
        }

        public void CalculateStatMods()
        {
            //smoke sensitivty / nightvision for headgear
            if (def.apparel.bodyPartGroups.Any(bpgd =>
            {
                if (bpgd == BodyPartGroupDefOf.Eyes || bpgd == BodyPartGroupDefOf.FullHead)
                    return true;
                else
                    return false;
            }))
            {
                if (def.equippedStatOffsets == null)
                {
                    def.equippedStatOffsets = new List<StatModifier>();
                }
                if (def.techLevel >= TechLevel.Industrial)
                {
                    modified_SmokeSensitivity = -1;
                }
                if (def.techLevel >= TechLevel.Spacer)
                {
                    modified_NightVisionEfficiency = 0.6f;
                }
            }

            //carryweight, carrybulk, shootingAccuracyPawn for body armors
            if (isArmor && (def.techLevel >= TechLevel.Industrial) && coversLegs && coversTorso && isShell)
            {
                modified_CarryWeight += 40;
                modified_CarryBulk += 5;
                if (isMid)
                {
                    modified_CarryWeight += 40;
                    modified_CarryBulk += 5;
                }
                if (original_ShootingAccuracyPawn == 0)
                {
                    modified_ShootingAccuracyPawn = 0.2f;
                }
            }
        }
    }
}
