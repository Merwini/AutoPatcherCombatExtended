using CombatExtended;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using Verse;
using UnityEngine;

namespace nuff.AutoPatcherCombatExtended
{
    class Window_CustomizeMortarShell : Window_CustomizeDef
    {
        DefDataHolderMortarShell dataHolder;

        private Vector2 scrollPosition = Vector2.zero;

        public Window_CustomizeMortarShell(DefDataHolder defDataHolder) : base(defDataHolder)
        {
        }

        public override void CastDataHolder(DefDataHolder defDataHolder)
        {
            this.dataHolder = defDataHolder as DefDataHolderMortarShell;
        }

        public override void DoWindowContents(Rect inRect)
        {
            base.DoWindowContents(inRect);

            Listing_Standard list = new Listing_Standard();
            list.Begin(inRect);
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(0f, 0f, inRect.width - 150f - 17f, 35f), $"{dataHolder.def.label} - {dataHolder.def.defName}");
            Text.Font = GameFont.Small;
            list.Gap(45);

            string modified_stackLimitBuffer = dataHolder.modified_stackLimit.ToString();
            list.TextFieldNumericLabeled("Stack Limit", ref dataHolder.modified_stackLimit, ref modified_stackLimitBuffer);

            if (Widgets.ButtonText(new Rect(inRect.width / 2 - 100, list.curY, 200f, 30f), dataHolder.modified_damageDef.defName))
            {
                Find.WindowStack.Add(new Window_SelectDamageDef(ref dataHolder.modified_damageDef));
            }

            string modified_damageAmountBuffer = dataHolder.modified_damageAmount.ToString();
            list.TextFieldNumericLabeled("Damage", ref dataHolder.modified_damageAmount, ref modified_damageAmountBuffer);

            string modified_explosionRadiusBuffer = dataHolder.modified_explosionRadius.ToString();
            list.TextFieldNumericLabeled("Explosion Radius", ref dataHolder.modified_explosionRadius, ref modified_explosionRadiusBuffer);

            //TODO fragments list and quantity

            list.End();
        }
    }
}