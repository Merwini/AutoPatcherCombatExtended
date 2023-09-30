using CombatExtended;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using Verse;
using UnityEngine;

namespace nuff.AutoPatcherCombatExtended
{
    partial class APCEController
    {
        public static void BasicException(Exception ex)
        {
            Log.Error(ex.ToString());
        }

        public static void DataHolderTypeException(Exception ex)
        {
            //TODO
        }

        public static List<ModContentPack> GetActiveModsList()
        {
            List<ModContentPack> activeMods = new List<ModContentPack>(LoadedModManager.RunningMods.Where(mod => !mod.IsOfficialMod
                                                                                                              && !(mod.AllDefs.Count() == 0)
                                                                                                              && !(mod.PackageId == "ceteam.combatextended")
                                                                                                              && !(mod.PackageId == "nuff.ceautopatcher")
                                                                                                              ).OrderBy(mod => mod.Name).ToList());
            return activeMods;
        }

        public static List<ModContentPack> RebuildModsToPatch()
        {
            Dictionary<string, ModContentPack> modDict = new Dictionary<string, ModContentPack>();
            List<ModContentPack> modsToPatch = new List<ModContentPack>();

            foreach (ModContentPack mod in APCESettings.activeMods)
            {
                modDict[mod.PackageId] = mod;
            }

            for (int i = APCESettings.modsByPackageId.Count - 1; i >= 0; i--)
            {
                string packageId = APCESettings.modsByPackageId[i];
                if (modDict.TryGetValue(packageId, out ModContentPack mod) && mod != null)
                {
                    modsToPatch.Add(mod);
                }
                else
                {
                    APCESettings.modsByPackageId.RemoveAt(i);
                }
            }
            APCESettings.thisMod = modDict.TryGetValue("nuff.ceautopatcher");
            
            return modsToPatch;
        }

        public static void CleanModList(List<ModContentPack> modList)
        {
            foreach (ModContentPack mod in modList)
            {
                //I know these could be combined into one check, but it's easier this way
                if (mod == null)
                {
                    modList.Remove(mod);
                    return;
                }
                if (mod.AllDefs == null || mod.AllDefs.Count() == 0)
                {
                    Log.Warning($"Mod named \"{mod.Name}\" has no defs to patch. Removing from the list.");
                    modList.Remove(mod);
                    return;
                }
            }
        }


        public static void PatchAllTools(ref List<Tool> tools, bool isPawn, TechLevel techLevel = TechLevel.Undefined)
        {
            if ((tools == null) || (tools.Count == 0))
                    return;

            List<Tool> newToolsCE = new List<Tool>();
            foreach (Tool tool in tools)
            {
                newToolsCE.Add(PatchTool(tool, isPawn, techLevel));
            }
            tools = newToolsCE;
        }

