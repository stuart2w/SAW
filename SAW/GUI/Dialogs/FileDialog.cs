using System;
using System.Diagnostics;
using System.Windows.Forms;
using System.Linq;


namespace SAW
{
	internal class FileDialog
	{
		// Handles global support for the standard Windows save and open dialogs
		// the main reason for this class is to support storing different current folders for documents and images (and various other things)
		// For each context the current load and save folder is remembered in the system configuration
		// values are read from AM.CurrentConfig which gives the possibility of overriding locations in user config later with minimal changes to this class

		public enum Context
		{
			Document = 0, // these should not be renumbered as they are effectively stored in the configuration; although it is not terminal if stored locations are lost
			Image,
			UserExport,
			AvatarImage,
			/// <summary>non-splash document to be used as a background.  Expects images, PDF and DOCX</summary>
			BackgroundDocument,
			Sound,
			/// <summary>Any other user doc.  Filter must be specified</summary>
			OtherUserDoc,

			// None of the rest are used by standard users
			ErrorReport = 10, // viewing only
			Translation,
			OtherTechnical // any other rare stuff; remembered folder will probably be inappropriate, but that's OK
		}

		private static OpenFileDialog g_dlgOpen;
		/// <summary>returns "" if cancelled</summary>
		public static string ShowOpen(Context context, string filter = "", string overrideFolder = "")
		{
			// If this is on Windows it will automatically use the version with the preview panel if the context is document
			if (string.IsNullOrEmpty(filter ))
				filter = GetDefaultFilter(context);
			string key = OpenConfigKey(context);
			string existing = Globals.Root.CurrentConfig.ReadString(key);
			if (string.IsNullOrEmpty(existing ))
				existing = Globals.Root.CurrentConfig.ReadString(SaveConfigKey(context)); // if the user has not opened, use any save folder
			if (!System.IO.Directory.Exists(existing))
				existing = "";
			if (!string.IsNullOrEmpty(overrideFolder))
				existing = overrideFolder;
			Debug.Assert(context != Context.Document || string.IsNullOrEmpty(filter) || filter == Document.LoadFilter(),
				"Specifying a filter with Document context in FileDialog.ShowOpen will fail as it uses the preview dialog which assumes the document filter");
			if (context == Context.Document)
				return ShowOpenWithDocumentPreview(filter, key);
			if (g_dlgOpen == null)
				g_dlgOpen = new OpenFileDialog();
			else
				g_dlgOpen.FileName = "";
			if (!string.IsNullOrEmpty(existing ))
				g_dlgOpen.InitialDirectory = existing;
			g_dlgOpen.Filter = Strings.Translate(filter);
			if (g_dlgOpen.ShowDialog() != DialogResult.OK)
				return null;
			Config.SystemConfig.Write(key, System.IO.Path.GetDirectoryName(g_dlgOpen.FileName));
			return g_dlgOpen.FileName;
		}

		private static string ShowOpenWithDocumentPreview(string filter, string key)
		{
			FilePreviewPanel preview = new FilePreviewPanel {Width = 50};
			// I don't think we can set the initial directory of these
			ExtensibleDialogs.OpenFileDialog extendedDialog = new ExtensibleDialogs.OpenFileDialog("splash", Document.LoadPattern(), filter, preview);
			extendedDialog.SelectionChanged += preview.FileSelectionChanged;
			if (extendedDialog.Show(Globals.Root.CurrentMainScreen()) != true)
				return null;
			Config.SystemConfig.Write(key, System.IO.Path.GetDirectoryName(extendedDialog.SelectedPath));
			return extendedDialog.SelectedPath;
		}

		private static SaveFileDialog g_dlgSave;
		/// <summary>returns null if cancelled.  filename should be existing name for file, but only within folder</summary>
		public static string ShowSave(Context context, string filter = "", string filename = "") 
		{
			Debug.Assert(!filename.Contains(System.IO.Path.DirectorySeparatorChar));
			Debug.Assert(context != Context.Image || !string.IsNullOrEmpty(filter ), "FileDialog.ShowSave: filter should be specified when saving images");
			if (string.IsNullOrEmpty(filter ))
				filter = GetDefaultFilter(context);
			if (g_dlgSave == null)
				g_dlgSave = new SaveFileDialog();
			string key = SaveConfigKey(context);
			string existing = Globals.Root.CurrentConfig.ReadString(key);
			if (string.IsNullOrEmpty(existing ))
				existing = Globals.Root.CurrentConfig.ReadString(OpenConfigKey(context)); // if the user has not manually saved yet, then use any open folder (probably where this document came from)
			if (!string.IsNullOrEmpty(existing ))
				g_dlgSave.InitialDirectory = existing;
			g_dlgSave.Filter = Strings.Translate(filter);
			g_dlgSave.FileName = filename;
#if DEBUG
			g_dlgSave.OverwritePrompt = false;
#endif
			if (g_dlgSave.ShowDialog() != DialogResult.OK)
				return null;
			Config.SystemConfig.Write(key, System.IO.Path.GetDirectoryName(g_dlgSave.FileName));
			return g_dlgSave.FileName;
		}

		public static string OpenConfigKey(Context context) => "Open_Folder_" + Convert.ToInt32(context);
		public static string SaveConfigKey(Context context) => "Save_Folder_" + Convert.ToInt32(context);

		private static string GetDefaultFilter(Context context)
		{
			// used at the beginning of ShowOpen and ShowSave; throws an ArgumentException if there is no default
			switch (context)
			{
				case Context.Document:
					return Document.LoadFilter();
				case Context.Image:
				case Context.AvatarImage:
					return Strings.Item("Filter_Image2"); // There is an alternative which would need to be provided
				case Context.ErrorReport:
					return "*.error|*.error";
				case Context.Translation:
					return "*.txt|*.txt";
				case Context.BackgroundDocument:
					return Strings.Item("Filter_BackDocument");
				case Context.Sound:
					return Strings.Item("Filter_Sound");
				default:
					throw new ArgumentException("FileDialog: filter must be provided for context = " + context);
			}
		}

	}

}
