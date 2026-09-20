using CombatExtended;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using UnityEngine;

namespace nuff.AutoPatcherCombatExtended;

public class DefDataHolderRangedWeapon : DefDataHolder
{
    private const string CustomVerbMessage = "Weapon is using a custom Verb but patching of custom verbs is disabled. Skipping this step. If you would like the auto-patcher to try anyway, go to the settings for this mod and enable the \"Try to patch weapons with custom Verbs\" setting.";

    public DefDataHolderRangedWeapon()
    {
        //empty constructor for use by SaveLoad
    }

    public DefDataHolderRangedWeapon(ThingDef def) : base(def)
    {
    }

    public ThingDef weaponThingDef;

    //original statbase stuff
    float original_Mass;
    float original_RangedWeaponCooldown;
    float original_WorkToMake;
    int original_BurstShotCount;

    bool stuffed;

    //original verbprops stuff - just store the whole thing, since it won't be modified
    VerbProperties original_VerbProperties;
    bool original_IsCustomVerb = true;

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
    internal Type modified_verbClass;
    internal float modified_muzzleFlashScale;
    internal int modified_ticksBetweenBurstShots;
    internal float modified_warmupTime = 1;
    internal int modified_burstShotCount = 1;
    internal float modified_recoilAmount;
    internal float modified_range;
    internal RecoilPattern modified_recoilPattern = RecoilPattern.None;
    internal ThingDef modified_defaultProjectile;
    internal string modified_defaultProjectileString;

    //modified comp stuff
    internal bool modified_UsesAmmo = true;
    internal int modified_magazineSize;
    internal int modified_AmmoGenPerMagOverride;
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
    internal bool modified_noSnapshot;
    internal AimMode modified_aiAimMode;

    //modified grenade stuff
    AmmoDef modified_ammoDef; // for use in grenades
    int modified_recipeCount; //also for grenades
    int modified_stackLimit;
    int modified_grenadeDamage;
    float modified_explosionRadius;

    float floorArmorPenetrationSharp;
    float floorArmorPenetrationBlunt;

    private bool IsUsingCustomVerb()
    {
        Type verbClass = original_VerbProperties?.verbClass ?? weaponThingDef?.Verbs?.FirstOrDefault()?.verbClass;

        bool vanillaOrCE = verbClass == typeof(Verb_Shoot)
            || verbClass == typeof(Verb_LaunchProjectile)
            || verbClass == typeof(Verb_ShootOneUse)
            || verbClass == typeof(Verb_SpewFire)
            || verbClass == typeof(Verb_ShootBeam)
            || verbClass == typeof(Verb_ShootCE)
            || verbClass == typeof(Verb_ShootCEOneUse)
            || verbClass == typeof(Verb_ShootMortarCE);

        return !vanillaOrCE;
    }

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

        APCEConstants.PatchStageLog log = StartNewLogEntry(APCEConstants.PatchStage.GetOriginalData);
        StringBuilder logText = log.Text;
        logText.AppendLine($"Starting GetOriginalData log entry for {def?.defName ?? "NULL DEF"}");

