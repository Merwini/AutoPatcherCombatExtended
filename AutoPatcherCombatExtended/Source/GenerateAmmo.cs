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
//        public static AmmoSetDef GenerateAmmoSet(ThingDef weapon, APCEConstants.gunKinds gunKind)
//        {

//            ThingCategoryDef newAmmoCat = new ThingCategoryDef();
//            AmmoSetDef newAmmoSet = new AmmoSetDef();
//            List<AmmoLink> newAmmoLinks = new List<AmmoLink>();
//            List<AmmoDef> newAmmos = new List<AmmoDef>(new AmmoDef[6]);
//            List<ThingDef> newProjectiles = new List<ThingDef>();

//            newAmmoSet.defName = ("APCE_Ammo_For_" + weapon.defName);
//            newAmmoSet.label = ("Ammo set for " + weapon.label);
//            newAmmoSet.description = ("A procedurally-generated ammo set for the " + weapon.label);
//            newAmmoSet.isMortarAmmoSet = false;

//            newAmmoCat.defName = weapon.defName + "_AmmoCat";
//            newAmmoCat.label = weapon.label + " Ammo";
//            newAmmoCat.childThingDefs = new List<ThingDef>();
//            newAmmoCat.childCategories = new List<ThingCategoryDef>();
//            newAmmoCat.childSpecialFilters = new List<SpecialThingFilterDef>();
//            newAmmoCat.iconPath = "UI/Icons/ThingCategories/Ammo";
//            newAmmoCat.resourceReadoutRoot = false;

//            float gunTechMult = 1f;
//            switch (weapon.techLevel)
//            {
//                case TechLevel.Animal:
//                    gunTechMult *= APCESettings.gunTechMultAnimal;
//                    break;
//                case TechLevel.Neolithic:
//                    gunTechMult *= APCESettings.gunTechMultNeolithic;
//                    break;
//                case TechLevel.Medieval:
//                    gunTechMult *= APCESettings.gunTechMultMedieval;
//                    break;
//                case TechLevel.Industrial:
//                    gunTechMult *= APCESettings.gunTechMultIndustrial;
//                    break;
//                case TechLevel.Spacer:
//                    gunTechMult *= APCESettings.gunTechMultSpacer;
//                    break;
//                case TechLevel.Ultra:
//                    gunTechMult *= APCESettings.gunTechMultUltratech;
//                    break;
//                case TechLevel.Archotech:
//                    gunTechMult *= APCESettings.gunTechMultArchotech;
//                    break;
//                default:
//                    break;
//            }

//            if ((gunKind == APCEConstants.gunKinds.Turret) && (weapon.techLevel == TechLevel.Undefined))
//            {//overrides tech level for turrets if undefined, since it seems common to not bother setting one
//                gunTechMult *= APCESettings.gunTechMultIndustrial;
//            }

//            switch (gunKind)
//            {
                
//                case APCEConstants.gunKinds.Bow:
//                    {
//                        GenerateAmmoBow(weapon, newAmmoCat, newAmmos, newProjectiles, gunTechMult);
//                        break;
//                    }

//                case APCEConstants.gunKinds.Shotgun:
//                    {
//                        GenerateAmmoShotgun(weapon, newAmmoCat, newAmmoSet, newAmmos, newProjectiles, gunTechMult);
//                        break;
//                    }

//                case APCEConstants.gunKinds.ExplosiveLauncher:
//                    {
//                        GenerateAmmoExplosiveLauncher(weapon, newAmmoCat, newAmmos, newProjectiles);
//                        break;
//                    }
//                case APCEConstants.gunKinds.Turret:
//                    {
//                        if ((weapon.Verbs[0].defaultProjectile.GetCompProperties<CompProperties_ExplosiveCE>() != null) 
//                            || (weapon.Verbs[0].defaultProjectile.GetCompProperties<CompProperties_Explosive>() != null)
//                            || (weapon.Verbs[0].defaultProjectile.thingClass == typeof(Projectile_Explosive)))
//                        {
//                            GenerateAmmoExplosiveLauncher(weapon, newAmmoCat, newAmmos, newProjectiles);
//                        }
//                        else
//                        {
//                            GenerateAmmoIndustrial(weapon, newAmmoCat, newAmmoSet, newAmmos, newProjectiles, gunTechMult);
//                        }
//                        break;
//                    }
//                default:
//                    {
//                        if ((weapon.techLevel == TechLevel.Spacer) || (weapon.techLevel == TechLevel.Ultra) || (weapon.techLevel == TechLevel.Archotech))
//                        {
//                            GenerateAmmoSpacer(weapon, newAmmoCat, newAmmoSet, newAmmos, newProjectiles, gunTechMult);

