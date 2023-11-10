using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using CombatExtended;

namespace nuff.AutoPatcherCombatExtended
{
    public class DefDataHolderAmmoSet : DefDataHolder
    {
        public DefDataHolderAmmoSet(ThingDef def) : base(def)
        {
        }

        ThingDef weaponDef;
        APCEConstants.gunKinds gunKind;
        ThingDef original_projectile;
        int original_damage;
        float original_armorPenetration;
        float original_speed;
        float original_explosionRadius;
        bool original_ai_IsIncendiary;
        bool original_applyDamageToExplosionCellsNeighbors;
        float original_techMult;
        DamageDef original_damageDef;
        List<ExtraDamage> original_extraDamages;

        //intermediate data
        float armorPenSharpModded;
        float armorPenBluntModded;

        //ThingCategoryDef and its serializable data
        ThingCategoryDef modified_ammoCat;
        string modified_categoryDefName;
        ThingCategoryDef modified_catParent;
        string modified_catParentName;

        //ammosetdef and its serializable data
        AmmoSetDef modified_ammoSetDef;
        List<AmmoLink> modified_ammoLinks = new List<AmmoLink>();
        string modified_ammoSetDefName;
        string modified_ammoSetLabel;
        string modified_ammoSetDescription;

        // List of ammos and their serializable data
        List<AmmoDef> modified_ammoDefs = new List<AmmoDef>();

        // List of projectiles and their serializable data
        //ThingDef stuff
        List<ThingDef> modified_projectiles = new List<ThingDef>();
        List<string> modified_projectileNames = new List<string>();
        List<string> modified_projectileLabels = new List<string>();
        List<APCEConstants.ThingClasses> modified_thingClasses = new List<APCEConstants.ThingClasses>();
        //List<Type> modified_thingClasses = new List<Type>(); //TODO cache references to usable thingClasses as startup?

        //projectile stuff
        List<DamageDef> modified_damageDefs = new List<DamageDef>();
        List<string> modified_damageDefNames = new List<string>(); //used for saving/loading
        List<int> modified_damages = new List<int>();
        List<float> modified_armorPenetrationSharps = new List<float>();
        List<float> modified_armorPenetrationBlunts = new List<float>();
        List<float> modified_speeds = new List<float>();
        List<float> modified_explosionRadii = new List<float>();
        List<int> modified_pelletCounts = new List<int>();
        List<float> modified_spreadMults = new List<float>();
        List<float> modified_empShieldBreakChance = new List<float>();
        List<float> modified_suppressionFactors = new List<float>();
        List<float> modified_dangerFactors = new List<float>();
        List<bool> modified_ai_IsIncendiary = new List<bool>();
        List<bool> modified_applyDamageToExplosionCellsNeighbors = new List<bool>();

        List<List<SecondaryDamage>> modified_secondaryDamages = new List<List<SecondaryDamage>>();
        List<List<DamageDef>> modified_secondaryDamageDefs = new List<List<DamageDef>>();
        List<List<string>> modified_secondaryDamageDefStrings = new List<List<string>>(); //used for saving/loading
        List<List<int>> modified_secondaryDamageAmounts = new List<List<int>>();
        List<List<float>> modified_secondaryDamageChances = new List<List<float>>();

        //casings / motes and filth
        //TODO

        //preExposionSpawn
        //TODO

        //list of recipes and their serializable data
        //TODO

        //compprops_fragments
        //TODO

        //sounds?
        //TODO

        string modified_defName;

        public override void GetOriginalData()
        {
            weaponDef = def as ThingDef;
            gunKind = DataHolderUtils.DetermineGunKind(weaponDef);
            original_projectile = weaponDef.Verbs[0].defaultProjectile;
            original_damage = original_projectile.projectile.GetDamageAmount(1);
            original_armorPenetration = original_projectile.projectile.GetArmorPenetration(1);
            original_speed = original_projectile.projectile.speed;
            original_explosionRadius = original_projectile.projectile.explosionRadius;
            original_ai_IsIncendiary = original_projectile.projectile.ai_IsIncendiary;
            original_applyDamageToExplosionCellsNeighbors = original_projectile.projectile.applyDamageToExplosionCellsNeighbors;
            original_damageDef = original_projectile.projectile.damageDef;
            original_extraDamages = original_projectile.projectile.extraDamages;

            CalculateWeaponTechMult();
        }

