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
    class APCEDefOf
    {
        #pragma warning disable CS0649
        //TODO fill this bad boy up

        public static ThingCategoryDef Ammo;
        public static ThingCategoryDef AmmoAdvanced;
        public static ThingCategoryDef AmmoArrows;

        public static DamageDef ArrowVenom;
        public static DamageDef ArrowFire;
        public static DamageDef Beanbag;
        public static DamageDef PrometheumFlame;
        public static DamageDef Thermobaric;


        public static ThingDef Filth_Fuel;
        public static ThingDef FilthPrometheum;
        public static ThingDef Filth_FireFoam;

        public static SoundDef MortarBomb_Explode;

        public static ThingDef Fragment_Small;

        //industrial gun
        public static ThingDef APCE_Gun_Generic;

        public static AmmoSetDef AmmoSet_APCEGenericGun;
        public static AmmoDef Ammo_APCEGeneric_FMJ;
        public static AmmoDef Ammo_APCEGeneric_AP;
        public static AmmoDef Ammo_APCEGeneric_HP;
        public static AmmoDef Ammo_APCEGeneric_Incendiary;
        public static AmmoDef Ammo_APCEGeneric_HE;
        public static AmmoDef Ammo_APCEGeneric_Sabot;

        //shotgun
        public static ThingDef APCE_Shotgun_Generic;

        public static AmmoSetDef AmmoSet_APCEGenericShotgun;
        public static AmmoDef Ammo_APCEGeneric_Buck;
        public static AmmoDef Ammo_APCEGeneric_Slug;
        public static AmmoDef Ammo_APCEGeneric_Beanbag;
        public static AmmoDef Ammo_APCEGeneric_ElectroSlug;

        //spacer gun
        public static ThingDef APCE_SpacerGun_Generic;

        public static AmmoSetDef AmmoSet_APCEGenericSpacerGun;
        public static AmmoDef Ammo_APCEGeneric_Charged;
        public static AmmoDef Ammo_APCEGeneric_ChargedAP;
        public static AmmoDef Ammo_APCEGeneric_ChargedIon;

        //spacer shotgun
        //might not be needed

        //explosive launcher
        public static ThingDef APCE_ExplosiveLauncher_Generic;

        public static AmmoSetDef AmmoSet_APCEGenericExplosiveLauncher;
        public static AmmoDef Ammo_APCELauncher_Incendiary;
        public static AmmoDef Ammo_APCELauncher_Thermobaric;
        public static AmmoDef Ammo_APCELauncher_Foam;

        //bow
        public static ThingDef APCE_Bow_Generic;

        public static AmmoSetDef AmmoSet_Arrow;
        public static AmmoDef Ammo_Arrow_Stone;
        public static AmmoDef Ammo_Arrow_Steel;
        public static AmmoDef Ammo_Arrow_Plasteel;
        public static AmmoDef Ammo_Arrow_Venom;
        public static AmmoDef Ammo_Arrow_Flame;

        //grenade
        public static ThingDef APCE_GrenadeGeneric;

        //mortar
        public static AmmoSetDef AmmoSet_81mmMortarShell;
        public static ThingDef Bullet_81mmMortarShell_HE;

        //CE similarTo ammosets
        public static AmmoSetDef AmmoSet_RifleIntermediate;
        public static AmmoSetDef AmmoSet_ChargedRifle;
        public static AmmoSetDef AmmoSet_Shotgun;

        //StatDefs

    }
}
