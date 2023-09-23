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
        public DefDataHolderHediff(HediffDef def)
        {
            this.def = def;
            defName = def.defName;
            parentModPackageId = def.modContentPack.PackageId;
            modData = DataHolderUtil.ReturnModDataOrDefault(def);

            GetOriginalData();
        }

        HediffDef def;

        List<float> original_ArmorRatingSharp = new List<float>();
        List<float> original_ArmorRatingBlunt = new List<float>();
        List<float> original_ArmorRatingHeat = new List<float>();
        List<Tool> original_Tools;
        HediffCompProperties_VerbGiver verbGiver;

        List<float> modified_ArmorRatingSharp = new List<float>();
        List<float> modified_ArmorRatingBlunt = new List<float>();
        List<float> modified_ArmorRatingHeat = new List<float>();
        List<Tool> modified_Tools = new List<Tool>();

        //TODO use more basic data structures so it can be serialized

        public override void GetOriginalData()
        {
            verbGiver = def.comps?.Find((HediffCompProperties c) => c is HediffCompProperties_VerbGiver) as HediffCompProperties_VerbGiver;
            if (verbGiver != null && verbGiver.tools != null)
            {
                original_Tools = verbGiver.tools;
            }

            if (def.stages == null)
                return;
            for (int i = 0; i < def.stages.Count; i++)
            {
                float armorRatingSharp = 0f;
                float armorRatingBlunt = 0f;
                float armorRatingHeat = 0f;

                if (def.stages[i].statOffsets != null)
                {
                    armorRatingSharp = def.stages[i].statOffsets.GetStatValueFromList(StatDefOf.ArmorRating_Sharp, 0);
                    armorRatingBlunt = def.stages[i].statOffsets.GetStatValueFromList(StatDefOf.ArmorRating_Blunt, 0);
                    armorRatingHeat = def.stages[i].statOffsets.GetStatValueFromList(StatDefOf.ArmorRating_Heat, 0);
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
            if (!def.stages.NullOrEmpty())
            {
                for (int i = 0; i < def.stages?.Count; i++)
                {
                    modified_ArmorRatingSharp.Add(original_ArmorRatingSharp[i] * modData.hediffSharpMult);
                    modified_ArmorRatingBlunt.Add(original_ArmorRatingBlunt[i] * modData.hediffBluntMult);
                }
            }
        }
        public override void Patch()
        {
            if (!def.stages.NullOrEmpty())
            {
                for (int i = 0; i < def.stages.Count; i++)
                {
                    DataHolderUtil.AddOrChangeStat(def.stages[i].statOffsets, StatDefOf.ArmorRating_Sharp, modified_ArmorRatingSharp[i]);
                    DataHolderUtil.AddOrChangeStat(def.stages[i].statOffsets, StatDefOf.ArmorRating_Blunt, modified_ArmorRatingBlunt[i]);
                    DataHolderUtil.AddOrChangeStat(def.stages[i].statOffsets, StatDefOf.ArmorRating_Heat, modified_ArmorRatingHeat[i]);
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


    }
}
