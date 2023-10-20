using CombatExtended;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using UnityEngine;

namespace nuff.AutoPatcherCombatExtended
{
    public class DefDataHolderRangedWeapon : DefDataHolder
    {
        public DefDataHolderRangedWeapon(ThingDef def) : base(def)
        {
        }

        public ThingDef thingDef;

        float rangedToolTechMult;

        //original statbase stuff
        float original_mass;
        float original_rangedWeaponCooldown;
        float original_workToMake;
        int original_burstShotCount;

        //original verbprops stuff - just store the whole thing, since it won't be modified
        VerbProperties original_VerbProperties;

        //original other
        APCEConstants.gunKinds gunKind;

        //modified statbase stuff
        float modified_mass;
        float modified_bulk;
        float modified_rangedWeaponCoolDown;
        float modified_workToMake;
        float modified_sightsEfficiency;
        float modified_shotSpread;
        float modified_swayFactor;
        float modified_recoil;

        //modified verbprops stuff
        Type modified_VerbClass;
        float modified_muzzleFlashScale;
        int modified_ticksBetweenBurstShots;
        float modified_warmupTime;
        int modified_burstShotCount;
        RecoilPattern modified_recoilPattern;

        //modified comp stuff
        int modified_magazineSize;
        int modified_ammoGenPerMagOverride;
        float modified_reloadTime;
        bool modified_throwMote;
        bool modified_reloadOneAtATime;
        float modified_loadedAmmoBulkFactor;
        AmmoSetDef modified_AmmoSetDef;

        int modified_aimedBurstShotCount;
        bool modified_aiUseBurstMode;
        bool modified_noSingleShot;
        bool modified_noSnapShot;
        AimMode modified_aiAimMode;

        //modified other stuff

        public override void GetOriginalData()
        {
            thingDef = def as ThingDef;

            //need to make sure these lists aren't null before starting DetermineGunKind
            if (thingDef.statBases == null)
            {
                thingDef.statBases = new List<StatModifier>();
            }
            if (thingDef.weaponTags == null)
            {
                thingDef.weaponTags = new List<string>();
            }

            original_Tools = thingDef.tools;
            original_VerbProperties = thingDef.Verbs[0]; // TODO eventually make compatible with MVCF
            original_mass = thingDef.statBases.GetStatValueFromList(StatDefOf.Mass, 0);
            original_rangedWeaponCooldown = thingDef.statBases.GetStatValueFromList(StatDefOf.RangedWeapon_Cooldown, 0);
            original_workToMake = thingDef.statBases.GetStatValueFromList(StatDefOf.WorkToMake, 0);
        }

        public override void AutoCalculate()
        {
            //TODO may need to allow user to override gunKind for better recalculating
            gunKind = DataHolderUtils.DetermineGunKind(thingDef);
            CalculateWeaponTechMult();

            //todo mortar

            if (original_Tools != null)
            {
                for (int i = 0; i < original_Tools.Count; i++)
                {
                    modified_Tools.Add(MakeToolGun(original_Tools[i]));
                }
            }
            CalculateStatBaseValues();
            CalculateCEVerbPropValues();
            if (gunKind != APCEConstants.gunKinds.Grenade)
            {
                CalculateCompFireModesValues();
                CalculateCompAmmoUserValues();
            }
            else
            {
                //todo grenade
            }
            //generate values for comps
        }
        public override void Patch()
        {
            //TODO
            //patch stat bases
            //create and add comps
            //patch tools
            //patch verb - copy old verb + add recoil
        }

        public override StringBuilder PrepExport()
        {
            //TODO
            return null;
        }

        public override void ExportXML()
        {
            //TODO
        }

        public override void ExposeData()
        {
            base.ExposeData();
            //TODO
        }

