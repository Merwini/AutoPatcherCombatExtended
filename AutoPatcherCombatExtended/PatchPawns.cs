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
        internal static void PatchPawn(ThingDef def, APCEPatchLogger log)
        {
            try
            {
                //TODO

                #region ArmorValues
                int sharpIndex = def.statBases.FindIndex(x => x.stat == StatDefOf.ArmorRating_Sharp);
                int bluntIndex = def.statBases.FindIndex(x => x.stat == StatDefOf.ArmorRating_Blunt);

                if (sharpIndex >= 0)
                {
                    def.statBases[sharpIndex].value *= APCESettings.pawnArmorSharpMult;
                }
                if (bluntIndex >= 0)
                {
                    def.statBases[bluntIndex].value *= APCESettings.pawnArmorBluntMult;
                }
                #endregion

                #region Tools
                PatchAllTools(def);
                #endregion

                log.PatchSucceeded();
            }
            catch (Exception ex)
            {
                log.PatchFailed(def.defName, ex);
            }
        }

        
    }
}