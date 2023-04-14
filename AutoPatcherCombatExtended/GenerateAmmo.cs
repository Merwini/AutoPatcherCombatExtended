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
        internal static AmmoSetDef GenerateAmmoSet(ThingDef weapon, APCESettings.gunKinds gunKind) //TODO WIP
        {

            ThingCategoryDef newAmmoCat = new ThingCategoryDef();
            AmmoSetDef newAmmoSet = new AmmoSetDef();
            List<AmmoLink> newAmmoLinks = new List<AmmoLink>();
            List<AmmoDef> newAmmos = new List<AmmoDef>(new AmmoDef[6]);
            List<ThingDef> newProjectiles = new List<ThingDef>();

            newAmmoSet.defName = ("APCE_Ammo_For_" + weapon.defName);
            newAmmoSet.label = ("Ammo set for " + weapon.label);
            newAmmoSet.description = ("A procedurally-generated ammo set for the " + weapon.label);
            newAmmoSet.isMortarAmmoSet = false;

            newAmmoCat.defName = weapon.defName + "_AmmoCat";
            newAmmoCat.label = weapon.label + " Ammo";
            newAmmoCat.childThingDefs = new List<ThingDef>();
            newAmmoCat.childCategories = new List<ThingCategoryDef>();
            newAmmoCat.childSpecialFilters = new List<SpecialThingFilterDef>();
            newAmmoCat.iconPath = "UI/Icons/ThingCategories/Ammo";
            newAmmoCat.resourceReadoutRoot = false;


            //TODO extract duplicate code
            switch (gunKind)
            {
                
                case APCESettings.gunKinds.Bow:
                    {
                        newAmmoCat.parent = APCEDefOf.AmmoArrows;
                        for (int i = 0; i < 5; i++)
                        {
                            ThingDef newProjectile = new ThingDef();
                            newProjectile.graphicData = new GraphicData();
                            ProjectilePropertiesCE newPPCE = new ProjectilePropertiesCE();
                            newPPCE.secondaryDamage = new List<SecondaryDamage>();

                            int damageHolder = weapon.Verbs[0].defaultProjectile.projectile.GetDamageAmount(1);
                            PatchBaseBullet(newProjectile);
                            newProjectile.graphicData.texPath = weapon.Verbs[0].defaultProjectile.graphicData.texPath;
                            newProjectile.graphicData.graphicClass = weapon.Verbs[0].defaultProjectile.graphicData.graphicClass;
                            newPPCE.damageDef = weapon.Verbs[0].defaultProjectile.projectile.damageDef;

                            newPPCE.speed = 20;
                            newPPCE.dropsCasings = false;
                            newPPCE.explosionDamageFalloff = true;
                            newPPCE.armorPenetrationSharp = weapon.Verbs[0].defaultProjectile.projectile.GetArmorPenetration(1) * APCESettings.gunSharpPenMult;
                            newPPCE.armorPenetrationBlunt = weapon.Verbs[0].defaultProjectile.projectile.GetArmorPenetration(1) * APCESettings.gunBluntPenMult;
                            newPPCE.flyOverhead = false;
                            //TODO maybe use CE's arrow textures?
                            switch (i)
                            {
                                case 0:
                                    {//stone
                                        newProjectile.defName = ("APCE_Stone_Arrow_" + weapon.defName);
                                        newProjectile.label = (weapon.label + " stone arrow");
                                        newPPCE.speed *= 0.9f;
                                        newPPCE.armorPenetrationSharp *= 0.4f;
                                        newPPCE.armorPenetrationBlunt *= 0.33f;
                                        newAmmos[i] = APCEDefOf.Ammo_Arrow_Stone;
                                        newPPCE.preExplosionSpawnChance = 0.333f;
                                        newPPCE.preExplosionSpawnThingDef = APCEDefOf.Ammo_Arrow_Stone;
                                        break;
                                    }
                                case 1:
                                    {//steel
                                        newProjectile.defName = ("APCE_Steel_Arrow_" + weapon.defName);
                                        newProjectile.label = (weapon.label + " steel arrow");
                                        damageHolder = (int)(damageHolder * 1.25f + 0.5f);
                                        newAmmos[i] = APCEDefOf.Ammo_Arrow_Steel;
                                        newPPCE.preExplosionSpawnChance = 0.666f;
                                        newPPCE.preExplosionSpawnThingDef = APCEDefOf.Ammo_Arrow_Steel;
                                        break;
                                    }
                                case 2:
                                    {//plasteel
                                        newProjectile.defName = ("APCE_Plasteel_Arrow_" + weapon.defName);
                                        newProjectile.label = (weapon.label + " plasteel arrow");
                                        newPPCE.speed *= 1.1f;
                                        newPPCE.armorPenetrationSharp *= 2f;
                                        newPPCE.armorPenetrationBlunt *= 0.5f;
                                        newAmmos[i] = APCEDefOf.Ammo_Arrow_Plasteel;
                                        newPPCE.preExplosionSpawnChance = 0.8f;
                                        newPPCE.preExplosionSpawnThingDef = APCEDefOf.Ammo_Arrow_Plasteel;
                                        break;
                                    }
                                case 3:
                                    {//venom
                                        newProjectile.defName = ("APCE_Venom_Arrow_" + weapon.defName);
                                        newProjectile.label = (weapon.label + " venom arrow");
                                        damageHolder = (int)(damageHolder * 1.25f + 0.5f);
                                        SecondaryDamage newSD2 = new SecondaryDamage();
                                        newSD2.def = APCEDefOf.ArrowVenom;
                                        newSD2.amount = (int)(damageHolder * 0.25f + 0.5f);
                                        newSD2.chance = 1;
                                        newPPCE.secondaryDamage.Add(newSD2);
                                        newAmmos[i] = APCEDefOf.Ammo_Arrow_Venom;
                                        newPPCE.preExplosionSpawnChance = 0.666f;
                                        newPPCE.preExplosionSpawnThingDef = APCEDefOf.Ammo_Arrow_Venom;
                                        break;
                                    }
                                case 4:
                                    {//flame
                                        newProjectile.defName = ("APCE_Flame_Arrow_" + weapon.defName);
                                        newProjectile.label = (weapon.label + " flame arrow");
                                        newProjectile.thingClass = typeof(CombatExtended.ProjectileCE_Explosive);
                                        newPPCE.damageDef = APCEDefOf.ArrowFire;
                                        damageHolder = (int)(damageHolder * 0.25f + 0.5f);
                                        newPPCE.armorPenetrationSharp = 0f;
                                        newPPCE.armorPenetrationBlunt = 0f;
                                        newAmmos[i] = APCEDefOf.Ammo_Arrow_Flame;
                                        newPPCE.preExplosionSpawnChance = 0.16f;
                                        newPPCE.preExplosionSpawnThingDef = APCEDefOf.Filth_Fuel;
                                        break;
                                    }
                            }
                            SetDamage(newPPCE, damageHolder);
                            newPPCE.secondaryDamage.AddRange(ExtraToSecondary(weapon.Verbs[0].defaultProjectile.projectile.extraDamages));
                            newProjectile.projectile = newPPCE;
                            newProjectiles.Add(newProjectile);
                            DefGenerator.AddImpliedDef<ThingDef>(newProjectile);
                        }
                        break;
                    }
                    
                case APCESettings.gunKinds.Shotgun:
                    {
                        newAmmoCat.parent = APCEDefOf.Ammo;
                        for (int i = 0; i < 4; i++)
                        {
                            ThingDef newProjectile = new ThingDef();
                            newProjectile.graphicData = new GraphicData();
                            ProjectilePropertiesCE newPPCE = new ProjectilePropertiesCE();
                            newPPCE.secondaryDamage = new List<SecondaryDamage>();

                            int damageHolder = weapon.Verbs[0].defaultProjectile.projectile.GetDamageAmount(1);
                            PatchBaseBullet(newProjectile);
                            newProjectile.graphicData.texPath = weapon.Verbs[0].defaultProjectile.graphicData.texPath;
                            newProjectile.graphicData.graphicClass = weapon.Verbs[0].defaultProjectile.graphicData.graphicClass;
                            newPPCE.damageDef = weapon.Verbs[0].defaultProjectile.projectile.damageDef;

                            newPPCE.dropsCasings = true;
                            newPPCE.explosionDamageFalloff = true;
                            newPPCE.armorPenetrationSharp = weapon.Verbs[0].defaultProjectile.projectile.GetArmorPenetration(1) * APCESettings.gunSharpPenMult;
                            newPPCE.armorPenetrationBlunt = weapon.Verbs[0].defaultProjectile.projectile.GetArmorPenetration(1) * APCESettings.gunBluntPenMult / 4f;
                            newPPCE.flyOverhead = false;
                            switch (i)
                            {
                                case 0:
                                    {//buck
                                        newProjectile.defName = ("APCE_Buckshot_Bullet_" + weapon.defName);
                                        newProjectile.label = (weapon.label + " buckshot bullet");
                                        newAmmos[i] = APCEDefOf.Ammo_APCEGeneric_Buck;
                                        damageHolder = (int)(damageHolder * 0.5f + 0.5f);
                                        newPPCE.pelletCount = 9;
                                        newPPCE.spreadMult = 8.9f;
                                        newPPCE.speed = 72;
                                        break;
                                    }
                                case 1:
                                    {//slug
                                        newProjectile.defName = ("APCE_Slug_Bullet_" + weapon.defName);
                                        newProjectile.label = (weapon.label + " slug bullet");
                                        newAmmos[i] = APCEDefOf.Ammo_APCEGeneric_Slug;
                                        damageHolder = (int)(damageHolder * 3f + 0.5f);
                                        newPPCE.armorPenetrationSharp *= 1.5f;
                                        newPPCE.armorPenetrationBlunt *= 20f;
                                        newPPCE.speed = 110;
                                        break;
                                    }
                                case 2:
                                    {//beanbag
                                        newProjectile.defName = ("APCE_Beanbag_Bullet_" + weapon.defName);
                                        newProjectile.label = (weapon.label + " beanbag bullet");
                                        newAmmos[i] = APCEDefOf.Ammo_APCEGeneric_Beanbag;
                                        damageHolder = (int)(damageHolder * 0.5f + 0.5f);
                                        newPPCE.damageDef = APCEDefOf.Beanbag;
                                        newPPCE.armorPenetrationSharp = 0f;
                                        newPPCE.armorPenetrationBlunt *= 0.75f;
                                        newPPCE.spreadMult = 2f;
                                        newPPCE.speed = 22;
                                        break;
                                    }
                                case 3:
                                    {//electroslug
                                        newProjectile.defName = ("APCE_ElectroSlug_Bullet_" + weapon.defName);
                                        newProjectile.label = (weapon.label + " ion charged bullet");
                                        newAmmos[i] = APCEDefOf.Ammo_APCEGeneric_ElectroSlug;
                                        damageHolder = (int)(damageHolder * 1.25f + 0.5f);
                                        newPPCE.damageDef = DamageDefOf.EMP;
                                        newPPCE.armorPenetrationSharp = 0f;
                                        newPPCE.armorPenetrationBlunt = 0f;
                                        newPPCE.speed = 30f;
                                        newPPCE.empShieldBreakChance = 0.2f;
                                        break;
                                    }
                                default:
                                    {
                                        break;
                                    }
                            }
                            SetDamage(newPPCE, damageHolder);
                            newPPCE.secondaryDamage.AddRange(ExtraToSecondary(weapon.Verbs[0].defaultProjectile.projectile.extraDamages));
                            newProjectile.projectile = newPPCE;
                            newProjectiles.Add(newProjectile);
                            DefGenerator.AddImpliedDef<ThingDef>(newProjectile);
                        }
                        break;
                    }
                
            case APCESettings.gunKinds.ExplosiveLauncher:
                {
                        newAmmoCat.parent = APCEDefOf.Ammo;
                        for (int i = 0; i < 4; i++)
                        {
                            ThingDef newProjectile = new ThingDef();
                            newProjectile.graphicData = new GraphicData();
                            ProjectilePropertiesCE newPPCE = new ProjectilePropertiesCE();
                            newPPCE.secondaryDamage = new List<SecondaryDamage>();

                            int damageHolder = weapon.Verbs[0].defaultProjectile.projectile.GetDamageAmount(1);
                            PatchBaseBullet(newProjectile);
                            newProjectile.graphicData.texPath = weapon.Verbs[0].defaultProjectile.graphicData.texPath;
                            newProjectile.graphicData.graphicClass = weapon.Verbs[0].defaultProjectile.graphicData.graphicClass;
                            newProjectile.graphicData.shaderType = weapon.Verbs[0].defaultProjectile.graphicData.shaderType;
                            newPPCE.damageDef = weapon.Verbs[0].defaultProjectile.projectile.damageDef;

                            newPPCE.dropsCasings = true;
                            newPPCE.explosionDamageFalloff = true;
                            newPPCE.armorPenetrationSharp = weapon.Verbs[0].defaultProjectile.projectile.GetArmorPenetration(1) * APCESettings.gunSharpPenMult;
                            newPPCE.armorPenetrationBlunt = weapon.Verbs[0].defaultProjectile.projectile.GetArmorPenetration(1) * APCESettings.gunBluntPenMult / 4f;
                            newPPCE.flyOverhead = false;
                            switch (i)
                            {
                                case 0:
                                    {//incendiary
                                        newProjectile.defName = ("APCE_Incendiary_Bullet_" + weapon.defName);
                                        newProjectile.label = (weapon.label + " incendiary bullet");
                                        newAmmos[i] = APCEDefOf.Ammo_APCELauncher_Incendiary;
                                        damageHolder = (int)(damageHolder * 0.66f + 0.5f);
                                        newPPCE.damageDef = APCEDefOf.PrometheumFlame;
                                        newPPCE.explosionRadius = weapon.Verbs[0].defaultProjectile.projectile.explosionRadius * 4f;
                                        newPPCE.speed = 40;
                                        newPPCE.preExplosionSpawnChance = 0.2f;
                                        newPPCE.preExplosionSpawnThingDef = APCEDefOf.FilthPrometheum;
                                        newPPCE.ai_IsIncendiary = true;
                                        break;
                                    }
                                case 1:
                                    {//thermobaric
                                        newProjectile.defName = ("APCE_Thermobaric_Bullet_" + weapon.defName);
                                        newProjectile.label = (weapon.label + " thermobaric bullet");
                                        newAmmos[i] = APCEDefOf.Ammo_APCELauncher_Thermobaric;
                                        damageHolder = (int)(damageHolder * 5f + 0.5f);
                                        newPPCE.damageDef = APCEDefOf.Thermobaric;
                                        newPPCE.explosionRadius = weapon.Verbs[0].defaultProjectile.projectile.explosionRadius * 2f;
                                        newPPCE.speed = 40;
                                        newPPCE.ai_IsIncendiary = true;
                                        newPPCE.applyDamageToExplosionCellsNeighbors = true;
                                        newPPCE.soundExplode = APCEDefOf.MortarBomb_Explode;
                                        break;
                                    }
                                case 2:
                                    {//foam
                                        newProjectile.defName = ("APCE_Foam_Bullet_" + weapon.defName);
                                        newProjectile.label = (weapon.label + " foam bullet");
                                        newAmmos[i] = APCEDefOf.Ammo_APCELauncher_Foam;
                                        //damageHolder = 0f; //TODO???
                                        newPPCE.damageDef = DamageDefOf.Extinguish;
                                        newPPCE.explosionRadius = weapon.Verbs[0].defaultProjectile.projectile.explosionRadius * 3f;
                                        newPPCE.speed = 40;
                                        newPPCE.suppressionFactor = 0f;
                                        newPPCE.dangerFactor = 0f;
                                        newPPCE.preExplosionSpawnChance = 1f;
                                        newPPCE.preExplosionSpawnThingDef = APCEDefOf.Filth_FireFoam;
                                        break;
                                    }
                                default:
                                    {
                                        break;
                                    }
                            }
                            SetDamage(newPPCE, damageHolder);
                            newPPCE.secondaryDamage.AddRange(ExtraToSecondary(weapon.Verbs[0].defaultProjectile.projectile.extraDamages));
                            newProjectile.projectile = newPPCE;
                            newProjectiles.Add(newProjectile);
                            DefGenerator.AddImpliedDef<ThingDef>(newProjectile);
                        }
                        break;
                }
                    /*
            case APCESettings.gunKinds.Grenade;
                {//TODO
                    break;
                }
            */
                default:
                    {
                        if ((weapon.techLevel - TechLevel.Spacer) >= 0)
                        {
                            newAmmoCat.parent = APCEDefOf.AmmoAdvanced;
                            newAmmoSet.similarTo = APCEDefOf.AmmoSet_ChargedRifle;

                            for (int i = 0; i < 3; i++)
                            {
                                ThingDef newProjectile = new ThingDef();
                                newProjectile.graphicData = new GraphicData();
                                ProjectilePropertiesCE newPPCE = new ProjectilePropertiesCE();
                                newPPCE.secondaryDamage = new List<SecondaryDamage>();

                                int damageHolder = weapon.Verbs[0].defaultProjectile.projectile.GetDamageAmount(1);
                                PatchBaseBullet(newProjectile);
                                newProjectile.graphicData.texPath = weapon.Verbs[0].defaultProjectile.graphicData.texPath;
                                //newProjectile.graphicData.texPath = "Things/Projectile/Bullet_Small";
                                newProjectile.graphicData.graphicClass = weapon.Verbs[0].defaultProjectile.graphicData.graphicClass;
                                //newProjectile.graphicData.graphicClass = typeof(Graphic_Single);
                                newPPCE.damageDef = weapon.Verbs[0].defaultProjectile.projectile.damageDef;
                                //newPPCE.damageDef = DamageDefOf.Bullet;

                                newPPCE.speed = 160;
                                newPPCE.dropsCasings = true;
                                newPPCE.explosionDamageFalloff = true;
                                newPPCE.armorPenetrationSharp = weapon.Verbs[0].defaultProjectile.projectile.GetArmorPenetration(1) * APCESettings.gunSharpPenMult;
                                newPPCE.armorPenetrationBlunt = weapon.Verbs[0].defaultProjectile.projectile.GetArmorPenetration(1) * APCESettings.gunBluntPenMult;
                                newPPCE.flyOverhead = false;

                                switch(i)
                                {
                                    case 0:
                                        {//Charged
                                            newProjectile.defName = ("APCE_Charged_Bullet_" + weapon.defName);
                                            newProjectile.label = (weapon.label + " charged bullet");
                                            newAmmos[i] = APCEDefOf.Ammo_APCEGeneric_Charged;
                                            break;
                                        }
                                    case 1:
                                        {//Conc
                                            newProjectile.defName = ("APCE_Conc_Bullet_" + weapon.defName);
                                            newProjectile.label = (weapon.label + " conc. charged bullet");
                                            damageHolder = (int)(damageHolder * 0.75f + 0.5f);
                                            newPPCE.armorPenetrationSharp *= 2;
                                            newAmmos[i] = APCEDefOf.Ammo_APCEGeneric_ChargedAP;
                                            break;

                                        }
                                    case 2:
                                        {//Ion
                                            newProjectile.defName = ("APCE_Ion_Bullet_" + weapon.defName);
                                            newProjectile.label = (weapon.label + " ion charged bullet");
                                            newAmmos[i] = APCEDefOf.Ammo_APCEGeneric_ChargedIon;
                                            damageHolder = (int)(damageHolder * 0.75f + 0.5f);
                                            newPPCE.armorPenetrationSharp *= 1.5f;
                                            SecondaryDamage newSD2 = new SecondaryDamage();
                                            newSD2.def = DamageDefOf.EMP;
                                            newSD2.amount = (int)(damageHolder * 0.5f + 0.5f);
                                            newSD2.chance = 1;
                                            newPPCE.empShieldBreakChance = 0.2f;
                                            newPPCE.secondaryDamage.Add(newSD2);
                                            break;
                                        }
                                    default:
                                        {
                                            break;
                                        }
                                }
                                SetDamage(newPPCE, damageHolder);
                                newPPCE.secondaryDamage.AddRange(ExtraToSecondary(weapon.Verbs[0].defaultProjectile.projectile.extraDamages));
                                newProjectile.projectile = newPPCE;
                                newProjectiles.Add(newProjectile);
                                DefGenerator.AddImpliedDef<ThingDef>(newProjectile);
                            }

                        }
                        else
                        {
                            newAmmoCat.parent = APCEDefOf.Ammo;
                            newAmmoSet.similarTo = APCEDefOf.AmmoSet_RifleIntermediate;

                            for (int i = 0; i < 6; i++)
                            {//make the projectiles
                                ThingDef newProjectile = new ThingDef();
                                newProjectile.graphicData = new GraphicData();
                                ProjectilePropertiesCE newPPCE = new ProjectilePropertiesCE();
                                newPPCE.secondaryDamage = new List<SecondaryDamage>();

                                int damageHolder = weapon.Verbs[0].defaultProjectile.projectile.GetDamageAmount(1);
                                PatchBaseBullet(newProjectile);
                                newProjectile.graphicData.texPath = weapon.Verbs[0].defaultProjectile.graphicData.texPath;
                                //newProjectile.graphicData.texPath = "Things/Projectile/Bullet_Small";
                                newProjectile.graphicData.graphicClass = weapon.Verbs[0].defaultProjectile.graphicData.graphicClass;
                                //newProjectile.graphicData.graphicClass = typeof(Graphic_Single);
                                newPPCE.damageDef = weapon.Verbs[0].defaultProjectile.projectile.damageDef;
                                //newPPCE.damageDef = DamageDefOf.Bullet;
                                newPPCE.speed = 185;
                                newPPCE.dropsCasings = true;
                                newPPCE.explosionDamageFalloff = true;
                                newPPCE.armorPenetrationSharp = weapon.Verbs[0].defaultProjectile.projectile.GetArmorPenetration(1) * APCESettings.gunSharpPenMult;
                                newPPCE.armorPenetrationBlunt = weapon.Verbs[0].defaultProjectile.projectile.GetArmorPenetration(1) * APCESettings.gunBluntPenMult;
                                newPPCE.flyOverhead = false;
                                switch (i)
                                {
                                    case 0:
                                        {//FMJ
                                            newProjectile.defName = ("APCE_FMJ_Bullet_" + weapon.defName);
                                            newProjectile.label = (weapon.label + " FMJ bullet");
                                            newAmmos[i] = APCEDefOf.Ammo_APCEGeneric_FMJ;
                                            break;
                                        }
                                    case 1:
                                        {//AP
                                            newProjectile.defName = ("APCE_AP_Bullet_" + weapon.defName);
                                            newProjectile.label = (weapon.label + " AP bullet");
                                            damageHolder = (int)(damageHolder * 0.66f + 0.5f);
                                            newPPCE.armorPenetrationSharp *= 2;
                                            newAmmos[i] = APCEDefOf.Ammo_APCEGeneric_AP;
                                            break;
                                        }
                                    case 2:
                                        {//HP
                                            newProjectile.defName = ("APCE_HP_Bullet_" + weapon.defName);
                                            newProjectile.label = (weapon.label + " HP bullet");
                                            damageHolder = (int)(damageHolder * 1.33f + 0.5f);
                                            newPPCE.armorPenetrationSharp *= 0.5f;
                                            newAmmos[i] = APCEDefOf.Ammo_APCEGeneric_HP;
                                            break;
                                        }
                                    case 3:
                                        {//API
                                            newProjectile.defName = ("APCE_API_Bullet_" + weapon.defName);
                                            newProjectile.label = (weapon.label + " AP-I bullet");
                                            damageHolder = (int)(damageHolder * 0.66f + 0.5f);
                                            SecondaryDamage newSD2 = new SecondaryDamage();
                                            newSD2.def = DamageDefOf.Burn;
                                            newSD2.amount = (int)(damageHolder * 0.33f + 0.5f);
                                            newSD2.chance = 1;
                                            newPPCE.secondaryDamage.Add(newSD2);
                                            newPPCE.armorPenetrationSharp *= 2;
                                            newAmmos[i] = APCEDefOf.Ammo_APCEGeneric_Incendiary;
                                            break;
                                        }
                                    case 4:
                                        {//HE
                                            newProjectile.defName = ("APCE_HE_Bullet_" + weapon.defName);
                                            newProjectile.label = (weapon.label + " HE bullet");
                                            SecondaryDamage newSD2 = new SecondaryDamage();
                                            newSD2.def = DamageDefOf.Bomb;
                                            newSD2.amount = (int)(damageHolder * 0.66f + 0.5f);
                                            newSD2.chance = 1;
                                            newPPCE.secondaryDamage.Add(newSD2);
                                            newAmmos[i] = APCEDefOf.Ammo_APCEGeneric_HE;
                                            break;
                                        }
                                    case 5:
                                        {//Sabot
                                            newProjectile.defName = ("APCE_Sabot_Bullet_" + weapon.defName);
                                            newProjectile.label = (weapon.label + " Sabot bullet");
                                            damageHolder = (int)(damageHolder * 0.5f + 0.5f);
                                            newPPCE.armorPenetrationSharp *= 3.5f;
                                            newPPCE.armorPenetrationBlunt *= 1.3f;
                                            newPPCE.speed *= 1.5f;
                                            newAmmos[i] = APCEDefOf.Ammo_APCEGeneric_Sabot;
                                            break;
                                        }
                                    default:
                                        {
                                            break;
                                        }
                                }
                                SetDamage(newPPCE, damageHolder);
                                newPPCE.secondaryDamage.AddRange(ExtraToSecondary(weapon.Verbs[0].defaultProjectile.projectile.extraDamages));
                                newProjectile.projectile = newPPCE;
                                newProjectiles.Add(newProjectile);
                                DefGenerator.AddImpliedDef<ThingDef>(newProjectile);
                            }
                        }
                        break;
                    }

            }

            for (int i = 0; i < newProjectiles.Count; i++)
            {
                newAmmos[i].projectile = newProjectiles[i].projectile;
                AmmoLink al = new AmmoLink(newAmmos[i], newProjectiles[i]);
                newAmmoLinks.Add(al);
                newAmmoCat.childThingDefs.Add(newAmmos[i]);
            }

            newAmmoCat.treeNode = new TreeNode_ThingCategory(newAmmoCat);
            DefGenerator.AddImpliedDef<ThingCategoryDef>(newAmmoCat);

            newAmmoSet.ammoTypes = newAmmoLinks;
            DefGenerator.AddImpliedDef<AmmoSetDef>(newAmmoSet);
            return newAmmoSet;
        }
    }
}