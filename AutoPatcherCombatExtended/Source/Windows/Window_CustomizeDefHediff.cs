using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using UnityEngine;

namespace nuff.AutoPatcherCombatExtended
{
    class Window_CustomizeDefHediff : Window_CustomizeDef
    {
        DefDataHolderHediff dataHolder;

        public Window_CustomizeDefHediff(DefDataHolder defDataHolder) : base(defDataHolder)
        {
        }

        public override void CastDataHolder(DefDataHolder defDataHolder)
        {
            dataHolder = defDataHolder as DefDataHolderHediff;
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

            if (dataHolder.modified_ArmorRatingSharp.Count == 0)
            {
                list.Label("No armor values on hediff. Probably was added due to verbs. Verb customization is a work in progress");
            }

            for (int i = 0; i < dataHolder.modified_ArmorRatingSharp.Count; i++)
            {
                string labelPrefix = $"Stage {i + 1}: ";

                float sharpValue = dataHolder.modified_ArmorRatingSharp[i];
                string sharpBuffer = sharpValue.ToString();
                list.TextFieldNumericLabeled(labelPrefix + "Armor Rating (Sharp)", ref sharpValue, ref sharpBuffer);
                dataHolder.modified_ArmorRatingSharp[i] = sharpValue;

                float bluntValue = dataHolder.modified_ArmorRatingBlunt[i];
                string bluntBuffer = bluntValue.ToString();
                list.TextFieldNumericLabeled(labelPrefix + "Armor Rating (Blunt)", ref bluntValue, ref bluntBuffer);
                dataHolder.modified_ArmorRatingBlunt[i] = bluntValue;

                float heatValue = dataHolder.modified_ArmorRatingHeat[i];
                string heatBuffer = heatValue.ToString();
                list.TextFieldNumericLabeled(labelPrefix + "Armor Rating (Heat)", ref heatValue, ref heatBuffer);
                dataHolder.modified_ArmorRatingHeat[i] = heatValue;
            }

            list.End();
        }
    }
}
