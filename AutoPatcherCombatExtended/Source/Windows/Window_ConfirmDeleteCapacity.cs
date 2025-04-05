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
    class Window_ConfirmDeleteCapacity : Window
    {

        DefDataHolder dataHolder;
        int i;
        int j;

        bool lastCapacity = false;

        public Window_ConfirmDeleteCapacity(DefDataHolder dataHolder, int i, int j)
        {
            this.dataHolder = dataHolder;
            this.i = i;
            this.j = j;
            lastCapacity = (dataHolder.modified_ToolCapacityDefs[i].Count == 1);
        }

        public override Vector2 InitialSize
        {
            get
            {
                return new Vector2(400, 200);
            }
        }

        public override void DoWindowContents(Rect inRect)
        {
            Listing_Standard list = new Listing_Standard();

            list.Begin(inRect);
            Text.Font = GameFont.Medium;
            if (!lastCapacity)
            {
                list.Label($"Are you sure you want to delete tool capacity: {dataHolder.modified_ToolCapacityDefs[i][j].defName} ?");
            }
            else
            {
                list.Label("Removing the last tool capacity def will cause errors.");
            }
            Text.Font = GameFont.Small;
            list.End();
            list.Gap(45);

            float buttonWidth = (inRect.width - 30) / 2;
            Rect acceptButtonRect = new Rect(inRect.x + 10, inRect.yMax - 40, buttonWidth, 30);
            Rect cancelButtonRect = new Rect(inRect.x + 20 + buttonWidth, inRect.yMax - 40, buttonWidth, 30);

            if (!lastCapacity && Widgets.ButtonText(acceptButtonRect, "Delete", true, false, Color.green) && dataHolder != null)
            {
                dataHolder.RemoveCapacity(i, j);
                Close();
            }

            if (Widgets.ButtonText(cancelButtonRect, "Cancel", true, false, Color.red))
            {
                Close();
            }

        }
    }
}