        public void CalculateStatBaseValues()
        {
            //recoil is calculated here since I don't want to make another switch in the other method
            float ssAccuracyMod = (thingDef.statBases.GetStatValueFromList(StatDefOf.AccuracyLong, 0.5f) * 0.1f);
            float gunTechModAdd = (thingDef.techLevel.CompareTo(TechLevel.Industrial) * 0.1f);
            float gunTechModMult = (1 - gunTechModAdd);
            float recoilTechMod = (1 - (((float)thingDef.techLevel - 3) * 0.2f));
            switch (gunKind)
            {
                //calc new stat bases
                case APCEConstants.gunKinds.Bow:
                    modified_sightsEfficiency = 0.6f;
                    modified_shotSpread = 1f;
                    modified_swayFactor = 2f;
                    modified_bulk = 2f * original_mass;
                    modified_recoil = 2f * recoilTechMod;
                    break;
                case APCEConstants.gunKinds.Handgun:
                    modified_shotSpread = (0.2f - ssAccuracyMod) * gunTechModMult;
                    modified_sightsEfficiency = 0.7f + gunTechModAdd;
                    modified_swayFactor = 1f;
                    modified_bulk = 1f * original_mass;
                    break;
                case APCEConstants.gunKinds.SMG:
                    modified_shotSpread = (0.17f - ssAccuracyMod) * gunTechModMult;
                    modified_sightsEfficiency = 0.7f + gunTechModAdd;
                    modified_swayFactor = 2f;
                    modified_bulk = 1f * original_mass;
                    break;
                case APCEConstants.gunKinds.Shotgun:
                    modified_shotSpread = (0.17f - ssAccuracyMod) * gunTechModMult;
                    modified_sightsEfficiency = 1f + gunTechModAdd;
                    modified_swayFactor = 1.2f;
                    modified_bulk = 2f * original_mass;
                    break;
                case APCEConstants.gunKinds.assaultRifle:
                    modified_shotSpread = (0.13f - ssAccuracyMod) * gunTechModMult;
                    modified_sightsEfficiency = 1f + gunTechModAdd;
                    modified_swayFactor = 1.33f;
                    modified_bulk = 2f * original_mass;
                    modified_recoil = 1.8f * recoilTechMod;
                    break;
                case APCEConstants.gunKinds.MachineGun:
                    modified_shotSpread = (0.13f - ssAccuracyMod) * gunTechModMult;
                    modified_sightsEfficiency = 1f + gunTechModAdd;
                    modified_swayFactor = 1.4f;
                    modified_bulk = 1.5f * original_mass;
                    modified_recoil = 2.3f * recoilTechMod;
                    break;
                case APCEConstants.gunKinds.precisionRifle:
                    modified_shotSpread = (0.1f - ssAccuracyMod) * gunTechModMult;
                    modified_sightsEfficiency = 2.6f + gunTechModAdd;
                    modified_swayFactor = 1.35f;
                    modified_bulk = 2f * original_mass;
                    break;
                case APCEConstants.gunKinds.ExplosiveLauncher:
                    modified_shotSpread = 0.122f + (thingDef.Verbs[0].ForcedMissRadius * 0.02f);
                    modified_sightsEfficiency = 1f + gunTechModAdd;
                    modified_swayFactor = 1.8f;
                    modified_bulk = 2f * original_mass;
                    modified_recoil = 2.3f * recoilTechMod;
                    break;
                case APCEConstants.gunKinds.Turret:
                    modified_shotSpread = (0.1f - ssAccuracyMod) * gunTechModMult;
                    modified_sightsEfficiency = 1f;
                    modified_swayFactor = 1.5f;
                    modified_bulk = 2f * original_mass;
                    modified_recoil = 1f;
                    break;
                case APCEConstants.gunKinds.Grenade:
                    modified_sightsEfficiency = 0.65f;
                    break;
                default:
                    modified_shotSpread = modified_shotSpread = (0.15f - ssAccuracyMod) * gunTechModMult; //somewhere between an SMG and assault rifle
                    modified_sightsEfficiency = 1f + gunTechModAdd;
                    modified_swayFactor = 2.0f;
                    modified_bulk = 2f * original_mass;
                    modified_recoil = 1f;
                    break;
            }
        }

