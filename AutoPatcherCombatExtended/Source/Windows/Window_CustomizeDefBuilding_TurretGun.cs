using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using UnityEngine;

namespace nuff.AutoPatcherCombatExtended
{
    class Window_CustomizeDefBuilding_TurretGun : Window_CustomizeDef
    {
        DefDataHolderBuilding_TurretGun dataHolder;

        public Window_CustomizeDefBuilding_TurretGun(DefDataHolder defDataHolder) : base(defDataHolder)
        {
        }

        public override void CastDataHolder(DefDataHolder defDataHolder)
        {
            dataHolder = defDataHolder as DefDataHolderBuilding_TurretGun;
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

            string modified_FillPercentBuffer = dataHolder.modified_FillPercent.ToString();
            list.TextFieldNumericLabeled("Fill Percent", ref dataHolder.modified_FillPercent, ref modified_FillPercentBuffer);

            string modified_TurretBurstCooldownTimeBuffer = dataHolder.modified_TurretBurstCooldownTime.ToString();
            list.TextFieldNumericLabeled("Turret Burst Cooldown Time", ref dataHolder.modified_TurretBurstCooldownTime, ref modified_TurretBurstCooldownTimeBuffer);

            string modified_AimingAccuracyBuffer = dataHolder.modified_AimingAccuracy.ToString();
            list.TextFieldNumericLabeled("Aiming Accuracy", ref dataHolder.modified_AimingAccuracy, ref modified_AimingAccuracyBuffer);

            list.End();
        }

    }
}
