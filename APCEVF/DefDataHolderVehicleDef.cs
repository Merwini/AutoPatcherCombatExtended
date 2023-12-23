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
        public DefDataHolderVehicleDef(ThingDef def) : base(def)
        {
        }

        VehicleDef vehicleDef;

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
        float modified_ArmorRatingSharp;
        float modified_ArmorRatingBlunt;
        float modified_ArmorRatingHeat;

        List<float> modified_ComponentArmorSharps = new List<float>();
        List<float> modified_ComponentArmorBlunts = new List<float>();
        List<int> modified_ComponentHealths = new List<int>();

        float modified_CargoCapacity;

        public override void GetOriginalData()
        {
            vehicleDef = def as VehicleDef;

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
            throw new NotImplementedException();
        }  
        
        //TODO
        public override void Patch()
        {
            throw new NotImplementedException();
        }

        public override StringBuilder PrepExport()
        {
            throw new NotImplementedException();
        }

        public override void ExportXML()
        {
            throw new NotImplementedException();
        }
    }
}
