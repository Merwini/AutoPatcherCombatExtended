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
    class DefDataHolderMeleeWeapon : DefDataHolder
    {
        public DefDataHolderMeleeWeapon(ThingDef def) : base(def)
        {
        }

        internal ThingDef thingDef;

        float original_Mass;
        float meleeToolTechMult;

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

            for (int i = 0; i < original_Tools.Count; i++)
            {
                modified_Tools.Add(MakeToolMelee(original_Tools[i]));
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

            DataHolderUtils.AddOrChangeStat(thingDef.statBases, CE_StatDefOf.Bulk, modified_Bulk);
            DataHolderUtils.AddOrChangeStat(thingDef.statBases, CE_StatDefOf.MeleeCounterParryBonus, modified_MeleeCounterParryBonus);

            DataHolderUtils.AddOrChangeStat(thingDef.equippedStatOffsets, CE_StatDefOf.MeleeDodgeChance, modified_MeleeDodgeChance);
            DataHolderUtils.AddOrChangeStat(thingDef.equippedStatOffsets, CE_StatDefOf.MeleeParryChance, modified_MeleeParryChance);
            DataHolderUtils.AddOrChangeStat(thingDef.equippedStatOffsets, CE_StatDefOf.MeleeCritChance, modified_MeleeCritChance);

            thingDef.tools.Clear();
            for (int i = 0; i < modified_Tools.Count; i++)
            {
                thingDef.tools.Add(modified_Tools[i]);
            }
            //TODO this is where I left off
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

        private ToolCE MakeToolMelee(Tool tool)
        {
            ToolCE newToolCE = DataHolderUtils.MakeToolBase(tool);
            newToolCE.power *= modData.weaponToolPowerMult;
            newToolCE.armorPenetrationSharp *= modData.weaponToolSharpPenetration * meleeToolTechMult;
            newToolCE.armorPenetrationBlunt *= modData.weaponToolBluntPenetration * meleeToolTechMult;

            return newToolCE;
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

            meleeToolTechMult = techMult;
        }
    }
}
