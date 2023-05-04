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
            if (parent.Map != null && parent.Position != null)
            {
                Thing replacementThing = ThingMaker.MakeThing(Props.thingToSpawn);
                IntVec3 position = parent.Position;
                Map map = parent.Map;
                replacementThing.stackCount = parent.stackCount;
                this.parent.Destroy(DestroyMode.Vanish);
                GenSpawn.Spawn(replacementThing, position, map);
            }
            base.CompTick();
        }
    }
}
