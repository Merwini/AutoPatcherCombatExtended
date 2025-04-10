using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Vehicles;
using UnityEngine;

namespace nuff.AutoPatcherCombatExtended.VF
{
    class Window_CustomizeDefVehicleTurret : Window_CustomizeDef
    {
        DefDataHolderVehicleTurretDef dataHolder;

        public Window_CustomizeDefVehicleTurret(DefDataHolder defDataHolder) : base(defDataHolder)
        {
        }

        public override void CastDataHolder(DefDataHolder defDataHolder)
        {
            dataHolder = defDataHolder as DefDataHolderVehicleTurretDef;
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

            string modified_WarmUpTimerBuffer = dataHolder.modified_WarmUpTimer.ToString();
            list.TextFieldNumericLabeled("WarmUp Time: ", ref dataHolder.modified_WarmUpTimer, ref modified_WarmUpTimerBuffer);

            string modified_ReloadTimerBuffer = dataHolder.modified_ReloadTimer.ToString();
            list.TextFieldNumericLabeled("Reload Time: ", ref dataHolder.modified_ReloadTimer, ref modified_ReloadTimerBuffer);

            string modified_MinRangeBuffer = dataHolder.modified_MinRange.ToString();
            list.TextFieldNumericLabeled("Minimum Range: ", ref dataHolder.modified_MinRange, ref modified_MinRangeBuffer);

            string modified_MaxRangeBuffer = dataHolder.modified_MaxRange.ToString();
            list.TextFieldNumericLabeled("Maximum Range: ", ref dataHolder.modified_MaxRange, ref modified_MaxRangeBuffer);

            string modified_MagazineCapacityBuffer = dataHolder.modified_MagazineCapacity.ToString();
            list.TextFieldNumericLabeled("Magazine Capacity: ", ref dataHolder.modified_MagazineCapacity, ref modified_MagazineCapacityBuffer);

            string modified_ChargePerAmmoCountBuffer = dataHolder.modified_ChargePerAmmoCount.ToString();
            list.TextFieldNumericLabeled("Charge per ammo: ", ref dataHolder.modified_ChargePerAmmoCount, ref modified_ChargePerAmmoCountBuffer);

            string modified_SpeedBuffer = dataHolder.modified_Speed.ToString();
            list.TextFieldNumericLabeled("Projectile speed: ", ref dataHolder.modified_Speed, ref modified_SpeedBuffer);

            string modified_SwayBuffer = dataHolder.modified_Sway.ToString();
            list.TextFieldNumericLabeled("Barrel Sway: ", ref dataHolder.modified_Sway, ref modified_SwayBuffer);

            string modified_SpreadBuffer = dataHolder.modified_Spread.ToString();
            list.TextFieldNumericLabeled("Shot spread: ", ref dataHolder.modified_Spread, ref modified_SpreadBuffer);

            string modified_RecoilBuffer = dataHolder.modified_Recoil.ToString();
            list.TextFieldNumericLabeled("Shot recoil: ", ref dataHolder.modified_Recoil, ref modified_RecoilBuffer);

            string modified_ShotHeightBuffer = dataHolder.modified_ShotHeight.ToString();
            list.TextFieldNumericLabeled("Shot height: ", ref dataHolder.modified_ShotHeight, ref modified_ShotHeightBuffer);

            if (Widgets.ButtonText(new Rect(list.curX, list.curY, 400f, 30f), dataHolder.modified_AmmoSetString))
            {
                Find.WindowStack.Add(new Window_SelectTurretAmmoSet(dataHolder));
            }

            list.End();
        }
    }
}