        public override void AutoCalculate()
        {
            modified_ammoSetDefName = "APCEAmmoSet_" + weaponDef.defName;
            modified_ammoSetLabel = "Ammo set for " + weaponDef.label;
            modified_ammoSetDescription = "A procedurally generated ammo set for the " + weaponDef.label;

            armorPenSharpModded = original_armorPenetration * modData.gunSharpPenMult * original_techMult;
            armorPenBluntModded = original_armorPenetration * modData.gunBluntPenMult * original_techMult;

            if (original_damageDef == DamageDefOf.Bomb && original_damage == 635)
            {//since CE changes the default damage of Bomb from 50 to 635, projectiles relying on the default value will do unintended levels of damage
                original_damage = 50;
            }

            switch (gunKind)
            {
                case APCEConstants.gunKinds.Bow:
                    {
                        GenerateAmmoBow();
                        break;
                    }
                case APCEConstants.gunKinds.Shotgun:
                    {
                        GenerateAmmoShotgun();
                        break;
                    }
                case APCEConstants.gunKinds.ExplosiveLauncher:
                    {
                        GenerateAmmoExplosiveLauncher();
                        break;
                    }
                case APCEConstants.gunKinds.Turret:
                    {
                        if ((weaponDef.Verbs[0].defaultProjectile.GetCompProperties<CompProperties_ExplosiveCE>() != null)
                            || (weaponDef.Verbs[0].defaultProjectile.GetCompProperties<CompProperties_Explosive>() != null)
                            || (weaponDef.Verbs[0].defaultProjectile.thingClass == typeof(Projectile_Explosive)))
                        {
                            GenerateAmmoExplosiveLauncher();
                        }
                        else
                        {
                            GenerateAmmoIndustrial();
                        }
                        break;
                    }
                default:
                    {
                        if ((weaponDef.techLevel == TechLevel.Spacer) || (weaponDef.techLevel == TechLevel.Ultra) || (weaponDef.techLevel == TechLevel.Archotech))
                        {
                            GenerateAmmoSpacer();

                        }
                        else
                        {
                            GenerateAmmoIndustrial();
                        }
                        break;
                    }
            }


        }

