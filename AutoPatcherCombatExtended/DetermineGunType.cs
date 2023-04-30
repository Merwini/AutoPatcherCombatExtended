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
        internal static APCEConstants.gunKinds DetermineGunKind(ThingDef weapon)
        {
            float gunMass = weapon.statBases.GetStatFactorFromList(StatDefOf.Mass);
            
            //a turret is tagged as TurretGun, because it inherits that from BaseWeaponTurret
            if (weapon.weaponTags.Any(str => str.IndexOf("Artillery", StringComparison.OrdinalIgnoreCase) >= 0))
                return APCEConstants.gunKinds.Mortar;
            else if (weapon.weaponTags.Any(str => str.IndexOf("TurretGun", StringComparison.OrdinalIgnoreCase) >= 0))
                return APCEConstants.gunKinds.Turret;
            //a bow is a pre-industrial ranged weapon with a burst count of 1. Can't find a good way to discern high-tech bows
            else if ((weapon.techLevel.CompareTo(TechLevel.Medieval) <= 0) && (weapon.Verbs[0].burstShotCount == 1))
                return APCEConstants.gunKinds.Bow;
            //a grenade uses a different verb from most weapons
            else if (weapon.Verbs[0].verbClass == typeof(Verb_LaunchProjectile))
                return APCEConstants.gunKinds.Grenade;
            //explosive launchers
            else if ((weapon.Verbs[0].CausesExplosion))
            {
                return APCEConstants.gunKinds.ExplosiveLauncher;
            }

            //a shotgun is an industrial or higher weapon and has one of the following: shotgun in its defname, label, or description, OR shotgun or gauge in its projectile
            else if ((weapon.defName.IndexOf("shotgun", 0, StringComparison.OrdinalIgnoreCase) != -1)
                        || (weapon.label.IndexOf("shotgun", 0, StringComparison.OrdinalIgnoreCase) != -1)
                        || (weapon.description.IndexOf("shotgun", 0, StringComparison.OrdinalIgnoreCase) != -1)
                        || (weapon.Verbs[0].defaultProjectile.ToString().IndexOf("shotgun", 0, StringComparison.OrdinalIgnoreCase) != -1)
                        || (weapon.Verbs[0].defaultProjectile.ToString().IndexOf("gauge", 0, StringComparison.OrdinalIgnoreCase) != -1))
                return APCEConstants.gunKinds.Shotgun;
            //a handgun is an industrial or higher weapon with burst count 1 and a range < 13
            else if ((weapon.techLevel.CompareTo(TechLevel.Industrial) >= 0) && (weapon.Verbs[0].burstShotCount == 1) && (weapon.Verbs[0].range < 13))
                return APCEConstants.gunKinds.Handgun;
            // a precision rifle is an industrial or higher weapon with burst count 1 and a range >= 13
            else if ((weapon.techLevel.CompareTo(TechLevel.Industrial) >= 0) && (weapon.Verbs[0].burstShotCount == 1) && (weapon.Verbs[0].range >= 13))
                return APCEConstants.gunKinds.precisionRifle;
            //an SMG is an industrial or higher weapon with burst count > 1 but < 6 and a range < 26
            else if ((weapon.techLevel.CompareTo(TechLevel.Industrial) >= 0) && (weapon.Verbs[0].burstShotCount > 1) && (weapon.Verbs[0].burstShotCount < 6) && (weapon.Verbs[0].range < 25.9))
                return APCEConstants.gunKinds.SMG;
            //an assault rifle is an industrial or higher weapon with burst count > 1 but <= 6 and a range >= 26
            else if ((weapon.techLevel.CompareTo(TechLevel.Industrial) >= 0) && (weapon.Verbs[0].burstShotCount > 1) && (weapon.Verbs[0].burstShotCount <= 3) && (weapon.Verbs[0].range >= 25.9))
                return APCEConstants.gunKinds.assaultRifle;
            //a machine gun is an industrial or higher weapon with range >= 26 and burst count >= 3
            else if ((weapon.techLevel.CompareTo(TechLevel.Industrial) >= 0) && (weapon.Verbs[0].range >= 25.8) && (weapon.Verbs[0].burstShotCount > 3))
                return APCEConstants.gunKinds.MachineGun;
            else
                return APCEConstants.gunKinds.Other;
        }
    }
}