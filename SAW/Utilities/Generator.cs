using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAW
{
	/// <summary>Contains some utilities for external code using SAW to generate selection sets. </summary>
	public static class Generator
	{
		/// <summary>Initialises SAW internal code for use as a library by external code.  Various items will fail if this is not called first. </summary>
		public static void InitialiseExternal(string exeFolder)
		{
			GUIUtilities.SystemDPI = 96; // usually taken from frmMenu - without any GUI available we just use the default.  It won't really be needed if just generating documents
			GUIUtilities.SystemDPIRelative = 1;
			Globals.IsEmbedded = true;
			if (!System.IO.File.Exists(System.IO.Path.Combine(exeFolder, "strings.txt")))
				throw new Exception("Invalid folder: strings.txt not found");
			Globals.Root = new RootApplication(null, exeFolder);
		}

		public static Document CreateNewDocument(Size sizeInPixels)
		{
			Document doc = new Document(false);
			doc.SetDefaultsForNewDocument();
			doc.Page(0).SetSize(sizeInPixels, 0);
			Globals.Root.CurrentDocument = doc;
			return doc;
		}

	}
}
