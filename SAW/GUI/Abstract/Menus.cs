using System;
using System.Windows.Forms;

namespace SAW
{
	public static class Menus
	{
		// Support for creating the Menus which can be shared between the user menu screen and the main screen

		#region Help
		public static ToolStripMenuItem CreateHelpMenu()
		{
			ToolStripMenuItem mnuHeading = new ToolStripMenuItem(Strings.Item("Menu_Help"));
			var imgPDF = Resources.AM.pdf_icon_16;
			var mnuManuals = new ToolStripMenuItem(Strings.Item("Menu_Manual"), imgPDF) { Visible = false };
			mnuManuals.Tag = "Manual";
			mnuManuals.Click += mnuManual_Click;
			mnuHeading.DropDownItems.Add(mnuManuals);

			//mnuHeading.DropDownItems.Add(new ToolStripMenuItem(Strings.Item("Menu_OtherHelpOnWeb"), Resources.AM.internet_16, (s, e) => Globals.Root.OpenURL(Strings.Item("Menu_OtherHelpOnWebURL"), Globals.Root.CurrentMainScreen())));
			mnuHeading.DropDownItems.Add(new ToolStripSeparator());
			mnuHeading.DropDownItems.Add(new ToolStripMenuItem(Strings.Item("Menu_Update"), null, mnuUpdate_Click));
			mnuHeading.DropDownItems.Add(new ToolStripMenuItem(Strings.Item("Menu_ReportProblem"), null, (s, e) => frmReportProblem.Display()));
			//mnuHeading.DropDownItems.Add(new ToolStripMenuItem(Strings.Item("Menu_About"), null, (s, e) => frmAbout.Display()));
			mnuHeading.Visible = false;
			return mnuHeading;
		}

		internal static void mnuUpdate_Click(object sender, EventArgs e)
		{ // actually currently linked to a menu on the main form
			if (Globals.DoRequestClose())
			{
				for (int index = 0; index <= Globals.Root.DocumentsCount - 1; index++)
				{
					if (Globals.Root.Documents(index) != null) // It is permitted to have a Nothing entry, in particular if it is the only entry
						Globals.Root.Documents(index).Changed = false; // shouldn't really be needed, but I did manage one update failure on laptop with a "Save?" box still open, stopping it from closing
				}
				Globals.Root.StartUpgrade();
			}
		}


		//	Private Declare Function ShellExecute Lib "shell32.dll" Alias "ShellExecuteA" (ByVal hwnd As Integer, ByVal intIgnoreVerb As Integer, ByVal lpFile As String, ByVal lpParameters As String, ByVal lpDirectory As String, ByVal nShowCmd As Integer) As Integer
		private static void mnuManual_Click(object sender, EventArgs e)
		{
			string file = ((ToolStripMenuItem)sender).Tag + ".pdf"; // this excludes any folder or language stuff
			string folder = Globals.Root.ManualFolder;
			string actual = folder + System.IO.Path.DirectorySeparatorChar + System.IO.Path.GetFileNameWithoutExtension(file) + "." + Server.Language2 + ".pdf";
			if (!System.IO.File.Exists(actual))
			{
				actual = folder + System.IO.Path.DirectorySeparatorChar + file;
				if (!System.IO.File.Exists(actual))
				{
					MessageBox.Show("Cannot find file: " + actual); // semi-deliberately not translated
					return;
				}
			}
			Globals.Root.ShellOpen(actual);
			//ShellExecute(0, 0, strActual, Nothing, Nothing, 0)
		}

		#endregion

		#region Support
		// the support menu heading must be created in the main screen, but this adds many of the entries
		// (Because a few items in frmMain cannot easily be replicated in here)
		// this menu is not visible to users, and only available in DEBUG builds

		public static void PopulateSupportMenu(ToolStripMenuItem mnuSupport)
		{
			// Created in reverse order so that we can keep inserting at position 0
			mnuSupport.DropDownItems.Insert(0, new ToolStripMenuItem("Save key shortcut table", null, mnuKeyTable_Click));
			ToolStripMenuItem mnuTranslations = new ToolStripMenuItem("Translations"); // Populated below...
			mnuSupport.DropDownItems.Insert(0, mnuTranslations);
			mnuSupport.DropDownItems.Insert(0, new ToolStripMenuItem("Open data folder", null, (s, e) => Globals.Root.ShellOpen(Globals.Root.SharedFolder)));

			mnuTranslations.DropDownItems.Add("Show IDs", null, mnuTranslationMode_Click);
			mnuTranslations.DropDownItems.Add("Save current text", null, mnuSaveText_Click);
			mnuTranslations.DropDownItems.Add("Save untranslated text", null, mnuSaveUntranslatedText_Click);

			// And anything added to the end
#if DEBUG
			mnuSupport.DropDownItems.Add("Generate config deltas", null, (s, e) => frmDeltas.Display()); // frmDeltas is only really compiled in debug mode
#endif
		}

