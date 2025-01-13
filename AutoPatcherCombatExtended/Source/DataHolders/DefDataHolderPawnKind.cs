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

        PawnKindDef kind;

        List<string> original_ApparelTags = new List<string>();
        List<string> original_WeaponTags = new List<string>();

        List<string> modified_ApparelTags = new List<string>();
        List<string> modified_WeaponTags = new List<string>();
        float modified_MinMags;
        float modified_MaxMags;

        public override void GetOriginalData()
        {
            kind = def as PawnKindDef;

            if (kind.apparelTags != null)
            {
                foreach (string str in kind.apparelTags)
                {
                    original_ApparelTags.Add(str);
                }
            }
            if (kind.weaponTags != null)
            {
                foreach (string str in kind.weaponTags)
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
                if (kind.apparelTags == null)
                    kind.apparelTags = new List<string>();
                kind.apparelTags.Clear();
                foreach (string str in modified_ApparelTags)
                {
                    kind.apparelTags.Add(str);
                }
            }

            if (modified_WeaponTags.Count > 0)
            {
                if (kind.weaponTags == null)
                    kind.weaponTags = new List<string>();
                kind.weaponTags.Clear();
                foreach (string str in modified_WeaponTags)
                {
                    kind.weaponTags.Add(str);
                }
            }

            if (kind.modExtensions == null)
            {
                kind.modExtensions = new List<DefModExtension>();
            }
            LoadoutPropertiesExtension loadout = new LoadoutPropertiesExtension();
            loadout.primaryMagazineCount = new FloatRange(modified_MinMags, modified_MaxMags);

            DataHolderUtils.AddOrReplaceExtension(kind, loadout);
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
            Scribe_Collections.Look(ref modified_ApparelTags, "modified_ApparelTags");
            Scribe_Collections.Look(ref modified_WeaponTags, "modified_WeaponTags");

            Scribe_Values.Look(ref modified_MinMags, "modified_MinMags");
            Scribe_Values.Look(ref modified_MaxMags, "modified_MaxMags");
        }
    }
}
