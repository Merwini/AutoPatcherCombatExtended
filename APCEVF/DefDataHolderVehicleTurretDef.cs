using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using Vehicles;
using System.Reflection;
using CombatExtended;

namespace nuff.AutoPatcherCombatExtended.VF
{
    public class DefDataHolderVehicleTurretDef : DefDataHolder
    {
        public DefDataHolderVehicleTurretDef()
        {
        }

        public DefDataHolderVehicleTurretDef(Def def) : base(def)
        {
        }

        VehicleTurretDef turretDef;

        ThingDef pseudoweapon; 
        APCEConstants.gunKinds gunKind;

        //original values
        ThingDef original_Projectile;
        float original_WarmUpTimer;
        float original_ReloadTimer;
        float original_MinRange;
        float original_MaxRange;
        int original_MagazineCapacity;
        float original_ChargePerAmmoCount;
        float original_speed;
        List<ThingDef> original_ammunitions = new List<ThingDef>();

        //modified values
        internal float modified_WarmUpTimer;
        internal float modified_ReloadTimer;
        internal float modified_MinRange;
        internal float modified_MaxRange;
        internal int modified_MagazineCapacity;
        internal float modified_ChargePerAmmoCount;

        //CETurretDataDefModExtension values
        internal float modified_Speed;
        internal float modified_Sway;
        internal float modified_Spread;
        internal float modified_Recoil;
        internal float modified_ShotHeight;
        internal string modified_AmmoSetString;

        DefDataHolderAmmoSet ammoSetDataHolder;

        public override void GetOriginalData()
        {
            if (def != null && turretDef == null)
            {
                this.turretDef = def as VehicleTurretDef;
            }
            else if (turretDef != null && def == null)
            {
                def = turretDef;
            }

            original_Projectile = turretDef.projectile;
            original_WarmUpTimer = turretDef.warmUpTimer;
            original_ReloadTimer = turretDef.reloadTimer;
            original_MinRange = turretDef.minRange;
            original_MaxRange = turretDef.maxRange;
            original_MagazineCapacity = turretDef.magazineCapacity;
            original_ChargePerAmmoCount = turretDef.chargePerAmmoCount;
            original_speed = turretDef.projectileSpeed;
            if (turretDef.ammunition != null && turretDef.ammunition.AllowedDefCount != 0)
            {
                foreach (ThingDef ammu in turretDef.ammunition.AllowedThingDefs)
                {
                    original_ammunitions.Add(ammu);
                }
            }

            pseudoweapon = CreatePseudoWeapon(turretDef);
        }

        public override void AutoCalculate()
        {
            DetermineVehicleTurretKind();
            ammoSetDataHolder = new DefDataHolderAmmoSet(pseudoweapon, gunKind);
            
            modified_AmmoSetString = ammoSetDataHolder.GeneratedAmmoSetDef.defName;
            modified_Speed = ammoSetDataHolder.GeneratedAmmoSetDef.ammoTypes[0].projectile.projectile.speed;

            modified_Sway = 0.82f;
            modified_Spread = 0.01f;
            modified_Recoil = -1;
            modified_WarmUpTimer = original_WarmUpTimer;
            modified_ReloadTimer = original_ReloadTimer * 2f; //TODO maybe change based on gunKind
            modified_MinRange = original_MinRange;
            modified_MaxRange = original_MaxRange * 2f;
            modified_MagazineCapacity = original_MagazineCapacity;
            modified_ChargePerAmmoCount = 1;
            
            modified_ShotHeight = 2f;
        }

