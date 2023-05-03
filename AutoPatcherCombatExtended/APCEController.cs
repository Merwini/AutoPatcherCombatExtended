﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using Verse.AI;

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
            //CleanModList(APCESettings.modsToPatch);
            InjectedDefHasher.PrepareReflection();
            foreach (ModContentPack mod in APCESettings.modsToPatch)
            {
                if (APCESettings.modsAlreadyPatched.Add(mod))
                {
                    PatchMod(mod);
                }
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
                if (def is ThingDef)
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
                            if (td.IsRangedWeapon 
                                && (!typeof(Verb_CastAbility).IsAssignableFrom(td.Verbs[0].verbClass))
                                && (!typeof(Verb_CastBase).IsAssignableFrom(td.Verbs[0].verbClass)))
                            {
                                PatchRangedWeapon(td, log);
                                continue;
                            }
                            else //if (td.IsMeleeWeapon)
                            {
                                PatchMeleeWeapon(td, log);
                                continue;
                            }
                        }
                        else
                        {
                            DisableGenericAmmos();
                        }
                    }
                    else if (typeof(Pawn).IsAssignableFrom(td.thingClass))
                    {
                        PatchPawn(td, log);
                        continue;
                    }
                    else if (typeof(Building_TurretGun).IsAssignableFrom(td.thingClass))
                    {
                        PatchTurretBase(td, log);
                    }
                    else if ((td.thingCategories != null) && td.thingCategories.Contains(APCEDefOf.MortarShells))
                    {
                        PatchMortarShell(td, log);
                    }
                }
                else if (def is HediffDef)
                {
                    HediffDef hd = def as HediffDef;
                    if (APCESettings.patchHediffs)
                    {
                        PatchHediff(hd, log);
                        continue;
                    }
                }
                else if (def is PawnKindDef)
                {
                    if (APCESettings.patchPawnKinds)
                    {
                        PawnKindDef pkd = def as PawnKindDef;
                        PatchPawnKind(pkd, log);
                    }
                }
            }
            log.EndPatch();
        }
    }


}
