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
            Listing_Standard list = new Listing_Standard();
            list.Begin(inRect);
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(0f, 0f, inRect.width - 150f - 17f, 35f), $"{dataHolder.def.label} - {dataHolder.def.defName}");
            Text.Font = GameFont.Small;
            list.Gap(45);

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

            list.End();
        }
    }
}
