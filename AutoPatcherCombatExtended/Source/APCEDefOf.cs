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
    public class APCEDefOf
    {
        #pragma warning disable CS0649

        public static ThingCategoryDef Ammo;
        public static ThingCategoryDef AmmoAdvanced;
        public static ThingCategoryDef AmmoArrows;
        public static ThingCategoryDef MortarShells;


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

        public static AmmoSetDef AmmoSet_RifleIntermediate;
        public static AmmoDef Ammo_RifleIntermediate_FMJ;
        public static AmmoDef Ammo_RifleIntermediate_AP;
        public static AmmoDef Ammo_RifleIntermediate_HP;
        public static AmmoDef Ammo_RifleIntermediate_Incendiary;
        public static AmmoDef Ammo_RifleIntermediate_HE;
        public static AmmoDef Ammo_RifleIntermediate_Sabot;

        //shotgun
        public static ThingDef APCE_Shotgun_Generic;

        public static AmmoSetDef AmmoSet_Shotgun;
        public static AmmoDef Ammo_Shotgun_Buck;
        public static AmmoDef Ammo_Shotgun_Slug;
        public static AmmoDef Ammo_Shotgun_Beanbag;
        public static AmmoDef Ammo_Shotgun_ElectroSlug;

        //spacer gun
        public static ThingDef APCE_SpacerGun_Generic;

        public static AmmoSetDef AmmoSet_ChargedRifle;
        public static AmmoDef Ammo_RifleCharged;
        public static AmmoDef Ammo_RifleCharged_AP;
        public static AmmoDef Ammo_RifleCharged_Ion;

        //spacer shotgun
        //might not be needed

        //explosive launcher
        public static ThingDef APCE_ExplosiveLauncher_Generic;

        public static AmmoSetDef AmmoSet_LauncherGrenade;
        public static AmmoDef Ammo_LauncherGrenade_HE;
        public static AmmoDef Ammo_LauncherGrenade_HEDP;
        public static AmmoDef Ammo_LauncherGrenade_EMP;
        public static AmmoDef Ammo_LauncherGrenade_Smoke;

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
        public static AmmoCategoryDef GrenadeHE;
        public static ThingCategoryDef Ammo81mmMortarShells;
        public static AmmoSetDef AmmoSet_81mmMortarShell;
        public static ThingDef Bullet_81mmMortarShell_HE;

        //StatDefs

        //misc ThingDefs
        public static ThingDef WoodLog;
        public static ThingDef Steel;
    }
}
