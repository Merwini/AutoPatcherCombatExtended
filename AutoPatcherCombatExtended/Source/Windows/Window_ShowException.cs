using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace nuff.AutoPatcherCombatExtended
{
    internal class Window_ShowException : Window
    {
        private readonly string exceptionText;
        private Vector2 scrollPosition;

        public override Vector2 InitialSize => new Vector2(900f, 650f);

        public Window_ShowException(Exception ex, string errorSource)
        {
            exceptionText = ($"Exception during {errorSource}. \n:" + ex?.ToString() ?? "Unknown exception");

            doCloseButton = false;
            doCloseX = true;
            absorbInputAroundWindow = true;
            forcePause = true;
            closeOnClickedOutside = false;
        }

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Medium;
            Rect titleRect = new Rect(inRect.x, inRect.y, inRect.width, 35f);
            Widgets.Label(titleRect, "Error!");

            Text.Font = GameFont.Small;

            Rect descRect = new Rect(inRect.x, titleRect.yMax + 8f, inRect.width, 50f);
            Widgets.Label(descRect, "An error occurred. Please copy this error and report it on the Steam Workshop page.");

            float buttonHeight = 35f;
            float gap = 10f;

            Rect buttonRect = new Rect(inRect.x, inRect.yMax - buttonHeight, 180f, buttonHeight);
            if (Widgets.ButtonText(buttonRect, "Copy error"))
            {
                GUIUtility.systemCopyBuffer = exceptionText;
                Messages.Message("error copied to clipboard.", MessageTypeDefOf.PositiveEvent, false);
            }

            Rect closeRect = new Rect(buttonRect.xMax + gap, inRect.yMax - buttonHeight, 180f, buttonHeight);
            if (Widgets.ButtonText(closeRect, "Close"))
            {
                Close();
            }

            Rect scrollOuterRect = new Rect(
                inRect.x,
                descRect.yMax + gap,
                inRect.width,
                closeRect.y - descRect.yMax - gap * 2f
            );

            float viewWidth = scrollOuterRect.width - 16f;
            float viewHeight = Text.CalcHeight(exceptionText, viewWidth);
            Rect scrollViewRect = new Rect(0f, 0f, viewWidth, viewHeight);

            Widgets.BeginScrollView(scrollOuterRect, ref scrollPosition, scrollViewRect);
            Widgets.Label(scrollViewRect, exceptionText);
            Widgets.EndScrollView();
        }
    }
}
