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
    class DefDataHolderGene : DefDataHolder
    {
        public DefDataHolderGene(GeneDef def)
        {

        }

        float original_ArmorRatingSharp;
        float original_ArmorRatingBlunt;
        float original_ArmorRatingHeat;

        float modified_ArmorRatingSharp;
        float modified_ArmorRatingBlunt;
        float modified_ArmorRatingHeat;

        public override void GetOriginalData()
        {
            //TODO
        }

        public override void AutoCalculate()
        {
            //TODO
        }

        public override void Patch()
        {
            //TODO
        }

        public override void Export()
        {
            //TODO
        }




    }
}
