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
    public abstract class DefDataHolder : IExposable
    {
        public bool isCustomized = false; //this will be changed by the customization window if the user changes any values //TODO add that logic //TODO also register the DDH in the MDH's customizedDefDict. Unregister if reset to default.
        public bool alreadyRegistered = false;
        public string defName;
        public string parentModPackageId; //todo I don't seem to use this for anything?

        public Def def;

        public ModDataHolder modData;

        public float techMult;

        public List<Tool> original_Tools = new List<Tool>();
        public List<string> modified_ToolIds = new List<string>();
        public List<string> modified_ToolLabels = new List<string>();
        public List<List<ToolCapacityDef>> modified_ToolCapacityDefs = new List<List<ToolCapacityDef>>();
        public List<BodyPartGroupDef> modified_ToolLinkedBodyPartGroupDefs = new List<BodyPartGroupDef>();
        public List<float> modified_ToolCooldownTimes = new List<float>();
        public List<float> modified_ToolArmorPenetrationSharps = new List<float>();
        public List<float> modified_ToolArmorPenetrationBlunts = new List<float>();
        public List<float> modified_ToolPowers = new List<float>();
        public List<float> modified_ToolChanceFactors = new List<float>();
        public List<ToolCE> modified_Tools = new List<ToolCE>();

        internal XmlNode xml;
        internal StringBuilder patch;
        internal List<string> patchOps;

        public DefDataHolder()
        {

        }

        public DefDataHolder(Def def)
        {
            this.def = def;
            defName = def.defName;
            parentModPackageId = def.modContentPack.PackageId;
            RegisterSelfInDicts();
            GetOriginalData();
        }


        public virtual void ExposeData()
        {
            if (Scribe.mode == LoadSaveMode.LoadingVars
                || (Scribe.mode == LoadSaveMode.Saving && isCustomized == true))
            {
                if (Scribe.mode == LoadSaveMode.Saving)
                {
                    //StringifyTool();
                }
                Scribe_Values.Look(ref defName, "defName");
                Scribe_Values.Look(ref parentModPackageId, "parentModPackageId");
                Scribe_Values.Look(ref isCustomized, "isCustomized");

                Scribe_Collections.Look(ref modified_ToolIds, "toolIds", LookMode.Value);
                Scribe_Collections.Look(ref modified_ToolLabels, "toolLabels", LookMode.Value);
                Scribe_Collections.LookListOfLists(ref modified_ToolCapacityDefs, true, "toolCapacityDefs", LookMode.Def);
                Scribe_Collections.Look(ref modified_ToolLinkedBodyPartGroupDefs, true, "toolLinkedBodyPartGroupDefs", LookMode.Def);
                Scribe_Collections.Look(ref modified_ToolCooldownTimes, "toolCooldownTimes", LookMode.Value);
                Scribe_Collections.Look(ref modified_ToolArmorPenetrationSharps, "toolArmorPenetrationSharps", LookMode.Value);
                Scribe_Collections.Look(ref modified_ToolArmorPenetrationBlunts, "toolArmorPenetrationBlunts", LookMode.Value);
                Scribe_Collections.Look(ref modified_ToolPowers, "toolPowers", LookMode.Value);
                Scribe_Collections.Look(ref modified_ToolChanceFactors, "toolChanceFactors", LookMode.Value);
            }

            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                //DestringifyTool();
                GetOriginalData();
                RegisterSelfInDicts();
                FixNullLists();
            }
        }


        //will get relevant values from the def and fill the original_ fields
        public abstract void GetOriginalData();

        public virtual void WhenLoaded()
        {
            return;
        }

        //will use modData and original_ fields to autocalculate modified_ fields
        public abstract void AutoCalculate();

        public virtual void PrePatch()
        {
            return;
        }

        //used for things like resolving references after all DataHolders have been loaded
        public virtual void PostPatch()
        {
            return;
        }

        //will use the modified_ fields to edit the def
        public abstract void Patch();

        //will use the modified_ fields to generate an xml patch for the def
        public virtual StringBuilder ExportXML()
        {
            patch = new StringBuilder();

            APCEPatchExport.CleanPatchOpsList(ref patchOps);

            APCEPatchExport.AddPatchHeader(def, patch);
            APCEPatchExport.AddNecessaryNodes(xml, def, patch, patchOps);
            APCEPatchExport.AddPatchOps(xml, def, patch, patchOps);

            return patch;
        }

        public virtual void SelfDelete()
        {
            APCESettings.defDataDict.Remove(def);
            modData.defDict.Remove(def);
            //TODO
        }

        public void ClearModdedTools()
        {
            modified_ToolIds.Clear();
            modified_ToolLabels.Clear();
            modified_ToolCapacityDefs.Clear();
            modified_ToolLinkedBodyPartGroupDefs.Clear();
            modified_ToolCooldownTimes.Clear();
            modified_ToolArmorPenetrationSharps.Clear();
            modified_ToolArmorPenetrationBlunts.Clear();
            modified_ToolPowers.Clear();
            modified_ToolChanceFactors.Clear();
            modified_Tools.Clear();
        }

        public virtual void ModToolAtIndex(int i)
        {
            Tool tool = original_Tools[i];
            modified_ToolIds.Add("APCE_Tool_" + tool.id);
            modified_ToolLabels.Add(tool.label);
            List<ToolCapacityDef> toolCapacityDefs = new List<ToolCapacityDef>();
            for (int j = 0; j < tool.capacities.Count; j++)
            {
                toolCapacityDefs.Add(tool.capacities[j]);
            }
            modified_ToolCapacityDefs.Add(toolCapacityDefs);
            modified_ToolLinkedBodyPartGroupDefs.Add(tool.linkedBodyPartsGroup);
            modified_ToolCooldownTimes.Add(tool.cooldownTime);
            modified_ToolArmorPenetrationSharps.Add(tool.armorPenetration); //will be multiplied in overrides
            modified_ToolArmorPenetrationBlunts.Add(tool.armorPenetration); //will be multiplied in overrides
            modified_ToolPowers.Add(tool.power); //will be multiplied in overrides
            modified_ToolChanceFactors.Add(tool.chanceFactor);
        }

        public void BuildTools()
        {
            modified_Tools.Clear();
            for (int i = 0; i < modified_ToolIds.Count; i++)
            {
                ToolCE newTool = new ToolCE()
                {
                    id = modified_ToolIds[i],
                    label = modified_ToolLabels[i],
                    capacities = modified_ToolCapacityDefs[i],
                    linkedBodyPartsGroup = modified_ToolLinkedBodyPartGroupDefs[i],
                    cooldownTime = modified_ToolCooldownTimes[i],
                    armorPenetrationSharp = modified_ToolArmorPenetrationSharps[i],
                    armorPenetrationBlunt = modified_ToolArmorPenetrationBlunts[i],
                    power = modified_ToolPowers[i],
                    chanceFactor = modified_ToolChanceFactors[i]
                };
                modified_Tools.Add(newTool);
            }
        }

        //public void StringifyTool()
        //{
        //    modified_ToolCapacityStrings.Clear();
        //    for (int i = 0; i < modified_ToolCapacityDefs.Count; i++)
        //    {
        //        modified_ToolCapacityStrings.Add(new List<string>());
        //        if (modified_ToolCapacityDefs[i].NullOrEmpty())
        //            continue;
        //        for (int j = 0; j < modified_ToolCapacityDefs[i].Count; j++)
        //        {
        //            modified_ToolCapacityStrings[i].Add(modified_ToolCapacityDefs[i][j].ToString());
        //        }
        //    }

        //    modified_ToolLinkedBPGStrings.Clear();
        //    for (int i = 0; i < modified_ToolLinkedBodyPartsGroupDefs.Count; i++)
        //    {
        //        modified_ToolLinkedBPGStrings.Add(modified_ToolLinkedBodyPartsGroupDefs[i]?.ToString() ?? "null"); //TODO maybe add actual null instead of the word "null"?
        //    }
        //}

        //public void DestringifyTool()
        //{
        //    modified_ToolCapacityDefs.Clear();
        //    for (int i = 0; i < modified_ToolCapacityStrings.Count; i++)
        //    {
        //        modified_ToolCapacityDefs.Add(new List<ToolCapacityDef>());
        //        if (modified_ToolCapacityStrings[i].NullOrEmpty())
        //            continue;
        //        for (int j = 0; j < modified_ToolCapacityStrings[i].Count; j++)
        //        {
        //            ToolCapacityDef tcd = DefDatabase<ToolCapacityDef>.GetNamed(modified_ToolCapacityStrings[i][j]);
        //            if (tcd == null)
        //            {
        //                Log.Warning("Warning: " + def.defName + (" has a ToolCapacityDef not in the DefDatabase. Might have been assigned one from a mod that is currently inactive. Substituting with Poke"));
        //                tcd = APCEDefOf.Poke;
        //            }
        //            modified_ToolCapacityDefs[i].Add(tcd);
        //        }
        //    }

        //    modified_ToolLinkedBodyPartsGroupDefs.Clear();
        //    for (int i = 0; i < modified_ToolLinkedBPGStrings.Count; i++)
        //    {
        //        if (modified_ToolLinkedBPGStrings[i] == "null") //TODO see todo in Stringify about null vs "null"
        //        {
        //            modified_ToolLinkedBodyPartsGroupDefs.Add(null);
        //            continue;
        //        }

        //        BodyPartGroupDef bpgd = DefDatabase<BodyPartGroupDef>.GetNamed(modified_ToolLinkedBPGStrings[i]);
        //        if (bpgd == null)
        //        {
        //            Log.Warning("Warning: " + def.defName + (" has a BodyPartGroupDef not in the DefDatabase. Might have been assigned one from a mod that is currently inactive. Substituting with RightHand"));
        //            bpgd = BodyPartGroupDefOf.RightHand;
        //        }
        //        modified_ToolLinkedBodyPartsGroupDefs.Add(bpgd);
        //    }
        //}

        public virtual void RegisterSelfInDicts()
        {
            APCESettings.defDataDict[def] = this;
            modData = DataHolderUtils.ReturnModDataOrDefault(def);
            modData.defDict[def] = this;
        }

        public virtual void DelayedRegister()
        {
            modData.delayedRegistrations.Add(this);
        }

        public void RemoveTool(int i)
        {
            modified_ToolIds.RemoveAt(i);
            modified_ToolLabels.RemoveAt(i);
            modified_ToolCapacityDefs.RemoveAt(i);
            modified_ToolLinkedBodyPartGroupDefs.RemoveAt(i);
            modified_ToolCooldownTimes.RemoveAt(i);
            modified_ToolArmorPenetrationSharps.RemoveAt(i);
            modified_ToolArmorPenetrationBlunts.RemoveAt(i);
            modified_ToolPowers.RemoveAt(i);
            modified_ToolChanceFactors.RemoveAt(i);
        }

        public void RemoveCapacity(int i, int j)
        {
            modified_ToolCapacityDefs[i].RemoveAt(j);
        }

        public void AddNewTool()
        {
            modified_ToolIds.Add($"APCE_Tool_{modified_ToolIds.Count}");
            modified_ToolLabels.Add("NewTool");
            modified_ToolCapacityDefs.Add(new List<ToolCapacityDef>()
            {
                APCEDefOf.Poke
            });
            modified_ToolLinkedBodyPartGroupDefs.Add(null);
            modified_ToolCooldownTimes.Add(1);
            modified_ToolArmorPenetrationSharps.Add(1);
            modified_ToolArmorPenetrationBlunts.Add(1);
            modified_ToolPowers.Add(1);
            modified_ToolChanceFactors.Add(1);
        }

        public void AddNewToolCapacity(int i)
        {
            modified_ToolCapacityDefs[i].Add(APCEDefOf.Poke);
        }

        public void FixNullLists()
        {
            if (modified_ToolLinkedBodyPartGroupDefs.NullOrEmpty())
            {
                modified_ToolLinkedBodyPartGroupDefs = new List<BodyPartGroupDef>();
                for (int i = 0; i < modified_ToolPowers.Count; i++)
                {
                    modified_ToolLinkedBodyPartGroupDefs.Add(null);
                }
            }
            if (modified_ToolLabels.NullOrEmpty())
            {
                modified_ToolLabels = new List<string>();
                for (int i = 0; i < modified_ToolPowers.Count; i++)
                {
                    modified_ToolLabels.Add(null);
                }
            }
        }

        public string GenerateToolPatchXML()
        {
            if (string.IsNullOrEmpty(defName) ||
                modified_ToolLabels.NullOrEmpty() ||
                modified_ToolCapacityDefs.NullOrEmpty() ||
                modified_ToolLinkedBodyPartGroupDefs.NullOrEmpty() ||
                modified_ToolCooldownTimes.NullOrEmpty() ||
                modified_ToolArmorPenetrationSharps.NullOrEmpty() ||
                modified_ToolArmorPenetrationBlunts.NullOrEmpty() ||
                modified_ToolPowers.NullOrEmpty() ||
                modified_ToolChanceFactors.NullOrEmpty())
            {
                Log.Warning($"Unable to generate Tools patch for {def.defName}, one of the modified value lists was either null or empty.");
                return null;
            }

            string xPath;

            if (this is DefDataHolderHediff)
            {
                xPath = $"\t\t<xpath>Defs/HediffDef[defName=\"{defName}\"]/comps/li[@Class=\"HediffCompProperties_VerbGiver\"]/tools</xpath>";
            }
            else
            {
                xPath = $"\t\t<xpath>Defs/ThingDef[defName=\"{defName}\"]/tools</xpath>";
            }

            StringBuilder patch = new StringBuilder();

            patch.AppendLine("\t<Operation Class=\"PatchOperationReplace\">");
            patch.AppendLine(xPath);
            patch.AppendLine("\t\t<value>");
            patch.AppendLine("\t\t\t<tools>");

            for (int i = 0; i < modified_ToolCapacityDefs.Count; i++)
            {
                patch.AppendLine("\t\t\t\t<li Class=\"CombatExtended.ToolCE\">");
                patch.AppendLine($"\t\t\t\t\t<label>{modified_ToolLabels[i]}</label>");

                if (modified_ToolCapacityDefs[i] != null && modified_ToolCapacityDefs[i].Count > 0)
                {
                    patch.AppendLine("\t\t\t\t\t<capacities>");
                    foreach (var cap in modified_ToolCapacityDefs[i])
                    {
                        if (cap != null)
                            patch.AppendLine($"\t\t\t\t\t\t<li>{cap.defName}</li>");
                    }
                    patch.AppendLine("\t\t\t\t\t</capacities>");
                }

                patch.AppendLine($"\t\t\t\t\t<power>{modified_ToolPowers[i]}</power>");
                patch.AppendLine($"\t\t\t\t\t<cooldownTime>{modified_ToolCooldownTimes[i]}</cooldownTime>");

                if (modified_ToolLinkedBodyPartGroupDefs[i] != null)
                {
                    patch.AppendLine($"\t\t\t\t\t<linkedBodyPartsGroup>{modified_ToolLinkedBodyPartGroupDefs[i].defName}</linkedBodyPartsGroup>");
                }

                if (modified_ToolChanceFactors[i] != 0f)
                {
                    patch.AppendLine($"\t\t\t\t\t<chanceFactor>{modified_ToolChanceFactors[i]}</chanceFactor>");
                }

                if (modified_ToolArmorPenetrationSharps[i] > 0f)
                {
                    patch.AppendLine($"\t\t\t\t\t<armorPenetrationSharp>{modified_ToolArmorPenetrationSharps[i]}</armorPenetrationSharp>");
                }

                if (modified_ToolArmorPenetrationBlunts[i] > 0f)
                {
                    patch.AppendLine($"\t\t\t\t\t<armorPenetrationBlunt>{modified_ToolArmorPenetrationBlunts[i]}</armorPenetrationBlunt>");
                }

                patch.AppendLine("\t\t\t\t</li>");
            }

            patch.AppendLine("\t\t\t</tools>");
            patch.AppendLine("\t\t</value>");
            patch.AppendLine("\t</Operation>");

            return patch.ToString();
        }
    }
}
