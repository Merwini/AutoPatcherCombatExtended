using CombatExtended;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace nuff.AutoPatcherCombatExtended;

public static class ModAndDefCheckUtils
{
    public static void FindModsNeedingPatched()
    {
        //make a list of all mods with defs detected as needing patching
        List<ModContentPack> modsNeedingPatched = new List<ModContentPack>();
        for (int i = 0; i < APCESettings.activeMods.Count; i++)
        {
            if (APCESettings.activeMods[i].Name != "Combat Extended" && APCESettings.activeMods[i].AllDefs.Count() != 0
                && CheckIfModNeedsPatched(APCESettings.activeMods[i]))
            {
                modsNeedingPatched.Add(APCESettings.activeMods[i]);
            }
        }

        //compare modsNeedingPatched list to mods currently selected to patch, add any missing to a list to recommend to the player
        APCESettings.modsToRecommendAdd = new List<ModContentPack>();
        foreach (ModContentPack mod in modsNeedingPatched)
        {
            if (!APCESettings.modsToPatch.Contains(mod))
            {
                APCESettings.modsToRecommendAdd.Add(mod);
            }
        }

        //compare modsToRecommendAdd to modsNeedingPatched, add any extras to a list to recommend removing, for cases where a patch comes out and the player still has the mod selected to patch
        APCESettings.modsToRecommendRemove = new List<ModContentPack>();
        foreach (ModContentPack mod in APCESettings.modsToPatch)
        {
            if (!modsNeedingPatched.Contains(mod))
            {
                APCESettings.modsToRecommendRemove.Add(mod);
            }
        }
    }

    public static bool CheckIfModNeedsPatched(ModContentPack mod)
    {
        //start as true because switching to false is more final
        bool needsPatched = true;
        List<Def> defsNeedingPatched = new List<Def>();
        List<Def> defsNotNeedingPatched = new List<Def>();

        if (mod.AllDefs == null || mod.AllDefs.Count() == 0)
            return false;

        //iteration does not break when a 'no' is found, so that it can still checks partially-patched mods
        //won't recommend them, but will be useful for finding patches that need updates
        foreach (Def def in mod.AllDefs)
        {
            APCEConstants.NeedsPatch defNeedsPatched = CheckIfDefNeedsPatched(def);
            if (defNeedsPatched == APCEConstants.NeedsPatch.yes)
            {
                defsNeedingPatched.Add(def);
            }
            else if (defNeedsPatched == APCEConstants.NeedsPatch.no)
            {
                defsNotNeedingPatched.Add(def);
                needsPatched = false;
            }
            //if unsure, added to either list
        }

        if (defsNeedingPatched.Count != 0)
        {
            APCELogUtility.LogDefsCause(defsNeedingPatched);
        }

        if (needsPatched && defsNeedingPatched.Count == 0)
        {
            needsPatched = false;
        }

        if (!needsPatched && defsNeedingPatched.Count != 0)
        {
            APCELogUtility.LogDefsCauseNotSuggested(defsNotNeedingPatched);
        }

        return needsPatched;
    }

