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

        internal BodyShapeDef modified_BodyShape;
        internal string modified_BodyShapeDefString;

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

            if (!pawnDef.tools.NullOrEmpty())
            {
                original_Tools = pawnDef.tools.ToList();
            }

            original_ArmorRatingSharp = pawnDef.statBases.GetStatValueFromList(StatDefOf.ArmorRating_Sharp, 0);
            original_ArmorRatingBlunt = pawnDef.statBases.GetStatValueFromList(StatDefOf.ArmorRating_Blunt, 0);
            original_ArmorRatingHeat = pawnDef.statBases.GetStatValueFromList(StatDefOf.ArmorRating_Heat, 0);
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

            if (pawnDef.race.Humanlike)
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
            if (Scribe.mode == LoadSaveMode.LoadingVars
                || (Scribe.mode == LoadSaveMode.Saving && isCustomized == true))
            {
                if (Scribe.mode == LoadSaveMode.Saving && modified_BodyShape != null)
                {
                    modified_BodyShapeDefString = modified_BodyShape.ToString();
                }

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

                Scribe_Values.Look(ref modified_BodyShapeDefString, "modified_BodyShapeDefString");

                if (Scribe.mode == LoadSaveMode.LoadingVars && modified_BodyShapeDefString != null)
                {
                    modified_BodyShape = DefDatabase<BodyShapeDef>.AllDefsListForReading.First(bsd => bsd.defName.Equals(modified_BodyShapeDefString));
                    if (modified_BodyShape == null)
                    {
                        Log.Warning("Failed to find BodyShapeDef for race " + pawnDef.label + ". Defaulting to quadruped.");
                        modified_BodyShape = CE_BodyShapeDefOf.Quadruped;
                    }
                }
            }
            base.ExposeData();
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
                bodyShape = modified_BodyShape
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
