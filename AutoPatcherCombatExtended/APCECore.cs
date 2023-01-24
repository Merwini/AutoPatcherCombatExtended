using CombatExtended;
using HugsLib;
using HugsLib.Utils;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Reflection;
using Verse;

namespace AutoPatcherCombatExtended
{
    [StaticConstructorOnStartup]
    public partial class Base : ModBase
    {
        public override string ModIdentifier
        {
            get { return "AutoPatcherCE"; }
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
