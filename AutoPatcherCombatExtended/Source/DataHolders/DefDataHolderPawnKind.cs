using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using CombatExtended;

namespace nuff.AutoPatcherCombatExtended
{
    class DefDataHolderPawnKind : DefDataHolder
    {
        public DefDataHolderPawnKind()
        {
            //empty constructor for use by SaveLoad
        }

        public DefDataHolderPawnKind(PawnKindDef def) : base(def)
        {
        }

        PawnKindDef kindDef;

        List<string> original_ApparelTags = new List<string>();
        List<string> original_WeaponTags = new List<string>();

        internal float original_CombatPower;
        internal float modified_CombatPower;

        internal List<string> modified_ApparelTags = new List<string>();
        internal List<string> modified_WeaponTags = new List<string>();
        internal float modified_MinMags;
        internal float modified_MaxMags;

        public override void GetOriginalData()
        {
            //constructed by APCEController, def assigned by constructor
            if (def != null && kindDef == null)
            {
                this.kindDef = def as PawnKindDef;
            }
            //constructed by SaveLoad, thingDef loaded from xml
            else if (kindDef != null && def == null)
            {
                def = kindDef;
            }

            StartNewLogEntry();
            logBuilder.AppendLine($"Starting GetOriginalData log entry for {def?.defName ?? "NULL DEF"}");

            try
            {
                original_CombatPower = kindDef.combatPower;

                if (kindDef.apparelTags != null)
                {
                    foreach (string str in kindDef.apparelTags)
                    {
                        original_ApparelTags.Add(str);
                    }
                }
                if (kindDef.weaponTags != null)
                {
                    foreach (string str in kindDef.weaponTags)
                    {
                        original_WeaponTags.Add(str);
                    }
                }
            }
            catch (Exception ex)
            {
                logBuilder.AppendLine($"Exception in GetOriginalData for: {def?.defName ?? "NULL DEF"}");
                logBuilder.AppendLine(ex.ToString());
                threwError = true;
            }
            finally
            {
                //TODO verbose logging
                PrintLog();
            }
        }

        public override void AutoCalculate()
        {
            StartNewLogEntry();
            logBuilder.AppendLine($"Starting AutoCalculate log entry for ammoset for {def?.defName ?? "NULL DEF"}");

            try
            {
                if (!original_ApparelTags.NullOrEmpty())
                {
                    foreach (string str in original_ApparelTags)
                    {
                        modified_ApparelTags.Add(str);
                    }
                }

                if (!original_WeaponTags.NullOrEmpty())
                {
                    foreach (string str in original_WeaponTags)
                    {
                        modified_WeaponTags.Add(str);
                    }

                    //if the PawnKind can spawn with weapons, give it the ability to have a backpack.
                    modified_ApparelTags.Add("IndustrialBasic");
                    modified_ApparelTags.Add("IndustrialMilitaryBasic");
                }

                modified_CombatPower = original_CombatPower;

                modified_MinMags = 2;
                modified_MaxMags = 5;
            }
            catch (Exception ex)
            {
                logBuilder.AppendLine($"Exception in AutoCalculate for: {def?.defName ?? "NULL DEF"}");
                logBuilder.AppendLine(ex.ToString());
                threwError = true;
            }
            finally
            {
                //TODO verbose logging
                PrintLog();
            }
        }

