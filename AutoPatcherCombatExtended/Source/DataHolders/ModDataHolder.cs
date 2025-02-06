using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace nuff.AutoPatcherCombatExtended
{
    public class ModDataHolder : IExposable
    {
        public ModContentPack mod;

        public string packageId;
        public bool isCustomized = true; //TODO revert to false
        //TODO methods to change values instead of modifying them directly, need this to flip isCustomized to true
        //Also TODO write a method that resets all values to match those in apceDefaults. Call this before running autocalcs or patches. This is so that, if the player changes the defaults, they don't have to restart to get those changes to permeate down to the ModDataHolders

        public Dictionary<Def, APCEConstants.NeedsPatch> defsToPatch = new Dictionary<Def, APCEConstants.NeedsPatch>();
        //for saving and loading so def names can be validated
        public List<string> defsToPatchNames = new List<string>();
        public List<string> defsToPatchTypes = new List<string>();

        //
        public Dictionary<Def, DefDataHolder> defDict = new Dictionary<Def, DefDataHolder>();

        //Needed to track if the mod has any customized defs, for use during saving. Didn't want to have to iterate through all of them to check isCustomized on each.
        public Dictionary<Def, DefDataHolder> customizedDefDict = new Dictionary<Def, DefDataHolder>();

        //toggles //todo remove
        public bool patchCustomVerbs = false;
        public bool limitWeaponMass = false;
        public bool patchHeadgearLayers = true;

        //apparel values
        public float apparelSharpMult = 10f;
        public float apparelBluntMult = 40f;
        public float apparelTechMultAnimal = 0.25f;
        public float apparelTechMultNeolithic = 0.5f;
        public float apparelTechMultMedieval = 0.75f;
        public float apparelTechMultIndustrial = 1f;
        public float apparelTechMultSpacer = 2f;
        public float apparelTechMultUltratech = 3f;
        public float apparelTechMultArchotech = 4f; 
        
        public float advancedArmorCarryWeight = 80f;
        public float advancedArmorCarryBulk = 10f;
        public float advancedArmorShootingAccuracy = 0.2f;

        //weapon settings
        public float gunSharpPenMult = 10f;
        public float gunBluntPenMult = 40f;
        public float gunTechMultAnimal = 0.5f;
        public float gunTechMultNeolithic = 1f;
        public float gunTechMultMedieval = 2f;
        public float gunTechMultIndustrial = 4;
        public float gunTechMultSpacer = 5;
        public float gunTechMultUltratech = 6;
        public float gunTechMultArchotech = 8;

        public float weaponToolPowerMult = 1f;
        public float weaponToolSharpPenetration = 1f;
        public float weaponToolBluntPenetration = 4f;
        public float weaponToolTechMultAnimal = 1f;
        public float weaponToolTechMultNeolithic = 1f;
        public float weaponToolTechMultMedieval = 1f;
        public float weaponToolTechMultIndustrial = 2f;
        public float weaponToolTechMultSpacer = 3f;
        public float weaponToolTechMultUltratech = 4f;
        public float weaponToolTechMultArchotech = 6f;

        public float maximumWeaponMass = 20f;

        //pawn settings
        public float pawnArmorSharpMult = 10;
        public float pawnArmorBluntMult = 40;

        public float pawnToolPowerMult = 1f;
        public float pawnToolSharpPenetration = 10f;
        public float pawnToolBluntPenetration = 40f;

        public float pawnKindMinMags = 2f;
        public float pawnKindMaxMags = 5f;

        public bool patchBackpacks = true;

        public float geneArmorSharpMult = 10;
        public float geneArmorBluntMult = 10;

        //hediff settings
        public float hediffSharpMult = 10;
        public float hediffBluntMult = 40;

        //other
        public float vehicleSharpMult = 15;
        public float vehicleBluntMult = 15;
        public float vehicleHealthMult = 3;

        public ModDataHolder()
        {
            //for being constructed by SaveLoad
        }

        public ModDataHolder(ModContentPack mcp)
        {
            this.mod = mcp;
            this.packageId = mcp.PackageId;
            RegisterSelfInDict();
            SelectDefsToPatch();
        }

        public void GetModContentPack()
        {
            if (packageId == null)
            {
                Log.Error("ModDataHolder tried to get ModContentPack but PackageId was null");
            }
            mod = LoadedModManager.RunningMods.FirstOrDefault(m => m.PackageId == packageId);
            if (mod == null)
            {
                Log.Error($"ModDataHolder tried to get ModContentPack, but found none with PackageId {packageId}");
            }
        }

        public void Reset()
        {
            //TODO reset values to those of nuff.apcedefaults. Maybe just construct a new one?
        }

        public void SelectDefsToPatch()
        {
            foreach (Def def in mod.AllDefs)
            {
                APCEConstants.NeedsPatch need = APCEController.CheckIfDefNeedsPatched(def);
                if (need == APCEConstants.NeedsPatch.yes)
                {
                    defsToPatch.Add(def, APCEConstants.NeedsPatch.yes);
                }
                else
                {
                    defsToPatch.Add(def, APCEConstants.NeedsPatch.no);
                }
            }
        }

        public void GenerateDefDataHolders()
        {
            foreach (var entry in defsToPatch)
            {
                if (entry.value == APCEConstants.NeedsPatch.yes && !defDict.ContainsKey(entry.Key))
                {
                    APCEController.TryGenerateDataHolderForDef(entry.Key);
                }
            }
        }

        public void ReCalc()
        {
            foreach (var entry in defDict)
            {
                if (!entry.Value.isCustomized)
                {
                    entry.Value.AutoCalculate();
                }
            }
        }

        public void PrePatch()
        {
            foreach (var entry in defDict)
            {
                try
                {
                    entry.Value.PrePatch();
                }
                catch (Exception ex)
                {
                    Log.Warning($"Failed to prepatch def {entry.Value.defName} from mod {entry.Value.def.modContentPack.Name} due to exception: \n" + ex.ToString());
                }
            }
        }

        public void PostPatch()
        {
            foreach (var entry in defDict)
            {
                try
                {
                    entry.Value.PostPatch();
                }
                catch (Exception ex)
                {
                    Log.Warning($"Failed to postpatching def {entry.Value.defName} from mod {entry.Value.def.modContentPack.Name} due to exception: \n" + ex.ToString());
                }
            }
        }

        public void Patch()
        {
            foreach (var entry in defDict)
            {
                try
                {
                    entry.Value.Patch();
                }
                catch (Exception ex)
                {
                    Log.Warning($"Failed to patch def {entry.Value.defName} from mod {entry.Value.def.modContentPack.Name} due to exception: \n" + ex.ToString());
                }
            }
        }

        //returns true if new entry is added to dict. returns false if value is replaced.
        public bool RegisterSelfInDict()
        {
            if (!APCESettings.modDataDict.TryAdd(packageId, this))
            {
                APCESettings.modDataDict[packageId] = this;
                return false;
            }
            return true;
        }

        public void RebuildDefsToPatchDict(List<string> namesList, List<string> typesList)
        {
            if (namesList.NullOrEmpty() || typesList.NullOrEmpty())
            {
                return;
            }

            if (namesList.Count != typesList.Count)
            {
                Log.Warning("Error in loading list of defs to patch, names list and types list are not the same length");
                return;
            }

            for (int i = 0; i < namesList.Count; i++)
            {
                Type defType = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .FirstOrDefault(t => t.FullName == typesList[i]);
                if (defType == null || !typeof(Def).IsAssignableFrom(defType))
                {
                    Log.Warning($"Skipping invalid or missing Def with name {namesList[i]} and type {defType}");
                    continue;
                }

                var method = typeof(DefDatabase<>).MakeGenericType(defType).GetMethod("GetNamedSilentFail", new Type[] { typeof(string) });
                if (method != null)
                {
                    Def foundDef = method.Invoke(null, new object[] { namesList[i] }) as Def;
                    if (foundDef != null)
                    {
                        defsToPatch[foundDef] = APCEConstants.NeedsPatch.yes;
                    }
                    else
                    {
                        Log.Warning($"Def not found: {namesList[i]} (Type: {namesList[i]})");
                    }
                }
                else
                {
                    Log.Warning($"Could not find GetNamedSilentFail for type: {namesList[i]}");
                }
            }

            //populate the rest of the dictionary for defs NOT to patch
            foreach (Def def in mod.AllDefs)
            {
                APCEConstants.NeedsPatch need = APCEController.CheckIfDefNeedsPatched(def);
                if (need != APCEConstants.NeedsPatch.ignore && !defsToPatch.ContainsKey(def))
                {
                    defsToPatch.Add(def, APCEConstants.NeedsPatch.no);
                }
            }
        }

        public void ExposeData()
        {
            //only bother to save data if the user has changed values. If not, just recalculate during patching
            if (Scribe.mode == LoadSaveMode.LoadingVars
                || (Scribe.mode == LoadSaveMode.Saving && isCustomized == true))
            {
                //turn the defsToPatch dictionary into a list of strings
                if (Scribe.mode == LoadSaveMode.Saving)
                {
                    defsToPatchNames = new List<string>();
                    defsToPatchTypes = new List<string>();
                    foreach (var entry in defsToPatch)
                    {
                        if (entry.Value == APCEConstants.NeedsPatch.yes)
                        {
                            defsToPatchNames.Add(entry.key.defName);
                            defsToPatchTypes.Add(entry.key.GetType().ToString());
                        }
                    }
                }

                Scribe_Values.Look(ref packageId, "packageId");
                Scribe_Values.Look(ref isCustomized, "isCustomized");
                Scribe_Collections.Look(ref defsToPatchNames, "defsToPatchNames");
                Scribe_Collections.Look(ref defsToPatchTypes, "defsToPatchTypes");

                //toggles
                Scribe_Values.Look(ref patchCustomVerbs, "patchCustomVerbs", false);
                Scribe_Values.Look(ref limitWeaponMass, "limitWeaponMass", false);
                Scribe_Values.Look(ref patchHeadgearLayers, "patchHeadgearLayers", true);

                // Apparel values
                Scribe_Values.Look(ref apparelSharpMult, "apparelSharpMult", 10);
                Scribe_Values.Look(ref apparelBluntMult, "apparelBluntMult", 40);
                Scribe_Values.Look(ref apparelTechMultAnimal, "apparelTechMultAnimal", 0.25f);
                Scribe_Values.Look(ref apparelTechMultNeolithic, "apparelTechMultNeolithic", 0.5f);
                Scribe_Values.Look(ref apparelTechMultMedieval, "apparelTechMultMedieval", 0.75f);
                Scribe_Values.Look(ref apparelTechMultIndustrial, "apparelTechMultIndustrial", 1f);
                Scribe_Values.Look(ref apparelTechMultSpacer, "apparelTechMultSpacer", 2f);
                Scribe_Values.Look(ref apparelTechMultUltratech, "apparelTechMultUltratech", 3f);
                Scribe_Values.Look(ref apparelTechMultArchotech, "apparelTechMultArchotech", 4f);

                Scribe_Values.Look(ref advancedArmorCarryWeight, "advancedArmorCarryWeight", 80f);
                Scribe_Values.Look(ref advancedArmorCarryBulk, "advancedArmorCarryBulk", 10f);
                Scribe_Values.Look(ref advancedArmorShootingAccuracy, "advancedArmorShootingAccuracy", 0.2f);

                // Weapon settings
                Scribe_Values.Look(ref gunSharpPenMult, "gunSharpPenMult", 10f);
                Scribe_Values.Look(ref gunBluntPenMult, "gunBluntPenMult", 40f);
                Scribe_Values.Look(ref gunTechMultAnimal, "gunTechMultAnimal", 0.5f);
                Scribe_Values.Look(ref gunTechMultNeolithic, "gunTechMultNeolithic", 1f);
                Scribe_Values.Look(ref gunTechMultMedieval, "gunTechMultMedieval", 2f);
                Scribe_Values.Look(ref gunTechMultIndustrial, "gunTechMultIndustrial", 4f);
                Scribe_Values.Look(ref gunTechMultSpacer, "gunTechMultSpacer", 5f);
                Scribe_Values.Look(ref gunTechMultUltratech, "gunTechMultUltratech", 6f);
                Scribe_Values.Look(ref gunTechMultArchotech, "gunTechMultArchotech", 8f);

                Scribe_Values.Look(ref weaponToolPowerMult, "weaponToolPowerMult", 1f);
                Scribe_Values.Look(ref weaponToolSharpPenetration, "weaponToolSharpPenetration", 1f);
                Scribe_Values.Look(ref weaponToolBluntPenetration, "weaponToolBluntPenetration", 4f);
                Scribe_Values.Look(ref weaponToolTechMultAnimal, "weaponToolTechMultAnimal", 1f);
                Scribe_Values.Look(ref weaponToolTechMultNeolithic, "weaponToolTechMultNeolithic", 1f);
                Scribe_Values.Look(ref weaponToolTechMultMedieval, "weaponToolTechMultMedieval", 1f);
                Scribe_Values.Look(ref weaponToolTechMultIndustrial, "weaponToolTechMultIndustrial", 2f);
                Scribe_Values.Look(ref weaponToolTechMultSpacer, "weaponToolTechMultSpacer", 3f);
                Scribe_Values.Look(ref weaponToolTechMultUltratech, "weaponToolTechMultUltratech", 4f);
                Scribe_Values.Look(ref weaponToolTechMultArchotech, "weaponToolTechMultArchotech", 6f);

                Scribe_Values.Look(ref maximumWeaponMass, "maximumWeaponMass", 20f);

                // Pawn settings
                Scribe_Values.Look(ref pawnArmorSharpMult, "pawnArmorSharpMult", 10f);
                Scribe_Values.Look(ref pawnArmorBluntMult, "pawnArmorBluntMult", 40f);

                Scribe_Values.Look(ref pawnToolPowerMult, "pawnToolPowerMult", 1f);
                Scribe_Values.Look(ref pawnToolSharpPenetration, "pawnToolSharpPenetration", 10f);
                Scribe_Values.Look(ref pawnToolBluntPenetration, "pawnToolBluntPenetration", 40f);

                Scribe_Values.Look(ref pawnKindMinMags, "pawnKindMinMags", 2f);
                Scribe_Values.Look(ref pawnKindMaxMags, "pawnKindMaxMags", 5f);

                Scribe_Values.Look(ref patchBackpacks, "patchBackpacks", true);

                Scribe_Values.Look(ref geneArmorSharpMult, "geneArmorSharpMult", 10f);
                Scribe_Values.Look(ref geneArmorBluntMult, "geneArmorBluntMult", 10f);

                // Hediff settings
                Scribe_Values.Look(ref hediffSharpMult, "hediffSharpMult", 10f);
                Scribe_Values.Look(ref hediffBluntMult, "hediffBluntMult", 40f);

                // Other
                Scribe_Values.Look(ref vehicleSharpMult, "vehicleSharpMult", 15f);
                Scribe_Values.Look(ref vehicleBluntMult, "vehicleBluntMult", 15f);
                Scribe_Values.Look(ref vehicleHealthMult, "vehicleHealthMult", 3f);
            }
            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                APCESettings.modDataDict.Add(packageId, this);
                mod = LoadedModManager.RunningMods.First(m => m.PackageId == packageId);

                //turn the list of defName strings back into a dictionary
                if (defsToPatchNames == null)
                {
                    defsToPatchNames = new List<string>();
                }
                RebuildDefsToPatchDict(defsToPatchNames, defsToPatchTypes);
            }
        }

    }
}
