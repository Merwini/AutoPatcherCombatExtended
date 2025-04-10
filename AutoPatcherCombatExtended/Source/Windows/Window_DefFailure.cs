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
        }

        public override void DoWindowContents(Rect inRect)
        {
            //Show a failure message
            throw new NotImplementedException();
        }
    }
}
