using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using CombatExtended;
using System.Reflection;
using UnityEngine;

namespace nuff.AutoPatcherCombatExtended
{
    public static class DataHolderUtils
    {
        //Made my own so I can use it to change equippedStatOffSets, too
        public static void AddOrChangeStat(List<StatModifier> list, StatDef stat, float value)
        {
            if (list == null)
            {
                list = new List<StatModifier>();
            }

            int index = list.FindIndex(x => x.stat == stat);
            if (index != -1)
            {
                list[index].value = value;
            }
            //can't think of a use case where I would need to add a 0 value statmod, and adding this check will save vetting in the patch methods
            else if (value != 0)
            {
                list.Add(new StatModifier() { stat = stat, value = value });
            }
        }

        public static void AddOrReplaceCompProps(ThingDef def, CompProperties comp)
        {
            if (def.comps == null)
            {
                def.comps = new List<CompProperties>();
            }

            int index = def.comps.FindIndex(c => c.GetType() == comp.GetType());

            if (index != -1)
            {
                def.comps[index] = comp;
            }
            else
            {
                def.comps.Add(comp);
            }
        }

        public static void AddOrReplaceExtension(Def def, DefModExtension extension)
        {
            if (def.modExtensions == null)
            {
                def.modExtensions = new List<DefModExtension>();
            }

            int index = def.modExtensions.FindIndex(ext => ext.GetType() == extension.GetType());

            if (index != -1)
            {
                def.modExtensions[index] = extension;
            }
            else
            {
                def.modExtensions.Add(extension);
            }
        }

        public static ModDataHolder ReturnModDataOrDefault(Def def)
        {
            ModDataHolder modData = APCESettings.modDataDict.TryGetValue(def.modContentPack.PackageId);
            if (modData == null)
            {
                modData = APCESettings.modDataDict.TryGetValue("nuff.apcedefaults");
            }
            return modData;
        }

