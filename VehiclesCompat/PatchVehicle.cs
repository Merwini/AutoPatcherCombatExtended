using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vehicles;
using CombatExtended;
using RimWorld;
using Verse;
using System.Reflection;

namespace nuff.AutoPatcherCombatExtended
{
    static class APCEPatchVehicle
    {
        public static void PatchVehicle(VehicleDef def, APCEPatchLogger log)
        {
            try
            {
                int sharpIndex = def.statBases?.FindIndex(i => i.stat == StatDefOf.ArmorRating_Sharp) ?? -1;
                int bluntIndex = def.statBases?.FindIndex(i => i.stat == StatDefOf.ArmorRating_Blunt) ?? -1;
                if (sharpIndex >= 0)
                {
                    def.statBases[sharpIndex].value *= APCESettings.vehicleSharpMult;
                }
                if (bluntIndex >= 0)
                {
                    def.statBases[bluntIndex].value *= APCESettings.vehicleBluntMult;
                }

                for (int i = 0; i < def.components.Count; i++)
                {
                    if (!def.components[i].armor.NullOrEmpty())
                    {
                        int iSharpIndex = def.components[i].armor?.FindIndex(j => j.stat == StatDefOf.ArmorRating_Sharp) ?? -1;
                        int iBluntIndex = def.components[i].armor?.FindIndex(j => j.stat == StatDefOf.ArmorRating_Blunt) ?? -1;
                        if (iSharpIndex >= 0)
                        {
                            def.components[i].armor[iSharpIndex].value *= APCESettings.vehicleSharpMult;
                        }
                        if (iBluntIndex >= 0)
                        {
                            def.components[i].armor[iBluntIndex].value *= APCESettings.vehicleBluntMult;
                        }
                        //TODO forget about patching only specific components, and just patch them all?
                        if (def.components[i].key.ToLower().Contains("armor")
                            || def.components[i].key.ToLower().Contains("panel")
                            || def.components[i].key.ToLower().Contains("roof"))
                        {
                            float newHealth = def.components[i].health * APCESettings.vehicleHealthMult;
                            def.components[i].health = (int)newHealth;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.PatchFailed(def.defName, ex);
            }
            log.PatchSucceeded();
        }

        public static void PatchVehicleTurret(VehicleTurretDef def, APCEPatchLogger log)
        {
            try
            {
                //def.projectile.thingClass = typeof(BulletCE); // I think this is just the default projectile, and thus irrelevant
                //change reload timer?
                //change warmup timer?
                def.chargePerAmmoCount = 1; // I think this is how much 'ammunition' each shot costs to reload. Since using real ammos, should only be 1
                def.genericAmmo = false;
                def.maxRange *= 2; // \_(ツ)_/¯


                //generate AmmoSet et al
                ThingDef pseudoWeapon = CreatePseudoWeapon(def);
                APCEConstants.gunKinds gunKind;

                CETurretDataDefModExtension dme = new CETurretDataDefModExtension();
                dme.shotHeight = 2;
                dme.spread = 0.01f;
                dme.sway = 0.82f;

                //categorize the weapon. can't really re-use DetermineGunKind without a major rewrite.
                if (def.ammunition.Allows(APCEDefOf.WoodLog))
                {
                    //pre-industrial stuff
                    gunKind = APCEConstants.gunKinds.Bow;
                }
                else if (def.ammunition.Allows(APCEDefOf.Steel) && def.projectile.thingClass == typeof(Projectile_Explosive))
                {
                    gunKind = APCEConstants.gunKinds.ExplosiveLauncher;
                    dme.speed = 120;
                }
                else if (def.ammunition.Allows(APCEDefOf.Steel) && def.projectile.thingClass == typeof(Bullet))
                {
                    gunKind = APCEConstants.gunKinds.MachineGun;
                    dme.speed = 180;
                }
                else
                {
                    gunKind = APCEConstants.gunKinds.Other;
                }
                AmmoSetDef ammoSet = APCEController.GenerateAmmoSet(pseudoWeapon, gunKind);
                List<ThingDef> ammos = new List<ThingDef>();
                for (int i = 0; i < ammoSet.ammoTypes.Count; i++)
                {
                    ammos.Add(ammoSet.ammoTypes[i].ammo);
                }

                FieldInfo fieldThingDefs = typeof(ThingFilter).GetField("thingDefs", BindingFlags.NonPublic | BindingFlags.Instance);
                fieldThingDefs.SetValue(def.ammunition, ammos);

                HashSet<ThingDef> ammosHashSet = new HashSet<ThingDef>(ammos);
                FieldInfo fieldAllowedDefs = typeof(ThingFilter).GetField("allowedDefs", BindingFlags.NonPublic | BindingFlags.Instance);
                fieldAllowedDefs.SetValue(def.ammunition, ammosHashSet);

                def.ammunition.ResolveReferences();

                dme._ammoSet = ammoSet;
                if (def.modExtensions == null)
                {
                    def.modExtensions = new List<DefModExtension>();
                }
                def.modExtensions.Add(dme);
                //TODO add or replace fire modes as relevant
            }
            catch (Exception ex)
            {
                log.PatchFailed(def.defName, ex);
            }
            log.PatchSucceeded();
        }

        public static ThingDef CreatePseudoWeapon(VehicleTurretDef def)
        {
            ThingDef td = new ThingDef();
            td.defName = def.defName + "_w";
            td.label = def.label + "_w";
            List<VerbProperties> newVerbs = new List<VerbProperties>();
            VerbProperties newVerb = new VerbProperties();
            newVerb.defaultProjectile = def.projectile;
            newVerbs.Add(newVerb);

            Type tv = typeof(ThingDef);
            FieldInfo vs = tv.GetField("verbs", BindingFlags.NonPublic | BindingFlags.Instance);
            vs.SetValue(td, newVerbs);
            return td;
        }
    }
}