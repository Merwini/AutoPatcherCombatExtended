using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace nuff.AutoPatcherCombatExtended
{
    [StaticConstructorOnStartup]
    public partial class APCEController
    {
        static APCEController()
        {
            Log.Message("APCE Controller constructed");
            APCESettings.activeMods = GetActiveModsList();
            APCESettings.modsToPatch = RebuildModsToPatch();
            APCEPatchController();
        }

        public static void APCEPatchController()
        {
            if (APCESettings.printLogs)
            {
                APCEPatchLogger.stopwatchMaster.Start();
            }
            CleanModList(APCESettings.modsToPatch);
            foreach (ModContentPack mod in APCESettings.modsToPatch)
            {
                PatchMod(mod);
            }
            if (APCESettings.printLogs)
            {
                APCEPatchLogger.stopwatchMaster.Stop();
                Log.Message($"Autopatcher for Combat Extended finished in {APCEPatchLogger.stopwatchMaster.ElapsedMilliseconds / 1000f} seconds.");
            }
        }

        public static void PatchMod(ModContentPack mod)
        {
            APCEPatchLogger log = new APCEPatchLogger(mod);
            log.BeginPatch();
            foreach (Def def in mod.AllDefs)
            {
                if (typeof(ThingDef).IsAssignableFrom(def.GetType()))
                {
                    ThingDef td = def as ThingDef;
                    if (td.IsApparel)
                    {
                        if (APCESettings.patchApparels)
                        {
                            PatchApparel(td, log);
                            continue;
                        }  
                    }
                    else if (td.IsWeapon)
                    {
                        if (APCESettings.patchWeapons)
                        {
                            if (td.IsRangedWeapon)
                            {
                                PatchRangedWeapon(td, log);
                                continue;
                            }
                            else if (td.IsMeleeWeapon)
                            {
                                PatchMeleeWeapon(td, log);
                                continue;
                            }
                        }
                    }
                    else if (typeof(Pawn).IsAssignableFrom(td.thingClass))
                    {
                        PatchPawn(td, log);
                        continue;
                    }
                }
                else if (typeof(HediffDef).IsAssignableFrom(def.GetType()))
                {
                    HediffDef hd = def as HediffDef;
                    if (APCESettings.patchHediffs)
                    {
                        PatchHediff(hd, log);
                        continue;
                    }
                }
            }
            log.EndPatch();
        }
    }


}
