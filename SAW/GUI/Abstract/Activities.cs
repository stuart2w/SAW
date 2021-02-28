using System;
using System.Windows.Forms;

namespace SAW
{
	/// <summary>Utilities for dealing with activities and selection thereof.
	/// An activity is just a configuration placed as a shim over the system settings and below the user settings.
	/// These aren't used (much) at the moment in SAW</summary>
	internal class Activities
	{
		public static bool AllActivitiesLoaded;

		private Activities()
		{
		}

		#region ID definitions

		internal static Guid PaletteID = new Guid("0ca6dcd6-4418-4f0a-bdc4-2a2c0f4ff4ba");
		internal static Guid SAW6 = new Guid("f4af4489-204a-4b28-9997-115e18a24124");
		internal static Guid Graphics = new Guid("0290fe4a-282c-4f29-8608-ae8114d0c819");

		#endregion

		public static string GetActivityText(Guid activity, bool translate, bool display)
		{
			if (activity.Equals(Guid.Empty))
				return Strings.Item("Activity_Default");
			Document document = GetActivitySettings(activity);
			if (document != null)
			{
				string name = display ? document.DisplayName : document.Name;
				if (translate)
					name = Strings.Translate(name);
				return name;
			}
			// if we haven't got the activity, the name is often copied to documents using it, so check the current document
			if (Globals.Root.CurrentDocument != null && Globals.Root.CurrentDocument.ActivityID.Equals(activity))
			{
				string name = translate ? Globals.Root.CurrentDocument.DisplayName : Globals.Root.CurrentDocument.Name;
				if (!string.IsNullOrEmpty(name))
				{
					if (translate)
						return Strings.Translate(name);
					return name;
				}
			}
			Utilities.LogSubError("GetActivityText: cannot locate name for " + activity + "\'");
			return "?";
		}

		/// <summary>Returns config DOCUMENT for current activity; or Nothing if no activity selected</summary>
		public static Document GetActivitySettings()
		{
			if (Globals.Root.CurrentDocument == null)
				return null; // no activity is loaded
			return GetActivitySettings(Globals.Root.CurrentDocument.ActivityID);
		}

		public static Document GetActivitySettings(Guid ID)
		{
			if (ID.Equals(Guid.Empty))
				return null; // this is the null activity
			if (Config.ActivityExists(ID))
				return Config.ActivityConfig(ID);
			if (AllActivitiesLoaded)
				return null; // cannot find it
			Config config = Globals.Root.LoadConfig(ID + Config.Extension, Config.Levels.ActivityUser);
			if (config.Document == null)
				MessageBox.Show("No document");
			if (config.AutoCreated)
			{
				Utilities.LogSubError("Auto created activity config: " + ID);
				config.Document.GetCreateBothSettings(new Transaction()); // both versions are required
				config.Document.ID = ID; // make sure it has the requested ID, not some random one!
			}
			else if (!config.Document.ID.Equals(ID))
			{
				Utilities.LogSubError("Activity config loaded from file " + ID + " had ID " + config.Document.ID + " - changing to match the requested ID");
				config.Document.ID = ID;
			}
			if (config.Document.BothSettings == null)
				config.Document.BothSettings = new Config();
			Config.AddActivity(config.Document);
			return config.Document;
		}

		internal static string ActivityConfigFolder => Globals.Root.ConfigFolder + System.IO.Path.DirectorySeparatorChar + "Activities";

		public static Guid ActivityID => Globals.Root.CurrentDocument == null ? Guid.Empty : Globals.Root.CurrentDocument.ActivityID;

		public static void EnsureAllActivitiesLoaded()
		{
			// Checks that all of the available activities have been loaded; usually they are only loaded when access
			if (AllActivitiesLoaded)
				return;
			foreach (string folder in new[] { ActivityConfigFolder, Globals.Root.InternalFolder + System.IO.Path.DirectorySeparatorChar + "Activities" })
			{
				foreach (string file in System.IO.Directory.GetFiles(folder, "*"+ Config.Extension))
				{
					var use = file;
					try
					{
						use = System.IO.Path.GetFileNameWithoutExtension(use);
						Guid ID = new Guid(use);
						if (!Config.ActivityExists(ID))
							GetActivitySettings(ID); // calls Config.AddActivity
					}
					catch (Exception ex) // mainly in case a filename is not a valid Guid (e.g. if the user manually fiddles with the contents of the folder)
					{
						Utilities.LogSubError("Failed EnsureAllActivitiesLoaded on file " + use + ": " + ex.Message);
					}
				}
			}
			AllActivitiesLoaded = true;
		}

		/// <summary>Simple check for graphics mode, which at the moment is a yes/no (graphics or normal).  Might need to be removed if further options are added</summary>
		public static bool IsGraphicsMode => ActivityID.Equals(Graphics);

	}

}
