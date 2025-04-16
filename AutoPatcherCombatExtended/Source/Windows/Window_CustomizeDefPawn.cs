using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using UnityEngine;

namespace nuff.AutoPatcherCombatExtended
{
    class Window_CustomizeDefPawn : Window_CustomizeDef
    {
        DefDataHolderPawn dataHolder;

        private Vector2 scrollPosition = Vector2.zero;

        public Window_CustomizeDefPawn(DefDataHolder defDataHolder) : base(defDataHolder)
        {
        }

        public override void CastDataHolder(DefDataHolder defDataHolder)
        {
            dataHolder = defDataHolder as DefDataHolderPawn;
        }

        public override void DoWindowContents(Rect inRect)
        {
            base.DoWindowContents(inRect);

            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(0f, 0f, inRect.width - 150f - 17f, 35f), $"{dataHolder.def.label} - {dataHolder.def.defName}");
            Text.Font = GameFont.Small;

            Rect outerRect = new Rect(inRect.x, inRect.y + 45f, inRect.width, inRect.height - 60f);

            float lineHeight = 30f;
            float estimatedLines = 20 + dataHolder.modified_Tools.Count * 20 + dataHolder.modified_ToolCapacityDefs.Count * 10;
            float viewHeight = estimatedLines * lineHeight * 2;

            Rect viewRect = new Rect(0f, 0f, outerRect.width - 16f, viewHeight);

            Widgets.BeginScrollView(outerRect, ref scrollPosition, viewRect);
            Listing_Standard list = new Listing_Standard();
            list.Begin(viewRect);
            list.Gap(45);

            string modified_ArmorRatingSharpBuffer = dataHolder.modified_ArmorRatingSharp.ToString();
            list.TextFieldNumericLabeled("Armor Rating (Sharp)", ref dataHolder.modified_ArmorRatingSharp, ref modified_ArmorRatingSharpBuffer);

            string modified_ArmorRatingBluntBuffer = dataHolder.modified_ArmorRatingBlunt.ToString();
            list.TextFieldNumericLabeled("Armor Rating (Blunt)", ref dataHolder.modified_ArmorRatingBlunt, ref modified_ArmorRatingBluntBuffer);

            string modified_ArmorRatingHeatBuffer = dataHolder.modified_ArmorRatingHeat.ToString();
            list.TextFieldNumericLabeled("Armor Rating (Heat)", ref dataHolder.modified_ArmorRatingHeat, ref modified_ArmorRatingHeatBuffer);

            string modified_MeleeDodgeChanceBuffer = dataHolder.modified_MeleeDodgeChance.ToString();
            list.TextFieldNumericLabeled("Melee Dodge Chance", ref dataHolder.modified_MeleeDodgeChance, ref modified_MeleeDodgeChanceBuffer);

            string modified_MeleeParryChanceBuffer = dataHolder.modified_MeleeParryChance.ToString();
            list.TextFieldNumericLabeled("Melee Parry Chance", ref dataHolder.modified_MeleeParryChance, ref modified_MeleeParryChanceBuffer);

            string modified_MeleeCritChanceBuffer = dataHolder.modified_MeleeCritChance.ToString();
            list.TextFieldNumericLabeled("Melee Crit Chance", ref dataHolder.modified_MeleeCritChance, ref modified_MeleeCritChanceBuffer);

            string modified_SmokeSensitivityBuffer = dataHolder.modified_SmokeSensitivity.ToString();
            list.TextFieldNumericLabeled("Smoke Sensitivity", ref dataHolder.modified_SmokeSensitivity, ref modified_SmokeSensitivityBuffer);

            string modified_SuppressabilityBuffer = dataHolder.modified_Suppressability.ToString();
            list.TextFieldNumericLabeled("Suppressability", ref dataHolder.modified_Suppressability, ref modified_SuppressabilityBuffer);

            string modified_NightVisionEfficiencyBuffer = dataHolder.modified_NightVisionEfficiency.ToString();
            list.TextFieldNumericLabeled("Night Vision Efficiency", ref dataHolder.modified_NightVisionEfficiency, ref modified_NightVisionEfficiencyBuffer);

            string modified_ReloadSpeedBuffer = dataHolder.modified_ReloadSpeed.ToString();
            list.TextFieldNumericLabeled("Reload Speed", ref dataHolder.modified_ReloadSpeed, ref modified_ReloadSpeedBuffer);

            string modified_AimingAccuracyBuffer = dataHolder.modified_AimingAccuracy.ToString();
            list.TextFieldNumericLabeled("Aiming Accuracy", ref dataHolder.modified_AimingAccuracy, ref modified_AimingAccuracyBuffer);

            string modified_CarryWeightBuffer = dataHolder.modified_CarryWeight.ToString();
            list.TextFieldNumericLabeled("Carry Weight", ref dataHolder.modified_CarryWeight, ref modified_CarryWeightBuffer);

            string modified_CarryBulkBuffer = dataHolder.modified_CarryBulk.ToString();
            list.TextFieldNumericLabeled("Carry Bulk", ref dataHolder.modified_CarryBulk, ref modified_CarryBulkBuffer);
            
