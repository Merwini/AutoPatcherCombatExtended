using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using CombatExtended;

namespace nuff.AutoPatcherCombatExtended
{
    public class DefDataHolderGene : DefDataHolder
    {
        //TODO can genes add verbs? Need to look into whether they would need patching.
        public DefDataHolderGene()
        {
            //empty constructor for use by SaveLoad
        }

        public DefDataHolderGene(GeneDef def) : base(def)
        {
        }

        public GeneDef geneDef;

        float original_ArmorRatingSharp;
        float original_ArmorRatingBlunt;
        float original_ArmorRatingHeat;

        float modified_ArmorRatingSharp;
        float modified_ArmorRatingBlunt;
        float modified_ArmorRatingHeat;

        public override void GetOriginalData()
        {
            //constructed by APCEController, def assigned by constructor
            if (def != null && geneDef == null)
            {
                this.geneDef = def as GeneDef;
            }
            //constructed by SaveLoad, thingDef loaded from xml
            else if (geneDef != null && def == null)
            {
                def = geneDef;
            }

            original_ArmorRatingSharp = geneDef.statOffsets.GetStatValueFromList(StatDefOf.ArmorRating_Sharp, 0);
            original_ArmorRatingBlunt = geneDef.statOffsets.GetStatValueFromList(StatDefOf.ArmorRating_Blunt, 0);
            original_ArmorRatingHeat = geneDef.statOffsets.GetStatValueFromList(StatDefOf.ArmorRating_Heat, 0);
        }

        public override void AutoCalculate()
        {
            modified_ArmorRatingSharp = original_ArmorRatingSharp * modData.geneArmorSharpMult;
            modified_ArmorRatingBlunt = original_ArmorRatingBlunt * modData.geneArmorBluntMult;
        }

        public override void Patch()
        {
            DataHolderUtils.AddOrChangeStat(geneDef.statOffsets, StatDefOf.ArmorRating_Sharp, modified_ArmorRatingSharp);
            DataHolderUtils.AddOrChangeStat(geneDef.statOffsets, StatDefOf.ArmorRating_Blunt, modified_ArmorRatingBlunt);
            DataHolderUtils.AddOrChangeStat(geneDef.statOffsets, StatDefOf.ArmorRating_Heat, modified_ArmorRatingHeat);
        }
        public override StringBuilder PrepExport()
        {
            //TODO
            return null;
        }

        public override void ExportXML()
        {
            //TODO
        }

        public override void ExposeData()
        {
            Scribe_Defs.Look(ref geneDef, "def");
            if (Scribe.mode == LoadSaveMode.LoadingVars
                || (Scribe.mode == LoadSaveMode.Saving && isCustomized == true))
            {
                Scribe_Values.Look(ref modified_ArmorRatingSharp, "modified_ArmorRatingSharp", 0f);
                Scribe_Values.Look(ref modified_ArmorRatingBlunt, "modified_ArmorRatingBlunt", 0f);
                Scribe_Values.Look(ref modified_ArmorRatingHeat, "modified_ArmorRatingHeat", 0f);
            }
            base.ExposeData();
        }
    }
}