        public void GenerateAmmoBow()
        {
            for (int i = 0; i < 5; i++)
            {
                List<DamageDef> secondaryDamageDefs = new List<DamageDef>();
                List<int> secondaryDamageAmounts = new List<int>();
                List<float> secondaryDamageChances = new List<float>();
                ExtraDamageToSecondary(ref secondaryDamageDefs, ref secondaryDamageAmounts, ref secondaryDamageChances);

                modified_pelletCounts.Add(1);
                modified_spreadMults.Add(1);
                modified_empShieldBreakChance.Add(1);
                modified_explosionRadii.Add(0);
                modified_suppressionFactors.Add(1);
                modified_dangerFactors.Add(1);
                modified_applyDamageToExplosionCellsNeighbors.Add(false);

                switch (i)
                {
                    case 0:
                        //stone
                        modified_projectileNames.Add("APCE_Stone_Arrow_" + weaponDef.defName);
                        modified_projectileLabels.Add(weaponDef.label + " stone arrow");
                        modified_thingClasses.Add(APCEConstants.ThingClasses.BulletCE);
                        modified_speeds.Add(31);
                        modified_damages.Add(original_damage * 1);
                        modified_damageDefs.Add(DamageDefOf.Bullet);
                        modified_armorPenetrationSharps.Add(armorPenSharpModded * 0.4f);
                        modified_armorPenetrationBlunts.Add(armorPenBluntModded * 0.33f);
                        modified_ai_IsIncendiary.Add(false);
                        break;
                    case 1:
                        //steel
                        modified_projectileNames.Add("APCE_Steel_Arrow_" + weaponDef.defName);
                        modified_projectileLabels.Add(weaponDef.label + " steel arrow");
                        modified_thingClasses.Add(APCEConstants.ThingClasses.BulletCE);
                        modified_speeds.Add(36);
                        modified_damages.Add((int)(original_damage * 1.25f + 0.5f));
                        modified_damageDefs.Add(DamageDefOf.Bullet);
                        modified_armorPenetrationSharps.Add(armorPenSharpModded * 1f);
                        modified_armorPenetrationBlunts.Add(armorPenBluntModded * 1f);
                        modified_ai_IsIncendiary.Add(false);
                        break;
                    case 2:
                        //plasteel
                        modified_projectileNames.Add("APCE_Plasteel_Arrow_" + weaponDef.defName);
                        modified_projectileLabels.Add(weaponDef.label + " plasteel arrow");
                        modified_thingClasses.Add(APCEConstants.ThingClasses.BulletCE);
                        modified_speeds.Add(39);
                        modified_damages.Add(original_damage * 1);
                        modified_damageDefs.Add(DamageDefOf.Bullet);
                        modified_armorPenetrationSharps.Add(armorPenSharpModded * 2);
                        modified_armorPenetrationBlunts.Add(armorPenBluntModded * 0.5f);
                        modified_ai_IsIncendiary.Add(false);
                        break;
                    case 3:
                        //venom
                        modified_projectileNames.Add("APCE_Venom_Arrow_" + weaponDef.defName);
                        modified_projectileLabels.Add(weaponDef.label + " venom arrow");
                        modified_thingClasses.Add(APCEConstants.ThingClasses.BulletCE);
                        modified_speeds.Add(36);
                        modified_damages.Add((int)(original_damage * 1.25f + 0.5f));
                        modified_damageDefs.Add(DamageDefOf.Bullet);
                        modified_armorPenetrationSharps.Add(armorPenSharpModded * 1f);
                        modified_armorPenetrationBlunts.Add(armorPenBluntModded * 1f);
                        secondaryDamageDefs.Add(APCEDefOf.ArrowVenom);
                        secondaryDamageAmounts.Add((int)(original_damage * 1.25f + 0.5f));
                        secondaryDamageChances.Add(1);
                        modified_ai_IsIncendiary.Add(false);
                        break;
                    case 4:
                        //flame
                        modified_projectileNames.Add("APCE_Flame_Arrow_" + weaponDef.defName);
                        modified_projectileLabels.Add(weaponDef.label + " flame arrow");
                        modified_thingClasses.Add(APCEConstants.ThingClasses.ProjectileCE_Explosive);
                        modified_speeds.Add(36);
                        modified_damages.Add((int)(original_damage * 0.25f + 0.5f));
                        modified_damageDefs.Add(APCEDefOf.ArrowFire);
                        modified_armorPenetrationSharps.Add(0f);
                        modified_armorPenetrationBlunts.Add(0f);
                        modified_ai_IsIncendiary.Add(true);
                        break;
                }

                modified_secondaryDamageDefs.Add(secondaryDamageDefs);
                modified_secondaryDamageAmounts.Add(secondaryDamageAmounts);
                modified_secondaryDamageChances.Add(secondaryDamageChances);
            }
        }

