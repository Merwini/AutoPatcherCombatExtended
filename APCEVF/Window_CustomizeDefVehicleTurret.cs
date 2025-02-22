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


    }
}
