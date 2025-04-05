using CombatExtended;
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
            StringBuilder masterPatch = new StringBuilder();
            masterPatch.AppendLine("<Patch>\n\n");

            foreach (var entry in modData.defsToPatch)
            {
                if (entry.value == APCEConstants.NeedsPatch.yes && modData.defDict.TryGetValue(entry.key, out DefDataHolder ddh))
                {
                    try
                    {
                        masterPatch.Append(ddh.ExportXML());
                    }
                    catch (Exception ex)
                    {
                        Log.Warning($"Failed to export patch for def {ddh.def.defName} from mod {modData.mod.Name} due to exception: \n" + ex.ToString());
                    }
                }
            }

            masterPatch.AppendLine("\n</Patch>");

            string modFolderPath = CreatePatchesFolderForMod(modData);

            if (modFolderPath == null)
            {
                //error already thrown in CreatePatchesFolderForMod
                return;
            }

            WritePatchToFile(modFolderPath, masterPatch, modData);
        }

        public static string GeneratePatchOperationFor(XmlNode node, string targetNode, string targetStat, float value)
        {
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

            string xpath = $"/Defs/{node.Name}[defName=\"{defName}\"]/{targetNode}";
            string patch;

            if (value == 0)
            {
                if (!statExists)
                {
                    return null;
                }

                patch = $@"<Operation Class=""PatchOperationReplace"">
    <xpath>{xpath}/{targetStat}</xpath>
    <value />
</Operation>";
            }
            else if (statExists)
            {
                patch = $@"<Operation Class=""PatchOperationReplace"">
    <xpath>{xpath}/{targetStat}</xpath>
    <value>
        <{targetStat}>{value}</{targetStat}>
    </value>
</Operation>";
            }
            else
            {
                patch = $@"<Operation Class=""PatchOperationAdd"">
    <xpath>{xpath}</xpath>
    <value>
        <{targetStat}>{value}</{targetStat}>
    </value>
</Operation>";
            }

            return patch;
        }

        public static string AddOrReplaceXmlNodeWhitespace(XmlNode node, string targetNode, string targetStat, float value)
        {
            string patch = GeneratePatchOperationFor(node, targetNode, targetStat, value);
            if (patch != null)
            {
                patch = patch + "\n\n";
            }

            return patch;
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

            bool needsStatBases = false;
            bool needsEquippedStatOffsets = false;
            bool needsComps = false;
            bool needsModExtensions = false;

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
                }
            }

            if (needsStatBases && !hasStatBases)
            {
                patch.AppendLine($@"<Operation Class=""PatchOperationAdd"">");
                patch.AppendLine($"\t<xpath>/Defs/{node.Name}[defName='{def.defName}']</xpath>");
                patch.AppendLine($"\t<value><statBases></statBases></value>");
                patch.AppendLine($"</Operation>");
            }

            if (needsEquippedStatOffsets && !hasEquippedStatOffsets)
            {
                patch.AppendLine($@"<Operation Class=""PatchOperationAdd"">");
                patch.AppendLine($"\t<xpath>/Defs/{node.Name}[defName='{def.defName}']</xpath>");
                patch.AppendLine($"\t<value><equippedStatOffsets></equippedStatOffsets></value>");
                patch.AppendLine($"</Operation>");
            }

            if (needsComps && !hasComps)
            {
                patch.AppendLine($@"<Operation Class=""PatchOperationAdd"">");
                patch.AppendLine($"\t<xpath>/Defs/{node.Name}[defName='{def.defName}']</xpath>");
                patch.AppendLine($"\t<value><comps></comps></value>");
                patch.AppendLine($"</Operation>");
            }

            if (needsModExtensions && !hasModExtensions)
            {
                patch.AppendLine($@"<Operation Class=""PatchOperationAdd"">");
                patch.AppendLine($"\t<xpath>/Defs/{node.Name}[defName='{def.defName}']</xpath>");
                patch.AppendLine($"\t<value><modExtensions></modExtensions></value>");
                patch.AppendLine($"</Operation>");
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

        public static void WritePatchToFile(string folderPath, StringBuilder patch, ModDataHolder modData)
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
    }
}