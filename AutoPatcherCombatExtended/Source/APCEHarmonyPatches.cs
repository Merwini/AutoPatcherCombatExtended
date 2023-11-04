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
        public static Type CETurretDataDefModExtension;
        public static MethodInfo PatchVehicle;
        public static MethodInfo PatchVehicleTurret;

        public APCEHarmonyPatches()
        {

            Harmony harmony = new Harmony("nuff.rimworld.autopatchercombatextended");

            #region SuggestionPopup
            //TODO
            MethodInfo DoMainMenuControlsMethod = typeof(MainMenuDrawer).GetMethod(nameof(MainMenuDrawer.DoMainMenuControls));
            MethodInfo DoMainMenuControlsPostfix = typeof(APCEMainMenuPatch).GetMethod(nameof(APCEMainMenuPatch.Postfix));
            harmony.Patch(DoMainMenuControlsMethod, postfix: new HarmonyMethod(DoMainMenuControlsPostfix));
            #endregion

            #region VFPatches
            if (ModsConfig.IsActive("SmashPhil.VehicleFramework"))
            {
                ModContentPack apcevf = LoadedModManager.RunningModsListForReading.First(mod => mod.PackageId.Equals("nuff.ceautopatcher"));
                Assembly apcevfAssembly = apcevf.assemblies.loadedAssemblies.First(ass => ass.ToString().Contains("APCEVF"));
                APCEPatchVehicle = apcevfAssembly.GetType("nuff.AutoPatcherCombatExtended.VF.APCEPatchVehicle");

                ModContentPack vf = LoadedModManager.RunningModsListForReading.First(mod => mod.PackageId.Equals("smashphil.vehicleframework"));
                Assembly vfAssembly = vf.assemblies.loadedAssemblies.First(ass => ass.ToString().Contains("Vehicles"));
                VehicleDef = vfAssembly.GetType("Vehicles.VehicleDef");
                CETurretDataDefModExtension = vfAssembly.GetType("Vehicles.CETurretDataDefModExtension");
                VehicleTurretDef = vfAssembly.GetType("Vehicles.VehicleTurretDef");
                PatchVehicle = APCEPatchVehicle.GetMethod("PatchVehicle");
                PatchVehicleTurret = APCEPatchVehicle.GetMethod("PatchVehicleTurret");


                MethodInfo SortAndPatchDefMethod = typeof(APCEController).GetMethod(nameof(APCEController.SortAndPatchDef));
                MethodInfo SortAndPatchDefPrefix = typeof(APCEVehiclePatch).GetMethod(nameof(APCEVehiclePatch.Prefix));
                harmony.Patch(SortAndPatchDefMethod, prefix: new HarmonyMethod(SortAndPatchDefPrefix));

                MethodInfo CheckIfDefNeedsPatchedMethod = typeof(APCEController).GetMethod(nameof(APCEController.CheckIfDefNeedsPatched));
                MethodInfo CheckIfDefNeedsPatchedPrefix = typeof(APCEDetectVehiclePatch).GetMethod(nameof(APCEDetectVehiclePatch.Prefix));
                harmony.Patch(CheckIfDefNeedsPatchedMethod, prefix: new HarmonyMethod(CheckIfDefNeedsPatchedPrefix));
            }
            #endregion
        }
    }

    public class APCEMainMenuPatch
    {
        public static void Postfix()
        {
            if (Current.ProgramState == ProgramState.Entry)
            {
                Find.WindowStack.Add(new Window_SuggestPatchMods());
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

    public class APCEDetectVehiclePatch
    {
        public static bool Prefix(ref Def def, ref bool __result)
        {
            if (def.GetType() == APCEHarmonyPatches.VehicleTurretDef
                && !def.modExtensions.Any(me => me.GetType() == APCEHarmonyPatches.CETurretDataDefModExtension))
            {
                __result = true;
                return false;
            }
            return true;
        }
    }
}