//                        }
//                        else
//                        {
//                            GenerateAmmoIndustrial(weapon, newAmmoCat, newAmmoSet, newAmmos, newProjectiles, gunTechMult);
//                        }
//                        break;
//                    }

//            }

//            for (int i = 0; i < newProjectiles.Count; i++)
//            {
//                newAmmos[i].projectile = newProjectiles[i].projectile;
//                AmmoLink al = new AmmoLink(newAmmos[i], newProjectiles[i]);
//                newAmmoLinks.Add(al);
//                newAmmoCat.childThingDefs.Add(newAmmos[i]);
//            }

//            newAmmoCat.treeNode = new TreeNode_ThingCategory(newAmmoCat);
//            InjectedDefHasher.GiveShortHashToDef(newAmmoCat, typeof(ThingCategoryDef));
//            DefGenerator.AddImpliedDef<ThingCategoryDef>(newAmmoCat);

//            newAmmoSet.ammoTypes = newAmmoLinks;
//            InjectedDefHasher.GiveShortHashToDef(newAmmoSet, typeof(AmmoSetDef));
//            DefGenerator.AddImpliedDef<AmmoSetDef>(newAmmoSet);
//            return newAmmoSet;
//        }

//        public static void GenerateAmmoBow(ThingDef weapon, ThingCategoryDef newAmmoCat, List<AmmoDef> newAmmos, List<ThingDef> newProjectiles, float gunTechMult)
//        {
//            newAmmoCat.parent = APCEDefOf.AmmoArrows;
//            for (int i = 0; i < 5; i++)
//            {
//                ThingDef newProjectile = new ThingDef();
//                newProjectile.graphicData = weapon.Verbs[0].defaultProjectile.graphicData;
//                ProjectilePropertiesCE newPPCE = new ProjectilePropertiesCE();
//                newPPCE.secondaryDamage = new List<SecondaryDamage>();

//                int damageHolder = weapon.Verbs[0].defaultProjectile.projectile.GetDamageAmount(1);
//                PatchBaseBullet(newProjectile);
//                newPPCE.damageDef = weapon.Verbs[0].defaultProjectile.projectile.damageDef;