        public void GenerateAmmoShotgun()
        {
            for (int i = 0; i < 4; i++)
            {
                List<DamageDef> secondaryDamageDefs = new List<DamageDef>();
                List<int> secondaryDamageAmounts = new List<int>();
                List<float> secondaryDamageChances = new List<float>();
                ExtraDamageToSecondary(ref secondaryDamageDefs, ref secondaryDamageAmounts, ref secondaryDamageChances);

                modified_thingClasses.Add(APCEConstants.ThingClasses.BulletCE);
                modified_explosionRadii.Add(0);
                modified_suppressionFactors.Add(1);
                modified_dangerFactors.Add(1);
                modified_ai_IsIncendiary.Add(false);
                modified_applyDamageToExplosionCellsNeighbors.Add(false);

                switch (i)
                {
                    case 0:
                        //buck
                        modified_projectileNames.Add("APCE_Buckshot_Bullet_" + weaponDef.defName);
                        modified_projectileLabels.Add(weaponDef.label + " buckshot bullet");
                        modified_speeds.Add(83);
                        modified_damages.Add((int)(original_damage * 0.5f + 0.5f));
                        modified_damageDefs.Add(original_damageDef);
                        modified_armorPenetrationSharps.Add(armorPenSharpModded);
                        modified_armorPenetrationBlunts.Add(armorPenBluntModded * 0.25f);
                        modified_pelletCounts.Add(9);
                        modified_spreadMults.Add(8.9f);
                        modified_empShieldBreakChance.Add(1);
                        break;

                    case 1:
                        //slug
                        modified_projectileNames.Add("APCE_Slug_Bullet_" + weaponDef.defName);
                        modified_projectileLabels.Add(weaponDef.label + " slug bullet");
                        modified_speeds.Add(114);
                        modified_damages.Add((int)(original_damage * 3f + 0.5f));
                        modified_damageDefs.Add(original_damageDef);
                        modified_armorPenetrationSharps.Add(armorPenSharpModded * 1.5f);
                        modified_armorPenetrationBlunts.Add(armorPenBluntModded * 5f);
                        modified_pelletCounts.Add(1);
                        modified_spreadMults.Add(1);
                        modified_empShieldBreakChance.Add(1);
                        break;

                    case 2:
                        //beanbag
                        modified_projectileNames.Add("APCE_Beanbag_Bullet_" + weaponDef.defName);
                        modified_projectileLabels.Add(weaponDef.label + " beanbag bullet");
                        modified_speeds.Add(30);
                        modified_damages.Add((int)(original_damage * 0.5f + 0.5f));
                        modified_damageDefs.Add(APCEDefOf.Beanbag);
                        modified_armorPenetrationSharps.Add(0f);
                        modified_armorPenetrationBlunts.Add(armorPenBluntModded * 0.2f);
                        modified_pelletCounts.Add(1);
                        modified_spreadMults.Add(2);
                        modified_empShieldBreakChance.Add(1);

                        break;

                    case 3:
                        //electroslug
                        modified_projectileNames.Add("APCE_Electroshot_Bullet_" + weaponDef.defName);
                        modified_projectileLabels.Add(weaponDef.label + " electroslug bullet");
                        modified_speeds.Add(43);
                        modified_damages.Add((int)(original_damage * 1.25f + 0.5f));
                        modified_damageDefs.Add(DamageDefOf.EMP);
                        modified_armorPenetrationSharps.Add(0);
                        modified_armorPenetrationBlunts.Add(0);
                        modified_pelletCounts.Add(1);
                        modified_spreadMults.Add(1);
                        modified_empShieldBreakChance.Add(0.2f);
                        break;
                }

                modified_secondaryDamageDefs.Add(secondaryDamageDefs);
                modified_secondaryDamageAmounts.Add(secondaryDamageAmounts);
                modified_secondaryDamageChances.Add(secondaryDamageChances);
            }
        }

