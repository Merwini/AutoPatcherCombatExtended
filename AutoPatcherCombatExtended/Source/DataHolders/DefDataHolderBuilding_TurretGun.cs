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
    public class DefDataHolderBuilding_TurretGun : DefDataHolder
    {
        public DefDataHolderBuilding_TurretGun()
        {
            //empty constructor for use by SaveLoad
        }

        public DefDataHolderBuilding_TurretGun(ThingDef def) : base(def)
        {
        }

        ThingDef thingDef;

        float original_FillPercent;
        float original_TurretBurstCooldownTime;

        internal float modified_FillPercent = 0.85f;
        internal float modified_TurretBurstCooldownTime;
        internal float modified_AimingAccuracy;

        public override void GetOriginalData()
        {
            //constructed by APCEController, def assigned by constructor
            if (def != null && thingDef == null)
            {
                this.thingDef = def as ThingDef;
            }
            //constructed by SaveLoad, thingDef loaded from xml
            else if (thingDef != null && def == null)
            {
                def = thingDef;
            }

            original_FillPercent = thingDef.fillPercent;
            original_TurretBurstCooldownTime = thingDef.building.turretBurstCooldownTime;
        }

        public override void AutoCalculate()
        {
            //if less than 0.85, will just set to the default 0.85, which is 1.49m in CE, enough to shoot over barricades but still be shot over by pawns.
            if (original_FillPercent > 0.85)
            {
                modified_FillPercent = original_FillPercent;
            }

            if (!(thingDef.weaponTags == null) && (thingDef.weaponTags.Any(str => str.IndexOf("Artillery", StringComparison.OrdinalIgnoreCase) >= 0)))
            {
                modified_TurretBurstCooldownTime = 2;
            }
            else
            {
                modified_TurretBurstCooldownTime = original_TurretBurstCooldownTime * 0.5f;
            }

            modified_AimingAccuracy = 1f;
        }

        public override void Patch()
        {
            thingDef.fillPercent = modified_FillPercent;
            thingDef.building.turretBurstCooldownTime = modified_TurretBurstCooldownTime;
            DataHolderUtils.AddOrChangeStat(thingDef.statBases, CE_StatDefOf.AimingAccuracy, modified_AimingAccuracy);
        }

        public override StringBuilder ExportXML()
        {
            xml = DataHolderUtils.GetXmlForDef(thingDef);

            patchOps = new List<string>();
            patchOps.Add(APCEPatchExport.AddOrReplaceXmlNodeWhitespace(xml, "statBases", "AimingAccuracy", modified_AimingAccuracy));

            if (modified_FillPercent != original_FillPercent)
            {
                patchOps.Add(MakeFillPercentPatch());
            }

            if (modified_TurretBurstCooldownTime != original_TurretBurstCooldownTime)
            {
                patchOps.Add(MakeTurretBurstCoolDownTimePatch());
            }

            base.ExportXML();

            return patch;

            string MakeFillPercentPatch()
            {
                if (modified_FillPercent == original_FillPercent)
                {
                    return null;
                }

                StringBuilder patch = new StringBuilder();

                bool nodeExists = xml.SelectSingleNode("fillPercent") != null;

                string xpath = $"Defs/ThingDef[defName=\"{defName}\"]{(nodeExists ? "/fillPercent" : "")}";

                patch.AppendLine($"\t<Operation Class=\"{(nodeExists ? "PatchOperationReplace" : "PatchOperationAdd")}\">");
                patch.AppendLine($"\t\t<xpath>{xpath}</xpath>");
                patch.AppendLine("\t\t<value>");
                patch.AppendLine($"\t\t\t<fillPercent>{modified_FillPercent}</fillPercent>");
                patch.AppendLine("\t\t</value>");
                patch.AppendLine("\t</Operation>");
                patch.AppendLine();

                return patch.ToString();
            }

            string MakeTurretBurstCoolDownTimePatch()
            {
                if (modified_TurretBurstCooldownTime == original_TurretBurstCooldownTime)
                {
                    return null;
                }

                StringBuilder patch = new StringBuilder();

                bool nodeExists = xml.SelectSingleNode("building")?.SelectSingleNode("turretBurstCooldownTime") != null;

                string xpath = $"Defs/ThingDef[defName=\"{defName}\"]/building{(nodeExists ? "/turretBurstCooldownTime" : "")}";

                patch.AppendLine($"\t<Operation Class=\"{(nodeExists ? "PatchOperationReplace" : "PatchOperationAdd")}\">");
                patch.AppendLine($"\t\t<xpath>{xpath}</xpath>");
                patch.AppendLine("\t\t<value>");
                patch.AppendLine($"\t\t\t<turretBurstCooldownTime>{modified_TurretBurstCooldownTime}</turretBurstCooldownTime>");
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
                Scribe_Defs.Look(ref thingDef, "def");
                Scribe_Values.Look(ref modified_FillPercent, "modified_FillPercent", original_FillPercent);
                Scribe_Values.Look(ref modified_TurretBurstCooldownTime, "modified_TurretBurstCooldownTime", original_TurretBurstCooldownTime);
                Scribe_Values.Look(ref modified_AimingAccuracy, "modified_AimingAccuracy", 1);
            }
            base.ExposeData();
        }
    }
}