        public static ToolCE PatchTool(Tool tool, bool isPawn, TechLevel techLevel = TechLevel.Undefined)
        {
            ToolCE newToolCE = new ToolCE();

            CopyFields(tool, newToolCE);
            newToolCE.id = "APCE_Tool_" + tool.id;

            float toolTechMult = 1f;
            switch (techLevel)
            {
                case TechLevel.Animal:
                    toolTechMult *= APCESettings.weaponToolTechMultAnimal;
                    break;
                case TechLevel.Neolithic:
                    toolTechMult *= APCESettings.weaponToolTechMultNeolithic;
                    break;
                case TechLevel.Medieval:
                    toolTechMult *= APCESettings.weaponToolTechMultMedieval;
                    break;
                case TechLevel.Industrial:
                    toolTechMult *= APCESettings.weaponToolTechMultIndustrial;
                    break;
                case TechLevel.Spacer:
                    toolTechMult *= APCESettings.weaponToolTechMultSpacer;
                    break;
                case TechLevel.Ultra:
                    toolTechMult *= APCESettings.weaponToolTechMultUltratech;
                    break;
                case TechLevel.Archotech:
                    toolTechMult *= APCESettings.weaponToolTechMultArchotech;
                    break;
                default:
                    break;
            }

            if (tool.armorPenetration <= 0) //CE is far more punishing if you have no armor penetration than vanilla is, so it is essential to have some
            {
                newToolCE.armorPenetrationSharp = tool.power * 0.1f;
                newToolCE.armorPenetrationBlunt = tool.power * 0.1f;
            }
            else
            {
                newToolCE.armorPenetrationSharp = tool.armorPenetration;
                newToolCE.armorPenetrationBlunt = tool.armorPenetration;
            }

            if (isPawn)
            {
                newToolCE.armorPenetrationSharp *= APCESettings.pawnToolSharpPenetration;
                newToolCE.armorPenetrationBlunt *= APCESettings.pawnToolBluntPenetration;
                newToolCE.power = tool.power * APCESettings.pawnToolPowerMult;
            }
            else
            {
                newToolCE.armorPenetrationSharp *= APCESettings.weaponToolSharpPenetration * toolTechMult;
                newToolCE.armorPenetrationBlunt *= APCESettings.weaponToolBluntPenetration * toolTechMult;
                newToolCE.power = tool.power * APCESettings.weaponToolPowerMult;
            }

            return newToolCE;
        }

        public static void PatchAllVerbs(ThingDef def)
        {
            List<VerbPropertiesCE> newVerbsCE = new List<VerbPropertiesCE>();
            foreach (VerbProperties vp in def.Verbs)
            {
                newVerbsCE.Add(PatchVerb(vp));
            }

            for (int i = 0; i < newVerbsCE.Count; i++)
            {
                def.Verbs[i] = newVerbsCE[i];
            }
        }

