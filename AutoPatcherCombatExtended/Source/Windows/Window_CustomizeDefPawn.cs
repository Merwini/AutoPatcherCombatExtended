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
            Listing_Standard list = new Listing_Standard();
            list.Begin(inRect);
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(0f, 0f, inRect.width - 150f - 17f, 35f), dataHolder.def.label);
            Text.Font = GameFont.Small;
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

            list.End();
        }
    }
}