//                newPPCE.speed = 20;
//                newPPCE.dropsCasings = false;
//                newPPCE.explosionDamageFalloff = true;
//                SetPenetrationMults(weapon, newPPCE, gunTechMult);
//                newPPCE.flyOverhead = false;
//                switch (i)
//                {
//                    case 0:
//                        {//stone
//                            newProjectile.defName = ("APCE_Stone_Arrow_" + weapon.defName);
//                            newProjectile.label = (weapon.label + " stone arrow");
//                            newPPCE.speed *= 0.9f;
//                            newPPCE.armorPenetrationSharp *= 0.4f;
//                            newPPCE.armorPenetrationBlunt *= 0.33f;
//                            newAmmos[i] = APCEDefOf.Ammo_Arrow_Stone;
//                            newPPCE.preExplosionSpawnChance = 0.333f;
//                            newPPCE.preExplosionSpawnThingDef = APCEDefOf.Ammo_Arrow_Stone;
//                            break;
//                        }
//                    case 1:
//                        {//steel
//                            newProjectile.defName = ("APCE_Steel_Arrow_" + weapon.defName);
//                            newProjectile.label = (weapon.label + " steel arrow");
//                            damageHolder = (int)(damageHolder * 1.25f + 0.5f);
//                            newAmmos[i] = APCEDefOf.Ammo_Arrow_Steel;
//                            newPPCE.preExplosionSpawnChance = 0.666f;
//                            newPPCE.preExplosionSpawnThingDef = APCEDefOf.Ammo_Arrow_Steel;
//                            break;
//                        }
//                    case 2:
//                        {//plasteel
//                            newProjectile.defName = ("APCE_Plasteel_Arrow_" + weapon.defName);
//                            newProjectile.label = (weapon.label + " plasteel arrow");
//                            newPPCE.speed *= 1.1f;
//                            newPPCE.armorPenetrationSharp *= 2f;
//                            newPPCE.armorPenetrationBlunt *= 0.5f;
//                            newAmmos[i] = APCEDefOf.Ammo_Arrow_Plasteel;
//                            newPPCE.preExplosionSpawnChance = 0.8f;
//                            newPPCE.preExplosionSpawnThingDef = APCEDefOf.Ammo_Arrow_Plasteel;
//                            break;
//                        }
//                    case 3:
//                        {//venom
//                            newProjectile.defName = ("APCE_Venom_Arrow_" + weapon.defName);
//                            newProjectile.label = (weapon.label + " venom arrow");
//                            damageHolder = (int)(damageHolder * 1.25f + 0.5f);
//                            SecondaryDamage newSD2 = new SecondaryDamage();
//                            newSD2.def = APCEDefOf.ArrowVenom;
//                            newSD2.amount = (int)(damageHolder * 0.25f + 0.5f);
//                            newSD2.chance = 1;
//                            newPPCE.secondaryDamage.Add(newSD2);
//                            newAmmos[i] = APCEDefOf.Ammo_Arrow_Venom;
//                            newPPCE.preExplosionSpawnChance = 0.666f;
//                            newPPCE.preExplosionSpawnThingDef = APCEDefOf.Ammo_Arrow_Venom;
//                            break;
//                        }
//                    case 4:
//                        {//flame
//                            newProjectile.defName = ("APCE_Flame_Arrow_" + weapon.defName);
//                            newProjectile.label = (weapon.label + " flame arrow");
//                            newProjectile.thingClass = typeof(CombatExtended.ProjectileCE_Explosive);
//                            newPPCE.damageDef = APCEDefOf.ArrowFire;
//                            damageHolder = (int)(damageHolder * 0.25f + 0.5f);
//                            newPPCE.armorPenetrationSharp = 0f;
//                            newPPCE.armorPenetrationBlunt = 0f;
//                            newAmmos[i] = APCEDefOf.Ammo_Arrow_Flame;
//                            newPPCE.preExplosionSpawnChance = 0.16f;
//                            newPPCE.preExplosionSpawnThingDef = APCEDefOf.Filth_Fuel;
//                            break;
//                        }
//                }
//                SetDamage(newPPCE, damageHolder);
//                newPPCE.secondaryDamage.AddRange(ExtraToSecondary(weapon.Verbs[0].defaultProjectile.projectile.extraDamages));
//                newProjectile.projectile = newPPCE;
//                newProjectiles.Add(newProjectile);
//                InjectedDefHasher.GiveShortHashToDef(newProjectile, typeof(ThingDef));
//                DefGenerator.AddImpliedDef<ThingDef>(newProjectile);
//            }
//        }

//        public static void GenerateAmmoShotgun(ThingDef weapon, ThingCategoryDef newAmmoCat, AmmoSetDef newAmmoSet, List<AmmoDef> newAmmos, List<ThingDef> newProjectiles, float gunTechMult)
//        {
//            newAmmoCat.parent = APCEDefOf.Ammo;
//            newAmmoSet.similarTo = APCEDefOf.AmmoSet_Shotgun;
//            for (int i = 0; i < 4; i++)
//            {
//                ThingDef newProjectile = new ThingDef();
//                newProjectile.graphicData = weapon.Verbs[0].defaultProjectile.graphicData;
//                ProjectilePropertiesCE newPPCE = new ProjectilePropertiesCE();
//                newPPCE.secondaryDamage = new List<SecondaryDamage>();

//                int damageHolder = weapon.Verbs[0].defaultProjectile.projectile.GetDamageAmount(1);
//                PatchBaseBullet(newProjectile);
//                newPPCE.damageDef = weapon.Verbs[0].defaultProjectile.projectile.damageDef;

