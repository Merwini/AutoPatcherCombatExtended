using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace nuff.AutoPatcherCombatExtended
{
    class CompProperties_GunAmmoFixer : CompProperties
    {
        public CompProperties_GunAmmoFixer()
        {
            compClass = typeof(CompGunAmmoFixer);
        }
    }
}