        public override void ApplyPatch()
        {
            StartNewLogEntry();
            logBuilder.AppendLine($"Starting ApplyPatch log entry for ammoset for {def?.defName ?? "NULL DEF"}");

            try
            {
                if (modified_ApparelTags.Count > 0)
                {
                    if (kindDef.apparelTags == null)
                        kindDef.apparelTags = new List<string>();
                    kindDef.apparelTags.Clear();
                    foreach (string str in modified_ApparelTags)
                    {
                        kindDef.apparelTags.Add(str);
                    }
                }

                if (modified_WeaponTags.Count > 0)
                {
                    if (kindDef.weaponTags == null)
                        kindDef.weaponTags = new List<string>();
                    kindDef.weaponTags.Clear();
                    foreach (string str in modified_WeaponTags)
                    {
                        kindDef.weaponTags.Add(str);
                    }
                }

                if (kindDef.modExtensions == null)
                {
                    kindDef.modExtensions = new List<DefModExtension>();
                }
                LoadoutPropertiesExtension loadout = new LoadoutPropertiesExtension();
                loadout.primaryMagazineCount = new FloatRange(modified_MinMags, modified_MaxMags);

                DataHolderUtils.AddOrReplaceExtension(kindDef, loadout);

                kindDef.combatPower = modified_CombatPower;
            }
            catch (Exception ex)
            {
                logBuilder.AppendLine($"Exception in Patch for: {def?.defName ?? "NULL DEF"}");
                logBuilder.AppendLine(ex.ToString());
                threwError = true;
            }
            finally
            {
                //TODO verbose logging
                PrintLog();
            }
        }

        public override StringBuilder ExportXML()
        {
            xml = DataHolderUtils.GetXmlForDef(kindDef);

            patchOps = new List<string>();

            //todo patches to add apparel tags only after I actually implement customizing them
            patchOps.Add(GenerateLoadoutExtensionPatch());
            patchOps.Add(GenerateCombatPowerPatch());

            base.ExportXML();

            return patch;

            string GenerateLoadoutExtensionPatch()
            {
                string xpath = $"Defs/PawnKindDef[defName=\"{defName}\"]";
                StringBuilder patch = new StringBuilder();

                patch.AppendLine("\t<Operation Class=\"PatchOperationAddModExtension\">");
                patch.AppendLine($"\t\t<xpath>{xpath}</xpath>");
                patch.AppendLine("\t\t<value>");
                patch.AppendLine("\t\t\t<li Class=\"CombatExtended.LoadoutPropertiesExtension\">");
                patch.AppendLine($"\t\t\t\t<primaryMagazineCount>");
                patch.AppendLine($"\t\t\t\t\t<min>{modified_MinMags}</min>");
                patch.AppendLine($"\t\t\t\t\t<max>{modified_MaxMags}</max>");
                patch.AppendLine($"\t\t\t\t</primaryMagazineCount>");
                patch.AppendLine("\t\t\t</li>");
                patch.AppendLine("\t\t</value>");
                patch.AppendLine("\t</Operation>");
                patch.AppendLine();

                return patch.ToString();
            }

            string GenerateCombatPowerPatch()
            {
                if (modified_CombatPower == original_CombatPower)
                {
                    return null;
                }

                StringBuilder patch = new StringBuilder();

                bool nodeExists = xml.SelectSingleNode("combatPower") != null;

                string xpath = $"Defs/PawnKindDef[defName=\"{defName}\"]{(nodeExists ? "/combatPower" : "")}";

                patch.AppendLine($"\t<Operation Class=\"{(nodeExists ? "PatchOperationReplace" : "PatchOperationAdd")}\">");
                patch.AppendLine($"\t\t<xpath>{xpath}</xpath>");
                patch.AppendLine("\t\t<value>");
                patch.AppendLine($"\t\t\t<combatPower>{modified_CombatPower}</combatPower>");
                patch.AppendLine("\t\t</value>");
                patch.AppendLine("\t</Operation>");
                patch.AppendLine();

                return patch.ToString();
            }
        }

        public override void ExposeData()
        {
            Scribe_Defs.Look(ref kindDef, "def");
            Scribe_Collections.Look(ref modified_ApparelTags, "modified_ApparelTags");
            Scribe_Collections.Look(ref modified_WeaponTags, "modified_WeaponTags");

            Scribe_Values.Look(ref modified_MinMags, "modified_MinMags");
            Scribe_Values.Look(ref modified_MaxMags, "modified_MaxMags");
            Scribe_Values.Look(ref modified_CombatPower, "modified_CombatPower");

            base.ExposeData();
        }
    }
}
