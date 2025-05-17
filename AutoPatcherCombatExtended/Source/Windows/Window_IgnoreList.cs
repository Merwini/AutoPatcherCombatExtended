using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace nuff.AutoPatcherCombatExtended
{
    class Window_IgnoreList : Window
    {
        private Vector2 leftScrollPosition = Vector2.zero;

        public Window_IgnoreList()
        {
            doCloseButton = true;
            draggable = true;
            closeOnClickedOutside = true;
        }

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(0f, 0f, inRect.width, 35f), "Ignored Mods");
            Text.Font = GameFont.Small;

            Rect listRect = new Rect(inRect.x + 10f, 45f, inRect.width - 20f, inRect.height - 90f);
            Rect viewRect = new Rect(0f, 0f, listRect.width - 16f, Math.Max(APCESettings.modIgnoreList.Count * 35f + 10f, listRect.height));

            GUI.BeginGroup(listRect, GUI.skin.box);
            Widgets.BeginScrollView(listRect.AtZero(), ref leftScrollPosition, viewRect);

            float y = 5f;
            List<string> removeList = new List<string>();

            if (!APCESettings.modIgnoreList.NullOrEmpty())
            {
                foreach (string mod in APCESettings.modIgnoreList)
                {
                    Rect rowRect = new Rect(0f, y, viewRect.width, 30f);
                    Rect labelRect = new Rect(rowRect.x + 5f, rowRect.y, rowRect.width - 90f, 30f);
                    Rect buttonRect = new Rect(rowRect.xMax - 80f, rowRect.y, 75f, 30f);

                    Widgets.Label(labelRect, mod);

                    if (Widgets.ButtonText(buttonRect, "Remove"))
                    {
                        removeList.Add(mod);
                    }

                    y += 35f;
                }
            }
            else
            {
                Widgets.Label(new Rect(5f, y, viewRect.width - 10f, 30f), "No mods currently being ignored.");
            }

            Widgets.EndScrollView();
            GUI.EndGroup();

            foreach (var mod in removeList)
            {
                APCESettings.modIgnoreList.Remove(mod);
            }
        }
    }
}
