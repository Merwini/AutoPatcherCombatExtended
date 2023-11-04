//using CombatExtended;
//using RimWorld;
//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Linq;
//using System.Reflection;
//using System.Text;
//using Verse;

//namespace nuff.AutoPatcherCombatExtended
//{
//    partial class APCEController
//    {
//        public static void PatchPawn(ThingDef def, APCEPatchLogger log)
//        {
//            try
//            {
//                #region ArmorValues
//                int sharpIndex = def.statBases.FindIndex(x => x.stat == StatDefOf.ArmorRating_Sharp);
//                int bluntIndex = def.statBases.FindIndex(x => x.stat == StatDefOf.ArmorRating_Blunt);

//                if (sharpIndex >= 0)
//                {
//                    def.statBases[sharpIndex].value *= APCESettings.pawnArmorSharpMult;
//                }
//                if (bluntIndex >= 0)
//                {
//                    def.statBases[bluntIndex].value *= APCESettings.pawnArmorBluntMult;
//                }
//                #endregion

//                #region MeleeStatBases
//                StatModifier mdc = new StatModifier();
//                mdc.stat = CE_StatDefOf.MeleeDodgeChance;
//                mdc.value = 1;
//                def.statBases.Add(mdc);

//                StatModifier mcc = new StatModifier();
//                mcc.stat = CE_StatDefOf.MeleeCritChance;
//                mcc.value = 1;
//                def.statBases.Add(mcc);

//                StatModifier mpc = new StatModifier();
//                mpc.stat = CE_StatDefOf.MeleeParryChance;
//                mcc.value = 1;
//                def.statBases.Add(mpc);

//                StatModifier smokes = new StatModifier();
//                smokes.stat = CE_StatDefOf.SmokeSensitivity;
//                smokes.value = 1;
//                def.statBases.Add(smokes);
//                #endregion

//                #region Comps
//                if (def.comps == null)
//                {
//                    def.comps = new List<CompProperties>();
//                }

//                CompProperties_Suppressable cp_s = new CompProperties_Suppressable();
//                def.comps.Add(cp_s);

//                CompProperties cpg = new CompProperties { compClass = typeof(CompPawnGizmo) };
//                def.comps.Add(cpg);

//                if (def.race.intelligence != Intelligence.Animal)
//                {
//                    CompProperties_Inventory cp_i = new CompProperties_Inventory();
//                    def.comps.Add(cp_i);
//                }
//                #endregion

//                #region ModExtensions
//                //TODO think of a better way to evaluate what bodyshape to use
//                RacePropertiesExtensionCE rpece = new RacePropertiesExtensionCE();
//                if (def.race.intelligence != Intelligence.Animal)
//                {
//                    rpece.bodyShape = CE_BodyShapeDefOf.Humanoid;
//                }
//                else
//                {
//                    rpece.bodyShape = CE_BodyShapeDefOf.Quadruped;
//                }
//                #endregion

//                #region Tools
//                PatchAllTools(ref def.tools, true);
//                #endregion

//                log.PatchSucceeded();
//            }
//            catch (Exception ex)
//            {
//                log.PatchFailed(def.defName, ex);
//            }
//        }

        
//    }
//}