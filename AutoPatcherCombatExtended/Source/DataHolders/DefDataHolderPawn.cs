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

        ThingDef pawn;

        float original_ArmorRatingSharp;
        float original_ArmorRatingBlunt;
        float original_ArmorRatingHeat;

        float modified_ArmorRatingSharp;
        float modified_ArmorRatingBlunt;
        float modified_ArmorRatingHeat;

        float modified_MeleeDodgeChance;
        float modified_MeleeParryChance;
        float modified_MeleeCritChance;

        float modified_SmokeSensitivity;
        float modified_Suppressability;
        float modified_NightVisionEfficiency;
        float modified_ReloadSpeed;
        float modified_AimingAccuracy;

        float modified_CarryWeight;
        float modified_CarryBulk;

        BodyShapeDef modified_BodyShape;
        string modified_BodyShapeDefString;

        public override void GetOriginalData()
        {
            pawn = def as ThingDef;
            if (!pawn.tools.NullOrEmpty())
            {
                original_Tools = pawn.tools.ToList();
            }

            original_ArmorRatingSharp = pawn.statBases.GetStatValueFromList(StatDefOf.ArmorRating_Sharp, 0);
            original_ArmorRatingBlunt = pawn.statBases.GetStatValueFromList(StatDefOf.ArmorRating_Blunt, 0);
            original_ArmorRatingHeat = pawn.statBases.GetStatValueFromList(StatDefOf.ArmorRating_Heat, 0);
        }

        public override void AutoCalculate()
        {
            modified_ArmorRatingSharp = original_ArmorRatingSharp * modData.pawnArmorSharpMult;
            modified_ArmorRatingBlunt = original_ArmorRatingBlunt * modData.pawnArmorBluntMult;
            modified_ArmorRatingHeat = original_ArmorRatingHeat;

            modified_MeleeDodgeChance = 1;
            modified_MeleeParryChance = 1;
            modified_MeleeCritChance = 1;

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

            if (pawn.race.Humanlike)
            {
                modified_BodyShape = CE_BodyShapeDefOf.Humanoid;
            }
            else
            {//todo too lazy to make any sort of guessing algorithm
                modified_BodyShape = CE_BodyShapeDefOf.Quadruped;
            }
        }

        //TODO
        public override void Patch()
        {
            PatchStatBases();

            pawn.tools.Clear();
            BuildTools();
            for (int i = 0; i < modified_Tools.Count; i++)
            {
                pawn.tools.Add(modified_Tools[i]);
            }

            PatchModExtensions();

            PatchComps();

            PatchITabs();
        }

        public override StringBuilder PrepExport()
        {
            throw new NotImplementedException();
        }

        public override void ExportXML()
        {
            throw new NotImplementedException();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            if (Scribe.mode == LoadSaveMode.LoadingVars
                || (Scribe.mode == LoadSaveMode.Saving && isCustomized == true))
            {
                if (Scribe.mode == LoadSaveMode.Saving && modified_BodyShape != null)
                {
                    modified_BodyShapeDefString = modified_BodyShape.ToString();
                }

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

                Scribe_Values.Look(ref modified_BodyShapeDefString, "modified_BodyShapeDefString");

                if (Scribe.mode == LoadSaveMode.LoadingVars && modified_BodyShapeDefString != null)
                {
                    modified_BodyShape = DefDatabase<BodyShapeDef>.AllDefsListForReading.First(bsd => bsd.defName.Equals(modified_BodyShapeDefString));
                    if (modified_BodyShape == null)
                    {
                        Log.Warning("Failed to find BodyShapeDef for race " + pawn.label + ". Defaulting to quadruped.");
                        modified_BodyShape = CE_BodyShapeDefOf.Quadruped;
                    }
                }
            }
               
        }

        public override void ModToolAtIndex(int i)
        {
            base.ModToolAtIndex(i);
            modified_ToolPowers[i] *= modData.pawnToolPowerMult;
            modified_ToolArmorPenetrationSharps[i] = Mathf.Clamp(modified_ToolArmorPenetrationSharps[i] * modData.pawnToolSharpPenetration, 0, 99999);
            modified_ToolArmorPenetrationBlunts[i] = Mathf.Clamp(modified_ToolArmorPenetrationBlunts[i] * modData.pawnToolBluntPenetration, 0, 99999);
        }

        public void PatchStatBases()
        {
            DataHolderUtils.AddOrChangeStat(pawn.statBases, StatDefOf.ArmorRating_Sharp, modified_ArmorRatingSharp);
            DataHolderUtils.AddOrChangeStat(pawn.statBases, StatDefOf.ArmorRating_Blunt, modified_ArmorRatingBlunt);
            DataHolderUtils.AddOrChangeStat(pawn.statBases, StatDefOf.ArmorRating_Heat, modified_ArmorRatingHeat);

            DataHolderUtils.AddOrChangeStat(pawn.statBases, CE_StatDefOf.MeleeDodgeChance, modified_MeleeDodgeChance);
            DataHolderUtils.AddOrChangeStat(pawn.statBases, CE_StatDefOf.MeleeParryChance, modified_MeleeParryChance);
            DataHolderUtils.AddOrChangeStat(pawn.statBases, CE_StatDefOf.MeleeCritChance, modified_MeleeCritChance);

            DataHolderUtils.AddOrChangeStat(pawn.statBases, CE_StatDefOf.SmokeSensitivity, modified_SmokeSensitivity);
            DataHolderUtils.AddOrChangeStat(pawn.statBases, CE_StatDefOf.Suppressability, modified_Suppressability);
            DataHolderUtils.AddOrChangeStat(pawn.statBases, CE_StatDefOf.NightVisionEfficiency, modified_NightVisionEfficiency);
            DataHolderUtils.AddOrChangeStat(pawn.statBases, CE_StatDefOf.ReloadSpeed, modified_ReloadSpeed);
            DataHolderUtils.AddOrChangeStat(pawn.statBases, CE_StatDefOf.AimingAccuracy, modified_AimingAccuracy);

            DataHolderUtils.AddOrChangeStat(pawn.statBases, CE_StatDefOf.CarryWeight, modified_CarryWeight);
            DataHolderUtils.AddOrChangeStat(pawn.statBases, CE_StatDefOf.CarryBulk, modified_CarryBulk);
        }
        public void PatchModExtensions()
        {
            RacePropertiesExtensionCE racePropsExt = new RacePropertiesExtensionCE()
            {
                bodyShape = modified_BodyShape
            };
            DataHolderUtils.AddOrReplaceExtension(pawn, racePropsExt);
        }

        public void PatchComps()
        {
            CompProperties_Inventory cp_inv = new CompProperties_Inventory();
            DataHolderUtils.AddOrReplaceCompProps(pawn, cp_inv);

            CompProperties_TacticalManager cp_tm = new CompProperties_TacticalManager();
            DataHolderUtils.AddOrReplaceCompProps(pawn, cp_tm);

            if (pawn.race.intelligence != Intelligence.Animal)
            {
                CompProperties_Suppressable cp_sup = new CompProperties_Suppressable();
                DataHolderUtils.AddOrReplaceCompProps(pawn, cp_sup);

                CompProperties cp_ag = new CompProperties()
                {
                    compClass = typeof(CompAmmoGiver)
                };
                DataHolderUtils.AddOrReplaceCompProps(pawn, cp_ag);

                CompProperties cp_pg = new CompProperties()
                {
                    compClass = typeof(CompPawnGizmo)
                };
                DataHolderUtils.AddOrReplaceCompProps(pawn, cp_pg);

                //TODO CompArmorDurability
            }
        }

        public void PatchITabs()
        {
            if (pawn.inspectorTabs == null)
            {
                pawn.inspectorTabs = new List<Type>();
            }
            int index = pawn.inspectorTabs.FindIndex(t => t.GetType() == typeof(ITab_Pawn_Gear));

            if (index != -1)
            {
                pawn.inspectorTabs[index] = typeof(ITab_Inventory);
            }
            else
            {
                pawn.inspectorTabs.Add(typeof(ITab_Inventory));
            }
        }

    }
}
