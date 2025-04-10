using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using System.Reflection;

namespace nuff.AutoPatcherCombatExtended
{
    class CompatibilityPatches
    {
        public CompatibilityPatches(ModContentPack content)
        {
            ConstructCompat(content);
        }

        //this is a stripped down version of CE's Controller.PostLoad using IModPart
        public void ConstructCompat(ModContentPack content)
        {
            Queue<Assembly> toProcess = new Queue<Assembly>(content.assemblies.loadedAssemblies);
            List<ICompat> modParts = new List<ICompat>();
            while (toProcess.Any())
            {
                Assembly assembly = toProcess.Dequeue();

                foreach (Type t in assembly.GetTypes().Where(x => typeof(ICompat).IsAssignableFrom(x) && !x.IsAbstract))
                {
                    ICompat imp = ((ICompat)t.GetConstructor(new Type[] { }).Invoke(new object[] { }));
                    modParts.Add(imp);
                }
            }
        }

        public void PatchMods()
        {
            PatchPBF();
            PatchMPBF();
        }

        public void PatchPBF()
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
        public void PatchMPBF()
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