		private static void mnuTranslationMode_Click(object sender, EventArgs e)
		{
			Strings.StartTranslationMode();
			MessageBox.Show("The ID will now be displayed after each item of text.  Note that some things will not fit very well on screen like this!  To revert to normal it is necessary to close and reopen the software.");
			//mnuTranslationHeader.Enabled = False
			if (Globals.Root.CurrentDocument != null)
				Globals.Root.ReopenMainScreen(Globals.Root.CurrentDocument);
		}

		private static void mnuSaveText_Click(object sender, EventArgs e)
		{
			if (MessageBox.Show("This saves a copy of the strings.txt file with the English replaced by the current translated text.  Where the text is not translated the line is indicated with \'>>\' before the ID.", RootApplication.AppName, MessageBoxButtons.OKCancel) != DialogResult.OK)
				return;
			string filename = FileDialog.ShowSave(FileDialog.Context.Translation);
			if (!string.IsNullOrEmpty(filename))
				Strings.SaveList(filename, false);
		}

		private static void mnuSaveUntranslatedText_Click(object sender, EventArgs e)
		{
			if (MessageBox.Show("This saves a file containing any items in strings.txt which do not appear in the local language file(s)", RootApplication.AppName, MessageBoxButtons.OKCancel) != DialogResult.OK)
				return;
			string filename = FileDialog.ShowSave(FileDialog.Context.Translation);
			if (string.IsNullOrEmpty(filename))
				return;
			Strings.SaveList(filename, true);
		}

		private static void mnuKeyTable_Click(object sender, EventArgs e)
		{
			// Generate the list of key shortcuts in HTML
			string filename = FileDialog.ShowSave(FileDialog.Context.OtherTechnical, "*.html|*.html");
			if (string.IsNullOrEmpty(filename))
				return;
			System.Text.StringBuilder output = new System.Text.StringBuilder();
			output.AppendLine("<HTML>").AppendLine("<HEAD></HEAD><BODY><TABLE>");
			output.AppendLine("<tr><th>Key</th><th>-</th><th>+Shift</th><th>+Control</th><th>+Shift+Control</th></tr>");
			Keys previousKey = Keys.None; // some keys seem to get reported more than once; filter this out
			foreach (Keys key in Enum.GetValues(typeof(Keys)))
			{
				// hopefully they will come out in some sort of sensible order
				if (key != previousKey)
				{
					string line = GenerateKeyLine(key);
					if (!string.IsNullOrEmpty(line))
						output.Append(line);
					line = GenerateKeyLine(key | Keys.Alt);
					if (!string.IsNullOrEmpty(line))
						output.Append(line);
				}
				previousKey = key;
			}
			output.AppendLine("</TABLE></BODY></HTML>");
			System.IO.StreamWriter fs = new System.IO.StreamWriter(filename, false);
			fs.Write(output.ToString());
			fs.Close();
			// I wanted to put it on the clipboard, but it turns out that HTML on the clipboard is complicated (and largely undocumented)
			MessageBox.Show("Saved");
		}

		private static string GenerateKeyLine(Keys key)
		{
			// generate one line for the above table.  This includes all the variations of shift and control
			// the Alt key must be put on a separate line
			System.Text.StringBuilder output = new System.Text.StringBuilder();
			bool defined = false; // true if any keys were defined
			output.Append("<tr><td>").Append(GUIUtilities.KeyDescription(key).Replace("<", "")).Append("</td>");
			foreach (Keys modifier in new[] { Keys.None, Keys.Shift, Keys.Control, Keys.Shift | Keys.Control })
			{
				Functions.Action action = Globals.Root.CurrentConfig.KeyAction(key | modifier);
				output.Append("<td>");
				if (!action.IsEmpty)
				{
					output.Append(action.DescriptionWithoutAccelerator());
					defined = true;
				}
				output.Append("</td>");
			}
			if (!defined)
				return "";
			output.AppendLine("</tr>");
			return output.ToString();
		}

		#endregion

	}
}
