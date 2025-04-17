using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;

namespace nuff.AutoPatcherCombatExtended
{
    public class Window_DefFailure : Window
    {
        Def def;

        public Window_DefFailure(Def def)
        {
            this.def = def;
            doCloseButton = true;
        }

        public override void DoWindowContents(Rect inRect)
        {
            Listing_Standard list = new Listing_Standard();
            list.Begin(inRect);
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(0f, 0f, inRect.width - 150f - 17f, 35f), $"Not sure how toc ustomize {def.defName}");
            Text.Font = GameFont.Small;
            list.End();
        }
    }
}
