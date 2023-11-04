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

namespace nuff.AutoPatcherCombatExtended.VF
{
    [StaticConstructorOnStartup]
    public static class APCEPatchVehicle
    {
        static APCEPatchVehicle()
        {

        }

        public static void PatchVehicle(Def def, APCEPatchLogger log)
        {
            VehicleDef vehicle = def as VehicleDef;
            try
            {
                int sharpIndex = vehicle.statBases?.FindIndex(i => i.stat == StatDefOf.ArmorRating_Sharp) ?? -1;
                int bluntIndex = vehicle.statBases?.FindIndex(i => i.stat == StatDefOf.ArmorRating_Blunt) ?? -1;
                if (sharpIndex >= 0)
                {
                    vehicle.statBases[sharpIndex].value *= APCESettings.vehicleSharpMult;
                }
                if (bluntIndex >= 0)
                {
                    vehicle.statBases[bluntIndex].value *= APCESettings.vehicleBluntMult;
                }

                for (int i = 0; i < vehicle.components.Count; i++)
                {
                    if (!vehicle.components[i].armor.NullOrEmpty())
                    {
                        int iSharpIndex = vehicle.components[i].armor?.FindIndex(j => j.stat == StatDefOf.ArmorRating_Sharp) ?? -1;
                        int iBluntIndex = vehicle.components[i].armor?.FindIndex(j => j.stat == StatDefOf.ArmorRating_Blunt) ?? -1;
                        if (iSharpIndex >= 0)
                        {
                            vehicle.components[i].armor[iSharpIndex].value *= APCESettings.vehicleSharpMult;
                        }
                        if (iBluntIndex >= 0)
                        {
                            vehicle.components[i].armor[iBluntIndex].value *= APCESettings.vehicleBluntMult;
                        }
                        //TODO forget about patching only specific components, and just patch them all?
                        if (vehicle.components[i].key.ToLower().Contains("armor")
                            || vehicle.components[i].key.ToLower().Contains("panel")
                            || vehicle.components[i].key.ToLower().Contains("roof"))
                        {
                            float newHealth = vehicle.components[i].health * APCESettings.vehicleHealthMult;
                            vehicle.components[i].health = (int)newHealth;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.PatchFailed(vehicle.defName, ex);
            }
            log.PatchSucceeded();
        }

        public static void PatchVehicleTurret(Def def, APCEPatchLogger log)
        {
            VehicleTurretDef turret = def as VehicleTurretDef;
            try
            {
                //def.projectile.thingClass = typeof(BulletCE); // I think this is just the default projectile, and thus irrelevant
                //change reload timer?
                //change warmup timer?
                turret.chargePerAmmoCount = 1; // I think this is how much 'ammunition' each shot costs to reload. Since using real ammos, should only be 1
                turret.genericAmmo = false;
                turret.maxRange *= 2; // \_(ツ)_/¯


                //generate AmmoSet et al
                ThingDef pseudoWeapon = CreatePseudoWeapon(turret);
                APCEConstants.gunKinds gunKind;

                CETurretDataDefModExtension dme = new CETurretDataDefModExtension();
                dme.shotHeight = 2;
                dme.spread = 0.01f;
                dme.sway = 0.82f;

                //categorize the weapon. can't really re-use DetermineGunKind without a major rewrite.
                if (turret.ammunition.Allows(APCEDefOf.WoodLog))
                {
                    //pre-industrial stuff
                    gunKind = APCEConstants.gunKinds.Bow;
                }
                else if (turret.ammunition.Allows(APCEDefOf.Steel) && turret.projectile.thingClass == typeof(Projectile_Explosive))
                {
                    gunKind = APCEConstants.gunKinds.ExplosiveLauncher;
                    dme.speed = 120;
                }
                else if (turret.ammunition.Allows(APCEDefOf.Steel) && turret.projectile.thingClass == typeof(Bullet))
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
                fieldThingDefs.SetValue(turret.ammunition, ammos);

                HashSet<ThingDef> ammosHashSet = new HashSet<ThingDef>(ammos);
                FieldInfo fieldAllowedDefs = typeof(ThingFilter).GetField("allowedDefs", BindingFlags.NonPublic | BindingFlags.Instance);
                fieldAllowedDefs.SetValue(turret.ammunition, ammosHashSet);

                turret.ammunition.ResolveReferences();

                dme._ammoSet = ammoSet;
                if (turret.modExtensions == null)
                {
                    turret.modExtensions = new List<DefModExtension>();
                }
                turret.modExtensions.Add(dme);
                //TODO add or replace fire modes as relevant
            }
            catch (Exception ex)
            {
                log.PatchFailed(turret.defName, ex);
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