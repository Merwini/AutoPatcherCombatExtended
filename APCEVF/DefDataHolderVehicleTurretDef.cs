using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using Vehicles;
using System.Reflection;

namespace nuff.AutoPatcherCombatExtended.VF
{
    public class DefDataHolderVehicleTurretDef : DefDataHolder
    {
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
        ThingDef modified_Projectile;
        float modified_WarmUpTimer;
        float modified_ReloadTimer;
        float modified_MinRange;
        float modified_MaxRange;
        int modified_MagazineCapacity;
        float modified_ChargePerAmmoCount;

        //CETurretDataDefModExtension values
        float modified_Speed;
        float modified_Sway;
        float modified_Spread;
        float modified_Recoil;
        float modified_ShotHeight;
        string modified_AmmoSetString;

        DefDataHolderAmmoSet ammoSetDataHolder;

        public override void GetOriginalData()
        {
            turretDef = def as VehicleTurretDef;

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
            turretDef.projectile = ammoSetDataHolder.GeneratedAmmoSetDef.ammoTypes[0].projectile;
            
            turretDef.projectileSpeed = modified_Speed;
            
            //This needs to be cleared. In vanilla, these shifts are just visual, but with CE they cause every shot to miss.
            turretDef.projectileShifting = new List<float>();


            PatchCEExtension();
        }

        public override StringBuilder PrepExport()
        {
            throw new NotImplementedException();
        }

        public override void ExportXML()
        {
            throw new NotImplementedException();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            //TODO
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
