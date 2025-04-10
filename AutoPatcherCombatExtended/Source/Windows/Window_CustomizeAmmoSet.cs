using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using UnityEngine;
using CombatExtended;

namespace nuff.AutoPatcherCombatExtended
{
    class Window_CustomizeAmmoSet : Window_CustomizeDef
    {
        DefDataHolderAmmoSet dataHolder;

        private Vector2 scrollPosition = Vector2.zero;

        public Window_CustomizeAmmoSet(DefDataHolder defDataHolder) : base(defDataHolder)
        {
        }

        public override void CastDataHolder(DefDataHolder defDataHolder)
        {
            this.dataHolder = defDataHolder as DefDataHolderAmmoSet;
        }

        public override void DoWindowContents(Rect inRect)
        {
            base.DoWindowContents(inRect);
            Listing_Standard list = new Listing_Standard();

            list.Begin(inRect);
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(0f, 0f, inRect.width - 150f - 17f, 35f), $"{dataHolder.def.label} - {dataHolder.def.defName}");
            Text.Font = GameFont.Small;
            list.End();
            list.Gap(45);

            Rect outerRect = new Rect(inRect.x, inRect.y + 45f, inRect.width, inRect.height - 145f);
            Rect innerRect = outerRect.ContractedBy(10f);
            float scrollHeight = 9999f; //TODO make dynamic
            Rect viewRect = new Rect(0, 0, innerRect.width - 16f, scrollHeight);

            // Begin measuring scroll height
            Widgets.BeginScrollView(innerRect, ref scrollPosition, viewRect);
            list.Begin(viewRect);

