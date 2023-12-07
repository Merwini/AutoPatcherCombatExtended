using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using CombatExtended;
using System.Reflection;

namespace nuff.AutoPatcherCombatExtended
{
    public static class DataHolderUtils
    {
        //Made my own so I can use it to change equippedStatOffSets, too
        public static void AddOrChangeStat(List<StatModifier> list, StatDef stat, float value)
        {
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

        public static void AddOrReplaceExtension(ThingDef def, DefModExtension extension)
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
                modData = APCESettings.modDataDict.TryGetValue("nuff.ceautopatcher");
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
            //a turret is tagged as TurretGun, because it inherits that from BaseWeaponTurret
            if (thingDef.weaponTags.Any(str => str.IndexOf("Artillery", StringComparison.OrdinalIgnoreCase) >= 0))
                return APCEConstants.gunKinds.Mortar;
            else if (thingDef.Verbs[0].verbClass == typeof(Verb_ShootBeam))
                return APCEConstants.gunKinds.BeamGun;
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
            {
                return APCEConstants.gunKinds.ExplosiveLauncher;
            }

            //a shotgun is an industrial or higher weapon and has one of the following: shotgun in its defname, label, or description, OR shotgun or gauge in its projectile
            else if ((thingDef.defName.IndexOf("shotgun", 0, StringComparison.OrdinalIgnoreCase) != -1)
                        || (thingDef.label.IndexOf("shotgun", 0, StringComparison.OrdinalIgnoreCase) != -1)
                        || (thingDef.description.IndexOf("shotgun", 0, StringComparison.OrdinalIgnoreCase) != -1)
                        || (thingDef.Verbs[0].defaultProjectile.ToString().IndexOf("shotgun", 0, StringComparison.OrdinalIgnoreCase) != -1)
                        || (thingDef.Verbs[0].defaultProjectile.ToString().IndexOf("gauge", 0, StringComparison.OrdinalIgnoreCase) != -1))
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
                case APCEConstants.DefTypes.RangedWeapon:
                    ddh = new DefDataHolderRangedWeapon(def as ThingDef);
                    break;
                case APCEConstants.DefTypes.Stuff: //TODO Stuff is not a def, figure out a way to work with them
                    break;
                case APCEConstants.DefTypes.Vehicle: //TODO these will need to be prefixed in by the compat
                    break;
                case APCEConstants.DefTypes.VehicleTurret:
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
    }
}
