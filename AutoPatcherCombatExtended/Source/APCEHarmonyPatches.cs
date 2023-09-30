using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using HarmonyLib;
using System.Reflection;

namespace nuff.AutoPatcherCombatExtended
{
    public class APCEHarmonyPatches
    {
        public static Type VehicleDef;
        public static Type VehicleTurretDef;
        public static Type APCEPatchVehicle;
        public static MethodInfo PatchVehicle;
        public static MethodInfo PatchVehicleTurret;

        public APCEHarmonyPatches()
        {

            Harmony harmony = new Harmony("nuff.rimworld.autopatchercombatextended");

            if (ModsConfig.IsActive("SmashPhil.VehicleFramework"))
            {
                foreach (ModContentPack mod in LoadedModManager.RunningModsListForReading)
                {
                    Log.Warning(mod.PackageId);
                }
                ModContentPack apcevf = LoadedModManager.RunningModsListForReading.First(mod => mod.PackageId.Equals("nuff.ceautopatcher"));
                Assembly apcevfAssembly = apcevf.assemblies.loadedAssemblies.First(ass => ass.ToString().Contains("APCEVF"));
                APCEPatchVehicle = apcevfAssembly.GetType("nuff.AutoPatcherCombatExtended.VF.APCEPatchVehicle");

                ModContentPack vf = LoadedModManager.RunningModsListForReading.First(mod => mod.PackageId.Equals("smashphil.vehicleframework"));
                Assembly vfAssembly = vf.assemblies.loadedAssemblies.First(ass => ass.ToString().Contains("Vehicles"));
                VehicleDef = vfAssembly.GetType("Vehicles.VehicleDef");
                VehicleTurretDef = vfAssembly.GetType("Vehicles.VehicleTurretDef");
                PatchVehicle = APCEPatchVehicle.GetMethod("PatchVehicle");
                PatchVehicleTurret = APCEPatchVehicle.GetMethod("PatchVehicleTurret");

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
            if (def.GetType() == APCEHarmonyPatches.VehicleDef)
            {
                APCEHarmonyPatches.PatchVehicle.Invoke(null, new object[] { def, log });
                return false;
            }
            else if (def.GetType() == APCEHarmonyPatches.VehicleTurretDef)
            {
                APCEHarmonyPatches.PatchVehicleTurret.Invoke(null, new object[] { def, log });
                return false;
            }
            return true;
        }
    }
}