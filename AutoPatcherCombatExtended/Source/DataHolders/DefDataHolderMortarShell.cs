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
        int original_damage;
        CompProperties_Explosive original_compPropsExplosive;
        string original_description;


        AmmoDef modified_shell;
        internal string modified_defName;
        internal int modified_stackLimit;

        ThingDef modified_projectile;
        internal string modified_projectileName;

        ProjectilePropertiesCE modified_ProjectilePropsCE;
        internal DamageDef modified_damageDef;
        internal int modified_damageAmount;
        internal float modified_explosionRadius;
        internal bool modified_ai_IsIncendiary;
        //internal SoundDef modified_soundExplode;
        internal bool modified_applyDamageToExplosionCellNeighbors;
        internal int modified_aimHeightOffset;
        //TODO shellingProps

        internal bool modified_fragmentsBool;
        internal List<ThingDef> modified_fragmentDefs = new List<ThingDef>();
        internal List<int> modified_fragmentsAmount = new List<int>();

        //TODO custom ammo category
        internal bool modified_CustomAmmoCat;
        internal AmmoCategoryDef modified_AmmoCategoryDef;
        internal string modified_AmmoCatDefName;
        internal string modified_AmmoCatLabel;
        internal string modified_AmmoCatLabelShort;
        internal string modified_AmmoCatDescription;

        internal AmmoSetDef modified_AmmoSet;
        internal AmmoSetDef previous_AmmoSet;

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

            original_description = thingDef.description.ToString();
            original_projectile = thingDef.projectileWhenLoaded;
            original_damage = original_projectile.projectile.damageAmountBase != -1 ? original_projectile.projectile.damageAmountBase : original_projectile.projectile.damageDef.defaultDamage;
            original_compPropsExplosive = thingDef.GetCompProperties<CompProperties_Explosive>();
        }

        public override void AutoCalculate()
        {
            modified_defName = "APCE_Shell_" + thingDef.defName;
            modified_stackLimit = 25;

            //todo maybe add fragments if damageDef is Bomb
            modified_fragmentsBool = false;

            modified_AmmoSet = APCEDefOf.AmmoSet_81mmMortarShell;

            CalculateMortarAmmoCategoryDef();
            CalculateMortarProjectileProps();
            CalculateMortarProjectileThing();
        }

        public override void Patch()
        {
            RebuildProjectileProps();
            RebuildProjectileCE();
            RebuildFragmentsComp();
            RebuildAmmo();
            AddAmmoLink();
            MarkForReplacement();
        }

        public override StringBuilder ExportXML()
        {
            //todo
            Log.Error("XML export for mortar shells not implemented yet");
            return null;
        }

        public override void ExposeData()
        {
            Scribe_Defs.Look(ref thingDef, "def");
            Scribe_Values.Look(ref modified_defName, "modified_defName");
            Scribe_Values.Look(ref modified_stackLimit, "modified_stackLimit");

            Scribe_Values.Look(ref modified_projectileName, "modified_projectileName");

            Scribe_Defs.Look(ref modified_damageDef, "modified_damageDef");
            Scribe_Values.Look(ref modified_damageAmount, "modified_damageAmount");
            Scribe_Values.Look(ref modified_explosionRadius, "modified_explosionRadius");
            Scribe_Values.Look(ref modified_ai_IsIncendiary, "modified_ai_IsIncendiary");
            Scribe_Values.Look(ref modified_applyDamageToExplosionCellNeighbors, "modified_applyDamageToExplosionCellNeighbors");
            Scribe_Values.Look(ref modified_aimHeightOffset, "modified_aimHeightOffset");

            Scribe_Values.Look(ref modified_fragmentsBool, "modified_fragmentsBool");
            Scribe_Collections.Look(ref modified_fragmentDefs, "modified_fragmentDefs", LookMode.Def);
            Scribe_Collections.Look(ref modified_fragmentsAmount, "modified_fragmentsAmount", LookMode.Value);

            Scribe_Defs.Look(ref modified_AmmoSet, "modified_AmmoSet");

            base.ExposeData();
        }

        public void RebuildProjectileProps()
        {
            modified_ProjectilePropsCE = new ProjectilePropertiesCE();
            DataHolderUtils.CopyFields(original_projectile.projectile, modified_ProjectilePropsCE);
            modified_ProjectilePropsCE.damageDef = modified_damageDef;
            modified_ProjectilePropsCE.damageAmountBase = modified_damageAmount;
            modified_ProjectilePropsCE.explosionRadius = modified_explosionRadius;
            modified_ProjectilePropsCE.ai_IsIncendiary = modified_ai_IsIncendiary;
            modified_ProjectilePropsCE.applyDamageToExplosionCellsNeighbors = modified_applyDamageToExplosionCellNeighbors;
            modified_ProjectilePropsCE.aimHeightOffset = modified_aimHeightOffset;
        }

        public void RebuildProjectileCE()
        {
            bool justBuilt = false;
            modified_projectile = DefDatabase<ThingDef>.GetNamedSilentFail(modified_projectileName);

            if (modified_projectile == null)
            {
                modified_projectile = new ThingDef()
                {
                    defName = modified_projectileName,
                    shortHash = 0
                };
                DataHolderUtils.CopyFields(original_projectile, modified_projectile, true);
                modified_projectile.comps = new List<CompProperties>(); //needs to be a separate list, to preserve the original
                foreach (CompProperties comp in original_projectile.comps)
                {
                    modified_projectile.comps.Add(comp);
                }
                justBuilt = true;
            }

            modified_projectile.projectile = modified_ProjectilePropsCE;

            if (modified_projectile.shortHash == 0)
            {
                InjectedDefHasher.GiveShortHashToDef(modified_projectile, typeof(ThingDef));
            }
            if (justBuilt)
            {
                DefGenerator.AddImpliedDef<ThingDef>(modified_projectile);
            }
        }

        public void RebuildFragmentsComp()
        {
            if (!modified_projectile.comps.NullOrEmpty())
            {
                modified_projectile.comps.RemoveAll(c => c is CompProperties_Fragments);
            }

            if (modified_fragmentsBool && !modified_fragmentDefs.NullOrEmpty() && modified_fragmentsAmount.NullOrEmpty())
            {
                CompProperties_Fragments newComp_Fragments = new CompProperties_Fragments();
                for (int i = 0; i < modified_fragmentDefs.Count; i++)
                {
                    ThingDefCountClass tdcc = new ThingDefCountClass()
                    {
                        thingDef = modified_fragmentDefs[i],
                        count = modified_fragmentsAmount[i] > 0 ? modified_fragmentsAmount[i] : 1
                    };

                    newComp_Fragments.fragments.Add(tdcc);
                }

                modified_projectile.comps.Add(newComp_Fragments);
            }
        }

        public void RebuildAmmo()
        {
            bool justBuilt = false;

            modified_shell = DefDatabase<AmmoDef>.GetNamedSilentFail(modified_defName);

            if (modified_shell == null)
            {
                modified_shell = new AmmoDef()
                {
                    defName = modified_defName,
                    shortHash = 0 //hash later
                };
                DataHolderUtils.CopyFields(thingDef, modified_shell, true);
                modified_shell.comps = new List<CompProperties>(); //needs to be a separate list, to preserve the original
                foreach (CompProperties comp in thingDef.comps)
                {
                    modified_shell.comps.Add(comp);
                }
                modified_shell.modContentPack = APCESettings.thisMod.Content;
                justBuilt = true;
            }

            modified_shell.stackLimit = modified_stackLimit;
            modified_shell.projectile = modified_ProjectilePropsCE;

            //todo let user choose to do this or customize a CompProperties_Explosive
            modified_shell.detonateProjectile = modified_projectile;
            modified_shell.comps.RemoveAll(comp => comp is CompProperties_Explosive);

            //TODO allow user to select or create their own AmmoCat
            modified_shell.ammoClass = APCEDefOf.GrenadeHE;

            if (modified_shell.shortHash == 0)
            {
                InjectedDefHasher.GiveShortHashToDef(modified_shell, typeof(AmmoDef));
            }
            if (justBuilt)
            {
                DefGenerator.AddImpliedDef<AmmoDef>(modified_shell);
            }
        }

        public void CalculateMortarAmmoCategoryDef()
        {
            modified_AmmoCatDefName = "APCE_AmmoCatDef_ " + thingDef.defName;
            modified_AmmoCatLabel = thingDef.label;
            modified_AmmoCatLabelShort = "Custom";
            modified_AmmoCatDescription = "Ammo category of " + thingDef.label;
        }

        public void CalculateMortarProjectileProps()
        {
            modified_damageDef = original_projectile.projectile.damageDef;
            modified_damageAmount = original_damage;
            modified_explosionRadius = original_projectile.projectile.explosionRadius;
            modified_ai_IsIncendiary = original_projectile.projectile.ai_IsIncendiary;
            modified_applyDamageToExplosionCellNeighbors = original_projectile.projectile.applyDamageToExplosionCellsNeighbors;
            modified_aimHeightOffset = 0;
        }

        public void CalculateMortarProjectileThing()
        {
            modified_projectileName = "APCE_Bullet_Shell_" + original_projectile.defName;
            //todo I think this is it? everything else is either the same as original or calculated in projectile props
        }

        public void AddAmmoLink()
        {
            //check if link already exists for selected AmmoSet, if it does just update the projectile
            int linkIndex = modified_AmmoSet.ammoTypes.FindIndex(link => link.ammo == modified_shell);
            if (linkIndex != -1)
            {
                modified_AmmoSet.ammoTypes[linkIndex].projectile = modified_projectile;
            }
            else
            {
                modified_AmmoSet.ammoTypes.Add(new AmmoLink(modified_shell, modified_projectile));
            }

            //clean up previous AmmoSet, in case it has been reassigned
            if (previous_AmmoSet != null && previous_AmmoSet != modified_AmmoSet)
            {
                previous_AmmoSet.ammoTypes.RemoveAll(link => link.ammo == modified_shell);
            }
            previous_AmmoSet = modified_AmmoSet;
        }

        public void MarkForReplacement()
        {
            CompProperties_ReplaceMe cp_rm = new CompProperties_ReplaceMe()
            {
                thingToSpawn = modified_shell
            };
            thingDef.comps.RemoveAll(comp => comp is CompProperties_ReplaceMe);
            thingDef.comps.Add(cp_rm);
            thingDef.description = original_description + "\n\n This mortar shell should be converted to a CE-compatible one as soon as soon as it touches the ground.";
        }

        public void AddNewFragment()
        {
            modified_fragmentDefs.Add(APCEDefOf.Fragment_Small);
            modified_fragmentsAmount.Add(1);
        }

        public void RemoveFragment(int i)
        {
            modified_fragmentDefs.RemoveAt(i);
            modified_fragmentsAmount.RemoveAt(i);
        }

    }
}
