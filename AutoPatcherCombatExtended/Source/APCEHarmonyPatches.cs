using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using HarmonyLib;
using System.Reflection;
using Vehicles;

namespace nuff.AutoPatcherCombatExtended
{
    public class APCEHarmonyPatches
    {
        public APCEHarmonyPatches()
        {
            Harmony harmony = new Harmony("nuff.rimworld.autopatchercombatextended");
            if (ModsConfig.IsActive("SmashPhil.VehicleFramework"))
            {
                MethodInfo original = typeof(APCEController).GetMethod(nameof(APCEController.DetermineDefType));
                MethodInfo vPrefix = typeof(APCEVehiclePatch).GetMethod(nameof(APCEVehiclePatch.Prefix));
                harmony.Patch(original, prefix: new HarmonyMethod(vPrefix));
            }
        }
    }

    //[HarmonyPatch(typeof(APCEController), nameof(APCEController.DetermineDefType))]
    public class APCEVehiclePatch
    {
        public static bool Prefix(ref Def def, ref APCEPatchLogger log)
        {
            if (def is VehicleDef vehicle)
            {
                APCEPatchVehicle.PatchVehicle(vehicle, log);
                return false;
            }
            else if (def is VehicleTurretDef turret)
            {
                APCEPatchVehicle.PatchVehicleTurret(turret, log);
                return false;
            }
            return true;
        }
    }
}
