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
        public DefDataHolderAmmoSet()
        {
            //empty constructor for use by SaveLoad
        }

        public DefDataHolderAmmoSet(ThingDef def) : base(def)
        {
        }

        //constructor for use by VehicleTurrets
        public DefDataHolderAmmoSet(ThingDef def, APCEConstants.gunKinds gunKind) : base()
        {
            this.def = def;
            this.gunKind = gunKind;
            parentModPackageId = def.modContentPack.PackageId;
            modData = DataHolderUtils.ReturnModDataOrDefault(def);
            //needs to not register self, so that .Patch() isn't re-run
            //RegisterSelfInDict();
            GetOriginalData();
            AutoCalculate();
            Patch();
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

        //I remembered I don't need ThingCategoryDefs because only generic ammos are assigned. Maybe someday if I add the ability to write new AmmoDefs, I will need these.
        //ThingCategoryDef and its serializable data
        //ThingCategoryDef modified_ammoCat;
        //string modified_categoryDefName;
        //string modified_categoryIconPath;
        //ThingCategoryDef modified_catParent;
        //string modified_catParentName;

        //ammosetdef and its serializable data
        AmmoSetDef modified_ammoSetDef;

        public AmmoSetDef GeneratedAmmoSetDef 
        { 
            get => modified_ammoSetDef;
        }
        List<AmmoLink> modified_ammoLinks = new List<AmmoLink>();
        string modified_ammoSetDefName;
        string modified_ammoSetLabel;
        string modified_ammoSetDescription;

        // List of ammos and their serializable data
        public List<AmmoDef> modified_ammoDefs = new List<AmmoDef>();
        List<string> modified_ammoDefStrings = new List<string>(); //used for saving/loading

        // List of projectiles and their serializable data
        //ThingDef stuff
        List<ThingDef> modified_projectiles = new List<ThingDef>();
        List<string> modified_projectileNames = new List<string>();
        List<string> modified_projectileLabels = new List<string>();
        List<APCEConstants.ThingClasses> modified_thingClasses = new List<APCEConstants.ThingClasses>();

        //projectile stuff
        List<DamageDef> modified_damageDefs = new List<DamageDef>();
        List<string> modified_damageDefStrings = new List<string>(); //used for saving/loading
        List<int> modified_damages = new List<int>();
        List<float> modified_armorPenetrationSharps = new List<float>();
        List<float> modified_armorPenetrationBlunts = new List<float>();
        List<float> modified_speeds = new List<float>();
        List<float> modified_explosionRadii = new List<float>();
        List<int> modified_pelletCounts = new List<int>();
        List<float> modified_spreadMults = new List<float>();
        List<float> modified_empShieldBreakChances = new List<float>();
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

        public override void GetOriginalData()
        {
            weaponDef = def as ThingDef;
            if (gunKind == APCEConstants.gunKinds.Default)
            {
                gunKind = DataHolderUtils.DetermineGunKind(weaponDef);
            }
            original_projectile = weaponDef.Verbs[0].defaultProjectile;
            if (original_projectile != null)
            {
                original_damage = original_projectile.projectile.GetDamageAmount(1);
                original_armorPenetration = original_projectile.projectile.GetArmorPenetration(1);
                original_speed = original_projectile.projectile.speed;
                original_explosionRadius = original_projectile.projectile.explosionRadius;
                original_ai_IsIncendiary = original_projectile.projectile.ai_IsIncendiary;
                original_applyDamageToExplosionCellsNeighbors = original_projectile.projectile.applyDamageToExplosionCellsNeighbors;
                original_damageDef = original_projectile.projectile.damageDef;
                original_extraDamages = original_projectile.projectile.extraDamages;
            }
            
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
                        CalculateAmmoBow();
                        break;
                    }
                case APCEConstants.gunKinds.Shotgun:
                    {
                        CalculateAmmoShotgun();
                        break;
                    }
                case APCEConstants.gunKinds.ExplosiveLauncher:
                    {
                        CalculateAmmoExplosiveLauncher();
                        break;
                    }
                case APCEConstants.gunKinds.Turret:
                    {
                        if ((weaponDef.Verbs[0].defaultProjectile.GetCompProperties<CompProperties_ExplosiveCE>() != null)
                            || (weaponDef.Verbs[0].defaultProjectile.GetCompProperties<CompProperties_Explosive>() != null)
                            || (weaponDef.Verbs[0].defaultProjectile.thingClass == typeof(Projectile_Explosive)))
                        {
                            CalculateAmmoExplosiveLauncher();
                        }
                        else
                        {
                            CalculateAmmoIndustrial();
                        }
                        break;
                    }
                case APCEConstants.gunKinds.Grenade:
                    {
                        CalculateAmmoGrenade();
                        break;
                    }
                default:
                    {
                        if ((weaponDef.techLevel == TechLevel.Spacer) || (weaponDef.techLevel == TechLevel.Ultra) || (weaponDef.techLevel == TechLevel.Archotech))
                        {
                            CalculateAmmoSpacer();

                        }
                        else
                        {
                            CalculateAmmoIndustrial();
                        }
                        break;
                    }
            }
        }

        public void CalculateAmmoBow()
        {
            for (int i = 0; i < 5; i++)
            {
                List<DamageDef> secondaryDamageDefs = new List<DamageDef>();
                List<int> secondaryDamageAmounts = new List<int>();
                List<float> secondaryDamageChances = new List<float>();
                ExtraDamageToSecondary(ref secondaryDamageDefs, ref secondaryDamageAmounts, ref secondaryDamageChances);

                modified_pelletCounts.Add(1);
                modified_spreadMults.Add(1);
                modified_empShieldBreakChances.Add(1);
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

                        modified_ammoDefs.Add(APCEDefOf.Ammo_Arrow_Stone);
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


                        modified_ammoDefs.Add(APCEDefOf.Ammo_Arrow_Steel);
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

                        modified_ammoDefs.Add(APCEDefOf.Ammo_Arrow_Plasteel);
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

                        modified_ammoDefs.Add(APCEDefOf.Ammo_Arrow_Venom);
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

                        modified_ammoDefs.Add(APCEDefOf.Ammo_Arrow_Flame);
                        break;
                }

                modified_secondaryDamageDefs.Add(secondaryDamageDefs);
                modified_secondaryDamageAmounts.Add(secondaryDamageAmounts);
                modified_secondaryDamageChances.Add(secondaryDamageChances);
            }
        }

        public void CalculateAmmoShotgun()
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
                        modified_empShieldBreakChances.Add(1);

                        modified_ammoDefs.Add(APCEDefOf.Ammo_Shotgun_Buck);
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
                        modified_empShieldBreakChances.Add(1);

                        modified_ammoDefs.Add(APCEDefOf.Ammo_Shotgun_Slug);
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
                        modified_empShieldBreakChances.Add(1);

                        modified_ammoDefs.Add(APCEDefOf.Ammo_Shotgun_Beanbag);
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
                        modified_empShieldBreakChances.Add(0.2f);

                        modified_ammoDefs.Add(APCEDefOf.Ammo_Shotgun_ElectroSlug);
                        break;
                }

                modified_secondaryDamageDefs.Add(secondaryDamageDefs);
                modified_secondaryDamageAmounts.Add(secondaryDamageAmounts);
                modified_secondaryDamageChances.Add(secondaryDamageChances);
            }
        }

        public void CalculateAmmoExplosiveLauncher()
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
                modified_empShieldBreakChances.Add(1);

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

                        modified_ammoDefs.Add(APCEDefOf.Ammo_LauncherGrenade_HE);
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

                        modified_ammoDefs.Add(APCEDefOf.Ammo_LauncherGrenade_HEDP);
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

                        modified_ammoDefs.Add(APCEDefOf.Ammo_LauncherGrenade_EMP);
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

                        modified_ammoDefs.Add(APCEDefOf.Ammo_LauncherGrenade_Smoke);
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

        public void CalculateAmmoIndustrial()
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
                modified_empShieldBreakChances.Add(1);
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
                        modified_armorPenetrationBlunts.Add(armorPenBluntModded);
                        modified_speeds.Add(168);

                        modified_ammoDefs.Add(APCEDefOf.Ammo_RifleIntermediate_FMJ);
                        break;
                    case 1:
                        //AP
                        modified_projectileNames.Add("APCE_AP_Bullet_" + weaponDef.defName);
                        modified_projectileLabels.Add(weaponDef.label + " AP bullet");
                        modified_damages.Add((int)(original_damage * 0.66f + 0.5f));
                        modified_armorPenetrationSharps.Add(armorPenSharpModded * 2f);
                        modified_armorPenetrationBlunts.Add(armorPenBluntModded);
                        modified_speeds.Add(168);

                        modified_ammoDefs.Add(APCEDefOf.Ammo_RifleIntermediate_AP);
                        break;
                    case 2:
                        //HP
                        modified_projectileNames.Add("APCE_HP_Bullet_" + weaponDef.defName);
                        modified_projectileLabels.Add(weaponDef.label + " HP bullet");
                        modified_damages.Add((int)(original_damage * 1.33f + 0.5f));
                        modified_armorPenetrationSharps.Add(armorPenSharpModded * 0.5f);
                        modified_armorPenetrationBlunts.Add(armorPenBluntModded);
                        modified_speeds.Add(168);

                        modified_ammoDefs.Add(APCEDefOf.Ammo_RifleIntermediate_HP);
                        break;
                    case 3:
                        //AP-I
                        modified_projectileNames.Add("APCE_API_Bullet_" + weaponDef.defName);
                        modified_projectileLabels.Add(weaponDef.label + " AP-I bullet");
                        modified_damages.Add((int)(original_damage * 0.66f + 0.5f));
                        modified_armorPenetrationSharps.Add(armorPenSharpModded * 2f);
                        modified_armorPenetrationBlunts.Add(armorPenBluntModded);
                        modified_speeds.Add(168);
                        secondaryDamageDefs.Add(CE_DamageDefOf.Flame_Secondary);
                        secondaryDamageAmounts.Add((int)(original_damage * 0.33f + 0.5f));
                        secondaryDamageChances.Add(1);

                        modified_ammoDefs.Add(APCEDefOf.Ammo_RifleIntermediate_Incendiary);
                        break;
                    case 4:
                        //HE
                        modified_projectileNames.Add("APCE_HE_Bullet_" + weaponDef.defName);
                        modified_projectileLabels.Add(weaponDef.label + " HE bullet");
                        modified_damages.Add(original_damage);
                        modified_armorPenetrationSharps.Add(armorPenSharpModded);
                        modified_armorPenetrationBlunts.Add(armorPenBluntModded);
                        modified_speeds.Add(168);
                        secondaryDamageDefs.Add(DamageDefOf.Bomb);
                        secondaryDamageAmounts.Add((int)(original_damage * 0.66f + 0.5f));
                        secondaryDamageChances.Add(1);

                        modified_ammoDefs.Add(APCEDefOf.Ammo_RifleIntermediate_HE);
                        break;
                    case 5:
                        //Sabot
                        modified_projectileNames.Add("APCE_Sabot_Bullet_" + weaponDef.defName);
                        modified_projectileLabels.Add(weaponDef.label + " sabot bullet");
                        modified_damages.Add((int)(original_damage * 0.5f + 0.5f));
                        modified_armorPenetrationSharps.Add(armorPenSharpModded * 3.5f);
                        modified_armorPenetrationBlunts.Add(armorPenBluntModded * 1.5f);
                        modified_speeds.Add(227);

                        modified_ammoDefs.Add(APCEDefOf.Ammo_RifleIntermediate_Sabot);
                        break;
                }

                modified_secondaryDamageDefs.Add(secondaryDamageDefs);
                modified_secondaryDamageAmounts.Add(secondaryDamageAmounts);
                modified_secondaryDamageChances.Add(secondaryDamageChances);
            }
        }

        public void CalculateAmmoSpacer()
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
                        modified_armorPenetrationBlunts.Add(armorPenBluntModded);
                        modified_empShieldBreakChances.Add(1);

                        modified_ammoDefs.Add(APCEDefOf.Ammo_RifleCharged);
                        break;
                    case 1:
                        //conc
                        modified_projectileNames.Add("APCE_Conc_Bullet_" + weaponDef.defName);
                        modified_projectileLabels.Add(weaponDef.label + " sabot bullet");
                        modified_damages.Add((int)(original_damage * 0.75f + 0.5f));
                        modified_armorPenetrationSharps.Add(armorPenSharpModded * 2f);
                        modified_armorPenetrationBlunts.Add(armorPenBluntModded);
                        modified_empShieldBreakChances.Add(1);

                        modified_ammoDefs.Add(APCEDefOf.Ammo_RifleCharged_AP);
                        break;
                    case 2:
                        //ion
                        modified_projectileNames.Add("APCE_Ion_Bullet_" + weaponDef.defName);
                        modified_projectileLabels.Add(weaponDef.label + " sabot bullet");
                        modified_damages.Add((int)(original_damage * 0.75f + 0.5f));
                        modified_armorPenetrationSharps.Add(armorPenSharpModded * 1.5f);
                        modified_armorPenetrationBlunts.Add(armorPenBluntModded);
                        modified_empShieldBreakChances.Add(0.2f);
                        secondaryDamageDefs.Add(DamageDefOf.EMP);
                        secondaryDamageAmounts.Add((int)(original_damage * 0.5f + 0.5f));
                        secondaryDamageChances.Add(1);

                        modified_ammoDefs.Add(APCEDefOf.Ammo_RifleCharged_Ion);
                        break;
                }

                modified_secondaryDamageDefs.Add(secondaryDamageDefs);
                modified_secondaryDamageAmounts.Add(secondaryDamageAmounts);
                modified_secondaryDamageChances.Add(secondaryDamageChances);
            }
        }

        public void CalculateAmmoGrenade()
        {
            //TODO
            List<DamageDef> secondaryDamageDefs = new List<DamageDef>();
            List<int> secondaryDamageAmounts = new List<int>();
            List<float> secondaryDamageChances = new List<float>();
            ExtraDamageToSecondary(ref secondaryDamageDefs, ref secondaryDamageAmounts, ref secondaryDamageChances);

            modified_thingClasses.Add(APCEConstants.ThingClasses.ProjectileCE_Explosive);
            modified_projectileNames.Add("APCE_Grenade_" + weaponDef.defName);
            modified_projectileLabels.Add(weaponDef.label + " projectile");
            modified_damageDefs.Add(original_damageDef);
            modified_damages.Add(original_damage);
            modified_armorPenetrationSharps.Add(0);
            modified_armorPenetrationBlunts.Add(0);
            modified_speeds.Add(original_speed);
            modified_explosionRadii.Add(original_explosionRadius);
            modified_pelletCounts.Add(1);
            modified_spreadMults.Add(1);
            modified_empShieldBreakChances.Add(1);
            modified_suppressionFactors.Add(1);
            modified_dangerFactors.Add(1);
            modified_ai_IsIncendiary.Add(original_ai_IsIncendiary);
            modified_applyDamageToExplosionCellsNeighbors.Add(original_applyDamageToExplosionCellsNeighbors);

            modified_secondaryDamageDefs.Add(secondaryDamageDefs);
            modified_secondaryDamageAmounts.Add(secondaryDamageAmounts);
            modified_secondaryDamageChances.Add(secondaryDamageChances);

            modified_ammoDefs.Add(null);
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
            //construct AmmoSet

            BuildSecondaryDamages();

            BuildProjectiles();

            BuildAmmoLinks();

            BuildAmmoSet();
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

            if (Scribe.mode == LoadSaveMode.LoadingVars
                || (Scribe.mode == LoadSaveMode.Saving && isCustomized == true))
            {
                if (Scribe.mode == LoadSaveMode.Saving)
                {
                    Stringify();
                }
                // Strings related to AmmoSetDef
                Scribe_Values.Look(ref modified_ammoSetDefName, "modified_ammoSetDefName");
                Scribe_Values.Look(ref modified_ammoSetLabel, "modified_ammoSetLabel");
                Scribe_Values.Look(ref modified_ammoSetDescription, "modified_ammoSetDescription");

                //AmmoDefs
                Scribe_Collections.Look(ref modified_ammoDefs, "modified_ammoDefs");

                //Projectile things
                Scribe_Collections.Look(ref modified_projectileNames, "modified_projectileNames");
                Scribe_Collections.Look(ref modified_projectileLabels, "modified_projectileLabels");
                Scribe_Collections.Look(ref modified_thingClasses, "modified_thingClasses");

                //Projectile Properties
                Scribe_Collections.Look(ref modified_damageDefStrings, "modified_damageDefNames");
                Scribe_Collections.Look(ref modified_damages, "modified_damages");
                Scribe_Collections.Look(ref modified_armorPenetrationSharps, "modified_armorPenetrationSharps");
                Scribe_Collections.Look(ref modified_armorPenetrationBlunts, "modified_armorPenetrationBlunts");
                Scribe_Collections.Look(ref modified_speeds, "modified_speeds");
                Scribe_Collections.Look(ref modified_explosionRadii, "modified_explosionRadii");
                Scribe_Collections.Look(ref modified_pelletCounts, "modified_pelletCounts");
                Scribe_Collections.Look(ref modified_spreadMults, "modified_spreadMults");
                Scribe_Collections.Look(ref modified_empShieldBreakChances, "modified_empShieldBreakChances");
                Scribe_Collections.Look(ref modified_suppressionFactors, "modified_suppressionFactors");
                Scribe_Collections.Look(ref modified_dangerFactors, "modified_dangerFactors");
                Scribe_Collections.Look(ref modified_ai_IsIncendiary, "modified_ai_IsIncendiary");
                Scribe_Collections.Look(ref modified_applyDamageToExplosionCellsNeighbors, "modified_applyDamageToExplosionCellsNeighbors");
                //Secondary Damages
                Scribe_Collections.Look(ref modified_secondaryDamageDefStrings, "modified_secondaryDamageDefStrings");
                Scribe_Collections.Look(ref modified_secondaryDamageAmounts, "modified_secondaryDamageAmounts");
                Scribe_Collections.Look(ref modified_secondaryDamageChances, "modified_secondaryDamageChances");

                if (Scribe.mode == LoadSaveMode.LoadingVars)
                {
                    Destringify();
                    Patch(); //unlike the other DataHolders, AmmoSet needs to Patch ASAP so the def is in the database by the time ranged weapons try to look it up
                }
            }
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

        public void BuildProjectiles()
        {
            modified_projectiles.Clear();
            for (int i = 0; i < modified_projectileNames.Count; i++)
            {
                ProjectilePropertiesCE newProjProps = new ProjectilePropertiesCE()
                {
                    damageDef = modified_damageDefs[i],
                    armorPenetrationSharp = modified_armorPenetrationSharps[i],
                    armorPenetrationBlunt = modified_armorPenetrationBlunts[i],
                    speed = modified_speeds[i],
                    explosionRadius = modified_explosionRadii[i],
                    pelletCount = modified_pelletCounts[i],
                    spreadMult = modified_spreadMults[i],
                    empShieldBreakChance = modified_empShieldBreakChances[i],
                    suppressionFactor = modified_suppressionFactors[i],
                    dangerFactor = modified_dangerFactors[i],
                    ai_IsIncendiary = modified_ai_IsIncendiary[i],
                    applyDamageToExplosionCellsNeighbors = modified_applyDamageToExplosionCellsNeighbors[i],

                    secondaryDamage = modified_secondaryDamages[i] //this should have been populated already at this point
                };

                DataHolderUtils.SetDamage(newProjProps, modified_damages[i]);

                ThingDef newProj = new ThingDef()
                {
                    defName = modified_projectileNames[i],
                    label = modified_projectileLabels[i],
                    thingClass = GetProjectileThingClass(i),
                    projectile = newProjProps
                };

                SetProjectileDefaults(newProj, original_projectile);

                ThingDef td = DefDatabase<ThingDef>.GetNamedSilentFail(newProj.defName);
                if (td != null)
                {
                    td = newProj;
                }
                else
                {
                    InjectedDefHasher.GiveShortHashToDef(newProj, typeof(ThingDef));
                    DefGenerator.AddImpliedDef<ThingDef>(newProj);
                }
                modified_projectiles.Add(newProj);
            }
        }

        public void SetProjectileDefaults(ThingDef newProj, ThingDef oldProj)
        {
            newProj.category = ThingCategory.Projectile;
            newProj.tickerType = TickerType.Normal;
            newProj.altitudeLayer = AltitudeLayer.Projectile;
            newProj.useHitPoints = false;
            newProj.neverMultiSelect = true;
            if (newProj.graphicData == null)
            {
                newProj.graphicData = new GraphicData();
            }
            newProj.graphicData.shaderType = ShaderTypeDefOf.Transparent;
            newProj.graphicData.graphicClass = typeof(Graphic_Single);
            newProj.graphicData.texPath = oldProj.graphicData.texPath; //TODO modifiable texPath //TODO handle VE's stupid fucking graphicData subclass
            newProj.projectile.explosionDamageFalloff = true;
        }

        private void BuildAmmoLinks()
        {
            modified_ammoLinks.Clear();

            for (int i = 0; i < modified_ammoDefs.Count; i++)
            {
                AmmoLink ammoLink = new AmmoLink()
                {
                    ammo = modified_ammoDefs[i],
                    projectile = modified_projectiles[i]
                };

                modified_ammoLinks.Add(ammoLink);
            }
        }

        public Type GetProjectileThingClass(int i)
        {
            APCEConstants.ThingClasses thingClass = modified_thingClasses[i];
            switch (thingClass)
            {
                case APCEConstants.ThingClasses.BulletCE:
                    return typeof(BulletCE);
                case APCEConstants.ThingClasses.ProjectileCE_Explosive:
                    return typeof(ProjectileCE_Explosive);
                case APCEConstants.ThingClasses.ProjectileCE:
                    return typeof(ProjectileCE);
                default:
                    return typeof(ProjectileCE);
            }
        }

        public void BuildAmmoSet()
        {
            AmmoSetDef ammoSet = new AmmoSetDef()
            {
                defName = modified_ammoSetDefName,
                label = modified_ammoSetLabel,
                ammoTypes = modified_ammoLinks
            };

            AmmoSetDef asd = DefDatabase<AmmoSetDef>.GetNamedSilentFail(ammoSet.defName);
            if (asd != null)
            {
                asd = ammoSet;
            }
            else
            {
                InjectedDefHasher.GiveShortHashToDef(ammoSet, typeof(AmmoSetDef));
                DefGenerator.AddImpliedDef<AmmoSetDef>(ammoSet);
            }
            modified_ammoSetDef = ammoSet;
        }

        public void Stringify()
        {
            //ammodefs
            modified_ammoDefStrings.Clear();
            for (int i = 0; i < modified_ammoDefs.Count; i++)
            {
                modified_ammoDefStrings.Add(modified_ammoDefs[i].ToString());
            }

            //projectile damageDef
            modified_damageDefStrings.Clear();
            for (int i = 0; i < modified_damageDefs.Count; i++)
            {
                modified_damageDefStrings.Add(modified_damageDefs[i].ToString());
            }

            //secondary damageDefs
            modified_secondaryDamageDefStrings.Clear();
            for (int i = 0; i < modified_secondaryDamageDefs.Count; i++)
            {
                modified_secondaryDamageDefStrings.Add(new List<string>());
                if (modified_secondaryDamageDefs[i].NullOrEmpty())
                    continue;
                for (int j = 0; j < modified_secondaryDamageDefs[i].Count; j++)
                {
                    modified_secondaryDamageDefStrings[i].Add(modified_secondaryDamageDefs[i][j].ToString());
                }
            }
        }

        public void Destringify()
        {
            //ammodefs
            modified_ammoDefs.Clear();
            for (int i = 0; i < modified_ammoDefStrings.Count; i++)
            {
                AmmoDef ammo = DefDatabase<AmmoDef>.GetNamed(modified_ammoDefStrings[i]);
                if (ammo == null)
                {
                    //todo handle failure -- default to some generic?
                }
                modified_ammoDefs.Add(ammo);
            }

            //projectile damageDef
            modified_damageDefs.Clear();
            for (int i = 0; i < modified_damageDefStrings.Count; i++)
            {
                DamageDef dam = DefDatabase<DamageDef>.GetNamed(modified_damageDefStrings[i]);
                if (dam == null)
                {
                    //todo handle failure -- default to Bullet?
                }
                modified_damageDefs.Add(dam);
            }

            //secondary damageDefs
            for (int i = 0; i < modified_secondaryDamageDefStrings.Count; i++)
            {
                modified_ToolCapacityDefs.Add(new List<ToolCapacityDef>());
                if (modified_secondaryDamageDefStrings[i].NullOrEmpty())
                    continue;
                for (int j = 0; j < modified_secondaryDamageDefStrings[i].Count; j++)
                {
                    DamageDef dam2 = DefDatabase<DamageDef>.GetNamed(modified_secondaryDamageDefStrings[i][j]);
                    if (dam2 == null)
                    {
                        //todo handle failure -- default to ???
                    }
                    modified_secondaryDamageDefs[i].Add(dam2);
                }
            }
        }

        //It is only after two hours of work that I realize that I don't need to make ThingCategoryDefs since I no longer generate new AmmoDefs, instead assigning generic Ammos.
        /*
        public void BuildThingCategoryDef()
        {
            ThingCategoryDef ammoCat = new ThingCategoryDef()
            {
                parent = ReturnModAmmoThingCategoryDef(),
                defName = "Ammo" + weaponDef.defName,
                label = weaponDef.label,
                iconPath = modified_categoryIconPath,
                resourceReadoutRoot = false
            };
            InjectedDefHasher.GiveShortHashToDef(ammoCat, typeof(ThingCategoryDef));
            DefGenerator.AddImpliedDef<ThingCategoryDef>(ammoCat);

            ammoCat.parent.childCategories.Add(ammoCat);
            foreach (ammo)
        }

        public ThingCategoryDef ReturnModAmmoThingCategoryDef()
        {
            if (APCESettings.modAmmoThingCategoryDict.TryGetValue(parentModPackageId, out ThingCategoryDef tcd))
            {
                return tcd;
            }
            else
            {
                ThingCategoryDef newCat = new ThingCategoryDef()
                {
                    parent = APCEDefOf.Ammo,
                    defName = "Ammo" + modData.mod.Name.Replace(" ",""),
                    label = modData.mod.Name,
                    iconPath = APCEDefOf.Ammo.iconPath,
                    resourceReadoutRoot = false
                };
                APCESettings.modAmmoThingCategoryDict.Add(parentModPackageId, tcd);
                APCEDefOf.Ammo.childCategories.Add(newCat);
                InjectedDefHasher.GiveShortHashToDef(newCat, typeof(ThingCategoryDef));
                DefGenerator.AddImpliedDef<ThingCategoryDef>(newCat);

                return tcd;
            }
        }
        */
    }
}
