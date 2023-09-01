using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace nuff.AutoPatcherCombatExtended
{
    partial class APCEController
    {
        internal static void PatchGene(GeneDef gene, APCEPatchLogger log)
        {
            try
            {
                if (!gene.statOffsets.NullOrEmpty())
                {
                    int sharpIndex = gene.statOffsets.FindIndex(i => i.stat == StatDefOf.ArmorRating_Sharp);
                    int bluntIndex = gene.statOffsets.FindIndex(i => i.stat == StatDefOf.ArmorRating_Blunt);
                    if (sharpIndex >= 0)
                    {
                        gene.statOffsets[sharpIndex].value *= APCESettings.geneArmorSharpMult;
                    }
                    if (bluntIndex >= 0)
                    {
                        gene.statOffsets[bluntIndex].value *= APCESettings.geneArmorBluntMult;
                    }
                }
                
                //TODO checks conditionalstataffecters for armor
                if (!gene.conditionalStatAffecters.NullOrEmpty())
                {
                    for (int i = 0; i < gene.conditionalStatAffecters.Count; i++)
                    {
                        int iSharpIndex = gene.conditionalStatAffecters[i].statOffsets?.FindIndex(j => j.stat == StatDefOf.ArmorRating_Sharp) ?? -1;
                        int iBluntIndex = gene.conditionalStatAffecters[i].statOffsets?.FindIndex(j => j.stat == StatDefOf.ArmorRating_Blunt) ?? -1;
                        if (iSharpIndex >= 0)
                        {
                            gene.conditionalStatAffecters[i].statOffsets[iSharpIndex].value *= APCESettings.geneArmorSharpMult;
                        }
                        if (iBluntIndex >= 0)
                        {
                            gene.conditionalStatAffecters[i].statOffsets[iBluntIndex].value *= APCESettings.geneArmorBluntMult;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.PatchFailed(gene.defName, ex);
            }
        }
    }
}
