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
        public DefDataHolderBuilding_TurretGun(ThingDef def) : base(def)
        {
        }

        ThingDef turretBase;

        float original_FillPercent;
        float original_TurretBurstCooldownTime;
        //ConceptDef original_SpawnedConcentLearnOpportunity;

        float modified_FillPercent = 0.85f;
        float modified_TurretBurstCooldownTime;
        float modified_AimingAccuracy;
        //ConceptDef modified_SpawnedConceptLearnOpportunity;
        //string modified_SpawnedConceptLearnOpportunityString;

        public override void GetOriginalData()
        {
            turretBase = def as ThingDef;

            original_FillPercent = turretBase.fillPercent;
            original_TurretBurstCooldownTime = turretBase.building.turretBurstCooldownTime;

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

            if (!(turretBase.weaponTags == null) && (turretBase.weaponTags.Any(str => str.IndexOf("Artillery", StringComparison.OrdinalIgnoreCase) >= 0)))
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
            turretBase.fillPercent = modified_FillPercent;
            turretBase.building.turretBurstCooldownTime = modified_TurretBurstCooldownTime;
            DataHolderUtils.AddOrChangeStat(turretBase.statBases, CE_StatDefOf.AimingAccuracy, modified_AimingAccuracy);
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
                Scribe_Values.Look(ref modified_FillPercent, "modified_FillPercent", original_FillPercent);
                Scribe_Values.Look(ref modified_TurretBurstCooldownTime, "modified_TurretBurstCooldownTime", original_TurretBurstCooldownTime);
                Scribe_Values.Look(ref modified_AimingAccuracy, "modified_AimingAccuracy", 1);
            }
        }
    }
}