        try
        {
            //need to make sure these lists aren't null before starting DetermineGunKind
            if (weaponThingDef.statBases == null)
            {
                weaponThingDef.statBases = new List<StatModifier>();
                logText.AppendLine("Had null statBases, made new List");
            }
            if (weaponThingDef.weaponTags == null)
            {
                weaponThingDef.weaponTags = new List<string>();
                logText.AppendLine("Had null weaponTags, made new List");
            }

            if (!weaponThingDef.tools.NullOrEmpty())
            {
                original_Tools = weaponThingDef.tools.ToList();
                logText.AppendLine($"Preserved original Tools list with count: {original_Tools.Count}");
            }
            else
            {
                logText.AppendLine("Had null or empty Tools list, skipping");
            }

            original_VerbProperties = weaponThingDef.Verbs[0]; // TODO eventually make compatible with MVCF
            logText.AppendLine($"Preserved original Verb of Type: {original_VerbProperties.verbClass}");

            original_IsCustomVerb = IsUsingCustomVerb(); 
            logText.AppendLine($"Verb class {original_VerbProperties.verbClass} is a {(original_IsCustomVerb ? "Custom" : "Vanilla or CE")} Verb.");

            original_Mass = weaponThingDef.statBases.GetStatValueFromList(StatDefOf.Mass, 0);
            logText.AppendLine($"original_Mass: {original_Mass}");

            original_RangedWeaponCooldown = weaponThingDef.statBases.GetStatValueFromList(StatDefOf.RangedWeapon_Cooldown, 0);
            logText.AppendLine($"original_RangedWeaponCooldown: {original_RangedWeaponCooldown}");

            original_WorkToMake = weaponThingDef.statBases.GetStatValueFromList(StatDefOf.WorkToMake, 0);
            logText.AppendLine($"original_WorkToMake: {original_WorkToMake}");

            original_BurstShotCount = original_VerbProperties.burstShotCount;
            logText.AppendLine($"original_BurstShotCount: {original_BurstShotCount}");

            stuffed = weaponThingDef.MadeFromStuff;
            logText.AppendLine($"stuffed: {stuffed}");
        }
        catch (Exception ex)
        {
            logText.AppendLine($"Exception in GetOriginalData for: {def?.defName ?? "NULL DEF"}");
            logText.AppendLine(ex.ToString());
            log.ThrewError = true;
        }
        finally
        {
            CloseLogEntry(APCEConstants.PatchStage.GetOriginalData);
        }
    }

    public override void AutoCalculate()
    {
        APCEConstants.PatchStageLog log = StartNewLogEntry(APCEConstants.PatchStage.AutoCalculate);
        StringBuilder logText = log.Text;
        logText.AppendLine($"Starting AutoCalculate log entry for {def?.defName ?? "NULL DEF"}");

        try
        {
            if (original_IsCustomVerb && !ModData.patchCustomVerbs)
            {
                logText.AppendLine(CustomVerbMessage);
                return;
            }

            logText.AppendLine("Attempting to determine kind of gun.");
            gunKind = GeneralUtils.DetermineGunKind(weaponThingDef, log);

            CalculateWeaponTechMult(logText);

            if (gunKind == APCEConstants.gunKinds.Mortar)
            {
                logText.AppendLine("Calculating stats for a mortar and stopping Autocalculate.");
                CalculateMortar();
                return;
            }

            if (!original_Tools.NullOrEmpty())
            {
                logText.AppendLine("Starting Tools.");
                ClearModdedTools();
                for (int i = 0; i < original_Tools.Count; i++)
                {
                    ModToolAtIndex(i);
                }
            }
            else
            {
                logText.AppendLine("No Tools, skipping.");
            }

            CalculateStatBaseValues(logText);

            modified_UsesAmmo = ModData.gunsUseAmmo && gunKind != APCEConstants.gunKinds.BeamGun;
            logText.AppendLine($"modified_UsesAmmo: {modified_UsesAmmo}");

            if (gunKind == APCEConstants.gunKinds.BeamGun)
            {
                logText.AppendLine("Autocalculate complete due to gun kind being BeamGun");
                return;
            }

            CalculateVerbPropValues(logText);

            if (gunKind == APCEConstants.gunKinds.Flamethrower)
            {
                modified_AmmoSetDef = APCEDefOf.AmmoSet_Flamethrower;
                logText.AppendLine("Gun kind is flamethrower, assigning CE flamethrower AmmoSet");
            }

            if (modified_AmmoSetDef == null)
            {
                FixAmmoSet(logText);
            }

            if (gunKind != APCEConstants.gunKinds.Grenade)
            {
                CalculateCompFireModesValues(logText);
                CalculateCompAmmoUserValues(logText);
            }
            else
            {
                CalculateGrenade();
            }
        }
        catch (Exception ex)
        {
            logText.AppendLine($"Exception in AutoCalculate for: {def?.defName ?? "NULL DEF"}");
            logText.AppendLine(ex.ToString());
            log.ThrewError = true;
        }
        finally
        {
            CloseLogEntry(APCEConstants.PatchStage.AutoCalculate);
        }
    }


    public override void PrePatch()
    {
        APCEConstants.PatchStageLog log = StartNewLogEntry(APCEConstants.PatchStage.PrePatch);
        StringBuilder logText = log.Text;
        logText.AppendLine($"Starting PrePatch log entry for ammoset for {def?.defName ?? "NULL DEF"}");

        try
        {
            if (original_IsCustomVerb && !ModData.patchCustomVerbs)
            {
                logText.AppendLine(CustomVerbMessage);
                return;
            }

            if (gunKind != APCEConstants.gunKinds.Grenade && gunKind != APCEConstants.gunKinds.BeamGun)
            {
                FixAmmoSet(logText);
                FixDefaultProjectile(logText);
            }

            base.PrePatch();
        }
        catch (Exception ex)
        {
            log.ThrewError = true;
            logText.AppendLine(ex.ToString());
        }
        finally
        {
            CloseLogEntry(APCEConstants.PatchStage.PrePatch);
        }
    }

    public override void PostPatch()
    {
        APCEConstants.PatchStageLog log = StartNewLogEntry(APCEConstants.PatchStage.PostPatch);
        StringBuilder logText = log.Text;
        logText.AppendLine($"Starting PostPatch log entry for ammoset for {def?.defName ?? "NULL DEF"}");

        try
        {
            if (original_IsCustomVerb && !ModData.patchCustomVerbs)
            {
                logText.AppendLine(CustomVerbMessage);
                return;
            }

            FixAmmoSet(logText);
            FixDefaultProjectile(logText);

            base.PostPatch();
        }
        catch (Exception ex)
        {
            log.ThrewError = true;
            logText.AppendLine(ex.ToString());
        }
        finally
        {
            CloseLogEntry(APCEConstants.PatchStage.PostPatch);
        }
    }

    public override void ApplyPatch()
    {
        APCEConstants.PatchStageLog log = StartNewLogEntry(APCEConstants.PatchStage.ApplyPatch);
        StringBuilder logText = log.Text;
        logText.AppendLine($"Starting ApplyPatch log entry for ammoset for {def?.defName ?? "NULL DEF"}");

        try
        {
            if (original_IsCustomVerb && !ModData.patchCustomVerbs)
            {
                logText.AppendLine(CustomVerbMessage);
                return;
            }

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
        catch (Exception ex)
        {
            logText.AppendLine($"Exception in Patch for: {def?.defName ?? "NULL DEF"}");
            logText.AppendLine(ex.ToString());
            log.ThrewError = true;
        }
        finally
        {
            //TODO verbose logging
            CloseLogEntry(APCEConstants.PatchStage.ApplyPatch);
        }
    }

    public override StringBuilder ExportXML()
    {
        if (original_IsCustomVerb && !ModData.patchCustomVerbs)
        {
            throw new Exception("Failed to export patch. Weapon uses a custom Verb, and custom Verb patching is disabled for this mod");
        }

        if (modified_UsesAmmo && modified_AmmoSetDef.defName.Contains("APCE"))
        {
            throw new Exception($"Failed to export patch. If the gun is set to use ammo, you must assign a real ammoset instead of keeping the one generated by this auto-patcher.");
        }

        if (!modified_UsesAmmo && (!(modified_defaultProjectile.projectile is ProjectilePropertiesCE) || modified_defaultProjectile.defName.Contains("APCE")))
        {
            throw new Exception("Failed to export patch. If the gun is set to not use ammo, you must assign a real CE projectile for it to fire.");
        }

        // If user hasn't closed/reloaded game since changing off of an autogenerated AmmoSet, defaultProjectile will be an autogen, too. Fix that.
        if (modified_UsesAmmo)
        {
            modified_defaultProjectile = modified_AmmoSetDef.ammoTypes[0].projectile;
        }

        xml = GeneralUtils.GetXmlForDef(weaponThingDef);

        patchOps = new List<string>();
        patchOps.Add(GeneratePatchOpMakeGunCECompatible());
        patchOps.Add(GenerateToolPatchXML());

        base.ExportXML();

        return patch;

        string GeneratePatchOpMakeGunCECompatible()
        {
            StringBuilder patchOp = new StringBuilder();
            patchOp.AppendLine("\t<Operation Class=\"CombatExtended.PatchOperationMakeGunCECompatible\">");
            patchOp.AppendLine($"\t\t<defName>{defName}</defName>");

            patchOp.AppendLine("\t\t<statBases>");
            if (modified_Mass != original_Mass)
                patchOp.AppendLine($"\t\t\t<Mass>{modified_Mass}</Mass>");
            if (modified_RangedWeaponCooldown != original_RangedWeaponCooldown)
                patchOp.AppendLine($"\t\t\t<RangedWeapon_Cooldown>{modified_RangedWeaponCooldown}</RangedWeapon_Cooldown>");
            if (modified_WorkToMake != original_WorkToMake)
                patchOp.AppendLine($"\t\t\t<WorkToMake>{modified_WorkToMake}</WorkToMake>");
            patchOp.AppendLine($"\t\t\t<Bulk>{modified_Bulk}</Bulk>");
            patchOp.AppendLine($"\t\t\t<SightsEfficiency>{modified_SightsEfficiency}</SightsEfficiency>");
            patchOp.AppendLine($"\t\t\t<ShotSpread>{modified_ShotSpread}</ShotSpread>");
            patchOp.AppendLine($"\t\t\t<SwayFactor>{modified_SwayFactor}</SwayFactor>");
            patchOp.AppendLine("\t\t</statBases>");

            patchOp.AppendLine($"\t\t<Properties>");
            patchOp.AppendLine($"\t\t\t<recoilAmount>{modified_recoilAmount}</recoilAmount>");
            patchOp.AppendLine($"\t\t\t<verbClass>{modified_verbClass.FullName}</verbClass>");
            patchOp.AppendLine($"\t\t\t<hasStandardCommand>{original_VerbProperties.hasStandardCommand.ToString()}</hasStandardCommand>");
            patchOp.AppendLine($"\t\t\t<defaultProjectile>{(modified_defaultProjectile)}</defaultProjectile>");

            patchOp.AppendLine($"\t\t\t<warmupTime>{modified_warmupTime}</warmupTime>");
            patchOp.AppendLine($"\t\t\t<range>{modified_range}</range>");
            if (original_VerbProperties.soundCast != null)
            {
                patchOp.AppendLine($"\t\t\t<soundCast>{original_VerbProperties.soundCast.defName}</soundCast>");
            }
            if (original_VerbProperties.soundCastTail != null)
            {
                patchOp.AppendLine($"\t\t\t<soundCastTail>{original_VerbProperties.soundCastTail.defName}</soundCastTail>");
            }
            patchOp.AppendLine($"\t\t\t<muzzleFlashScale>{modified_muzzleFlashScale}</muzzleFlashScale>");
            if (original_VerbProperties.targetParams.canTargetLocations)
            {
                patchOp.AppendLine($"\t\t\t<targetParams>");
                patchOp.AppendLine($"\t\t\t\t<canTargetLocations>true</canTargetLocations>");
                patchOp.AppendLine($"\t\t\t</targetParams>");
            }
            patchOp.AppendLine($"\t\t</Properties>");

            if (modified_UsesAmmo)
            {
                patchOp.AppendLine($"\t\t<AmmoUser>");
                patchOp.AppendLine($"\t\t\t<ammoSet>{modified_AmmoSetDef.defName}</ammoSet>");
                patchOp.AppendLine($"\t\t\t<magazineSize>{modified_magazineSize}</magazineSize>");
                if (modified_reloadTime != 1)
                {
                    patchOp.AppendLine($"\t\t\t<reloadTime>{modified_reloadTime}</reloadTime>");
                }
                if (modified_AmmoGenPerMagOverride != 0)
                {
                    patchOp.AppendLine($"\t\t\t<AmmoGenPerMagOverride>{modified_AmmoGenPerMagOverride}</AmmoGenPerMagOverride>");
                }
                if (!modified_throwMote)
                {
                    patchOp.AppendLine("\t\t\t<throwMote>false</throwMote>");
                }
                if (modified_reloadOneAtATime)
                {
                    patchOp.AppendLine("\t\t\t<reloadOneAtATime>true</reloadOneAtATime>");
                }
                if (modified_loadedAmmoBulkFactor != 0)
                {
                    patchOp.AppendLine($"\t\t\t<loadedAmmoBulkFactor>{modified_loadedAmmoBulkFactor}</loadedAmmoBulkFactor>");
                }
                patchOp.AppendLine($"\t\t</AmmoUser>");
            }

            // Separating this out so it is easier to detect if it can be omitted
            string fireModes = GenerateCompPropsFireModesXML();
            if (!string.IsNullOrEmpty(fireModes))
            {
                patchOp.AppendLine($"\t\t<FireModes>");
                patchOp.Append(fireModes);
                patchOp.AppendLine($"\t\t</FireModes>");
            }

            patchOp.AppendLine("\t</Operation>");

            return patchOp.ToString();
        }

        string GenerateCompPropsFireModesXML()
        {
            StringBuilder fire = new StringBuilder();

            if (modified_aimedBurstShotCount != 1 && modified_aimedBurstShotCount != modified_burstShotCount / 2)
            {
                fire.AppendLine($"\t\t\t<aimedBurstShotCount>{modified_aimedBurstShotCount}</aimedBurstShotCount>");
            }
            if (modified_aiUseBurstMode)
            {
                fire.AppendLine("\t\t\t<aiUseBurstMode>true</aiUseBurstMode>");
            }
            if (modified_noSingleShot)
            {
                fire.AppendLine("\t\t\t<noSingleShot>true</noSingleShot>");
            }
            if (modified_noSnapshot)
            {
                fire.AppendLine("\t\t\t<noSnapshot>true</noSnapshot>");
            }
            if (modified_aiAimMode != AimMode.AimedShot)
            {
                fire.AppendLine($"\t\t\t<aiAimMode>{modified_aiAimMode}</aiAimMode>");
            }

            return fire.ToString();
        }
    }

    // I forgot that PatchOperationMakeGunCECompatible existed. Been too long since I manually wrote a mod patch.
    /*
    public override StringBuilder ExportXML()
    {
        if (modified_UsesAmmo && modified_AmmoSetDef.defName.Contains("APCE"))
        {
            throw(new Exception($"Failed to export patch. If the gun is set to use ammo, you must assign a real ammoset instead of keeping the one generated by this auto-patcher."));
        }

        xml = DataHolderUtils.GetXmlForDef(weaponThingDef);

        patchOps = new List<string>();
        patchOps.Add(APCEPatchExport.GeneratePatchOperationFor(xml, "statBases", "Mass", modified_Mass, original_Mass));
        patchOps.Add(APCEPatchExport.GeneratePatchOperationFor(xml, "statBases", "Bulk", modified_Bulk));
        patchOps.Add(APCEPatchExport.GeneratePatchOperationFor(xml, "statBases", "RangedWeaponCooldown", modified_RangedWeaponCooldown, original_RangedWeaponCooldown));
        patchOps.Add(APCEPatchExport.GeneratePatchOperationFor(xml, "statBases", "WorkToMake", modified_WorkToMake, original_WorkToMake));
        patchOps.Add(APCEPatchExport.GeneratePatchOperationFor(xml, "statBases", "SightsEfficiency", modified_SightsEfficiency));
        patchOps.Add(APCEPatchExport.GeneratePatchOperationFor(xml, "statBases", "ShotSpread", modified_ShotSpread));
        patchOps.Add(APCEPatchExport.GeneratePatchOperationFor(xml, "statBases", "SwayFactor", modified_SwayFactor));
        //patchOps.Add(APCEPatchExport.GeneratePatchOperationFor(xml, "statBases", "WeaponToughness", modified_WeaponToughness)); //let weapon toughness autopatcher do its thing

        patchOps.Add(APCEPatchExport.RemoveXmlNode(xml, "statBases", "AccuracyTouch"));
        patchOps.Add(APCEPatchExport.RemoveXmlNode(xml, "statBases", "AccuracyShort"));
        patchOps.Add(APCEPatchExport.RemoveXmlNode(xml, "statBases", "AccuracyMedium"));
        patchOps.Add(APCEPatchExport.RemoveXmlNode(xml, "statBases", "AccuracyLong"));

        patchOps.Add(GenerateVerbPatchXML());

        patchOps.Add(GenerateCompsPatchXML());

        patchOps.Add(GenerateToolPatchXML());

        base.ExportXML();

        return patch;

        string GenerateVerbPatchXML()
        {
            string xpath = $"Defs/ThingDef[defName=\"{defName}\"]/verbs";
            StringBuilder patch = new StringBuilder();

            patch.AppendLine("\t<Operation Class=\"PatchOperationReplace\">");
            patch.AppendLine($"\t\t<xpath>{xpath}</xpath>");
            patch.AppendLine("\t\t<value>");
            patch.AppendLine("\t\t\t<verbs>");
            patch.AppendLine("\t\t\t\t<li Class=\"CombatExtended.VerbPropertiesCE\">");
            patch.AppendLine($"\t\t\t\t\t<recoilAmount>{modified_recoilAmount}</recoilAmount>");
            patch.AppendLine($"\t\t\t\t\t<verbClass>{modified_verbClass.FullName}</verbClass>");
            patch.AppendLine($"\t\t\t\t\t<hasStandardCommand>{original_VerbProperties.hasStandardCommand.ToString()}</hasStandardCommand>");
            patch.AppendLine($"\t\t\t\t\t<defaultProjectile>{(!modified_UsesAmmo ? original_VerbProperties.defaultProjectile.defName : modified_AmmoSetDef.ammoTypes[0].projectile.defName)}</defaultProjectile>");
            patch.AppendLine($"\t\t\t\t\t<warmupTime>{modified_warmupTime}</warmupTime>");
            patch.AppendLine($"\t\t\t\t\t<range>{modified_range}</range>");
            if (original_VerbProperties.soundCast != null)
            {
                patch.AppendLine($"\t\t\t\t\t<soundCast>{original_VerbProperties.soundCast.defName}</soundCast>");
            }
            if (original_VerbProperties.soundCastTail != null)
            {
                patch.AppendLine($"\t\t\t\t\t<soundCastTail>{original_VerbProperties.soundCastTail.defName}</soundCastTail>");
            }
            patch.AppendLine($"\t\t\t\t\t<muzzleFlashScale>{modified_muzzleFlashScale}</muzzleFlashScale>");
            if (original_VerbProperties.targetParams.canTargetLocations)
            {
                patch.AppendLine($"\t\t\t\t\t<targetParams>");
                patch.AppendLine($"\t\t\t\t\t\t<canTargetLocations>true</canTargetLocations>");
                patch.AppendLine($"\t\t\t\t\t</targetParams>");
            }
            patch.AppendLine("\t\t\t\t</li>");
            patch.AppendLine("\t\t\t</verbs>");
            patch.AppendLine("\t\t</value>");
            patch.AppendLine("\t</Operation>");
            patch.AppendLine();

            return patch.ToString();
        }

        string GenerateCompsPatchXML()
        {
            string xpath = $"Defs/ThingDef[defName=\"{defName}\"]/comps";
            StringBuilder patch = new StringBuilder();

            patch.AppendLine("\t<Operation Class=\"PatchOperationAdd\">");
            patch.AppendLine($"\t\t<xpath>{xpath}</xpath>");
            patch.AppendLine("\t\t<value>");
            if (modified_UsesAmmo)
            {
                patch.AppendLine("\t\t\t<li Class=\"CombatExtended.CompProperties_AmmoUser\">");
                patch.AppendLine($"\t\t\t\t<ammoSet>{modified_AmmoSetDef.defName}</ammoSet>");
                patch.AppendLine($"\t\t\t\t<magazineSize>{modified_magazineSize}</magazineSize>");
                if (modified_reloadTime != 1)
                {
                    patch.AppendLine($"\t\t\t\t<reloadTime>{modified_reloadTime}</reloadTime>");
                }
                if (modified_AmmoGenPerMagOverride != 0)
                {
                    patch.AppendLine($"\t\t\t\t<AmmoGenPerMagOverride>{modified_AmmoGenPerMagOverride}</AmmoGenPerMagOverride>");
                }
                if (!modified_throwMote)
                {
                    patch.AppendLine("\t\t\t\t<throwMote>false</throwMote>");
                }
                if (modified_reloadOneAtATime)
                {
                    patch.AppendLine("\t\t\t\t<reloadOneAtATime>true</reloadOneAtATime>");
                }
                if (modified_loadedAmmoBulkFactor != 0)
                {
                    patch.AppendLine($"\t\t\t\t<loadedAmmoBulkFactor>{modified_loadedAmmoBulkFactor}</loadedAmmoBulkFactor>");
                }
                patch.AppendLine("\t\t\t</li>");
            }


            patch.AppendLine("\t\t\t<li Class=\"CombatExtended.CompProperties_FireModes\">");
            if (modified_aimedBurstShotCount != 1 && modified_aimedBurstShotCount != modified_burstShotCount / 2)
            {
                patch.AppendLine($"\t\t\t\t<aimedBurstShotCount>{modified_aimedBurstShotCount}</aimedBurstShotCount>");
            }
            if (modified_aiUseBurstMode)
            {
                patch.AppendLine("\t\t\t\t<aiUseBurstMode>true</aiUseBurstMode>");
            }
            if (modified_noSingleShot)
            {
                patch.AppendLine("\t\t\t\t<noSingleShot>true</noSingleShot>");
            }
            if (modified_noSnapshot)
            {
                patch.AppendLine("\t\t\t\t<noSnapshot>true</noSnapshot>");
            }
            if (modified_aiAimMode != AimMode.AimedShot)
            {
                patch.AppendLine($"\t\t\t\t<aiAimMode>{modified_aiAimMode}</aiAimMode>");
            }

            patch.AppendLine("\t\t\t</li>");

            patch.AppendLine("\t\t</value>");
            patch.AppendLine("\t</Operation>");
            patch.AppendLine();

            return patch.ToString();
        }
    }
    */

    public override void ExposeData()
    {
        if (Scribe.mode == LoadSaveMode.LoadingVars
            || (Scribe.mode == LoadSaveMode.Saving && isCustomized == true))
        {
            if (Scribe.mode == LoadSaveMode.Saving)
            {
                if (modified_AmmoSetDef != null)
                {
                    modified_AmmoSetDefString = modified_AmmoSetDef.defName;
                }
                if (modified_defaultProjectile != null)
                {
                    modified_defaultProjectileString = modified_defaultProjectile.defName;
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

            string verbClassName = modified_verbClass?.AssemblyQualifiedName;
            Scribe_Values.Look(ref verbClassName, "modified_VerbClass");
            Scribe_Values.Look(ref modified_muzzleFlashScale, "modified_muzzleFlashScale");
            Scribe_Values.Look(ref modified_ticksBetweenBurstShots, "modified_ticksBetweenBurstShots");
            Scribe_Values.Look(ref modified_warmupTime, "modified_warmupTime");
            Scribe_Values.Look(ref modified_burstShotCount, "modified_burstShotCount");
            Scribe_Values.Look(ref modified_recoilAmount, "modified_recoilAmount");
            Scribe_Values.Look(ref modified_recoilPattern, "modified_recoilPattern", RecoilPattern.None);
            Scribe_Values.Look(ref modified_range, "modified_range");
            Scribe_Values.Look(ref modified_defaultProjectileString, "modified_defaultProjectileString");

            Scribe_Values.Look(ref modified_magazineSize, "modified_magazineSize", 1);
            Scribe_Values.Look(ref modified_AmmoGenPerMagOverride, "modified_ammoGenPerMagOverride");
            Scribe_Values.Look(ref modified_reloadTime, "modified_reloadTime");
            Scribe_Values.Look(ref modified_throwMote, "modified_throwMote");
            Scribe_Values.Look(ref modified_reloadOneAtATime, "modified_reloadOneAtATime");
            Scribe_Values.Look(ref modified_loadedAmmoBulkFactor, "modified_loadedAmmoBulkFactor");
            Scribe_Values.Look(ref modified_AmmoSetDefString, "modified_AmmoSetDefString");

            Scribe_Values.Look(ref modified_aimedBurstShotCount, "modified_aimedBurstShotCount");
            Scribe_Values.Look(ref modified_aiUseBurstMode, "modified_aiUseBurstMode");
            Scribe_Values.Look(ref modified_noSingleShot, "modified_noSingleShot");
            Scribe_Values.Look(ref modified_noSnapshot, "modified_noSnapShot");
            Scribe_Values.Look(ref modified_aiAimMode, "modified_aiAimMode");

            Scribe_Values.Look(ref modified_recipeCount, "modified_recipeCount");
            Scribe_Values.Look(ref modified_stackLimit, "modified_stackLimit");
            Scribe_Values.Look(ref modified_grenadeDamage, "modified_grenadeDamage");

            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                APCEConstants.PatchStageLog log = StartNewLogEntry(APCEConstants.PatchStage.ExposeData);
                StringBuilder logText = log.Text;
                logText.AppendLine($"Starting ExposeData log entry for ammoset for {def?.defName ?? "NULL DEF"}");

                if (!string.IsNullOrEmpty(verbClassName))
                {
                    modified_verbClass = Type.GetType(verbClassName);

                    if (modified_verbClass == null)
                    {
                        Log.Warning($"Failed to load modified_VerbClass: {verbClassName}. Type not found.");
                    }
                }

                FixAmmoSet(logText);
                FixDefaultProjectile(logText);

                CloseLogEntry(APCEConstants.PatchStage.ExposeData);
            }
            //if (Scribe.mode == LoadSaveMode.LoadingVars && gunKind == APCEConstants.gunKinds.Grenade)
            //{
            //    modified_ammoDef = GenerateGrenadeAmmoDef();
            //}
        }
        base.ExposeData();
    }
    public void CalculateStatBaseValues(StringBuilder logText)
    {
        logText.AppendLine("Starting Stat Base calculation.");

        float ssAccuracyMod = weaponThingDef.statBases.GetStatValueFromList(StatDefOf.AccuracyLong, 0.5f) * 0.1f;
        logText.AppendLine($"ssAccuracyMod: {ssAccuracyMod}");

        float gunTechModAdd = weaponThingDef.techLevel.CompareTo(TechLevel.Industrial) * 0.1f;
        logText.AppendLine($"gunTechModAdd: {gunTechModAdd}");

        float gunTechModMult = 1 - gunTechModAdd;
        logText.AppendLine($"gunTechModMult: {gunTechModMult}");

        float recoilTechMod = 1 - (((float)weaponThingDef.techLevel - 3) * 0.2f);
        logText.AppendLine($"recoilTechMod: {recoilTechMod}");

        //Recoil is also calculated here since I don't want to make another switch in the other method
        switch (gunKind)
        {
            case APCEConstants.gunKinds.Bow:
                modified_SightsEfficiency = 0.6f;
                modified_ShotSpread = 1f;
                modified_SwayFactor = 2f;
                modified_Mass = Math.Min(original_Mass, 30);
                modified_Bulk = Math.Min(2f * original_Mass, 20f);
                modified_recoilAmount = 2f * recoilTechMod;
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
            case APCEConstants.gunKinds.AssaultRifle:
                modified_ShotSpread = (0.13f - ssAccuracyMod) * gunTechModMult;
                modified_SightsEfficiency = 1f + gunTechModAdd;
                modified_SwayFactor = 1.33f;
                modified_Mass = Math.Min(original_Mass, 30);
                modified_Bulk = Math.Min(2f * original_Mass, 20f);
                modified_recoilAmount = 1.8f * recoilTechMod;
                break;
            case APCEConstants.gunKinds.MachineGun:
                modified_ShotSpread = (0.13f - ssAccuracyMod) * gunTechModMult;
                modified_SightsEfficiency = 1f + gunTechModAdd;
                modified_SwayFactor = 1.4f;
                modified_Mass = Math.Min(original_Mass, 30);
                modified_Bulk = Math.Min(1.5f * original_Mass, 20f);
                modified_recoilAmount = 2.3f * recoilTechMod;
                break;
            case APCEConstants.gunKinds.PrecisionRifle:
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
                modified_recoilAmount = 2.3f * recoilTechMod;
                break;
            case APCEConstants.gunKinds.Turret:
                modified_ShotSpread = (0.1f - ssAccuracyMod) * gunTechModMult;
                modified_SightsEfficiency = 1f;
                modified_SwayFactor = 1.5f;
                modified_Mass = Math.Min(original_Mass, 30);
                modified_Bulk = Math.Min(2f * original_Mass, 20f);
                modified_recoilAmount = 1f;
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
                modified_recoilAmount = 1f;
                break;
        }

        // Put the logging down here so I don't have duplicate lines in almost every case. None of the above calcs should cause an exception.
        logText.AppendLine($"modified_SightsEfficiency: {modified_SightsEfficiency}");
        logText.AppendLine($"modified_ShotSpread: {modified_ShotSpread}");
        logText.AppendLine($"modified_SwayFactor: {modified_SwayFactor}");
        logText.AppendLine($"modified_Mass: {modified_Mass}");
        logText.AppendLine($"modified_Bulk: {modified_Bulk}");
        logText.AppendLine($"modified_recoilAmount: {modified_recoilAmount}");

        modified_WeaponToughness = GeneralUtils.WeaponToughnessAutocalc(weaponThingDef, modified_Bulk);
        logText.AppendLine($"modified_WeaponToughness: {modified_WeaponToughness}");

        modified_WorkToMake = original_WorkToMake;
        logText.AppendLine($"modified_WorkToMake: {modified_WorkToMake}");

        modified_RangedWeaponCooldown = original_RangedWeaponCooldown;
        logText.AppendLine($"modified_RangedWeaponCooldown: {modified_RangedWeaponCooldown}");
    }

    public void CalculateVerbPropValues(StringBuilder logText)
    {
        logText.AppendLine("Starting Verb Props calculation.");

        //if verb doesn't need patching, early return
        if ((original_VerbProperties.verbClass == typeof(Verb_ShootCE)) || (original_VerbProperties.verbClass == typeof(Verb_ShootCEOneUse)) || original_VerbProperties.verbClass == typeof(Verb_ShootBeam))
        {
            logText.AppendLine("Verb is already CE-compatible, skipping.");
            return;
        }

        modified_ticksBetweenBurstShots = original_VerbProperties.ticksBetweenBurstShots;
        logText.AppendLine($"modified_ticksBetweenBurstShots : {modified_ticksBetweenBurstShots}");

        modified_range = original_VerbProperties.range;
        logText.AppendLine($"modified_range : {modified_range}");

        //if warmupTime is too low, some weapons will get stuck permanently unable to fire, since it fires when the timer ticks from 1 to 0, not when it is AT 0
        modified_warmupTime = original_VerbProperties.warmupTime;
        logText.AppendLine($"modified_warmupTime : {modified_warmupTime}");
        if (modified_warmupTime < 0.07)
        {
            modified_warmupTime = 0.07f;
            logText.AppendLine("warmupTime less than 0.07 can cause weapon to be unable to fire. Setting to 0.07 instead.");
        }

        //burst sizes are usually doubled, but need to account for single-shot weapons
        modified_burstShotCount = original_BurstShotCount;
        if (modified_burstShotCount != 1)
            modified_burstShotCount *= 2;
        logText.AppendLine($"modified_burstShotCount : {modified_burstShotCount}");

        if (gunKind == APCEConstants.gunKinds.Turret || gunKind == APCEConstants.gunKinds.MachineGun)
            modified_recoilPattern = RecoilPattern.Mounted;
        else
            modified_recoilPattern = RecoilPattern.Regular;
        logText.AppendLine($"modified_recoilPattern : {modified_recoilPattern}");

        if (original_VerbProperties.verbClass == typeof(Verb_Shoot))
            modified_verbClass = typeof(Verb_ShootCE);
        else if (original_VerbProperties.verbClass == typeof(Verb_LaunchProjectile)
            || (original_VerbProperties.verbClass == typeof(Verb_ShootOneUse)))
            modified_verbClass = typeof(Verb_ShootCEOneUse);
        else if (original_VerbProperties.verbClass == typeof(Verb_SpewFire))
            modified_verbClass = typeof(Verb_SpewFire);
        else
        {
            modified_verbClass = typeof(Verb_ShootCE);
        }
        logText.AppendLine($"modified_verbClass : {modified_verbClass}");
    }

    public void CalculateCompFireModesValues(StringBuilder logText)
    {
        logText.AppendLine("Starting Fire Modes calculation.");

        if (modified_burstShotCount > 1)
            modified_aimedBurstShotCount = (int)(modified_burstShotCount / 2);
        else
            modified_aimedBurstShotCount = 1;
        logText.AppendLine($"modified_aimedBurstShotCount : {modified_aimedBurstShotCount}");

        if (gunKind != APCEConstants.gunKinds.Turret)
        {
            modified_aiUseBurstMode = true;
            modified_noSingleShot = false;
            modified_noSnapshot = false;
            modified_aiAimMode = AimMode.Snapshot;
        }
        else
        {
            modified_aiUseBurstMode = false;
            modified_noSingleShot = true;
            modified_noSnapshot = true;
            modified_aiAimMode = AimMode.AimedShot;
        }
        logText.AppendLine($"modified_aiUseBurstMode : {modified_aiUseBurstMode}");
        logText.AppendLine($"modified_noSingleShot : {modified_noSingleShot}");
        logText.AppendLine($"modified_noSnapshot : {modified_noSnapshot}");
        logText.AppendLine($"modified_aiAimMode : {modified_aiAimMode}");
    }

    public void CalculateCompAmmoUserValues(StringBuilder logText)
    {
        logText.AppendLine("Starting Ammo User calculation.");

        modified_loadedAmmoBulkFactor = 0;
        logText.AppendLine($"modified_loadedAmmoBulkFactor : {modified_loadedAmmoBulkFactor}");

        modified_throwMote = true;
        logText.AppendLine($"modified_throwMote : {modified_throwMote}");

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
        logText.AppendLine($"modified_magazineSize : {modified_magazineSize}");
        logText.AppendLine($"modified_reloadTime : {modified_reloadTime}");
        logText.AppendLine($"modified_throwMote : {modified_throwMote}");
        logText.AppendLine($"modified_reloadOneAtATime : {modified_reloadOneAtATime}");
    }

    public void CalculateWeaponTechMult(StringBuilder logText)
    {
        float techMult = 1f;
        switch (weaponThingDef.techLevel)
        {
            case TechLevel.Animal:
                techMult *= ModData.gunTechMultAnimal;
                break;
            case TechLevel.Neolithic:
                techMult *= ModData.gunTechMultNeolithic;
                break;
            case TechLevel.Medieval:
                techMult *= ModData.gunTechMultMedieval;
                break;
            case TechLevel.Industrial:
                techMult *= ModData.gunTechMultIndustrial;
                break;
            case TechLevel.Spacer:
                techMult *= ModData.gunTechMultSpacer;
                break;
            case TechLevel.Ultra:
                techMult *= ModData.gunTechMultUltratech;
                break;
            case TechLevel.Archotech:
                techMult *= ModData.gunTechMultArchotech;
                break;
            default:
                break;
        }
        this.techMult = techMult;
        logText.AppendLine($"Tech level multiplier of {techMult} based on tech level {weaponThingDef.techLevel}");
    }

    public override void ModToolAtIndex(int i)
    {
        // TODO logging
        base.ModToolAtIndex(i);
        modified_ToolPowers[i] *= ModData.weaponToolPowerMult;
        CalculateMinimumPenetrations(i);
        modified_ToolArmorPenetrationSharps[i] = Mathf.Clamp(modified_ToolArmorPenetrationSharps[i] * ModData.weaponToolSharpPenetration, floorArmorPenetrationSharp, 99999);
        modified_ToolArmorPenetrationBlunts[i] = Mathf.Clamp(modified_ToolArmorPenetrationBlunts[i] * ModData.weaponToolBluntPenetration, floorArmorPenetrationBlunt, 99999);
    }

    public void CalculateMinimumPenetrations(int i)
    {
        //TODO null checks
        // No tech mult for gun tools, wouldn't make much sense
        DamageArmorCategoryDef ac = modified_ToolCapacityDefs[i][0].VerbsProperties.First().meleeDamageDef.armorCategory;
        if (ac == DamageArmorCategoryDefOf.Sharp)
        {
            floorArmorPenetrationSharp = modified_ToolPowers[i] * 0.1f;
            floorArmorPenetrationBlunt = floorArmorPenetrationSharp;
        }
        else if (ac == APCEDefOfTwo.Blunt)
        {
            floorArmorPenetrationSharp = 0;
            floorArmorPenetrationBlunt = modified_ToolPowers[i] * 0.33f;
        }
        else //heat or maybe mods add new ones
        {
            floorArmorPenetrationSharp = 0;
            floorArmorPenetrationBlunt = 0;
        }
    }

    public void CalculateMortar()
    {
        // Nothing is really calculated, so skipping logging

        //statbases
        modified_SightsEfficiency = 0.5f;

        //comps
        modified_magazineSize = 1;
        modified_reloadTime = 5;
        modified_AmmoSetDef = APCEDefOf.AmmoSet_81mmMortarShell;

        //verb
        modified_verbClass = typeof(Verb_ShootMortarCE);
        modified_warmupTime = original_VerbProperties.warmupTime;
    }

    #region Grenade
    public void CalculateGrenade()
    {
        // TODO logging
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
        modified_grenadeDamage = modified_AmmoSetDef.ammoTypes[0].projectile.projectile.GetDamageAmount(1, null);
        modified_explosionRadius = modified_AmmoSetDef.ammoTypes[0].projectile.projectile.explosionRadius;
        //CompProperties_ExplosiveCE (for if the Thing is damaged)
        //CompProperties_Fragments
        //todo
        //projectile
        //thingClass CombatExtended.ProjectileCE_Explosive
        //projectilepropsCE
        //make sure comps aren't null, add Fragments comp if necessary -- TODO, explosive launcher needs fragments as well
    }

    
    public void PatchStatBases()
    {
        RemoveVanillaStatBases();

        GeneralUtils.AddOrChangeStat(ref weaponThingDef.statBases, StatDefOf.Mass, modified_Mass);
        GeneralUtils.AddOrChangeStat(ref weaponThingDef.statBases, CE_StatDefOf.Bulk, modified_Bulk);
        GeneralUtils.AddOrChangeStat(ref weaponThingDef.statBases, StatDefOf.RangedWeapon_Cooldown, modified_RangedWeaponCooldown);
        GeneralUtils.AddOrChangeStat(ref weaponThingDef.statBases, StatDefOf.WorkToMake, modified_WorkToMake);
        GeneralUtils.AddOrChangeStat(ref weaponThingDef.statBases, CE_StatDefOf.SightsEfficiency, modified_SightsEfficiency);
        GeneralUtils.AddOrChangeStat(ref weaponThingDef.statBases, CE_StatDefOf.ShotSpread, modified_ShotSpread);
        GeneralUtils.AddOrChangeStat(ref weaponThingDef.statBases, CE_StatDefOf.SwayFactor, modified_SwayFactor);
        GeneralUtils.AddOrChangeStat(ref weaponThingDef.statBases, CE_StatDefOf.BurstShotCount, modified_burstShotCount);
        if (stuffed)
        {
            GeneralUtils.AddOrChangeStat(ref weaponThingDef.statBases, CE_StatDefOf.StuffEffectMultiplierToughness, modified_WeaponToughness);
        }
        else
        {
            GeneralUtils.AddOrChangeStat(ref weaponThingDef.statBases, CE_StatDefOf.ToughnessRating, modified_WeaponToughness);
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

        if (modified_UsesAmmo)
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
            noSnapshot = modified_noSnapshot,
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
        GeneralUtils.CopyFields(weaponThingDef.Verbs[0], newVerbPropsCE);

        newVerbPropsCE.ticksBetweenBurstShots = modified_ticksBetweenBurstShots;
        newVerbPropsCE.range = modified_range;
        newVerbPropsCE.warmupTime = modified_warmupTime;
        newVerbPropsCE.burstShotCount = modified_burstShotCount;
        newVerbPropsCE.recoilPattern = modified_recoilPattern;
        newVerbPropsCE.verbClass = modified_verbClass;
        newVerbPropsCE.muzzleFlashScale = modified_muzzleFlashScale;
        //newVerbPropsCE.ejectsCasings //TODO
        //newVerbPropsCE.indirectFirePenalty //TODO
        newVerbPropsCE.defaultProjectile = modified_defaultProjectile;

        weaponThingDef.Verbs[0] = newVerbPropsCE;
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

        // TODO logging

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
        GeneralUtils.CopyFields(weaponThingDef, ammoGrenade);

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
        this.modified_defaultProjectile = modified_AmmoSetDef.ammoTypes[0].projectile;
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

    public void FixAmmoSet(StringBuilder logText)
    {
        logText.AppendLine("Starting FixAmmoSet");

        if (modified_AmmoSetDef != null)
        {
            logText.AppendLine($"Already has an AmmoSetDef: {modified_AmmoSetDef.defName}.");
            return;
        }

        if (modified_AmmoSetDefString != null)
        {
            logText.AppendLine($"Has no AmmoSetDef, but has the defName of one saved: {modified_AmmoSetDefString}. Trying to find this AmmoSetDef.");
            modified_AmmoSetDef = DefDatabase<AmmoSetDef>.GetNamedSilentFail(modified_AmmoSetDefString);
            if (modified_AmmoSetDef == null)
            {
                logText.AppendLine($"Failed to find a matching AmmoSetDef in DefDatabase.");
            }
            else
            {
                logText.AppendLine($"Successfully found and assigned AmmoSetDef.");
            }
        }

        if (modified_AmmoSetDef == null)
        {
            logText.AppendLine($"Generating new AmmoSet.");
            GenerateAmmoSet();
        }
    }

    public void FixDefaultProjectile(StringBuilder logText)
    {
        logText.AppendLine("Starting FixDefaultProjectile");

        if (modified_defaultProjectile != null)
        {
            logText.AppendLine("Already has a default projectile set.");
            return;
        }

        if (modified_defaultProjectileString != null)
        {
            logText.AppendLine("Has no default projectile set, but has the defName of one saved.");
            modified_defaultProjectile = DefDatabase<ThingDef>.GetNamedSilentFail(modified_defaultProjectileString);
            if (modified_defaultProjectile == null)
            {
                logText.AppendLine($"Failed to find a matching def in DefDatabase.");
            }
            else
            {
                logText.AppendLine($"Successfully found and assigned default projectile.");
            }
        }

        if (modified_defaultProjectile == null)
        {
            logText.AppendLine($"Using AmmoSetDef to assign default projectile.");
            if (modified_AmmoSetDef == null)
            {
                logText.AppendLine($"Somehow reached this point with no AmmoSetDef assigned.");
                FixAmmoSet(logText);
            }
            modified_defaultProjectile = modified_AmmoSetDef.ammoTypes[0].projectile;
            logText.AppendLine($"Default projectile: {modified_defaultProjectile.defName}.");
        }
    }

    //this returns a float instead of just setting the value, so that the customization window can suggest it if burst shot is changed from 1 to another number
    //public float CalculateRecoilAmount()
    //{

    //}
}
