using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using HarmonyLib;

namespace nuff.AutoPatcherCombatExtended
{
    [HarmonyPatch(typeof(PlayDataLoader))]
    [HarmonyPatch("DoPlayLoad")]
    [HarmonyPatch(new Type[0])]
    internal static class PlayDataLoader_APCEPatch
    {
        [HarmonyPostfix]
        private static void APCEInitModsHook()
        {
            AutoPatcherCombatExtended.CleanModList(APCESettings.modsToPatch);
            //TODO call patch method
        }
    }
}
