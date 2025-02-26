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
    public class DefDataHolderMortarShell : DefDataHolder
    {
        public DefDataHolderMortarShell()
        {
        }

        public DefDataHolderMortarShell(ThingDef def) : base(def)
        {
        }

        ThingDef thingDef;

        ThingDef original_projectile;
        CompProperties_Explosive original_compPropsExplosive;


        ThingDef new_shell;
        internal string modified_defName;
        internal int modified_stackLimit;

        internal DamageDef modified_damageDef;
        internal int modified_damageAmount;
        internal float modified_explosionRadius;
        internal bool modified_ai_IsIncendiary;
        //internal SoundDef modified_soundExplode;
        internal bool modified_applyDamageToExplosionCellNeighbors;
        internal int modified_aimHeightOffset;
        //TODO shellingProps

        internal bool modified_fragmentsBool;
        internal int modified_fragmentsLarge;
        internal int modified_fragmentsSmall;

        public override void GetOriginalData()
        {
            //constructed by APCEController, def assigned by constructor
            if (def != null && thingDef == null)
            {
                this.thingDef = def as ThingDef;
            }
            //constructed by SaveLoad, thingDef loaded from xml
            else if (thingDef != null && def == null)
            {
                def = thingDef;
            }

            original_projectile = thingDef.projectileWhenLoaded;
            original_compPropsExplosive = thingDef.GetCompProperties<CompProperties_Explosive>();
        }

        public override void AutoCalculate()
        {
            modified_defName = "APCE_Shell_" + thingDef.defName;
            modified_stackLimit = 25;

            modified_damageDef = original_projectile.projectile.damageDef;
            modified_damageAmount = original_projectile.projectile.damageAmountBase;
            modified_explosionRadius = original_projectile.projectile.explosionRadius;
            modified_ai_IsIncendiary = original_projectile.projectile.ai_IsIncendiary;
            modified_applyDamageToExplosionCellNeighbors = original_projectile.projectile.applyDamageToExplosionCellsNeighbors;
            modified_aimHeightOffset = 0;

            modified_fragmentsBool = false;
            modified_fragmentsLarge = 0;
            modified_fragmentsSmall = 0;
        }

        public override void Patch()
        {
            throw new NotImplementedException();
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
            Scribe_Defs.Look(ref thingDef, "thingDef");

            base.ExposeData();
        }

        public static AmmoDef MakeNewMortarAmmo(ThingDef def)
        {
            AmmoDef newAmmo = new AmmoDef();
            DataHolderUtils.CopyFields(def, newAmmo);
            newAmmo.defName = "APCE_Shell_" + def.defName;
            newAmmo.thingClass = typeof(CombatExtended.AmmoThing);
            newAmmo.shortHash = 0;
            newAmmo.modContentPack = APCESettings.thisMod;

            newAmmo.comps = new List<CompProperties>();
            foreach (CompProperties cp in def.comps) //slightly redundant, but the way CopyFields works causes problems with adding the ReplaceMe CompProp
            {
                newAmmo.comps.Add(cp);
            }

            ConvertCompProperties_Explosive(newAmmo);

            /* the mortar shell is not actually the projectile
            if (newAmmo.projectile != null)
            {
                newAmmo.projectile = ConvertPP(newAmmo.projectile);
            }
            */
            newAmmo.projectileWhenLoaded = MakeNewMortarProjectile(def.projectileWhenLoaded);
            newAmmo.projectileWhenLoaded.projectile.speed = 0;

            newAmmo.thingCategories.Add(APCEDefOf.Ammo81mmMortarShells); //TODO maybe remove old categories
            newAmmo.stackLimit = 25;
            newAmmo.cookOffFlashScale = 30;
            newAmmo.cookOffSound = APCEDefOf.MortarBomb_Explode;
            newAmmo.isMortarAmmo = true;
            newAmmo.menuHidden = false;
            newAmmo.ammoClass = MakeMortarAmmoCategoryDef(newAmmo);

            InjectedDefHasher.GiveShortHashToDef(newAmmo, typeof(AmmoDef));
            DefGenerator.AddImpliedDef<ThingDef>(newAmmo);

            def.description = def.description + "\n This mortar shell should be converted to a CE-compatible one as soon as it spawns in.";

            return newAmmo;
        }

        public static AmmoCategoryDef MakeMortarAmmoCategoryDef(ThingDef mortarShell)
        {
            AmmoCategoryDef newAmmoCat = new AmmoCategoryDef();
            newAmmoCat.defName = "APCE_AmmoCatDef_ " + mortarShell.defName;
            newAmmoCat.label = mortarShell.label;
            newAmmoCat.description = "Ammo category of " + mortarShell.label;

            InjectedDefHasher.GiveShortHashToDef(newAmmoCat, typeof(AmmoCategoryDef));
            DefGenerator.AddImpliedDef<AmmoCategoryDef>(newAmmoCat);

            return newAmmoCat;
        }

        public static ThingDef MakeNewMortarProjectile(ThingDef oldProjectile)
        {//WIP
            ThingDef newProjectile = new ThingDef();
            newProjectile.defName = ("APCE_Bullet_Shell_ " + oldProjectile.defName);
            newProjectile.label = oldProjectile.label;
            newProjectile.graphicData = oldProjectile.graphicData;
            PatchBaseBullet(newProjectile);
            newProjectile.projectile = ConvertPP(oldProjectile.projectile);

            InjectedDefHasher.GiveShortHashToDef(newProjectile, typeof(ThingDef));
            DefGenerator.AddImpliedDef<ThingDef>(newProjectile);

            return newProjectile;
        }

        public static void MakeMortarAmmoLink(AmmoDef ammoDef)
        {
            AmmoLink ammoDefLink = new AmmoLink(ammoDef, ammoDef.projectileWhenLoaded);
            AmmoSetDef ammoSet81mm = APCEDefOf.AmmoSet_81mmMortarShell;
            ammoSet81mm.ammoTypes.Add(ammoDefLink);
        }

        public static void MarkForReplacement(ThingDef def, AmmoDef newAmmo)
        {
            CompProperties_ReplaceMe cp_rm = new CompProperties_ReplaceMe();
            cp_rm.thingToSpawn = newAmmo;
            def.comps.Add(cp_rm);
        }
    }
}
