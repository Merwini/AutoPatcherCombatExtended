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
        public DefDataHolderMeleeWeapon(ThingDef def) : base(def)
        {
        }

        public ThingDef thingDef;

        float original_Mass;

        float modified_Mass;
        float modified_Bulk;
        float modified_MeleeCounterParryBonus; //reminder - statbase
        float modified_MeleeDodgeChance;
        float modified_MeleeParryChance;
        float modified_MeleeCritChance;

        //TODO
        public override void GetOriginalData()
        {
            thingDef = def as ThingDef;

            original_Tools = thingDef.tools;
            original_Mass = thingDef.statBases.GetStatValueFromList(StatDefOf.Mass, 0);
        }

        public override void AutoCalculate()
        {
            modified_Mass = original_Mass;
            modified_Bulk = modified_Mass * 2f;
            CalculateStatMods();
            CalculateWeaponTechMult();

            for (int i = 0; i < original_Tools.Count; i++)
            {
                ModToolAtIndex(i);
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

            DataHolderUtils.AddOrChangeStat(thingDef.statBases, StatDefOf.Mass, modified_Mass);
            DataHolderUtils.AddOrChangeStat(thingDef.statBases, CE_StatDefOf.Bulk, modified_Bulk);
            DataHolderUtils.AddOrChangeStat(thingDef.statBases, CE_StatDefOf.MeleeCounterParryBonus, modified_MeleeCounterParryBonus);

            DataHolderUtils.AddOrChangeStat(thingDef.equippedStatOffsets, CE_StatDefOf.MeleeDodgeChance, modified_MeleeDodgeChance);
            DataHolderUtils.AddOrChangeStat(thingDef.equippedStatOffsets, CE_StatDefOf.MeleeParryChance, modified_MeleeParryChance);
            DataHolderUtils.AddOrChangeStat(thingDef.equippedStatOffsets, CE_StatDefOf.MeleeCritChance, modified_MeleeCritChance);

            thingDef.tools.Clear();
            BuildTools();
            for (int i = 0; i < modified_Tools.Count; i++)
            {
                thingDef.tools.Add(modified_Tools[i]);
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
            base.ExposeData();
            if (Scribe.mode == LoadSaveMode.LoadingVars
                || (Scribe.mode == LoadSaveMode.Saving && isCustomized == true))
            {
                Scribe_Values.Look(ref modified_Mass, "modified_Mass", 0f);
                Scribe_Values.Look(ref modified_Bulk, "modified_Bulk", 0f);
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
            float massFactor = Mathf.Clamp((modified_Mass * 0.2f), 0.1f, 0.9f);

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
            modified_ToolArmorPenetrationSharps[i] *= modData.weaponToolSharpPenetration * techMult;
            modified_ToolArmorPenetrationBlunts[i] *= modData.weaponToolBluntPenetration * techMult;
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
