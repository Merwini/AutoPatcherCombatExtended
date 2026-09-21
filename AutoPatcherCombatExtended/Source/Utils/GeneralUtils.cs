using CombatExtended;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using UnityEngine;
using Verse;

namespace nuff.AutoPatcherCombatExtended;

public static class GeneralUtils
{

    public static void AddOrChangeStat(ref List<StatModifier> list, StatDef stat, float value, StringBuilder logText)
    {
        bool addedStat = false;

        if (list == null)
        {
            list = new List<StatModifier>();
            logText.AppendLine("AddOrChangeStat called with null List. Assigned empty List.");
        }

        if (stat == null)
        {
            logText.AppendLine("AddOrChangeStat called with null StatDef. Returning.");
            return;
        }

        int index = list.FindIndex(x => x.stat == stat);
        if (index != -1)
        {
            list[index].value = value;
        }

        //can't think of a use case where I would need to add a 0 value statmod, and adding this check will save vetting in the patch methods
        else if (value != 0)
        {
            addedStat = true;
            list.Add(new StatModifier() { stat = stat, value = value });
        }

        string action = addedStat ? "Added new stat" : "Updated existing stat";
        logText.AppendLine($"{action} {stat.defName} with value {value}");
    }

    public static void AddOrReplaceCompProps(ThingDef def, CompProperties comp, StringBuilder logText)
    {
        if (def == null)
        {
            logText.AppendLine("AddOrReplaceCompProps called with null def. Returning.");
            return;
        }

        if (comp == null)
        {
            logText.AppendLine("AddOrReplaceCompProps called with null CompProprties. Returning.");
            return;
        }

        if (def.comps == null)
        {
            def.comps = new List<CompProperties>();
            logText.AppendLine("Null comps on, assigned empty List.");
        }

        Type compType = comp.GetType();
        int index = def.comps.FindIndex(c => c.GetType() == compType);

        if (index != -1)
        {
            def.comps[index] = comp;
            logText.AppendLine("Found existing CompProperties and replaced it with the new one.");
        }
        else
        {
            def.comps.Add(comp);
            logText.AppendLine("Found no existing CompProperties, added the new one.");
        }
    }

    public static void AddOrReplaceExtension(Def def, DefModExtension newExtension, StringBuilder logText)
    {
        if (def == null)
        {
            logText.AppendLine("AddOrReplaceExtension called with null def. Returning.");
            return;
        }

        if (newExtension == null)
        {
            logText.AppendLine("AddOrReplaceExtension called with null DefModExtension. Returning.");
            return;
        }

        if (def.modExtensions == null)
        {
            def.modExtensions = new List<DefModExtension>();
            logText.AppendLine("Null modExtensions, assigned empty List.");
        }

        Type extensionType = newExtension.GetType();
        logText.AppendLine($"Checking List for defModExtension of Type {extensionType}");
        int index = def.modExtensions.FindIndex(ext => ext.GetType() == extensionType);

        if (index != -1)
        {
            def.modExtensions[index] = newExtension;
            logText.AppendLine("Found existing defModExtension and replaced it with the new one.");
        }
        else
        {
            def.modExtensions.Add(newExtension);
            logText.AppendLine("Found no existing defModExtension, added the new one.");
        }
    }

    public static bool AddCompReplaceMe(ThingDef oldThingDef, ThingDef newThingDef, StringBuilder logText)
    {
        CompProperties_ReplaceMe newComp_ReplaceMe = new CompProperties_ReplaceMe()
        {
            thingToSpawn = newThingDef
        };
        if (oldThingDef.comps == null)
        {
            oldThingDef.comps = new List<CompProperties>();
            logText.AppendLine($"Null comps on {oldThingDef.defName}, made empty List");
        }

        oldThingDef.comps.Add(newComp_ReplaceMe);
        logText.AppendLine($"Added CompProperties_ReplaceMe.");

        return true;
    }

    public static void CopyFields(object source, object destination, bool skipDefNameAndHash = false)
    {
        if (source == null || destination == null)
        {
            return;
        }
        Type sourceType = source.GetType();
        Type destType = destination.GetType();

        foreach (FieldInfo sourceField in sourceType.GetFields(BindingFlags.Public | BindingFlags.Instance))
        {
            if (skipDefNameAndHash && (sourceField.Name == "defName" || sourceField.Name == "shortHash"))
            {
                continue;
            }

            FieldInfo destField = destType.GetField(sourceField.Name, BindingFlags.Public | BindingFlags.Instance);
            if (destField != null && destField.FieldType == sourceField.FieldType)
            {
                object value = sourceField.GetValue(source);
                if (destField != null)
                {
                    destField.SetValue(destination, value);
                }
            }
        }
    }