        public static void CopyFields(object source, object destination)
        {
            if (source == null || destination == null)
            {
                return;
            }
            Type sourceType = source.GetType();
            Type destType = destination.GetType();

            foreach (FieldInfo sourceField in sourceType.GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
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
            CopyFields(tool, newToolCE);
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

        public static APCEConstants.gunKinds DetermineGunKind(ThingDef thingDef)
        {
            try
            {
                //a turret is tagged as TurretGun, because it inherits that from BaseWeaponTurret
                if (thingDef.weaponTags.Any(str => str.IndexOf("Artillery", StringComparison.OrdinalIgnoreCase) >= 0))
                    return APCEConstants.gunKinds.Mortar;
                else if (thingDef.Verbs[0].verbClass == typeof(Verb_ShootBeam))
                    return APCEConstants.gunKinds.BeamGun;
                else if (thingDef.Verbs[0].verbClass == typeof(Verb_SpewFire))
                    return APCEConstants.gunKinds.Flamethrower;
                else if (thingDef.weaponTags.Any(str => str.IndexOf("TurretGun", StringComparison.OrdinalIgnoreCase) >= 0))
                    return APCEConstants.gunKinds.Turret;
                //a bow is a pre-industrial ranged weapon with a burst count of 1. Can't find a good way to discern high-tech bows
                else if ((thingDef.techLevel.CompareTo(TechLevel.Medieval) <= 0) && (thingDef.Verbs[0].burstShotCount == 1))
                    return APCEConstants.gunKinds.Bow;
                //a grenade uses a different verb from most weapons
                else if (thingDef.Verbs[0].verbClass == typeof(Verb_LaunchProjectile))
                    return APCEConstants.gunKinds.Grenade;
                //explosive launchers
                else if ((thingDef.Verbs[0].CausesExplosion))
                    return APCEConstants.gunKinds.ExplosiveLauncher;

                //a shotgun is an industrial or higher weapon and has one of the following: shotgun in its defname, label, or description, OR shotgun or gauge in its projectile
                else if ((thingDef.defName.IndexOf("shotgun", 0, StringComparison.OrdinalIgnoreCase) != -1)
                            || (thingDef.label.IndexOf("shotgun", 0, StringComparison.OrdinalIgnoreCase) != -1)
                            || (thingDef.description.IndexOf("shotgun", 0, StringComparison.OrdinalIgnoreCase) != -1)
                            || (thingDef.Verbs[0].defaultProjectile != null && ((thingDef.Verbs[0].defaultProjectile.ToString().IndexOf("shotgun", 0, StringComparison.OrdinalIgnoreCase) != -1)
                                                                             || (thingDef.Verbs[0].defaultProjectile.ToString().IndexOf("gauge", 0, StringComparison.OrdinalIgnoreCase) != -1))))
                    return APCEConstants.gunKinds.Shotgun;
                //a handgun is an industrial or higher weapon with burst count 1 and a range < 13
                else if ((thingDef.techLevel.CompareTo(TechLevel.Industrial) >= 0) && (thingDef.Verbs[0].burstShotCount == 1) && (thingDef.Verbs[0].range < 13))
                    return APCEConstants.gunKinds.Handgun;
                // a precision rifle is an industrial or higher weapon with burst count 1 and a range >= 13
                else if ((thingDef.techLevel.CompareTo(TechLevel.Industrial) >= 0) && (thingDef.Verbs[0].burstShotCount == 1) && (thingDef.Verbs[0].range >= 13))
                    return APCEConstants.gunKinds.precisionRifle;
                //an SMG is an industrial or higher weapon with burst count > 1 but < 6 and a range < 26
                else if ((thingDef.techLevel.CompareTo(TechLevel.Industrial) >= 0) && (thingDef.Verbs[0].burstShotCount > 1) && (thingDef.Verbs[0].burstShotCount < 6) && (thingDef.Verbs[0].range < 25.9))
                    return APCEConstants.gunKinds.SMG;
                //an assault rifle is an industrial or higher weapon with burst count > 1 but <= 6 and a range >= 26
                else if ((thingDef.techLevel.CompareTo(TechLevel.Industrial) >= 0) && (thingDef.Verbs[0].burstShotCount > 1) && (thingDef.Verbs[0].burstShotCount <= 3) && (thingDef.Verbs[0].range >= 25.9))
                    return APCEConstants.gunKinds.assaultRifle;
                //a machine gun is an industrial or higher weapon with range >= 26 and burst count >= 3
                else if ((thingDef.techLevel.CompareTo(TechLevel.Industrial) >= 0) && (thingDef.Verbs[0].range >= 25.8) && (thingDef.Verbs[0].burstShotCount > 3))
                    return APCEConstants.gunKinds.MachineGun;
                else
                    return APCEConstants.gunKinds.Other;
            }
            catch (Exception ex)
            {
                Log.Warning($"Exception when trying to determine a gun kind for def {thingDef.defName} from mod {thingDef.modContentPack.Name}. Returning gunKinds.Other. Exception is: \n" + ex.ToString());
                return APCEConstants.gunKinds.Other;
            }
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
                case APCEConstants.DefTypes.Stuff: //TODO Stuff is not a def, figure out a way to work with them
                    break;
            }
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

        //modified from CE's built-in so that it can rerun on the same def, if that def's Bulk is modified
        public static float WeaponToughnessAutocalc(ThingDef def, float bulk = 1)
        {

            StatDef SHARP_ARMOR_STUFF_POWER = StatDefOf.ArmorRating_Sharp.GetStatPart<StatPart_Stuff>().stuffPowerStat;

            // Approximate weapon thickness with the bulk of the weapon.
            // Longswords get about 2.83mm, knives get 1mm, spears get about 3.162mm
            float weaponThickness = def.statBases
                .Find(statMod => statMod.stat == CE_StatDefOf.Bulk)?.value
                ?? 0f;
                
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
            if (!firstRecipeDef?.ingredients?.NullOrEmpty() ?? false)
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
    }
}
