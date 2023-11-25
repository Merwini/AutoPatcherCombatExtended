using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;


namespace nuff.AutoPatcherCombatExtended
{
    public class DefDataHolderPawn : DefDataHolder
    {
        public DefDataHolderPawn(ThingDef def) : base(def)
        {
        }

        public override void AutoCalculate()
        {
            throw new NotImplementedException();
        }

        public override void ExportXML()
        {
            throw new NotImplementedException();
        }

        public override void GetOriginalData()
        {
            throw new NotImplementedException();
        }

        //TODO
        public override void Patch()
        {
            //TODO Comp notes:
                //All pawns get CompInventory and CompTacticalManager, as well as ITabInventory added to base. Humanlikes get CompAmmoGiver, CompSuppressable, and CompArmorDurability
            throw new NotImplementedException();
        }

        public override StringBuilder PrepExport()
        {
            throw new NotImplementedException();
        }
    }
}
