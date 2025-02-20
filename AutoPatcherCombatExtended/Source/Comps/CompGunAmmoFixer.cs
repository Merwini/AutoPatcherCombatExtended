using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using CombatExtended;

namespace nuff.AutoPatcherCombatExtended
{
    class CompGunAmmoFixer : ThingComp
    {
        //weapons don't tick... preserving this in case I want to repurpose the code
        //this comp will try to fix ammo for patched guns that are on the ground / in storage and thus tickable. separate comp for pawns to fix their equipped guns.
        CompAmmoUser compAmmo;
        CompProperties_AmmoUser compPropAmmo;

        public override void CompTick()
        {
            Log.Warning($"ticking comp on {parent.Label}");
            compAmmo = parent.TryGetComp<CompAmmoUser>();
            compPropAmmo = parent.def.comps.FirstOrDefault(comp => comp is CompProperties_AmmoUser) as CompProperties_AmmoUser;
            if (compAmmo != null && compPropAmmo != null)
            {
                if (compAmmo.UseAmmo && (compAmmo.CurrentAmmo == null || !compPropAmmo.ammoSet.ammoTypes.Any(link => link.ammo == compAmmo.CurrentAmmo)))
                {
                    compAmmo.CurrentAmmo = compPropAmmo.ammoSet.ammoTypes.First().ammo;
                }

                if (compAmmo.CurAmmoCount > compPropAmmo.magazineSize)
                {
                    compAmmo.curMagCountInt = compPropAmmo.magazineSize;
                }
            }

            //self-destruct when no longer needed, to not waste calculations ticking
            parent.comps.RemoveAll(comp => comp is CompGunAmmoFixer);
        }
    }
}
