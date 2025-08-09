using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using CombatExtended;

namespace nuff.AutoPatcherCombatExtended
{
    public class DefDataHolderGene : DefDataHolder
    {
        //TODO can genes add verbs? Need to look into whether they would need patching.
        public DefDataHolderGene()
        {
            //empty constructor for use by SaveLoad
        }

        public DefDataHolderGene(GeneDef def) : base(def)
        {
        }

        public GeneDef geneDef;

        float original_ArmorRatingSharp;
        float original_ArmorRatingBlunt;
        float original_ArmorRatingHeat;

        internal float modified_ArmorRatingSharp;
        internal float modified_ArmorRatingBlunt;
        internal float modified_ArmorRatingHeat;

        public override void GetOriginalData()
        {
            //constructed by APCEController, def assigned by constructor
            if (def != null && geneDef == null)
            {
                this.geneDef = def as GeneDef;
            }
            //constructed by SaveLoad, thingDef loaded from xml
            else if (geneDef != null && def == null)
            {
                def = geneDef;
            }

            StartNewLogEntry();
            logBuilder.AppendLine($"Starting GetOriginalData log entry for {def?.defName ?? "NULL DEF"}");

            try
            {
                original_ArmorRatingSharp = geneDef.statOffsets.GetStatValueFromList(StatDefOf.ArmorRating_Sharp, 0);
                original_ArmorRatingBlunt = geneDef.statOffsets.GetStatValueFromList(StatDefOf.ArmorRating_Blunt, 0);
                original_ArmorRatingHeat = geneDef.statOffsets.GetStatValueFromList(StatDefOf.ArmorRating_Heat, 0);
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
                modified_ArmorRatingSharp = original_ArmorRatingSharp * ModData.geneArmorSharpMult;
                modified_ArmorRatingBlunt = original_ArmorRatingBlunt * ModData.geneArmorBluntMult;
                modified_ArmorRatingHeat = original_ArmorRatingHeat;
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
                DataHolderUtils.AddOrChangeStat(geneDef.statOffsets, StatDefOf.ArmorRating_Sharp, modified_ArmorRatingSharp);
                DataHolderUtils.AddOrChangeStat(geneDef.statOffsets, StatDefOf.ArmorRating_Blunt, modified_ArmorRatingBlunt);
                DataHolderUtils.AddOrChangeStat(geneDef.statOffsets, StatDefOf.ArmorRating_Heat, modified_ArmorRatingHeat);
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
            xml = DataHolderUtils.GetXmlForDef(geneDef);

            patchOps = new List<string>();
            patchOps.Add(APCEPatchExport.GeneratePatchOperationFor(xml, "statOffsets", "ArmorRating_Sharp", modified_ArmorRatingSharp, original_ArmorRatingSharp));
            patchOps.Add(APCEPatchExport.GeneratePatchOperationFor(xml, "statOffsets", "ArmorRating_Blunt", modified_ArmorRatingBlunt, original_ArmorRatingBlunt));
            patchOps.Add(APCEPatchExport.GeneratePatchOperationFor(xml, "statOffsets", "ArmorRating_Heat", modified_ArmorRatingHeat, original_ArmorRatingHeat));

            base.ExportXML();

            return patch;
        }

        public override void ExposeData()
        {
            Scribe_Defs.Look(ref geneDef, "def");
            if (Scribe.mode == LoadSaveMode.LoadingVars
                || (Scribe.mode == LoadSaveMode.Saving && isCustomized == true))
            {
                Scribe_Values.Look(ref modified_ArmorRatingSharp, "modified_ArmorRatingSharp", 0f);
                Scribe_Values.Look(ref modified_ArmorRatingBlunt, "modified_ArmorRatingBlunt", 0f);
                Scribe_Values.Look(ref modified_ArmorRatingHeat, "modified_ArmorRatingHeat", 0f);
            }
            base.ExposeData();
        }
    }
}
