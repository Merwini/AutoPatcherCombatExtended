using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using CombatExtended;
using UnityEngine;
using Verse;

namespace nuff.AutoPatcherCombatExtended
{
    class Window_ShowPatchInfo : Window
    {
        string folderPath;
        string patchLogString;
        string modName;

        private Vector2 scrollPosition = Vector2.zero;

        public Window_ShowPatchInfo(StringBuilder patchLog, string folderPath, string modName)
        {
            patchLogString = patchLog.ToString();
            this.folderPath = folderPath;
            this.modName = modName;
            doCloseButton = true;
            draggable = true;
            absorbInputAroundWindow = true;
        }

        public override Vector2 InitialSize
        {
            get
            {
                return new Vector2(900, 700);
            }
        }

        public override void DoWindowContents(Rect inRect)
        {
            float headerHeight = 35f;
            float spacing = 10f;

            // Draw mod name header
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(inRect.x, inRect.y, inRect.width, headerHeight), $"Patch Log for {modName}");
            Text.Font = GameFont.Small;

            // Scrollable area for patch content
            Rect outerRect = new Rect(inRect.x, inRect.y + headerHeight + spacing, inRect.width, inRect.height - headerHeight - spacing - 50f);

            float textHeight = Text.CalcHeight(patchLogString, outerRect.width - 20f);
            Rect viewRect = new Rect(0f, 0f, outerRect.width - 16f, textHeight + 20f);

            Widgets.BeginScrollView(outerRect, ref scrollPosition, viewRect);
            Widgets.Label(viewRect, patchLogString);
            Widgets.EndScrollView();

            // Button to open patch location
            if (Widgets.ButtonText(
                rect: inRect.BottomPart(0.15f).BottomPart(0.5f).LeftPart(0.3f).LeftPart(0.5f),
                "Open Patch Location"))
            {
                try
                {
                    Process.Start($@"{folderPath}");
                }
                catch
                {
                    Log.Error("Failed to open folder");
                }
            }
        }
    }
}
