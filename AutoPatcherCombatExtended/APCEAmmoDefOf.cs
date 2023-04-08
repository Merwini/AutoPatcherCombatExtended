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
    [DefOf]
    class APCEAmmoDefOf
    {
        #pragma warning disable CS0649
        //TODO fill this bad boy up

        //industrial gun
        public static ThingDef APCE_Gun_Generic;

        public static AmmoSetDef APCE_Generic_AmmoSetGun;
        public static AmmoDef Ammo_APCEGeneric_FMJ;
        public static AmmoDef Ammo_APCEGeneric_AP;
        public static AmmoDef Ammo_APCEGeneric_HP;
        public static AmmoDef Ammo_APCEGeneric_Incendiary;
        public static AmmoDef Ammo_APCEGeneric_HE;
        public static AmmoDef Ammo_APCEGeneric_Sabot;

        //shotgun
        public static ThingDef APCE_Shotgun_Generic;

        public static AmmoSetDef APCE_Generic_AmmoSetShotgun;
        public static AmmoDef Ammo_APCEGeneric_Buck;
        public static AmmoDef Ammo_APCEGeneric_Slug;
        public static AmmoDef Ammo_APCEGeneric_Beanbag;
        public static AmmoDef Ammo_APCEGeneric_ElectroSlug;

    }
}
