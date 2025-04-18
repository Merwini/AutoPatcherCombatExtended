using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using UnityEngine;

namespace nuff.AutoPatcherCombatExtended
{
    class Window_CustomizeDefRangedWeapon : Window_CustomizeDef
    {
        DefDataHolderRangedWeapon dataHolder;

        private Vector2 scrollPosition = Vector2.zero;

        public Window_CustomizeDefRangedWeapon(DefDataHolder defDataHolder) : base(defDataHolder)
        {
        }

        public override void CastDataHolder(DefDataHolder defDataHolder)
        {
            dataHolder = defDataHolder as DefDataHolderRangedWeapon;
        }

        public override void DoWindowContents(Rect inRect)
        {
            base.DoWindowContents(inRect);
            Listing_Standard list = new Listing_Standard();

            // Begin main listing (Header)
            list.Begin(inRect);
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(0f, 0f, inRect.width - 150f - 17f, 35f), $"{dataHolder.def.label} - {dataHolder.def.defName}");
            Text.Font = GameFont.Small;
            list.End();
            list.Gap(45);

            // Define innerRect and scrolling parameters
            Rect outerRect = new Rect(inRect.x, inRect.y + 45f, inRect.width, inRect.height - 145f);
            Rect innerRect = outerRect.ContractedBy(10f); // Add some padding
            float scrollHeight = 9999f; // Temporarily large to measure actual content height
            Rect viewRect = new Rect(0, 0, innerRect.width - 16f, scrollHeight);

            // Begin measuring scroll height
            Widgets.BeginScrollView(innerRect, ref scrollPosition, viewRect);
            list.Begin(viewRect);

            // List of fields
            string modified_massBuffer = dataHolder.modified_Mass.ToString();
            list.TextFieldNumericLabeled("Mass", ref dataHolder.modified_Mass, ref modified_massBuffer);

            string modified_bulkBuffer = dataHolder.modified_Bulk.ToString();
            list.TextFieldNumericLabeled("Bulk", ref dataHolder.modified_Bulk, ref modified_bulkBuffer);

            string modified_workToMakeBuffer = dataHolder.modified_WorkToMake.ToString();
            list.TextFieldNumericLabeled("Work To Make", ref dataHolder.modified_WorkToMake, ref modified_workToMakeBuffer);

            string modified_weaponToughnessBuffer = dataHolder.modified_WeaponToughness.ToString();
            list.TextFieldNumericLabeled("Weapon Toughness", ref dataHolder.modified_WeaponToughness, ref modified_weaponToughnessBuffer);

            string modified_rangeBuffer = dataHolder.modified_range.ToString();
            list.TextFieldNumericLabeled("Range", ref dataHolder.modified_range, ref modified_rangeBuffer);

            string modified_rangedWeaponCooldownBuffer = dataHolder.modified_RangedWeaponCooldown.ToString();
            list.TextFieldNumericLabeled("Ranged Weapon Cooldown", ref dataHolder.modified_RangedWeaponCooldown, ref modified_rangedWeaponCooldownBuffer);

            string modified_burstShotCountBuffer = dataHolder.modified_burstShotCount.ToString();
            list.TextFieldNumericLabeled("Burst Shot Count", ref dataHolder.modified_burstShotCount, ref modified_burstShotCountBuffer);

            string modified_aimedBurstShotCountBuffer = dataHolder.modified_aimedBurstShotCount.ToString();
            list.TextFieldNumericLabeled("Aimed Burst Shot Count", ref dataHolder.modified_aimedBurstShotCount, ref modified_aimedBurstShotCountBuffer);

            string modified_sightsEfficiencyBuffer = dataHolder.modified_SightsEfficiency.ToString();
            list.TextFieldNumericLabeled("Sights Efficiency", ref dataHolder.modified_SightsEfficiency, ref modified_sightsEfficiencyBuffer);

            string modified_shotSpreadBuffer = dataHolder.modified_ShotSpread.ToString();
            list.TextFieldNumericLabeled("Shot Spread", ref dataHolder.modified_ShotSpread, ref modified_shotSpreadBuffer);

            string modified_swayFactorBuffer = dataHolder.modified_SwayFactor.ToString();
            list.TextFieldNumericLabeled("Sway Factor", ref dataHolder.modified_SwayFactor, ref modified_swayFactorBuffer);

            // Magazine & Reload
            string modified_magazineSizeBuffer = dataHolder.modified_magazineSize.ToString();
            list.TextFieldNumericLabeled("Magazine Size", ref dataHolder.modified_magazineSize, ref modified_magazineSizeBuffer);

            string modified_ammoGenPerMagOverrideBuffer = dataHolder.modified_AmmoGenPerMagOverride.ToString();
            list.TextFieldNumericLabeled("Ammo Gen Per Mag Override", ref dataHolder.modified_AmmoGenPerMagOverride, ref modified_ammoGenPerMagOverrideBuffer);

            string modified_reloadTimeBuffer = dataHolder.modified_reloadTime.ToString();
            list.TextFieldNumericLabeled("Reload Time", ref dataHolder.modified_reloadTime, ref modified_reloadTimeBuffer);

            list.CheckboxLabeled("Throw Mote", ref dataHolder.modified_throwMote);
            list.CheckboxLabeled("Reload One At A Time", ref dataHolder.modified_reloadOneAtATime);

            string modified_loadedAmmoBulkFactorBuffer = dataHolder.modified_loadedAmmoBulkFactor.ToString();
            list.TextFieldNumericLabeled("Loaded Ammo Bulk Factor", ref dataHolder.modified_loadedAmmoBulkFactor, ref modified_loadedAmmoBulkFactorBuffer);

            // Aimed Burst & AI Settings
            list.CheckboxLabeled("AI Use Burst Mode", ref dataHolder.modified_aiUseBurstMode);
            list.CheckboxLabeled("No Single Shot", ref dataHolder.modified_noSingleShot);
            list.CheckboxLabeled("No Snap Shot", ref dataHolder.modified_noSnapshot);
            list.CheckboxLabeled("Uses Ammo", ref dataHolder.modified_UsesAmmo);

            if (dataHolder.modified_UsesAmmo)
            {
                list.Gap();
                list.Label("AmmoSet to use:");
                if (Widgets.ButtonText(new Rect(list.curX, list.curY, 400f, 30f), dataHolder.modified_AmmoSetDef.defName))
                {
                    Find.WindowStack.Add(new Window_SelectAmmoSet(dataHolder));
                }
                list.Gap(35f);
            }
            else
            {
                list.Gap();
                list.Label("Projectile to use:");
                if (Widgets.ButtonText(new Rect(list.curX, list.curY, 400f, 30f), dataHolder.modified_defaultProjectile.defName))
                {
                    Find.WindowStack.Add(new Window_SelectProjectileDef(ref dataHolder.modified_defaultProjectile, newDef => dataHolder.modified_defaultProjectile = newDef));
                }
            }

            list.End();

            scrollHeight = list.CurHeight + 20f;
            viewRect.height = scrollHeight;

            Widgets.EndScrollView();
        }
    }
}
