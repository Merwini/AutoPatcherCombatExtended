    using CombatExtended;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using Verse;

namespace nuff.AutoPatcherCombatExtended
{
    partial class APCEController
    {
        internal static void PatchMeleeWeapon(ThingDef def, APCEPatchLogger log)
        {
            PatchAllTools(ref def.tools, false);

            AddMeleeWeaponStats(def);
        }
        internal static void AddMeleeWeaponStats(ThingDef weapon)
        {
            float mass = weapon.statBases.GetStatValueFromList(StatDefOf.Mass, 1);
            float massFactor = Mathf.Clamp((mass * 0.2f), 0.1f, 0.9f);
            float value = weapon.BaseMarketValue;
            float valueLog = (float)Math.Log10(value);
            float sumCritParry = valueLog / 2f;

            if (weapon.equippedStatOffsets == null)
            {
                weapon.equippedStatOffsets = new List<StatModifier>();
            }
            if (weapon.statBases == null)
            {
                weapon.statBases = new List<StatModifier>();
            }

            StatModifier bulk = new StatModifier();
            bulk.stat = CE_StatDefOf.Bulk;
            bulk.value = mass * 2f;

            StatModifier meleeCounterParryBonus = new StatModifier();
            meleeCounterParryBonus.stat = CE_StatDefOf.MeleeCounterParryBonus;
            meleeCounterParryBonus.value = 0.1f * valueLog;

            StatModifier meleeDodgeChance = new StatModifier();
            meleeDodgeChance.stat = CE_StatDefOf.MeleeDodgeChance;
            meleeDodgeChance.value = 0.2f; //TODO formula

            StatModifier meleeParryChance = new StatModifier();
            meleeParryChance.stat = CE_StatDefOf.MeleeParryChance;
            meleeParryChance.value = valueLog * massFactor;

            StatModifier meleeCritChance = new StatModifier();
            meleeCritChance.stat = CE_StatDefOf.MeleeCritChance;
            meleeCritChance.value = valueLog * (1 - massFactor);

            weapon.AddOrChangeStat(meleeCounterParryBonus);
            weapon.AddOrChangeStat(bulk);

            weapon.equippedStatOffsets.Add(meleeDodgeChance);
            weapon.equippedStatOffsets.Add(meleeCritChance);
            weapon.equippedStatOffsets.Add(meleeParryChance);

            //weapon.statBases.Add(meleeDodgeChance);
            //weapon.statBases.Add(meleeCritChance);
            //weapon.statBases.Add(meleeParryChance);
        }
    }

    
}