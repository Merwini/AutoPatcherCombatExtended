using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using Vehicles;

namespace nuff.AutoPatcherCombatExtended.VF
{
    [StaticConstructorOnStartup]
    public static class APCE_VFCompatController
    {
        //TODO 
        static APCE_VFCompatController()
        {
            Log.Message("APCE Vehicle Compatibility Controller Constructed");
        }

        internal static void RegisterDelegates()
        {
            Action<Def> patchVehicle = new Action<Def>(GenerateDefDataHolderVehicle);
            Action<Def> patchVehicleTurret = new Action<Def>(GenerateDefDataHolderVehicleTurret);

            APCESettings.typeHandlerDictionaryGenerate.Add(typeof(VehicleDef), patchVehicle);
            APCESettings.typeHandlerDictionaryGenerate.Add(typeof(VehicleTurretDef), patchVehicleTurret);

        }

        internal static void RegisterFuncs()
        {
            Func<Def, bool> vehicleFunc = new Func<Def, bool>(CheckIfVehicleNeedsPatch);
            Func<Def, bool> vehicleTurretFunc = new Func<Def, bool>(CheckIfVehicleTurretNeedsPatch);

            APCESettings.typeHandlerDictionaryCheck.Add(typeof(VehicleDef), vehicleFunc);
            APCESettings.typeHandlerDictionaryCheck.Add(typeof(VehicleTurretDef), vehicleTurretFunc);
        }

        public static void GenerateDefDataHolderVehicle(Def def)
        {
            ThingDef td = def as ThingDef;
            DefDataHolder ddhv = new DefDataHolderVehicleDef(td);
        }

        public static void GenerateDefDataHolderVehicleTurret(Def def)
        {
            DefDataHolder ddhv = new DefDataHolderVehicleTurretDef(def);
        }

        public static bool CheckIfVehicleNeedsPatch(Def def)
        {
            //TODO check if vehicle needs patch - no good way to do this
            return false;
        }

        public static bool CheckIfVehicleTurretNeedsPatch(Def def)
        {
            VehicleTurretDef vtd = def as VehicleTurretDef;
            if (!vtd.HasModExtension<CETurretDataDefModExtension>())
                return true;
            return false;
        }
    }
}
