using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using CombatExtended;
using UnityEngine;

namespace nuff.AutoPatcherCombatExtended
{
    public class DefDataHolderMeleeWeapon : DefDataHolder
    {
        public DefDataHolderMeleeWeapon()
        {
            //empty constructor for use by SaveLoad
        }

        public DefDataHolderMeleeWeapon(ThingDef def) : base(def)
        {
        }

        public ThingDef thingDef;

        float original_Mass;
        bool stuffed;

        internal float modified_Mass;
        internal float modified_Bulk;
        internal float modified_WeaponToughness;
        internal float modified_MeleeCounterParryBonus; //reminder - statbase
        internal float modified_MeleeDodgeChance;
        internal float modified_MeleeParryChance;
        internal float modified_MeleeCritChance;

        float floorArmorPenetrationSharp;
        float floorArmorPenetrationBlunt;

        //TODO
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

            try
            {
                if (!thingDef.tools.NullOrEmpty())
                {
                    original_Tools = thingDef.tools.ToList();
                }
                original_Mass = thingDef.statBases.GetStatValueFromList(StatDefOf.Mass, 0);
                stuffed = thingDef.MadeFromStuff;
            }
            catch (Exception ex)
            {
                Log.Error($"Exception in GetOriginalData() for: {def.defName}");
                Log.Error(ex.ToString());
            }
        }

