using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace nuff.AutoPatcherCombatExtended
{
    public abstract class DefDataHolder : IExposable
    {
        internal bool isCustomized;
        internal string defName;
        public bool IsCustomized => isCustomized;

        public virtual void ExposeData()
        {
            if (Scribe.mode == LoadSaveMode.LoadingVars
                && !defName.NullOrEmpty())
            {
                APCESettings.defDataDict.Add(defName, this);
            }
        }

        public abstract void Reset();
    }
}
