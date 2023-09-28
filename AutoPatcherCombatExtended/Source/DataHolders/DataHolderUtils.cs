using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using CombatExtended;
using System.Reflection;

namespace nuff.AutoPatcherCombatExtended
{
    public static class DataHolderUtils
    {
        //Made my own so I can use it to change equippedStatOffSets, too
        public static void AddOrChangeStat(List<StatModifier> list, StatDef stat, float value)
        {
            int index = list.FindIndex(x => x.stat == stat);
            if (index != -1)
            {
                list[index].value = value;
            }
            //can't think of a use case where I would need to add a 0 value statmod, and adding this check will save vetting in the patch methods
            else if (value != 0)
            {
                list.Add(new StatModifier() { stat = stat, value = value });
            }
        }

        public static ModDataHolder ReturnModDataOrDefault(Def def)
        {
            ModDataHolder modData = APCESettings.modDataDict.TryGetValue(def.modContentPack.PackageId);
            if (modData == null)
            {
                modData = APCESettings.modDataDict.TryGetValue("nuff.ceautopatcher");
            }
            return modData;
        }

        internal static void CopyFields(object source, object destination)
        {
            if (source == null || destination == null)
            {
                return;
            }
            Type sourceType = source.GetType();
            Type destType = destination.GetType();

            foreach (FieldInfo sourceField in sourceType.GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                FieldInfo destField = destType.GetField(sourceField.Name, BindingFlags.Public | BindingFlags.Instance);
                if (destField != null && destField.FieldType == sourceField.FieldType)
                {
                    object value = sourceField.GetValue(source);
                    if (destField != null)
                    {
                        destField.SetValue(destination, value);
                    }
                }
            }
        }

        public static ToolCE MakeToolBase(Tool tool)
        {
            ToolCE newToolCE = new ToolCE();
            CopyFields(tool, newToolCE);
            newToolCE.id = "APCE_Tool_" + tool.id;

            //CE is far more punishing if you have no armor penetration than vanilla is, so it is essential to have some
            if (tool.armorPenetration <= 0)
            {
                newToolCE.armorPenetrationSharp = tool.power * 0.1f;
                newToolCE.armorPenetrationBlunt = tool.power * 0.1f;
            }
            else
            {
                newToolCE.armorPenetrationSharp = tool.armorPenetration;
                newToolCE.armorPenetrationBlunt = tool.armorPenetration;
            }

            return newToolCE;
        }
    }
}