//                newPPCE.dropsCasings = true;
//                newPPCE.casingMoteDefname = "Mote_ShotgunShell";
//                newPPCE.casingFilthDefname = "Filth_ShotgunAmmoCasings";
//                newPPCE.explosionDamageFalloff = true;
//                SetPenetrationMults(weapon, newPPCE, gunTechMult);
//                newPPCE.armorPenetrationBlunt *= 0.25f; //just for shotguns
//                newPPCE.flyOverhead = false;
//                switch (i)
//                {
//                    case 0:
//                        {//buck
//                            newProjectile.defName = ("APCE_Buckshot_Bullet_" + weapon.defName);
//                            newProjectile.label = (weapon.label + " buckshot bullet");
//                            newAmmos[i] = APCEDefOf.Ammo_APCEGeneric_Buck;
//                            damageHolder = (int)(damageHolder * 0.5f + 0.5f);
//                            newPPCE.pelletCount = 9;
//                            newPPCE.spreadMult = 8.9f;
//                            newPPCE.speed = 72;
//                            break;
//                        }
//                    case 1:
//                        {//slug
//                            newProjectile.defName = ("APCE_Slug_Bullet_" + weapon.defName);
//                            newProjectile.label = (weapon.label + " slug bullet");
//                            newAmmos[i] = APCEDefOf.Ammo_APCEGeneric_Slug;
//                            damageHolder = (int)(damageHolder * 3f + 0.5f);
//                            newPPCE.armorPenetrationSharp *= 1.5f;
//                            newPPCE.armorPenetrationBlunt *= 20f;
//                            newPPCE.speed = 110;
//                            break;
//                        }
//                    case 2:
//                        {//beanbag
//                            newProjectile.defName = ("APCE_Beanbag_Bullet_" + weapon.defName);
//                            newProjectile.label = (weapon.label + " beanbag bullet");
//                            newAmmos[i] = APCEDefOf.Ammo_APCEGeneric_Beanbag;
//                            damageHolder = (int)(damageHolder * 0.5f + 0.5f);
//                            newPPCE.damageDef = APCEDefOf.Beanbag;
//                            newPPCE.armorPenetrationSharp = 0f;
//                            newPPCE.armorPenetrationBlunt *= 0.75f;
//                            newPPCE.spreadMult = 2f;
//                            newPPCE.speed = 22;
//                            break;
//                        }
//                    case 3:
//                        {//electroslug
//                            newProjectile.defName = ("APCE_ElectroSlug_Bullet_" + weapon.defName);
//                            newProjectile.label = (weapon.label + " ion charged bullet");
//                            newAmmos[i] = APCEDefOf.Ammo_APCEGeneric_ElectroSlug;
//                            damageHolder = (int)(damageHolder * 1.25f + 0.5f);
//                            newPPCE.damageDef = DamageDefOf.EMP;
//                            newPPCE.armorPenetrationSharp = 0f;
//                            newPPCE.armorPenetrationBlunt = 0f;
//                            newPPCE.speed = 30f;
//                            newPPCE.empShieldBreakChance = 0.2f;
//                            break;
//                        }
//                }
//                SetDamage(newPPCE, damageHolder);
//                newPPCE.secondaryDamage.AddRange(ExtraToSecondary(weapon.Verbs[0].defaultProjectile.projectile.extraDamages));
//                newProjectile.projectile = newPPCE;
//                newProjectiles.Add(newProjectile);
//                InjectedDefHasher.GiveShortHashToDef(newProjectile, typeof(ThingDef));
//                DefGenerator.AddImpliedDef<ThingDef>(newProjectile);
//            }
//        }

//        public static void GenerateAmmoExplosiveLauncher(ThingDef weapon, ThingCategoryDef newAmmoCat, List<AmmoDef> newAmmos, List<ThingDef> newProjectiles)
//        {
//            newAmmoCat.parent = APCEDefOf.Ammo;

//            for (int i = 0; i < 3; i++)
//            {
//                ThingDef newProjectile = new ThingDef();
//                newProjectile.graphicData = weapon.Verbs[0].defaultProjectile.graphicData;
//                ProjectilePropertiesCE newPPCE = new ProjectilePropertiesCE();
//                newPPCE.secondaryDamage = new List<SecondaryDamage>();

//                int damageHolder = weapon.Verbs[0].defaultProjectile.projectile.GetDamageAmount(1);
//                if (weapon.Verbs[0].defaultProjectile.projectile.damageDef == DamageDefOf.Bomb && damageHolder == 635)
//                {//CE Changes the defaultDamage of the Bomb DamageDef to 635. Any weapon relying on the default will do too much damage
//                    damageHolder = 50;
//                }
//                PatchBaseBullet(newProjectile);

