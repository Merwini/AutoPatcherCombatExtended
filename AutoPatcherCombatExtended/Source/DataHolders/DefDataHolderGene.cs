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
    class DefDataHolderGene : DefDataHolder
    {
        //TODO can genes add verbs? Need to look into whether they would need patching.
        public DefDataHolderGene(GeneDef def)
        {
            this.def = def;
            defName = def.defName;
            parentModPackageId = def.modContentPack.PackageId;
            modData = DataHolderUtil.ReturnModDataOrDefault(def);

            GetOriginalData();
        }

        internal GeneDef def;

        float original_ArmorRatingSharp;
        float original_ArmorRatingBlunt;
        float original_ArmorRatingHeat;

        float modified_ArmorRatingSharp;
        float modified_ArmorRatingBlunt;
        float modified_ArmorRatingHeat;

        public override void GetOriginalData()
        {
            original_ArmorRatingSharp = def.statOffsets.GetStatValueFromList(StatDefOf.ArmorRating_Sharp, 0);
            original_ArmorRatingBlunt = def.statOffsets.GetStatValueFromList(StatDefOf.ArmorRating_Blunt, 0);
            original_ArmorRatingHeat = def.statOffsets.GetStatValueFromList(StatDefOf.ArmorRating_Heat, 0);
        }

        public override void AutoCalculate()
        {
            modified_ArmorRatingSharp = original_ArmorRatingSharp * modData.geneArmorSharpMult;
            modified_ArmorRatingBlunt = original_ArmorRatingBlunt * modData.geneArmorBluntMult;
        }

        public override void Patch()
        {
            DataHolderUtil.AddOrChangeStat(def.statOffsets, StatDefOf.ArmorRating_Sharp, modified_ArmorRatingSharp);
            DataHolderUtil.AddOrChangeStat(def.statOffsets, StatDefOf.ArmorRating_Blunt, modified_ArmorRatingBlunt);
            DataHolderUtil.AddOrChangeStat(def.statOffsets, StatDefOf.ArmorRating_Heat, modified_ArmorRatingHeat);
        }
        public override StringBuilder PrepExport()
        {
            //TODO
            return null;
        }

        public override void Export()
        {
            //TODO
        }

        public override void ExposeData()
        {
            base.ExposeData();
            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                def = DefDatabase<GeneDef>.GetNamed(defName, false);
                if (def != null)
                {
                    GetOriginalData();
                }
            }
            Scribe_Values.Look(ref modified_ArmorRatingSharp, "modified_ArmorRatingSharp", 0);
            Scribe_Values.Look(ref modified_ArmorRatingBlunt, "modified_ArmorRatingBlunt", 0f);
            Scribe_Values.Look(ref modified_ArmorRatingHeat, "modified_ArmorRatingHeat", 0f);
        }
    }
}