        public void CalculateCEVerbPropValues()
        {
            //if verb doesn't need patching, early return
            if ((original_VerbProperties.verbClass == typeof(Verb_ShootCE)) || (original_VerbProperties.verbClass == typeof(Verb_ShootCEOneUse)) || original_VerbProperties.verbClass == typeof(Verb_ShootBeam))
            {
                return;
            }

            modified_ticksBetweenBurstShots = original_VerbProperties.ticksBetweenBurstShots;

            //if warmupTime is too low, some weapons will get stuck permanently unable to fire
            modified_warmupTime = original_VerbProperties.warmupTime;
            if (modified_warmupTime < 0.07)
                modified_warmupTime = 0.07f;

            //burst sizes are usually doubled, but need to account for single-shot weapons
            modified_burstShotCount = original_VerbProperties.burstShotCount;
            if (modified_burstShotCount != 1)
                modified_burstShotCount *= 2;

            if (gunKind == APCEConstants.gunKinds.Turret || gunKind == APCEConstants.gunKinds.MachineGun)
                modified_recoilPattern = RecoilPattern.Mounted;
            else
                modified_recoilPattern = RecoilPattern.Regular;

            if (original_VerbProperties.verbClass == typeof(Verb_Shoot))
                modified_VerbClass = typeof(Verb_ShootCE);
            else if (original_VerbProperties.verbClass == typeof(Verb_LaunchProjectile) 
                || (original_VerbProperties.verbClass == typeof(Verb_ShootOneUse)))
                modified_VerbClass = typeof(Verb_ShootCEOneUse);
            else
            {
                if (APCESettings.patchCustomVerbs)
                    modified_VerbClass = typeof(Verb_ShootCE);
                else
                    throw new Exception($"Unable to patch {thingDef.label} due to unrecognized and/or custom verbClass: {original_VerbProperties.verbClass}");
            }
        }

        public void CalculateCompFireModesValues()
        {
            if (modified_burstShotCount > 1)
                modified_aimedBurstShotCount = (int)(modified_burstShotCount / 2);
            else
                modified_aimedBurstShotCount = 1;

            if (gunKind != APCEConstants.gunKinds.Turret)
            {
                modified_aiUseBurstMode = true;
                modified_noSingleShot = false;
                modified_noSnapShot = false;
                modified_aiAimMode = AimMode.Snapshot;
            }
            else
            {
                modified_aiUseBurstMode = false;
                modified_noSingleShot = true;
                modified_noSnapShot = true;
                modified_aiAimMode = AimMode.AimedShot;
            }
        }

        public void CalculateCompAmmoUserValues()
        {
            modified_loadedAmmoBulkFactor = 0;
            modified_throwMote = true;

            if (gunKind == APCEConstants.gunKinds.Bow)
            {
                modified_magazineSize = 1;
                modified_reloadTime = 1f;
                modified_throwMote = false;
                modified_reloadOneAtATime = true;
            }
            else if (gunKind == APCEConstants.gunKinds.Mortar)
            {
                modified_magazineSize = 1;
                modified_reloadTime = 5f;
                modified_throwMote = false;
                modified_reloadOneAtATime = true;
            }
            else if (gunKind == APCEConstants.gunKinds.MachineGun)
            {
                modified_magazineSize = modified_burstShotCount * 10;
                modified_reloadTime = Mathf.Clamp(modified_magazineSize * 0.09f, 0.1f, 12f);
            }
            else
            {
                modified_magazineSize = modified_burstShotCount * 5;
                modified_reloadTime = 4f;
            }

        }

        public void CalculateWeaponTechMult()
        {
            float techMult = 1f;
            switch (thingDef.techLevel)
            {
                case TechLevel.Animal:
                    techMult *= modData.weaponToolTechMultAnimal;
                    break;
                case TechLevel.Neolithic:
                    techMult *= modData.weaponToolTechMultNeolithic;
                    break;
                case TechLevel.Medieval:
                    techMult *= modData.weaponToolTechMultMedieval;
                    break;
                case TechLevel.Industrial:
                    techMult *= modData.weaponToolTechMultIndustrial;
                    break;
                case TechLevel.Spacer:
                    techMult *= modData.weaponToolTechMultSpacer;
                    break;
                case TechLevel.Ultra:
                    techMult *= modData.weaponToolTechMultUltratech;
                    break;
                case TechLevel.Archotech:
                    techMult *= modData.weaponToolTechMultArchotech;
                    break;
                default:
                    break;
            }
            rangedToolTechMult = techMult;
        }

        public ToolCE MakeToolGun(Tool tool)
        {
            ToolCE newToolCE = DataHolderUtils.MakeToolBase(tool);
            newToolCE.power *= modData.weaponToolPowerMult;
            newToolCE.armorPenetrationSharp *= modData.weaponToolSharpPenetration;
            newToolCE.armorPenetrationBlunt *= modData.weaponToolBluntPenetration;

            return newToolCE;
        }

        public void PatchMortar()
        {

        }

        public void PatchGrenade()
        {

        }
    }
}
