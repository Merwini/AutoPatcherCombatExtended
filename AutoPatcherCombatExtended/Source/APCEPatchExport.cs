using CombatExtended;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using Verse;

namespace nuff.AutoPatcherCombatExtended
{
    public static class APCEPatchExport
    {


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

                // Remove the node by replacing it with nothing
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
                patch = patch + "\n";
            }

            return patch;
        }
    }
}