//                newPPCE.dropsCasings = true;
//                newPPCE.explosionDamageFalloff = true;
//                newPPCE.flyOverhead = false;
//                switch (i)
//                {
//                    case 0:
//                        {//incendiary
//                            newProjectile.defName = ("APCE_Incendiary_Bullet_" + weapon.defName);
//                            newProjectile.label = (weapon.label + " incendiary bullet");
//                            newAmmos[i] = APCEDefOf.Ammo_APCELauncher_Incendiary;
//                            damageHolder = (int)(damageHolder * 0.66f + 0.5f);
//                            newPPCE.damageDef = APCEDefOf.PrometheumFlame;
//                            newPPCE.explosionRadius = weapon.Verbs[0].defaultProjectile.projectile.explosionRadius * 4f;
//                            newPPCE.speed = 40;
//                            newPPCE.preExplosionSpawnChance = 0.2f;
//                            newPPCE.preExplosionSpawnThingDef = APCEDefOf.FilthPrometheum;
//                            newPPCE.ai_IsIncendiary = true;
//                            break;
//                        }
//                    case 1:
//                        {//thermobaric
//                            newProjectile.defName = ("APCE_Thermobaric_Bullet_" + weapon.defName);
//                            newProjectile.label = (weapon.label + " thermobaric bullet");
//                            newAmmos[i] = APCEDefOf.Ammo_APCELauncher_Thermobaric;
//                            damageHolder = (int)(damageHolder * 5f + 0.5f);
//                            newPPCE.damageDef = APCEDefOf.Thermobaric;
//                            newPPCE.explosionRadius = weapon.Verbs[0].defaultProjectile.projectile.explosionRadius * 2f;
//                            newPPCE.speed = 40;
//                            newPPCE.ai_IsIncendiary = true;
//                            newPPCE.applyDamageToExplosionCellsNeighbors = true;
//                            newPPCE.soundExplode = APCEDefOf.MortarBomb_Explode;
//                            break;
//                        }
//                    case 2:
//                        {//foam
//                            newProjectile.defName = ("APCE_Foam_Bullet_" + weapon.defName);
//                            newProjectile.label = (weapon.label + " foam bullet");
//                            newAmmos[i] = APCEDefOf.Ammo_APCELauncher_Foam;
//                            newPPCE.damageDef = DamageDefOf.Extinguish;
//                            newPPCE.explosionRadius = weapon.Verbs[0].defaultProjectile.projectile.explosionRadius * 3f;
//                            newPPCE.speed = 40;
//                            newPPCE.suppressionFactor = 0f;
//                            newPPCE.dangerFactor = 0f;
//                            newPPCE.preExplosionSpawnChance = 1f;
//                            newPPCE.preExplosionSpawnThingDef = APCEDefOf.Filth_FireFoam;
//                            break;
//                        }
//                }
//                SetDamage(newPPCE, damageHolder);
//                newPPCE.secondaryDamage.AddRange(ExtraToSecondary(weapon.Verbs[0].defaultProjectile.projectile.extraDamages));
//                newProjectile.projectile = newPPCE;
//                newProjectiles.Add(newProjectile);
//                InjectedDefHasher.GiveShortHashToDef(newProjectile, typeof(ThingDef));
//                DefGenerator.AddImpliedDef<ThingDef>(newProjectile);
//            }
//        }

//        public static void GenerateAmmoIndustrial(ThingDef weapon, ThingCategoryDef newAmmoCat, AmmoSetDef newAmmoSet, List<AmmoDef> newAmmos, List<ThingDef> newProjectiles, float gunTechMult)
//        {
//            newAmmoCat.parent = APCEDefOf.Ammo;
//            newAmmoSet.similarTo = APCEDefOf.AmmoSet_RifleIntermediate;

//            for (int i = 0; i < 6; i++)
//            {//make the projectiles
//                ThingDef newProjectile = new ThingDef();
//                newProjectile.graphicData = weapon.Verbs[0].defaultProjectile.graphicData;
//                ProjectilePropertiesCE newPPCE = new ProjectilePropertiesCE();
//                newPPCE.secondaryDamage = new List<SecondaryDamage>();

//                int damageHolder = weapon.Verbs[0].defaultProjectile.projectile.GetDamageAmount(1);
//                PatchBaseBullet(newProjectile);

