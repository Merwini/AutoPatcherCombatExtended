using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using System.Reflection;

namespace nuff.AutoPatcherCombatExtended
{

    #region HugsLibLicense
    /*
	 * Code used from HugsLib by UnlimitedHugs. License info:
	 * 
	 * This is free and unencumbered software released into the public domain.

		Anyone is free to copy, modify, publish, use, compile, sell, or
		distribute this software, either in source code form or as a compiled
		binary, for any purpose, commercial or non-commercial, and by any means.

		HugsLib includes a compiled version of the Harmony library made by Andreas Pardeike (MIT license).
	 */
    #endregion

    public static class InjectedDefHasher
	{
		private delegate void GiveShortHashTakenHashes(Def def, Type defType, HashSet<ushort> takenHashes);
		private delegate void GiveShortHash(Def def, Type defType);
		private static GiveShortHash giveShortHashDelegate;

		public static void PrepareReflection()
		{
			try
			{
				var takenHashesField = typeof(ShortHashGiver).GetField(
					"takenHashesPerDeftype", BindingFlags.Static | BindingFlags.NonPublic);
				var takenHashesDictionary = takenHashesField?.GetValue(null) as Dictionary<Type, HashSet<ushort>>;
				if (takenHashesDictionary == null) throw new Exception("taken hashes");

				var methodInfo = typeof(ShortHashGiver).GetMethod(
					"GiveShortHash", BindingFlags.NonPublic | BindingFlags.Static,
					null, new[] { typeof(Def), typeof(Type), typeof(HashSet<ushort>) }, null);
				if (methodInfo == null) throw new Exception("hashing method");

				var hashDelegate = (GiveShortHashTakenHashes)Delegate.CreateDelegate(
					typeof(GiveShortHashTakenHashes), methodInfo);
				giveShortHashDelegate = (def, defType) => {
					var takenHashes = takenHashesDictionary.TryGetValue(defType);
					if (takenHashes == null)
					{
						takenHashes = new HashSet<ushort>();
						takenHashesDictionary.Add(defType, takenHashes);
					}
					hashDelegate(def, defType, takenHashes);
				};
			}
			catch (Exception ex)
			{
				//HugsLibController.Logger.Error($"Failed to reflect short hash dependencies: {e.Message}");
				//TODO make my own exception for this
			}
		}

		/// <summary>
		/// Give a short hash to a def created at runtime.
		/// Short hashes are used for proper saving of defs in compressed maps within a save file.
		/// </summary>
		/// <param name="newDef"></param>
		/// <param name="defType">The type of defs your def will be saved with. For example,
		/// use typeof(ThingDef) if your def extends ThingDef.</param>
		public static void GiveShortHashToDef(Def newDef, Type defType)
		{
			if (giveShortHashDelegate == null) throw new Exception("Hasher not initialized");
			giveShortHashDelegate(newDef, defType);
		}
	}
}
