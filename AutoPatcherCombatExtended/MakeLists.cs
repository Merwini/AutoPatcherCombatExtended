using CombatExtended;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using Verse;
/*
namespace nuff.AutoPatcherCombatExtended

{

    public partial class AutoPatcherCombatExtended : Mod
    {
        // declare lists here TODO
        public List<ThingDef> weaponList = new List<ThingDef>();
        public List<ThingDef> apparelList = new List<ThingDef>();
        public List<ThingDef> pawnList = new List<ThingDef>();
        public List<ThingDef> turretList = new List<ThingDef>();

        public List<ModContentPack> modsNeedingPatches = new List<ModContentPack>();

        public List<ThingDef> projectileList = new List<ThingDef>();
        public List<AmmoSetDef> ammoSetList = new List<AmmoSetDef>();
        public List<AmmoDef> ammoDefList = new List<AmmoDef>();

        private void MakeLists()
        {
            #region TimerStart
            if (Settings.printDebug)
            {
                stopwatch.Start();
                Log.Message("Autopatcher for Combat Extended list-making has started.");
            }
            #endregion

            // make da lists TODO
                // find gun/ammo/recipe references
                // find weapons,apparel,pawns,turrets in DefDatabase
                // find hediffs

            foreach (ThingDef td in DefDatabase<ThingDef>.AllDefs)
            {
                try
                {
                    //why did I make a list of projectiles in CEAP? TODO
                    //if is weapon and needs patched
                    if (td.IsWeapon
                        && (td.thingClass != null)
                        && (!td.thingClass.ToString().Contains("TurretGun"))
                        && (td.tools != null)
                        && td.tools.Any(tool => !typeof(ToolCE).IsAssignableFrom(tool.GetType()))
                        )
                    {
                        UpdateModList(td);
                        weaponList.Add(td);
                        continue;
                    }
                    //if is apparel and needs patched
                    if (td.IsApparel
                        && (td.statBases.FindIndex(wob => wob.ToString().Contains("Bulk")) == -1))
                    {
                        UpdateModList(td);
                        apparelList.Add(td);
                        continue;
                    }

                    //if is pawn and needs patched
                    if (typeof(Pawn).IsAssignableFrom(td.thingClass)
                        && td.tools.Any(tool => !typeof(ToolCE).IsAssignableFrom(tool.GetType())))
                    {
                        UpdateModList(td);
                        pawnList.Add(td);
                        continue;
                    }

                    //if is turret and needs patched
                    if ((td.thingClass != null) 
                        && (td.thingClass.ToString().Contains("TurretGun")))
                    {
                        turretList.Add(td);
                        continue;
                    }
                }
                catch(Exception ex)
                {
                    Log.Message("Error on Thing: " + td.defName);
                    BasicException(ex);
                    continue;
                }
                finally
                {

                }
            }

            /* TODO figure out how to determine if a Hediff needs to be patched
             * mainly concerned with multiplying armor values, but no way to tell if they need to be other than assuming based on value
            foreach (HediffDef hd in DefDatabase<HediffDef>.AllDefs)
             * idea: make a list of mods with other content that needs patching
            {
                try
                {

                }
                catch(Exception ex)
                {
                    BasicException(ex);
                    continue;
                }
                finally
                {

                }
            }

#region TimerStop
if (Settings.printDebug)
            {
                stopwatch.Stop();
                Log.Message($"Combat Extended Autopatcher has finished making lists in {stopwatch.ElapsedMilliseconds / 1000f} seconds.");
                stopwatch.Reset();
            }
            #endregion
        }
    }
    
}
*/