﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using UnityEngine;

namespace nuff.AutoPatcherCombatExtended
{
    class Window_SuggestPatchMods : Window
    {
        bool noMods = true;
        bool showModsToAdd = false;
        bool showModsToRemove = false;
        bool selectAll = false;
        private Vector2 scrollPosition = Vector2.zero;

        public Window_SuggestPatchMods()
        {
            
        }

        public override void DoWindowContents(Rect inRect)
        {
            Listing_Standard list = new Listing_Standard();
            list.Begin(inRect);
            Text.Font = GameFont.Medium;
            list.Label("Nuff's Auto-Patcher for Combat Extended");
            list.Gap();

            Text.Font = GameFont.Small;

            #region AddMods
            if (showModsToAdd)
            {
                list.Label("The auto-patcher has detected new mods that it thinks need patching, that are not on the list of mods to patch.");
                list.Label("Please click the red \"X\" to select mods that you want patched, or click \"Select All\" down below.");
                list.Label("Hover over the \"i\" icon to see what defs caused that mod to be selected. Click \"Ignore\" to not suggest that mod again.");
                list.Gap(20);
                Rect scrollViewRect = new Rect(0f, 115f, inRect.width, inRect.height - 200);
                Rect viewRect = new Rect(0f, 0f, scrollViewRect.width - 20f, APCESettings.modsToRecommendAddDict.Count * 35f);
                List<ModContentPack> checksToChange = new List<ModContentPack>();
                List<ModContentPack> modsToIgnore = new List<ModContentPack>();

                GUI.BeginGroup(scrollViewRect, style: GUI.skin.box);
                Widgets.BeginScrollView(scrollViewRect.AtZero(), ref scrollPosition, viewRect, true);

                Rect checkboxRect = new Rect(0f, 0f, 30f, 30f);
                foreach (var mod in APCESettings.modsToRecommendAddDict)
                {
                    bool checkBool = mod.Value;

                    Widgets.Checkbox(checkboxRect.position, ref checkBool);

                    float labelX = checkboxRect.xMax + 10f;
                    float infoWidth = 24f;
                    float ignoreWidth = 90f;
                    float labelWidth = viewRect.width - checkboxRect.width - infoWidth - ignoreWidth - 20f;
                    Rect labelRect = new Rect(labelX, checkboxRect.y, labelWidth, 30f);
                    Widgets.Label(labelRect, mod.Key.Name);

                    Rect infoRect = new Rect(labelRect.xMax + 5f, checkboxRect.y, infoWidth, 24f);
                    APCESettings.modUnpatchedDefsDict.TryGetValue(mod.Key, out string str);
                    TooltipHandler.TipRegion(infoRect, $"Suggested due to: \n{str}");
                    GUI.DrawTexture(infoRect, TexButton.Info);

                    Rect ignoreButtonRect = new Rect(infoRect.xMax + 5f, checkboxRect.y, ignoreWidth, 30f);
                    if (Widgets.ButtonText(ignoreButtonRect, "Ignore"))
                    {
                        if (!APCESettings.modIgnoreList.Contains(mod.Key.PackageId))
                        {
                            APCESettings.modIgnoreList.Add(mod.Key.PackageId);
                            modsToIgnore.Add(mod.Key);
                        }
                    }

                    if (checkBool != mod.Value)
                    {
                        checksToChange.Add(mod.Key);
                    }

                    checkboxRect.y += 35f;
                }
                Widgets.EndScrollView();
                GUI.EndGroup();


                Rect selectAllRect = new Rect(inRect.width / 2 - 70f, inRect.height - 60f, 120f, 30f);
                if (Widgets.ButtonText(selectAllRect, "Select All"))
                {
                    selectAll = true;
                }

                if (selectAll)
                {
                    foreach (var mod in APCESettings.modsToRecommendAdd)
                    {
                        APCESettings.modsToRecommendAddDict[mod] = true;
                        selectAll = false;
                    }
                }
                else
                {
                    foreach (ModContentPack mod in checksToChange)
                    {
                        APCESettings.modsToRecommendAddDict[mod] = !APCESettings.modsToRecommendAddDict[mod];
                    }
                }

                foreach (ModContentPack mod in modsToIgnore)
                {
                    APCESettings.modsToRecommendAddDict.Remove(mod);
                }

                Rect continueRect = new Rect(inRect.width - 130f, inRect.height - 60f, 120f, 30f);
                if (Widgets.ButtonText(continueRect, "Continue"))
                {
                    showModsToAdd = false;
                }
            }
            #endregion

            #region RemoveMods
            else if (showModsToRemove)
            {
                list.Label("The auto-patcher has detected mods on your patch list that it believes are already CE-compatible.");
                list.Label("Please click the red \"X\" to select mods that you want to remove, or click \"Select All\" down below.");
                list.Gap(20);
                Rect scrollViewRect = new Rect(0f, 90f, inRect.width, inRect.height - 200);
                Rect viewRect = new Rect(0f, 0f, scrollViewRect.width - 20f, APCESettings.modsToRecommendRemoveDict.Count * 35f);
                List<ModContentPack> checksToChange = new List<ModContentPack>();

                GUI.BeginGroup(scrollViewRect, style: GUI.skin.box);
                Widgets.BeginScrollView(scrollViewRect.AtZero(), ref scrollPosition, viewRect, true);

                Rect checkboxRect = new Rect(0f, 0f, 30f, 30f);
                foreach (var mod in APCESettings.modsToRecommendRemoveDict)
                {
                    bool checkBool = mod.Value;

                    Widgets.Checkbox(checkboxRect.position, ref checkBool);
                    Rect labelRect = new Rect(checkboxRect.xMax + 10f, checkboxRect.y, viewRect.width - checkboxRect.width - 10f, 30f);
                    Widgets.Label(labelRect, mod.Key.Name);

                    if (checkBool != mod.Value)
                    {
                        checksToChange.Add(mod.Key);
                    }

                    checkboxRect.y += 35f;
                }
                Widgets.EndScrollView();
                GUI.EndGroup();


                Rect selectAllRect = new Rect(inRect.width / 2 - 70f, inRect.height - 60f, 120f, 30f);
                if (Widgets.ButtonText(selectAllRect, "Select All"))
                {
                    selectAll = true;
                }

                if (selectAll)
                {
                    foreach (var mod in APCESettings.modsToRecommendRemove)
                    {
                        APCESettings.modsToRecommendRemoveDict[mod] = true;
                        selectAll = false;
                    }
                }
                else
                {
                    foreach (ModContentPack mod in checksToChange)
                    {
                        APCESettings.modsToRecommendRemoveDict[mod] = !APCESettings.modsToRecommendRemoveDict[mod];
                    }
                }

                Rect continueRect = new Rect(inRect.width - 130f, inRect.height - 60f, 120f, 30f);
                if (Widgets.ButtonText(continueRect, "Continue"))
                {
                    showModsToRemove = false;
                }
            }
            #endregion

            else
            {
                this.Close();
            }

            list.End();
        }

