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
        public List<string> modified_toolIds = new List<string>();
        public List<List<ToolCapacityDef>> modified_ToolCapacityDefs = new List<List<ToolCapacityDef>>();
        public List<List<string>> modified_ToolCapacityStrings = new List<List<string>>(); //for save/load purposes
        public List<BodyPartGroupDef> modified_ToolLinkedBodyPartsGroupDefs = new List<BodyPartGroupDef>();
        public List<string> modified_ToolLinkedBPGStrings = new List<string>();
        public List<float> modified_ToolCooldownTimes = new List<float>();
        public List<float> modified_ToolArmorPenetrationSharps = new List<float>();
        public List<float> modified_ToolArmorPenetrationBlunts = new List<float>();
        public List<float> modified_ToolPowers = new List<float>();
        public List<float> modified_ToolChanceFactors = new List<float>();
        public List<ToolCE> modified_Tools = new List<ToolCE>();

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
                    StringifyTool();
                }
                Scribe_Values.Look(ref defName, "defName");
                Scribe_Values.Look(ref parentModPackageId, "parentModPackageId");
                Scribe_Values.Look(ref isCustomized, "isCustomized");

                Scribe_Collections.Look(ref modified_toolIds, "toolIds", LookMode.Value);
                Scribe_Collections.Look(ref modified_ToolCapacityStrings, "toolCapacityStrings", LookMode.Value);
                Scribe_Collections.Look(ref modified_ToolLinkedBPGStrings, "toolLinkedBPGStrings", LookMode.Value);
                Scribe_Collections.Look(ref modified_ToolCooldownTimes, "toolCooldownTimes", LookMode.Value);
                Scribe_Collections.Look(ref modified_ToolArmorPenetrationSharps, "toolArmorPenetrationSharps", LookMode.Value);
                Scribe_Collections.Look(ref modified_ToolArmorPenetrationBlunts, "toolArmorPenetrationBlunts", LookMode.Value);
                Scribe_Collections.Look(ref modified_ToolPowers, "toolPowers", LookMode.Value);
                Scribe_Collections.Look(ref modified_ToolChanceFactors, "toolChanceFactors", LookMode.Value);
            }

            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                DestringifyTool();
                GetOriginalData();
                RegisterSelfInDicts();
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

        //will use the modified_ fields to generate an xml patch for the def. Need to change the return type so it can be used for exporting single patches or as part of patching the whole mod
        public abstract StringBuilder PrepExport();

        //will call PrepExport and allow the user to save the resulting xml patch for just the current def
        public abstract void ExportXML();

        public virtual void SelfDelete()
        {
            APCESettings.defDataDict.Remove(def);
            modData.defDict.Remove(def);
            //TODO
        }

        public void ClearModdedTools()
        {
            modified_toolIds.Clear();
            modified_ToolCapacityDefs.Clear();
            modified_ToolCapacityStrings.Clear();
            modified_ToolLinkedBodyPartsGroupDefs.Clear();
            modified_ToolLinkedBPGStrings.Clear();
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
            modified_toolIds.Add("APCE_Tool_" + tool.id);
            List<ToolCapacityDef> toolCapacityDefs = new List<ToolCapacityDef>();
            for (int j = 0; j < tool.capacities.Count; j++)
            {
                toolCapacityDefs.Add(tool.capacities[j]);
            }
            modified_ToolCapacityDefs.Add(toolCapacityDefs);
            modified_ToolLinkedBodyPartsGroupDefs.Add(tool.linkedBodyPartsGroup);
            modified_ToolCooldownTimes.Add(tool.cooldownTime);
            modified_ToolArmorPenetrationSharps.Add(tool.armorPenetration); //will be multiplied in overrides
            modified_ToolArmorPenetrationBlunts.Add(tool.armorPenetration); //will be multiplied in overrides
            modified_ToolPowers.Add(tool.power); //will be multiplied in overrides
            modified_ToolChanceFactors.Add(tool.chanceFactor);
        }

        public void BuildTools()
        {
            modified_Tools.Clear();
            for (int i = 0; i < modified_toolIds.Count; i++)
            {
                ToolCE newTool = new ToolCE()
                {
                    id = modified_toolIds[i],
                    capacities = modified_ToolCapacityDefs[i],
                    linkedBodyPartsGroup = modified_ToolLinkedBodyPartsGroupDefs[i],
                    cooldownTime = modified_ToolCooldownTimes[i],
                    armorPenetrationSharp = modified_ToolArmorPenetrationSharps[i],
                    armorPenetrationBlunt = modified_ToolArmorPenetrationBlunts[i],
                    power = modified_ToolPowers[i],
                    chanceFactor = modified_ToolChanceFactors[i]
                };
                modified_Tools.Add(newTool);
            }
        }

        public void StringifyTool()
        {
            modified_ToolCapacityStrings.Clear();
            for (int i = 0; i < modified_ToolCapacityDefs.Count; i++)
            {
                modified_ToolCapacityStrings.Add(new List<string>());
                if (modified_ToolCapacityDefs[i].NullOrEmpty())
                    continue;
                for (int j = 0; j < modified_ToolCapacityDefs[i].Count; j++)
                {
                    modified_ToolCapacityStrings[i].Add(modified_ToolCapacityDefs[i][j].ToString());
                }
            }

            modified_ToolLinkedBPGStrings.Clear();
            for (int i = 0; i < modified_ToolLinkedBodyPartsGroupDefs.Count; i++)
            {
                modified_ToolLinkedBPGStrings.Add(modified_ToolLinkedBodyPartsGroupDefs[i]?.ToString() ?? "null"); //TODO maybe add actual null instead of the word "null"?
            }
        }

        public void DestringifyTool()
        {
            modified_ToolCapacityDefs.Clear();
            for (int i = 0; i < modified_ToolCapacityStrings.Count; i++)
            {
                modified_ToolCapacityDefs.Add(new List<ToolCapacityDef>());
                if (modified_ToolCapacityStrings[i].NullOrEmpty())
                    continue;
                for (int j = 0; j < modified_ToolCapacityStrings[i].Count; j++)
                {
                    ToolCapacityDef tcd = DefDatabase<ToolCapacityDef>.GetNamed(modified_ToolCapacityStrings[i][j]);
                    if (tcd == null)
                    {
                        Log.Warning("Warning: " + def.defName + (" has a ToolCapacityDef not in the DefDatabase. Might have been assigned one from a mod that is currently inactive. Substituting with Poke"));
                        tcd = APCEDefOf.Poke;
                    }
                    modified_ToolCapacityDefs[i].Add(tcd);
                }
            }

            modified_ToolLinkedBodyPartsGroupDefs.Clear();
            for (int i = 0; i < modified_ToolLinkedBPGStrings.Count; i++)
            {
                if (modified_ToolLinkedBPGStrings[i] == "null") //TODO see todo in Stringify about null vs "null"
                {
                    modified_ToolLinkedBodyPartsGroupDefs.Add(null);
                    continue;
                }

                BodyPartGroupDef bpgd = DefDatabase<BodyPartGroupDef>.GetNamed(modified_ToolLinkedBPGStrings[i]);
                if (bpgd == null)
                {
                    Log.Warning("Warning: " + def.defName + (" has a BodyPartGroupDef not in the DefDatabase. Might have been assigned one from a mod that is currently inactive. Substituting with RightHand"));
                    bpgd = BodyPartGroupDefOf.RightHand;
                }
                modified_ToolLinkedBodyPartsGroupDefs.Add(bpgd);
            }
        }

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
    }
}
