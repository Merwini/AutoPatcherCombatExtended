using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace nuff.AutoPatcherCombatExtended
{
    class CompatibilityPatches
    {
        public CompatibilityPatches()
        {
        }

        internal void PatchMods()
        {
            PatchPBF();
            PatchMPBF();
        }

        internal void PatchPBF()
        {
            if (ModsConfig.IsActive("statistno1.personabond"))
            {
                ModContentPack personabond = null;

                foreach (ModContentPack mod in LoadedModManager.RunningModsListForReading)
                {
                    if (mod.PackageId == "statistno1.personabond")
                    {
                        personabond = mod;
                        break;
                    }
                }

                foreach (ThingDef def in DefDatabase<ThingDef>.AllDefs)
                {
                    if (def.defName.StartsWith("PBF_"))
                    {
                        def.modContentPack = personabond;
                        personabond.AddDef(def);
                    }
                }
            }
        }
        internal void PatchMPBF()
        {
            if (ModsConfig.IsActive("daria40K.mightypersonabondforgepatch"))
            {
                ModContentPack mightypersonabond = null;

                foreach (ModContentPack mod in LoadedModManager.RunningModsListForReading)
                {
                    if (mod.PackageId == "daria40k.mightypersonabondforgepatch")
                    {
                        mightypersonabond = mod;
                        break;
                    }
                }

                foreach (ThingDef def in DefDatabase<ThingDef>.AllDefs)
                {
                    if (def.modContentPack == null
                        && def.defName.EndsWith("_Bond"))
                    {
                        def.modContentPack = mightypersonabond;
                        mightypersonabond.AddDef(def);
                    }
                }
            }
        }
    }
}