    public static APCEConstants.NeedsPatch CheckIfDefNeedsPatched(Def def)
    {
        APCEConstants.NeedsPatch needsPatched = APCEConstants.NeedsPatch.ignore;
        try
        {
            if (DefIsDelegatedType(def))
            {
                needsPatched = HandleDelegatedDefTypesCheck(def);
            }
            else if (def is ThingDef thingDef)
            {
                if (thingDef.IsApparel)
                {
                    if (thingDef.statBases.Any(sm => sm.stat == CE_StatDefOf.Bulk)
                    || thingDef.statBases.Any(sm => sm.stat == CE_StatDefOf.WornBulk))
                    {
                        needsPatched = APCEConstants.NeedsPatch.no;
                    }
                    else
                    {
                        needsPatched = APCEConstants.NeedsPatch.yes;
                    }
                }
                else if (thingDef.IsWeapon)
                {
                    if (thingDef.IsRangedWeapon)
                    {
                        if (thingDef.Verbs[0].verbClass.ToString().Contains("CE")
                        || thingDef.statBases.Any(sm => sm.stat == CE_StatDefOf.Bulk)
                        || (!thingDef.comps.NullOrEmpty() && thingDef.comps.Any(comp => comp is CompProperties_AmmoUser)))
                        {
                            needsPatched = APCEConstants.NeedsPatch.no;
                        }
                        else
                        {
                            needsPatched = APCEConstants.NeedsPatch.yes;
                        }
                    }
                    else if (thingDef.IsMeleeWeapon)
                    {
                        if (thingDef.tools.NullOrEmpty() || thingDef.tools.Any(tool => tool is ToolCE))
                        {
                            needsPatched = APCEConstants.NeedsPatch.no;
                        }
                        else
                        {
                            needsPatched = APCEConstants.NeedsPatch.yes;
                        }
                    }
                    //else default/unsure
                }
                else if (typeof(Pawn).IsAssignableFrom(thingDef.thingClass))
                {
                    //can't be sure the Pawn is patched, could be inheriting tools from a patched base
                    if (!thingDef.tools.NullOrEmpty() && thingDef.tools.Any(tool => tool is ToolCE))
                    {
                        needsPatched = APCEConstants.NeedsPatch.unsure;
                    }
                    if (!thingDef.tools.NullOrEmpty() && !thingDef.tools.Any(tool => tool is ToolCE))
                    {
                        needsPatched = APCEConstants.NeedsPatch.yes;
                    }
                }
                else if (typeof(Building_TurretGun).IsAssignableFrom(thingDef.thingClass) && thingDef.fillPercent < 0.85f)
                {
                    needsPatched = APCEConstants.NeedsPatch.yes;
                }
                //is the AmmoDef check necessary?
                else if (!(thingDef is AmmoDef) && thingDef.projectileWhenLoaded != null)
                {
                    needsPatched = APCEConstants.NeedsPatch.yes;
                }
            }
            else if (def is HediffDef hd)
            {
                if (!hd.comps.NullOrEmpty())
                {
                    HediffCompProperties_VerbGiver hcp_vg = (HediffCompProperties_VerbGiver)hd.comps.FirstOrDefault(c => c is HediffCompProperties_VerbGiver);
                    if (hcp_vg != null && !hcp_vg.tools.NullOrEmpty() && hcp_vg.tools.Any(tool => tool is ToolCE))
                    {
                        needsPatched = APCEConstants.NeedsPatch.no;
                    }
                    else if (hcp_vg != null && !hcp_vg.tools.NullOrEmpty() && !hcp_vg.tools.Any(tool => tool is ToolCE))
                    {
                        needsPatched = APCEConstants.NeedsPatch.yes;
                    }
                }
                else if (hd.stages != null && hd.stages.Any(stage => stage.statOffsets != null && stage.statOffsets.Any(stat =>
                       stat.stat == StatDefOf.ArmorRating_Sharp ||
                       stat.stat == StatDefOf.ArmorRating_Blunt ||
                       stat.stat == StatDefOf.ArmorRating_Heat)))
                {
                    needsPatched = APCEConstants.NeedsPatch.unsure;
                }
                //implicit else ignore
            }
            else if (ModLister.BiotechInstalled && def is GeneDef gene)
            {
                if (gene.statOffsets != null && gene.statOffsets.Any(stat =>
                       stat.stat == StatDefOf.ArmorRating_Sharp ||
                       stat.stat == StatDefOf.ArmorRating_Blunt ||
                       stat.stat == StatDefOf.ArmorRating_Heat))
                {
                    needsPatched = APCEConstants.NeedsPatch.unsure;
                }
            }
            else if (def is PawnKindDef pkd)
            {
                if (pkd.race.race.intelligence != Intelligence.Animal)
                {
                    if (!pkd.modExtensions.NullOrEmpty() && pkd.modExtensions.Any(ext => ext is LoadoutPropertiesExtension))
                    {
                        needsPatched = APCEConstants.NeedsPatch.no;
                    }
                    else
                    {
                        needsPatched = APCEConstants.NeedsPatch.unsure;
                    }
                }
            }
            //else default/unsure
            else
            {
                needsPatched = HandleUnknownDefCheck(def);
            }
        }
        catch (Exception ex)
        {
            Log.Warning($"Exception when checking if def {def.defName} from mod {def.modContentPack.Name} needs patching. Exception is: \n" + ex.ToString());
        }

        if (needsPatched != APCEConstants.NeedsPatch.ignore)
        {
            APCESettings.patchableDefs[def] = needsPatched;
        }
        return needsPatched;
    }

    public static bool DefIsDelegatedType(Def def)
    {
        Type defType = def.GetType();
        if (APCESettings.typeHandlerDictionaryCheck.ContainsKey(defType))
        {
            return true;
        }
        return false;
    }

    public static APCEConstants.NeedsPatch HandleDelegatedDefTypesCheck(Def def)
    {
        Type defType = def.GetType();

        if (APCESettings.typeHandlerDictionaryCheck.TryGetValue(defType, out var handler))
        {
            return handler.Invoke(def);
        }
        else
        {
            return APCEConstants.NeedsPatch.ignore;
        }
    }

    public static APCEConstants.NeedsPatch HandleUnknownDefCheck(Def def)
    {
        //TODO remove?
        //Log.Warning($"Unable to check if def {def.defName} from mod {def.modContentPack.Name} needs patching. Type {def.GetType()} is unrecognized.");
        return APCEConstants.NeedsPatch.ignore;
    }

}
