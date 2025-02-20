using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using UnityEngine;
using CombatExtended;

namespace nuff.AutoPatcherCombatExtended
{
    class Window_SelectAmmoSet : Window
    {
        DefDataHolderRangedWeapon dataHolder;
        APCEConstants.DefNameOrLabel selector = APCEConstants.DefNameOrLabel.label;

        string searchTerm = "";
        private Vector2 leftScrollPosition = new Vector2();
        private AmmoSetDef selectedDef = null;

        public Window_SelectAmmoSet(DefDataHolderRangedWeapon dataHolder)
        {
            this.dataHolder = dataHolder;
        }

        public override void DoWindowContents(Rect inRect)
        {
            Listing_Standard list = new Listing_Standard();

            // Begin main listing (Header)
            list.Begin(inRect);
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(0f, 0f, inRect.width - 17f, 35f), $"Select AmmoSet for {dataHolder.def.label}");
            Text.Font = GameFont.Small;
            list.End();
            list.Gap(45);  // Added gap to separate elements

            // Enum Selector for sorting method
            float enumHeight = 25f;
            float enumTop = 50f;
            Rect enumRect = new Rect(inRect.x + 10, inRect.y + 45, inRect.width - 20, enumHeight);
            list.EnumSelector(ref selector, "", "", "List AmmoSets by defName or label");

            // Search box positioned below the Enum Selector
            float searchBoxHeight = 25f;
            Rect searchBoxRect = new Rect(inRect.x + 10, enumRect.yMax + 10, inRect.width - 20, searchBoxHeight);
            searchTerm = Widgets.TextField(searchBoxRect, searchTerm);

            // Adjusted list area to fill available space
            float listTop = searchBoxRect.yMax + 10; // Position list below search box
            float listBottomPadding = 50f; // Space reserved for buttons
            Rect listArea = new Rect(inRect.x + 10, listTop, inRect.width - 20, inRect.height - listTop - listBottomPadding);
            GUI.BeginGroup(listArea, new GUIStyle(GUI.skin.box));

            List<AmmoSetDef> tempList = new List<AmmoSetDef>();

            if (selector == APCEConstants.DefNameOrLabel.defName)
            {
                tempList = DefDatabase<AmmoSetDef>.AllDefsListForReading
                    .Where(item => item.defName.ToLower().Contains(searchTerm.ToLower()))
                    .OrderBy(def => def.defName)
                    .ToList();
            }
            else if (selector == APCEConstants.DefNameOrLabel.label)
            {
                tempList = DefDatabase<AmmoSetDef>.AllDefsListForReading
                    .Where(item => item.label.ToLower().Contains(searchTerm.ToLower()))
                    .OrderBy(def => def.label)
                    .ToList();
            }

            float num = 3f;
            Rect viewRect = new Rect(0f, 0f, listArea.width - 16f, tempList.Count * 32f);
            Widgets.BeginScrollView(listArea.AtZero(), ref leftScrollPosition, viewRect);

            if (!tempList.NullOrEmpty())
            {
                foreach (AmmoSetDef def in tempList)
                {
                    Rect rowRect = new Rect(x: 5, y: num, width: listArea.width - 6, height: 30);
                    Widgets.DrawHighlightIfMouseover(rowRect);
                    if (def == selectedDef)
                        Widgets.DrawHighlightSelected(rowRect);
                    if (selector == APCEConstants.DefNameOrLabel.defName)
                    {
                        Widgets.Label(rowRect, def.defName);
                    }
                    else if (selector == APCEConstants.DefNameOrLabel.label)
                    {
                        Widgets.Label(rowRect, def.label);
                    }
                    if (Widgets.ButtonInvisible(rowRect))
                        selectedDef = def;

                    num += 32f;
                }
            }

            Widgets.EndScrollView();
            GUI.EndGroup();

            float buttonWidth = (inRect.width - 30) / 2;
            Rect acceptButtonRect = new Rect(inRect.x + 10, inRect.yMax - 40, buttonWidth, 30);
            Rect cancelButtonRect = new Rect(inRect.x + 20 + buttonWidth, inRect.yMax - 40, buttonWidth, 30);

            if (Widgets.ButtonText(acceptButtonRect, "Accept", true, false, Color.green) && selectedDef != null)
            {
                dataHolder.modified_AmmoSetDef = selectedDef;
                Close();
            }

            if (Widgets.ButtonText(cancelButtonRect, "Cancel", true, false, Color.red))
            {
                Close();
            }
        }
    }
}
