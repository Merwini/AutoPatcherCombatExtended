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

        public ThingDef weaponThingDef;

        float rangedToolTechMult;

        //original statbase stuff
        float original_Mass;
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
        float modified_rangedWeaponCooldown;
        float modified_workToMake;
        float modified_sightsEfficiency;
        float modified_shotSpread;
        float modified_swayFactor;

        //modified verbprops stuff
        Type modified_VerbClass;
        float modified_muzzleFlashScale;
        int modified_ticksBetweenBurstShots;
        float modified_warmupTime = 1;
        int modified_burstShotCount = 1;
        float modified_recoilAmount;
        RecoilPattern modified_recoilPattern = RecoilPattern.None;

        //modified comp stuff
        int modified_magazineSize;
        int modified_ammoGenPerMagOverride;
        float modified_reloadTime;
        bool modified_throwMote;
        bool modified_reloadOneAtATime;
        float modified_loadedAmmoBulkFactor;
        AmmoSetDef modified_AmmoSetDef;
        string modified_AmmoSetDefString;
        DefDataHolderAmmoSet ammoSetDataHolder;

        int modified_aimedBurstShotCount;
        bool modified_aiUseBurstMode;
        bool modified_noSingleShot;
        bool modified_noSnapShot;
        AimMode modified_aiAimMode;

        //modified other stuff
        AmmoDef modified_ammoDef; // for use in grenades
        int modified_recipeCount; //also for grenades
        int modified_stackLimit;


        public override void GetOriginalData()
        {
            weaponThingDef = def as ThingDef;

            //need to make sure these lists aren't null before starting DetermineGunKind
            if (weaponThingDef.statBases == null)
            {
                weaponThingDef.statBases = new List<StatModifier>();
            }
            if (weaponThingDef.weaponTags == null)
            {
                weaponThingDef.weaponTags = new List<string>();
            }

            original_Tools = weaponThingDef.tools;
            original_VerbProperties = weaponThingDef.Verbs[0]; // TODO eventually make compatible with MVCF
            original_Mass = weaponThingDef.statBases.GetStatValueFromList(StatDefOf.Mass, 0);
            original_rangedWeaponCooldown = weaponThingDef.statBases.GetStatValueFromList(StatDefOf.RangedWeapon_Cooldown, 0);
            original_workToMake = weaponThingDef.statBases.GetStatValueFromList(StatDefOf.WorkToMake, 0);
        }

        public override void AutoCalculate()
        {
            //TODO may need to allow user to override gunKind for better recalculating
            gunKind = DataHolderUtils.DetermineGunKind(weaponThingDef);
            CalculateWeaponTechMult();

            if (gunKind == APCEConstants.gunKinds.Mortar)
            {
                CalculateMortar();
                return;
            }

            if (original_Tools != null)
            {
                for (int i = 0; i < original_Tools.Count; i++)
                {
                    ModToolAtIndex(i);
                }
            }

            CalculateStatBaseValues();

            if (gunKind == APCEConstants.gunKinds.BeamGun)
            {
                return;
            }

            if (gunKind != APCEConstants.gunKinds.Grenade)
            {
                CalculateCEVerbPropValues();
                CalculateCompFireModesValues();
                CalculateCompAmmoUserValues();
            }
            else
            {
                CalculateGrenade();
            }
            if (modified_AmmoSetDef == null)
            {
                GenerateAmmoSet();
            }
        }

        public override void PostLoad()
        {
            if (modified_AmmoSetDef == null && modified_AmmoSetDefString != null)
            {
                modified_AmmoSetDef = DefDatabase<AmmoSetDef>.AllDefsListForReading.First(asd => asd.defName.Equals(modified_AmmoSetDefString));
            }
            base.PostLoad();
        }

        public override void PrePatch()
        {
            //try again to find the AmmoSetDef, and generate a new one if unable to
            if (modified_AmmoSetDef == null && modified_AmmoSetDefString != null)
            {
                modified_AmmoSetDef = DefDatabase<AmmoSetDef>.AllDefsListForReading.First(asd => asd.defName.Equals(modified_AmmoSetDefString));
            }
            if (modified_AmmoSetDef == null)
            {
                GenerateAmmoSet();
            }
            base.PrePatch();
        }

        public override void Patch()
        {
            //TODO
            PatchStatBases();
            PatchComps();
            BuildTools();
            for (int i = 0; i < modified_Tools.Count; i++)
            {
                weaponThingDef.tools.Add(modified_Tools[i]);
            }
            PatchVerb();
            
            if (gunKind == APCEConstants.gunKinds.Grenade)
            {
                DataHolderUtils.AddCompReplaceMe(weaponThingDef, modified_ammoDef);
                bool hasRecipe = DataHolderUtils.ReplaceRecipes(weaponThingDef, modified_ammoDef, modified_recipeCount);
                if (APCESettings.printLogs)
                {
                    Log.Message("ThingDef " + weaponThingDef.defName + " classified as a grenade, found a recipe to modify: " + hasRecipe.ToString());
                }
            }
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
            if (Scribe.mode == LoadSaveMode.LoadingVars
                || (Scribe.mode == LoadSaveMode.Saving && isCustomized == true))
            {
                if (Scribe.mode == LoadSaveMode.Saving && modified_AmmoSetDef != null)
                {
                    modified_AmmoSetDefString = modified_AmmoSetDef.ToString();
                }
                Scribe_Values.Look(ref gunKind, "gunKind");

                Scribe_Values.Look(ref modified_mass, "modified_mass", original_Mass);
                Scribe_Values.Look(ref modified_bulk, "modified_bulk", 1f);
                Scribe_Values.Look(ref modified_rangedWeaponCooldown, "modified_rangedWeaponCooldown", 1f);
                Scribe_Values.Look(ref modified_workToMake, "modified_workToMake", 1000);
                Scribe_Values.Look(ref modified_sightsEfficiency, "modified_sightsEfficiency", 1f);
                Scribe_Values.Look(ref modified_shotSpread, "modified_shotSpread", 0.1f);
                Scribe_Values.Look(ref modified_swayFactor, "modified_swayFactor", 2f);

                Scribe_Deep.Look(ref modified_VerbClass, "modified_VerbClass");
                Scribe_Values.Look(ref modified_muzzleFlashScale, "modified_muzzleFlashScale", 9);
                Scribe_Values.Look(ref modified_ticksBetweenBurstShots, "modified_ticksBetweenBurstShots", 6);
                Scribe_Values.Look(ref modified_warmupTime, "modified_warmupTime", 1f);
                Scribe_Values.Look(ref modified_burstShotCount, "modified_burstShotCount", 1);
                Scribe_Values.Look(ref modified_recoilAmount, "modified_recoilAmount", 0);
                Scribe_Values.Look(ref modified_recoilPattern, "modified_recoilPattern", RecoilPattern.None);

                Scribe_Values.Look(ref modified_magazineSize, "modified_magazineSize", 1);
                Scribe_Values.Look(ref modified_ammoGenPerMagOverride, "modified_ammoGenPerMagOverride", 0);
                Scribe_Values.Look(ref modified_reloadTime, "modified_reloadTime", 4);
                Scribe_Values.Look(ref modified_throwMote, "modified_throwMote", false);
                Scribe_Values.Look(ref modified_reloadOneAtATime, "modified_reloadOneAtATime", false);
                Scribe_Values.Look(ref modified_loadedAmmoBulkFactor, "modified_loadedAmmoBulkFactor", 0);
                Scribe_Values.Look(ref modified_AmmoSetDefString, "modified_AmmoSetDefString");

                Scribe_Values.Look(ref modified_aimedBurstShotCount, "modified_aimedBurstShotCount");
                Scribe_Values.Look(ref modified_aiUseBurstMode, "modified_aiUseBurstMode");
                Scribe_Values.Look(ref modified_noSingleShot, "modified_noSingleShot");
                Scribe_Values.Look(ref modified_noSnapShot, "modified_noSnapShot");
                Scribe_Values.Look(ref modified_aiAimMode, "modified_aiAimMode");

                Scribe_Values.Look(ref modified_recipeCount, "modified_recipeCount", 1);
                Scribe_Values.Look(ref modified_stackLimit, "modified_stackLimit", 25);

                if (Scribe.mode == LoadSaveMode.LoadingVars && gunKind == APCEConstants.gunKinds.Grenade)
                {
                    GenerateGrenadeAmmoDef();
                }
            }
        }

        public void CalculateStatBaseValues()
        {
            //recoil is calculated here since I don't want to make another switch in the other method
            float ssAccuracyMod = (weaponThingDef.statBases.GetStatValueFromList(StatDefOf.AccuracyLong, 0.5f) * 0.1f);
            float gunTechModAdd = (weaponThingDef.techLevel.CompareTo(TechLevel.Industrial) * 0.1f);
            float gunTechModMult = (1 - gunTechModAdd);
            float recoilTechMod = (1 - (((float)weaponThingDef.techLevel - 3) * 0.2f));
            switch (gunKind)
            {
                //calc new stat bases
                case APCEConstants.gunKinds.Bow:
                    modified_sightsEfficiency = 0.6f;
                    modified_shotSpread = 1f;
                    modified_swayFactor = 2f;
                    modified_mass = Math.Min(original_Mass, 30);
                    modified_bulk = Math.Min(2f * original_Mass, 20f);
                    modified_recoilAmount = 2f * recoilTechMod;
                    break;
                case APCEConstants.gunKinds.Handgun:
                    modified_shotSpread = (0.2f - ssAccuracyMod) * gunTechModMult;
                    modified_sightsEfficiency = 0.7f + gunTechModAdd;
                    modified_swayFactor = 1f;
                    modified_mass = Math.Min(original_Mass, 30);
                    modified_bulk = Math.Min(1f * original_Mass, 20f);
                    break;
                case APCEConstants.gunKinds.SMG:
                    modified_shotSpread = (0.17f - ssAccuracyMod) * gunTechModMult;
                    modified_sightsEfficiency = 0.7f + gunTechModAdd;
                    modified_swayFactor = 2f;
                    modified_mass = Math.Min(original_Mass, 30);
                    modified_bulk = Math.Min(1f * original_Mass, 20f);
                    break;
                case APCEConstants.gunKinds.Shotgun:
                    modified_shotSpread = (0.17f - ssAccuracyMod) * gunTechModMult;
                    modified_sightsEfficiency = 1f + gunTechModAdd;
                    modified_swayFactor = 1.2f;
                    modified_mass = Math.Min(original_Mass, 30);
                    modified_bulk = Math.Min(2f * original_Mass, 20f);
                    break;
                case APCEConstants.gunKinds.assaultRifle:
                    modified_shotSpread = (0.13f - ssAccuracyMod) * gunTechModMult;
                    modified_sightsEfficiency = 1f + gunTechModAdd;
                    modified_swayFactor = 1.33f;
                    modified_mass = Math.Min(original_Mass, 30);
                    modified_bulk = Math.Min(2f * original_Mass, 20f);
                    modified_recoilAmount = 1.8f * recoilTechMod;
                    break;
                case APCEConstants.gunKinds.MachineGun:
                    modified_shotSpread = (0.13f - ssAccuracyMod) * gunTechModMult;
                    modified_sightsEfficiency = 1f + gunTechModAdd;
                    modified_swayFactor = 1.4f;
                    modified_mass = Math.Min(original_Mass, 30);
                    modified_bulk = Math.Min(1.5f * original_Mass, 20f);
                    modified_recoilAmount = 2.3f * recoilTechMod;
                    break;
                case APCEConstants.gunKinds.precisionRifle:
                    modified_shotSpread = (0.1f - ssAccuracyMod) * gunTechModMult;
                    modified_sightsEfficiency = 2.6f + gunTechModAdd;
                    modified_swayFactor = 1.35f;
                    modified_mass = Math.Min(original_Mass, 30);
                    modified_bulk = Math.Min(2f * original_Mass, 20f);
                    break;
                case APCEConstants.gunKinds.ExplosiveLauncher:
                    modified_shotSpread = 0.122f + (weaponThingDef.Verbs[0].ForcedMissRadius * 0.02f);
                    modified_sightsEfficiency = 1f + gunTechModAdd;
                    modified_swayFactor = 1.8f;
                    modified_mass = Math.Min(original_Mass, 30);
                    modified_bulk = Math.Min(2f * original_Mass, 20f);
                    modified_recoilAmount = 2.3f * recoilTechMod;
                    break;
                case APCEConstants.gunKinds.Turret:
                    modified_shotSpread = (0.1f - ssAccuracyMod) * gunTechModMult;
                    modified_sightsEfficiency = 1f;
                    modified_swayFactor = 1.5f;
                    modified_mass = Math.Min(original_Mass, 30);
                    modified_bulk = Math.Min(2f * original_Mass, 20f);
                    modified_recoilAmount = 1f;
                    break;
                case APCEConstants.gunKinds.Grenade:
                    modified_sightsEfficiency = 1.00f;
                    modified_bulk = 0.87f;
                    modified_mass = 0.7f;
                    break;
                default:
                    modified_shotSpread = modified_shotSpread = (0.15f - ssAccuracyMod) * gunTechModMult; //somewhere between an SMG and assault rifle
                    modified_sightsEfficiency = 1f + gunTechModAdd;
                    modified_swayFactor = 2.0f;
                    modified_mass = Math.Min(original_Mass, 30);
                    modified_bulk = Math.Min(2f * original_Mass, 20f);
                    modified_recoilAmount = 1f;
                    break;
            }

            modified_workToMake = original_workToMake;
            modified_rangedWeaponCooldown = original_rangedWeaponCooldown;
        }

        public void PatchStatBases()
        {
            RemoveVanillaStatBases();

            DataHolderUtils.AddOrChangeStat(weaponThingDef.statBases, StatDefOf.Mass, modified_mass);
            DataHolderUtils.AddOrChangeStat(weaponThingDef.statBases, CE_StatDefOf.Bulk, modified_bulk);
            DataHolderUtils.AddOrChangeStat(weaponThingDef.statBases, StatDefOf.RangedWeapon_Cooldown, modified_rangedWeaponCooldown);
            DataHolderUtils.AddOrChangeStat(weaponThingDef.statBases, StatDefOf.WorkToMake, modified_workToMake);
            DataHolderUtils.AddOrChangeStat(weaponThingDef.statBases, CE_StatDefOf.SightsEfficiency, modified_sightsEfficiency);
            DataHolderUtils.AddOrChangeStat(weaponThingDef.statBases, CE_StatDefOf.ShotSpread, modified_shotSpread);
            DataHolderUtils.AddOrChangeStat(weaponThingDef.statBases, CE_StatDefOf.SwayFactor, modified_swayFactor);
        }

        public void PatchComps()
        {
            if (weaponThingDef.comps == null)
            {
                weaponThingDef.comps = new List<CompProperties>();
            }

            if (gunKind == APCEConstants.gunKinds.Grenade)
            {
                CompProperties_ExplosiveCE newComp_ExCE = new CompProperties_ExplosiveCE()
                {
                    damageAmountBase = modified_AmmoSetDef.ammoTypes[0].projectile.projectile.GetDamageAmount(1),
                    explosiveDamageType = modified_AmmoSetDef.ammoTypes[0].projectile.projectile.damageDef,
                    explosiveRadius = modified_AmmoSetDef.ammoTypes[0].projectile.projectile.explosionRadius
                };
                weaponThingDef.comps.Add(newComp_ExCE);
            }

            //TODO customization
            if (gunKind == APCEConstants.gunKinds.Mortar)
            {
                CompProperties_Charges newComp_Charges = new CompProperties_Charges()
                {
                    chargeSpeeds = new List<int>()
                    {
                        30,
                        50,
                        70,
                        90
                    }
                };
            }

            CompProperties_AmmoUser newComp_AmmoUser = new CompProperties_AmmoUser()
            {
                magazineSize = modified_magazineSize,
                reloadTime = modified_reloadTime,
                reloadOneAtATime = modified_reloadOneAtATime,
                throwMote = modified_throwMote,
                ammoSet = modified_AmmoSetDef,
                //loadedAmmoBulkFactor = modified_loadedAmmoBulkFactor //TODO
            };
            weaponThingDef.comps.Add(newComp_AmmoUser);

            if (gunKind == APCEConstants.gunKinds.Mortar)
                return;

            CompProperties_FireModes newComp_FireModes = new CompProperties_FireModes()
            {
                aimedBurstShotCount = modified_aimedBurstShotCount,
                aiUseBurstMode = modified_aiUseBurstMode,
                noSingleShot = modified_noSingleShot,
                noSnapshot = modified_noSnapShot,
                aiAimMode = modified_aiAimMode
            };
            weaponThingDef.comps.Add(newComp_FireModes);
        }

        public void PatchVerb()
        {
            VerbPropertiesCE newVerbPropsCE = new VerbPropertiesCE();
            DataHolderUtils.CopyFields(weaponThingDef.Verbs[0], newVerbPropsCE);

            newVerbPropsCE.ticksBetweenBurstShots = modified_ticksBetweenBurstShots;
            newVerbPropsCE.warmupTime = modified_warmupTime;
            newVerbPropsCE.burstShotCount = modified_burstShotCount;
            newVerbPropsCE.recoilPattern = modified_recoilPattern;
            newVerbPropsCE.verbClass = modified_VerbClass;
            //newVerbPropsCE.ejectsCasings //TODO
            //newVerbPropsCE.indirectFirePenalty //TODO
            newVerbPropsCE.defaultProjectile = modified_AmmoSetDef.ammoTypes[0].projectile;
        }

        public void CalculateCEVerbPropValues()
        {
            //if verb doesn't need patching, early return
            if ((original_VerbProperties.verbClass == typeof(Verb_ShootCE)) || (original_VerbProperties.verbClass == typeof(Verb_ShootCEOneUse)) || original_VerbProperties.verbClass == typeof(Verb_ShootBeam))
            {
                return;
            }

            modified_ticksBetweenBurstShots = original_VerbProperties.ticksBetweenBurstShots;

            //if warmupTime is too low, some weapons will get stuck permanently unable to fire, since it fires when the timer ticks from 1 to 0, not when it is AT 0
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
                    throw new Exception($"Unable to patch {weaponThingDef.label} due to unrecognized and/or custom verbClass: {original_VerbProperties.verbClass}");
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
            switch (weaponThingDef.techLevel)
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
            rangedToolTechMult = techMult;
        }

        public override void ModToolAtIndex(int i)
        {
            base.ModToolAtIndex(i);
            modified_ToolPowers[i] *= modData.weaponToolPowerMult;
            modified_ToolArmorPenetrationSharps[i] *= modData.weaponToolSharpPenetration; //TODO - I think gun tools should not use techMult? Will weaken things with intended weapons like bayonets, but be better for most cases
            modified_ToolArmorPenetrationBlunts[i] *= modData.weaponToolBluntPenetration;
        }

        public void CalculateMortar()
        {
            //statbases
            modified_sightsEfficiency = 0.5f;

            //comps
            modified_magazineSize = 1;
            modified_reloadTime = 5;
            modified_AmmoSetDef = APCEDefOf.AmmoSet_81mmMortarShell;

            //verb
            modified_VerbClass = typeof(Verb_ShootMortarCE);
            modified_warmupTime = original_VerbProperties.warmupTime;
        }

        public void CalculateGrenade()
        {
            if (modified_ammoDef == null)
            {
                modified_ammoDef = GenerateGrenadeAmmoDef();
            }
            if (modified_toolIds.NullOrEmpty())
            {
                modified_toolIds.Add("APCE_Tool_" + weaponThingDef.defName);
                modified_ToolCapacityDefs.Add(new List<ToolCapacityDef>() { APCEDefOf.Blunt });
                modified_ToolLinkedBodyPartsGroupDefs.Add(APCEDefOf.Base);
                modified_ToolCooldownTimes.Add(1.75f);
                modified_ToolArmorPenetrationSharps.Add(0f);
                modified_ToolArmorPenetrationBlunts.Add(1f);
                modified_ToolPowers.Add(2);
                modified_ToolChanceFactors.Add(1);
            }

            modified_stackLimit = 75;
            modified_recipeCount = 10;
            //Apply ReplaceMe comp to ThingDef, just in case
            //CompProperties_ExplosiveCE (for if the Thing is damaged)
            //CompProperties_Fragments 
            //todo
            //projectile
            //thingClass CombatExtended.ProjectileCE_Explosive
            //projectilepropsCE
            //make sure comps aren't null, add Fragments comp if necessary -- TODO, explosive launcher needs fragments as well
        }

        public AmmoDef GenerateGrenadeAmmoDef()
        {
            AmmoDef ammoGrenade = new AmmoDef();
            DataHolderUtils.CopyFields(weaponThingDef, ammoGrenade);

            ammoGrenade.graphicData.graphicClass = typeof(Graphic_Multi);
            ammoGrenade.graphicData.onGroundRandomRotateAngle = 0;

            //make new tag lists so I can .Clear() the ones on the ThingDef version
            if (!weaponThingDef.tradeTags.NullOrEmpty())
            {// this will remove the ThingDef version of the grenade from most traders' stock, unless the mod has a custom trader with the ThingDef explicitly added as stock -- TODO search/remove
                List<string> newTradeTags = new List<string>(weaponThingDef.tradeTags);
                ammoGrenade.tradeTags = newTradeTags;
                weaponThingDef.tradeTags.Clear();
            }
            if (!weaponThingDef.weaponTags.NullOrEmpty())
            {// this will hopefully prevent Pawns from spawning with the ThingDef version
                List<string> newWeaponTags = new List<string>(weaponThingDef.weaponTags);
                ammoGrenade.weaponTags = newWeaponTags;
                weaponThingDef.weaponTags.Clear();
            }
            else
            {
                ammoGrenade.weaponTags = new List<string>();
            }
            ammoGrenade.weaponTags.Add("CE_AI_Grenade");
            ammoGrenade.weaponTags.Add("CE_AI_AOE"); // TODO might need to make these conditional, if I end up re-using code for non-explosive thrown weapons
            ammoGrenade.weaponTags.Add("CE_OneHandedWeapon");
            weaponThingDef.generateAllowChance = 0;
            weaponThingDef.generateCommonality = 0;

            return ammoGrenade;
        } 

        public void GenerateAmmoSet()
        {
            ammoSetDataHolder = new DefDataHolderAmmoSet(weaponThingDef);
            //RegisterSelfInDict, GetOriginalData, and Autocalculate are called by constructor
            ammoSetDataHolder.Patch();
            this.modified_AmmoSetDef = ammoSetDataHolder.GeneratedAmmoSetDef;
            ammoSetDataHolder.isCustomized = true; //so it will save
        }

        public void RemoveVanillaStatBases()
        {
            weaponThingDef.statBases = weaponThingDef.statBases
                .Where(statModifier => !IsVanillaStat(statModifier.stat))
                .ToList();

            bool IsVanillaStat(StatDef statDef)
            {
                return statDef == StatDefOf.AccuracyLong ||
                       statDef == StatDefOf.AccuracyMedium ||
                       statDef == StatDefOf.AccuracyShort ||
                       statDef == StatDefOf.AccuracyTouch;
            }
        }

        //this returns a float instead of just setting the value, so that the customization window can suggest it if burst shot is changed from 1 to another number
        //public float CalculateRecoilAmount()
        //{

        //}
    }
}
