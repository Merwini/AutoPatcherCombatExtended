using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using UnityEngine;

namespace nuff.AutoPatcherCombatExtended
{
    class Window_SelectDamageDef : Window
    {
        string searchTerm = "";
        Vector2 leftScrollPosition = new Vector2();
        DamageDef selectedDef = null;

        List<DamageDef> defList;
        int index;
        bool isListMode = false;

        private DamageDef originalDef;

        public Window_SelectDamageDef(List<DamageDef> defList, int index)
        {
            this.defList = defList;
            this.index = index;
            this.selectedDef = defList[index];
            this.isListMode = true;
        }

        public Window_SelectDamageDef(ref DamageDef damageDef)
        {
            this.originalDef = damageDef;
        }

        public override void DoWindowContents(Rect inRect)
        {
            Listing_Standard list = new Listing_Standard();

            list.Begin(inRect);
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(0f, 0f, inRect.width - 17f, 35f), "Select DamageDef");
            Text.Font = GameFont.Small;
            list.End();
            list.Gap(45);  // Added gap to separate elements

            float searchBoxHeight = 25f;
            Rect searchBoxRect = new Rect(inRect.x + 10, 60, inRect.width - 20, searchBoxHeight);
            searchTerm = Widgets.TextField(searchBoxRect, searchTerm);

            // Adjusted list area to fill available space
            float listTop = searchBoxRect.yMax + 10; // Position list below search box
            float listBottomPadding = 50f; // Space reserved for buttons
            Rect listArea = new Rect(inRect.x + 10, listTop, inRect.width - 20, inRect.height - listTop - listBottomPadding);
            GUI.BeginGroup(listArea, new GUIStyle(GUI.skin.box));

            List<DamageDef> tempList = new List<DamageDef>();

            tempList = DefDatabase<DamageDef>.AllDefsListForReading
                .Where(item => item.defName.ToLower().Contains(searchTerm.ToLower()))
                .OrderBy(def => def.defName)
                .ToList();
            

            float num = 3f;
            Rect viewRect = new Rect(0f, 0f, listArea.width - 16f, tempList.Count * 32f);
            Widgets.BeginScrollView(listArea.AtZero(), ref leftScrollPosition, viewRect);

            if (!tempList.NullOrEmpty())
            {
                foreach (DamageDef def in tempList)
                {
                    Rect rowRect = new Rect(x: 5, y: num, width: listArea.width - 6, height: 30);
                    Widgets.DrawHighlightIfMouseover(rowRect);
                    if (def == selectedDef)
                    {
                        Widgets.DrawHighlightSelected(rowRect);
                    }

                    
                    Widgets.Label(rowRect, def.defName);

                    if (Widgets.ButtonInvisible(rowRect))
                    {
                        selectedDef = def;
                    }

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
                if (isListMode)
                {
                    defList[index] = selectedDef;
                }
                else
                {
                    originalDef = selectedDef;
                }
                Close();
            }

            if (Widgets.ButtonText(cancelButtonRect, "Cancel", true, false, Color.red))
            {
                Close();
            }
        }
    }
}
