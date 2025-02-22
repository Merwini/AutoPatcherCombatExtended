using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using Vehicles;
using nuff.AutoPatcherCombatExtended;

namespace nuff.AutoPatcherCombatExtended.VF
{
    public class APCE_VFCompatController : ICompat
    {
        //TODO 
        public APCE_VFCompatController()
        {
            Log.Message("APCE Vehicle Compatibility Controller Constructed");
            RegisterCheckDelegates();
            RegisterGenerateDelegates();
            RegisterWindowDicts();
        }

        internal static void RegisterGenerateDelegates()
        {
            Action<Def> patchVehicle = new Action<Def>(GenerateDefDataHolderVehicle);
            Action<Def> patchVehicleTurret = new Action<Def>(GenerateDefDataHolderVehicleTurret);

            APCESettings.typeHandlerDictionaryGenerate.Add(typeof(VehicleDef), patchVehicle);
            APCESettings.typeHandlerDictionaryGenerate.Add(typeof(VehicleTurretDef), patchVehicleTurret);

        }

        internal static void RegisterCheckDelegates()
        {
            Func<Def, APCEConstants.NeedsPatch> vehicleFunc = new Func<Def, APCEConstants.NeedsPatch>(CheckIfVehicleNeedsPatch);
            Func<Def, APCEConstants.NeedsPatch> vehicleTurretFunc = new Func<Def, APCEConstants.NeedsPatch>(CheckIfVehicleTurretNeedsPatch);

            APCESettings.typeHandlerDictionaryCheck.Add(typeof(VehicleDef), vehicleFunc);
            APCESettings.typeHandlerDictionaryCheck.Add(typeof(VehicleTurretDef), vehicleTurretFunc);
        }

        public static void GenerateDefDataHolderVehicle(Def def)
        {
            VehicleDef td = def as VehicleDef;
            DefDataHolder ddhv = new DefDataHolderVehicleDef(td);
        }

        public static void GenerateDefDataHolderVehicleTurret(Def def)
        {
            DefDataHolder ddhv = new DefDataHolderVehicleTurretDef(def);
        }

        public static APCEConstants.NeedsPatch CheckIfVehicleNeedsPatch(Def def)
        {
            return APCEConstants.NeedsPatch.yes;
        }

        public static APCEConstants.NeedsPatch CheckIfVehicleTurretNeedsPatch(Def def)
        {
            VehicleTurretDef vtd = def as VehicleTurretDef;
            if (vtd.HasModExtension<CETurretDataDefModExtension>())
                return APCEConstants.NeedsPatch.no;
            return APCEConstants.NeedsPatch.yes;
        }

        //TODO customization windows, register them in APCESettings.defCustomizationWindowDictionary
        public static void RegisterWindowDicts()
        {
            APCESettings.defCustomizationWindowDictionary[typeof(DefDataHolderVehicleDef)] = typeof(Window_CustomizeDefVehicle);
            APCESettings.defCustomizationWindowDictionary[typeof(DefDataHolderVehicleTurretDef)] = typeof(Window_CustomizeDefVehicleTurret);
        }
    }
}
