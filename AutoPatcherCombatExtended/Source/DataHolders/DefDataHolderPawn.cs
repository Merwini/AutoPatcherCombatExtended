using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using CombatExtended;
using UnityEngine;


namespace nuff.AutoPatcherCombatExtended
{
    public class DefDataHolderPawn : DefDataHolder
    {
        public DefDataHolderPawn()
        {
            //empty constructor for use by SaveLoad
        }

        public DefDataHolderPawn(ThingDef def) : base(def)
        {
        }

        ThingDef pawnDef;

        float original_ArmorRatingSharp;
        float original_ArmorRatingBlunt;
        float original_ArmorRatingHeat;

        internal float modified_ArmorRatingSharp;
        internal float modified_ArmorRatingBlunt;
        internal float modified_ArmorRatingHeat;

        internal float modified_MeleeDodgeChance;
        internal float modified_MeleeParryChance;
        internal float modified_MeleeCritChance;

        internal float modified_SmokeSensitivity;
        internal float modified_Suppressability;
        internal float modified_NightVisionEfficiency;
        internal float modified_ReloadSpeed;
        internal float modified_AimingAccuracy;

        internal float modified_CarryWeight;
        internal float modified_CarryBulk;

        internal BodyShapeDef modified_BodyShapeDef;

        float floorArmorPenetrationSharp;
        float floorArmorPenetrationBlunt;

        public override void GetOriginalData()
        {
            //constructed by APCEController, def assigned by constructor
            if (def != null && pawnDef == null)
            {
                this.pawnDef = def as ThingDef;
            }
            //constructed by SaveLoad, thingDef loaded from xml
            else if (pawnDef != null && def == null)
            {
                def = pawnDef;
            }

            StartNewLogEntry();
            logBuilder.AppendLine($"Starting GetOriginalData log entry for {def?.defName ?? "NULL DEF"}");

            try
            {
                if (!pawnDef.tools.NullOrEmpty())
                {
                    original_Tools = pawnDef.tools.ToList();
                }

                original_ArmorRatingSharp = pawnDef.statBases.GetStatValueFromList(StatDefOf.ArmorRating_Sharp, 0);
                original_ArmorRatingBlunt = pawnDef.statBases.GetStatValueFromList(StatDefOf.ArmorRating_Blunt, 0);
                original_ArmorRatingHeat = pawnDef.statBases.GetStatValueFromList(StatDefOf.ArmorRating_Heat, 0);
            }
            catch (Exception ex)
            {
                logBuilder.AppendLine($"Exception in GetOriginalData for: {def?.defName ?? "NULL DEF"}");
                logBuilder.AppendLine(ex.ToString());
                threwError = true;
            }
            finally
            {
                //TODO verbose logging
                PrintLog();
            }
        }

        public override void AutoCalculate()
        {
            StartNewLogEntry();
            logBuilder.AppendLine($"Starting AutoCalculate log entry for ammoset for {def?.defName ?? "NULL DEF"}");

            try
            {
                modified_ArmorRatingSharp = original_ArmorRatingSharp * ModData.pawnArmorSharpMult;
                modified_ArmorRatingBlunt = original_ArmorRatingBlunt * ModData.pawnArmorBluntMult;
                modified_ArmorRatingHeat = original_ArmorRatingHeat;


                modified_SmokeSensitivity = 1;
                modified_Suppressability = 1;
                modified_NightVisionEfficiency = 0;
                modified_ReloadSpeed = 1;
                modified_AimingAccuracy = 1;

                modified_CarryWeight = 40;
                modified_CarryBulk = 20;

                ClearModdedTools();
                for (int i = 0; i < original_Tools.Count; i++)
                {
                    ModToolAtIndex(i);
                }

                if (pawnDef.race.Humanlike)
                {
                    modified_BodyShapeDef = CE_BodyShapeDefOf.Humanoid;

                    modified_MeleeDodgeChance = 1f;
                    modified_MeleeParryChance = 1f;
                    modified_MeleeCritChance = 1f;
                }
                else
                {//todo too lazy to make any sort of guessing algorithm
                    modified_BodyShapeDef = CE_BodyShapeDefOf.Quadruped;

                    modified_MeleeDodgeChance = 0.1f;
                    modified_MeleeParryChance = 0.1f;
                    modified_MeleeCritChance = 0.1f;
                }
            }
            catch (Exception ex)
            {
                logBuilder.AppendLine($"Exception in AutoCalculate for: {def?.defName ?? "NULL DEF"}");
                logBuilder.AppendLine(ex.ToString());
                threwError = true;
            }
            finally
            {
                //TODO verbose logging
                PrintLog();
            }
        }

