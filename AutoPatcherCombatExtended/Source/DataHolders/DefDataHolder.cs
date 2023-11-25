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
        public bool isCustomized = false; //this will be changed by the customization window if the user changes any values
        public bool defGeneratedAlready = false;
        public string defName;
        public string parentModPackageId;

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

        public bool IsCustomized => isCustomized;
         
        public DefDataHolder(Def def)
        {
            this.def = def;
            defName = def.defName;
            parentModPackageId = def.modContentPack.PackageId;
            modData = DataHolderUtils.ReturnModDataOrDefault(def);
            RegisterSelfInDict();
            GetOriginalData();
            AutoCalculate();
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

                Scribe_Values.Look(ref techMult, "techMult", 1f);

                Scribe_Collections.Look(ref modified_toolIds, "toolIds", LookMode.Value);
                Scribe_Collections.Look(ref modified_ToolCapacityStrings, "toolCapacityStrings", LookMode.Value);
                Scribe_Collections.Look(ref modified_ToolLinkedBPGStrings, "toolLinkedBPGStrings", LookMode.Value);
                Scribe_Collections.Look(ref modified_ToolCooldownTimes, "toolCooldownTimes", LookMode.Value);
                Scribe_Collections.Look(ref modified_ToolArmorPenetrationSharps, "toolArmorPenetrationSharps", LookMode.Value);
                Scribe_Collections.Look(ref modified_ToolArmorPenetrationBlunts, "toolArmorPenetrationBlunts", LookMode.Value);
                Scribe_Collections.Look(ref modified_ToolPowers, "toolPowers", LookMode.Value);
                Scribe_Collections.Look(ref modified_ToolChanceFactors, "toolChanceFactors", LookMode.Value);
                if (Scribe.mode == LoadSaveMode.LoadingVars)
                {
                    DestringifyTool();
                }
            }

            if (Scribe.mode == LoadSaveMode.LoadingVars
                && defName != null)
            {
                def = DefDatabase<Def>.GetNamed(defName, false);
                if (def != null) //TODO exception for AmmoSet, since they will not exist in DefDatabase until Patch is run
                {
                    GetOriginalData();
                    RegisterSelfInDict();
                }
            }
        }

        //will get relevant values from the def and fill the original_ fields
        public abstract void GetOriginalData();
        
        //will use modData and original_ fields to autocalculate modified_ fields
        public abstract void AutoCalculate();

        //will use the modified_ fields to edit the def
        public abstract void Patch();

        //will use the modified_ fields to generate an xml patch for the def. Need to change the return type so it can be used for exporting single patches or as part of patching the whole mod
        public abstract StringBuilder PrepExport();

        //will call PrepExport and allow the user to save the resulting xml patch for just the current def
        public abstract void ExportXML();

        public virtual void SelfDelete()
        {
            APCESettings.defDataDict.Remove(this.defName);
            //TODO
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
            modified_ToolArmorPenetrationSharps.Add(tool.armorPenetration); //will be multiplied in overrides
            modified_ToolPowers.Add(tool.power);
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
                modified_ToolLinkedBPGStrings.Add(modified_ToolLinkedBodyPartsGroupDefs.ToString());
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
                BodyPartGroupDef bpgd = DefDatabase<BodyPartGroupDef>.GetNamed(modified_ToolLinkedBPGStrings[i]);
                if (bpgd == null)
                {
                    Log.Warning("Warning: " + def.defName + (" has a BodyPartGroupDef not in the DefDatabase. Might have been assigned one from a mod that is currently inactive. Substituting with RightHand"));
                    bpgd = BodyPartGroupDefOf.RightHand;
                }
                modified_ToolLinkedBodyPartsGroupDefs.Add(bpgd);
            }
        }

        public void RegisterSelfInDict()
        {
            APCESettings.defDataDict.Add(this.defName, this);
        }
    }
}
