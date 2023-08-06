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
            Modlist,
            Balance_Control,
        }
        public enum BalanceTabs
        {
            Apparel,
            Weapons,
            Pawns,
            Hediffs,
        }
        public enum BalanceWeaponTabs
        {
            Ranged,
            Melee
        }
        public enum gunKinds
        {
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
            Other
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
    }
}