        public override void AutoCalculate()
        {
            try
            {
                modified_Mass = original_Mass;
                modified_Bulk = modified_Mass * 2f; //TODO better calculation
                modified_WeaponToughness = DataHolderUtils.WeaponToughnessAutocalc(thingDef, modified_Bulk);
                CalculateStatMods();
                CalculateWeaponTechMult();
                if (!original_Tools.NullOrEmpty())
                {
                    ClearModdedTools();
                    for (int i = 0; i < original_Tools.Count; i++)
                    {
                        ModToolAtIndex(i);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Exception in AutoCalculate() for: {def.defName}");
                Log.Error(ex.ToString());
            }
        }

        public override void Patch()
        {
            try
            {
                if (thingDef.equippedStatOffsets == null)
                {
                    thingDef.equippedStatOffsets = new List<StatModifier>();
                }
                if (thingDef.statBases == null)
                {
                    thingDef.statBases = new List<StatModifier>();
                }

                DataHolderUtils.AddOrChangeStat(thingDef.statBases, StatDefOf.Mass, modified_Mass);
                DataHolderUtils.AddOrChangeStat(thingDef.statBases, CE_StatDefOf.Bulk, modified_Bulk);
                if (stuffed)
                {
                    DataHolderUtils.AddOrChangeStat(thingDef.statBases, CE_StatDefOf.StuffEffectMultiplierToughness, modified_WeaponToughness);
                }
                else
                {
                    DataHolderUtils.AddOrChangeStat(thingDef.statBases, CE_StatDefOf.ToughnessRating, modified_WeaponToughness);
                }
                DataHolderUtils.AddOrChangeStat(thingDef.statBases, CE_StatDefOf.MeleeCounterParryBonus, modified_MeleeCounterParryBonus);

                DataHolderUtils.AddOrChangeStat(thingDef.equippedStatOffsets, CE_StatDefOf.MeleeDodgeChance, modified_MeleeDodgeChance);
                DataHolderUtils.AddOrChangeStat(thingDef.equippedStatOffsets, CE_StatDefOf.MeleeParryChance, modified_MeleeParryChance);
                DataHolderUtils.AddOrChangeStat(thingDef.equippedStatOffsets, CE_StatDefOf.MeleeCritChance, modified_MeleeCritChance);

                if (!original_Tools.NullOrEmpty())
                {
                    thingDef.tools.Clear();
                    BuildTools();
                    for (int i = 0; i < modified_Tools.Count; i++)
                    {
                        thingDef.tools.Add(modified_Tools[i]);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Exception in Patch() for: {def.defName}");
                Log.Error(ex.ToString());
            }
        }

        public override StringBuilder ExportXML()
        {
            xml = DataHolderUtils.GetXmlForDef(thingDef);

            patchOps = new List<string>();
            patchOps.Add(APCEPatchExport.GeneratePatchOperationFor(xml, "statBases", "Mass", modified_Mass, original_Mass));
            patchOps.Add(APCEPatchExport.GeneratePatchOperationFor(xml, "statBases", "Bulk", modified_Bulk));
            //patchOps.Add(APCEPatchExport.GeneratePatchOperationFor(xml, "statBases", "ToughnessRating", modified_WeaponToughness)); //let weapon toughness autopatcher do its thing
            patchOps.Add(APCEPatchExport.GeneratePatchOperationFor(xml, "statBases", "MeleeCounterParryBonus", modified_MeleeCounterParryBonus));
            patchOps.Add(APCEPatchExport.GeneratePatchOperationFor(xml, "equippedStatOffsets", "MeleeDodgeChance", modified_MeleeDodgeChance));
            patchOps.Add(APCEPatchExport.GeneratePatchOperationFor(xml, "equippedStatOffsets", "MeleeParryChance", modified_MeleeParryChance));
            patchOps.Add(APCEPatchExport.GeneratePatchOperationFor(xml, "equippedStatOffsets", "MeleeCritChance", modified_MeleeCritChance));

            patchOps.Add(GenerateToolPatchXML());

            base.ExportXML();

            return patch;
        }

        public override void ExposeData()
        {
            if (Scribe.mode == LoadSaveMode.LoadingVars
                || (Scribe.mode == LoadSaveMode.Saving && isCustomized == true))
            {
                Scribe_Defs.Look(ref thingDef, "def");
                Scribe_Values.Look(ref modified_Mass, "modified_Mass", original_Mass);
                Scribe_Values.Look(ref modified_Bulk, "modified_Bulk", 1f);
                Scribe_Values.Look(ref modified_WeaponToughness, "modified_weaponToughness", 1f);
                Scribe_Values.Look(ref modified_MeleeCounterParryBonus, "modified_MeleeCounterParryBonus", 0f);
                Scribe_Values.Look(ref modified_MeleeDodgeChance, "modified_MeleeDodgeChance", 0f);
                Scribe_Values.Look(ref modified_MeleeParryChance, "modified_MeleeParryChance", 0f);
                Scribe_Values.Look(ref modified_MeleeCritChance, "modified_MeleeCritChance", 0f);
            }
            base.ExposeData();
        }

        private void CalculateStatMods()
        {
            float value = thingDef.BaseMarketValue;
            float valueLog = (float)Math.Log10(value);
            float massFactor = Mathf.Clamp((modified_Mass * 0.2f), 0.1f, 0.9f);

            //todo refine these... again
            //the *100 /100 is to round to 2 decimal places
            modified_MeleeCounterParryBonus = Mathf.Round(valueLog * 0.1f * 100f) / 100f;
            modified_MeleeDodgeChance = Mathf.Round(0.2f * 100f) / 100f;
            modified_MeleeParryChance = Mathf.Round(valueLog * massFactor * 100f) / 100f;
            modified_MeleeCritChance = Mathf.Round(valueLog * (1 - massFactor) * 100f) / 100f;
        }

        public override void ModToolAtIndex(int i)
        {
            base.ModToolAtIndex(i);
            modified_ToolPowers[i] *= ModData.weaponToolPowerMult;
            CalculateMinimumPenetrations(i);
            modified_ToolArmorPenetrationSharps[i] = Mathf.Clamp(modified_ToolArmorPenetrationSharps[i] * ModData.weaponToolSharpPenetration * techMult, floorArmorPenetrationSharp, 99999);
            modified_ToolArmorPenetrationBlunts[i] = Mathf.Clamp(modified_ToolArmorPenetrationBlunts[i] * ModData.weaponToolBluntPenetration * techMult, floorArmorPenetrationBlunt, 99999);
        }

        public void CalculateMinimumPenetrations(int i)
        {
            //TODO null checks
            DamageArmorCategoryDef ac = modified_ToolCapacityDefs[i][0].VerbsProperties.First().meleeDamageDef.armorCategory;
            if (ac == DamageArmorCategoryDefOf.Sharp)
            {
                floorArmorPenetrationSharp = modified_ToolPowers[i] * techMult * 0.1f;
                floorArmorPenetrationBlunt = floorArmorPenetrationSharp;
            }
            else if (ac == APCEDefOfTwo.Blunt)
            {
                floorArmorPenetrationSharp = 0;
                floorArmorPenetrationBlunt = modified_ToolPowers[i] * techMult * 0.33f;
            }
            else //heat or maybe mods add new ones
            {
                floorArmorPenetrationSharp = 0;
                floorArmorPenetrationBlunt = 0;
            }
        }

        public void CalculateWeaponTechMult()
        {
            float techMult = 1f;
            switch (thingDef.techLevel)
            {
                case TechLevel.Animal:
                    techMult *= ModData.weaponToolTechMultAnimal;
                    break;
                case TechLevel.Neolithic:
                    techMult *= ModData.weaponToolTechMultNeolithic;
                    break;
                case TechLevel.Medieval:
                    techMult *= ModData.weaponToolTechMultMedieval;
                    break;
                case TechLevel.Industrial:
                    techMult *= ModData.weaponToolTechMultIndustrial;
                    break;
                case TechLevel.Spacer:
                    techMult *= ModData.weaponToolTechMultSpacer;
                    break;
                case TechLevel.Ultra:
                    techMult *= ModData.weaponToolTechMultUltratech;
                    break;
                case TechLevel.Archotech:
                    techMult *= ModData.weaponToolTechMultArchotech;
                    break;
                default:
                    break;
            }

            this.techMult = techMult;
        }
    }
}
