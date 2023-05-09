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
        internal static void PatchHediff(HediffDef def, APCEPatchLogger log)
        {
            try
            {
                if (def.stages != null)
                {
                    foreach (HediffStage hs in def.stages)
                    {
                        if (hs.statOffsets != null)
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
                    }
                }
                
                HediffCompProperties_VerbGiver hcp_vg = def.comps?.Find((HediffCompProperties c) => c is HediffCompProperties_VerbGiver) as HediffCompProperties_VerbGiver;

                if (hcp_vg != null)
                {
                    PatchAllTools(ref hcp_vg.tools, false);
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