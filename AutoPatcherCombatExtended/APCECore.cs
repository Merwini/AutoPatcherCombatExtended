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
            AutoPatcherCombatExtended.CleanModList(APCESettings.modsToPatch);
        }

        public override string SettingsCategory()
        {
            return "Autopatcher for Combat Extended";
        }

        public void APCEPatchController()
        {
            stopwatchMaster.Start();

            MakeLists();
            //PatchWeapons(weaponList);
            //PatchApparel(apparelList);
            //PatchPawns(alienList);
            //PatchTurrets(turretList);
            //PatchHediffs(hediffList);
            //TODO: patch hediffs (fix armor values etc)

            stopwatchMaster.Stop();
            if (Settings.printDebug)
            {
                Log.Message($"Autopatcher for Combat Extended finished in {stopwatchMaster.ElapsedMilliseconds / 1000f} seconds.");
            }
        }
    }
}
