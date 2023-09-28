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
    class DefDataHolderMeleeWeapon : DefDataHolder
    {
        public DefDataHolderMeleeWeapon(ThingDef def) : base(def)
        {
        }

        internal ThingDef thingDef;

        float original_Mass;

        float modified_mass;
        float modified_Bulk;
        float modified_MeleeCounterParryBonus; //reminder - statbase
        float modified_MeleeDodgeChance;
        float modified_MeleeParryChance;
        float modified_MeleeCritChance;

        //TODO
        public override void GetOriginalData()
        {
            thingDef = def as ThingDef;

            original_Tools = thingDef.tools;
            original_Mass = thingDef.statBases.GetStatValueFromList(StatDefOf.Mass, 0);
        }

        public override void AutoCalculate()
        {
            throw new NotImplementedException();
        }

        public override void Patch()
        {
            throw new NotImplementedException();
        }
    }
}
