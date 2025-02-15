using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using UnityEngine;

namespace nuff.AutoPatcherCombatExtended
{
    class Window_CustomizeDefGene : Window_CustomizeDef
    {
        DefDataHolderGene dataHolder;

        public Window_CustomizeDefGene(DefDataHolder defDataHolder) : base(defDataHolder)
        {
        }

        public override void CastDataHolder(DefDataHolder defDataHolder)
        {
            dataHolder = defDataHolder as DefDataHolderGene;
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

            list.End();
        }
    }
}
