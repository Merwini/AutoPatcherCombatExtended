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
    class Window_ConfirmDeleteTool : Window
    {
        DefDataHolder dataHolder;
        int i;

        bool lastTool = false;

        public Window_ConfirmDeleteTool(DefDataHolder dataHolder, int i)
        {
            this.dataHolder = dataHolder;
            this.i = i;
            lastTool = (dataHolder.modified_ToolPowers.Count == 1);
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
            if (!lastTool)
            {
                list.Label("Are you sure you want to delete this tool?");
            }
            else
            {
                list.Label("Removing the last tool will cause errors");
            }
            Text.Font = GameFont.Small;
            list.End();
            list.Gap(45);

            float buttonWidth = (inRect.width - 30) / 2;
            Rect acceptButtonRect = new Rect(inRect.x + 10, inRect.yMax - 40, buttonWidth, 30);
            Rect cancelButtonRect = new Rect(inRect.x + 20 + buttonWidth, inRect.yMax - 40, buttonWidth, 30);

            if (!lastTool && Widgets.ButtonText(acceptButtonRect, "Delete", true, false, Color.green) && dataHolder != null)
            {
                dataHolder.RemoveTool(i);
                Close();
            }

            if (Widgets.ButtonText(cancelButtonRect, "Cancel", true, false, Color.red))
            {
                Close();
            }
        }
    }
}
