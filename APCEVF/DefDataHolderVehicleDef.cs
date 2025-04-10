using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using Vehicles;

namespace nuff.AutoPatcherCombatExtended.VF
{
    public class DefDataHolderVehicleDef : DefDataHolder
    {
        public DefDataHolderVehicleDef()
        {
        }

        public DefDataHolderVehicleDef(VehicleDef def) : base(def)
        {
        }

        internal VehicleDef vehicleDef;

        //original values
        float original_ArmorRatingSharp;
        float original_ArmorRatingBlunt;
        float original_ArmorRatingHeat;

        List<float> original_ComponentArmorSharps = new List<float>();
        List<float> original_ComponentArmorBlunts = new List<float>();
        List<int> original_ComponentHealths = new List<int>();

        float original_CargoCapacity;
        int cargoIndex;

        //modified values
        internal float modified_ArmorRatingSharp;
        internal float modified_ArmorRatingBlunt;
        internal float modified_ArmorRatingHeat;

        internal List<float> modified_ComponentArmorSharps = new List<float>();
        internal List<float> modified_ComponentArmorBlunts = new List<float>();
        internal List<int> modified_ComponentHealths = new List<int>();

        internal float modified_CargoCapacity;

        public override void GetOriginalData()
        {
            //constructed by APCEController, def assigned by constructor
            if (def != null && vehicleDef == null)
            {
                this.vehicleDef = def as VehicleDef;
            }
            //constructed by SaveLoad, thingDef loaded from xml
            else if (vehicleDef != null && def == null)
            {
                def = vehicleDef;
            }

            original_ArmorRatingSharp = vehicleDef.statBases.GetStatValueFromList(StatDefOf.ArmorRating_Sharp, 0);
            original_ArmorRatingBlunt = vehicleDef.statBases.GetStatValueFromList(StatDefOf.ArmorRating_Blunt, 0);
            original_ArmorRatingHeat = vehicleDef.statBases.GetStatValueFromList(StatDefOf.ArmorRating_Heat, 0);

            original_ComponentArmorSharps.Clear();
            original_ComponentArmorBlunts.Clear();
            original_ComponentHealths.Clear();
            for (int i = 0; i < vehicleDef.components.Count; i++)
            {
                original_ComponentArmorSharps.Add(vehicleDef.components[i].armor.GetStatValueFromList(StatDefOf.ArmorRating_Sharp, 0));
                original_ComponentArmorBlunts.Add(vehicleDef.components[i].armor.GetStatValueFromList(StatDefOf.ArmorRating_Blunt, 0));
                original_ComponentHealths.Add(vehicleDef.components[i].health);
            }

            for (int i = 0; i < vehicleDef.vehicleStats.Count; i++)
            {
                if (vehicleDef.vehicleStats[i].statDef == VehicleStatDefOf.CargoCapacity)
                {
                    cargoIndex = i;
                    original_CargoCapacity = vehicleDef.vehicleStats[i].value;
                    break;
                }
            }
        }

        public override void AutoCalculate()
        {
            modified_ArmorRatingSharp = original_ArmorRatingSharp * modData.vehicleSharpMult;
            modified_ArmorRatingBlunt = original_ArmorRatingBlunt * modData.vehicleBluntMult;
            modified_ArmorRatingHeat = original_ArmorRatingHeat;

            modified_ComponentArmorSharps.Clear();
            modified_ComponentArmorBlunts.Clear();
            modified_ComponentHealths.Clear();

            for (int i = 0; i < vehicleDef.components.Count; i++)
            {
                modified_ComponentArmorSharps.Add(original_ComponentArmorSharps[i] * modData.vehicleSharpMult);
                modified_ComponentArmorBlunts.Add(original_ComponentArmorBlunts[i] * modData.vehicleBluntMult);
                modified_ComponentHealths.Add((int)(original_ComponentHealths[i] * modData.vehicleHealthMult));
            }

            modified_CargoCapacity = original_CargoCapacity;
        }  
        
        public override void Patch()
        {
            PatchVehicleStatBases();
            PatchVehicleComponents();
            PatchVehicleStats();
        }

        public override StringBuilder ExportXML()
        {
            Log.Warning("Patch export for Vehicle Defs not yet implemented");
            return null;
        }

        public override void ExposeData()
        {
            Scribe_Defs.Look(ref vehicleDef, "vehicleDef");
            Scribe_Values.Look(ref modified_ArmorRatingSharp, "modified_ArmorRatingSharp", 0f);
            Scribe_Values.Look(ref modified_ArmorRatingBlunt, "modified_ArmorRatingBlunt", 0f);
            Scribe_Values.Look(ref modified_ArmorRatingHeat, "modified_ArmorRatingHeat", 0f);

            Scribe_Collections.Look(ref modified_ComponentArmorSharps, "modified_ComponentArmorSharps", LookMode.Value);
            Scribe_Collections.Look(ref modified_ComponentArmorBlunts, "modified_ComponentArmorBlunts", LookMode.Value);
            Scribe_Collections.Look(ref modified_ComponentHealths, "modified_ComponentHealths", LookMode.Value);

            Scribe_Values.Look(ref modified_CargoCapacity, "modified_CargoCapacity", 0f);
            base.ExposeData();
        }

        internal void PatchVehicleStatBases()
        {
            DataHolderUtils.AddOrChangeStat(vehicleDef.statBases, StatDefOf.ArmorRating_Sharp, modified_ArmorRatingSharp);
            DataHolderUtils.AddOrChangeStat(vehicleDef.statBases, StatDefOf.ArmorRating_Blunt, modified_ArmorRatingBlunt);
            DataHolderUtils.AddOrChangeStat(vehicleDef.statBases, StatDefOf.ArmorRating_Heat, modified_ArmorRatingHeat);
        }

        internal void PatchVehicleComponents()
        {
            for (int i = 0; i < vehicleDef.components.Count; i++)
            {
                DataHolderUtils.AddOrChangeStat(vehicleDef.components[i].armor, StatDefOf.ArmorRating_Sharp, modified_ComponentArmorSharps[i]);
                DataHolderUtils.AddOrChangeStat(vehicleDef.components[i].armor, StatDefOf.ArmorRating_Blunt, modified_ComponentArmorBlunts[i]);
                vehicleDef.components[i].health = modified_ComponentHealths[i];
            }
        }

        internal void PatchVehicleStats()
        {
            vehicleDef.vehicleStats[cargoIndex].value = modified_CargoCapacity;
        }
    }
}
