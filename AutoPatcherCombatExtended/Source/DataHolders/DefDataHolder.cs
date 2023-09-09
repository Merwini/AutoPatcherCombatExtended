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
        internal string parentModPackageId;
        public bool IsCustomized => isCustomized;

        public virtual void ExposeData()
        {
            if (Scribe.mode == LoadSaveMode.LoadingVars
                || (Scribe.mode == LoadSaveMode.Saving && isCustomized == true))
            {
                Scribe_Values.Look(ref defName, "defName");
                Scribe_Values.Look(ref parentModPackageId, "parentModPackageId");
                Scribe_Values.Look(ref isCustomized, "isCustomized");
            }

                if (Scribe.mode == LoadSaveMode.LoadingVars
                && !defName.NullOrEmpty())
            {
                APCESettings.defDataDict.Add(defName, this);
            }
        }

        public abstract void Reset();
    }
}
