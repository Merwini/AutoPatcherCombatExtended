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

            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(0f, 0f, inRect.width - 150f - 17f, 35f), $"{dataHolder.def.label} - {dataHolder.def.defName}");
            Text.Font = GameFont.Small;

            Rect outerRect = new Rect(inRect.x, inRect.y + 45f, inRect.width, inRect.height - 60f);

            float lineHeight = 30f;
            float estimatedLines = 20 + dataHolder.modified_Tools.Count * 20 + dataHolder.modified_ToolCapacityDefs.Count * 10;
            float viewHeight = estimatedLines * lineHeight * 2;

            Rect viewRect = new Rect(0f, 0f, outerRect.width - 16f, viewHeight);

            Widgets.BeginScrollView(outerRect, ref scrollPosition, viewRect);
            Listing_Standard list = new Listing_Standard();
            list.Begin(viewRect);
            list.Gap(45);

            string modified_stackLimitBuffer = dataHolder.modified_stackLimit.ToString();
            list.TextFieldNumericLabeled("Stack Limit", ref dataHolder.modified_stackLimit, ref modified_stackLimitBuffer);

            if (Widgets.ButtonText(list.GetRect(30f), dataHolder.modified_damageDef.defName))
            {
                Find.WindowStack.Add(new Window_SelectDamageDef(ref dataHolder.modified_damageDef));
            }

            string modified_damageAmountBuffer = dataHolder.modified_damageAmount.ToString();
            list.TextFieldNumericLabeled("Damage", ref dataHolder.modified_damageAmount, ref modified_damageAmountBuffer);

            string modified_explosionRadiusBuffer = dataHolder.modified_explosionRadius.ToString();
            list.TextFieldNumericLabeled("Explosion Radius", ref dataHolder.modified_explosionRadius, ref modified_explosionRadiusBuffer);

            list.CheckboxLabeled("Explosion creates fragments", ref dataHolder.modified_fragmentsBool);

            list.Label("TODO ability select what ammo set to add the mortar shell to (currently just uses 81mm)");

            if (dataHolder.modified_fragmentsBool)
            {
                for (int i = 0; i < dataHolder.modified_fragmentDefs.Count; i++)
                {
                    list.Label("Fragment to spawn:");
                    if (Widgets.ButtonText(list.GetRect(30f), dataHolder.modified_fragmentDefs[i].defName ?? "null"))
                    {
                        Find.WindowStack.Add(new Window_SelectFragmentDef(dataHolder.modified_fragmentDefs, i));
                    }

                    string modified_fragmentsAmountBuffer = dataHolder.modified_fragmentsAmount[i].ToString();
                    int fragAmount = dataHolder.modified_fragmentsAmount[i];
                    list.TextFieldNumericLabeled("Fragment amount", ref fragAmount, ref modified_fragmentsAmountBuffer);
                    dataHolder.modified_fragmentsAmount[i] = fragAmount;

                    list.Gap(10f);

                    if (Widgets.ButtonText(list.GetRect(30f), "REMOVE THIS FRAGMENT"))
                    {
                        dataHolder.RemoveFragment(i);
                    }

                    list.Gap(40f);
                }

                if (Widgets.ButtonText(list.GetRect(30f), "ADD ANOTHER FRAGMENT"))
                {
                    dataHolder.AddNewFragment();
                }
            }

            list.End();
            Widgets.EndScrollView();
        }
    }
}