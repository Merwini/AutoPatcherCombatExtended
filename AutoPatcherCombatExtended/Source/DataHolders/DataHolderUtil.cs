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
    public static class DefDataHolderUtil
    {
        //Made my own so I can use it to change equippedStatOffSets, too
        public static void AddOrChangeStat(List<StatModifier> list, StatDef stat, float value)
        {
            int index = list.FindIndex(x => x.stat == stat);
            if (index != -1)
            {
                list[index].value = value;
            }
            //can't think of a use case where I would need to add a 0 value statmod, and adding this check will save vetting in the patch methods
            else if (value != 0)
            {
                list.Add(new StatModifier() { stat = stat, value = value });
            }
        }
    }
}
