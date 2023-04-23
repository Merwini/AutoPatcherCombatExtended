using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace nuff.AutoPatcherCombatExtended
{
    public class CompReplaceMe : ThingComp
    {
        CompProperties_ReplaceMe Props => props as CompProperties_ReplaceMe;

        public override void CompTick()
        {
            if (true) //TODO add setting to enable/disable this behavior
            {

            }
            this.parent.Destroy(DestroyMode.Vanish);
            Thing replacementThing = ThingMaker.MakeThing(Props.thingToSpawn);
            GenSpawn.Spawn(replacementThing, parent.Position, parent.Map);

            base.CompTick();
        }
    }
}
