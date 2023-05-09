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
    public partial class APCEController
    {
        internal static void PatchPawnKind(PawnKindDef def, APCEPatchLogger log)
        {
            if (def.race != null)
            {
                if (APCESettings.patchBackpacks)
                {
                    if ((def.race.race.intelligence == Intelligence.Humanlike)
                        && (def.weaponTags != null) && (def.weaponTags.Count != 0))
                    {
                        if (def.apparelTags == null)
                        {
                            def.apparelTags = new List<string>();
                        }
                        def.apparelTags.Add("IndustrialBasic");
                        def.apparelTags.Add("IndustrialMilitaryBasic");
                    }
                }

                if (def.modExtensions == null)
                {
                    def.modExtensions = new List<DefModExtension>();
                }
                LoadoutPropertiesExtension loadout = new LoadoutPropertiesExtension();
                loadout.primaryMagazineCount = new FloatRange(APCESettings.pawnKindMinMags, APCESettings.pawnKindMaxMags);

                def.modExtensions.Add(loadout);

                /*
                if (def.race.comps != null)
                {
                    if (def.race.comps.Any(cp => cp is CompProperties_Inventory))
                    {
                        if (def.modExtensions == null)
                        {
                            def.modExtensions = new List<DefModExtension>();
                        }

                        LoadoutPropertiesExtension loadout = new LoadoutPropertiesExtension();
                        loadout.primaryMagazineCount = new FloatRange(APCESettings.pawnKindMinMags, APCESettings.pawnKindMaxMags);

                        def.modExtensions.Add(loadout);
                    }
                }
                */
            }
        }
    }
}