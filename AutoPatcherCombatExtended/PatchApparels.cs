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
        internal static void PatchApparel(ThingDef def, APCEPatchLogger log)
        {
            try
            {
                #region ArmorValues
                float armorTechMult = 1f;
                switch (def.techLevel)
                {
                    case TechLevel.Animal:
                        armorTechMult *= APCESettings.armorTechMultAnimal;
                        break;
                    case TechLevel.Neolithic:
                        armorTechMult *= APCESettings.armorTechMultNeolithic;
                        break;
                    case TechLevel.Medieval:
                        armorTechMult *= APCESettings.armorTechMultMedieval;
                        break;
                    case TechLevel.Industrial:
                        armorTechMult *= APCESettings.armorTechMultIndustrial;
                        break;
                    case TechLevel.Spacer:
                        armorTechMult *= APCESettings.armorTechMultSpacer;
                        break;
                    case TechLevel.Ultra:
                        armorTechMult *= APCESettings.armorTechMultUltratech;
                        break;
                    case TechLevel.Archotech:
                        armorTechMult *= APCESettings.armorTechMultArchotech;
                        break;
                    default:
                        break;
                }

                int sharpIndex = def.statBases.FindIndex(i => i.stat == StatDefOf.ArmorRating_Sharp);
                int bluntIndex = def.statBases.FindIndex(i => i.stat == StatDefOf.ArmorRating_Blunt);

                //using ifs avoids apparel with no armor values?
                if (sharpIndex >= 0)
                {
                    def.statBases[sharpIndex].value *= APCESettings.apparelSharpMult * armorTechMult;
                }
                if (bluntIndex >= 0)
                {
                    def.statBases[bluntIndex].value *= APCESettings.apparelBluntMult * armorTechMult;
                }
                #endregion

                #region BulkValues
                #pragma warning disable CS0219
                bool isSkin = false;
                bool isMid = false;
                bool isShell = false;
                float newBulk = 0;
                float newWornBulk = 0;

                foreach (ApparelLayerDef ald in def.apparel.layers)
                {
                    int massIndex = def.statBases.FindIndex(x => x.stat == StatDefOf.Mass);
                    float mass = 0;
                    if (massIndex >= 0)
                    {
                        mass = def.statBases[massIndex].value;
                    }
                    if (ald == ApparelLayerDefOf.OnSkin || ald.ToString().ToUpper().Contains("SKIN") || ald.ToString().ToUpper().Contains("STRAPPED"))
                    {
                        isSkin = true;
                    }
                    if (ald == ApparelLayerDefOf.Middle || ald.ToString().ToUpper().Contains("MID") || ald == ApparelLayerDefOf.Overhead)
                    {
                        isMid = true;
                        if (mass > 2)
                        {
                            newBulk += APCESettings.midBulkAdd;
                            newWornBulk += APCESettings.midWulkAdd;
                        }
                        if ((APCESettings.patchHeadgearLayers) && (ald == ApparelLayerDefOf.Overhead))
                        {
                            def.apparel.layers.Add(CE_ApparelLayerDefOf.OnHead);
                            if (def.thingCategories.Contains(ThingCategoryDefOf.ArmorHeadgear))
                            {
                                def.apparel.layers.Add(CE_ApparelLayerDefOf.StrappedHead);
                            }
                        }

                    }
                    if (ald == ApparelLayerDefOf.Shell || ald.ToString().ToUpper().Contains("SHELL") || ald.ToString().ToUpper().Contains("OUTER")) //had to add extra conditions to try to account for modded alien layers
                    {
                        isShell = true;
                        if (mass > 2)
                        {
                            if (newWornBulk == 0)
                            {
                                newBulk += APCESettings.shellBulkAdd;
                                newWornBulk += APCESettings.shellWulkAdd;
                            }
                            else
                            {
                                newBulk *= APCESettings.shellBulkMult;
                                newWornBulk *= APCESettings.shellWulkMult;
                            }
                        }
                    }
                }
                StatModifier statModBulk = new StatModifier();
                statModBulk.stat = StatDef.Named("Bulk");
                statModBulk.value = newBulk;

                StatModifier statModWornBulk = new StatModifier();
                statModWornBulk.stat = StatDef.Named("WornBulk");
                statModWornBulk.value = newWornBulk;

                def.AddOrChangeStat(statModBulk);
                def.AddOrChangeStat(statModWornBulk);
                #endregion

                #region HelmetMods             
                if (def.apparel.bodyPartGroups != null)
                {
                    if (def.apparel.bodyPartGroups.Any(bpgd =>
                    {
                        if (bpgd == BodyPartGroupDefOf.Eyes || bpgd == BodyPartGroupDefOf.FullHead)
                            return true;
                        else
                            return false;
                    }))
                    {
                        if (def.equippedStatOffsets == null)
                        {
                            def.equippedStatOffsets = new List<StatModifier>();
                        }
                        if (armorTechMult >= 1)
                        {
                            StatModifier statModSmoke = new StatModifier();
                            statModSmoke.stat = CE_StatDefOf.SmokeSensitivity;
                            statModSmoke.value = -1;
                            def.equippedStatOffsets.Add(statModSmoke);
                        }
                        if (armorTechMult >= 2)
                        {
                            StatModifier statModNightVision = new StatModifier();
                            statModNightVision.stat = CE_StatDefOf.NightVisionEfficiency;
                            statModNightVision.value = 0.6f;
                            def.equippedStatOffsets.Add(statModNightVision);
                        }
                    }
                }
                #endregion

                #region BodyArmorMods
                if (def.apparel.bodyPartGroups != null)
                {
                    if (def.apparel.bodyPartGroups.Any(bpgd => bpgd == BodyPartGroupDefOf.Torso))
                    {
                        if (def.equippedStatOffsets == null)
                        {
                            def.equippedStatOffsets = new List<StatModifier>();
                        }
                        if (armorTechMult >= 2)
                        {
                            StatModifier statModCarryWeight = new StatModifier();
                            statModCarryWeight.stat = CE_StatDefOf.CarryWeight;
                            statModCarryWeight.value = APCESettings.advancedArmorCarryWeight;
                            def.equippedStatOffsets.Add(statModCarryWeight);

                            StatModifier statModCarryBulk = new StatModifier();
                            statModCarryBulk.stat = CE_StatDefOf.CarryBulk;
                            statModCarryBulk.value = APCESettings.advancedArmorCarryBulk;
                            def.equippedStatOffsets.Add(statModCarryBulk);

                            if (!def.equippedStatOffsets.Any(eso => eso.stat == StatDefOf.ShootingAccuracyPawn))
                            {
                                StatModifier statModShootingAccuracy = new StatModifier();
                                statModShootingAccuracy.stat = StatDefOf.ShootingAccuracyPawn;
                                statModShootingAccuracy.value = APCESettings.advancedArmorShootingAccuracy;
                                def.equippedStatOffsets.Add(statModShootingAccuracy);
                            }
                        }
                    }
                }
                #endregion

                log.PatchSucceeded();
            }
            catch (Exception ex)
            {
                log.PatchFailed(def.defName, ex);
            }
        }
    }
}