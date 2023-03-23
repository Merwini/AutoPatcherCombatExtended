using CombatExtended;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Reflection;
using Verse;

namespace nuff.AutoPatcherCombatExtended
{
    [StaticConstructorOnStartup]
    public partial class AutoPatcherCombatExtended : Mod
    {
        APCESettings Settings;

        public AutoPatcherCombatExtended(ModContentPack content) : base(content)
        {
            this.Settings = GetSettings<APCESettings>();
        }

        public override string SettingsCategory()
        {
            return "Autopatcher for Combat Extended";
        }

        public override void DefsLoaded()
        {
            if (!ModIsActive)
                return;

            stopwatchMaster.Start();

            MakeLists();
            //PatchWeapons(weaponList);
            //PatchApparel(apparelList);
            //PatchPawns(alienList);
            //PatchTurrets(turretList);
            //PatchHediffs(hediffList);
            //TODO: patch hediffs (fix armor values etc)

            stopwatchMaster.Stop();
            if (printDebug)
            {
                Logger.Message($"Auto-Patcher for Combat Extended finished in {stopwatchMaster.ElapsedMilliseconds / 1000f} seconds.");
            }

            if (modsNeedingPatches.Count > 0)
            {
                foreach (ModContentPack mcp in modsNeedingPatches)
                {
                    Logger.Message("Mod needing patches: " + mcp.PackageId);
                }
            }

            foreach (ThingDef td in weaponList)
            {
                Logger.Message("weapons patched: " + td.defName);
            }

            foreach (ThingDef td in apparelList)
            {
                Logger.Message("apparels patched: " + td.defName);
            }
        }
    }
}
