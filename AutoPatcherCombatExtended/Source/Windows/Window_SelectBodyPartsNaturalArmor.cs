using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using UnityEngine;

namespace nuff.AutoPatcherCombatExtended
{
    class Window_SelectBodyPartsNaturalArmor : Window
    {
        public override void DoWindowContents(Rect inRect)
        {
            //TODO
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(0f, 0f, inRect.width - 150f - 17f, 35f), $"Not implmented yet. Use escape key to close");
        }
    }
}