        public static VerbPropertiesCE PatchVerb(VerbProperties vp)
        {
            try
            {
                if ((vp.verbClass == typeof(Verb_ShootCE)) || (vp.verbClass == typeof(Verb_ShootCEOneUse)) || vp.verbClass == typeof(Verb_ShootBeam))
                {
                    return vp as VerbPropertiesCE;
                }
                VerbPropertiesCE newVPCE = new VerbPropertiesCE();
                CopyFields(vp, newVPCE);
                /*
                newVPCE.label = vp.label;
                newVPCE.soundCast = vp.soundCast;
                newVPCE.soundCastTail = vp.soundCastTail;
                newVPCE.soundAiming = vp.soundAiming;
                newVPCE.muzzleFlashScale = vp.muzzleFlashScale;
                newVPCE.hasStandardCommand = vp.hasStandardCommand;
                newVPCE.range = vp.range;
                newVPCE.ticksBetweenBurstShots = vp.ticksBetweenBurstShots;
                
                if (vp.warmupTime >= 0.07f)
                {
                    //This took so long to debug why a turret would fire once then stop. 
                    //Turns out that a turret fires when burstWarmupTicksLeft ticks down to 0, and a warmupTime lower than 0.07 causes it to get stuck at 0, unable to tick down. 
                    newVPCE.warmupTime = vp.warmupTime;
                }
                else
                {
                    newVPCE.warmupTime = 0.1f;
                }
                newVPCE.targetParams = vp.targetParams;
                newVPCE.rangedFireRulepack = vp.rangedFireRulepack;
                newVPCE.ai_IsBuildingDestroyer = vp.ai_IsBuildingDestroyer;
                newVPCE.ai_AvoidFriendlyFireRadius = vp.ai_AvoidFriendlyFireRadius;
                newVPCE.onlyManualCast = vp.onlyManualCast;
                newVPCE.stopBurstWithoutLos = vp.stopBurstWithoutLos;
                if (vp.burstShotCount != 1)
                {
                    newVPCE.burstShotCount = vp.burstShotCount * 2;
                }
                else
                {
                    newVPCE.burstShotCount = vp.burstShotCount;
                }
                newVPCE.defaultProjectile = vp.defaultProjectile;
                */

                //if warmupTime is too low, some weapons will get stuck in a nonfiring state since they are only able to fire when a certain counter ticks down to 0, and low numbers get divided and rounded down to 0
                if (newVPCE.warmupTime < 0.07)
                {
                    newVPCE.warmupTime = 0.1f;
                }
                if (newVPCE.burstShotCount != 1)
                {
                    newVPCE.burstShotCount *= 2;
                }

                

                if (vp.verbClass == typeof(Verb_Shoot))
                {
                    newVPCE.verbClass = typeof(CombatExtended.Verb_ShootCE);
                    /*
                    //these are done just in case. weapon should not ever use defaultProjectile.
                    newVPCE.defaultProjectile.thingClass = typeof(CombatExtended.BulletCE);
                    newVPCE.defaultProjectile.projectile = ConvertPP(newVPCE.defaultProjectile.projectile);
                    */
                }
                else if (vp.verbClass == typeof(Verb_LaunchProjectile) || (vp.verbClass == typeof(Verb_ShootOneUse)))
                {
                    newVPCE.verbClass = typeof(Verb_ShootCEOneUse);
                    /*
                    newVPCE.defaultProjectile.thingClass = typeof(CombatExtended.ProjectileCE_Explosive);
                    newVPCE.defaultProjectile.projectile = ConvertPP(newVPCE.defaultProjectile.projectile);
                    */
                }
                else
                {
                    if (APCESettings.patchCustomVerbs)
                    {
                        newVPCE.verbClass = typeof(CombatExtended.Verb_ShootCE);
                        /*
                        newVPCE.defaultProjectile.thingClass = typeof(CombatExtended.BulletCE);
                        newVPCE.defaultProjectile.projectile = ConvertPP(newVPCE.defaultProjectile.projectile);
                        */
                    }
                    else
                    {
                        throw new Exception($"Unable to patch verb {vp.label} due to unrecognized verbClass: {vp.verbClass}");
                    }
                }
                return newVPCE;
            }

            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static ProjectilePropertiesCE ConvertPP(ProjectileProperties ppHolder)
        {
            ProjectilePropertiesCE ppceHolder = new ProjectilePropertiesCE();
            CopyFields(ppHolder, ppceHolder);
            /*
            ppceHolder.speed = ppHolder.speed;
            ppceHolder.ai_IsIncendiary = ppHolder.ai_IsIncendiary;
            ppceHolder.explosionEffect = ppHolder.explosionEffect;
            ppceHolder.explosionDamageFalloff = ppHolder.explosionDamageFalloff;
            ppceHolder.explosionChanceToStartFire = ppHolder.explosionChanceToStartFire;
            ppceHolder.explosionRadius = ppHolder.explosionRadius;
            ppceHolder.applyDamageToExplosionCellsNeighbors = ppHolder.applyDamageToExplosionCellsNeighbors;
            ppceHolder.postExplosionSpawnThingCount = ppHolder.postExplosionSpawnThingCount;
            ppceHolder.postExplosionSpawnChance = ppHolder.postExplosionSpawnChance;
            ppceHolder.postExplosionSpawnThingDef = ppHolder.postExplosionSpawnThingDef;
            ppceHolder.preExplosionSpawnChance = ppHolder.preExplosionSpawnChance;
            ppceHolder.preExplosionSpawnThingDef = ppHolder.preExplosionSpawnThingDef;
            ppceHolder.preExplosionSpawnThingCount = ppHolder.preExplosionSpawnThingCount;
            ppceHolder.explosionDelay = ppHolder.explosionDelay;
            ppceHolder.soundAmbient = ppHolder.soundAmbient;
            ppceHolder.flyOverhead = ppHolder.flyOverhead;
            ppceHolder.explosionRadius = ppHolder.explosionRadius;
            ppceHolder.damageDef = ppHolder.damageDef;
            ppceHolder.stoppingPower = ppHolder.stoppingPower;
            ppceHolder.alwaysFreeIntercept = ppHolder.alwaysFreeIntercept;
            ppceHolder.shadowSize = ppHolder.shadowSize;
            ppceHolder.soundHitThickRoof = ppHolder.soundHitThickRoof;
            ppceHolder.soundExplode = ppHolder.soundExplode;
            ppceHolder.soundImpactAnticipate = ppHolder.soundImpactAnticipate;
            ppceHolder.arcHeightFactor = ppHolder.arcHeightFactor;
            */
            ppceHolder.armorPenetrationBlunt = 1;
            ppceHolder.armorPenetrationSharp = 1;
            SetDamage(ppceHolder, ppHolder.GetDamageAmount(1));
            ppceHolder.secondaryDamage = ExtraToSecondary(ppHolder.extraDamages);
            return ppceHolder;
        }

        public static List<SecondaryDamage> ExtraToSecondary(List<ExtraDamage> ed)
        {
            List<SecondaryDamage> newSDList = new List<SecondaryDamage>();
            if (!(ed == null) && !(ed.Count == 0))
            {
                for (int i = 0; i < ed.Count; i++)
                {
                    SecondaryDamage newSD = new SecondaryDamage();
                    newSD.def = ed[i].def;
                    newSD.amount = (int)(ed[i].amount + 0.5f); //extra 0.5f is because casting to int is a floor round
                    newSD.chance = ed[i].chance;
                    newSDList.Add(newSD);
                }
            }

            return newSDList;
        }

        public static void SetDamage(ProjectilePropertiesCE newPPCE, int damage)
        {
            //experimental reflection attempt
            Type tpp = typeof(ProjectileProperties);
            FieldInfo dab = tpp.GetField("damageAmountBase", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            dab.SetValue(newPPCE, (int)damage);
        }

        public static List<StatModifier> PatchStatBases(ThingDef def, APCEConstants.gunKinds gunKind)
        {
            List<StatModifier> newStatBases = new List<StatModifier>();
            foreach (string statMod in Enum.GetNames(typeof(APCEConstants.SharedStatBases)))
            {
                int index = def.statBases.FindIndex(sm => sm.stat.ToString().EqualsIgnoreCase(statMod));
                if (index < 0)
                {
                    continue;
                }
                else
                {
                    StatModifier newStatMod = new StatModifier();
                    newStatMod.stat = StatDef.Named(statMod);
                    newStatMod.value = def.statBases[index].value;
                    newStatBases.Add(newStatMod);
                    continue;
                }
            }

            //sights efficiency, float, ex: bows 0.6, small industrial arms 0.7, assault rifle 1.0, sniper rifle 3.5, charge rifle 1.1, charge smg 1.1, positive correlation to accuracy, tech level seems to be a factor, 
            StatModifier sightsEfficiency = new StatModifier();
            sightsEfficiency.stat = CE_StatDefOf.SightsEfficiency;

            //ShotSpread: float, ex: charge rifle 0.12, FAL 0.06, sniper rifles 0.02-0.04, negatively correlates to accuracy
            StatModifier shotSpread = new StatModifier();
            shotSpread.stat = CE_StatDefOf.ShotSpread;

            StatModifier swayFactor = new StatModifier();
            swayFactor.stat = CE_StatDefOf.SwayFactor;

            //Bulk: float, for "1 handed" guns seems to be = mass, for "2 handed" = 2 * mass
            StatModifier gunBulk = new StatModifier();
            gunBulk.stat = CE_StatDefOf.Bulk;

            //this is not actually used? Need to refactor code to not use this, but still get a value into the verb
            StatModifier recoil = new StatModifier();
            recoil.stat = CE_StatDefOf.Recoil;

            float gunTechModFlat = (def.techLevel.CompareTo(TechLevel.Industrial) * 0.1f);
            float gunTechModPercent = (1 - gunTechModFlat);
            float accuracyLong = def.statBases.GetStatValueFromList(StatDefOf.AccuracyLong, 0.5f);
            float ssAccuracyMod = (accuracyLong * 0.1f); 
            //float ssAccuracyMod = 0.1f;
            float seDefault = 1f + gunTechModFlat;
            float recoilTechMod = (1 - (((float)def.techLevel - 3) * 0.2f));
            LimitWeaponMass(def);
            float gunMass = def.statBases.GetStatValueFromList(StatDefOf.Mass, 1);
            float forcedMissRadius = def.Verbs[0].ForcedMissRadius;

            switch (gunKind)
            {
                case APCEConstants.gunKinds.Bow:
                    sightsEfficiency.value = 0.6f;
                    shotSpread.value = 1f;
                    swayFactor.value = 2f;
                    gunBulk.value = 2f * gunMass;
                    //reloadTime.value = 1f;
                    break;
                case APCEConstants.gunKinds.Handgun:
                    shotSpread.value = (0.2f - ssAccuracyMod) * gunTechModPercent;
                    sightsEfficiency.value = 0.7f + gunTechModFlat;
                    swayFactor.value = 1f;
                    gunBulk.value = 1f * gunMass;
                    break;
                case APCEConstants.gunKinds.SMG:
                    shotSpread.value = (0.17f - ssAccuracyMod) * gunTechModPercent;
                    sightsEfficiency.value = 0.7f + gunTechModFlat;
                    swayFactor.value = 2f;
                    gunBulk.value = 1f * gunMass;
                    recoil.value = 2f  * recoilTechMod;
                    break;
                case APCEConstants.gunKinds.Shotgun:
                    shotSpread.value = (0.17f - ssAccuracyMod) * gunTechModPercent;
                    sightsEfficiency.value = seDefault;
                    swayFactor.value = 1.2f;
                    gunBulk.value = 2f * gunMass;
                    break;
                case APCEConstants.gunKinds.assaultRifle:
                    shotSpread.value = (0.13f - ssAccuracyMod) * gunTechModPercent;
                    sightsEfficiency.value = seDefault;
                    swayFactor.value = 1.33f;
                    gunBulk.value = 2f * gunMass;
                    recoil.value = 1.8f * recoilTechMod;
                    break;
                case APCEConstants.gunKinds.MachineGun:
                    shotSpread.value = (0.13f - ssAccuracyMod) * gunTechModPercent;
                    sightsEfficiency.value = seDefault;
                    swayFactor.value = 1.4f;
                    gunBulk.value = 1.5f * gunMass;
                    recoil.value = 2.3f * recoilTechMod;
                    break;
                case APCEConstants.gunKinds.precisionRifle:
                    shotSpread.value = (0.1f - ssAccuracyMod) * gunTechModPercent;
                    sightsEfficiency.value = 2.6f + gunTechModFlat;
                    swayFactor.value = 1.35f;
                    gunBulk.value = 2f * gunMass;
                    break;
                case APCEConstants.gunKinds.ExplosiveLauncher:
                    shotSpread.value = 0.122f + (forcedMissRadius * 0.02f);
                    sightsEfficiency.value = seDefault;
                    swayFactor.value = 1.8f;
                    gunBulk.value = 2f * gunMass;
                    recoil.value = 2.3f * recoilTechMod;
                    break;
                case APCEConstants.gunKinds.Turret:
                    shotSpread.value = (0.1f - ssAccuracyMod) * gunTechModPercent;
                    sightsEfficiency.value = seDefault;
                    swayFactor.value = 1.5f;
                    gunBulk.value = 2f * gunMass;
                    recoil.value = 1f;
                    break;
                case APCEConstants.gunKinds.Grenade:
                    sightsEfficiency.value = 0.65f;
                    break;
                default:
                    shotSpread.value = shotSpread.value = (0.15f - ssAccuracyMod) * gunTechModPercent; //somewhere between an SMG and assault rifle
                    sightsEfficiency.value = seDefault;
                    swayFactor.value = 2.0f;
                    gunBulk.value = 2f * gunMass;
                    recoil.value = 1f;
                    break;
            }

            //TicksBetweenBurstShots is part of the verb_shoot in xml, but somehow ends up in statbases? rimworld is confusing. int ex: 4 for LMGs, 10 for AR, 12 for CR
            StatModifier ticksBBS = new StatModifier();
            ticksBBS.stat = CE_StatDefOf.TicksBetweenBurstShots;
            ticksBBS.value = def.Verbs[0].ticksBetweenBurstShots;

            //BurstShotCount as above. int, ex: AR 3, LMG 10, pistols 1
            StatModifier burstShotCount = new StatModifier();
            burstShotCount.stat = CE_StatDefOf.BurstShotCount;
            if (def.Verbs[0].burstShotCount == 1)
                burstShotCount.value = 1;
            else
                burstShotCount.value = 2 * def.Verbs[0].burstShotCount;

            if (gunKind != APCEConstants.gunKinds.Grenade)
            {
                newStatBases.Add(shotSpread);
                newStatBases.Add(swayFactor);
            }
            newStatBases.Add(sightsEfficiency);
            newStatBases.Add(gunBulk);
            newStatBases.Add(ticksBBS);
            newStatBases.Add(burstShotCount);
            newStatBases.Add(recoil);

            return newStatBases;
        }

        public static void AddCompProperties_AmmoUser(ThingDef weapon, APCEConstants.gunKinds gunKind)
        {
            CombatExtended.CompProperties_AmmoUser newAUComp = new CombatExtended.CompProperties_AmmoUser();

            if (!(gunKind == APCEConstants.gunKinds.Bow) && !(gunKind == APCEConstants.gunKinds.Mortar) && !(gunKind == APCEConstants.gunKinds.MachineGun))
            {
                newAUComp.magazineSize = weapon.Verbs[0].burstShotCount * 5;
                newAUComp.reloadTime = 4f; 
                newAUComp.throwMote = true;
            }
            else if (gunKind == APCEConstants.gunKinds.MachineGun)
            {
                newAUComp.magazineSize = weapon.Verbs[0].burstShotCount * 10;
                newAUComp.reloadTime = Mathf.Clamp(newAUComp.magazineSize * 0.09f, 0.1f, 12f);
                newAUComp.throwMote = true;
            }
            else if (gunKind == APCEConstants.gunKinds.Bow)
            {
                newAUComp.magazineSize = 1;
                newAUComp.reloadTime = 1f;
                newAUComp.throwMote = false;
            }
            else if (gunKind == APCEConstants.gunKinds.Mortar)
            {
                newAUComp.magazineSize = 1;
                newAUComp.reloadTime = 5f;
                newAUComp.throwMote = false;
            }

            if (!(gunKind == APCEConstants.gunKinds.Mortar))
            {
                newAUComp.ammoSet = GenerateAmmoSet(weapon, gunKind);
            }
            else
            {
                newAUComp.ammoSet = APCEDefOf.AmmoSet_81mmMortarShell;
            }

            newAUComp.reloadOneAtATime = false; //TODO heuristic?
            newAUComp.loadedAmmoBulkFactor = 0;
            newAUComp.compClass = typeof(CombatExtended.CompAmmoUser);
            weapon.comps.Add(newAUComp);
        }

        public static void AddCompProperties_FireModes(ThingDef weapon, APCEConstants.gunKinds gunKind)
        {
            CombatExtended.CompProperties_FireModes newFMComp = new CombatExtended.CompProperties_FireModes();
            if (weapon.Verbs[0].burstShotCount > 1)
            {
                newFMComp.aimedBurstShotCount = (int)(weapon.Verbs[0].burstShotCount / 2);
            }
            else
            {
                newFMComp.aimedBurstShotCount = 1;
            }
            if (!(gunKind == APCEConstants.gunKinds.Turret))
            {
                newFMComp.aiUseBurstMode = true;
                newFMComp.noSingleShot = false;
                newFMComp.noSnapshot = false;
                newFMComp.aiAimMode = AimMode.Snapshot;
            }
            else
            {
                newFMComp.aiUseBurstMode = false;
                newFMComp.noSingleShot = true;
                newFMComp.noSnapshot = true;
                newFMComp.aiAimMode = AimMode.AimedShot;
            }
            
            weapon.comps.Add(newFMComp);
        }

        public static void PatchBaseBullet(ThingDef bullet)
        {
            bullet.category = ThingCategory.Projectile;
            bullet.tickerType = TickerType.Normal;
            bullet.altitudeLayer = AltitudeLayer.Projectile;
            bullet.thingClass = typeof(CombatExtended.BulletCE);
            bullet.useHitPoints = false;
            bullet.neverMultiSelect = true;
            if (bullet.graphicData != null)
            {
                bullet.graphicData.shaderType = ShaderTypeDefOf.Transparent;
                bullet.graphicData.graphicClass = typeof(Graphic_Single);
            }
            
        }

        public static void CopyFields(object source, object destination)
        {
            if (source == null || destination == null)
            {
                return;
            }
            Type sourceType = source.GetType();
            Type destType = destination.GetType();

            foreach (FieldInfo sourceField in sourceType.GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                FieldInfo destField = destType.GetField(sourceField.Name, BindingFlags.Public | BindingFlags.Instance);
                if (destField != null && destField.FieldType == sourceField.FieldType)
                {
                    object value = sourceField.GetValue(source);
                    if (destField != null)
                    {
                        destField.SetValue(destination, value);
                    }
                }
            }
        }

        public static void RemoveListDuplicates(List<string> list)
        {
            HashSet<string> uniqueItems = new HashSet<string>();
            for (int i = list.Count - 1; i >= 0; i--)
            {
                if (!uniqueItems.Add(list[i]))
                {
                    list.RemoveAt(i);
                }
            }
        }

        public static void DisableGenericAmmos()
        {
            List<AmmoDef> genericAmmos = MakeGenericAmmoList();
            foreach (AmmoDef ammo in genericAmmos)
            {
                ammo.generateCommonality = 0;
                ammo.tradeability = Tradeability.None;
                ammo.generateAllowChance = 0;
            }
        }

        public static List<AmmoDef> MakeGenericAmmoList()
        {
            List<AmmoDef> genericAmmos = new List<AmmoDef>
            {
                APCEDefOf.Ammo_APCEGeneric_FMJ,
                APCEDefOf.Ammo_APCEGeneric_AP,
                APCEDefOf.Ammo_APCEGeneric_HP,
                APCEDefOf.Ammo_APCEGeneric_Incendiary,
                APCEDefOf.Ammo_APCEGeneric_HE,
                APCEDefOf.Ammo_APCEGeneric_Sabot,

                APCEDefOf.Ammo_APCEGeneric_Buck,
                APCEDefOf.Ammo_APCEGeneric_Slug,
                APCEDefOf.Ammo_APCEGeneric_Beanbag,
                APCEDefOf.Ammo_APCEGeneric_ElectroSlug,

                APCEDefOf.Ammo_APCEGeneric_Charged,
                APCEDefOf.Ammo_APCEGeneric_ChargedAP,
                APCEDefOf.Ammo_APCEGeneric_ChargedIon,

                APCEDefOf.Ammo_APCELauncher_Incendiary,
                APCEDefOf.Ammo_APCELauncher_Thermobaric,
                APCEDefOf.Ammo_APCELauncher_Foam
            };

            return genericAmmos;
        }
        public static void LimitWeaponMass(ThingDef td)
        {
            if (APCESettings.limitWeaponMass)
            {
                StatModifier mass = td.statBases.FirstOrDefault(x => x.stat == StatDefOf.Mass);
                if (mass != null)
                {
                    if (mass.value > APCESettings.maximumWeaponMass)
                    {
                        mass.value = APCESettings.maximumWeaponMass;
                    }
                }
            }
        }



        public static void HandleUnknownDef(Def def, APCEPatchLogger log)
        {
            //log.PatchFailed(def.defName, new Exception("Unrecognized def type"));
            return;
        }


    }

}