        public override void Patch()
        {
            turretDef.warmUpTimer = modified_WarmUpTimer;
            turretDef.reloadTimer = modified_ReloadTimer;
            turretDef.minRange = modified_MinRange;
            turretDef.maxRange = modified_MaxRange;
            turretDef.magazineCapacity = modified_MagazineCapacity;
            turretDef.chargePerAmmoCount = modified_ChargePerAmmoCount;
            turretDef.genericAmmo = false;
            turretDef.projectile = ammoSetDataHolder?.GeneratedAmmoSetDef.ammoTypes[0].projectile ?? DefDatabase<AmmoSetDef>.AllDefsListForReading.First(def => def.defName == modified_AmmoSetString).ammoTypes[0].projectile;
            
            turretDef.projectileSpeed = modified_Speed;
            
            //This needs to be cleared. In vanilla, these shifts are just visual, but with CE they cause every shot to miss.
            turretDef.projectileShifting = new List<float>();

            PatchCEExtension();
        }

        public override StringBuilder ExportXML()
        {
            Log.Warning("Patch export for Vehicle Turret Defs not yet implemented");
            return null;
        }

        public override void ExposeData()
        {
            Scribe_Defs.Look(ref turretDef, "turretDef");

            Scribe_Values.Look(ref modified_WarmUpTimer, "modified_WarmUpTimer");
            Scribe_Values.Look(ref modified_ReloadTimer, "modified_ReloadTimer");
            Scribe_Values.Look(ref modified_MinRange, "modified_MinRange");
            Scribe_Values.Look(ref modified_MaxRange, "modified_MaxRange");
            Scribe_Values.Look(ref modified_MagazineCapacity, "modified_MagazineCapacity");
            Scribe_Values.Look(ref modified_ChargePerAmmoCount, "modified_ChargePerAmmoCount");

            Scribe_Values.Look(ref modified_Speed, "modified_Speed");
            Scribe_Values.Look(ref modified_Sway, "modified_Sway");
            Scribe_Values.Look(ref modified_Spread, "modified_Spread");
            Scribe_Values.Look(ref modified_Recoil, "modified_Recoil");
            Scribe_Values.Look(ref modified_ShotHeight, "modified_ShotHeight");
            Scribe_Values.Look(ref modified_AmmoSetString, "modified_AmmoSetString");

            base.ExposeData();
        }

        public void PatchCEExtension()
        {
            CETurretDataDefModExtension extension = new CETurretDataDefModExtension()
            {
                speed = modified_Speed,
                sway = modified_Sway,
                spread = modified_Spread,
                recoil = modified_Recoil,
                shotHeight = modified_ShotHeight,
                ammoSet = modified_AmmoSetString
            };

            DataHolderUtils.AddOrReplaceExtension(turretDef, extension);
        }

        public ThingDef CreatePseudoWeapon(VehicleTurretDef def)
        {
            ThingDef td = new ThingDef();
            td.defName = def.defName + "_pw";
            td.label = def.label + "_pw";
            td.modContentPack = def.modContentPack;
            List<VerbProperties> newVerbs = new List<VerbProperties>();
            VerbProperties newVerb = new VerbProperties();
            newVerb.defaultProjectile = def.projectile;
            newVerbs.Add(newVerb);

            Type tv = typeof(ThingDef);
            FieldInfo vs = tv.GetField("verbs", BindingFlags.NonPublic | BindingFlags.Instance);
            vs.SetValue(td, newVerbs);
            return td;
        }

        public void DetermineVehicleTurretKind()
        {
            if (turretDef.ammunition != null)
            {
                if (turretDef.ammunition.Allows(APCEDefOf.WoodLog))
                {
                    //pre-industrial stuff
                    gunKind = APCEConstants.gunKinds.Bow;

                }
                else if (turretDef.ammunition.Allows(APCEDefOf.Steel) && turretDef.projectile.thingClass == typeof(Projectile_Explosive))
                {
                    gunKind = APCEConstants.gunKinds.ExplosiveLauncher;
                    modified_Speed = 120;
                }
                else if (turretDef.ammunition.Allows(APCEDefOf.Steel) && turretDef.projectile.thingClass == typeof(Bullet))
                {
                    gunKind = APCEConstants.gunKinds.MachineGun;
                    modified_Speed = 180;
                }
                else
                {
                    gunKind = APCEConstants.gunKinds.Other;
                }
            }
            else
            {
                gunKind = APCEConstants.gunKinds.Other;
            }
        }
    }
}
