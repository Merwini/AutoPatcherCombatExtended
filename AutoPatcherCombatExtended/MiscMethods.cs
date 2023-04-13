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
        public void BasicException(Exception ex)
        {
            Log.Error(ex.ToString());
            //TODO more
        }

        public static List<ModContentPack> GetActiveModsList()
        {
            List<ModContentPack> activeMods = new List<ModContentPack>(LoadedModManager.RunningMods.Where(mod => !mod.IsOfficialMod).OrderBy(mod => mod.Name).ToList());
            return activeMods;
        }

        public static List<ModContentPack> RebuildModsToPatch()
        {
            List<ModContentPack> modsToPatch = new List<ModContentPack>();
            foreach (ModContentPack mod in APCESettings.activeMods)
            {
                if (APCESettings.modsByPackageId.Contains(mod.PackageId))
                {
                    modsToPatch.Add(mod);
                }
            }
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

        private static void PatchAllTools(ThingDef def)
        {
            if (def.tools == null)
                return;

            List<Tool> newToolsCE = new List<Tool>();
            foreach (Tool tool in def.tools)
            {
                newToolsCE.Add(PatchTool(tool, !def.IsWeapon));
            }
            def.tools = newToolsCE;
        }

        public static ToolCE PatchTool(Tool tool, bool isPawn)
        {
            ToolCE newToolCE = new ToolCE();
            newToolCE.label = tool.label;
            newToolCE.capacities = tool.capacities;
            newToolCE.power = tool.power * APCESettings.pawnToolPowerMult;
            newToolCE.cooldownTime = tool.cooldownTime;
            newToolCE.linkedBodyPartsGroup = tool.linkedBodyPartsGroup;
            if (tool.armorPenetration >= 0)
            {
                if (isPawn)
                {
                    newToolCE.armorPenetrationSharp = tool.armorPenetration * APCESettings.pawnToolSharpPenetration;
                    newToolCE.armorPenetrationBlunt = tool.armorPenetration * APCESettings.pawnToolBluntPenetration;
                }
                else
                {
                    newToolCE.armorPenetrationSharp = tool.armorPenetration * APCESettings.weaponToolSharpPenetration;
                    newToolCE.armorPenetrationBlunt = tool.armorPenetration * APCESettings.weaponToolBluntPenetration;
                }
                
            }
            return newToolCE;
        }

        public static ProjectileCE PatchProjectile(Projectile proj)
        {
            //TODO
            return null;
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
                VerbPropertiesCE newVPCE = new VerbPropertiesCE();
                if ((vp.verbClass.ToString().Equals("Verse.Verb_Shoot")) || (vp.verbClass.ToString().Equals("Verse.Verb_ShootOneUse")) || vp.verbClass.ToString().Equals("Verse.Verb_LaunchProjectile")) //todo make this only for verbshoot once the other verbs are implemented
                {
                    newVPCE.verbClass = typeof(CombatExtended.Verb_ShootCE);
                    newVPCE.label = vp.label;
                    newVPCE.soundCast = vp.soundCast;
                    newVPCE.soundCastTail = vp.soundCastTail;
                    newVPCE.soundAiming = vp.soundAiming;
                    newVPCE.muzzleFlashScale = vp.muzzleFlashScale;
                    newVPCE.hasStandardCommand = vp.hasStandardCommand;
                    newVPCE.range = vp.range;
                    newVPCE.ticksBetweenBurstShots = vp.ticksBetweenBurstShots;
                    newVPCE.warmupTime = vp.warmupTime;
                    newVPCE.targetParams = vp.targetParams;
                    newVPCE.rangedFireRulepack = vp.rangedFireRulepack;
                    newVPCE.ai_IsBuildingDestroyer = vp.ai_IsBuildingDestroyer;
                    newVPCE.ai_AvoidFriendlyFireRadius = vp.ai_AvoidFriendlyFireRadius;
                    newVPCE.onlyManualCast = vp.onlyManualCast;
                    newVPCE.stopBurstWithoutLos = vp.stopBurstWithoutLos;
                    //newVPCE.ForcedMissRadius = vp.ForcedMissRadius; //not used by Verb_ShootCE
                    newVPCE.burstShotCount = vp.burstShotCount * 2;
                    newVPCE.defaultProjectile = vp.defaultProjectile;
                    newVPCE.defaultProjectile.thingClass = typeof(CombatExtended.BulletCE);
                    newVPCE.defaultProjectile.projectile = ConvertPP(newVPCE.defaultProjectile.projectile);
                    return newVPCE;
                }
                else
                {
                    return null;
                }
            }

            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static ProjectilePropertiesCE ConvertPP(ProjectileProperties ppHolder)
        {
            ProjectilePropertiesCE ppceHolder = new ProjectilePropertiesCE();

            ppceHolder.speed = ppHolder.speed;
            ppceHolder.ai_IsIncendiary = ppHolder.ai_IsIncendiary;
            ppceHolder.explosionEffect = ppHolder.explosionEffect;
            ppceHolder.explosionDamageFalloff = ppHolder.explosionDamageFalloff;
            ppceHolder.explosionChanceToStartFire = ppHolder.explosionChanceToStartFire;
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
            ppceHolder.damageDef = ppHolder.damageDef; // TODO DamageDefOf.Stun seems to not actually injure the target, just stun them. might need to account for that
            ppceHolder.stoppingPower = ppHolder.stoppingPower;
            ppceHolder.alwaysFreeIntercept = ppHolder.alwaysFreeIntercept;
            ppceHolder.shadowSize = ppHolder.shadowSize;
            ppceHolder.soundHitThickRoof = ppHolder.soundHitThickRoof;
            ppceHolder.soundExplode = ppHolder.soundExplode;
            ppceHolder.soundImpactAnticipate = ppHolder.soundImpactAnticipate;
            ppceHolder.arcHeightFactor = ppHolder.arcHeightFactor;
            ppceHolder.armorPenetrationBlunt = 1;
            ppceHolder.armorPenetrationSharp = 1; //TODO maybe change these? only applicable if default projectile is used, which it never is
            SetDamage(ppceHolder, ppHolder.GetDamageAmount(1f, null));
            ppceHolder.secondaryDamage = ExtraToSecondary(ppHolder.extraDamages);
            return ppceHolder;
        }

        internal static List<SecondaryDamage> ExtraToSecondary(List<ExtraDamage> ed)
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

        internal static void SetDamage(ProjectilePropertiesCE newPPCE, int damage)
        {
            //experimental reflection attempt
            Type tpp = typeof(ProjectileProperties);
            FieldInfo dab = tpp.GetField("damageAmountBase", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            dab.SetValue(newPPCE, (int)damage);
        }

        internal static List<StatModifier> PatchStatBases(ThingDef def, APCESettings.gunKinds gunKind)
        {
            List<StatModifier> newStatBases = new List<StatModifier>();
            foreach (string statMod in Enum.GetNames(typeof(APCESettings.SharedStatBases)))
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
            sightsEfficiency.stat = StatDef.Named("SightsEfficiency");

            //ShotSpread: float, ex: charge rifle 0.12, FAL 0.06, sniper rifles 0.02-0.04, negatively correlates to accuracy
            StatModifier shotSpread = new StatModifier();
            shotSpread.stat = StatDef.Named("ShotSpread");

            StatModifier swayFactor = new StatModifier();
            swayFactor.stat = StatDef.Named("SwayFactor");

            //Bulk: float, for "1 handed" guns seems to be = mass, for "2 handed" = 2 * mass
            StatModifier gunBulk = new StatModifier();
            gunBulk.stat = StatDef.Named("Bulk");

            StatModifier recoil = new StatModifier();
            recoil.stat = StatDef.Named("Recoil");

            //ReloadTime: in seconds
            //StatModifier reloadTime = new StatModifier();
            //reloadTime.stat = StatDef.Named("ReloadTime");

            float gunTechModFlat = (((float)def.techLevel - (float)TechLevel.Industrial) * 0.1f);
            float gunTechModPercent = (1 - gunTechModFlat);
            float ssAccuracyMod = (def.Verbs[0].accuracyLong * 0.1f);
            float seDefault = 1f + gunTechModFlat;
            float recoilTechMod = (1 - (((float)def.techLevel - 3) * 0.2f));
            float gunMass = def.statBases.GetStatValueFromList(StatDefOf.Mass, 1);
            float forcedMissRadius = def.Verbs[0].ForcedMissRadius;

            switch (gunKind)
            {
                case APCESettings.gunKinds.Bow:
                    sightsEfficiency.value = 0.6f;
                    shotSpread.value = 1f;
                    swayFactor.value = 2f;
                    gunBulk.value = 2f * gunMass;
                    //reloadTime.value = 1f;
                    break;
                case APCESettings.gunKinds.Handgun:
                    shotSpread.value = (0.2f - ssAccuracyMod) * gunTechModPercent;
                    sightsEfficiency.value = 0.7f + gunTechModFlat;
                    swayFactor.value = 1f;
                    gunBulk.value = 1f * gunMass;
                    break;
                case APCESettings.gunKinds.SMG:
                    shotSpread.value = (0.17f - ssAccuracyMod) * gunTechModPercent;
                    sightsEfficiency.value = 0.7f + gunTechModFlat;
                    swayFactor.value = 2f;
                    gunBulk.value = 1f * gunMass;
                    recoil.value = (2f - (gunMass * 0.1f)) * recoilTechMod;
                    break;
                case APCESettings.gunKinds.Shotgun:
                    shotSpread.value = (0.17f - ssAccuracyMod) * gunTechModPercent;
                    sightsEfficiency.value = seDefault;
                    swayFactor.value = 1.2f;
                    gunBulk.value = 2f * gunMass;
                    break;
                case APCESettings.gunKinds.assaultRifle:
                    shotSpread.value = (0.13f - ssAccuracyMod) * gunTechModPercent;
                    sightsEfficiency.value = seDefault;
                    swayFactor.value = 1.33f;
                    gunBulk.value = 2f * gunMass;
                    recoil.value = (1.8f - (gunMass * 0.1f)) * recoilTechMod;
                    break;
                case APCESettings.gunKinds.MachineGun:
                    shotSpread.value = (0.13f - ssAccuracyMod) * gunTechModPercent;
                    sightsEfficiency.value = seDefault;
                    swayFactor.value = 1.4f;
                    gunBulk.value = 1.5f * gunMass;
                    recoil.value = (2.3f - (gunMass * 0.1f)) * recoilTechMod;
                    break;
                case APCESettings.gunKinds.precisionRifle:
                    shotSpread.value = (0.1f - ssAccuracyMod) * gunTechModPercent;
                    sightsEfficiency.value = 2.6f + gunTechModFlat;
                    swayFactor.value = 1.35f;
                    gunBulk.value = 2f * gunMass;
                    break;
                case APCESettings.gunKinds.ExplosiveLauncher:
                    shotSpread.value = 0.122f + (forcedMissRadius * 0.02f);
                    sightsEfficiency.value = seDefault;
                    swayFactor.value = 1.8f;
                    gunBulk.value = 2f * gunMass;
                    break;
                case APCESettings.gunKinds.Turret:
                    shotSpread.value = (0.1f - ssAccuracyMod) * gunTechModPercent;
                    sightsEfficiency.value = seDefault;
                    swayFactor.value = 1.5f;
                    gunBulk.value = 2f * gunMass;
                    recoil.value = 1f;
                    break;
                case APCESettings.gunKinds.Grenade:
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
            ticksBBS.stat = StatDef.Named("TicksBetweenBurstShots");
            ticksBBS.value = def.Verbs[0].ticksBetweenBurstShots;

            //BurstShotCount as above. int, ex: AR 3, LMG 10, pistols 1
            StatModifier burstShotCount = new StatModifier();
            burstShotCount.stat = StatDef.Named("BurstShotCount");
            if (def.Verbs[0].burstShotCount == 1)
                burstShotCount.value = 1;
            else
                burstShotCount.value = 2 * def.Verbs[0].burstShotCount;

            if (gunKind != APCESettings.gunKinds.Grenade)
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

        internal static void AddCompsAmmoUser(ThingDef weapon, APCESettings.gunKinds gunKind)
        {
            CombatExtended.CompProperties_AmmoUser newAUComp = new CombatExtended.CompProperties_AmmoUser();
            newAUComp.magazineSize = weapon.Verbs[0].burstShotCount * 5; //TODO maybe switch based on gun kind
            switch (gunKind)
            {
                case APCESettings.gunKinds.Bow:
                    {
                        newAUComp.reloadTime = 1f;
                        break;
                    }
                case APCESettings.gunKinds.MachineGun:
                    {
                        newAUComp.reloadTime = newAUComp.magazineSize * 0.09f;
                        break;
                    }
                default:
                    {
                        newAUComp.reloadTime = 4f;
                        break;
                    }
            }
            newAUComp.reloadOneAtATime = false; //TODO heuristic
            newAUComp.throwMote = true;
            newAUComp.ammoSet = GenerateAmmoSet(weapon, gunKind);
            newAUComp.loadedAmmoBulkFactor = 0;
            newAUComp.compClass = typeof(CombatExtended.CompAmmoUser);
            weapon.comps.Add(newAUComp);
        }

        internal static void AddCompsFireModes(ThingDef weapon, APCESettings.gunKinds gunKind) // TODO WIP 
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
            newFMComp.aiUseBurstMode = true;
            newFMComp.noSingleShot = false; //TODO figure out what types of CE guns don't have this
            newFMComp.noSnapshot = false; //TODO same as above
            newFMComp.aiAimMode = AimMode.Snapshot; //TODO if statement based on gun type?
            weapon.comps.Add(newFMComp);
        }

        internal static void PatchBaseBullet(ThingDef bullet)
        {
            bullet.category = ThingCategory.Projectile;
            bullet.tickerType = TickerType.Normal;
            bullet.altitudeLayer = AltitudeLayer.Projectile;
            bullet.thingClass = typeof(CombatExtended.BulletCE);
            bullet.useHitPoints = false;
            bullet.neverMultiSelect = true;
            bullet.graphicData.shaderType = ShaderTypeDefOf.Transparent;
        }
    }
}