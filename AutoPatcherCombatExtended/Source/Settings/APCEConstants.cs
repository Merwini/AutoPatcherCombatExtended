using CombatExtended;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using Verse;

namespace nuff.AutoPatcherCombatExtended
{
    [StaticConstructorOnStartup]
    public class APCEConstants
    {
        public enum SettingsTabs
        {
            General_Settings,
            Balance_Control,
            Modlist,
        }

        public enum ModSettingsTabs
        {
            General_Settings,
            Balance_Control,
            Deflist,
        }

        public enum BalanceTabs
        {
            Apparel,
            Weapons,
            Pawns,
            Other,
        }
        public enum BalanceWeaponTabs
        {
            Ranged,
            Melee
        }
        public enum gunKinds
        {
            Default,
            Bow,
            Grenade,
            Handgun,
            SMG,
            assaultRifle,
            Shotgun,
            precisionRifle,
            MachineGun,
            ExplosiveLauncher,
            Turret,
            Mortar,
            BeamGun,
            Flamethrower,
            Other
        }

        public enum NeedsPatch
        {
            yes,
            no,
            ignore,
            unsure
        }

        public enum VanillaStatBases //StatModifiers used by vanilla and not CE
        {
            AccuracyTouch,
            AccuracyShort,
            AccuracyMedium,
            AccuracyLong,
        }

        public enum CEStatBases //StatModifiers used by CE but not vanilla
        {
            SightsEfficiency,
            ShotSpread,
            SwayFactor,
            Bulk,
            TicksBetweenBurstShots,
            BurstShotCount,
            Recoil,
            ReloadTime
        }
        public enum SharedStatBases //StatModifiers used by both vanilla and CE
        {
            MaxHitPoints,
            Flammability,
            DeteriorationRate,
            Beauty,
            SellPriceFactor,
            MarketValue,
            Mass,
            RangedWeapon_Cooldown,
            WorkToMake
        }

        public enum ThingClasses
        {
            BulletCE,
            ProjectileCE_Explosive,
            ProjectileCE,
            //ProjectileCE_Bursting,
            //ProjectileCE_BunkerBuster
        }

        public enum DefTypes
        {
            Apparel,
            Building_TurretGun,
            Gene,
            Hediff,
            MeleeWeapon,
            MortarShell,
            Pawn,
            PawnKind,
            RangedWeapon,
            Stuff,
            Vehicle,
            VehicleTurret
        }

        public enum DefNameOrLabel
        {
            defName,
            label
        }

        public enum LoggingLevel
        {
            None,
            Normal,
            Verbose
        }
    }
}