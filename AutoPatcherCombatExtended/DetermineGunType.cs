using CombatExtended;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using Verse;

namespace nuff.AutoPatcherCombatExtended
{
    partial class APCEController
    {
        //TODO
        internal static APCESettings.gunKinds DetermineGunKind(ThingDef weapon)
        {
            float gunMass = weapon.statBases.GetStatFactorFromList(StatDefOf.Mass);
            //TODO new logic. This is from the old version of the mod

            //a turret is tagged as TurretGun, because it inherits that from BaseWeaponTurret
            if (weapon.weaponTags.Contains("TurretGun"))
                return APCESettings.gunKinds.Turret;
            //a bow is a pre-industrial ranged weapon with a burst count of 1. Can't find a good way to discern high-tech bows
            else if ((weapon.techLevel.CompareTo(TechLevel.Medieval) <= 0) && (weapon.Verbs[0].burstShotCount == 1))
                return APCESettings.gunKinds.Bow;
            //a grenade uses a different verb from most weapons
            else if (weapon.Verbs[0].verbClass.ToString().EqualsIgnoreCase("Verse.Verb_ShootLaunchProjectile"))
                return APCESettings.gunKinds.Grenade;
            //explosive launchers
            else if ((weapon.Verbs[0].CausesExplosion))
            {
                return APCESettings.gunKinds.ExplosiveLauncher;
            }

            //a shotgun is an industrial or higher weapon and has one of the following: shotgun in its defname, label, or description, OR shotgun or gauge in its projectile
            else if ((weapon.techLevel.CompareTo(TechLevel.Industrial) >= 0) && ((weapon.defName.IndexOf("shotgun", 0, StringComparison.OrdinalIgnoreCase) != -1)
                                                                                || (weapon.label.IndexOf("shotgun", 0, StringComparison.OrdinalIgnoreCase) != -1)
                                                                                || (weapon.description.IndexOf("shotgun", 0, StringComparison.OrdinalIgnoreCase) != -1)
                                                                                || (weapon.Verbs[0].defaultProjectile.ToString().IndexOf("shotgun", 0, StringComparison.OrdinalIgnoreCase) != -1)
                                                                                || (weapon.Verbs[0].defaultProjectile.ToString().IndexOf("gauge", 0, StringComparison.OrdinalIgnoreCase) != -1)))
                return APCESettings.gunKinds.Shotgun;
            //a handgun is an industrial or higher weapon with burst count 1 and a range < 13
            else if ((weapon.techLevel.CompareTo(TechLevel.Industrial) >= 0) && (weapon.Verbs[0].burstShotCount == 1) && (weapon.Verbs[0].range < 13))
                return APCESettings.gunKinds.Handgun;
            // a precision rifle is an industrial or higher weapon with burst count 1 and a range >= 13
            else if ((weapon.techLevel.CompareTo(TechLevel.Industrial) >= 0) && (weapon.Verbs[0].burstShotCount == 1) && (weapon.Verbs[0].range >= 13))
                return APCESettings.gunKinds.precisionRifle;
            //an SMG is an industrial or higher weapon with burst count > 1 but < 6 and a range < 26
            else if ((weapon.techLevel.CompareTo(TechLevel.Industrial) >= 0) && (weapon.Verbs[0].burstShotCount > 1) && (weapon.Verbs[0].burstShotCount < 6) && (weapon.Verbs[0].range < 26))
                return APCESettings.gunKinds.SMG;
            //an assault rifle is an industrial or higher weapon with burst count > 1 but <= 6 and a range >= 26
            else if ((weapon.techLevel.CompareTo(TechLevel.Industrial) >= 0) && (weapon.Verbs[0].burstShotCount > 1) && (weapon.Verbs[0].burstShotCount <= 3) && (weapon.Verbs[0].range >= 26))
                return APCESettings.gunKinds.assaultRifle;
            //a machine gun is an industrial or higher weapon with burst count > 1 and  and range >= 26 or burst count >= 6
            else if ((weapon.techLevel.CompareTo(TechLevel.Industrial) >= 0) && (weapon.Verbs[0].range >= 26) && (weapon.Verbs[0].burstShotCount > 3))
                return APCESettings.gunKinds.MachineGun;
            else
                return APCESettings.gunKinds.Other;
        }
    }
}