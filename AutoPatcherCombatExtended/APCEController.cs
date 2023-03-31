using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace nuff.AutoPatcherCombatExtended
{
    [StaticConstructorOnStartup]
    partial class APCEController
    {
        public APCEController()
        {
            APCESettings.activeMods = GetActiveModsList();
            APCESettings.modsToPatch = RebuildModsToPatch();
            APCEPatchController();
        }

        public void APCEPatchController()
        {
            if (APCESettings.printDebug)
            {
                APCEPatchLogger.stopwatchMaster.Start();
            }
            CleanModList(APCESettings.modsToPatch);
            foreach (ModContentPack mod in APCESettings.modsToPatch)
            {
                PatchMod(mod);
            }
            if (APCESettings.printDebug)
            {
                APCEPatchLogger.stopwatchMaster.Stop();
                Log.Message($"Autopatcher for Combat Extended finished in {APCEPatchLogger.stopwatchMaster.ElapsedMilliseconds / 1000f} seconds.");
            }
        }

        public void PatchMod(ModContentPack mod)
        {
            APCEPatchLogger log = new APCEPatchLogger(mod);
            foreach (Def def in mod.AllDefs)
            {
                Log.Message(def.defName);
            }
            //PatchWeapons(weaponList);
            //PatchApparel(apparelList);
            //PatchPawns(pawnList);
            //PatchHediffs(hediffList);
            //PatchTurrets(turretList);
        }
    }


}
