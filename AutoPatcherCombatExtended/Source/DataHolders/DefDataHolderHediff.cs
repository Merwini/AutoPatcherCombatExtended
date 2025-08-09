using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
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

        internal List<float> modified_ArmorRatingSharp = new List<float>();
        internal List<float> modified_ArmorRatingBlunt = new List<float>();
        internal List<float> modified_ArmorRatingHeat = new List<float>();
        
        public override void GetOriginalData()
        {
            //constructed by APCEController, def assigned by constructor
            if (def != null && hediffDef == null)
            {
                this.hediffDef = def as HediffDef;
            }
            //constructed by SaveLoad, thingDef loaded from xml
            else if (hediffDef != null && def == null)
            {
                def = hediffDef;
            }

            StartNewLogEntry();
            logBuilder.AppendLine($"Starting GetOriginalData log entry for {def?.defName ?? "NULL DEF"}");

            try
            {
                verbGiver = hediffDef.comps?.Find((HediffCompProperties c) => c is HediffCompProperties_VerbGiver) as HediffCompProperties_VerbGiver;
                if (verbGiver != null && verbGiver.tools != null)
                {
                    original_Tools = verbGiver.tools.ToList();
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
            catch (Exception ex)
            {
                logBuilder.AppendLine($"Exception in GetOriginalData for: {def?.defName ?? "NULL DEF"}");
                logBuilder.AppendLine(ex.ToString());
                threwError = true;
            }
            finally
            {
                //TODO verbose logging
                PrintLog();
            }
        }

        public override void AutoCalculate()
        {
            StartNewLogEntry();
            logBuilder.AppendLine($"Starting AutoCalculate log entry for ammoset for {def?.defName ?? "NULL DEF"}");

            try
            {
                if (!original_Tools.NullOrEmpty())
                {
                    ClearModdedTools();
                    for (int i = 0; i < original_Tools.Count; i++)
                    {
                        ModToolAtIndex(i);
                    }
                }
                if (!hediffDef.stages.NullOrEmpty())
                {
                    for (int i = 0; i < hediffDef.stages.Count; i++)
                    {
                        modified_ArmorRatingSharp.Add(original_ArmorRatingSharp[i] * ModData.hediffSharpMult);
                        modified_ArmorRatingBlunt.Add(original_ArmorRatingBlunt[i] * ModData.hediffBluntMult);
                        modified_ArmorRatingHeat.Add(original_ArmorRatingHeat[i]);
                    }
                }
            }
            catch (Exception ex)
            {
                logBuilder.AppendLine($"Exception in AutoCalculate for: {def?.defName ?? "NULL DEF"}");
                logBuilder.AppendLine(ex.ToString());
                threwError = true;
            }
            finally
            {
                //TODO verbose logging
                PrintLog();
            }
        }
        public override void ApplyPatch()
        {
            StartNewLogEntry();
            logBuilder.AppendLine($"Starting ApplyPatch log entry for ammoset for {def?.defName ?? "NULL DEF"}");

            try
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
            catch (Exception ex)
            {
                logBuilder.AppendLine($"Exception in Patch for: {def?.defName ?? "NULL DEF"}");
                logBuilder.AppendLine(ex.ToString());
                threwError = true;
            }
            finally
            {
                //TODO verbose logging
                PrintLog();
            }
        }

        public override StringBuilder ExportXML()
        {
            xml = DataHolderUtils.GetXmlForDef(hediffDef);

            //for keeping track of whether any of the patchOps for armor values have had to add a statOffets node for a given <stages> index, so it isn't re-added by one of the others
            List<bool> addedStatOffsets = new List<bool>();
            for (int i = 0; i < modified_ArmorRatingSharp.Count; i++)
            {
                addedStatOffsets.Add(false);
            }

            patchOps = new List<string>();
            patchOps.Add(GenerateHediffStagesArmorPatch(xml, "ArmorRating_Sharp", modified_ArmorRatingSharp, original_ArmorRatingSharp));
            patchOps.Add(GenerateHediffStagesArmorPatch(xml, "ArmorRating_Blunt", modified_ArmorRatingBlunt, original_ArmorRatingBlunt));
            patchOps.Add(GenerateHediffStagesArmorPatch(xml, "ArmorRating_Heat", modified_ArmorRatingHeat, original_ArmorRatingHeat));

            patchOps.Add(GenerateToolPatchXML());

            base.ExportXML();

            return patch;

            //based on APCEPatchExport.GeneratePatchOperationFor()
            string GenerateHediffStagesArmorPatch(XmlNode node, string targetStat, List<float> values, List<float> original_values)
            {
                //no need for warning/error, as hediffs with VerbGivers don't usually have stages but this will run anyway
                if (values.NullOrEmpty())
                {
                    return null;
                }

                if (node == null)
                {
                    Log.Warning("Cannot generate patch: XML node is null.");
                    return null;
                }

                XmlNode defNameNode = node.SelectSingleNode("defName");
                if (defNameNode == null)
                {
                    Log.Warning("Cannot generate patch: defName not found in XML node.");
                    return null;
                }

                string defName = defNameNode.InnerText;

                XmlNode stagesNode = node.SelectSingleNode("stages");
                if (stagesNode == null)
                {
                    Log.Warning($"Cannot generate patch: could not find <stages> node for {defName}");
                    return null;
                }

                XmlNodeList stageNodes = stagesNode.SelectNodes("li");
                if (stageNodes == null || stageNodes.Count == 0)
                {
                    Log.Warning($"Cannot generate patch: No <stages> found for HediffDef '{defName}'");
                    return null;
                }

                StringBuilder stagesPatch = new StringBuilder();

                for (int i = 0; i < values.Count; i++)
                {
                    //skip making a patch for a value that is unchanged
                    if (values[i] == original_values[i])
                    {
                        continue;
                    }

                    float value = values[i];

                    //try to find matching stage node
                    if (i >= stageNodes.Count)
                    {
                        Log.Warning($"HediffDef '{defName}' has fewer stages than expected (index {i}). Skipping extra values.");
                        break;
                    }

                    XmlNode stage = stageNodes.Item(i);
                    bool targetExists = false;

                    XmlNode statOffsets = stage.SelectSingleNode("statOffsets");
                    if (statOffsets != null)
                    {
                        foreach (XmlNode child in statOffsets.ChildNodes)
                        {
                            if (child.Name == targetStat)
                            {
                                targetExists = true;
                                break;
                            }
                        }
                    }

                    string baseXPath = $"Defs/HediffDef[defName=\"{defName}\"]/stages/li[{i + 1}]";
                    string xpath = $"{baseXPath}/statOffsets";

                    //add statOffsets node if it is missing and not already added
                    if (statOffsets == null && value != 0 && !addedStatOffsets[i])
                    {
                        stagesPatch.AppendLine("\t<Operation Class=\"PatchOperationAdd\">");
                        stagesPatch.AppendLine($"\t\t<xpath>{baseXPath}</xpath>");
                        stagesPatch.AppendLine("\t\t<value>");
                        stagesPatch.AppendLine("\t\t\t<statOffsets></statOffsets>"); //can I just do <statOffsets /> ?
                        stagesPatch.AppendLine("\t\t</value>");
                        stagesPatch.AppendLine("\t</Operation>");
                        addedStatOffsets[i] = true;
                    }

                    if (value == 0)
                    {
                        if (targetExists)
                        {
                            //remove node
                            stagesPatch.AppendLine("\t<Operation Class=\"PatchOperationReplace\">");
                            stagesPatch.AppendLine($"\t\t<xpath>{xpath}/{targetStat}</xpath>");
                            stagesPatch.AppendLine("\t\t<value />");
                            stagesPatch.AppendLine("\t</Operation>");
                        }
                        // else: no need to add or remove what doesn't exist
                    }
                    else
                    {
                        if (targetExists)
                        {
                            //replace value
                            stagesPatch.AppendLine("\t<Operation Class=\"PatchOperationReplace\">");
                            stagesPatch.AppendLine($"\t\t<xpath>{xpath}/{targetStat}</xpath>");
                            stagesPatch.AppendLine("\t\t<value>");
                            stagesPatch.AppendLine($"\t\t\t<{targetStat}>{value}</{targetStat}>");
                            stagesPatch.AppendLine("\t\t</value>");
                            stagesPatch.AppendLine("\t</Operation>");
                        }
                        else
                        {
                            // Add new node
                            stagesPatch.AppendLine("\t<Operation Class=\"PatchOperationAdd\">");
                            stagesPatch.AppendLine($"\t\t<xpath>{xpath}</xpath>");
                            stagesPatch.AppendLine("\t\t<value>");
                            stagesPatch.AppendLine($"\t\t\t<{targetStat}>{value}</{targetStat}>");
                            stagesPatch.AppendLine("\t\t</value>");
                            stagesPatch.AppendLine("\t</Operation>");
                        }
                    }
                }

                return stagesPatch.ToString();
            }
        }


        public override void ModToolAtIndex(int i)
        {
            base.ModToolAtIndex(i);
            modified_ToolPowers[i] *= ModData.pawnToolPowerMult;
            modified_ToolArmorPenetrationSharps[i] *= ModData.pawnToolSharpPenetration;
            modified_ToolArmorPenetrationSharps[i] *= ModData.pawnToolBluntPenetration;
        }

        //TODO
        public override void ExposeData()
        {
            if (Scribe.mode == LoadSaveMode.LoadingVars
                || (Scribe.mode == LoadSaveMode.Saving && isCustomized == true))
            {
                Scribe_Defs.Look(ref hediffDef, "def");
                Scribe_Collections.Look(ref modified_ArmorRatingSharp, "modified_ArmorRatingSharp", LookMode.Value);
                Scribe_Collections.Look(ref modified_ArmorRatingBlunt, "modified_ArmorRatingBlunt", LookMode.Value);
                Scribe_Collections.Look(ref modified_ArmorRatingHeat, "modified_ArmorRatingHeat", LookMode.Value);
            }
            base.ExposeData();
        }
    }
}