        public override void ApplyPatch()
        {
            StartNewLogEntry();
            logBuilder.AppendLine($"Starting ApplyPatch log entry for ammoset for {def?.defName ?? "NULL DEF"}");

            try
            {
                PatchStatBases();

                pawnDef.tools.Clear();
                BuildTools();
                for (int i = 0; i < modified_Tools.Count; i++)
                {
                    pawnDef.tools.Add(modified_Tools[i]);
                }

                PatchModExtensions();

                PatchComps();

                PatchITabs();
            }
            catch (Exception ex)
            {
                logBuilder.AppendLine($"Exception in Patch for: {def?.defName ?? "NULL DEF"}");
                logBuilder.AppendLine(ex.ToString());
                threwError = true;
            }
            finally
            {
                //TODO verbose logging
                PrintLog();
            }
        }

        public override StringBuilder ExportXML()
        {
            xml = DataHolderUtils.GetXmlForDef(pawnDef);

            patchOps = new List<string>();

            patchOps.Add(APCEPatchExport.GeneratePatchOperationFor(xml, "statBases", "ArmorRating_Sharp", modified_ArmorRatingSharp));
            patchOps.Add(APCEPatchExport.GeneratePatchOperationFor(xml, "statBases", "ArmorRating_Blunt", modified_ArmorRatingBlunt));
            patchOps.Add(APCEPatchExport.GeneratePatchOperationFor(xml, "statBases", "ArmorRating_Heat", modified_ArmorRatingHeat));

            patchOps.Add(APCEPatchExport.GeneratePatchOperationFor(xml, "statBases", "MeleeDodgeChance", modified_MeleeDodgeChance));
            patchOps.Add(APCEPatchExport.GeneratePatchOperationFor(xml, "statBases", "MeleeParryChance", modified_MeleeParryChance));
            patchOps.Add(APCEPatchExport.GeneratePatchOperationFor(xml, "statBases", "MeleeCritChance", modified_MeleeCritChance));

            patchOps.Add(APCEPatchExport.GeneratePatchOperationFor(xml, "statBases", "SmokeSensitivity", modified_SmokeSensitivity));
            patchOps.Add(APCEPatchExport.GeneratePatchOperationFor(xml, "statBases", "Suppressability", modified_Suppressability));
            patchOps.Add(APCEPatchExport.GeneratePatchOperationFor(xml, "statBases", "NightVisionEfficiency", modified_NightVisionEfficiency));
            patchOps.Add(APCEPatchExport.GeneratePatchOperationFor(xml, "statBases", "ReloadSpeed", modified_ReloadSpeed));
            patchOps.Add(APCEPatchExport.GeneratePatchOperationFor(xml, "statBases", "AimingAccuracy", modified_AimingAccuracy));

            patchOps.Add(APCEPatchExport.GeneratePatchOperationFor(xml, "statBases", "CarryWeight", modified_CarryWeight));
            patchOps.Add(APCEPatchExport.GeneratePatchOperationFor(xml, "statBases", "CarryBulk", modified_CarryBulk));

            patchOps.Add(GenerateModExtensionPatch());

            patchOps.Add(GenerateToolPatchXML());

            base.ExportXML();

            return patch;

            string GenerateModExtensionPatch()
            {
                string xpath = $"Defs/ThingDef[defName=\"{defName}\"]";
                StringBuilder patch = new StringBuilder();

                patch.AppendLine("\t<Operation Class=\"PatchOperationAddModExtension\">");
                patch.AppendLine($"\t\t<xpath>{xpath}</xpath>");
                patch.AppendLine("\t\t<value>");
                patch.AppendLine("\t\t\t<li Class=\"CombatExtended.RacePropertiesExtensionCE\">");
                patch.AppendLine($"\t\t\t\t<bodyShape>{modified_BodyShapeDef}</bodyShape>");
                patch.AppendLine("\t\t\t</li>");
                patch.AppendLine("\t\t</value>");
                patch.AppendLine("\t</Operation>");
                patch.AppendLine();

                return patch.ToString();
            }
        }

