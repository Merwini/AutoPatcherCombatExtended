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
        List<Type> modified_thingClasses = new List<Type>(); //TODO cache references to usable thingClasses as startup?

        //projectile stuff
        List<DamageDef> modified_damageDefs = new List<DamageDef>();
        List<string> modified_damageDefNames = new List<string>(); //used for saving/loading
        List<int> modified_damages = new List<int>();
        List<float> modified_armorPenetrationSharps = new List<float>();
        List<float> modified_armorPenetrationBlunts = new List<float>();
        List<float> modified_speeds = new List<float>();
        List<int> modified_pelletCounts = new List<int>();
        List<float> modified_spreadMults = new List<float>();
        List<float> modified_empShieldBreakChance = new List<float>();

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



        string modified_defName;

        public override void GetOriginalData()
        {
            weaponDef = def as ThingDef;
            gunKind = DataHolderUtils.DetermineGunKind(weaponDef);
            original_projectile = weaponDef.Verbs[0].defaultProjectile;
            original_damage = original_projectile.projectile.GetDamageAmount(1);
            original_armorPenetration = original_projectile.projectile.GetArmorPenetration(1);
            original_speed = original_projectile.projectile.speed;
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

                switch (i)
                {
                    case 0:
                        //stone
                        modified_projectileNames.Add("APCE_Stone_Arrow_" + weaponDef.defName);
                        modified_projectileLabels.Add(weaponDef.label + " stone arrow");
                        modified_speeds.Add(31);
                        modified_damages.Add(original_damage * 1);
                        modified_damageDefs.Add(DamageDefOf.Bullet);
                        modified_armorPenetrationSharps.Add(armorPenSharpModded * 0.4f);
                        modified_armorPenetrationBlunts.Add(armorPenBluntModded * 0.33f);
                        modified_pelletCounts.Add(1);
                        modified_spreadMults.Add(1);
                        modified_empShieldBreakChance.Add(1);
                        break;
                    case 1:
                        //steel
                        modified_projectileNames.Add("APCE_Steel_Arrow_" + weaponDef.defName);
                        modified_projectileLabels.Add(weaponDef.label + " steel arrow");
                        modified_speeds.Add(36);
                        modified_damages.Add((int)(original_damage * 1.25f + 0.5f));
                        modified_damageDefs.Add(DamageDefOf.Bullet);
                        modified_armorPenetrationSharps.Add(armorPenSharpModded * 1f);
                        modified_armorPenetrationBlunts.Add(armorPenBluntModded * 1f);
                        modified_pelletCounts.Add(1);
                        modified_spreadMults.Add(1);
                        modified_empShieldBreakChance.Add(1);
                        break;
                    case 2:
                        //plasteel
                        modified_projectileNames.Add("APCE_Plasteel_Arrow_" + weaponDef.defName);
                        modified_projectileLabels.Add(weaponDef.label + " plasteel arrow");
                        modified_speeds.Add(39);
                        modified_damages.Add(original_damage * 1);
                        modified_damageDefs.Add(DamageDefOf.Bullet);
                        modified_armorPenetrationSharps.Add(armorPenSharpModded * 2);
                        modified_armorPenetrationBlunts.Add(armorPenBluntModded * 0.5f);
                        modified_pelletCounts.Add(1);
                        modified_spreadMults.Add(1);
                        modified_empShieldBreakChance.Add(1);
                        break;
                    case 3:
                        //venom
                        modified_projectileNames.Add("APCE_Venom_Arrow_" + weaponDef.defName);
                        modified_projectileLabels.Add(weaponDef.label + " venom arrow");
                        modified_speeds.Add(36);
                        modified_damages.Add((int)(original_damage * 1.25f + 0.5f));
                        modified_damageDefs.Add(DamageDefOf.Bullet);
                        modified_armorPenetrationSharps.Add(armorPenSharpModded * 1f);
                        modified_armorPenetrationBlunts.Add(armorPenBluntModded * 1f);
                        modified_pelletCounts.Add(1);
                        modified_spreadMults.Add(1);
                        modified_empShieldBreakChance.Add(1);
                        secondaryDamageDefs.Add(APCEDefOf.ArrowVenom);
                        secondaryDamageAmounts.Add((int)(original_damage * 1.25f + 0.5f));
                        secondaryDamageChances.Add(1);
                        break;
                    case 4:
                        //flame
                        modified_projectileNames.Add("APCE_Flame_Arrow_" + weaponDef.defName);
                        modified_projectileLabels.Add(weaponDef.label + " flame arrow");
                        //TODO thingClass = typeof(ProjectileCE_Explosive)
                        modified_speeds.Add(36);
                        modified_damages.Add((int)(original_damage * 0.25f + 0.5f));
                        modified_damageDefs.Add(APCEDefOf.ArrowFire);
                        modified_armorPenetrationSharps.Add(0f);
                        modified_armorPenetrationBlunts.Add(0f);
                        modified_pelletCounts.Add(1);
                        modified_spreadMults.Add(1);
                        modified_empShieldBreakChance.Add(1);
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
                        modified_secondaryDamages.Add(new List<SecondaryDamage>());
                        break;
                }

                modified_secondaryDamageDefs.Add(secondaryDamageDefs);
                modified_secondaryDamageAmounts.Add(secondaryDamageAmounts);
                modified_secondaryDamageChances.Add(secondaryDamageChances);
            }
        }

        public void GenerateAmmoExplosiveLauncher()
        {

        }

        public void GenerateAmmoIndustrial()
        {

        }

        public void GenerateAmmoSpacer()
        {

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
            throw new NotImplementedException();
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
        }
    }
}
