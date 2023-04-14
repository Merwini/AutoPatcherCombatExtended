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
        internal static void PatchGrenade(ThingDef grenade)
        {
            grenade.thingClass = typeof(CombatExtended.AmmoThing);
            grenade.stackLimit = 75;
            grenade.resourceReadoutPriority = ResourceCountPriority.First;
            /* I dunno what this patch operation does. Need to figure out how to do it in code
             <Operation Class="PatchOperationAttributeSet">
                    <xpath>Defs/ThingDef[defName="Weapon_GrenadeFrag"]</xpath>
                    <attribute>Class</attribute>
                    <value>CombatExtended.AmmoDef</value>
                </Operation>
            */


            PatchGrenadePP(grenade);

            AddCompExplosiveCE(grenade);
            if (grenade.Verbs[0].defaultProjectile.projectile.damageDef == DamageDefOf.Bomb)
            {
                AddCompFragments(grenade);
            }
        }

        internal static void PatchGrenadePP(ThingDef grenade)
        {
            ProjectilePropertiesCE grenadePPCE = grenade.Verbs[0].defaultProjectile.projectile as ProjectilePropertiesCE;
            grenadePPCE.dropsCasings = true;
            grenadePPCE.casingMoteDefname = "Mote_GrenadePin";
            grenadePPCE.casingFilthDefname = "Filth_GrenadeAmmoCasings";
            grenadePPCE.applyDamageToExplosionCellsNeighbors = true;
            grenadePPCE.suppressionFactor = 3f;
            grenadePPCE.dangerFactor = 2f;
            grenadePPCE.airborneSuppressionFactor = 0.25f;
            SetForcedMiss(grenade.Verbs[0], 1.9f);
            
            int damageHolder = grenade.Verbs[0].defaultProjectile.projectile.GetDamageAmount(1);
            if (grenade.Verbs[0].defaultProjectile.projectile.damageDef == DamageDefOf.Bomb && damageHolder == 635)
            {//CE Changes the defaultDamage of the Bomb DamageDef to 635. Any weapon relying on the default will do too much damage
                damageHolder = 50;
            }
            SetDamage(grenadePPCE, damageHolder);
        }

        internal static void AddCompExplosiveCE(ThingDef weapon)
        {
            //TODO
            CompProperties_ExplosiveCE compEx = new CompProperties_ExplosiveCE();
            compEx.damageAmountBase = weapon.Verbs[0].defaultProjectile.projectile.GetDamageAmount(1);
            compEx.explosiveDamageType = weapon.Verbs[0].defaultProjectile.projectile.damageDef;
            compEx.explosiveRadius = weapon.Verbs[0].defaultProjectile.projectile.explosionRadius;
            weapon.comps.Add(compEx);
        }

        internal static void AddCompFragments(ThingDef weapon)
        {
            //TODO
            CompProperties_Fragments compFrag = new CompProperties_Fragments();
            compFrag.fragments = new List<ThingDefCountClass>() { new ThingDefCountClass(APCEDefOf.Fragment_Small, 40)}; //maybe change 40 to something like damage * 0.8?
            weapon.comps.Add(compFrag);
        }

        internal static void SetForcedMiss(VerbProperties vp, float radius)
        {
            //experimental reflection attempt
            Type tvp = typeof(VerbProperties);
            FieldInfo fmr = tvp.GetField("forcedMissRadius", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            fmr.SetValue(vp, (float)radius);
        }
    }
}