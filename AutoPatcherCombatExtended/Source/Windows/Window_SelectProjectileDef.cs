using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CombatExtended;
using UnityEngine;
using Verse;

namespace nuff.AutoPatcherCombatExtended
{
    class Window_SelectProjectileDef : Window
    {
        string searchTerm = "";
        Vector2 leftScrollPosition = new Vector2();
        ThingDef selectedDef = null;

        List<ThingDef> projList;
        int index;
        bool isListMode = false;

        private ThingDef originalDef;
        private Action<ThingDef> onAccept;

        public Window_SelectProjectileDef(List<ThingDef> projList, int index)
        {
            this.projList = projList;
            this.index = index;
            this.selectedDef = projList[index];
            isListMode = true;
        }

        public Window_SelectProjectileDef(ref ThingDef thingDef, Action<ThingDef> onAccept)
        {
            this.originalDef = thingDef;
            this.onAccept = onAccept;
            selectedDef = originalDef;
        }

        public override void DoWindowContents(Rect inRect)
        {
            Listing_Standard list = new Listing_Standard();

            list.Begin(inRect);
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(0f, 0f, inRect.width - 17f, 35f), "Select Projectile");
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

            List<ThingDef> tempList = DefDatabase<ThingDef>.AllDefsListForReading
                .Where(item => item.projectile != null && item.projectile is ProjectilePropertiesCE).ToList();

            List<ThingDef> tempList2 = new List<ThingDef>();

            tempList2 = tempList
                .Where(item => item.defName.ToLower().Contains(searchTerm.ToLower()))
                .OrderBy(def => def.defName)
                .ToList();


            float num = 3f;
            Rect viewRect = new Rect(0f, 0f, listArea.width - 16f, tempList2.Count * 32f);
            Widgets.BeginScrollView(listArea.AtZero(), ref leftScrollPosition, viewRect);

            if (!tempList2.NullOrEmpty())
            {
                foreach (ThingDef def in tempList2)
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
                    projList[index] = selectedDef;
                }
                else
                {
                    onAccept?.Invoke(selectedDef);
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