    public static ToolCE MakeToolBase(Tool tool)
    {
        ToolCE newToolCE = new ToolCE();
        GeneralUtils.CopyFields(tool, newToolCE);
        newToolCE.id = "APCE_Tool_" + tool.id;

        //CE is far more punishing if you have no armor penetration than vanilla is, so it is essential to have some
        if (tool.armorPenetration <= 0)
        {
            newToolCE.armorPenetrationSharp = tool.power * 0.1f;
            newToolCE.armorPenetrationBlunt = tool.power * 0.1f;
        }
        else
        {
            newToolCE.armorPenetrationSharp = tool.armorPenetration;
            newToolCE.armorPenetrationBlunt = tool.armorPenetration;
        }

        return newToolCE;
    }

    public static void SetDamage(ProjectilePropertiesCE newPPCE, int damage)
    {
        Type tpp = typeof(ProjectileProperties);
        FieldInfo dab = tpp.GetField("damageAmountBase", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        dab.SetValue(newPPCE, (int)damage);
    }

    public static APCEConstants.gunKinds DetermineGunKind(ThingDef thingDef, APCEConstants.PatchStageLog log)
    {
        StringBuilder logText = log.Text;
        try
        {
            if (thingDef.weaponTags.Any(str => str.IndexOf("Artillery", StringComparison.OrdinalIgnoreCase) >= 0))
            {
                logText.AppendLine("Gun has weaponTag \"Artillery\", returning Mortar as gun kind.");
                return APCEConstants.gunKinds.Mortar;
            }
            else if (thingDef.Verbs[0].verbClass == typeof(Verb_ShootBeam))
            {
                logText.AppendLine("Gun has verbClass \"Verb_ShootBeam\", returning BeamGun as gun kind.");
                return APCEConstants.gunKinds.BeamGun;
            }
            else if (thingDef.Verbs[0].verbClass == typeof(Verb_SpewFire))
            {
                logText.AppendLine("Gun has verbClass \"Verb_SpewFire\", returning Flamethrower as gun kind.");
                return APCEConstants.gunKinds.Flamethrower;
            }
            
            //a turret is tagged as TurretGun, because it inherits that from BaseWeaponTurret
            else if (thingDef.weaponTags.Any(str => str.IndexOf("TurretGun", StringComparison.OrdinalIgnoreCase) >= 0))
            {
                logText.AppendLine("Gun has weaponTag \"TurretGun\", returning Turret as gun kind.");
                return APCEConstants.gunKinds.Turret;
            }

            //a bow is a pre-industrial ranged weapon with a burst count of 1. Can't find a good way to discern high-tech bows
            else if ((thingDef.techLevel.CompareTo(TechLevel.Medieval) <= 0) && (thingDef.Verbs[0].burstShotCount == 1))
            {
                logText.AppendLine("Gun is Medieval or lower tech level and fires a single projectile, returning Bow as gun kind.");
                return APCEConstants.gunKinds.Bow;
            }

            //a grenade uses a different verb from most weapons
            //TODO switch this back to grenade after I actually implement grenade-patching
            else if (thingDef.Verbs[0].verbClass == typeof(Verb_LaunchProjectile))
            {
                logText.AppendLine("Gun has verbClass \"Verb_LaunchProjectile\", is probably a grenade but grenade patching isn't impelemented so returning ExplosiveLauncher as gun kind instead.");
                //return APCEConstants.gunKinds.Grenade;
                return APCEConstants.gunKinds.ExplosiveLauncher;
            }
            //explosive launchers
            else if (thingDef.Verbs[0].CausesExplosion)
            {
                logText.AppendLine("Gun's primary Verb causes an explosion, returning ExplosiveLauncher as gun kind.");
                return APCEConstants.gunKinds.ExplosiveLauncher;
            }

            //a shotgun is an industrial or higher weapon and has one of the following: shotgun in its defname, label, or description, OR shotgun or gauge in its projectile
            else if ((thingDef.defName.IndexOf("shotgun", 0, StringComparison.OrdinalIgnoreCase) != -1)
                        || (thingDef.label.IndexOf("shotgun", 0, StringComparison.OrdinalIgnoreCase) != -1)
                        || (thingDef.description.IndexOf("shotgun", 0, StringComparison.OrdinalIgnoreCase) != -1)
                        || (thingDef.Verbs[0]?.defaultProjectile != null && ((thingDef.Verbs[0].defaultProjectile.ToString().IndexOf("shotgun", 0, StringComparison.OrdinalIgnoreCase) != -1)
                                                                         || (thingDef.Verbs[0].defaultProjectile.ToString().IndexOf("gauge", 0, StringComparison.OrdinalIgnoreCase) != -1))))
            {
                logText.AppendLine("Gun has \"shotgun\" in its name or description, or the name or description of its projectile. Returning Shotgun as gun kind.");
                return APCEConstants.gunKinds.Shotgun;
            }

            //a handgun is an industrial or higher weapon with burst count 1 and a range < 13
            else if ((thingDef.techLevel.CompareTo(TechLevel.Industrial) >= 0) && (thingDef.Verbs[0].burstShotCount == 1) && (thingDef.Verbs[0].range < 13))
            {
                logText.AppendLine("Gun is Industrial tech level or higher, fires a single shot, and has range less than 13 cells. Returning Handgun as gun kind.");
                return APCEConstants.gunKinds.Handgun;
            }

            // a precision rifle is an industrial or higher weapon with burst count 1 and a range >= 13
            else if ((thingDef.techLevel.CompareTo(TechLevel.Industrial) >= 0) && (thingDef.Verbs[0].burstShotCount == 1) && (thingDef.Verbs[0].range >= 13))
            {
                logText.AppendLine("Gun is Industrial tech level or higher, fires a single shot, and has range greater than or equal to 13 cells. Returning PrecisionRifle as gun kind.");
                return APCEConstants.gunKinds.PrecisionRifle;
            }

            //an SMG is an industrial or higher weapon with burst count > 1 and a range < 25.9
            else if ((thingDef.techLevel.CompareTo(TechLevel.Industrial) >= 0) && (thingDef.Verbs[0].burstShotCount > 1) && (thingDef.Verbs[0].range < 25.9))
            {
                logText.AppendLine("Gun is Industrial tech level or higher, fires a multi-shot burst, and has range less than 26 cells. Returning SMG as gun kind.");
                return APCEConstants.gunKinds.SMG;
            }

            //an assault rifle is an industrial or higher weapon with burst count > 1 but <= 3 and a range >= 25.9
            else if ((thingDef.techLevel.CompareTo(TechLevel.Industrial) >= 0) && (thingDef.Verbs[0].burstShotCount > 1) && (thingDef.Verbs[0].burstShotCount <= 3) && (thingDef.Verbs[0].range >= 25.9))
            {
                logText.AppendLine("Gun is Industrial tech level or higher, fires a multi-shot burst between 2 and 3 projectiles, and has range greater than or equal to 26 cells. Returning AssaultRifle as gun kind.");
                return APCEConstants.gunKinds.AssaultRifle;
            }

            //a machine gun is an industrial or higher weapon with range >= 26 and burst count > 3
            else if ((thingDef.techLevel.CompareTo(TechLevel.Industrial) >= 0) && (thingDef.Verbs[0].range >= 25.9) && (thingDef.Verbs[0].burstShotCount > 3))
            {
                logText.AppendLine("Gun is Industrial tech level or higher, fires a multi-shot burst greater than 3 projectiles, and has range greater than or equal to 26 cells. Returning MachineGun as gun kind.");
                return APCEConstants.gunKinds.MachineGun;
            }

            else
            {
                logText.AppendLine("Failed to match any gun kind rules, returning Other as gun kind");
                return APCEConstants.gunKinds.Other;
            }
        }
        catch (Exception ex)
        {
            log.ThrewError = true;
            logText.AppendLine($"Exception when trying to determine a gun kind for def {thingDef?.defName} from mod {thingDef.modContentPack?.Name}. Returning Other as gun kind. Exception is: \n" + ex.ToString());
            return APCEConstants.gunKinds.Other;
        }
    }

    //modified from CE's built-in so that it can rerun on the same def, if that def's Bulk is modified
    public static float WeaponToughnessAutocalc(ThingDef def, float bulk = 1)
    {

        StatDef SHARP_ARMOR_STUFF_POWER = StatDefOf.ArmorRating_Sharp.GetStatPart<StatPart_Stuff>().stuffPowerStat;

        // Approximate weapon thickness with the bulk of the weapon.
        // Longswords get about 2.83mm, knives get 1mm, spears get about 3.162mm
        float weaponThickness = bulk;

        weaponThickness = Mathf.Sqrt(weaponThickness);

        // Tech level improves toughness
        switch (def.techLevel)
        {
            //Plasteel
            case (TechLevel.Spacer):
                weaponThickness *= 2f;
                break;
            case (TechLevel.Ultra):
                weaponThickness *= 4f;
                break;
            case (TechLevel.Archotech):
                weaponThickness *= 8f;
                break;
        }

        // Blunt-only weapons get additional weapon thickness. Ranged weapons excluded
        if (!def.IsRangedWeapon
                && (!def.tools?
                    .Any(tool => tool.VerbsProperties
                        .Any(property => property.meleeDamageDef
                            .armorCategory == DamageArmorCategoryDefOf.Sharp))
                    ?? false))
        {
            weaponThickness *= 2f;
        }

        // Stuffable weapons receive the multiplier stat, to be applied in the DefDataHolder
        if (def.MadeFromStuff)
        {
            return weaponThickness;
        }

        // Non-stuffable weapons get the rating value, to be applied in the DefDataHolder
        // Search for a fitting recipe
        RecipeDef firstRecipeDef = DefDatabase<RecipeDef>.AllDefs
            .FirstOrDefault(recipeDef => recipeDef.products?
                    .Any(productDef => productDef.thingDef == def) ?? false);

        IngredientCount biggestIngredientCount = null;
        if (!firstRecipeDef?.ingredients?.Empty() ?? false)
        {
            biggestIngredientCount = firstRecipeDef.ingredients
                .MaxBy(ingredientCount => ingredientCount.count);
        }

        float strongestIngredientSharpArmor = 1f;

        // Recipe does exist and has a fixed ingredient
        if (biggestIngredientCount?.IsFixedIngredient ?? false)
        {
            strongestIngredientSharpArmor = biggestIngredientCount.FixedIngredient.statBases
                .Find(statMod => statMod.stat == SHARP_ARMOR_STUFF_POWER)?.value
                ?? 0f;
            strongestIngredientSharpArmor *= biggestIngredientCount.FixedIngredient
                .GetModExtension<StuffToughnessMultiplierExtensionCE>()?.toughnessMultiplier
                ?? 1f;
        }
        // Recipe may or may not exist
        else
        {
            strongestIngredientSharpArmor = biggestIngredientCount?.filter?.thingDefs?
                .Max(thingDef => (thingDef.statBases?.Find(statMod => statMod.stat == SHARP_ARMOR_STUFF_POWER)?.value ?? 0f)
                        * (thingDef.GetModExtension<StuffToughnessMultiplierExtensionCE>()?.toughnessMultiplier ?? 1f))
                ?? 1f;
        }

        return weaponThickness * strongestIngredientSharpArmor;
    }

    public static XmlNode GetXmlForDef(Def def)
    {

        if (def?.modContentPack == null || string.IsNullOrEmpty(def.fileName))
        {
            Log.Warning($"Could not find fileName for Def: {def?.defName} while trying to get original XML");
            return null;
        }

        LoadableXmlAsset asset = def.modContentPack
            .LoadDefs(true)
            .FirstOrDefault(x => x.name == def.fileName);

        if (asset?.xmlDoc == null)
        {
            Log.Warning($"Could not load XML document for file: {def.fileName}");
            return null;
        }

        Type defType = def.GetType();
        XmlDocument xmlDoc = asset.xmlDoc;

        if (xmlDoc != null)
        {
            XmlNodeList allDefs = xmlDoc.DocumentElement?.ChildNodes;
            if (allDefs != null)
            {
                foreach (XmlNode node in allDefs)
                {
                    // Modded GetType().Name works for vanilla types, but modded types need FullName in order to match
                    if (node.Name != defType.Name && node.Name != defType.FullName)
                        continue;

                    XmlNode defNameNode = node.SelectSingleNode("defName");
                    if (defNameNode != null && defNameNode.InnerText == def.defName)
                    {
                        return node;
                    }
                }
            }
        }

        Log.Warning($"Could not find Def with defName '{def.defName}' in file: {def.fileName}");
        return null;
    }

}
