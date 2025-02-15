using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using UnityEngine;

namespace nuff.AutoPatcherCombatExtended
{
    class Window_CustomizeDefPawnKind : Window_CustomizeDef
    {
        DefDataHolderPawnKind dataHolder;

        public Window_CustomizeDefPawnKind(DefDataHolder defDataHolder) : base(defDataHolder)
        {
        }

        public override void CastDataHolder(DefDataHolder defDataHolder)
        {
            dataHolder = defDataHolder as DefDataHolderPawnKind;
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

            string modified_MinMagsBuffer = dataHolder.modified_MinMags.ToString();
            list.TextFieldNumericLabeled("Min Mags", ref dataHolder.modified_MinMags, ref modified_MinMagsBuffer);

            string modified_MaxMagsBuffer = dataHolder.modified_MaxMags.ToString();
            list.TextFieldNumericLabeled("Max Mags", ref dataHolder.modified_MaxMags, ref modified_MaxMagsBuffer);

            //TODO modified_ApparelTags
            //TODO modified_WeaponTags

            list.End();
        }
    }
}