        public override void ExposeData()
        {
            if (Scribe.mode == LoadSaveMode.LoadingVars
                || (Scribe.mode == LoadSaveMode.Saving && isCustomized == true))
            {
                Scribe_Defs.Look(ref pawnDef, "def");

                // Modified Armor Ratings
                Scribe_Values.Look(ref modified_ArmorRatingSharp, "modified_ArmorRatingSharp");
                Scribe_Values.Look(ref modified_ArmorRatingBlunt, "modified_ArmorRatingBlunt");
                Scribe_Values.Look(ref modified_ArmorRatingHeat, "modified_ArmorRatingHeat");

                // Melee Combat Modifiers
                Scribe_Values.Look(ref modified_MeleeDodgeChance, "modified_MeleeDodgeChance");
                Scribe_Values.Look(ref modified_MeleeParryChance, "modified_MeleeParryChance");
                Scribe_Values.Look(ref modified_MeleeCritChance, "modified_MeleeCritChance");

                // Other Modifiers
                Scribe_Values.Look(ref modified_SmokeSensitivity, "modified_SmokeSensitivity");
                Scribe_Values.Look(ref modified_Suppressability, "modified_Suppressability");
                Scribe_Values.Look(ref modified_NightVisionEfficiency, "modified_NightVisionEfficiency");
                Scribe_Values.Look(ref modified_ReloadSpeed, "modified_ReloadSpeed");
                Scribe_Values.Look(ref modified_AimingAccuracy, "modified_AimingAccuracy");

                // Carry Capacity Modifiers
                Scribe_Values.Look(ref modified_CarryWeight, "modified_CarryWeight");
                Scribe_Values.Look(ref modified_CarryBulk, "modified_CarryBulk");

                Scribe_Defs.Look(ref modified_BodyShapeDef, "modified_BodyShapeDef");
            }
            base.ExposeData();
        }

        public override void ModToolAtIndex(int i)
        {
            base.ModToolAtIndex(i);
            modified_ToolPowers[i] *= ModData.pawnToolPowerMult;
            CalculateMinimumPenetrations(i);
            modified_ToolArmorPenetrationSharps[i] = Mathf.Clamp(modified_ToolArmorPenetrationSharps[i] * ModData.pawnToolSharpPenetration, floorArmorPenetrationSharp, 99999);
            modified_ToolArmorPenetrationBlunts[i] = Mathf.Clamp(modified_ToolArmorPenetrationBlunts[i] * ModData.pawnToolBluntPenetration, floorArmorPenetrationBlunt, 99999);
        }

        public void CalculateMinimumPenetrations(int i)
        {
            //TODO null checks
            DamageArmorCategoryDef ac = modified_ToolCapacityDefs[i][0].VerbsProperties.First().meleeDamageDef.armorCategory;
            if (ac == DamageArmorCategoryDefOf.Sharp)
            {
                floorArmorPenetrationSharp = modified_ToolPowers[i] * 0.1f * pawnDef.race.baseBodySize;
                floorArmorPenetrationBlunt = floorArmorPenetrationSharp;
            }
            else if (ac == APCEDefOfTwo.Blunt)
            {
                floorArmorPenetrationSharp = 0;
                floorArmorPenetrationBlunt = modified_ToolPowers[i] * 0.25f * pawnDef.race.baseBodySize;
            }
            else //heat or maybe mods add new ones
            {
                floorArmorPenetrationSharp = 0;
                floorArmorPenetrationBlunt = 0;
            }
        }

