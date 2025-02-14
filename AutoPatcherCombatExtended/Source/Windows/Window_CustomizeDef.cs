using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using UnityEngine;

namespace nuff.AutoPatcherCombatExtended
{
    public abstract class Window_CustomizeDef : Window
    {
        DefDataHolder dataHolder;

        //logic for all Def types in here, or separate classes and have the previous window select which window to open?
        public Window_CustomizeDef(DefDataHolder dataHolder)
        {
            this.dataHolder = dataHolder;
            CastDataHolder(dataHolder);
            dataHolder.isCustomized = true;
            doCloseButton = true;
        }

        public abstract void CastDataHolder(DefDataHolder defDataHolder);

        public override Vector2 InitialSize
        {
            get
            {
                return new Vector2(900, 700);
            }
        }

        public override void DoWindowContents(Rect inRect)
        {
            if (Widgets.ButtonText(rect: inRect.BottomPart(0.15f).BottomPart(0.5f).RightPart(0.3f).RightPart(0.5f), "Reset and close"))
            {
                dataHolder.isCustomized = false;
                dataHolder.AutoCalculate(); //recalc here even though it will recalc when settings write, so that if user reopens window it will already be recalc'd
                Close();
            }
        }

        public override void PreClose()
        {
            base.PreClose();
            dataHolder.PrePatch();
            dataHolder.Patch();
            dataHolder.PostPatch();
        }

        public void DiscardChanges()
        {
            //TODO
            //Set DefDataHolder.isCustomized back to false so it recalcs when settings are written
        }
    }
}