        public void GenerateAmmoExplosiveLauncher()
        {
            for (int i = 0; i < 4; i++)
            {
                List<DamageDef> secondaryDamageDefs = new List<DamageDef>();
                List<int> secondaryDamageAmounts = new List<int>();
                List<float> secondaryDamageChances = new List<float>();
                ExtraDamageToSecondary(ref secondaryDamageDefs, ref secondaryDamageAmounts, ref secondaryDamageChances);

                modified_thingClasses.Add(APCEConstants.ThingClasses.ProjectileCE_Explosive);
                modified_speeds.Add(40);
                modified_pelletCounts.Add(1);
                modified_spreadMults.Add(1);
                modified_empShieldBreakChance.Add(1);

                switch (i)
                {
                    case 0:
                        //HE
                        modified_projectileNames.Add("APCE_HE_Bullet_" + weaponDef.defName);
                        modified_projectileLabels.Add(weaponDef.label + " HE bullet");
                        modified_damages.Add((int)(original_damage * 0.5f + 0.5f));
                        modified_armorPenetrationBlunts.Add(0);
                        modified_armorPenetrationSharps.Add(0);
                        modified_damageDefs.Add(original_damageDef);
                        modified_explosionRadii.Add(original_explosionRadius);
                        modified_suppressionFactors.Add(1);
                        modified_dangerFactors.Add(1);
                        modified_ai_IsIncendiary.Add(true);
                        modified_applyDamageToExplosionCellsNeighbors.Add(true);
                        break;
                    case 1:
                        //HEDP
                        modified_projectileNames.Add("APCE_HEDP_Bullet_" + weaponDef.defName);
                        modified_projectileLabels.Add(weaponDef.label + " HEDP bullet");
                        modified_damages.Add((int)(original_damage * 0.75f + 0.5f));
                        modified_armorPenetrationBlunts.Add(5.942f);
                        modified_armorPenetrationSharps.Add(63f);
                        modified_damageDefs.Add(original_damageDef);
                        modified_explosionRadii.Add(original_explosionRadius * 0.5f);
                        modified_suppressionFactors.Add(1);
                        modified_dangerFactors.Add(1);
                        modified_ai_IsIncendiary.Add(true);
                        modified_applyDamageToExplosionCellsNeighbors.Add(true);
                        break;
                    case 2:
                        //EMP
                        modified_projectileNames.Add("APCE_EMP_Bullet_" + weaponDef.defName);
                        modified_projectileLabels.Add(weaponDef.label + " EMP bullet");
                        modified_damages.Add((int)(original_damage * 0.5f + 0.5f));
                        modified_armorPenetrationBlunts.Add(0);
                        modified_armorPenetrationSharps.Add(0);
                        modified_damageDefs.Add(DamageDefOf.EMP);
                        modified_explosionRadii.Add(original_explosionRadius * 1.5f);
                        modified_suppressionFactors.Add(1);
                        modified_dangerFactors.Add(1);
                        modified_ai_IsIncendiary.Add(true);
                        modified_applyDamageToExplosionCellsNeighbors.Add(false);
                        break;
                    case 3:
                        //Smoke
                        modified_projectileNames.Add("APCE_Smoke_Bullet_" + weaponDef.defName);
                        modified_projectileLabels.Add(weaponDef.label + " smoke bullet");
                        modified_damages.Add((int)(0));
                        modified_armorPenetrationBlunts.Add(0);
                        modified_armorPenetrationSharps.Add(0);
                        modified_damageDefs.Add(DamageDefOf.Smoke);
                        modified_explosionRadii.Add(original_explosionRadius * 2f);
                        modified_suppressionFactors.Add(0);
                        modified_dangerFactors.Add(0);
                        modified_ai_IsIncendiary.Add(false);
                        modified_applyDamageToExplosionCellsNeighbors.Add(false);
                        break;
                    /*
                    case 0:
                        //incendiary
                        modified_projectileNames.Add("APCE_Incendiary_Bullet_" + weaponDef.defName);
                        modified_projectileLabels.Add(weaponDef.label + " incendiary bullet");
                        modified_damages.Add((int)(original_damage * 0.66f + 0.5f));
                        modified_damageDefs.Add(APCEDefOf.PrometheumFlame);
                        modified_explosionRadii.Add(original_explosionRadius * 4f);
                        modified_suppressionFactors.Add(1);
                        modified_dangerFactors.Add(1);
                        modified_ai_IsIncendiary.Add(true);
                        modified_applyDamageToExplosionCellsNeighbors.Add(false);
                        break;
                        
                    case 1:
                        //thermobaric
                        modified_projectileNames.Add("APCE_Thermobaric_Bullet_" + weaponDef.defName);
                        modified_projectileLabels.Add(weaponDef.label + " thermobaric bullet");
                        modified_damages.Add((int)(original_damage * 5f + 0.5f));
                        modified_damageDefs.Add(APCEDefOf.Thermobaric);
                        modified_explosionRadii.Add(original_explosionRadius * 2f);
                        modified_suppressionFactors.Add(1);
                        modified_dangerFactors.Add(1);
                        modified_ai_IsIncendiary.Add(true);
                        modified_applyDamageToExplosionCellsNeighbors.Add(true);
                        break;
                        
                    case 2:
                        //foam
                        modified_projectileNames.Add("APCE_Foam_Bullet_" + weaponDef.defName);
                        modified_projectileLabels.Add(weaponDef.label + " foam bullet");
                        modified_damages.Add(99999);
                        modified_damageDefs.Add(DamageDefOf.Extinguish);
                        modified_explosionRadii.Add(original_explosionRadius * 3f);
                        modified_suppressionFactors.Add(0);
                        modified_dangerFactors.Add(0);
                        modified_ai_IsIncendiary.Add(false);
                        modified_applyDamageToExplosionCellsNeighbors.Add(false);
                        break;
                    */
                }

                modified_secondaryDamageDefs.Add(secondaryDamageDefs);
                modified_secondaryDamageAmounts.Add(secondaryDamageAmounts);
                modified_secondaryDamageChances.Add(secondaryDamageChances);
            }
        }

