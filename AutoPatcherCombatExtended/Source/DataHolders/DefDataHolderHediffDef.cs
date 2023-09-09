using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using CombatExtended;

namespace nuff.AutoPatcherCombatExtended
{
    class DefDataHolderHediffDef : DefDataHolder
    {
        float armorRatingSharp;
        float armorRatingBlunt;

        //TODO use more basic data structures so it can be serialized
        List<ToolCE> tools;

        //TODO
        public override void Reset()
        {
            throw new NotImplementedException();
        }
    }
}
