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
        public string defName;
        public string parentModPackageId;

        public Def def;

        public ModDataHolder modData;

        public float techMult;

        public List<Tool> original_Tools = new List<Tool>();
        public List<string> modified_toolIds = new List<string>();
        public List<List<string>> modified_toolCapacities = new List<List<string>>();
        public List<string> modified_toolLinkedBPG = new List<string>();
        public List<float> modified_toolCooldownTimes = new List<float>();
        public List<float> modified_toolArmorPenetrationSharps = new List<float>();
        public List<float> modified_toolArmorPenetrationBlunts = new List<float>();
        public List<float> modified_toolPowers = new List<float>();
        public List<float> modified_toolChanceFactors = new List<float>();
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
            if (Scribe.mode == LoadSaveMode.Saving)
            {
            }

            if (Scribe.mode == LoadSaveMode.LoadingVars
                || (Scribe.mode == LoadSaveMode.Saving && isCustomized == true))
            {
                Scribe_Values.Look(ref defName, "defName");
                Scribe_Values.Look(ref parentModPackageId, "parentModPackageId");
                Scribe_Values.Look(ref isCustomized, "isCustomized");

                Scribe_Values.Look(ref techMult, "techMult", 1f);

                Scribe_Collections.Look(ref modified_toolIds, "toolIds", LookMode.Value);
                Scribe_Collections.Look(ref modified_toolCapacities, "toolCapacities", LookMode.Value);
                Scribe_Collections.Look(ref modified_toolLinkedBPG, "toolLinkedBPG", LookMode.Value);
                Scribe_Collections.Look(ref modified_toolCooldownTimes, "toolCooldownTimes", LookMode.Value);
                Scribe_Collections.Look(ref modified_toolArmorPenetrationSharps, "toolArmorPenetrationSharps", LookMode.Value);
                Scribe_Collections.Look(ref modified_toolArmorPenetrationBlunts, "toolArmorPenetrationBlunts", LookMode.Value);
                Scribe_Collections.Look(ref modified_toolPowers, "toolPowers", LookMode.Value);
                Scribe_Collections.Look(ref modified_toolChanceFactors, "toolChanceFactors", LookMode.Value);
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
            List<string> toolCapacityStrings = new List<string>();
            for (int j = 0; j < tool.capacities.Count; j++)
            {
                toolCapacityStrings.Add(tool.capacities[j].ToString());
            }
            modified_toolCapacities.Add(toolCapacityStrings);
            modified_toolLinkedBPG.Add(tool.linkedBodyPartsGroup.ToString());
            modified_toolCooldownTimes.Add(tool.cooldownTime);
            modified_toolArmorPenetrationSharps.Add(tool.armorPenetration); //will be multiplied in overrides
            modified_toolArmorPenetrationSharps.Add(tool.armorPenetration); //will be multiplied in overrides
            modified_toolPowers.Add(tool.power);
            modified_toolChanceFactors.Add(tool.chanceFactor);
        }

        public void ClearToolSerializedLists()
        {
            modified_toolIds.Clear();
            modified_toolCapacities.Clear();
            modified_toolLinkedBPG.Clear();
            modified_toolCooldownTimes.Clear();
            modified_toolArmorPenetrationSharps.Clear();
            modified_toolArmorPenetrationBlunts.Clear();
            modified_toolPowers.Clear();
            modified_toolChanceFactors.Clear();
        }

        public void SerializeTools()
        {
            if (!modified_Tools.NullOrEmpty())
            {
                for (int i = 0; i < modified_Tools.Count; i++)
                {
                    modified_toolIds[i] = modified_Tools[i].id;

                    for (int j = 0; j < modified_Tools[i].capacities.Count; j++)
                    {
                        modified_toolCapacities[i][j] = modified_Tools[i].capacities[j].ToString();
                    }

                    modified_toolLinkedBPG[i] = modified_Tools[i].linkedBodyPartsGroup.ToString();
                    modified_toolCooldownTimes[i] = modified_Tools[i].cooldownTime;
                    modified_toolArmorPenetrationSharps[i] = modified_Tools[i].armorPenetrationSharp;
                    modified_toolArmorPenetrationBlunts[i] = modified_Tools[i].armorPenetrationBlunt;
                    modified_toolPowers[i] = modified_Tools[i].power;
                    modified_toolChanceFactors[i] = modified_Tools[i].chanceFactor;
                }
            }
        }

        public void DeserializeTools()
        {
            modified_Tools.Clear();
            if (!modified_toolIds.NullOrEmpty())
            {
                for (int i = 0; i < modified_toolIds.Count; i++)
                {
                    ToolCE tool = new ToolCE();
                    tool.id = modified_toolIds[i];

                    //tool.capacities is instantiated as an empty list by default
                    for (int j = 0; j < modified_toolCapacities.Count; j++)
                    {
                        tool.capacities.Add(DefDatabase<ToolCapacityDef>.GetNamed(modified_toolCapacities[i][j]));
                    }

                    tool.linkedBodyPartsGroup = DefDatabase<BodyPartGroupDef>.GetNamed(modified_toolLinkedBPG[i]);
                    tool.cooldownTime = modified_toolCooldownTimes[i];
                    tool.armorPenetrationSharp = modified_toolArmorPenetrationSharps[i];
                    tool.armorPenetrationBlunt = modified_toolArmorPenetrationBlunts[i];
                    tool.power = modified_toolPowers[i];
                    tool.chanceFactor = modified_toolChanceFactors[i];

                    modified_Tools.Add(tool);
                }
            }
        }

        public void RegisterSelfInDict()
        {
            APCESettings.defDataDict.Add(this.defName, this);
        }
    }
}
