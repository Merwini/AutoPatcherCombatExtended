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

        public List<Tool> original_Tools = new List<Tool>();
        public List<string> toolIds = new List<string>();
        public List<List<string>> toolCapacities = new List<List<string>>();
        public List<string> toolLinkedBPG = new List<string>();
        public List<float> toolCooldownTimes = new List<float>();
        public List<float> toolArmorPenetrationSharps = new List<float>();
        public List<float> toolArmorPenetrationBlunts = new List<float>();
        public List<float> toolPowers = new List<float>();
        public List<float> toolChanceFactors = new List<float>();
        public List<ToolCE> modified_Tools = new List<ToolCE>();

        public bool IsCustomized => isCustomized;
         
        public DefDataHolder(Def def)
        {
            this.def = def;
            defName = def.defName;
            parentModPackageId = def.modContentPack.PackageId;
            modData = DataHolderUtils.ReturnModDataOrDefault(def);
            GetOriginalData();
            AutoCalculate();
        }

        public virtual void ExposeData()
        {
            if (Scribe.mode == LoadSaveMode.Saving)
            {
                //convert tools into lists before saving those lists
                ClearToolSerializedLists();
                SerializeTools();
            }

            if (Scribe.mode == LoadSaveMode.LoadingVars
                || (Scribe.mode == LoadSaveMode.Saving && isCustomized == true))
            {
                Scribe_Values.Look(ref defName, "defName");
                Scribe_Values.Look(ref parentModPackageId, "parentModPackageId");
                Scribe_Values.Look(ref isCustomized, "isCustomized");

                Scribe_Collections.Look(ref toolIds, "toolIds", LookMode.Value);
                Scribe_Collections.Look(ref toolCapacities, "toolCapacities", LookMode.Value);
                Scribe_Collections.Look(ref toolLinkedBPG, "toolLinkedBPG", LookMode.Value);
                Scribe_Collections.Look(ref toolCooldownTimes, "toolCooldownTimes", LookMode.Value);
                Scribe_Collections.Look(ref toolArmorPenetrationSharps, "toolArmorPenetrationSharps", LookMode.Value);
                Scribe_Collections.Look(ref toolArmorPenetrationBlunts, "toolArmorPenetrationBlunts", LookMode.Value);
                Scribe_Collections.Look(ref toolPowers, "toolPowers", LookMode.Value);
            }

            if (Scribe.mode == LoadSaveMode.LoadingVars
                && defName != null)
            {
                def = DefDatabase<Def>.GetNamed(defName, false);
                if (def != null)
                {
                    GetOriginalData();
                    APCESettings.defDataDict.Add(def.defName, this);

                    //convert lists into toolsCE after loading the lists
                    DeserializeTools();
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

        public void ClearToolSerializedLists()
        {
            toolIds.Clear();
            toolCapacities.Clear();
            toolLinkedBPG.Clear();
            toolCooldownTimes.Clear();
            toolArmorPenetrationSharps.Clear();
            toolArmorPenetrationBlunts.Clear();
            toolPowers.Clear();
            toolChanceFactors.Clear();
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
                    toolChanceFactors[i] = modified_Tools[i].chanceFactor;
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
                    tool.chanceFactor = toolChanceFactors[i];

                    modified_Tools.Add(tool);
                }
            }
        }
    }
}