        public void PatchStatBases()
        {
            DataHolderUtils.AddOrChangeStat(pawnDef.statBases, StatDefOf.ArmorRating_Sharp, modified_ArmorRatingSharp);
            DataHolderUtils.AddOrChangeStat(pawnDef.statBases, StatDefOf.ArmorRating_Blunt, modified_ArmorRatingBlunt);
            DataHolderUtils.AddOrChangeStat(pawnDef.statBases, StatDefOf.ArmorRating_Heat, modified_ArmorRatingHeat);

            DataHolderUtils.AddOrChangeStat(pawnDef.statBases, CE_StatDefOf.MeleeDodgeChance, modified_MeleeDodgeChance);
            DataHolderUtils.AddOrChangeStat(pawnDef.statBases, CE_StatDefOf.MeleeParryChance, modified_MeleeParryChance);
            DataHolderUtils.AddOrChangeStat(pawnDef.statBases, CE_StatDefOf.MeleeCritChance, modified_MeleeCritChance);

            DataHolderUtils.AddOrChangeStat(pawnDef.statBases, CE_StatDefOf.SmokeSensitivity, modified_SmokeSensitivity);
            DataHolderUtils.AddOrChangeStat(pawnDef.statBases, CE_StatDefOf.Suppressability, modified_Suppressability);
            DataHolderUtils.AddOrChangeStat(pawnDef.statBases, CE_StatDefOf.NightVisionEfficiency, modified_NightVisionEfficiency);
            DataHolderUtils.AddOrChangeStat(pawnDef.statBases, CE_StatDefOf.ReloadSpeed, modified_ReloadSpeed);
            DataHolderUtils.AddOrChangeStat(pawnDef.statBases, CE_StatDefOf.AimingAccuracy, modified_AimingAccuracy);

            DataHolderUtils.AddOrChangeStat(pawnDef.statBases, CE_StatDefOf.CarryWeight, modified_CarryWeight);
            DataHolderUtils.AddOrChangeStat(pawnDef.statBases, CE_StatDefOf.CarryBulk, modified_CarryBulk);
        }
        public void PatchModExtensions()
        {
            RacePropertiesExtensionCE racePropsExt = new RacePropertiesExtensionCE()
            {
                bodyShape = modified_BodyShapeDef
            };
            DataHolderUtils.AddOrReplaceExtension(pawnDef, racePropsExt);
        }

        public void PatchComps()
        {
            CompProperties_Inventory cp_inv = new CompProperties_Inventory();
            DataHolderUtils.AddOrReplaceCompProps(pawnDef, cp_inv);

            CompProperties_TacticalManager cp_tm = new CompProperties_TacticalManager();
            DataHolderUtils.AddOrReplaceCompProps(pawnDef, cp_tm);

            if (pawnDef.race.intelligence != Intelligence.Animal)
            {
                CompProperties_Suppressable cp_sup = new CompProperties_Suppressable();
                DataHolderUtils.AddOrReplaceCompProps(pawnDef, cp_sup);

                CompProperties cp_ag = new CompProperties()
                {
                    compClass = typeof(CompAmmoGiver)
                };
                DataHolderUtils.AddOrReplaceCompProps(pawnDef, cp_ag);

                CompProperties cp_pg = new CompProperties()
                {
                    compClass = typeof(CompPawnGizmo)
                };
                DataHolderUtils.AddOrReplaceCompProps(pawnDef, cp_pg);

                //TODO CompArmorDurability
            }
        }

        public void PatchITabs()
        {
            if (pawnDef.inspectorTabs == null)
            {
                pawnDef.inspectorTabs = new List<Type>();
            }
            int index = pawnDef.inspectorTabs.FindIndex(t => t.GetType() == typeof(ITab_Pawn_Gear));

            if (index != -1)
            {
                pawnDef.inspectorTabs[index] = typeof(ITab_Inventory);
            }
            else
            {
                pawnDef.inspectorTabs.Add(typeof(ITab_Inventory));
            }
        }

    }
}
