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
    class Window_CustomizeMod : Window
    {
        ModContentPack mod;

        public Window_CustomizeMod(ModContentPack mod)
        {
            this.mod = mod;
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
            Listing_Standard list = new Listing_Standard();
            list.Begin(inRect);
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(0f, 0f, inRect.width - 150f - 17f, 35f), mod.Name);
            list.Gap(45);
            Text.Font = GameFont.Small;

            list.Label("test");

            list.End();
        }
    }
}