        public void GenerateAmmoIndustrial()
        {
            for (int i = 0; i < 6; i++)
            {
                List<DamageDef> secondaryDamageDefs = new List<DamageDef>();
                List<int> secondaryDamageAmounts = new List<int>();
                List<float> secondaryDamageChances = new List<float>();
                ExtraDamageToSecondary(ref secondaryDamageDefs, ref secondaryDamageAmounts, ref secondaryDamageChances);

                modified_thingClasses.Add(APCEConstants.ThingClasses.BulletCE);
                modified_damageDefs.Add(original_damageDef);
                modified_explosionRadii.Add(0);
                modified_pelletCounts.Add(1);
                modified_spreadMults.Add(1);
                modified_empShieldBreakChance.Add(1);
                modified_suppressionFactors.Add(1);
                modified_dangerFactors.Add(1);
                modified_ai_IsIncendiary.Add(false);
                modified_applyDamageToExplosionCellsNeighbors.Add(false);

                switch (i)
                {
                    case 0:
                        //FMJ
                        modified_projectileNames.Add("APCE_FMJ_Bullet_" + weaponDef.defName);
                        modified_projectileLabels.Add(weaponDef.label + " FMJ bullet");
                        modified_damages.Add(original_damage);
                        modified_armorPenetrationSharps.Add(armorPenSharpModded);
                        modified_armorPenetrationSharps.Add(armorPenBluntModded);
                        modified_speeds.Add(168);
                        break;
                    case 1:
                        //AP
                        modified_projectileNames.Add("APCE_AP_Bullet_" + weaponDef.defName);
                        modified_projectileLabels.Add(weaponDef.label + " AP bullet");
                        modified_damages.Add((int)(original_damage * 0.66f + 0.5f));
                        modified_armorPenetrationSharps.Add(armorPenSharpModded * 2f);
                        modified_armorPenetrationSharps.Add(armorPenBluntModded);
                        modified_speeds.Add(168);
                        break;
                    case 2:
                        //HP
                        modified_projectileNames.Add("APCE_HP_Bullet_" + weaponDef.defName);
                        modified_projectileLabels.Add(weaponDef.label + " HP bullet");
                        modified_damages.Add((int)(original_damage * 1.33f + 0.5f));
                        modified_armorPenetrationSharps.Add(armorPenSharpModded * 0.5f);
                        modified_armorPenetrationSharps.Add(armorPenBluntModded);
                        modified_speeds.Add(168);
                        break;
                    case 3:
                        //AP-I
                        modified_projectileNames.Add("APCE_API_Bullet_" + weaponDef.defName);
                        modified_projectileLabels.Add(weaponDef.label + " AP-I bullet");
                        modified_damages.Add((int)(original_damage * 0.66f + 0.5f));
                        modified_armorPenetrationSharps.Add(armorPenSharpModded * 2f);
                        modified_armorPenetrationSharps.Add(armorPenBluntModded);
                        modified_speeds.Add(168);
                        secondaryDamageDefs.Add(CE_DamageDefOf.Flame_Secondary);
                        secondaryDamageAmounts.Add((int)(original_damage * 0.33f + 0.5f));
                        secondaryDamageChances.Add(1);
                        break;
                    case 4:
                        //HE
                        modified_projectileNames.Add("APCE_HE_Bullet_" + weaponDef.defName);
                        modified_projectileLabels.Add(weaponDef.label + " HE bullet");
                        modified_damages.Add(original_damage);
                        modified_armorPenetrationSharps.Add(armorPenSharpModded);
                        modified_armorPenetrationSharps.Add(armorPenBluntModded);
                        modified_speeds.Add(168);
                        secondaryDamageDefs.Add(DamageDefOf.Bomb);
                        secondaryDamageAmounts.Add((int)(original_damage * 0.66f + 0.5f));
                        secondaryDamageChances.Add(1);
                        break;
                    case 5:
                        //Sabot
                        modified_projectileNames.Add("APCE_Sabot_Bullet_" + weaponDef.defName);
                        modified_projectileLabels.Add(weaponDef.label + " sabot bullet");
                        modified_damages.Add((int)(original_damage * 0.5f + 0.5f));
                        modified_armorPenetrationSharps.Add(armorPenSharpModded * 3.5f);
                        modified_armorPenetrationSharps.Add(armorPenBluntModded * 1.5f);
                        modified_speeds.Add(227);
                        break;
                }

                modified_secondaryDamageDefs.Add(secondaryDamageDefs);
                modified_secondaryDamageAmounts.Add(secondaryDamageAmounts);
                modified_secondaryDamageChances.Add(secondaryDamageChances);
            }
        }

