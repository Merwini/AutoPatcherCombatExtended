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
    class DefDataHolderPawnKind : DefDataHolder
    {
        public DefDataHolderPawnKind()
        {
            //empty constructor for use by SaveLoad
        }

        public DefDataHolderPawnKind(PawnKindDef def) : base(def)
        {
        }

        PawnKindDef kindDef;

        List<string> original_ApparelTags = new List<string>();
        List<string> original_WeaponTags = new List<string>();

        List<string> modified_ApparelTags = new List<string>();
        List<string> modified_WeaponTags = new List<string>();
        float modified_MinMags;
        float modified_MaxMags;

        public override void GetOriginalData()
        {
            //constructed by APCEController, def assigned by constructor
            if (def != null && kindDef == null)
            {
                this.kindDef = def as PawnKindDef;
            }
            //constructed by SaveLoad, thingDef loaded from xml
            else if (kindDef != null && def == null)
            {
                def = kindDef;
            }

            if (kindDef.apparelTags != null)
            {
                foreach (string str in kindDef.apparelTags)
                {
                    original_ApparelTags.Add(str);
                }
            }
            if (kindDef.weaponTags != null)
            {
                foreach (string str in kindDef.weaponTags)
                {
                    original_WeaponTags.Add(str);
                }
            }
        }

        public override void AutoCalculate()
        {
            if (!original_ApparelTags.NullOrEmpty())
            {
                foreach (string str in original_ApparelTags)
                {
                    modified_ApparelTags.Add(str);
                }
            }

            if (!original_WeaponTags.NullOrEmpty())
            {
                foreach (string str in original_WeaponTags)
                {
                    modified_WeaponTags.Add(str);
                }

                //if the PawnKind can spawn with weapons, give it the ability to have a backpack.
                modified_ApparelTags.Add("IndustrialBasic");
                modified_ApparelTags.Add("IndustrialMilitaryBasic");
            }

            modified_MinMags = 2;
            modified_MaxMags = 5;
        }

        public override void Patch()
        {
            if (modified_ApparelTags.Count > 0)
            {
                if (kindDef.apparelTags == null)
                    kindDef.apparelTags = new List<string>();
                kindDef.apparelTags.Clear();
                foreach (string str in modified_ApparelTags)
                {
                    kindDef.apparelTags.Add(str);
                }
            }

            if (modified_WeaponTags.Count > 0)
            {
                if (kindDef.weaponTags == null)
                    kindDef.weaponTags = new List<string>();
                kindDef.weaponTags.Clear();
                foreach (string str in modified_WeaponTags)
                {
                    kindDef.weaponTags.Add(str);
                }
            }

            if (kindDef.modExtensions == null)
            {
                kindDef.modExtensions = new List<DefModExtension>();
            }
            LoadoutPropertiesExtension loadout = new LoadoutPropertiesExtension();
            loadout.primaryMagazineCount = new FloatRange(modified_MinMags, modified_MaxMags);

            DataHolderUtils.AddOrReplaceExtension(kindDef, loadout);
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
            Scribe_Defs.Look(ref kindDef, "def");
            Scribe_Collections.Look(ref modified_ApparelTags, "modified_ApparelTags");
            Scribe_Collections.Look(ref modified_WeaponTags, "modified_WeaponTags");

            Scribe_Values.Look(ref modified_MinMags, "modified_MinMags");
            Scribe_Values.Look(ref modified_MaxMags, "modified_MaxMags");

            base.ExposeData();
        }
    }
}
