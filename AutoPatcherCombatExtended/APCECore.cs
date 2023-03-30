using CombatExtended;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Reflection;
using Verse;
using UnityEngine;

namespace nuff.AutoPatcherCombatExtended
{
    [StaticConstructorOnStartup]
    public partial class AutoPatcherCombatExtended : Mod
    {
        APCESettings Settings;

        public AutoPatcherCombatExtended(ModContentPack content) : base(content)
        {
            this.Settings = GetSettings<APCESettings>();
            APCESettings.activeMods = AutoPatcherCombatExtended.GetActiveModsList();
            APCEPatchController();
        }

        public override string SettingsCategory()
        {
            return "Autopatcher for Combat Extended";
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
            //PatchWeapons(weaponList);
            //PatchApparel(apparelList);
            //PatchPawns(alienList);
            //PatchTurrets(turretList);
            //PatchHediffs(hediffList);
            //TODO: patch hediffs (fix armor values etc)
        }
    }
}