            //TODO modified_BodyShape

            //TODO covered by natural armor
            
            list.Gap(30f);

            for (int i = 0; i < dataHolder.modified_ToolPowers.Count; i++)
            {
                list.Label($"Tool {i + 1}");
                list.Gap(10f);

                string id = dataHolder.modified_ToolIds[i];
                list.TextEntryLabeled($"Tool {i + 1} ID (only matters for autopatching, not exported patches): ", id);
                dataHolder.modified_ToolIds[i] = id;

                string labels = dataHolder.modified_ToolLabels[i];
                list.TextEntryLabeled($"Tool {i + 1} label: ", labels);
                dataHolder.modified_ToolLabels[i] = labels;

                string modified_toolPowersBuffer = dataHolder.modified_ToolPowers[i].ToString();
                float powers = dataHolder.modified_ToolPowers[i];
                list.TextFieldNumericLabeled($"Tool {i + 1} power (damage): ", ref powers, ref modified_toolPowersBuffer);
                dataHolder.modified_ToolPowers[i] = powers;

                string modified_ToolCooldownTimesBuffer = dataHolder.modified_ToolCooldownTimes[i].ToString();
                float cooldowns = dataHolder.modified_ToolCooldownTimes[i];
                list.TextFieldNumericLabeled($"Tool {i + 1} cooldown time: ", ref cooldowns, ref modified_ToolCooldownTimesBuffer);
                dataHolder.modified_ToolCooldownTimes[i] = cooldowns;

                string modified_ToolArmorPenetrationSharpsBuffer = dataHolder.modified_ToolArmorPenetrationSharps[i].ToString();
                float apsharps = dataHolder.modified_ToolArmorPenetrationSharps[i];
                list.TextFieldNumericLabeled($"Tool {i + 1} sharp penetration: ", ref apsharps, ref modified_ToolArmorPenetrationSharpsBuffer);
                dataHolder.modified_ToolArmorPenetrationSharps[i] = apsharps;

                string modified_ToolArmorPenetrationBluntsBuffer = dataHolder.modified_ToolArmorPenetrationBlunts[i].ToString();
                float apblunts = dataHolder.modified_ToolArmorPenetrationBlunts[i];
                list.TextFieldNumericLabeled($"Tool {i + 1} blunt penetration: ", ref apblunts, ref modified_ToolArmorPenetrationBluntsBuffer);
                dataHolder.modified_ToolArmorPenetrationBlunts[i] = apblunts;

                string modified_ToolChanceFactorsBuffer = dataHolder.modified_ToolChanceFactors[i].ToString();
                float chances = dataHolder.modified_ToolChanceFactors[i];
                list.TextFieldNumericLabeled($"Tool {i + 1} chance factor: ", ref chances, ref modified_ToolChanceFactorsBuffer);
                dataHolder.modified_ToolChanceFactors[i] = chances;

                list.Label("Linked Body Part Group:");
                if (Widgets.ButtonText(list.GetRect(30f), dataHolder.modified_ToolLinkedBodyPartGroupDefs[i]?.defName ?? "null"))
                {
                    Find.WindowStack.Add(new Window_SelectLinkedBodyPartGroupDef(dataHolder.modified_ToolLinkedBodyPartGroupDefs, i));
                }

                list.Label("Tool Capacities:");
                for (int j = 0; j < dataHolder.modified_ToolCapacityDefs[i].Count; j++)
                {
                    list.Gap(5f);
                    var cap = dataHolder.modified_ToolCapacityDefs[i][j];

                    Rect capacityButtonRow = list.GetRect(30f);
                    float halfWidth = capacityButtonRow.width / 2f;

                    Rect selectButtonRect = new Rect(capacityButtonRow.x, capacityButtonRow.y, halfWidth - 5f, capacityButtonRow.height);
                    Rect removeButtonRect = new Rect(capacityButtonRow.x + halfWidth + 5f, capacityButtonRow.y, halfWidth - 5f, capacityButtonRow.height);

                    if (Widgets.ButtonText(selectButtonRect, cap.defName))
                    {
                        Find.WindowStack.Add(new Window_SelectToolCapacityDef(dataHolder.modified_ToolCapacityDefs[i], j));
                    }

                    if (Widgets.ButtonText(removeButtonRect, "REMOVE"))
                    {
                        Find.WindowStack.Add(new Window_ConfirmDeleteCapacity(dataHolder, i, j));
                    }

                    if (Widgets.ButtonText(list.GetRect(30f), "ADD ANOTHER TOOL CAPACITY"))
                    {
                        dataHolder.AddNewToolCapacity(i);
                    }
                }

                list.Gap(10f);

                if (Widgets.ButtonText(list.GetRect(30f), "REMOVE TOOL"))
                {
                    Find.WindowStack.Add(new Window_ConfirmDeleteTool(dataHolder, i));
                }

                list.Gap(40f);
            }

            if (Widgets.ButtonText(list.GetRect(30f), "ADD ANOTHER TOOL"))
            {
                dataHolder.AddNewTool();
            }

            list.End();
            Widgets.EndScrollView();
        }
    }
}
