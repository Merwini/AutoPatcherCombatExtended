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
                            if (PatchApparel(td))
                            {

                            }
                            else
                            {

                            }
                        }
                        
                    }
                    else if (td.IsWeapon)
                    {
                        if (APCESettings.patchWeapons)
                        {
                            if (PatchWeapon(td))
                            {

                            }
                            else
                            {

                            }
                        }
                        
                    }
                    else if (typeof(Pawn).IsAssignableFrom(td.GetType()))
                    {
                        if (APCESettings.patchPawns)
                        {
                            if (PatchPawn(td))
                            {

                            }
                            else
                            {

                            }
                        }
                    }
                }
                else if (typeof(HediffDef).IsAssignableFrom(def.GetType()))
                {
                    HediffDef hd = def as HediffDef;
                    if (APCESettings.patchHediffs)
                    {
                        if (PatchHediff(hd))
                        {

                        }
                        else
                        {

                        }
                    }
                }
            }
            //PatchWeapons(weaponList);
            //PatchApparel(apparelList);
            //PatchPawns(pawnList);
            //PatchHediffs(hediffList);
            //PatchTurrets(turretList);
            log.EndPatch();
        }
    }


}
