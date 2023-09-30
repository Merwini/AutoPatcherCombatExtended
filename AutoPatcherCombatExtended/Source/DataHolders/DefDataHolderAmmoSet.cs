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
        float baseArmorPenSharp;
        float baseArmorPenBlunt;

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
        List<ThingDef> modified_projectiles = new List<ThingDef>();
        List<string> modified_projectileNames = new List<string>();
        List<string> modified_projectileLabels = new List<string>();
        List<DamageDef> modified_damageDefs = new List<DamageDef>();
        List<string> modified_damageDefNames = new List<string>();
        List<int> modified_damages = new List<int>();
        List<float> modified_armorPenetrationSharps = new List<float>();
        List<float> modified_armorPenetrationBlunts = new List<float>();
        List<float> modified_speeds = new List<float>();
        List<bool> modified_dropsCasings = new List<bool>();
        List<int> modified_pelletCounts = new List<int>();
        List<SecondaryDamage> modified_secondaryDamages = new List<SecondaryDamage>();
        List<string> modified_secondaryDamageDefStrings = new List<string>();
        List<int> modified_secondaryDamageAmounts = new List<int>();
        List<float> modified_secondaryDamageChances = new List<float>();

        //list of recipes and their serializable data




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

            baseArmorPenSharp = original_armorPenetration * modData.gunSharpPenMult * original_techMult;
            baseArmorPenBlunt = original_armorPenetration * modData.gunBluntPenMult * original_techMult;

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
                switch (i)
                {
                    case 0:
                        //stone
                        modified_projectileNames.Add("APCE_Stone_Arrow_" + weaponDef.defName);
                        modified_projectileLabels.Add(weaponDef.label + " stone arrow");
                        modified_speeds.Add(31);
                        break;
                    case 1:
                        //steel
                        modified_projectileNames.Add("APCE_Steel_Arrow_" + weaponDef.defName);
                        modified_projectileLabels.Add(weaponDef.label + " steel arrow");
                        modified_speeds.Add(36);
                        break;
                    case 2:
                        //plasteel
                        modified_projectileNames.Add("APCE_Plasteel_Arrow_" + weaponDef.defName);
                        modified_projectileLabels.Add(weaponDef.label + " plasteel arrow");
                        modified_speeds.Add(39);
                        break;
                    case 3:
                        //venom
                        modified_projectileNames.Add("APCE_Venom_Arrow_" + weaponDef.defName);
                        modified_projectileLabels.Add(weaponDef.label + " venom arrow");
                        modified_speeds.Add(36);
                        break;
                    case 4:
                        //flame
                        modified_projectileNames.Add("APCE_Flame_Arrow_" + weaponDef.defName);
                        modified_projectileLabels.Add(weaponDef.label + " flame arrow");
                        modified_speeds.Add(36);
                        break;
                }
            }
        }

        public void GenerateAmmoShotgun()
        {

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
    }
}
