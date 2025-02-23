using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using CombatExtended;

namespace nuff.AutoPatcherCombatExtended
{
    public class DefDataHolderMortarShell : DefDataHolder
    {
        public DefDataHolderMortarShell()
        {
        }

        public DefDataHolderMortarShell(ThingDef def) : base(def)
        {
        }

        ThingDef thingDef;

        ThingDef original_projectile;



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

            original_projectile = thingDef.projectileWhenLoaded;
        }

        public override void AutoCalculate()
        {
            throw new NotImplementedException();
        }

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

        public override void ExposeData()
        {
            Scribe_Defs.Look(ref thingDef, "thingDef");

            base.ExposeData();
        }
    }
}
