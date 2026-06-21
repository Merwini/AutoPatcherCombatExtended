using CombatExtended;
using RimWorld;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using UnityEngine;
using Verse;
using Verse.AI;

namespace nuff.AutoPatcherCombatExtended
{
    public static class DataHolderUtils
    {

        public static ModDataHolder ReturnModDataWithFallbacks(Def def)
        {
            ModDataHolder modData = APCESettings.modDataDict.TryGetValue(def.modContentPack.PackageId);
            if (modData == null)
            {
                modData = APCESettings.modDataDict.TryGetValue(def.modContentPack.PackageId + "_steam");
            }
            if (modData == null)
            {
                modData = APCESettings.modDataDict.TryGetValue("nuff.ceautopatcher");
            }
            if (modData == null)
            {
                modData = APCESettings.modDataDict.TryGetValue("nuff.ceautopatcher_steam");
            }
            return modData;
        }

        public static void PrepareModDataHolder(ModContentPack mod)
        {
            //Don't generate a ModDataHolder if it already generated during SaveLoad
            if (!APCESettings.modDataDict.ContainsKey(mod.PackageId))
            {
                ModDataHolder mdh = new ModDataHolder(mod);
            }
        }

        public static bool TryGenerateDataHolderForDef(Def def)
        {
            try
            {
                if (HandleDelegatedDefTypesGenerate(def))
                {
                    return true;
                }
                else if (def is ThingDef td)
                {
                    if (td.IsApparel)
                    {
                        GenerateDefDataHolder(def, APCEConstants.DefTypes.Apparel);
                        return true;
                    }
                    else if (td.IsWeapon)
                    {
                        if (td.IsRangedWeapon
                            && (!typeof(Verb_CastAbility).IsAssignableFrom(td.Verbs[0].verbClass))
                            && (!typeof(Verb_CastBase).IsAssignableFrom(td.Verbs[0].verbClass)))
                        {
                            GenerateDefDataHolder(def, APCEConstants.DefTypes.RangedWeapon);
                            return true;
                        }
                        else //if (td.IsMeleeWeapon)
                        {
                            GenerateDefDataHolder(def, APCEConstants.DefTypes.MeleeWeapon);
                            return true;
                        }
                    }
                    else if (typeof(Pawn).IsAssignableFrom(td.thingClass))
                    {
                        GenerateDefDataHolder(def, APCEConstants.DefTypes.Pawn);
                        return true;
                    }
                    else if (typeof(Building_TurretGun).IsAssignableFrom(td.thingClass))
                    {
                        GenerateDefDataHolder(def, APCEConstants.DefTypes.Building_TurretGun);
                        return true;
                    }
                    else if ((td.thingCategories != null) && td.thingCategories.Contains(APCEDefOf.MortarShells))
                    {
                        GenerateDefDataHolder(def, APCEConstants.DefTypes.MortarShell);
                        return true;
                    }
                }
                else if (def is HediffDef hd)
                {
                    GenerateDefDataHolder(def, APCEConstants.DefTypes.Hediff);
                    return true;
                }
                else if (def is PawnKindDef pkd)
                {
                    GenerateDefDataHolder(def, APCEConstants.DefTypes.PawnKind);
                    return true;
                }
                else if (ModLister.BiotechInstalled
                    && def is GeneDef gene)
                {
                    GenerateDefDataHolder(def, APCEConstants.DefTypes.Gene);
                    return true;
                }
                else
                {
                    HandleUnknownDefGenerate(def);
                }
                return false;
            }
            catch (Exception ex)
            {
                Log.Warning($"Exception while trying to generate DefDataHolder for def {def.defName} from mod {def.modContentPack.Name}. Exception: \n" + ex.ToString());
                return false;
            }
        }

        public static bool HandleDelegatedDefTypesGenerate(Def def)
        {
            Type defType = def.GetType();
            if (APCESettings.typeHandlerDictionaryGenerate.TryGetValue(defType, out var handler))
            {
                handler.DynamicInvoke(def);
                return true;
            }
            return false;
        }

        public static void GenerateDefDataHolder(Def def, APCEConstants.DefTypes defType)
        {
            DefDataHolder ddh;
            switch (defType)
            {
                case APCEConstants.DefTypes.Apparel:
                    ddh = new DefDataHolderApparel(def as ThingDef);
                    break;
                case APCEConstants.DefTypes.Building_TurretGun:
                    ddh = new DefDataHolderBuilding_TurretGun(def as ThingDef);
                    break;
                case APCEConstants.DefTypes.Gene:
                    ddh = new DefDataHolderGene(def as GeneDef);
                    break;
                case APCEConstants.DefTypes.Hediff:
                    ddh = new DefDataHolderHediff(def as HediffDef);
                    break;
                case APCEConstants.DefTypes.MeleeWeapon:
                    ddh = new DefDataHolderMeleeWeapon(def as ThingDef);
                    break;
                case APCEConstants.DefTypes.Pawn:
                    ddh = new DefDataHolderPawn(def as ThingDef);
                    break;
                case APCEConstants.DefTypes.PawnKind:
                    ddh = new DefDataHolderPawnKind(def as PawnKindDef);
                    break;
                case APCEConstants.DefTypes.RangedWeapon:
                    ddh = new DefDataHolderRangedWeapon(def as ThingDef);
                    break;
                case APCEConstants.DefTypes.MortarShell:
                    ddh = new DefDataHolderMortarShell(def as ThingDef);
                    break;
                case APCEConstants.DefTypes.Stuff: //TODO Stuff is not a def, figure out a way to work with them
                    break;
            }
        }

        public static void HandleUnknownDefGenerate(Def def)
        {
            //TODO remove? most unhandled def types are not in need of patching
            //Log.Warning($"Unable to generate DefDataHolder for {def.defName} from mod {def.modContentPack.Name}. Type {def.GetType()} is unrecognized.");
            return;
        }

        public static void SetDamage(ProjectilePropertiesCE newPPCE, int damage)
        {
            //experimental reflection attempt
            Type tpp = typeof(ProjectileProperties);
            FieldInfo dab = tpp.GetField("damageAmountBase", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            dab.SetValue(newPPCE, (int)damage);
        }

        public static bool ReplaceRecipes(ThingDef oldThingDef, ThingDef newThingDef, int newRecipeCount)
        {
            bool foundRecipe = false;
            foreach (RecipeDef recipe in DefDatabase<RecipeDef>.AllDefs)
            {
                foreach (ThingDefCountClass products in recipe.products)
                {
                    if (products.thingDef == oldThingDef)
                    {
                        foundRecipe = true;
                        products.thingDef = newThingDef;
                        products.count = newRecipeCount;
                    }
                }
            }
            return foundRecipe;
        }

        public static bool AddCompReplaceMe(ThingDef oldThingDef, ThingDef newThingDef)
        {
            CompProperties_ReplaceMe newComp_ReplaceMe = new CompProperties_ReplaceMe()
            {
                thingToSpawn = newThingDef
            };
            if (oldThingDef.comps == null)
                oldThingDef.comps = new List<CompProperties>();
            oldThingDef.comps.Add(newComp_ReplaceMe);
            return true;
        }

        /*
        public static string ReturnModLabelNoSpaces(string packageID)
        {
            ModContentPack mod = LoadedModManager.RunningModsListForReading.First(mcp => mcp.PackageId == packageID);
            string labelNoSpaces = mod.Name.Replace(" ", "");
            return labelNoSpaces;
        }
        */

    }
}