//                newPPCE.damageDef = weapon.Verbs[0].defaultProjectile.projectile.damageDef;
//                newPPCE.speed = 185;
//                newPPCE.dropsCasings = true;
//                newPPCE.casingMoteDefname = "Mote_EmptyCasing";
//                newPPCE.casingFilthDefname = "Filth_RifleAmmoCasings";
//                newPPCE.explosionDamageFalloff = true;
//                SetPenetrationMults(weapon, newPPCE, gunTechMult);
//                newPPCE.flyOverhead = false;
//                switch (i)
//                {
//                    case 0:
//                        {//FMJ
//                            newProjectile.defName = ("APCE_FMJ_Bullet_" + weapon.defName);
//                            newProjectile.label = (weapon.label + " FMJ bullet");
//                            newAmmos[i] = APCEDefOf.Ammo_APCEGeneric_FMJ;
//                            break;
//                        }
//                    case 1:
//                        {//AP
//                            newProjectile.defName = ("APCE_AP_Bullet_" + weapon.defName);
//                            newProjectile.label = (weapon.label + " AP bullet");
//                            damageHolder = (int)(damageHolder * 0.66f + 0.5f);
//                            newPPCE.armorPenetrationSharp *= 2;
//                            newAmmos[i] = APCEDefOf.Ammo_APCEGeneric_AP;
//                            break;
//                        }
//                    case 2:
//                        {//HP
//                            newProjectile.defName = ("APCE_HP_Bullet_" + weapon.defName);
//                            newProjectile.label = (weapon.label + " HP bullet");
//                            damageHolder = (int)(damageHolder * 1.33f + 0.5f);
//                            newPPCE.armorPenetrationSharp *= 0.5f;
//                            newAmmos[i] = APCEDefOf.Ammo_APCEGeneric_HP;
//                            break;
//                        }
//                    case 3:
//                        {//API
//                            newProjectile.defName = ("APCE_API_Bullet_" + weapon.defName);
//                            newProjectile.label = (weapon.label + " AP-I bullet");
//                            damageHolder = (int)(damageHolder * 0.66f + 0.5f);
//                            SecondaryDamage newSD2 = new SecondaryDamage();
//                            newSD2.def = DamageDefOf.Burn;
//                            newSD2.amount = (int)(damageHolder * 0.33f + 0.5f);
//                            newSD2.chance = 1;
//                            newPPCE.secondaryDamage.Add(newSD2);
//                            newPPCE.armorPenetrationSharp *= 2;
//                            newAmmos[i] = APCEDefOf.Ammo_APCEGeneric_Incendiary;
//                            break;
//                        }
//                    case 4:
//                        {//HE
//                            newProjectile.defName = ("APCE_HE_Bullet_" + weapon.defName);
//                            newProjectile.label = (weapon.label + " HE bullet");
//                            SecondaryDamage newSD2 = new SecondaryDamage();
//                            newSD2.def = DamageDefOf.Bomb;
//                            newSD2.amount = (int)(damageHolder * 0.66f + 0.5f);
//                            newSD2.chance = 1;
//                            newPPCE.secondaryDamage.Add(newSD2);
//                            newAmmos[i] = APCEDefOf.Ammo_APCEGeneric_HE;
//                            break;
//                        }
//                    case 5:
//                        {//Sabot
//                            newProjectile.defName = ("APCE_Sabot_Bullet_" + weapon.defName);
//                            newProjectile.label = (weapon.label + " Sabot bullet");
//                            damageHolder = (int)(damageHolder * 0.5f + 0.5f);
//                            newPPCE.armorPenetrationSharp *= 3.5f;
//                            newPPCE.armorPenetrationBlunt *= 1.3f;
//                            newPPCE.speed *= 1.5f;
//                            newAmmos[i] = APCEDefOf.Ammo_APCEGeneric_Sabot;
//                            break;
//                        }
//                }
//                SetDamage(newPPCE, damageHolder);
//                newPPCE.secondaryDamage.AddRange(ExtraToSecondary(weapon.Verbs[0].defaultProjectile.projectile.extraDamages));
//                newProjectile.projectile = newPPCE;
//                newProjectiles.Add(newProjectile);
//                InjectedDefHasher.GiveShortHashToDef(newProjectile, typeof(ThingDef));
//                DefGenerator.AddImpliedDef<ThingDef>(newProjectile);
//            }
//        }