        public void GenerateAmmoSpacer()
        {
            for (int i = 0; i < 3; i++)
            {
                List<DamageDef> secondaryDamageDefs = new List<DamageDef>();
                List<int> secondaryDamageAmounts = new List<int>();
                List<float> secondaryDamageChances = new List<float>();
                ExtraDamageToSecondary(ref secondaryDamageDefs, ref secondaryDamageAmounts, ref secondaryDamageChances);

                modified_thingClasses.Add(APCEConstants.ThingClasses.BulletCE);
                modified_damageDefs.Add(original_damageDef);
                modified_speeds.Add(151);
                modified_explosionRadii.Add(0);
                modified_pelletCounts.Add(1);
                modified_spreadMults.Add(1);
                modified_suppressionFactors.Add(1);
                modified_dangerFactors.Add(1);
                modified_ai_IsIncendiary.Add(false);
                modified_applyDamageToExplosionCellsNeighbors.Add(false);

                switch (i)
                {
                    case 0:
                        //charged
                        modified_projectileNames.Add("APCE_Charged_Bullet_" + weaponDef.defName);
                        modified_projectileLabels.Add(weaponDef.label + " sabot bullet");
                        modified_damages.Add((int)(original_damage));
                        modified_armorPenetrationSharps.Add(armorPenSharpModded);
                        modified_armorPenetrationSharps.Add(armorPenBluntModded);
                        modified_empShieldBreakChance.Add(1);
                        break;
                    case 1:
                        //conc
                        modified_projectileNames.Add("APCE_Conc_Bullet_" + weaponDef.defName);
                        modified_projectileLabels.Add(weaponDef.label + " sabot bullet");
                        modified_damages.Add((int)(original_damage * 0.75f + 0.5f));
                        modified_armorPenetrationSharps.Add(armorPenSharpModded * 2f);
                        modified_armorPenetrationSharps.Add(armorPenBluntModded);
                        modified_empShieldBreakChance.Add(1);
                        break;
                    case 2:
                        //ion
                        modified_projectileNames.Add("APCE_Ion_Bullet_" + weaponDef.defName);
                        modified_projectileLabels.Add(weaponDef.label + " sabot bullet");
                        modified_damages.Add((int)(original_damage * 0.75f + 0.5f));
                        modified_armorPenetrationSharps.Add(armorPenSharpModded * 1.5f);
                        modified_armorPenetrationSharps.Add(armorPenBluntModded);
                        modified_empShieldBreakChance.Add(0.2f);
                        secondaryDamageDefs.Add(DamageDefOf.EMP);
                        secondaryDamageAmounts.Add((int)(original_damage * 0.5f + 0.5f));
                        secondaryDamageChances.Add(1);
                        break;
                }

                modified_secondaryDamageDefs.Add(secondaryDamageDefs);
                modified_secondaryDamageAmounts.Add(secondaryDamageAmounts);
                modified_secondaryDamageChances.Add(secondaryDamageChances);
            }
        }

