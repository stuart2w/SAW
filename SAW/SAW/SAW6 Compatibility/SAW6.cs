using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace SAW
{
	/// <summary>Compatability stuff for SAW6</summary>
	internal static class SAW6
	{
		//private static string g_DefaultRootFolder;

		//static SAW6()
		//{
		//	g_DefaultRootFolder = Globals.Root.InternalFolder;
		//	//if (Directory.Exists("C:\\Program Files\\ACE Centre\\SAW 6"))
		//	//	g_DefaultRootFolder = "C:\\Program Files\\ACE Centre\\SAW 6";
		//	//else
		//	//	g_DefaultRootFolder = "C:\\Program Files (x86)\\ACE Centre\\SAW 6";
		//}


		public static string[] GetDefaultGraphicFolders()
		{
			return new[] { Path.Combine(Globals.Root.InternalFolder, "SAW6Graphics") };
		}

		public static string[] GetDefaultSoundsFolders()
		{
			return new[] { Path.Combine(Globals.Root.InternalFolder, "SAW6sounds") };
		}

		/// <summary>Used when loading old file.  Locates the actual file given the name stored in the file, which can be a complete mess of local names, full paths or partial names within defaults</summary>
		public static string LocateResourceFile(string file, IEnumerable<string> defaultFolders, string selectionSetFolder)
		{
			if (string.IsNullOrEmpty(file))
				return null;
			// option 1 is it is a complete path
			if (file.Contains(':'))
			{
				if (File.Exists(file))
					return file;
				string defaultFile = defaultFolders.FirstOrDefault(x => file.StartsWith(x, StringComparison.InvariantCultureIgnoreCase));
				if (defaultFile != null)
					// if it's in the main SAW default, then SAW 6 reduced it to a partial path.  
					file = file.Substring(defaultFile.Length);
				else // any unknown fully qual file was reduced to just leaf and searched
					file = Path.GetFileName(file);
			}
			// first try partial path from selection set.  Not sure SAW 6 actually allowed this unless file was just a leaf name, but no harm trying either way
			if (File.Exists(Path.Combine(selectionSetFolder, file)))
				return Path.Combine(selectionSetFolder, file);
			if (file.Contains('\\'))
			{
				// partial path, must be in one of the defaults using the entire path
				// from the possible full paths return the first which actually exists
				return (from def in defaultFolders select Path.Combine(def, file)).FirstOrDefault(File.Exists);
			}
			// path is a leaf
			return (from def in defaultFolders select SearchFolder(file, def)).FirstOrDefault(x => x != null);
			// returns first file found by SearchFolder, or null if all returned null
		}

		private static string SearchFolder(string leaf, string folder)
		{
			var path = Path.Combine(folder, leaf);
			if (File.Exists(path))
				return path;
			// runs searchfolder on all subs and returns any non-null result found
			try
			{
				return (from sub in Directory.GetDirectories(folder) select SearchFolder(leaf, sub)).FirstOrDefault(x => x != null);
			}
			catch (Exception ex)
			{
				Globals.Root.Log.WriteLine($"Folder search in {folder} failed with {ex.Message}");
				return null;
			}
		}

		#region Default colours
		/// <summary>The default colours used in SAW6</summary>
		public static Color[] StandardColours =
		{
			Color.FromArgb(255, 0, 0, 0),
			Color.FromArgb(255, 165, 42, 0),
			Color.FromArgb(255, 0, 64, 64),
			Color.FromArgb(255, 0, 85, 0),
			Color.FromArgb(255, 0, 0, 94),
			Color.FromArgb(255, 0, 0, 139),
			Color.FromArgb(255, 75, 0, 130),
			Color.FromArgb(255, 40, 40, 40),
			Color.FromArgb(255, 139, 0, 0),
			Color.FromArgb(255, 255, 104, 32),
			Color.FromArgb(255, 139, 139, 0),
			Color.FromArgb(255, 0, 147, 0),
			Color.FromArgb(255, 56, 142, 142),
			Color.FromArgb(255, 0, 0, 255),
			Color.FromArgb(255, 123, 123, 192),
			Color.FromArgb(255, 102, 102, 102),
			Color.FromArgb(255, 255, 0, 0),
			Color.FromArgb(255, 255, 173, 91),
			Color.FromArgb(255, 50, 205, 50),
			Color.FromArgb(255, 60, 179, 113),
			Color.FromArgb(255, 127, 255, 212),
			Color.FromArgb(255, 125, 158, 192),
			Color.FromArgb(255, 128, 0, 128),
			Color.FromArgb(255, 127, 127, 127),
			Color.FromArgb(255, 255, 192, 203),
			Color.FromArgb(255, 255, 215, 0),
			Color.FromArgb(255, 255, 255, 0),
			Color.FromArgb(255, 0, 255, 0),
			Color.FromArgb(255, 64, 224, 208),
			Color.FromArgb(255, 192, 255, 255),
			Color.FromArgb(255, 72, 0, 72),
			Color.FromArgb(255, 192, 192, 192),
			Color.FromArgb(255, 255, 228, 225),
			Color.FromArgb(255, 210, 180, 140),
			Color.FromArgb(255, 255, 255, 224),
			Color.FromArgb(255, 152, 251, 152),
			Color.FromArgb(255, 175, 238, 238),
			Color.FromArgb(255, 104, 131, 139),
			Color.FromArgb(255, 230, 230, 250),
			Color.FromArgb(255, 255, 255, 255)
		};
		#endregion

	}
}