        public override Vector2 InitialSize
        {
            get
            {
                return new Vector2(900, 700);
            }
        }

        public override void PreOpen()
        {
            base.PreOpen();

            APCEController.FindModsNeedingPatched();

            if (!APCESettings.modsToRecommendAdd.NullOrEmpty())
            {
                showModsToAdd = true;
                for (int i = 0; i < APCESettings.modsToRecommendAdd.Count; i++)
                {
                    if (!APCESettings.modIgnoreList.Contains(APCESettings.modsToRecommendAdd[i].PackageId))
                    {
                        APCESettings.modsToRecommendAddDict.Add(APCESettings.modsToRecommendAdd[i], false);
                    }
                }
            }
            if (!APCESettings.modsToRecommendRemove.NullOrEmpty())
            {
                showModsToRemove = true;
                for (int i = 0; i < APCESettings.modsToRecommendRemove.Count; i++)
                {
                    APCESettings.modsToRecommendRemoveDict.Add(APCESettings.modsToRecommendRemove[i], false);
                }
            }
        }

        public override void PreClose()
        {
            base.PreClose();
            if (APCESettings.modsByPackageId == null)
            {
                Log.Warning("List modsByPackageId was null, it should not have been");
                APCESettings.modsByPackageId = new List<string>();
            }
            if (APCESettings.modsToPatch == null)
            {
                Log.Warning("List modsToPatch was null, it should not have been");
                APCESettings.modsToPatch = new List<ModContentPack>();
            }
            if (!APCESettings.modsToRecommendAdd.NullOrEmpty())
            {
                foreach (var mod in APCESettings.modsToRecommendAddDict)
                {
                    try
                    {
                        if (APCESettings.modsToRecommendAddDict[mod.Key])
                        {
                            APCESettings.modsToPatch.Add(mod.Key);
                            APCESettings.modsByPackageId.Add(mod.Key.PackageId);
                            APCESettings.modsToRecommendAdd.Remove(mod.Key);
                        }
                    }
                    catch (Exception ex)
                    {
                        string error = "unknown";
                        if (mod.Key == null)
                            error = "a ModContentPack is null";
                        else if (mod.Key.PackageId == null)
                            error = $"PackageId for mod {mod.Key.Name} is null";
                        Log.Error($"Exception while adding mod to modsToPatch list. \nError is: {error}\n{ex.ToString()}");
                    }
                }
            }
            if (!APCESettings.modsToRecommendRemove.NullOrEmpty())
            {
                foreach (var mod in APCESettings.modsToRecommendRemoveDict)
                {
                    try
                    {
                        if (APCESettings.modsToRecommendRemoveDict[mod.Key])
                        {
                            APCESettings.modsToPatch.Remove(mod.Key);
                            APCESettings.modsByPackageId.Remove(mod.Key.PackageId);
                            APCESettings.modsToRecommendRemove.Remove(mod.Key);
                        }
                    }
                    catch (Exception ex)
                    {
                        string error = "unknown";
                        if (mod.Key == null)
                            error = "a ModContentPack is null";
                        else if (mod.Key.PackageId == null)
                            error = $"PackageId for mod {mod.Key.Name} is null";
                        Log.Error($"Exception while removing mod from modsToPatch list. \nError is: {error}\n{ex.ToString()}");
                    }
                }
            }

        }

        public override void PostClose()
        {
            base.PostClose();
            APCESettings.thisMod.WriteSettings();
        }
    }
}
