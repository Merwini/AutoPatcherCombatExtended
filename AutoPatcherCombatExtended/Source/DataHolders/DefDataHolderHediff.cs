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
        List<Tool> original_Tools = new List<Tool>();
        HediffCompProperties_VerbGiver verbGiver;

        List<float> modified_ArmorRatingSharp = new List<float>();
        List<float> modified_ArmorRatingBlunt = new List<float>();
        List<float> modified_ArmorRatingHeat = new List<float>();
        List<ToolCE> modified_Tools = new List<ToolCE>();

        List<string> toolIds = new List<string>();
        List<List<string>> toolCapacities = new List<List<string>>();
        List<string> toolLinkedBPG = new List<string>();
        List<float> toolCooldownTimes = new List<float>();
        List<float> toolArmorPenetrationSharps = new List<float>();
        List<float> toolArmorPenetrationBlunts = new List<float>();
        List<float> toolPowers = new List<float>();



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
                verbGiver.tools.Clear();
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

            if (Scribe.mode == LoadSaveMode.Saving)
            {
                //convert tools into lists before saving those lists
                ClearToolSerializedLists();
                SerializeTools();
            }

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

            else if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                //convert lists into toolsCE after loading the lists
                DeserializeTools();
            }

        }

        public void ClearToolSerializedLists()
        {
            toolIds.Clear();
            toolCapacities.Clear();
            toolLinkedBPG.Clear();
            toolCooldownTimes.Clear();
            toolArmorPenetrationSharps.Clear();
            toolArmorPenetrationBlunts.Clear();
            toolPowers.Clear();
        }

        public void SerializeTools()
        {
            if (!modified_Tools.NullOrEmpty())
            {
                for (int i = 0; i < modified_Tools.Count; i++)
                {
                    toolIds[i] = modified_Tools[i].id;

                    for (int j = 0; j < modified_Tools[i].capacities.Count; j++)
                    {
                        toolCapacities[i][j] = modified_Tools[i].capacities[j].ToString();
                    }

                    toolLinkedBPG[i] = modified_Tools[i].linkedBodyPartsGroup.ToString();
                    toolCooldownTimes[i] = modified_Tools[i].cooldownTime;
                    toolArmorPenetrationSharps[i] = modified_Tools[i].armorPenetrationSharp;
                    toolArmorPenetrationBlunts[i] = modified_Tools[i].armorPenetrationBlunt;
                    toolPowers[i] = modified_Tools[i].power;
                }
            }
        }

        public void DeserializeTools()
        {
            modified_Tools.Clear();
            if (!toolIds.NullOrEmpty())
            {
                for (int i = 0; i < toolIds.Count; i++)
                {
                    ToolCE tool = new ToolCE();
                    tool.id = toolIds[i];

                    //tool.capacities is instantiated as an empty list by default
                    for (int j = 0; j < toolCapacities.Count; j++)
                    {
                        tool.capacities.Add(DefDatabase<ToolCapacityDef>.GetNamed(toolCapacities[i][j]));
                    }

                    tool.linkedBodyPartsGroup = DefDatabase<BodyPartGroupDef>.GetNamed(toolLinkedBPG[i]);
                    tool.cooldownTime = toolCooldownTimes[i];
                    tool.armorPenetrationSharp = toolArmorPenetrationSharps[i];
                    tool.armorPenetrationBlunt = toolArmorPenetrationBlunts[i];
                    tool.power = toolPowers[i];

                    modified_Tools.Add(tool);
                }
            }
        }
    }
}
