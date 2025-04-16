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
    class Window_SelectLinkedBodyPartGroupDef : Window
    {
        string searchTerm = "";
        Vector2 leftScrollPosition = new Vector2();
        BodyPartGroupDef selectedDef = null;

        List<BodyPartGroupDef> defList;
        int index;

        public Window_SelectLinkedBodyPartGroupDef(List<BodyPartGroupDef> defList, int index)
        {
            this.defList = defList;
            this.index = index;
            this.selectedDef = defList[index];
        }

        public override void DoWindowContents(Rect inRect)
        {
            Listing_Standard list = new Listing_Standard();

            list.Begin(inRect);
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(0f, 0f, inRect.width - 17f, 35f), "Select BodyPartGroupDef");
            Text.Font = GameFont.Small;
            list.End();
            list.Gap(45);

            float searchBoxHeight = 25f;
            Rect searchBoxRect = new Rect(inRect.x + 10, 60, inRect.width - 20, searchBoxHeight);
            searchTerm = Widgets.TextField(searchBoxRect, searchTerm);

            float listTop = searchBoxRect.yMax + 10;
            float listBottomPadding = 50f;
            Rect listArea = new Rect(inRect.x + 10, listTop, inRect.width - 20, inRect.height - listTop - listBottomPadding);
            GUI.BeginGroup(listArea, new GUIStyle(GUI.skin.box));

            List<BodyPartGroupDef> tempList = new List<BodyPartGroupDef>();

            tempList = DefDatabase<BodyPartGroupDef>.AllDefsListForReading
                .Where(item => item.defName.ToLower().Contains(searchTerm.ToLower()))
                .OrderBy(def => def.defName)
                .ToList();


            float num = 3f;
            Rect viewRect = new Rect(0f, 0f, listArea.width - 16f, tempList.Count * 32f);
            Widgets.BeginScrollView(listArea.AtZero(), ref leftScrollPosition, viewRect);

            if (!tempList.NullOrEmpty())
            {
                foreach (BodyPartGroupDef def in tempList)
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

            float buttonWidth = (inRect.width - 30) / 3;
            Rect acceptButtonRect = new Rect(inRect.x + 10, inRect.yMax - 40, buttonWidth, 30);
            Rect cancelButtonRect = new Rect(inRect.x + 20 + buttonWidth, inRect.yMax - 40, buttonWidth, 30);
            Rect nullButtonRect = new Rect(inRect.x + 20 + buttonWidth + buttonWidth, inRect.yMax - 40, buttonWidth, 30);

            if (Widgets.ButtonText(acceptButtonRect, "Accept", true, false, Color.green) && selectedDef != null)
            {
                defList[index] = selectedDef;
                Close();
            }

            if (Widgets.ButtonText(cancelButtonRect, "Cancel", true, false, Color.red))
            {
                Close();
            }

            if (Widgets.ButtonText(cancelButtonRect, "Select Null", true, false, Color.blue))
            {
                defList[index] = null;
                Close();
            }
        }
    }
}