//        public static void GenerateAmmoSpacer(ThingDef weapon, ThingCategoryDef newAmmoCat, AmmoSetDef newAmmoSet, List<AmmoDef> newAmmos, List<ThingDef> newProjectiles, float gunTechMult)
//        {
//            newAmmoCat.parent = APCEDefOf.AmmoAdvanced;
//            newAmmoSet.similarTo = APCEDefOf.AmmoSet_ChargedRifle;

//            for (int i = 0; i < 3; i++)
//            {
//                ThingDef newProjectile = new ThingDef();
//                newProjectile.graphicData = weapon.Verbs[0].defaultProjectile.graphicData;
//                ProjectilePropertiesCE newPPCE = new ProjectilePropertiesCE();
//                newPPCE.secondaryDamage = new List<SecondaryDamage>();

//                int damageHolder = weapon.Verbs[0].defaultProjectile.projectile.GetDamageAmount(1);
//                PatchBaseBullet(newProjectile);
//                newPPCE.damageDef = weapon.Verbs[0].defaultProjectile.projectile.damageDef;
//                //newPPCE.damageDef = DamageDefOf.Bullet;

//                newPPCE.speed = 160;
//                newPPCE.dropsCasings = false;
//                newPPCE.explosionDamageFalloff = true;
//                SetPenetrationMults(weapon, newPPCE, gunTechMult);
//                newPPCE.flyOverhead = false;

//                switch (i)
//                {
//                    case 0:
//                        {//Charged
//                            newProjectile.defName = ("APCE_Charged_Bullet_" + weapon.defName);
//                            newProjectile.label = (weapon.label + " charged bullet");
//                            newAmmos[i] = APCEDefOf.Ammo_APCEGeneric_Charged;
//                            break;
//                        }
//                    case 1:
//                        {//Conc
//                            newProjectile.defName = ("APCE_Conc_Bullet_" + weapon.defName);
//                            newProjectile.label = (weapon.label + " conc. charged bullet");
//                            damageHolder = (int)(damageHolder * 0.75f + 0.5f);
//                            newPPCE.armorPenetrationSharp *= 2;
//                            newAmmos[i] = APCEDefOf.Ammo_APCEGeneric_ChargedAP;
//                            break;

//                        }
//                    case 2:
//                        {//Ion
//                            newProjectile.defName = ("APCE_Ion_Bullet_" + weapon.defName);
//                            newProjectile.label = (weapon.label + " ion charged bullet");
//                            newAmmos[i] = APCEDefOf.Ammo_APCEGeneric_ChargedIon;
//                            damageHolder = (int)(damageHolder * 0.75f + 0.5f);
//                            newPPCE.armorPenetrationSharp *= 1.5f;
//                            SecondaryDamage newSD2 = new SecondaryDamage();
//                            newSD2.def = DamageDefOf.EMP;
//                            newSD2.amount = (int)(damageHolder * 0.5f + 0.5f);
//                            newSD2.chance = 1;
//                            newPPCE.empShieldBreakChance = 0.2f;
//                            newPPCE.secondaryDamage.Add(newSD2);
//                            break;
//                        }
//                }
//                SetDamage(newPPCE, damageHolder);
//                newPPCE.secondaryDamage.AddRange(ExtraToSecondary(weapon.Verbs[0].defaultProjectile.projectile.extraDamages));
//                newProjectile.projectile = newPPCE;
//                newProjectiles.Add(newProjectile);
//                InjectedDefHasher.GiveShortHashToDef(newProjectile, typeof(ThingDef));
//                DefGenerator.AddImpliedDef<ThingDef>(newProjectile);
//            }
//        }

//        public static void SetPenetrationMults(ThingDef weapon, ProjectilePropertiesCE newPPCE, float gunTechMult)
//        {
//            newPPCE.armorPenetrationSharp = weapon.Verbs[0].defaultProjectile.projectile.GetArmorPenetration(1) * APCESettings.gunSharpPenMult * gunTechMult;
//            newPPCE.armorPenetrationBlunt = weapon.Verbs[0].defaultProjectile.projectile.GetArmorPenetration(1) * APCESettings.gunBluntPenMult * gunTechMult;
//        }
//    }
//}