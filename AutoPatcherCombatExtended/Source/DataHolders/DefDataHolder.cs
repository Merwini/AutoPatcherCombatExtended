using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace nuff.AutoPatcherCombatExtended
{
    public abstract class DefDataHolder : IExposable
    {
        internal bool isCustomized = false; //this will be changed by the customization window if the user changes any values
        internal string defName;
        internal string parentModPackageId;

        internal ModDataHolder modData;

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

        //will get relevant values from the def and fill the original_ fields
        public abstract void GetOriginalData();
        
        //will use modData and original_ fields to autocalculate modified_ fields
        public abstract void AutoCalculate();

        //will use the modified_ fields to edit the def
        public abstract void Patch();

        //will use the modified_ fields to generate an xml patch for the def. Need to change the return type so it can be used for exporting single patches or as part of patching the whole mod
        public abstract StringBuilder PrepExport();

        //will call PrepExport and allow the user to save the resulting xml patch for just the current def
        public abstract void Export();
    }
}
