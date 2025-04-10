﻿using CombatExtended;
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
        public DefDataHolderRangedWeapon()
        {
            //empty constructor for use by SaveLoad
        }

        public DefDataHolderRangedWeapon(ThingDef def) : base(def)
        {
        }

        public ThingDef weaponThingDef;

        float rangedToolTechMult;

        //original statbase stuff
        float original_Mass;
        float original_RangedWeaponCooldown;
        float original_WorkToMake;
        int original_BurstShotCount;

        bool stuffed;

        //original verbprops stuff - just store the whole thing, since it won't be modified
        VerbProperties original_VerbProperties;

        //original other
        APCEConstants.gunKinds gunKind;

        //modified statbase stuff
        internal float modified_Mass;
        internal float modified_Bulk;
        internal float modified_RangedWeaponCooldown;
        internal float modified_WorkToMake;
        internal float modified_SightsEfficiency;
        internal float modified_ShotSpread;
        internal float modified_SwayFactor;
        internal float modified_WeaponToughness;

        //modified verbprops stuff
        internal Type modified_VerbClass;
        internal float modified_muzzleFlashScale;
        internal int modified_TicksBetweenBurstShots;
        internal float modified_WarmupTime = 1;
        internal int modified_BurstShotCount = 1;
        internal float modified_RecoilAmount;
        internal float modified_Range; //TODO
        internal RecoilPattern modified_RecoilPattern = RecoilPattern.None;

        //modified comp stuff
        internal bool modified_usesAmmo = true;
        internal int modified_magazineSize;
        internal int modified_ammoGenPerMagOverride;
        internal float modified_reloadTime;
        internal bool modified_throwMote;
        internal bool modified_reloadOneAtATime;
        internal float modified_loadedAmmoBulkFactor;
        internal AmmoSetDef modified_AmmoSetDef;
        internal string modified_AmmoSetDefString;
        internal DefDataHolderAmmoSet ammoSetDataHolder;

        internal int modified_aimedBurstShotCount;
        internal bool modified_aiUseBurstMode;
        internal bool modified_noSingleShot;
        internal bool modified_noSnapShot;
        internal AimMode modified_aiAimMode;

        //modified grenade stuff
        AmmoDef modified_ammoDef; // for use in grenades
        int modified_recipeCount; //also for grenades
        int modified_stackLimit;
        int modified_grenadeDamage;
        float modified_explosionRadius;


        public override void GetOriginalData()
        {
            //constructed by APCEController, def assigned by constructor
            if (def != null && weaponThingDef == null)
            {
                this.weaponThingDef = def as ThingDef;
            }
            //constructed by SaveLoad, thingDef loaded from xml
            else if (weaponThingDef != null && def == null)
            {
                def = weaponThingDef;
            }

            //need to make sure these lists aren't null before starting DetermineGunKind
            if (weaponThingDef.statBases == null)
            {
                weaponThingDef.statBases = new List<StatModifier>();
            }
            if (weaponThingDef.weaponTags == null)
            {
                weaponThingDef.weaponTags = new List<string>();
            }

            if (!weaponThingDef.tools.NullOrEmpty())
            {
                original_Tools = weaponThingDef.tools.ToList();
            }
            original_VerbProperties = weaponThingDef.Verbs[0]; // TODO eventually make compatible with MVCF
            original_Mass = weaponThingDef.statBases.GetStatValueFromList(StatDefOf.Mass, 0);
            original_RangedWeaponCooldown = weaponThingDef.statBases.GetStatValueFromList(StatDefOf.RangedWeapon_Cooldown, 0);
            original_WorkToMake = weaponThingDef.statBases.GetStatValueFromList(StatDefOf.WorkToMake, 0);
            original_BurstShotCount = original_VerbProperties.burstShotCount;
            stuffed = weaponThingDef.MadeFromStuff;
        }
        public override void AutoCalculate()
        {
            //TODO may need to allow user to override gunKind for better recalculating
            gunKind = DataHolderUtils.DetermineGunKind(weaponThingDef);
            if (APCESettings.printLogs)
            {
                Log.Message($"APCE thinks that gun {def.label} from {def.modContentPack.Name} is a gun of kind: " + gunKind.ToString());
            }
            CalculateWeaponTechMult();
            if (gunKind == APCEConstants.gunKinds.Mortar)
            {
                CalculateMortar();
                return;
            }

            if (!original_Tools.NullOrEmpty())
            {
                ClearModdedTools();
                for (int i = 0; i < original_Tools.Count; i++)
                {
                    ModToolAtIndex(i);
                }
            }

            CalculateStatBaseValues();

            if (gunKind == APCEConstants.gunKinds.BeamGun)
            {
                modified_usesAmmo = false;
                return;
            }

            CalculateVerbPropValues();

            if (gunKind == APCEConstants.gunKinds.Flamethrower)
            {
                modified_AmmoSetDef = APCEDefOf.AmmoSet_Flamethrower;
            }

            if (modified_AmmoSetDef == null)
            {
                GenerateAmmoSet();
            }

            if (gunKind != APCEConstants.gunKinds.Grenade)
            {
                CalculateCompFireModesValues();
                CalculateCompAmmoUserValues();
            }
            else
            {
                CalculateGrenade();
            }
        }
        

        public override void PrePatch()
        {
            //try to find the AmmoSetDef, and generate a new one if unable to
            if (modified_AmmoSetDef == null && modified_AmmoSetDefString != null)
            {
                modified_AmmoSetDef = DefDatabase<AmmoSetDef>.GetNamedSilentFail(modified_AmmoSetDefString);
            }
            if (modified_AmmoSetDef == null && gunKind != APCEConstants.gunKinds.Grenade && gunKind != APCEConstants.gunKinds.BeamGun)
            {
                GenerateAmmoSet();
            }
            base.PrePatch();
        }

        public override void PostPatch()
        {
            if (modified_AmmoSetDef == null && modified_AmmoSetDefString != null)
            {
                modified_AmmoSetDef = DefDatabase<AmmoSetDef>.AllDefsListForReading.First(asd => asd.defName.Equals(modified_AmmoSetDefString));
            }
            base.PostPatch();
        }

        public override void Patch()
        {
            PatchStatBases();
            BuildTools();

            if (!modified_Tools.NullOrEmpty())
            {
                weaponThingDef.tools = new List<Tool>(); // changed from clear, because grenades are given a generic tool despite having an empty list, and this combines a null check + clear into one
                for (int i = 0; i < modified_Tools.Count; i++)
                {
                    weaponThingDef.tools.Add(modified_Tools[i]);
                }
            }

            if (gunKind == APCEConstants.gunKinds.BeamGun)
                return;

            PatchVerb();

            if (gunKind == APCEConstants.gunKinds.Grenade)
            {
                PatchGrenade();
                return;
            }

            PatchComps();
        }

        public override StringBuilder ExportXML()
        {
            xml = DataHolderUtils.GetXmlForDef(weaponThingDef);

            patchOps = new List<string>();



            return null;
        }

        public override void ExposeData()
        {
            if (Scribe.mode == LoadSaveMode.LoadingVars
                || (Scribe.mode == LoadSaveMode.Saving && isCustomized == true))
            {
                if (Scribe.mode == LoadSaveMode.Saving)
                {
                    if (modified_AmmoSetDef != null)
                    {
                        //doesn't need to be destringified during loading, since PrePatch() will do that
                        modified_AmmoSetDefString = modified_AmmoSetDef.ToString();
                    }
                }
                
                Scribe_Defs.Look(ref weaponThingDef, "def");
                Scribe_Values.Look(ref gunKind, "gunKind");

                Scribe_Values.Look(ref modified_Mass, "modified_mass");
                Scribe_Values.Look(ref modified_Bulk, "modified_bulk");
                Scribe_Values.Look(ref modified_RangedWeaponCooldown, "modified_rangedWeaponCooldown");
                Scribe_Values.Look(ref modified_WorkToMake, "modified_workToMake");
                Scribe_Values.Look(ref modified_SightsEfficiency, "modified_sightsEfficiency");
                Scribe_Values.Look(ref modified_ShotSpread, "modified_shotSpread");
                Scribe_Values.Look(ref modified_SwayFactor, "modified_swayFactor");
                Scribe_Values.Look(ref modified_WeaponToughness, "modified_weaponToughness");

                string verbClassName = modified_VerbClass?.AssemblyQualifiedName;
                Scribe_Values.Look(ref verbClassName, "modified_VerbClass");
                Scribe_Values.Look(ref modified_muzzleFlashScale, "modified_muzzleFlashScale");
                Scribe_Values.Look(ref modified_TicksBetweenBurstShots, "modified_ticksBetweenBurstShots");
                Scribe_Values.Look(ref modified_WarmupTime, "modified_warmupTime");
                Scribe_Values.Look(ref modified_BurstShotCount, "modified_burstShotCount");
                Scribe_Values.Look(ref modified_RecoilAmount, "modified_recoilAmount");
                Scribe_Values.Look(ref modified_RecoilPattern, "modified_recoilPattern", RecoilPattern.None);
                Scribe_Values.Look(ref modified_Range, "modified_range");

                Scribe_Values.Look(ref modified_magazineSize, "modified_magazineSize", 1);
                Scribe_Values.Look(ref modified_ammoGenPerMagOverride, "modified_ammoGenPerMagOverride");
                Scribe_Values.Look(ref modified_reloadTime, "modified_reloadTime");
                Scribe_Values.Look(ref modified_throwMote, "modified_throwMote");
                Scribe_Values.Look(ref modified_reloadOneAtATime, "modified_reloadOneAtATime");
                Scribe_Values.Look(ref modified_loadedAmmoBulkFactor, "modified_loadedAmmoBulkFactor");
                Scribe_Values.Look(ref modified_AmmoSetDefString, "modified_AmmoSetDefString");

                Scribe_Values.Look(ref modified_aimedBurstShotCount, "modified_aimedBurstShotCount");
                Scribe_Values.Look(ref modified_aiUseBurstMode, "modified_aiUseBurstMode");
                Scribe_Values.Look(ref modified_noSingleShot, "modified_noSingleShot");
                Scribe_Values.Look(ref modified_noSnapShot, "modified_noSnapShot");
                Scribe_Values.Look(ref modified_aiAimMode, "modified_aiAimMode");

                Scribe_Values.Look(ref modified_recipeCount, "modified_recipeCount");
                Scribe_Values.Look(ref modified_stackLimit, "modified_stackLimit");
                Scribe_Values.Look(ref modified_grenadeDamage, "modified_grenadeDamage");

                if (Scribe.mode == LoadSaveMode.LoadingVars)
                {
                    if (!string.IsNullOrEmpty(verbClassName))
                    {
                        modified_VerbClass = Type.GetType(verbClassName);

                        if (modified_VerbClass == null)
                        {
                            Log.Warning($"Failed to load modified_VerbClass: {verbClassName}. Type not found.");
                        }
                    }
                }
                //if (Scribe.mode == LoadSaveMode.LoadingVars && gunKind == APCEConstants.gunKinds.Grenade)
                //{
                //    modified_ammoDef = GenerateGrenadeAmmoDef();
                //}
            }
            base.ExposeData();
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
                    modified_SightsEfficiency = 0.6f;
                    modified_ShotSpread = 1f;
                    modified_SwayFactor = 2f;
                    modified_Mass = Math.Min(original_Mass, 30);
                    modified_Bulk = Math.Min(2f * original_Mass, 20f);
                    modified_RecoilAmount = 2f * recoilTechMod;
                    break;
                case APCEConstants.gunKinds.Handgun:
                    modified_ShotSpread = (0.2f - ssAccuracyMod) * gunTechModMult;
                    modified_SightsEfficiency = 0.7f + gunTechModAdd;
                    modified_SwayFactor = 1f;
                    modified_Mass = Math.Min(original_Mass, 30);
                    modified_Bulk = Math.Min(1f * original_Mass, 20f);
                    break;
                case APCEConstants.gunKinds.SMG:
                    modified_ShotSpread = (0.17f - ssAccuracyMod) * gunTechModMult;
                    modified_SightsEfficiency = 0.7f + gunTechModAdd;
                    modified_SwayFactor = 2f;
                    modified_Mass = Math.Min(original_Mass, 30);
                    modified_Bulk = Math.Min(1f * original_Mass, 20f);
                    break;
                case APCEConstants.gunKinds.Shotgun:
                    modified_ShotSpread = (0.17f - ssAccuracyMod) * gunTechModMult;
                    modified_SightsEfficiency = 1f + gunTechModAdd;
                    modified_SwayFactor = 1.2f;
                    modified_Mass = Math.Min(original_Mass, 30);
                    modified_Bulk = Math.Min(2f * original_Mass, 20f);
                    break;
                case APCEConstants.gunKinds.assaultRifle:
                    modified_ShotSpread = (0.13f - ssAccuracyMod) * gunTechModMult;
                    modified_SightsEfficiency = 1f + gunTechModAdd;
                    modified_SwayFactor = 1.33f;
                    modified_Mass = Math.Min(original_Mass, 30);
                    modified_Bulk = Math.Min(2f * original_Mass, 20f);
                    modified_RecoilAmount = 1.8f * recoilTechMod;
                    break;
                case APCEConstants.gunKinds.MachineGun:
                    modified_ShotSpread = (0.13f - ssAccuracyMod) * gunTechModMult;
                    modified_SightsEfficiency = 1f + gunTechModAdd;
                    modified_SwayFactor = 1.4f;
                    modified_Mass = Math.Min(original_Mass, 30);
                    modified_Bulk = Math.Min(1.5f * original_Mass, 20f);
                    modified_RecoilAmount = 2.3f * recoilTechMod;
                    break;
                case APCEConstants.gunKinds.precisionRifle:
                    modified_ShotSpread = (0.1f - ssAccuracyMod) * gunTechModMult;
                    modified_SightsEfficiency = 2.6f + gunTechModAdd;
                    modified_SwayFactor = 1.35f;
                    modified_Mass = Math.Min(original_Mass, 30);
                    modified_Bulk = Math.Min(2f * original_Mass, 20f);
                    break;
                case APCEConstants.gunKinds.ExplosiveLauncher:
                    modified_ShotSpread = 0.122f + (weaponThingDef.Verbs[0].ForcedMissRadius * 0.02f);
                    modified_SightsEfficiency = 1f + gunTechModAdd;
                    modified_SwayFactor = 1.8f;
                    modified_Mass = Math.Min(original_Mass, 30);
                    modified_Bulk = Math.Min(2f * original_Mass, 20f);
                    modified_RecoilAmount = 2.3f * recoilTechMod;
                    break;
                case APCEConstants.gunKinds.Turret:
                    modified_ShotSpread = (0.1f - ssAccuracyMod) * gunTechModMult;
                    modified_SightsEfficiency = 1f;
                    modified_SwayFactor = 1.5f;
                    modified_Mass = Math.Min(original_Mass, 30);
                    modified_Bulk = Math.Min(2f * original_Mass, 20f);
                    modified_RecoilAmount = 1f;
                    break;
                case APCEConstants.gunKinds.Grenade:
                    modified_SightsEfficiency = 1.00f;
                    modified_Bulk = 0.87f;
                    modified_Mass = 0.7f;
                    break;
                default:
                    modified_ShotSpread = modified_ShotSpread = (0.15f - ssAccuracyMod) * gunTechModMult; //somewhere between an SMG and assault rifle
                    modified_SightsEfficiency = 1f + gunTechModAdd;
                    modified_SwayFactor = 2.0f;
                    modified_Mass = Math.Min(original_Mass, 30);
                    modified_Bulk = Math.Min(2f * original_Mass, 20f);
                    modified_RecoilAmount = 1f;
                    break;
            }

            modified_WeaponToughness = DataHolderUtils.WeaponToughnessAutocalc(weaponThingDef, modified_Bulk);
            modified_WorkToMake = original_WorkToMake;
            modified_RangedWeaponCooldown = original_RangedWeaponCooldown;
        }
        public void PatchStatBases()
        {
            RemoveVanillaStatBases();

            DataHolderUtils.AddOrChangeStat(weaponThingDef.statBases, StatDefOf.Mass, modified_Mass);
            DataHolderUtils.AddOrChangeStat(weaponThingDef.statBases, CE_StatDefOf.Bulk, modified_Bulk);
            DataHolderUtils.AddOrChangeStat(weaponThingDef.statBases, StatDefOf.RangedWeapon_Cooldown, modified_RangedWeaponCooldown);
            DataHolderUtils.AddOrChangeStat(weaponThingDef.statBases, StatDefOf.WorkToMake, modified_WorkToMake);
            DataHolderUtils.AddOrChangeStat(weaponThingDef.statBases, CE_StatDefOf.SightsEfficiency, modified_SightsEfficiency);
            DataHolderUtils.AddOrChangeStat(weaponThingDef.statBases, CE_StatDefOf.ShotSpread, modified_ShotSpread);
            DataHolderUtils.AddOrChangeStat(weaponThingDef.statBases, CE_StatDefOf.SwayFactor, modified_SwayFactor);
            DataHolderUtils.AddOrChangeStat(weaponThingDef.statBases, CE_StatDefOf.BurstShotCount, modified_BurstShotCount);
            if (stuffed)
            {
                DataHolderUtils.AddOrChangeStat(weaponThingDef.statBases, CE_StatDefOf.StuffEffectMultiplierToughness, modified_WeaponToughness);
            }
            else
            {
                DataHolderUtils.AddOrChangeStat(weaponThingDef.statBases, CE_StatDefOf.ToughnessRating, modified_WeaponToughness);
            }
        }
        public void PatchComps()
        {
            if (weaponThingDef.comps == null)
            {
                weaponThingDef.comps = new List<CompProperties>();
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

            //remove existing CompProperties_AmmoUser
            weaponThingDef.comps.RemoveAll(c => c is CompProperties_AmmoUser);

            if (modified_usesAmmo)
            {
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
            }
            
            if (gunKind == APCEConstants.gunKinds.Mortar)
                return;

            weaponThingDef.comps.RemoveAll(c => c is CompProperties_FireModes);

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
            if (original_VerbProperties.verbClass == typeof(Verb_ShootBeam))
            {
                return;
            }

            VerbPropertiesCE newVerbPropsCE = new VerbPropertiesCE();
            DataHolderUtils.CopyFields(weaponThingDef.Verbs[0], newVerbPropsCE);

            newVerbPropsCE.ticksBetweenBurstShots = modified_TicksBetweenBurstShots;
            newVerbPropsCE.range = modified_Range;
            newVerbPropsCE.warmupTime = modified_WarmupTime;
            newVerbPropsCE.burstShotCount = modified_BurstShotCount;
            newVerbPropsCE.recoilPattern = modified_RecoilPattern;
            newVerbPropsCE.verbClass = modified_VerbClass;
            //newVerbPropsCE.ejectsCasings //TODO
            //newVerbPropsCE.indirectFirePenalty //TODO
            newVerbPropsCE.defaultProjectile = modified_AmmoSetDef.ammoTypes[0].projectile;

            weaponThingDef.Verbs[0] = newVerbPropsCE;
        }

        public void CalculateVerbPropValues()
        {
            //if verb doesn't need patching, early return
            if ((original_VerbProperties.verbClass == typeof(Verb_ShootCE)) || (original_VerbProperties.verbClass == typeof(Verb_ShootCEOneUse)) || original_VerbProperties.verbClass == typeof(Verb_ShootBeam))
            {
                return;
            }

            modified_TicksBetweenBurstShots = original_VerbProperties.ticksBetweenBurstShots;

            modified_Range = original_VerbProperties.range;

            //if warmupTime is too low, some weapons will get stuck permanently unable to fire, since it fires when the timer ticks from 1 to 0, not when it is AT 0
            modified_WarmupTime = original_VerbProperties.warmupTime;
            if (modified_WarmupTime < 0.07)
                modified_WarmupTime = 0.07f;

            //burst sizes are usually doubled, but need to account for single-shot weapons
            modified_BurstShotCount = original_BurstShotCount;
            if (modified_BurstShotCount != 1)
                modified_BurstShotCount *= 2;

            if (gunKind == APCEConstants.gunKinds.Turret || gunKind == APCEConstants.gunKinds.MachineGun)
                modified_RecoilPattern = RecoilPattern.Mounted;
            else
                modified_RecoilPattern = RecoilPattern.Regular;

            if (original_VerbProperties.verbClass == typeof(Verb_Shoot))
                modified_VerbClass = typeof(Verb_ShootCE);
            else if (original_VerbProperties.verbClass == typeof(Verb_LaunchProjectile)
                || (original_VerbProperties.verbClass == typeof(Verb_ShootOneUse)))
                modified_VerbClass = typeof(Verb_ShootCEOneUse);
            else if (original_VerbProperties.verbClass == typeof(Verb_SpewFire))
                modified_VerbClass = typeof(Verb_SpewFire);
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
            if (modified_BurstShotCount > 1)
                modified_aimedBurstShotCount = (int)(modified_BurstShotCount / 2);
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
                modified_magazineSize = modified_BurstShotCount * 10;
                modified_reloadTime = Mathf.Clamp(modified_magazineSize * 0.09f, 0.1f, 12f);
            }
            else
            {
                modified_magazineSize = modified_BurstShotCount * 5;
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
            //TODO - I think gun tools should not use techMult? Will weaken things with intended weapons like bayonets, but be better for most cases
            modified_ToolArmorPenetrationSharps[i] = Mathf.Clamp(modified_ToolArmorPenetrationSharps[i] * modData.weaponToolSharpPenetration, 0, 99999);
            modified_ToolArmorPenetrationBlunts[i] = Mathf.Clamp(modified_ToolArmorPenetrationBlunts[i] * modData.weaponToolBluntPenetration, 0, 99999);
        }

        public void CalculateMortar()
        {
            //statbases
            modified_SightsEfficiency = 0.5f;

            //comps
            modified_magazineSize = 1;
            modified_reloadTime = 5;
            modified_AmmoSetDef = APCEDefOf.AmmoSet_81mmMortarShell;

            //verb
            modified_VerbClass = typeof(Verb_ShootMortarCE);
            modified_WarmupTime = original_VerbProperties.warmupTime;
        }

        #region Grenade
        public void CalculateGrenade()
        {
            
            if (modified_ToolIds.NullOrEmpty())
            {
                modified_ToolIds.Add("APCE_Tool_" + weaponThingDef.defName);
                modified_ToolLabels.Add("Body");
                modified_ToolCapacityDefs.Add(new List<ToolCapacityDef>() { APCEDefOf.Blunt });
                modified_ToolLinkedBodyPartGroupDefs.Add(APCEDefOf.Base);
                modified_ToolCooldownTimes.Add(1.75f);
                modified_ToolArmorPenetrationSharps.Add(0f);
                modified_ToolArmorPenetrationBlunts.Add(1f);
                modified_ToolPowers.Add(2);
                modified_ToolChanceFactors.Add(1);
            }

            modified_stackLimit = 75;
            modified_recipeCount = 10;
            modified_grenadeDamage = modified_AmmoSetDef.ammoTypes[0].projectile.projectile.GetDamageAmount(1);
            modified_explosionRadius = modified_AmmoSetDef.ammoTypes[0].projectile.projectile.explosionRadius;
            //CompProperties_ExplosiveCE (for if the Thing is damaged)
            //CompProperties_Fragments 
            //todo
            //projectile
            //thingClass CombatExtended.ProjectileCE_Explosive
            //projectilepropsCE
            //make sure comps aren't null, add Fragments comp if necessary -- TODO, explosive launcher needs fragments as well
        }

        public void PatchGrenade()
        {
            //if (modified_ammoDef == null)
            //{
            //    modified_ammoDef = GenerateGrenadeAmmoDef();
            //}
            //DataHolderUtils.AddCompReplaceMe(weaponThingDef, modified_ammoDef);

            //bool hasRecipe = DataHolderUtils.ReplaceRecipes(weaponThingDef, modified_ammoDef, modified_recipeCount);
            //if (APCESettings.printLogs)
            //{
            //    Log.Message("ThingDef " + weaponThingDef.defName + " classified as a grenade, found a recipe to modify: " + hasRecipe.ToString());
            //}

            weaponThingDef.thingClass = typeof(AmmoThing);

            //remove old CompProperties_Explosive, as wel as CE version so duplicates don't get added if patch is rerun
            weaponThingDef.comps.RemoveAll(c => c is CompProperties_Explosive || c is CompProperties_ExplosiveCE);
            CompProperties_ExplosiveCE newComp_ExCE = new CompProperties_ExplosiveCE()
            {
                damageAmountBase = modified_grenadeDamage,
                explosiveDamageType = original_VerbProperties.defaultProjectile.projectile.damageDef,
                explosiveRadius = modified_explosionRadius
            };
            weaponThingDef.comps.Add(newComp_ExCE);
            //TODO comp fragments

            return;
        }

        //TODO re-implement
        public AmmoDef GenerateGrenadeAmmoDef()
        {
            AmmoDef ammoGrenade = new AmmoDef();
            DataHolderUtils.CopyFields(weaponThingDef, ammoGrenade);

            ammoGrenade.thingClass = typeof(AmmoThing);
            ammoGrenade.graphicData.graphicClass = typeof(Graphic_Multi);
            ammoGrenade.graphicData.onGroundRandomRotateAngle = 0;

            //make new tag lists so I can .Clear() the ones on the ThingDef version
            // this will remove the ThingDef version of the grenade from most traders' stock, unless the mod has a custom trader with the ThingDef explicitly added as stock -- TODO search/remove
            if (!weaponThingDef.tradeTags.NullOrEmpty())
            {
                List<string> newTradeTags = new List<string>(weaponThingDef.tradeTags);
                ammoGrenade.tradeTags = newTradeTags;
                weaponThingDef.tradeTags.Clear();
            }
            
            // this will hopefully prevent Pawns from spawning with the ThingDef version
            if (!weaponThingDef.weaponTags.NullOrEmpty())
            {
                List<string> newWeaponTags = new List<string>(weaponThingDef.weaponTags);
                ammoGrenade.weaponTags = newWeaponTags;
                weaponThingDef.weaponTags.Clear();
            }
            else
            {
                ammoGrenade.weaponTags = new List<string>();
            }

            //make a new list of comps so the CompReplaceMe isn't added to the new def
            ammoGrenade.comps = new List<CompProperties>();
            foreach (CompProperties comp in weaponThingDef.comps)
            {
                ammoGrenade.comps.Add(comp);
            }

            ammoGrenade.weaponTags.Add("CE_AI_Grenade");
            ammoGrenade.weaponTags.Add("CE_AI_AOE"); // TODO might need to make these conditional, if I end up re-using code for non-explosive thrown weapons
            ammoGrenade.weaponTags.Add("CE_OneHandedWeapon");
            weaponThingDef.generateAllowChance = 0;
            weaponThingDef.generateCommonality = 0;

            //InjectedDefHasher.GiveShortHashToDef(ammoGrenade, typeof(ThingDef));
            //DefGenerator.AddImpliedDef<ThingDef>(ammoGrenade);
            return ammoGrenade;
        }
        
        #endregion

        public void GenerateAmmoSet()
        {
            ammoSetDataHolder = new DefDataHolderAmmoSet(weaponThingDef, gunKind);
            //RegisterSelfInDict, GetOriginalData, and Autocalculate are called by constructor
            this.modified_AmmoSetDef = ammoSetDataHolder.GeneratedAmmoSetDef;
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
