using CombatExtended;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using Verse;

namespace nuff.AutoPatcherCombatExtended
{
    partial class APCEController
    {
        //TODO
        internal static void PatchHediff(HediffDef def, APCEPatchLogger log)
        {
            try
            {
                //TODO
                foreach (HediffStage hs in def.stages)
                {
                    int sharpIndex = hs.statOffsets.FindIndex(x => x.stat == StatDefOf.ArmorRating_Sharp);
                    int bluntIndex = hs.statOffsets.FindIndex(x => x.stat == StatDefOf.ArmorRating_Blunt);

                    if (sharpIndex >= 0)
                    {
                        hs.statOffsets[sharpIndex].value *= APCESettings.hediffSharpMult;
                    }
                    if (bluntIndex >= 0)
                    {
                        hs.statOffsets[bluntIndex].value *= APCESettings.hediffBluntMult;
                    }
                }

                log.PatchSucceeded();
            }
            catch (Exception ex)
            {
                log.PatchFailed(def.defName, ex);
            }
        }
    }
}