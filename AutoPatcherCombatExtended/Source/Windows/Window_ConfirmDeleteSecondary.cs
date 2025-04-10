using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using UnityEngine;

namespace nuff.AutoPatcherCombatExtended
{
    class Window_ConfirmDeleteSecondary : Window
    {
        DefDataHolderAmmoSet dataHolder;
        int i;
        int j;

        public Window_ConfirmDeleteSecondary(DefDataHolderAmmoSet dataHolder, int i, int j)
        {
            this.dataHolder = dataHolder;
            this.i = i;
            this.j = j;
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
            list.Label("Are you sure you want to delete this secondary damage?");
            Text.Font = GameFont.Small;
            list.End();
            list.Gap(45);

            float buttonWidth = (inRect.width - 30) / 2;
            Rect acceptButtonRect = new Rect(inRect.x + 10, inRect.yMax - 40, buttonWidth, 30);
            Rect cancelButtonRect = new Rect(inRect.x + 20 + buttonWidth, inRect.yMax - 40, buttonWidth, 30);

            if (Widgets.ButtonText(acceptButtonRect, "Delete", true, false, Color.green) && dataHolder != null)
            {
                dataHolder.RemoveSecondaryDamage(i, j);
                Close();
            }

            if (Widgets.ButtonText(cancelButtonRect, "Cancel", true, false, Color.red))
            {
                Close();
            }

        }
    }
}
