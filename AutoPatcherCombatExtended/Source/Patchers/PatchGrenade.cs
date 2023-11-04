//using CombatExtended;
//using RimWorld;
//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Linq;
//using System.Reflection;
//using System.Text;
//using Verse;

//namespace nuff.AutoPatcherCombatExtended
//{
//    partial class APCEController
//    {
//        public static void PatchGrenade(ThingDef grenade)
//        {
//            grenade.thingClass = typeof(CombatExtended.AmmoThing);
//            grenade.stackLimit = 75;
//            grenade.resourceReadoutPriority = ResourceCountPriority.First;
//            /* I dunno what this patch operation does. Need to figure out how to do it in code
//             <Operation Class="PatchOperationAttributeSet">
//                    <xpath>Defs/ThingDef[defName="Weapon_GrenadeFrag"]</xpath>
//                    <attribute>Class</attribute>
//                    <value>CombatExtended.AmmoDef</value>
//                </Operation>
//            */

//            PatchAllVerbs(grenade);
//            PatchGrenadePP(grenade);

//            ConvertCompProperties_Explosive(grenade);
//            if (grenade.Verbs[0].defaultProjectile.projectile.damageDef == DamageDefOf.Bomb)
//            {
//                AddCompProperties_Fragments(grenade);
//            }
//        }

//        public static void PatchGrenadePP(ThingDef grenade)
//        {
//            ProjectilePropertiesCE grenadePPCE = new ProjectilePropertiesCE();//grenade.Verbs[0].defaultProjectile.projectile as ProjectilePropertiesCE;
//            ProjectileProperties grenadePP = grenade.Verbs[0].defaultProjectile.projectile;
//            CopyFields(grenadePP, grenadePPCE);
//            grenadePPCE.dropsCasings = true;
//            grenadePPCE.casingMoteDefname = "Mote_GrenadePin";
//            grenadePPCE.casingFilthDefname = "Filth_GrenadeAmmoCasings";
//            grenadePPCE.applyDamageToExplosionCellsNeighbors = true;
//            grenadePPCE.suppressionFactor = 3f;
//            grenadePPCE.dangerFactor = 2f;
//            grenadePPCE.airborneSuppressionFactor = 0.25f;
//            SetForcedMiss(grenade.Verbs[0], 1.9f);
            
//            int damageHolder = grenadePP.GetDamageAmount(1);
//            if (grenadePP.damageDef == DamageDefOf.Bomb && damageHolder == 635)
//            {//CE Changes the defaultDamage of the Bomb DamageDef to 635. Any weapon relying on the default will do too much damage
//                damageHolder = 50;
//            }
//            SetDamage(grenadePPCE, damageHolder);
//        }

//        public static void AddCompProperties_ExplosiveCE(ThingDef weapon)
//        {
//            CompProperties_ExplosiveCE compEx = new CompProperties_ExplosiveCE();
//            compEx.damageAmountBase = weapon.Verbs[0].defaultProjectile.projectile.GetDamageAmount(1);
//            compEx.explosiveDamageType = weapon.Verbs[0].defaultProjectile.projectile.damageDef;
//            compEx.explosiveRadius = weapon.Verbs[0].defaultProjectile.projectile.explosionRadius;
//            weapon.comps.Add(compEx);
//        }

//        public static void ConvertCompProperties_Explosive(ThingDef weapon)
//        {
//            CompProperties_Explosive compEx = weapon.GetCompProperties<CompProperties_Explosive>();
//            if (compEx != null)
//            {
//                //convert logic
//                CompProperties_ExplosiveCE compExCE = new CompProperties_ExplosiveCE();

//                // Get all the fields of the two classes
//                FieldInfo[] fieldsExplosive = typeof(CompProperties_Explosive).GetFields(BindingFlags.Public | BindingFlags.Instance);
//                FieldInfo[] fieldsExplosiveCE = typeof(CompProperties_ExplosiveCE).GetFields(BindingFlags.Public | BindingFlags.Instance);

//                // Iterate through the fields
//                foreach (FieldInfo fieldExplosive in fieldsExplosive)
//                {
//                    foreach (FieldInfo fieldExplosiveCE in fieldsExplosiveCE)
//                    {
//                        // Check if the fields have the same name and type
//                        if (fieldExplosive.Name == fieldExplosiveCE.Name && fieldExplosive.FieldType == fieldExplosiveCE.FieldType)
//                        {
//                            // Copy the value from the CompProperties_Explosive field to the CompProperties_ExplosiveCE field
//                            fieldExplosiveCE.SetValue(compExCE, fieldExplosive.GetValue(compEx));
//                            break;
//                        }
//                    }
//                }

//                if (compExCE.damageAmountBase == 635 && compExCE.explosiveDamageType == DamageDefOf.Bomb)
//                {
//                    compExCE.damageAmountBase = 50;
//                }

//                compExCE.compClass = typeof(CompExplosiveCE); // need to change this back, since the above reflection changes it to CompExplosive

//                weapon.comps.RemoveAll(cp => cp is CompProperties_Explosive);

//                /*
//                for (int i = weapon.comps.Count - 1; i >= 0; i--)
//                {
//                    if (weapon.comps[i] is CompProperties_Explosive)
//                    {
//                        weapon.comps.RemoveAt(i);
//                    }
//                }
//                */

//                weapon.comps.Add(compExCE);
//            }
//            else
//            {
//                return;
//            }
//        }

//        public static void AddCompProperties_Fragments(ThingDef weapon)
//        {
//            CompProperties_Fragments compFrag = new CompProperties_Fragments();
//            compFrag.fragments = new List<ThingDefCountClass>() { new ThingDefCountClass(APCEDefOf.Fragment_Small, 40)}; //TODO maybe change 40 to something like damage * 0.8?
//            weapon.comps.Add(compFrag);
//        }

//        public static void SetForcedMiss(VerbProperties vp, float radius)
//        {
//            Type tvp = typeof(VerbProperties);
//            FieldInfo fmr = tvp.GetField("forcedMissRadius", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
//            fmr.SetValue(vp, (float)radius);
//        }
//    }
//}