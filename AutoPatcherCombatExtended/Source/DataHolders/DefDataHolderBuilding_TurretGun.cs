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
        //ConceptDef original_SpawnedConcentLearnOpportunity;

        internal float modified_FillPercent = 0.85f;
        internal float modified_TurretBurstCooldownTime;
        internal float modified_AimingAccuracy;
        //ConceptDef modified_SpawnedConceptLearnOpportunity;
        //string modified_SpawnedConceptLearnOpportunityString;

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

            //all this does is give you a Learning Helper lesson when you build one, not worth patching
            //original_SpawnedConcentLearnOpportunity = turretBase.building.spawnedConceptLearnOpportunity;
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
                //modified_SpawnedConceptLearnOpportunity = CE_ConceptDefOf.CE_MortarDirectFire;
            }
            else
            {
                modified_TurretBurstCooldownTime = original_TurretBurstCooldownTime * 0.5f;
                //modified_SpawnedConceptLearnOpportunity = original_SpawnedConcentLearnOpportunity;
            }

            modified_AimingAccuracy = 1f;

        }

        //TODO
        public override void Patch()
        {
            thingDef.fillPercent = modified_FillPercent;
            thingDef.building.turretBurstCooldownTime = modified_TurretBurstCooldownTime;
            DataHolderUtils.AddOrChangeStat(thingDef.statBases, CE_StatDefOf.AimingAccuracy, modified_AimingAccuracy);
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
                Scribe_Defs.Look(ref thingDef, "def");
                Scribe_Values.Look(ref modified_FillPercent, "modified_FillPercent", original_FillPercent);
                Scribe_Values.Look(ref modified_TurretBurstCooldownTime, "modified_TurretBurstCooldownTime", original_TurretBurstCooldownTime);
                Scribe_Values.Look(ref modified_AimingAccuracy, "modified_AimingAccuracy", 1);
            }
            base.ExposeData();
        }
    }
}
