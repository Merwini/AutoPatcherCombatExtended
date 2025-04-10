using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using UnityEngine;

namespace nuff.AutoPatcherCombatExtended
{
    class Window_CustomizeDefMeleeWeapon : Window_CustomizeDef
    {
        DefDataHolderMeleeWeapon dataHolder;

        private Vector2 scrollPosition = Vector2.zero;

        public Window_CustomizeDefMeleeWeapon(DefDataHolder defDataHolder) : base(defDataHolder)
        {
        }

        public override void CastDataHolder(DefDataHolder defDataHolder)
        {
            dataHolder = defDataHolder as DefDataHolderMeleeWeapon;
        }


        public override void DoWindowContents(Rect inRect)
        {
            base.DoWindowContents(inRect);

            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(0f, 0f, inRect.width - 150f - 17f, 35f), $"{dataHolder.def.label} - {dataHolder.def.defName}");
            Text.Font = GameFont.Small;

            Rect outerRect = new Rect(inRect.x, inRect.y + 45f, inRect.width, inRect.height - 60f);

            // Dynamically calculate view height
            float lineHeight = 30f;
            float estimatedLines = 20 + dataHolder.modified_Tools.Count * 20 + dataHolder.modified_ToolCapacityDefs.Count * 10; // tweak multiplier based on number of fields per tool
            float viewHeight = estimatedLines * lineHeight;

            Rect viewRect = new Rect(0f, 0f, outerRect.width - 16f, viewHeight);

            Widgets.BeginScrollView(outerRect, ref scrollPosition, viewRect);
            Listing_Standard list = new Listing_Standard();
            list.Begin(viewRect);

            string buffer;

            buffer = dataHolder.modified_Mass.ToString();
            list.TextFieldNumericLabeled("Mass", ref dataHolder.modified_Mass, ref buffer);

            buffer = dataHolder.modified_Bulk.ToString();
            list.TextFieldNumericLabeled("Bulk", ref dataHolder.modified_Bulk, ref buffer);

            buffer = dataHolder.modified_WeaponToughness.ToString();
            list.TextFieldNumericLabeled("Weapon Toughness", ref dataHolder.modified_WeaponToughness, ref buffer);

            buffer = dataHolder.modified_MeleeCounterParryBonus.ToString();
            list.TextFieldNumericLabeled("Melee Counter Parry Bonus", ref dataHolder.modified_MeleeCounterParryBonus, ref buffer);

            buffer = dataHolder.modified_MeleeDodgeChance.ToString();
            list.TextFieldNumericLabeled("Melee Dodge Chance", ref dataHolder.modified_MeleeDodgeChance, ref buffer);

            buffer = dataHolder.modified_MeleeParryChance.ToString();
            list.TextFieldNumericLabeled("Melee Parry Chance", ref dataHolder.modified_MeleeParryChance, ref buffer);

            buffer = dataHolder.modified_MeleeCritChance.ToString();
            list.TextFieldNumericLabeled("Melee Crit Chance", ref dataHolder.modified_MeleeCritChance, ref buffer);

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
                list.Gap(20f);

                string modified_ToolCooldownTimesBuffer = dataHolder.modified_ToolCooldownTimes[i].ToString();
                float cooldowns = dataHolder.modified_ToolCooldownTimes[i];
                list.TextFieldNumericLabeled($"Tool {i + 1} cooldown time: ", ref cooldowns, ref modified_ToolCooldownTimesBuffer);
                dataHolder.modified_ToolCooldownTimes[i] = cooldowns;
                list.Gap(20f);

                string modified_ToolArmorPenetrationSharpsBuffer = dataHolder.modified_ToolArmorPenetrationSharps[i].ToString();
                float apsharps = dataHolder.modified_ToolArmorPenetrationSharps[i];
                list.TextFieldNumericLabeled($"Tool {i + 1} sharp penetration: ", ref apsharps, ref modified_ToolArmorPenetrationSharpsBuffer);
                dataHolder.modified_ToolArmorPenetrationSharps[i] = apsharps;
                list.Gap(20f);

                string modified_ToolArmorPenetrationBluntsBuffer = dataHolder.modified_ToolArmorPenetrationBlunts[i].ToString();
                float apblunts = dataHolder.modified_ToolArmorPenetrationBlunts[i];
                list.TextFieldNumericLabeled($"Tool {i + 1} blunt penetration: ", ref apblunts, ref modified_ToolArmorPenetrationBluntsBuffer);
                dataHolder.modified_ToolArmorPenetrationBlunts[i] = apblunts;
                list.Gap(20f);