        public void SetBasePenetrations()
        {
            //todo I think I moved this into autocalculate
        }

        public void CalculateWeaponTechMult()
        {
            float techMult = 1f;
            switch (weaponDef.techLevel)
            {
                case TechLevel.Animal:
                    techMult *= modData.gunTechMultAnimal;
                    break;
                case TechLevel.Neolithic:
                    techMult *= modData.gunTechMultNeolithic;
                    break;
                case TechLevel.Medieval:
                    techMult *= modData.gunTechMultMedieval;
                    break;
                case TechLevel.Industrial:
                    techMult *= modData.gunTechMultIndustrial;
                    break;
                case TechLevel.Spacer:
                    techMult *= modData.gunTechMultSpacer;
                    break;
                case TechLevel.Ultra:
                    techMult *= modData.gunTechMultUltratech;
                    break;
                case TechLevel.Archotech:
                    techMult *= modData.gunTechMultArchotech;
                    break;
                default:
                    break;
            }
            original_techMult = techMult;
        }

        public void ExtraDamageToSecondary(ref List<DamageDef> secondaryDamageDefs, ref List<int> secondaryDamageAmounts, ref List<float> secondaryDamageChances)
        {

            //TODO should I flip the check and do an early return instead?
            if (!original_extraDamages.NullOrEmpty())
            {
                for (int i = 0; i < original_extraDamages.Count; i++)
                {
                    secondaryDamageDefs.Add(original_extraDamages[i].def);
                    secondaryDamageAmounts.Add((int)(original_extraDamages[i].amount + 0.5f)); //extra 0.5 for rounding purposes
                    secondaryDamageChances.Add(original_extraDamages[i].chance);
                }
            }

            modified_secondaryDamageDefs.Add(secondaryDamageDefs);
            modified_secondaryDamageAmounts.Add(secondaryDamageAmounts);
            modified_secondaryDamageChances.Add(secondaryDamageChances);

            return;
        }

        public override void Patch()
        {
            //construct secondaryDamages
            //construct projectiles
            //construct AmmoLinks
            //construct AmmoCat
            //construct AmmoSet

            BuildSecondaryDamages();

            /*public static void PatchBaseBullet(ThingDef bullet)
        {
            bullet.category = ThingCategory.Projectile;
            bullet.tickerType = TickerType.Normal;
            bullet.altitudeLayer = AltitudeLayer.Projectile;
            bullet.useHitPoints = false;
            bullet.neverMultiSelect = true;
            if (bullet.graphicData != null)
            {
                bullet.graphicData.shaderType = ShaderTypeDefOf.Transparent;
                bullet.graphicData.graphicClass = typeof(Graphic_Single);
            }
            
        }*/
        }

        public override StringBuilder PrepExport()
        {
            throw new NotImplementedException();
        }

        public override void ExportXML()
        {
            throw new NotImplementedException();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            //TODO
            //note - unlike other defs, should run Patch when loaded, so that it exists before ranged weapons are patched
        }

        public void BuildSecondaryDamages()
        {
            //make sure things don't get duplicated if player re-patches
            modified_secondaryDamages.Clear();

            for (int i = 0; i < modified_secondaryDamageDefs.Count; i++)
            {
                modified_secondaryDamages.Add(new List<SecondaryDamage>());
                if (modified_secondaryDamageDefs[i].NullOrEmpty())
                {
                    continue;
                }
                for (int j = 0; j < modified_secondaryDamageDefs[i].Count; j++)
                {
                    SecondaryDamage newSecDam = new SecondaryDamage
                    {
                        def = modified_secondaryDamageDefs[i][j],
                        amount = modified_secondaryDamageAmounts[i][j],
                        chance = modified_secondaryDamageChances[i][j]
                    };
                    modified_secondaryDamages[i].Add(newSecDam);
                }
            }
        }
    }
}
