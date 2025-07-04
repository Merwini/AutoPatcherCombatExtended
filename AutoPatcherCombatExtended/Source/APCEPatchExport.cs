﻿using CombatExtended;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.IO;
using Verse;

namespace nuff.AutoPatcherCombatExtended
{
    public static class APCEPatchExport
    {
        public static void ExportPatchesForMod(ModDataHolder modData)
        {
            //I keep forgetting to close the mod settings window before closing the game.
            APCESettings.thisMod.WriteSettings();

            StringBuilder masterPatch = new StringBuilder();
            StringBuilder patchLog = new StringBuilder();

            patchLog.AppendLine($"Patch info for mod {modData.mod.Name}");
            patchLog.AppendLine();

            foreach (var entry in modData.defsToPatch)
            {
                if (entry.Value == APCEConstants.NeedsPatch.yes && modData.defDict.TryGetValue(entry.Key, out DefDataHolder ddh))
                {
                    patchLog.AppendLine($"Patch info for {ddh.def.defName}:");
                    try
                    {
                        masterPatch.Append(ddh.ExportXML());
                        patchLog.AppendLine("Patch exported successfully.");
                    }
                    catch (Exception ex)
                    {
                        patchLog.AppendLine("Patch failed to export due to issue:");
                        patchLog.AppendLine(ex.ToString());
                    }
                    finally
                    {
                        patchLog.AppendLine();
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(masterPatch.ToString()))
            {
                patchLog.AppendLine($"ExportPatchesForMod {modData.mod.Name} aborted: no patches were generated. Make sure to select some defs to patch first.");
                return;
            }

            masterPatch.Insert(0, "<Patch>\n\n");
            masterPatch.AppendLine("\n</Patch>");

            string modFolderPath = CreatePatchesFolderForMod(modData);

            if (modFolderPath == null)
            {
                //error already thrown in CreatePatchesFolderForMod
                return;
            }

            WritePatchToFile(modFolderPath, masterPatch);
            WritePatchLogToFile(modFolderPath, patchLog);

            Find.WindowStack.Add(new Window_ShowPatchInfo(patchLog, modFolderPath, modData.mod.Name));
        }

        public static string GeneratePatchOperationFor(XmlNode node, string targetNode, string targetStat, float value, float originalValue = -55555.55555f)
        {
            if (value == originalValue)
            {
                return null;
            }

            if (node == null)
            {
                Log.Warning("Cannot generate patch: XML node is null.");
                return null;
            }

            XmlNode defNameNode = node.SelectSingleNode("defName");
            if (defNameNode == null)
            {
                Log.Warning("Cannot generate patch: defName not found in XML node.");
                return null;
            }

            string defName = defNameNode.InnerText;
            XmlNode statGroupNode = node.SelectSingleNode(targetNode);
            bool statExists = false;

            if (statGroupNode != null)
            {
                foreach (XmlNode statNode in statGroupNode.ChildNodes)
                {
                    if (statNode.Name == targetStat)
                    {
                        statExists = true;
                        break;
                    }
                }
            }

            string xpath = $"Defs/{node.Name}[defName=\"{defName}\"]/{targetNode}";
            StringBuilder patch = new StringBuilder();

            if (value == 0)
            {
                if (!statExists)
                {
                    return null;
                }

                patch.AppendLine("\t<Operation Class=\"PatchOperationReplace\">");
                patch.AppendLine($"\t\t<xpath>{xpath}/{targetStat}</xpath>");
                patch.AppendLine("\t\t<value />");
                patch.AppendLine("\t</Operation>");
                patch.AppendLine();
            }
            else if (statExists)
            {
                patch.AppendLine("\t<Operation Class=\"PatchOperationReplace\">");
                patch.AppendLine($"\t\t<xpath>{xpath}/{targetStat}</xpath>");
                patch.AppendLine("\t\t<value>");
                patch.AppendLine($"\t\t\t<{targetStat}>{value}</{targetStat}>");
                patch.AppendLine("\t\t</value>");
                patch.AppendLine("\t</Operation>");
                patch.AppendLine();
            }
            else
            {
                patch.AppendLine("\t<Operation Class=\"PatchOperationAdd\">");
                patch.AppendLine($"\t\t<xpath>{xpath}</xpath>");
                patch.AppendLine("\t\t<value>");
                patch.AppendLine($"\t\t\t<{targetStat}>{value}</{targetStat}>");
                patch.AppendLine("\t\t</value>");
                patch.AppendLine("\t</Operation>");
                patch.AppendLine();
            }

            return patch.ToString();
        }

        //redundant
        //public static string AddOrReplaceXmlNodeWhitespace(XmlNode node, string targetNode, string targetStat, float value, float originalValue = -55555.55555f)
        //{
        //    string patch = GeneratePatchOperationFor(node, targetNode, targetStat, value, originalValue);
        //    if (patch != null)
        //    {
        //        patch = patch + "\n\n";
        //    }

        //    return patch;
        //}

        public static string RemoveXmlNode(XmlNode node, string targetParent, string targetStat)
        {
            if (node == null)
            {
                Log.Warning($"Cannot generate patch to remove {targetStat}: XML node is null.");
                return null;
            }

            XmlNode defNameNode = node.SelectSingleNode("defName");
            if (defNameNode == null)
            {
                Log.Warning($"Cannot generate patch to remove {targetStat}: defName not found in the XML node.");
                return null;
            }

            string defName = defNameNode.InnerText;

            XmlNode parentNode = node.SelectSingleNode(targetParent);
            if (parentNode == null)
            {
                return null;
            }

            bool targetExists = false;
            foreach (XmlNode child in parentNode.ChildNodes)
            {
                if (child.Name == targetStat)
                {
                    targetExists = true;
                    break;
                }
            }

            if (!targetExists)
            {
                return null;
            }

            string xpath = $"Defs/{node.Name}[defName=\"{defName}\"]/{targetParent}/{targetStat}";

            StringBuilder patch = new StringBuilder();
            patch.AppendLine("\t<Operation Class=\"PatchOperationRemove\">");
            patch.AppendLine($"\t\t<xpath>{xpath}</xpath>");
            patch.AppendLine("\t</Operation>");
            patch.AppendLine();

            return patch.ToString();
        }

        public static void CleanPatchOpsList(ref List<string> patchOps)
        {
            if (patchOps == null)
            {
                patchOps = new List<string>();
                return;
            }

            patchOps = patchOps.Where(p => p != null).ToList();
        }

        public static void AddPatchHeader(Def def, StringBuilder patch)
        {
            patch.Append($"<!-- === {def.label} === -->\n");
        }

        public static void AddNecessaryNodes(XmlNode node, Def def, StringBuilder patch, List<string> patchOps)
        {
            bool hasStatBases = node.SelectSingleNode("statBases") != null;
            bool hasEquippedStatOffsets = node.SelectSingleNode("equippedStatOffsets") != null;
            bool hasComps = node.SelectSingleNode("comps") != null;
            bool hasModExtensions = node.SelectSingleNode("modExtensions") != null;
            bool hasTools = node.SelectSingleNode("tools") != null;

            bool needsStatBases = false;
            bool needsEquippedStatOffsets = false;
            bool needsComps = false;
            bool needsModExtensions = false;
            bool needsTools = false;

            foreach (string patchOp in patchOps)
            {
                if (patchOp.Contains("PatchOperationAdd"))
                {
                    if (patchOp.Contains("statBases"))
                        needsStatBases = true;
                    else if (patchOp.Contains("equippedStatOffsets"))
                        needsEquippedStatOffsets = true;
                    else if (patchOp.Contains("comps"))
                        needsComps = true;
                    else if (patchOp.Contains("ModExtension"))
                        needsModExtensions = true;
                    else if (patchOp.Contains("tools"))
                        needsTools = true;
                }
            }

            if (needsStatBases && !hasStatBases)
            {
                patch.AppendLine($"\t<Operation Class=\"PatchOperationAdd\">");
                patch.AppendLine($"\t\t<xpath>Defs/{node.Name}[defName='{def.defName}']</xpath>");
                patch.AppendLine($"\t\t<value><statBases></statBases></value>");
                patch.AppendLine($"\t</Operation>");
                patch.AppendLine();
            }

            if (needsEquippedStatOffsets && !hasEquippedStatOffsets)
            {
                patch.AppendLine($"\t<Operation Class=\"PatchOperationAdd\">");
                patch.AppendLine($"\t\t<xpath>Defs/{node.Name}[defName='{def.defName}']</xpath>");
                patch.AppendLine($"\t\t<value><equippedStatOffsets></equippedStatOffsets></value>");
                patch.AppendLine($"\t</Operation>");
                patch.AppendLine();
            }

            if (needsComps && !hasComps)
            {
                patch.AppendLine($"\t<Operation Class=\"PatchOperationAdd\">");
                patch.AppendLine($"\t\t<xpath>Defs/{node.Name}[defName='{def.defName}']</xpath>");
                patch.AppendLine($"\t\t<value><comps></comps></value>");
                patch.AppendLine($"\t</Operation>");
                patch.AppendLine();
            }

            if (needsModExtensions && !hasModExtensions)
            {
                patch.AppendLine($"\t<Operation Class=\"PatchOperationAdd\">");
                patch.AppendLine($"\t\t<xpath>Defs/{node.Name}[defName='{def.defName}']</xpath>");
                patch.AppendLine($"\t\t<value><modExtensions></modExtensions></value>");
                patch.AppendLine($"\t</Operation>");
                patch.AppendLine();
            }

            if (needsTools && !hasTools)
            {
                patch.AppendLine($"\t<Operation Class=\"PatchOperationAdd\">");
                patch.AppendLine($"\t\t<xpath>Defs/{node.Name}[defName='{def.defName}']</xpath>");
                patch.AppendLine($"\t\t<value><tools></tools></value>");
                patch.AppendLine($"\t</Operation>");
                patch.AppendLine();
            }
        }

        public static void AddPatchOps(XmlNode node, Def def, StringBuilder patch, List<string> patchOps)
        {
            foreach (string op in patchOps)
            {
                patch.Append(op);
            }
        }

        public static string CreatePatchesFolderForMod(ModDataHolder mdh)
        {
            //these folders are created by APCESaveLoad, which will have already run
            string folderPath = Path.Combine(GenFilePaths.SaveDataFolderPath, "NuffsAutoPatcher");
            string exportedPatchesPath = Path.Combine(folderPath, "ExportedPatches");

            string modFolderPath = Path.Combine(exportedPatchesPath, mdh.packageId);

            try
            {
                if (!Directory.Exists(modFolderPath))
                {
                    Directory.CreateDirectory(modFolderPath);
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Error while making folders to save exported patches for mod {mdh.mod.Name}: \n {ex.ToString()}");
            }

            if (Directory.Exists(modFolderPath))
            {
                return modFolderPath;
            }
            else
            {
                return null;
            }
        }

        public static void WritePatchToFile(string folderPath, StringBuilder patch)
        {
            string filePath = Path.Combine(folderPath, "Patches.xml");

            try
            {
                File.WriteAllText(filePath, patch.ToString());

                Log.Message($"Patch successfully written to {filePath}");
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to write patch to {filePath}: {ex.Message}");
            }
        }

        public static void WritePatchLogToFile(string folderPath, StringBuilder patchLog)
        {
            string filePath = Path.Combine(folderPath, "PatchLog.txt");

            try
            {
                File.WriteAllText(filePath, patchLog.ToString());

                Log.Message($"Patch log successfully written to {filePath}");
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to write patch log to {filePath}: {ex.Message}");
            }
        }
    }
}