                string modified_ToolChanceFactorsBuffer = dataHolder.modified_ToolChanceFactors[i].ToString();
                float chances = dataHolder.modified_ToolChanceFactors[i];
                list.TextFieldNumericLabeled($"Tool {i + 1} chance factor: ", ref chances, ref modified_ToolChanceFactorsBuffer);
                dataHolder.modified_ToolChanceFactors[i] = chances;
                list.Gap(20f);

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

                list.Gap(10f); // Add spacing between capacity buttons and tool remove

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


        /*
        public override void DoWindowContents(Rect inRect)
        {
            base.DoWindowContents(inRect);
            Listing_Standard list = new Listing_Standard();
            list.Begin(inRect);
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(0f, 0f, inRect.width - 150f - 17f, 35f), $"{dataHolder.def.label} - {dataHolder.def.defName}");
            Text.Font = GameFont.Small;
            list.Gap(45);

            Rect outerRect = new Rect(inRect.x, inRect.y + 45f, inRect.width, inRect.height - 145f);
            Rect innerRect = outerRect.ContractedBy(10f);
            float scrollHeight = 9999f; //TODO make dynamic
            Rect viewRect = new Rect(0, 0, innerRect.width - 16f, scrollHeight);

            // Begin measuring scroll height
            Widgets.BeginScrollView(innerRect, ref scrollPosition, viewRect);
            list.Begin(viewRect);

            string modified_MassBuffer = dataHolder.modified_mass.ToString();
            list.TextFieldNumericLabeled("Mass", ref dataHolder.modified_mass, ref modified_MassBuffer);

            string modified_BulkBuffer = dataHolder.modified_bulk.ToString();
            list.TextFieldNumericLabeled("Bulk", ref dataHolder.modified_bulk, ref modified_BulkBuffer);

            string modified_WeaponToughnessBuffer = dataHolder.modified_weaponToughness.ToString();
            list.TextFieldNumericLabeled("Weapon Toughness", ref dataHolder.modified_weaponToughness, ref modified_WeaponToughnessBuffer);

            string modified_MeleeCounterParryBonusBuffer = dataHolder.modified_MeleeCounterParryBonus.ToString();
            list.TextFieldNumericLabeled("Melee Counter Parry Bonus", ref dataHolder.modified_MeleeCounterParryBonus, ref modified_MeleeCounterParryBonusBuffer);

            string modified_MeleeDodgeChanceBuffer = dataHolder.modified_MeleeDodgeChance.ToString();
            list.TextFieldNumericLabeled("Melee Dodge Chance", ref dataHolder.modified_MeleeDodgeChance, ref modified_MeleeDodgeChanceBuffer);

            string modified_MeleeParryChanceBuffer = dataHolder.modified_MeleeParryChance.ToString();
            list.TextFieldNumericLabeled("Melee Parry Chance", ref dataHolder.modified_MeleeParryChance, ref modified_MeleeParryChanceBuffer);

            string modified_MeleeCritChanceBuffer = dataHolder.modified_MeleeCritChance.ToString();
            list.TextFieldNumericLabeled("Melee Crit Chance", ref dataHolder.modified_MeleeCritChance, ref modified_MeleeCritChanceBuffer);

            for (int i = 0; i < dataHolder.modified_Tools.Count; i++)
            {
                float boxHeight = 500f;
                float spacing = 10f;

                Rect boxRect = new Rect(inRect.x, list.CurHeight, inRect.width - 30f, boxHeight + (dataHolder.modified_Tools.Count * 150f));

                GUI.BeginGroup(boxRect, GUI.skin.box);

                Listing_Standard toolList = new Listing_Standard();
                toolList.Begin(new Rect(10f, 10f, boxRect.width - 20f, boxRect.height - 20));

                if (!dataHolder.modified_Tools.NullOrEmpty())
                {
                    toolList.Gap(35f);

                    string id = dataHolder.modified_toolIds[i];
                    toolList.TextEntryLabeled($"Tool {i + 1} ID: ", id);
                    dataHolder.modified_toolIds[i] = id;

                    string modified_toolPowersBuffer = dataHolder.modified_ToolPowers[i].ToString();
                    float powers = dataHolder.modified_ToolPowers[i];
                    toolList.TextFieldNumericLabeled($"Tool {i + 1} power (damage): ", ref powers, ref modified_toolPowersBuffer);
                    dataHolder.modified_ToolPowers[i] = powers;
                    toolList.Gap(20f);

                    string modified_ToolCooldownTimesBuffer = dataHolder.modified_ToolCooldownTimes[i].ToString();
                    float cooldowns = dataHolder.modified_ToolCooldownTimes[i];
                    toolList.TextFieldNumericLabeled($"Tool {i + 1} cooldown time: ", ref cooldowns, ref modified_ToolCooldownTimesBuffer);
                    dataHolder.modified_ToolCooldownTimes[i] = cooldowns;
                    toolList.Gap(20f);

                    string modified_ToolArmorPenetrationSharpsBuffer = dataHolder.modified_ToolArmorPenetrationSharps[i].ToString();
                    float apsharps = dataHolder.modified_ToolArmorPenetrationSharps[i];
                    toolList.TextFieldNumericLabeled($"Tool {i + 1} sharp penetration: ", ref apsharps, ref modified_ToolArmorPenetrationSharpsBuffer);
                    dataHolder.modified_ToolArmorPenetrationSharps[i] = apsharps;
                    toolList.Gap(20f);

                    string modified_ToolArmorPenetrationBluntsBuffer = dataHolder.modified_ToolArmorPenetrationBlunts[i].ToString();
                    float apblunts = dataHolder.modified_ToolArmorPenetrationBlunts[i];
                    toolList.TextFieldNumericLabeled($"Tool {i + 1} blunt penetration: ", ref apblunts, ref modified_ToolArmorPenetrationBluntsBuffer);
                    dataHolder.modified_ToolArmorPenetrationBlunts[i] = apblunts;
                    toolList.Gap(20f);

                    string modified_ToolChanceFactorsBuffer = dataHolder.modified_ToolChanceFactors[i].ToString();
                    float chances = dataHolder.modified_ToolChanceFactors[i];
                    toolList.TextFieldNumericLabeled($"Tool {i + 1} chance factor: ", ref chances, ref modified_ToolChanceFactorsBuffer);
                    dataHolder.modified_ToolChanceFactors[i] = chances;
                    toolList.Gap(20f);

                    toolList.Label("Linked Body Part Group:");
                    if (Widgets.ButtonText(new Rect(boxRect.width / 2 - 100, toolList.curY, 200f, 30f), dataHolder.modified_ToolLinkedBodyPartsGroupDefs[i]?.defName ?? "null"))
                    {
                        Find.WindowStack.Add(new Window_SelectLinkedBodyPartGroupDef(dataHolder.modified_ToolLinkedBodyPartsGroupDefs, i));
                    }

                    toolList.Label("Tool Capacities:");
                    for (int j = 0; j < dataHolder.modified_ToolCapacityDefs[i].Count; j++)
                    {
                        Rect secondaryRect = new Rect(0, toolList.CurHeight, boxRect.width - 30f, 150f);

                        GUI.BeginGroup(secondaryRect, GUI.skin.box);

                        Listing_Standard secondaryList = new Listing_Standard();
                        secondaryList.Begin(new Rect(10f, 10f, secondaryRect.width - 20f, secondaryRect.height - 20));

                        if (!dataHolder.modified_ToolCapacityDefs[i].NullOrEmpty())
                        {
                            if (Widgets.ButtonText(new Rect(boxRect.width / 2 - 100, toolList.curY, 200f, 30f), dataHolder.modified_ToolCapacityDefs[i][j].defName))
                            {
                                Find.WindowStack.Add(new Window_SelectToolCapacityDef(dataHolder.modified_ToolCapacityDefs[i], j));
                            }
                        }

                        if (Widgets.ButtonText(new Rect(boxRect.width / 2 - 100, toolList.curY, 200f, 30f), "REMOVE TOOL CAPACITY"))
                        {
                            Find.WindowStack.Add(new Window_ConfirmDeleteCapacity(dataHolder, i, j));
                        }

                        toolList.Gap(boxRect.height + 10f);
                    }

                    if (Widgets.ButtonText(new Rect(boxRect.width / 2 - 100, toolList.curY, 200f, 30f), "REMOVE TOOL"))
                    {
                        Find.WindowStack.Add(new Window_ConfirmDeleteTool(dataHolder, i));
                    }

                    toolList.End();
                    GUI.EndGroup();
                }

                list.End();

                scrollHeight = list.CurHeight + 20f;
                viewRect.height = scrollHeight;

                Widgets.EndScrollView();
            }
        }
        */
    }
}
