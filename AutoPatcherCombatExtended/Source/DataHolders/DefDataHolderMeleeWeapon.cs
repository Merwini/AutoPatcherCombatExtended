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

        float modified_mass;
        float modified_bulk;
        float modified_weaponToughness;
        float modified_MeleeCounterParryBonus; //reminder - statbase
        float modified_MeleeDodgeChance;
        float modified_MeleeParryChance;
        float modified_MeleeCritChance;

        //TODO
        public override void GetOriginalData()
        {
            thingDef = def as ThingDef;
            if (!thingDef.tools.NullOrEmpty())
            {
                original_Tools = thingDef.tools.ToList();
            }
            Log.Warning($"Weapon {def.defName} has {original_Tools.Count} after GetOriginalData");
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

            Log.Warning($"Weapon {def.defName} has {modified_toolIds.Count} after AutoCalculate");
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

            Log.Warning($"Weapon {def.defName} has {modified_Tools.Count} after Patch");
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
            base.ExposeData();
            if (Scribe.mode == LoadSaveMode.LoadingVars
                || (Scribe.mode == LoadSaveMode.Saving && isCustomized == true))
            {
                Scribe_Values.Look(ref modified_mass, "modified_Mass", original_Mass);
                Scribe_Values.Look(ref modified_bulk, "modified_Bulk", 1f);
                Scribe_Values.Look(ref modified_weaponToughness, "modified_weaponToughness", 1f);
                Scribe_Values.Look(ref modified_MeleeCounterParryBonus, "modified_MeleeCounterParryBonus", 0f);
                Scribe_Values.Look(ref modified_MeleeDodgeChance, "modified_MeleeDodgeChance", 0f);
                Scribe_Values.Look(ref modified_MeleeParryChance, "modified_MeleeParryChance", 0f);
                Scribe_Values.Look(ref modified_MeleeCritChance, "modified_MeleeCritChance", 0f);
            }
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
            modified_ToolArmorPenetrationSharps[i] = Mathf.Clamp(modified_ToolArmorPenetrationSharps[i] * modData.weaponToolSharpPenetration * techMult, 0, 99999);
            modified_ToolArmorPenetrationBlunts[i] = Mathf.Clamp(modified_ToolArmorPenetrationSharps[i] * modData.weaponToolBluntPenetration * techMult, 0, 99999);
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
