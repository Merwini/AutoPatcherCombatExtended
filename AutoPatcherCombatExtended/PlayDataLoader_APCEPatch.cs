using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using HarmonyLib;

namespace nuff.AutoPatcherCombatExtended
{
    [HarmonyPatch(typeof(PlayDataLoader), "DoPlayLoad")]
    class PlayDataLoader_APCEPatch
    {
        [HarmonyPostfix]
        static void PostFix()
        {
            Log.Message("Hook is working");
            APCESettings.activeMods = AutoPatcherCombatExtended.GetActiveModsList();
            AutoPatcherCombatExtended.CleanModList(APCESettings.modsToPatch);
            //TODO call patch method
        }
    }
}
