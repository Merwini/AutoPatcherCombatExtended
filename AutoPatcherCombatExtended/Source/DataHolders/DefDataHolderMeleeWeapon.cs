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

        internal float modified_mass;
        internal float modified_bulk;
        internal float modified_weaponToughness;
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

            if (!thingDef.tools.NullOrEmpty())
            {
                original_Tools = thingDef.tools.ToList();
            }
            original_Mass = thingDef.statBases.GetStatValueFromList(StatDefOf.Mass, 0);
            stuffed = thingDef.MadeFromStuff;
        }

        public override void AutoCalculate()
        {
            modified_mass = original_Mass;
            modified_bulk = modified_mass * 2f; //TODO better calculation
            modified_weaponToughness = DataHolderUtils.WeaponToughnessAutocalc(thingDef, modified_bulk);
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

        public override void Patch()
        {
            if (thingDef.equippedStatOffsets == null)
            {
                thingDef.equippedStatOffsets = new List<StatModifier>();
            }
            if (thingDef.statBases == null)
            {
                thingDef.statBases = new List<StatModifier>();
            }

            DataHolderUtils.AddOrChangeStat(thingDef.statBases, StatDefOf.Mass, modified_mass);
            DataHolderUtils.AddOrChangeStat(thingDef.statBases, CE_StatDefOf.Bulk, modified_bulk);
            if (stuffed)
            {
                DataHolderUtils.AddOrChangeStat(thingDef.statBases, CE_StatDefOf.StuffEffectMultiplierToughness, modified_weaponToughness);
            }
            else
            {
                DataHolderUtils.AddOrChangeStat(thingDef.statBases, CE_StatDefOf.ToughnessRating, modified_weaponToughness);
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

        public override StringBuilder PrepExport()
        {
            //TODO
            return null;
        }

        public override void ExportXML()
        {
            //TODO
        }

        public override void ExposeData()
        {
            if (Scribe.mode == LoadSaveMode.LoadingVars
                || (Scribe.mode == LoadSaveMode.Saving && isCustomized == true))
            {
                Scribe_Defs.Look(ref thingDef, "def");
                Scribe_Values.Look(ref modified_mass, "modified_Mass", original_Mass);
                Scribe_Values.Look(ref modified_bulk, "modified_Bulk", 1f);
                Scribe_Values.Look(ref modified_weaponToughness, "modified_weaponToughness", 1f);
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
            float massFactor = Mathf.Clamp((modified_mass * 0.2f), 0.1f, 0.9f);

            //todo refine these... again
            modified_MeleeCounterParryBonus = valueLog * 0.1f;
            modified_MeleeDodgeChance = 0.2f;
            modified_MeleeParryChance = valueLog * massFactor;
            modified_MeleeCritChance = valueLog * (1 - massFactor);
        }

        public override void ModToolAtIndex(int i)
        {
            base.ModToolAtIndex(i);
            modified_ToolPowers[i] *= modData.weaponToolPowerMult;
            CalculateMinimumPenetrations(i);
            modified_ToolArmorPenetrationSharps[i] = Mathf.Clamp(modified_ToolArmorPenetrationSharps[i] * modData.weaponToolSharpPenetration * techMult, floorArmorPenetrationSharp, 99999);
            modified_ToolArmorPenetrationBlunts[i] = Mathf.Clamp(modified_ToolArmorPenetrationBlunts[i] * modData.weaponToolBluntPenetration * techMult, floorArmorPenetrationBlunt, 99999);
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
                    techMult *= modData.weaponToolTechMultAnimal;
                    break;
                case TechLevel.Neolithic:
                    techMult *= modData.weaponToolTechMultNeolithic;
                    break;
                case TechLevel.Medieval:
                    techMult *= modData.weaponToolTechMultMedieval;
                    break;
                case TechLevel.Industrial:
                    techMult *= modData.weaponToolTechMultIndustrial;
                    break;
                case TechLevel.Spacer:
                    techMult *= modData.weaponToolTechMultSpacer;
                    break;
                case TechLevel.Ultra:
                    techMult *= modData.weaponToolTechMultUltratech;
                    break;
                case TechLevel.Archotech:
                    techMult *= modData.weaponToolTechMultArchotech;
                    break;
                default:
                    break;
            }

            this.techMult = techMult;
        }
    }
}
