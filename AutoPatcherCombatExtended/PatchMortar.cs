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

        
    }
}