using CombatExtended;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using Verse;
using UnityEngine;

namespace nuff.AutoPatcherCombatExtended
{
    public class Window_CustomizeDefApparel : Window_CustomizeDef
    {
        DefDataHolderApparel dataHolder;

        public Window_CustomizeDefApparel(DefDataHolder defDataHolder) : base(defDataHolder)
        {
        }

        public override void CastDataHolder(DefDataHolder defDataHolder)
        {
            dataHolder = defDataHolder as DefDataHolderApparel;
        }

        public override void DoWindowContents(Rect inRect)
        {
            base.DoWindowContents(inRect);
            Listing_Standard list = new Listing_Standard();
            list.Begin(inRect);
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(0f, 0f, inRect.width - 150f - 17f, 35f), $"{dataHolder.def.label} - {dataHolder.def.defName}");
            Text.Font = GameFont.Small;
            list.Gap(45);

            string modified_MassBuffer = dataHolder.modified_Mass.ToString();
            list.TextFieldNumericLabeled("Mass", ref dataHolder.modified_Mass, ref modified_MassBuffer);

            string modified_BulkBuffer = dataHolder.modified_Bulk.ToString();
            list.TextFieldNumericLabeled("Carried Bulk", ref dataHolder.modified_Bulk, ref modified_BulkBuffer);

            string modified_WornBulkBuffer = dataHolder.modified_WornBulk.ToString();
            list.TextFieldNumericLabeled("Worn Bulk", ref dataHolder.modified_WornBulk, ref modified_WornBulkBuffer);

            string modified_ArmorRatingSharpBuffer = dataHolder.modified_ArmorRatingSharp.ToString();
            list.TextFieldNumericLabeled("Sharp Armor Rating ", ref dataHolder.modified_ArmorRatingSharp, ref modified_ArmorRatingSharpBuffer);

            string modified_ArmorRatingBluntBuffer = dataHolder.modified_ArmorRatingBlunt.ToString();
            list.TextFieldNumericLabeled("Blunt Armor Rating", ref dataHolder.modified_ArmorRatingBlunt, ref modified_ArmorRatingBluntBuffer);

            string modified_ArmorRatingHeatBuffer = dataHolder.modified_ArmorRatingHeat.ToString();
            list.TextFieldNumericLabeled("Heat Armor Rating", ref dataHolder.modified_ArmorRatingHeat, ref modified_ArmorRatingHeatBuffer);

            string modified_MaxHitPointsBuffer = dataHolder.modified_MaxHitPoints.ToString();
            list.TextFieldNumericLabeled("Max Hit Points", ref dataHolder.modified_MaxHitPoints, ref modified_MaxHitPointsBuffer);

            if (!dataHolder.thingDef.stuffCategories.NullOrEmpty())
            {
                string modified_StuffEffectMultiplierArmorBuffer = dataHolder.modified_StuffEffectMultiplierArmor.ToString();
                list.TextFieldNumericLabeled("Stuff Effect Multiplier", ref dataHolder.modified_StuffEffectMultiplierArmor, ref modified_StuffEffectMultiplierArmorBuffer);
            }

            string modified_CarryWeightBuffer = dataHolder.modified_CarryWeight.ToString();
            list.TextFieldNumericLabeled("Carry Weight Offset (load-bearing gear)", ref dataHolder.modified_CarryWeight, ref modified_CarryWeightBuffer);

            string modified_CarryBulkBuffer = dataHolder.modified_CarryBulk.ToString();
            list.TextFieldNumericLabeled("Carry Bulk Offset (load-bearing gear)", ref dataHolder.modified_CarryBulk, ref modified_CarryBulkBuffer);

            string modified_SmokeSensitivityBuffer = dataHolder.modified_SmokeSensitivity.ToString();
            list.TextFieldNumericLabeled("Smoke Sensitivity Offset", ref dataHolder.modified_SmokeSensitivity, ref modified_SmokeSensitivityBuffer);

            string modified_NightVisionEfficiencyBuffer = dataHolder.modified_NightVisionEfficiency.ToString();
            list.TextFieldNumericLabeled("Night Vision Efficiency", ref dataHolder.modified_NightVisionEfficiency, ref modified_NightVisionEfficiencyBuffer);

            string modified_ShootingAccuracyPawnBuffer = dataHolder.modified_ShootingAccuracyPawn.ToString();
            list.TextFieldNumericLabeled("Pawn Shooting Accuracy Offset", ref dataHolder.modified_ShootingAccuracyPawn, ref modified_ShootingAccuracyPawnBuffer);
            
            list.End();
        }
    }
}