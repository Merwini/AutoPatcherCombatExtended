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
    public class DefDataHolderApparel : DefDataHolder
    {
        //TODO: headgear layers
        public DefDataHolderApparel()
        {
            //empty constructor for use by SaveLoad
        }

        public DefDataHolderApparel(ThingDef def) : base(def)
        {
        }

        public ThingDef thingDef;

        //should I make this a list or dictionary or something?
        //unsaved values taken from unpatched def
        float original_ArmorRatingSharp;
        float original_ArmorRatingBlunt;
        float original_ArmorRatingHeat;
        float original_Mass;
        float original_CarryWeight;
        float original_ShootingAccuracyPawn;
        float original_MaxHitPoints;
        float original_StuffEffectMultiplierArmor;

        //unsaved calculated values
        float apparelTechMult;
        bool isHeadgear; 
        bool isSkin = false;
        bool isMid = false;
        bool isShell = false;
        bool coversTorso = false;
        bool coversLegs = false;
        bool isArmor = false;

        //saved modified values
        //vanilla bloc
        internal float modified_ArmorRatingSharp;
        internal float modified_ArmorRatingBlunt;
        internal float modified_ArmorRatingHeat;
        internal float modified_Mass;
        internal float modified_MaxHitPoints;
        internal float modified_ShootingAccuracyPawn;
        internal float modified_StuffEffectMultiplierArmor;

        //CE bloc
        internal float modified_Bulk;
        internal float modified_WornBulk;
        internal float modified_CarryWeight;
        internal float modified_CarryBulk;
        internal float modified_SmokeSensitivity;
        internal float modified_NightVisionEfficiency;

        public override void GetOriginalData()
        {
            //constructed by APCEController, def assigned by constructor
            if (def != null && thingDef == null)
            {
                this.thingDef = def as ThingDef;
            }
            //constructed by SaveLoad, thingDef loaded from xml
            else if (thingDef != null && def == null)
            {
                def = thingDef;
            }

            original_ArmorRatingSharp = thingDef.statBases.GetStatValueFromList(StatDefOf.ArmorRating_Sharp, 0.01f);
            original_ArmorRatingBlunt = thingDef.statBases.GetStatValueFromList(StatDefOf.ArmorRating_Blunt, 0.01f);
            original_ArmorRatingHeat = thingDef.statBases.GetStatValueFromList(StatDefOf.ArmorRating_Heat, 0);
            original_Mass = thingDef.statBases.GetStatValueFromList(StatDefOf.Mass, 0.01f);
            original_MaxHitPoints = thingDef.statBases.GetStatValueFromList(StatDefOf.MaxHitPoints, 0);
            original_CarryWeight = thingDef.equippedStatOffsets.GetStatValueFromList(CE_StatDefOf.CarryWeight, 0);
            original_ShootingAccuracyPawn = thingDef.equippedStatOffsets.GetStatValueFromList(StatDefOf.ShootingAccuracyPawn, 0);
            original_StuffEffectMultiplierArmor = thingDef.statBases.GetStatValueFromList(StatDefOf.StuffEffectMultiplierArmor, 0);

            CheckWhatCovers();
        }

        public override void AutoCalculate()
        {
            CalculateApparelTechMult();

            modified_ArmorRatingSharp = original_ArmorRatingSharp * modData.apparelSharpMult * apparelTechMult;
            modified_ArmorRatingBlunt = original_ArmorRatingBlunt * modData.apparelBluntMult * apparelTechMult;
            modified_ArmorRatingHeat = original_ArmorRatingHeat;

            modified_Mass = Math.Min(original_Mass, 50);
            modified_MaxHitPoints = original_MaxHitPoints;
            modified_StuffEffectMultiplierArmor = original_StuffEffectMultiplierArmor;

            CalculateBulk();
            CalculateStatMods();
        }

        public override void Patch()
        {
            //check for null def in case this object was loaded without the def present e.g. from the mod source not being active
            if (def != null)
            {
                DataHolderUtils.AddOrChangeStat(thingDef.statBases, StatDefOf.ArmorRating_Sharp, modified_ArmorRatingSharp);
                DataHolderUtils.AddOrChangeStat(thingDef.statBases, StatDefOf.ArmorRating_Blunt, modified_ArmorRatingBlunt);
                DataHolderUtils.AddOrChangeStat(thingDef.statBases, StatDefOf.ArmorRating_Heat, modified_ArmorRatingHeat);
                DataHolderUtils.AddOrChangeStat(thingDef.statBases, StatDefOf.Mass, modified_Mass);
                DataHolderUtils.AddOrChangeStat(thingDef.statBases, StatDefOf.MaxHitPoints, modified_MaxHitPoints);
                DataHolderUtils.AddOrChangeStat(thingDef.equippedStatOffsets, StatDefOf.ShootingAccuracyPawn, modified_ShootingAccuracyPawn);

                DataHolderUtils.AddOrChangeStat(thingDef.statBases, CE_StatDefOf.Bulk, modified_Bulk);
                DataHolderUtils.AddOrChangeStat(thingDef.statBases, CE_StatDefOf.WornBulk, modified_WornBulk);
                DataHolderUtils.AddOrChangeStat(thingDef.equippedStatOffsets, CE_StatDefOf.CarryWeight, modified_CarryWeight);
                DataHolderUtils.AddOrChangeStat(thingDef.equippedStatOffsets, CE_StatDefOf.CarryBulk, modified_CarryBulk);
                DataHolderUtils.AddOrChangeStat(thingDef.equippedStatOffsets, CE_StatDefOf.SmokeSensitivity, modified_SmokeSensitivity);
                DataHolderUtils.AddOrChangeStat(thingDef.equippedStatOffsets, CE_StatDefOf.NightVisionEfficiency, modified_NightVisionEfficiency);
            }
        }

        public override StringBuilder ExportXML()
        {
            xml = DataHolderUtils.GetXmlForDef(thingDef);

            patchOps = new List<string>();
            patchOps.Add(APCEPatchExport.AddOrReplaceXmlNodeWhitespace(xml, "statBases", "ArmorRating_Sharp", modified_ArmorRatingSharp));
            patchOps.Add(APCEPatchExport.AddOrReplaceXmlNodeWhitespace(xml, "statBases", "ArmorRating_Blunt", modified_ArmorRatingBlunt));
            patchOps.Add(APCEPatchExport.AddOrReplaceXmlNodeWhitespace(xml, "statBases", "ArmorRating_Heat", modified_ArmorRatingHeat));
            patchOps.Add(APCEPatchExport.AddOrReplaceXmlNodeWhitespace(xml, "statBases", "Mass", modified_Mass));
            patchOps.Add(APCEPatchExport.AddOrReplaceXmlNodeWhitespace(xml, "statBases", "Bulk", modified_Bulk));
            patchOps.Add(APCEPatchExport.AddOrReplaceXmlNodeWhitespace(xml, "statBases", "WornBulk", modified_WornBulk));
            patchOps.Add(APCEPatchExport.AddOrReplaceXmlNodeWhitespace(xml, "statBases", "MaxHitPoints", modified_MaxHitPoints));

            patchOps.Add(APCEPatchExport.AddOrReplaceXmlNodeWhitespace(xml, "equippedStatOffsets", "CarryBulk", modified_CarryBulk));
            patchOps.Add(APCEPatchExport.AddOrReplaceXmlNodeWhitespace(xml, "equippedStatOffsets", "CarryWeight", modified_CarryWeight));
            patchOps.Add(APCEPatchExport.AddOrReplaceXmlNodeWhitespace(xml, "equippedStatOffsets", "ShootingAccuracyPawn", modified_ShootingAccuracyPawn));
            patchOps.Add(APCEPatchExport.AddOrReplaceXmlNodeWhitespace(xml, "equippedStatOffsets", "SmokeSensitivity", modified_SmokeSensitivity));
            patchOps.Add(APCEPatchExport.AddOrReplaceXmlNodeWhitespace(xml, "equippedStatOffsets", "NightVisionEfficiency", modified_NightVisionEfficiency));

            base.ExportXML();

            return patch;
        }

        public override void ExposeData()
        {
            if (Scribe.mode == LoadSaveMode.LoadingVars
                || (Scribe.mode == LoadSaveMode.Saving && isCustomized == true))
            {
                Scribe_Defs.Look(ref thingDef, "def");
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
                Scribe_Values.Look(ref modified_StuffEffectMultiplierArmor, "modified_StuffEffectMultiplierArmor", 0f);
            }
            base.ExposeData();
        }

        public void CalculateApparelTechMult()
        {
            float techMult = 1f;
            switch (thingDef.techLevel)
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
            for (int i = 0; i < thingDef.apparel.layers.Count; i++)
            {
                //string matching is sloppy but helps with compatibility for custom alien race layers
                if (thingDef.apparel.layers[i] == ApparelLayerDefOf.OnSkin || thingDef.apparel.layers[i].ToString().ToUpper().Contains("SKIN") || thingDef.apparel.layers[i] == CE_ApparelLayerDefOf.StrappedHead)
                {
                    isSkin = true;
                }
                if (thingDef.apparel.layers[i] == ApparelLayerDefOf.Middle || thingDef.apparel.layers[i].ToString().ToUpper().Contains("MID") || thingDef.apparel.layers[i] == ApparelLayerDefOf.Overhead)
                {
                    isMid = true;
                    if (thingDef.apparel.layers[i] == ApparelLayerDefOf.Overhead)
                    {
                        isHeadgear = true;
                    }
                }
                if (thingDef.apparel.layers[i] == ApparelLayerDefOf.Shell || thingDef.apparel.layers[i].ToString().ToUpper().Contains("SHELL") || thingDef.apparel.layers[i].ToString().ToUpper().Contains("OUTER"))
                {
                    isShell = true;
                }
            }
            if (thingDef.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.Torso))
            {
                coversTorso = true;
            }
            if (thingDef.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.Legs))
            {
                coversLegs = true;
            }
            //probably still need a more elegant method to decide if an apparel should be counted as armor, but this is better than the old method of checking if mass > 2
            if ((thingDef.thingCategories != null && (thingDef.thingCategories.Contains(ThingCategoryDefOf.ApparelArmor) || thingDef.thingCategories.Contains(ThingCategoryDefOf.ArmorHeadgear)))
                || (thingDef.tradeTags != null && thingDef.tradeTags.Any(tag => tag.ToLower().Contains("armor")))
                || original_ArmorRatingSharp > 0.5f
                || original_ArmorRatingBlunt > 0.5f)
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
            if (thingDef.apparel.bodyPartGroups.Any(bpgd =>
            {
                if (bpgd == BodyPartGroupDefOf.Eyes || bpgd == BodyPartGroupDefOf.FullHead)
                    return true;
                else
                    return false;
            }))
            {
                if (thingDef.equippedStatOffsets == null)
                {
                    thingDef.equippedStatOffsets = new List<StatModifier>();
                }
                if (thingDef.techLevel >= TechLevel.Industrial)
                {
                    modified_SmokeSensitivity = -1;
                }
                if (thingDef.techLevel >= TechLevel.Spacer)
                {
                    modified_NightVisionEfficiency = 0.6f;
                }
            }

            //carryweight, carrybulk, shootingAccuracyPawn for body armors
            if (isArmor && (thingDef.techLevel >= TechLevel.Industrial) && coversLegs && coversTorso && isShell)
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