            for (int i = 0; i < dataHolder.GeneratedAmmoSetDef.ammoTypes.Count; i++)
            {
                float boxHeight = 500f;
                float spacing = 10f;

                Rect boxRect = new Rect(inRect.x, list.CurHeight, inRect.width - 30f, boxHeight + (dataHolder.modified_secondaryDamageDefs[i].Count * 150f));

                GUI.BeginGroup(boxRect, GUI.skin.box);

                Listing_Standard ammoList = new Listing_Standard();
                ammoList.Begin(new Rect(10f, 10f, boxRect.width - 20f, boxRect.height - 20f));

                ammoList.TextEntryLabeled($"Projectile {i + 1} defName: ", dataHolder.modified_projectileNames[i]);
                ammoList.TextEntryLabeled($"Projectile {i + 1} label: ", dataHolder.modified_projectileLabels[i]);
                ammoList.Label("TODO: projectile ThingClass");
                ammoList.Gap();

                string modified_damagesBuffer = dataHolder.modified_damages[i].ToString();
                int damage = dataHolder.modified_damages[i];
                ammoList.TextFieldNumericLabeled($"Projectile {i + 1} damage: ", ref damage, ref modified_damagesBuffer);
                dataHolder.modified_damages[i] = damage;

                ammoList.Label("Secondary Damages: ");

                for (int j = 0; j < dataHolder.modified_secondaryDamageDefs[i].Count; j++)
                {
                    Rect secondaryRect = new Rect(0, ammoList.CurHeight, boxRect.width - 30f, 150f);

                    GUI.BeginGroup(secondaryRect, GUI.skin.box);

                    Listing_Standard secondaryList = new Listing_Standard();
                    secondaryList.Begin(new Rect(10f, 10f, secondaryRect.width - 20f, secondaryRect.height - 20));

                    if (!dataHolder.modified_secondaryDamageDefs[i].NullOrEmpty())
                    {
                        if (Widgets.ButtonText(new Rect(secondaryRect.width / 2 - 100, secondaryList.curY, 200f, 30f), dataHolder.modified_secondaryDamageDefs[i][j].defName))
                        {
                            Find.WindowStack.Add(new Window_SelectDamageDef(dataHolder.modified_secondaryDamageDefs[i], j));
                        }
                        secondaryList.Gap(35f);

                        string modified_secondaryDamageAmountsBuffer = dataHolder.modified_secondaryDamageAmounts[i][j].ToString();
                        int damages = dataHolder.modified_secondaryDamageAmounts[i][j];
                        secondaryList.TextFieldNumericLabeled($"Secondary damage {i + 1} amount: ", ref damages, ref modified_secondaryDamageAmountsBuffer);
                        dataHolder.modified_secondaryDamageAmounts[i][j] = damages;

                        string modified_secondaryDamageChancesBuffer = dataHolder.modified_secondaryDamageChances[i][j].ToString();
                        float chances = dataHolder.modified_secondaryDamageChances[i][j];
                        secondaryList.TextFieldNumericLabeled($"Secondary damage {i + 1} chance (1.0 = 100%): ", ref chances, ref modified_secondaryDamageChancesBuffer);
                        dataHolder.modified_secondaryDamageChances[i][j] = chances;
                        secondaryList.Gap(20f);

                        if (Widgets.ButtonText(new Rect(secondaryRect.width / 2 - 100, secondaryList.curY, 200f, 30f), "REMOVE THIS"))
                        {
                            Find.WindowStack.Add(new Window_ConfirmDeleteSecondary(dataHolder, i, j));
                        }

                        ammoList.Gap(secondaryRect.height + 10f);
                    }

                    secondaryList.End();
                    GUI.EndGroup();
                }

                if (Widgets.ButtonText(new Rect(ammoList.curX + 200, ammoList.curY + 10f, 200f, 30f), "Add Secondary Damage"))
                {
                    dataHolder.AddGenericSecondaryDamage(i);
                }

                ammoList.Gap(40f);

                string modified_armorPenetrationSharpsBuffer = dataHolder.modified_armorPenetrationSharps[i].ToString();
                float apsharp = dataHolder.modified_armorPenetrationSharps[i];
                ammoList.TextFieldNumericLabeled($"Projectile {i + 1} sharp armor penetration: ", ref apsharp, ref modified_armorPenetrationSharpsBuffer);
                dataHolder.modified_armorPenetrationSharps[i] = apsharp;

                string modified_armorPenetrationBluntsBuffer = dataHolder.modified_armorPenetrationBlunts[i].ToString();
                float apblunt = dataHolder.modified_armorPenetrationBlunts[i];
                ammoList.TextFieldNumericLabeled($"Projectile {i + 1} blunt armor penetration: ", ref apblunt, ref modified_armorPenetrationBluntsBuffer);
                dataHolder.modified_armorPenetrationBlunts[i] = apblunt;

                string modified_speedsBuffer = dataHolder.modified_speeds[i].ToString();
                float speed = dataHolder.modified_speeds[i];
                ammoList.TextFieldNumericLabeled($"Projectile {i + 1} speed: ", ref speed, ref modified_speedsBuffer);
                dataHolder.modified_speeds[i] = speed;

                string modified_pelletCountsBuffer = dataHolder.modified_pelletCounts[i].ToString();
                int pellets = dataHolder.modified_pelletCounts[i];
                ammoList.TextFieldNumericLabeled($"Projectile {i + 1} pellet count: ", ref pellets, ref modified_pelletCountsBuffer);
                dataHolder.modified_pelletCounts[i] = pellets;

                string modified_spreadMultsBuffer = dataHolder.modified_spreadMults[i].ToString();
                float spread = dataHolder.modified_spreadMults[i];
                ammoList.TextFieldNumericLabeled($"Projectile {i + 1} spread multiplier: ", ref spread, ref modified_spreadMultsBuffer);
                dataHolder.modified_spreadMults[i] = spread;

                //TODO if statement to only show for explosive projectiles
                string modified_explosionRadiiBuffer = dataHolder.modified_explosionRadii[i].ToString();
                float expRadius = dataHolder.modified_explosionRadii[i];
                ammoList.TextFieldNumericLabeled($"Projectile {i + 1} explosion radius: ", ref expRadius, ref modified_explosionRadiiBuffer);
                dataHolder.modified_explosionRadii[i] = expRadius;

                //TODO if statement to only show for emp projectiles
                string modified_empShieldBreakChancesBuffer = dataHolder.modified_empShieldBreakChances[i].ToString();
                float emp = dataHolder.modified_empShieldBreakChances[i];
                ammoList.TextFieldNumericLabeled($"Projectile {i + 1} EMP shield break chance: ", ref emp, ref modified_empShieldBreakChancesBuffer);
                dataHolder.modified_empShieldBreakChances[i] = emp;

                string modified_suppressionFactorsBuffer = dataHolder.modified_suppressionFactors[i].ToString();
                float suppression = dataHolder.modified_suppressionFactors[i];
                ammoList.TextFieldNumericLabeled($"Projectile {i + 1} supression factor: ", ref suppression, ref modified_suppressionFactorsBuffer);
                dataHolder.modified_suppressionFactors[i] = suppression;

                string modified_dangerFactorsBuffer = dataHolder.modified_dangerFactors[i].ToString();
                float danger = dataHolder.modified_dangerFactors[i];
                ammoList.TextFieldNumericLabeled($"Projectile {i + 1} danger factor: ", ref danger, ref modified_dangerFactorsBuffer);
                dataHolder.modified_dangerFactors[i] = danger;

                bool incendiary = dataHolder.modified_ai_IsIncendiary[i];
                ammoList.CheckboxLabeled($"Projectile {i + 1} AI treats as incendiary: ", ref incendiary);
                dataHolder.modified_ai_IsIncendiary[i] = incendiary;

                //TODO if statement to only show for explosive projectiles
                bool neighbors = dataHolder.modified_applyDamageToExplosionCellsNeighbors[i];
                ammoList.CheckboxLabeled($"Projectile {i + 1} explosion applies damage to neighboring cells: ", ref neighbors);
                dataHolder.modified_applyDamageToExplosionCellsNeighbors[i] = neighbors;

                ammoList.End();
                GUI.EndGroup();

                list.Gap(boxHeight + spacing + (dataHolder.modified_secondaryDamageDefs[i].Count * 150f));
            }

            //TODO button to add additional ammos

            list.End();

            scrollHeight = list.CurHeight + 20f;
            viewRect.height = scrollHeight;

            Widgets.EndScrollView();
        }
    }
}
