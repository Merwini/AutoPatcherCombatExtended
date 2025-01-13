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
    public class DefDataHolderHediff : DefDataHolder
    {
        public DefDataHolderHediff()
        {
            //empty constructor for use by SaveLoad
        }

        public DefDataHolderHediff(HediffDef def) : base(def)
        {
        }

        HediffDef hediffDef;

        List<float> original_ArmorRatingSharp = new List<float>();
        List<float> original_ArmorRatingBlunt = new List<float>();
        List<float> original_ArmorRatingHeat = new List<float>();
        HediffCompProperties_VerbGiver verbGiver;

        List<float> modified_ArmorRatingSharp = new List<float>();
        List<float> modified_ArmorRatingBlunt = new List<float>();
        List<float> modified_ArmorRatingHeat = new List<float>();

        public override void GetOriginalData()
        {
            hediffDef = def as HediffDef;

            verbGiver = hediffDef.comps?.Find((HediffCompProperties c) => c is HediffCompProperties_VerbGiver) as HediffCompProperties_VerbGiver;
            if (verbGiver != null && verbGiver.tools != null)
            {
                original_Tools = verbGiver.tools;
            }

            if (hediffDef.stages.NullOrEmpty())
                return;
            for (int i = 0; i < hediffDef.stages.Count; i++)
            {
                float armorRatingSharp = 0f;
                float armorRatingBlunt = 0f;
                float armorRatingHeat = 0f;

                if (hediffDef.stages[i].statOffsets != null)
                {
                    armorRatingSharp = hediffDef.stages[i].statOffsets.GetStatValueFromList(StatDefOf.ArmorRating_Sharp, 0);
                    armorRatingBlunt = hediffDef.stages[i].statOffsets.GetStatValueFromList(StatDefOf.ArmorRating_Blunt, 0);
                    armorRatingHeat = hediffDef.stages[i].statOffsets.GetStatValueFromList(StatDefOf.ArmorRating_Heat, 0);
                }

                original_ArmorRatingSharp.Add(armorRatingSharp);
                original_ArmorRatingBlunt.Add(armorRatingBlunt);
                original_ArmorRatingHeat.Add(armorRatingHeat);
            }
        }

        public override void AutoCalculate()
        {
            if (!original_Tools.NullOrEmpty())
            {
                for (int i = 0; i < original_Tools.Count; i++)
                {
                    ModToolAtIndex(i);
                }
            }
            if (!hediffDef.stages.NullOrEmpty())
            {
                for (int i = 0; i < hediffDef.stages.Count; i++)
                {
                    modified_ArmorRatingSharp.Add(original_ArmorRatingSharp[i] * modData.hediffSharpMult);
                    modified_ArmorRatingBlunt.Add(original_ArmorRatingBlunt[i] * modData.hediffBluntMult);
                    modified_ArmorRatingHeat.Add(original_ArmorRatingHeat[i]);
                }
            }
        }
        public override void Patch()
        {
            if (!hediffDef.stages.NullOrEmpty())
            {
                for (int i = 0; i < hediffDef.stages.Count; i++)
                {
                    DataHolderUtils.AddOrChangeStat(hediffDef.stages[i].statOffsets, StatDefOf.ArmorRating_Sharp, modified_ArmorRatingSharp[i]);
                    DataHolderUtils.AddOrChangeStat(hediffDef.stages[i].statOffsets, StatDefOf.ArmorRating_Blunt, modified_ArmorRatingBlunt[i]);
                    DataHolderUtils.AddOrChangeStat(hediffDef.stages[i].statOffsets, StatDefOf.ArmorRating_Heat, modified_ArmorRatingHeat[i]);
                }
            }
            if (verbGiver != null && !original_Tools.NullOrEmpty())
            {
                verbGiver.tools.Clear();
                BuildTools();
                for (int i = 0; i < modified_Tools.Count; i++)
                {
                    verbGiver.tools.Add(modified_Tools[i]);
                }
            }
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

        public override void ModToolAtIndex(int i)
        {
            base.ModToolAtIndex(i);
            modified_ToolPowers[i] *= modData.pawnToolPowerMult;
            modified_ToolArmorPenetrationSharps[i] *= modData.pawnToolSharpPenetration;
            modified_ToolArmorPenetrationSharps[i] *= modData.pawnToolBluntPenetration;
        }

        //TODO
        public override void ExposeData()
        {
            base.ExposeData();

            if (Scribe.mode == LoadSaveMode.LoadingVars
                || (Scribe.mode == LoadSaveMode.Saving && isCustomized == true))
            {
                Scribe_Collections.Look(ref modified_ArmorRatingSharp, "modified_ArmorRatingSharp", LookMode.Value);
                Scribe_Collections.Look(ref modified_ArmorRatingBlunt, "modified_ArmorRatingBlunt", LookMode.Value);
                Scribe_Collections.Look(ref modified_ArmorRatingHeat, "modified_ArmorRatingHeat", LookMode.Value);
            }
        }
    }
}
