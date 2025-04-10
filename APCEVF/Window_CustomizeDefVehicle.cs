using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Vehicles;
using UnityEngine;

namespace nuff.AutoPatcherCombatExtended.VF
{
    class Window_CustomizeDefVehicle : Window_CustomizeDef
    {
        DefDataHolderVehicleDef dataHolder;

        private Vector2 scrollPosition = Vector2.zero;

        public Window_CustomizeDefVehicle(DefDataHolder defDataHolder) : base(defDataHolder)
        {
        }

        public override void CastDataHolder(DefDataHolder defDataHolder)
        {
            this.dataHolder = defDataHolder as DefDataHolderVehicleDef;
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

            string modified_ArmorRatingSharpBuffer = dataHolder.modified_ArmorRatingSharp.ToString();
            list.TextFieldNumericLabeled("Base Sharp Armor Rating", ref dataHolder.modified_ArmorRatingSharp, ref modified_ArmorRatingSharpBuffer);

            string modified_ArmorRatingBluntBuffer = dataHolder.modified_ArmorRatingBlunt.ToString();
            list.TextFieldNumericLabeled("Base Blunt Armor Rating", ref dataHolder.modified_ArmorRatingBlunt, ref modified_ArmorRatingBluntBuffer);

            string modified_CargoCapacityBuffer = dataHolder.modified_CargoCapacity.ToString();
            list.TextFieldNumericLabeled("Cargo Capacity", ref dataHolder.modified_CargoCapacity, ref modified_CargoCapacityBuffer);

            list.End();

            Rect outerRect = new Rect(inRect.x, inRect.y + 110f, inRect.width, inRect.height - 145f);
            Rect innerRect = outerRect.ContractedBy(10f);

            float scrollHeight = 9999f; //TODO make dynamic
            Rect viewRect = new Rect(0, 0, innerRect.width - 16f, scrollHeight);

            // Begin measuring scroll height
            Widgets.BeginScrollView(innerRect, ref scrollPosition, viewRect);
            list.Begin(viewRect);

            for (int i = 0; i < dataHolder.modified_ComponentHealths.Count; i++)
            {
                float boxHeight = 160f;
                float spacing = 10f;

                Rect boxRect = new Rect(inRect.x, list.CurHeight, inRect.width - 30f, boxHeight);

                GUI.BeginGroup(boxRect, GUI.skin.box);

                Listing_Standard componentList = new Listing_Standard();
                componentList.Begin(new Rect(10f, 10f, boxRect.width - 20f, boxRect.height - 20f));

                componentList.Label(dataHolder.vehicleDef.components[i].label);

                string modified_ComponentArmorSharpsBuffer = dataHolder.modified_ComponentArmorSharps[i].ToString();
                float compArmorSharp = dataHolder.modified_ComponentArmorSharps[i];
                componentList.TextFieldNumericLabeled($"Component {i + 1} sharp armor: ", ref compArmorSharp, ref modified_ComponentArmorSharpsBuffer);
                dataHolder.modified_ComponentArmorSharps[i] = compArmorSharp;

                string modified_ComponentArmorBluntsBuffer = dataHolder.modified_ComponentArmorBlunts[i].ToString();
                float compArmorBlunt = dataHolder.modified_ComponentArmorBlunts[i];
                componentList.TextFieldNumericLabeled($"Component {i + 1} blunt armor: ", ref compArmorBlunt, ref modified_ComponentArmorBluntsBuffer);
                dataHolder.modified_ComponentArmorBlunts[i] = compArmorBlunt;

                string modified_ComponentHealthsBuffer = dataHolder.modified_ComponentHealths[i].ToString();
                int compHealth = dataHolder.modified_ComponentHealths[i];
                componentList.TextFieldNumericLabeled($"Component {i + 1} health: ", ref compArmorBlunt, ref modified_ComponentHealthsBuffer);
                dataHolder.modified_ComponentHealths[i] = compHealth;

                componentList.End();
                GUI.EndGroup();

                list.Gap(boxHeight + spacing);
            }

            list.End();

            scrollHeight = list.CurHeight + 20f;
            viewRect.height = scrollHeight;

            Widgets.EndScrollView();
        }
    }
}
