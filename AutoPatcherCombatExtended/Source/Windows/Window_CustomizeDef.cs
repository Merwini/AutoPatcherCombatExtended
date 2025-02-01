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
    public class Window_CustomizeDef : Window
    {
        //logic for all Def types in here, or separate classes and have the previous window select which window to open?
        public Window_CustomizeDef(Def def)
        {

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
            throw new NotImplementedException();
        }
    }
}