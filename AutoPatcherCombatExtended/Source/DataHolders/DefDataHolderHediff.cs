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
    class DefDataHolderHediff : DefDataHolder
    {
        public DefDataHolderHediff(HediffDef def) : base(def)
        {
        }

        HediffDef hediffDef;

        List<float> original_ArmorRatingSharp = new List<float>();
        List<float> original_ArmorRatingBlunt = new List<float>();
        List<float> original_ArmorRatingHeat = new List<float>();
        List<Tool> original_Tools;
        HediffCompProperties_VerbGiver verbGiver;

        List<float> modified_ArmorRatingSharp = new List<float>();
        List<float> modified_ArmorRatingBlunt = new List<float>();
        List<float> modified_ArmorRatingHeat = new List<float>();
        List<Tool> modified_Tools = new List<Tool>();

        List<string> toolIds;
        List<List<string>> toolCapacities;
        List<string> toolLinkedBPG;
        List<float> toolCooldownTimes;
        List<float> toolArmorPenetrationSharps;
        List<float> toolArmorPenetrationBlunts;
        List<float> toolPowers;



        //TODO use more basic data structures so it can be serialized

        public override void GetOriginalData()
        {
            verbGiver = hediffDef.comps?.Find((HediffCompProperties c) => c is HediffCompProperties_VerbGiver) as HediffCompProperties_VerbGiver;
            if (verbGiver != null && verbGiver.tools != null)
            {
                original_Tools = verbGiver.tools;
            }

            if (hediffDef.stages == null)
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
                    modified_Tools.Add(MakeToolHediff(original_Tools[i]));
                }
            }
            if (!hediffDef.stages.NullOrEmpty())
            {
                for (int i = 0; i < hediffDef.stages?.Count; i++)
                {
                    modified_ArmorRatingSharp.Add(original_ArmorRatingSharp[i] * modData.hediffSharpMult);
                    modified_ArmorRatingBlunt.Add(original_ArmorRatingBlunt[i] * modData.hediffBluntMult);
                }
            }
        }
        public override void Patch()
        {
            if (!hediffDef.stages.NullOrEmpty())
            {
                for (int i = 0; i < hediffDef.stages.Count; i++)
                {
                    DataHolderUtil.AddOrChangeStat(hediffDef.stages[i].statOffsets, StatDefOf.ArmorRating_Sharp, modified_ArmorRatingSharp[i]);
                    DataHolderUtil.AddOrChangeStat(hediffDef.stages[i].statOffsets, StatDefOf.ArmorRating_Blunt, modified_ArmorRatingBlunt[i]);
                    DataHolderUtil.AddOrChangeStat(hediffDef.stages[i].statOffsets, StatDefOf.ArmorRating_Heat, modified_ArmorRatingHeat[i]);
                }
            }
            if (verbGiver != null && original_Tools != null)
            {
                verbGiver.tools = modified_Tools;
            }
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

        public ToolCE MakeToolHediff(Tool tool)
        {
            ToolCE newToolCE = DataHolderUtil.MakeToolBase(tool);
            newToolCE.power *= modData.pawnToolPowerMult;
            newToolCE.armorPenetrationSharp *= modData.pawnToolSharpPenetration;
            newToolCE.armorPenetrationBlunt *= modData.pawnToolBluntPenetration;

            return newToolCE;
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

                Scribe_Collections.Look(ref toolIds, "toolIds", LookMode.Value);
                Scribe_Collections.Look(ref toolCapacities, "toolCapacities", LookMode.Value);
                Scribe_Collections.Look(ref toolLinkedBPG, "toolLinkedBPG", LookMode.Value);
                Scribe_Collections.Look(ref toolCooldownTimes, "toolCooldownTimes", LookMode.Value);
                Scribe_Collections.Look(ref toolArmorPenetrationSharps, "toolArmorPenetrationSharps", LookMode.Value);
                Scribe_Collections.Look(ref toolArmorPenetrationBlunts, "toolArmorPenetrationBlunts", LookMode.Value);
                Scribe_Collections.Look(ref toolPowers, "toolPowers", LookMode.Value);
            }
        }
    }
}
