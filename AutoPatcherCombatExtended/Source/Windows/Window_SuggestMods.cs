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

        //TODO
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

                Rect scrollViewRect = new Rect(0f, 0f, inRect.width, inRect.height);
                Rect checkboxRect = new Rect(0f, 200f, 30f, 30f);
                List<ModContentPack> checksToChange = new List<ModContentPack>();
                Widgets.BeginScrollView(inRect, ref scrollPosition, scrollViewRect, true);
                foreach (var mod in APCESettings.modsToRecommendAddDict)
                {
                    bool checkBool = mod.Value;

                    Widgets.Checkbox(checkboxRect.position, ref checkBool);
                    Rect labelRect = new Rect(checkboxRect.xMax + 10f, checkboxRect.y, inRect.width - checkboxRect.width - 10f, 30f);
                    Widgets.Label(labelRect, mod.Key.Name);

                    if (checkBool != mod.Value)
                    {
                        checksToChange.Add(mod.Key);
                    }

                    checkboxRect.y += 35f;
                }
                Widgets.EndScrollView();


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

                // Continue Button
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

                Rect scrollViewRect = new Rect(0f, 0f, inRect.width, inRect.height);
                Rect checkboxRect = new Rect(0f, 200f, 30f, 30f);
                List<ModContentPack> checksToChange = new List<ModContentPack>();
                Widgets.BeginScrollView(inRect, ref scrollPosition, scrollViewRect, true);
                foreach (var mod in APCESettings.modsToRecommendRemoveDict)
                {
                    bool checkBool = mod.Value;

                    Widgets.Checkbox(checkboxRect.position, ref checkBool);
                    Rect labelRect = new Rect(checkboxRect.xMax + 10f, checkboxRect.y, inRect.width - checkboxRect.width - 10f, 30f);
                    Widgets.Label(labelRect, mod.Key.Name);

                    if (checkBool != mod.Value)
                    {
                        checksToChange.Add(mod.Key);
                    }

                    checkboxRect.y += 35f;
                }
                Widgets.EndScrollView();


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

                // Continue Button
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

            ////TODO remove the below after rewrite
            //if (noMods)
            //{
            //    list.Label("The auto-patcher has detected no mods in need of patching, or all mods are already on the patch list.");
            //}
            //else
            //{
            //    list.Label("The auto-patcher has detected mods that it thinks need patching.");
            //    list.Label("Please click the red \"X\" to select mods that you want patched, or click \"Select All\" down below. Once a mod is selected, the auto-patcher will not ask about it next time you open the game.");
                
            //}
            //list.Label("Mods can be manually added and removed from the patching list through Mod Options. Removing mods requires a game restart afterward, as they cannot be un-patched.");
            //if (!noMods)
            //{
            //    Rect scrollViewRect = new Rect(0f, 0f, inRect.width, inRect.height);
            //    Rect checkboxRect = new Rect(0f, 200f, 30f, 30f);
            //    List<ModContentPack> checksToChange = new List<ModContentPack>();
            //    Widgets.BeginScrollView(inRect, ref scrollPosition, scrollViewRect, true);
            //    foreach (var mod in APCESettings.modsToRecommendAddDict)
            //    {
            //        bool checkBool = mod.Value;

            //        Widgets.Checkbox(checkboxRect.position, ref checkBool);
            //        Rect labelRect = new Rect(checkboxRect.xMax + 10f, checkboxRect.y, inRect.width - checkboxRect.width - 10f, 30f);
            //        Widgets.Label(labelRect, mod.Key.Name);

            //        if (checkBool != mod.Value)
            //        {
            //            checksToChange.Add(mod.Key);
            //        }

            //        checkboxRect.y += 35f;
            //    }
            //    Widgets.EndScrollView();
                

            //    Rect selectAllRect = new Rect(inRect.width / 2 - 70f, inRect.height - 60f, 120f, 30f);
            //    if (Widgets.ButtonText(selectAllRect, "Select All"))
            //    {
            //        selectAll = true;
            //    }

            //    if (selectAll)
            //    {
            //        foreach (var mod in APCESettings.modsToRecommendAdd)
            //        {
            //            APCESettings.modsToRecommendAddDict[mod] = true;
            //            selectAll = false;
            //        }
            //    }
            //    else
            //    {
            //        foreach (ModContentPack mod in checksToChange)
            //        {
            //            APCESettings.modsToRecommendAddDict[mod] = !APCESettings.modsToRecommendAddDict[mod];
            //        }
            //    }

            //    // Continue Button
            //    Rect continueRect = new Rect(inRect.width - 130f, inRect.height - 60f, 120f, 30f);
            //    if (Widgets.ButtonText(continueRect, "Continue"))
            //    {
            //        this.Close();
            //    }
            //}

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
                    APCESettings.modsToRecommendAddDict.Add(APCESettings.modsToRecommendAdd[i], false);
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
            try
            {
                foreach (var mod in APCESettings.modsToRecommendAddDict)
                {
                    if (APCESettings.modsToRecommendAddDict[mod.Key])
                    {
                        APCESettings.modsToPatch.Add(mod.Key);
                        APCESettings.modsByPackageId.Add(mod.Key.PackageId);
                        APCESettings.modsToRecommendAdd.Remove(mod.Key);
                    }
                }
                foreach (var mod in APCESettings.modsToRecommendRemoveDict)
                {
                    if (APCESettings.modsToRecommendRemoveDict[mod.Key])
                    {
                        APCESettings.modsToPatch.Remove(mod.Key);
                        APCESettings.modsByPackageId.Remove(mod.Key.PackageId);
                        APCESettings.modsToRecommendRemove.Remove(mod.Key);
                    }
                }
                APCESettings.thisMod.WriteSettings();
            }
            catch (Exception ex)
            {
                Log.Error("Exception while closing mod suggestion window: \n" + ex.ToString());
            }


        }

        public override void PostClose()
        {
            APCEController.APCEPatchController();
            base.PostClose();
        }
    }
}
