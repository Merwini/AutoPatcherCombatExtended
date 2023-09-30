using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using HarmonyLib;
using System.Reflection;

namespace nuff.AutoPatcherCombatExtended
{
    [StaticConstructorOnStartup]
    public static class APCEVehiclesHarmony
    {
        public static Type VehicleDef;
        public static Type VehicleTurretDef;

        static APCEVehiclesHarmony()
        {
            Harmony harmony = new Harmony("nuff.rimworld.autopatchercombatextended");
            harmony.PatchAll();
            if (ModsConfig.IsActive("SmashPhil.VehicleFramework"))
            {
                VehicleDef = Type.GetType("SmashPhil.VehicleFramework.VehicleDef");
                VehicleTurretDef = Type.GetType("SmashPhil.VehicleFramework.VehicleTurretDef");

                if (VehicleDef != null)
                {
                    MethodInfo original = typeof(APCEController).GetMethod(nameof(APCEController.DetermineDefType));
                    MethodInfo vPrefix = typeof(APCEVehiclePatch).GetMethod(nameof(APCEVehiclePatch.Prefix));
                    harmony.Patch(original, prefix: new HarmonyMethod(vPrefix));
                }
            }
        } 
    }

    [HarmonyPatch(typeof(APCEController), nameof(APCEController.DetermineDefType))]
    public class APCEVehiclePatch
    {
        public static bool Prefix(ref Def def, ref APCEPatchLogger log)
        {
            if (APCEVehiclesHarmony.VehicleDef != null && def.GetType() == APCEVehiclesHarmony.VehicleDef)
            {

                APCEPatchVehicle.PatchVehicle(vehicle, log);
                return false;
            }
            else if (APCEVehiclesHarmony.VehicleTurretDef != null && def.GetType() == APCEVehiclesHarmony.VehicleTurretDef)
            {
                APCEPatchVehicle.PatchVehicleTurret(turret, log);
                return false;
            }
            return true;
        }
    }
}