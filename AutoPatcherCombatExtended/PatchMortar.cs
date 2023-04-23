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
        internal static void PatchMortar(ThingDef mortar)
        {
            AddMortarStats(mortar);
            AddCompProperties_AmmoUser(mortar, APCESettings.gunKinds.Mortar);
            AddCompProperties_Charges(mortar);
            mortar.weaponTags.Add("TurretGun");
            PatchVerbMortar(mortar);
        }

        internal static void AddMortarStats(ThingDef mortar)
        {
            StatModifier sightsEfficiency = new StatModifier();
            sightsEfficiency.stat = CE_StatDefOf.SightsEfficiency;
            sightsEfficiency.value = 0.5f;
            mortar.statBases.Add(sightsEfficiency);
        }

        internal static void AddCompProperties_Charges(ThingDef mortar)
        {
            CompProperties_Charges compProp_Ch = new CompProperties_Charges();
            compProp_Ch.chargeSpeeds = new List<int> { 30, 50, 70, 90 };
            mortar.comps.Add(compProp_Ch);
        }

        internal static void PatchVerbMortar(ThingDef mortar)
        {
            VerbProperties oldVerb = mortar.Verbs[0];
            VerbPropertiesCE newVerb = new VerbPropertiesCE();
            newVerb.verbClass = typeof(Verb_ShootMortarCE);
            newVerb.label = oldVerb.label;
            newVerb.forceNormalTimeSpeed = false;
            newVerb.hasStandardCommand = true;
            newVerb.requireLineOfSight = false;
            newVerb.defaultProjectile = APCEDefOf.Bullet_81mmMortarShell_HE;
            newVerb.warmupTime = oldVerb.warmupTime;
            newVerb.minRange = oldVerb.minRange;
            newVerb.range = oldVerb.range;
            newVerb.burstShotCount = oldVerb.burstShotCount;
            newVerb.soundCast = oldVerb.soundCast;
            newVerb.soundCastTail = oldVerb.soundCastTail;
            newVerb.muzzleFlashScale = oldVerb.muzzleFlashScale;
            newVerb.circularError = 1;
            newVerb.indirectFirePenalty = 0.2f;
            newVerb.targetParams = oldVerb.targetParams;
            mortar.Verbs.Clear();
            mortar.Verbs.Add(newVerb);
        }

        internal static void PatchMortarShell(ThingDef mortarShell, APCEPatchLogger log)
        {
            try
            {
                AmmoDef newAmmo = MakeNewMortarAmmo(mortarShell);
                MakeMortarAmmoLink(newAmmo);
                MarkForReplacement(mortarShell, newAmmo);
            }
            catch (Exception ex)
            {
                log.PatchFailed(mortarShell.defName, ex);
            }

            log.PatchSucceeded();
        }

        internal static AmmoDef MakeNewMortarAmmo(ThingDef def)
        {
            AmmoDef newAmmo = new AmmoDef();
            CopyFields(def, newAmmo);
            newAmmo.defName = "APCE_Shell_" + def.defName;
            newAmmo.shortHash = 0;
            newAmmo.modContentPack = APCESettings.thisMod;

            newAmmo.comps = new List<CompProperties>();
            foreach (CompProperties cp in def.comps) //slightly redundant, but the way CopyFields works causes problems with adding the ReplaceMe CompProp
            {
                newAmmo.comps.Add(cp);
            }
            
            ConvertCompProperties_Explosive(newAmmo);

            /* the mortar shell is not actually the projectile
            if (newAmmo.projectile != null)
            {
                newAmmo.projectile = ConvertPP(newAmmo.projectile);
            }
            */
            newAmmo.projectileWhenLoaded = MakeNewMortarProjectile(def.projectileWhenLoaded);
            newAmmo.projectileWhenLoaded.projectile.speed = 0;

            newAmmo.thingCategories.Add(APCEDefOf.Ammo81mmMortarShells); //TODO maybe remove old categories
            newAmmo.stackLimit = 25;
            newAmmo.cookOffFlashScale = 30;
            newAmmo.cookOffSound = APCEDefOf.MortarBomb_Explode;
            newAmmo.isMortarAmmo = true;
            newAmmo.menuHidden = false;
            newAmmo.ammoClass = MakeMortarAmmoCategoryDef(newAmmo);

            InjectedDefHasher.GiveShortHashToDef(newAmmo, typeof(AmmoDef));
            DefGenerator.AddImpliedDef<ThingDef>(newAmmo);

            def.description = def.description + "\n This mortar shell should be converted to a CE-compatible one as soon as it spawns in.";

            return newAmmo;
        }

        internal static AmmoCategoryDef MakeMortarAmmoCategoryDef(ThingDef mortarShell)
        {
            AmmoCategoryDef newAmmoCat = new AmmoCategoryDef();
            newAmmoCat.defName = "APCE_AmmoCatDef_ " + mortarShell.defName;
            newAmmoCat.label = mortarShell.label;
            newAmmoCat.description = "Ammo category of " + mortarShell.label;

            InjectedDefHasher.GiveShortHashToDef(newAmmoCat, typeof(AmmoCategoryDef));
            DefGenerator.AddImpliedDef<AmmoCategoryDef>(newAmmoCat);

            return newAmmoCat;
        }

        internal static ThingDef MakeNewMortarProjectile(ThingDef oldProjectile)
        {//WIP
            ThingDef newProjectile = new ThingDef();
            newProjectile.defName = ("APCE_Bullet_Shell_ " + oldProjectile.defName);
            newProjectile.label = oldProjectile.label;
            newProjectile.graphicData = oldProjectile.graphicData;
            PatchBaseBullet(newProjectile);
            newProjectile.projectile = ConvertPP(oldProjectile.projectile);

            InjectedDefHasher.GiveShortHashToDef(newProjectile, typeof(ThingDef));
            DefGenerator.AddImpliedDef<ThingDef>(newProjectile);

            return newProjectile;
        }

        internal static void MakeMortarAmmoLink(AmmoDef ammoDef)
        {
            AmmoLink ammoDefLink = new AmmoLink(ammoDef, ammoDef.projectileWhenLoaded);
            AmmoSetDef ammoSet81mm = APCEDefOf.AmmoSet_81mmMortarShell;
            ammoSet81mm.ammoTypes.Add(ammoDefLink);
        }

        internal static void MarkForReplacement(ThingDef def, AmmoDef newAmmo)
        {
            CompProperties_ReplaceMe cp_rm = new CompProperties_ReplaceMe();
            cp_rm.thingToSpawn = newAmmo;
            def.comps.Add(cp_rm);
        }

    }
}