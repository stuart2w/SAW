using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace SAW
{
	/// <summary>Support for loading translations, and a few other bits</summary>
	public static class Strings
	{
		private static readonly Dictionary<string, string> g_Strings = new Dictionary<string, string>();
		private static readonly Regex g_regInclude = new Regex("\\%\\=(?<num>[^%]+)\\%", RegexOptions.Compiled | RegexOptions.Singleline);
		/// <summary>defined by Load (stored here for use in SaveFile)</summary>
		private static string g_Folder;

		public static void Load()
		{
			Load(false, false);
			// And one or two items are added, which are derived from Windows
			// adding them like this allows data items which can be translated to access them
			g_Strings.Add("Local_Currency", System.Globalization.CultureInfo.CurrentCulture.NumberFormat.CurrencySymbol);
			// Currency_SubUnit also appears in the actual file
			g_Strings.Add("Local_Decimal", System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
			g_Strings.Add("Local_ThousandsSeparator", System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberGroupSeparator);
		}

		private static void Load(bool omitEnglish, bool ignoreIncludes)
		{
			// bolOmitEnglish is just used by SaveList, below
			g_Strings.Clear();
			g_Folder = Globals.Root.InternalFolder;
			string culture = System.Globalization.CultureInfo.CurrentUICulture.Name;
			// folder bodges for my machines... ' 
#if DEBUG
			if (System.IO.Directory.Exists("d:\\data\\Siberix") && !g_Folder.ToLower().Contains("program"))
			{
				g_Folder = System.Reflection.Assembly.GetExecutingAssembly().Location.Replace("\\x86", "").Replace("\\bin", ""); // will work if x86 present or not
																																 // still need to remove filename and go up outside bin folder
				g_Folder = System.IO.Path.GetDirectoryName(System.IO.Path.GetDirectoryName(g_Folder));
			}
			//culture = "sv-xx"
#endif
			if (culture.ToLower() == "da-dk" || culture.ToLower() == "no" || culture.StartsWith("nb-"))
			{
				Globals.Root.Log.WriteLine("locale (" + culture + ") will be replaced by sv-SE");
				culture = "sv-SE";
			}
			Server.Language2 = culture.Substring(0, 2);

			List<string> suffixes = new List<string> {""};

			foreach (string suffix in suffixes)
			{
				if (!omitEnglish)
					LoadFile(g_Folder + System.IO.Path.DirectorySeparatorChar + "strings" + suffix + ".txt");
				LoadFile(g_Folder + System.IO.Path.DirectorySeparatorChar + "strings." + culture.Substring(0, 2) + suffix + ".txt");
				LoadFile(g_Folder + System.IO.Path.DirectorySeparatorChar + "strings." + culture + suffix + ".txt");
			}

			// we cannot iterate through m_Strings directly, because assigning breaks the iteration (even if the item already exists) ((AAARGH!))
			// therefore make a list of the keys which need changing
			if (!ignoreIncludes)
			{
				List<string> substitutions = new List<string>();
				foreach (string key in g_Strings.Keys)
				{
					if (g_Strings[key].Contains("%="))
						substitutions.Add(key);
				}
				foreach (string key in substitutions)
				{
					g_Strings[key] = ProcessIncludes(g_Strings[key]);
				}
			}
		}

		private static string ProcessIncludes(string str)
		{
			MatchCollection matches = g_regInclude.Matches(str);
			if (matches.Count == 0)
				Utilities.LogSubError("Invalid %= in: " + str);
			foreach (Match match in matches)
			{
				System.Text.RegularExpressions.Group g = match.Groups["num"];
				if (g.Success)
				{
					foreach (Capture objCapture in g.Captures) //objMatch.Captures
					{
						string ID = objCapture.Value;
						if (!g_Strings.ContainsKey(ID))
						{
							Utilities.LogSubError("Invalid reference: %=" + ID + "% in " + str);
							if (!IsTranslationMode)
								str = str.Replace("%=" + ID + "%", "???");
							// else just leaves the %=xxx% in
						}
						else
						{
							if (IsTranslationMode)
							{
								string sub = g_Strings[ID];
								sub = sub.Replace("[", "{");
								sub = sub.Replace("]", "}").Replace("&", "");
								str = str.Replace("%=" + ID + "%", sub);
							}
							else
							{
								string insert = g_Strings[ID];
								if (insert.EndsWith("..."))
									insert = insert.Substring(0, insert.Length - 3);
								insert = insert.Replace("&", ""); // remove accelerator keys.  When eg a menu name is referenced in explanatory text the & would probably be displayed - explanations, prompts etc. often don't support accelerators
								str = str.Replace("%=" + ID + "%", insert);
							}
						}
					}
				}
			}
			return str;
		}

		private static Regex g_regexLine = new Regex("^\\s*(?<id>[a-zA-Z0-9_\']+)\\s*=\\s*(?<text>.+)$", RegexOptions.Singleline | RegexOptions.ExplicitCapture | RegexOptions.Compiled);
		private static void LoadFile(string file)
		{
			if (!System.IO.File.Exists(file))
			{
				Debug.WriteLine("Strings file does not exist: " + System.IO.Path.GetFileName(file));
				return;
			}
			Debug.WriteLine("Loading strings: " + System.IO.Path.GetFileName(file));
			System.IO.StreamReader input = new System.IO.StreamReader(file, System.Text.Encoding.Default);
			string line = input.ReadLine();
			while (line != null)
			{
				if (line != "" && !line.StartsWith("#"))
				{
					Match match = g_regexLine.Match(line);
					if (!match.Success)
						Debug.WriteLine("Could not interpret line: " + line);
					else
					{
						string ID = match.Groups[1].Captures[0].Value; // grp 0 is entire string
						string strText = match.Groups[2].Captures[0].Value;
						if (IsTranslationMode)
							strText += " [" + ID + "]";
						g_Strings[ID] = strText;
					}
				}
				line = input.ReadLine();
			}
			input.Close();
		}

		public static string Item(string ID, bool removeAmpersand = false)
		{
			Debug.Assert(!ID.StartsWith("["));
			if (g_Strings == null)
				return "[" + ID + "]"; // possibly at design time
			else if (!g_Strings.ContainsKey(ID))
			{
				Debug.WriteLine("Text item does not exist: " + ID);
				return "[" + ID + "]";
			}
			else if (removeAmpersand)
				return g_Strings[ID].Replace("&", "");
			else
				return g_Strings[ID];
		}

		/// <summary>As Item() but performs .Replace("%0", replacement0) on the translation.  Just a convenience that can make the calling code more readable</summary>
		public static string Item(string ID, string replacement0)
		{
			return Item(ID).Replace("%0", replacement0);
		}


		public static bool Exists(string ID)
		{
			if (g_Strings == null)
				return false;
			return g_Strings.ContainsKey(ID);
		}

		/// <summary>If the text is enclosed in [] then the contents is looked up in the translations table;  if not the input string is returned.
		/// This allows for easy support of text that MIGHT need translating</summary>
		public static string Translate(string text)
		{
			// if the provided text is of the form "[message-id]" then this returns the text, otherwise it returns the original text
			// it can be used when it is not clear whether the text is already literal text or something which needs translating
			// i.e. this permits putting text or resource IDs in the GUI during development
			if (string.IsNullOrWhiteSpace(text))
				return "";
			if (text.StartsWith("[") && text.EndsWith("]") && text.Length >= 3)
			{
				string ID = text.Substring(1, text.Length - 2);
				return Item(ID); // if the message does not exist it will in fact return the original text
			}
			return text;
		}

		/// <summary>Translates the given control and all children.  Usually just call on each form or user control after the InitialiseComponent</summary>
		public static void Translate(Control control, ToolTip tt = null)
		{
			if (control == null)
				return;
			control.SuspendLayout();
			control.Text = Translate(control.Text);
			if (control is LinkLabel)
				AutoLink((LinkLabel)control);
			if (control is Form && control.Text.Contains("&"))
				control.Text = control.Text.Replace("&", ""); // in case of accelerator keys, which don't get processed in form titles
			if (tt != null)
			{
				string strText = tt.GetToolTip(control);
				if (strText.StartsWith("["))
					tt.SetToolTip(control, Translate(strText));
			}
			foreach (Control sub in control.Controls)
			{
				Translate(sub, tt);
			}
			if (control is ComboBox)
			{
				ComboBox cmb = (ComboBox)control;
				for (int i = 0; i < cmb.Items.Count; i++)
					if (cmb.Items[i] is string)
						cmb.Items[i] = Translate(cmb.Items[i].ToString());

			}
			if (control is ToolStrip)
				foreach (ToolStripItem objSub in ((ToolStrip)control).Items)
				{
					Translate(objSub);
				}
			else if (control is ListView)
				foreach (ColumnHeader column in ((ListView)control).Columns)
				{
					column.Text = Translate(column.Text);
				}
			control.ResumeLayout();
		}

		/// <summary>Converts any text within a LinkLabel enclosed in (( )) into a link</summary>
		public static void AutoLink(LinkLabel lnk)
		{
			int start = lnk.Text.IndexOf("((");
			int end = lnk.Text.IndexOf("))");
			if (start >= 0 && end > start)
			{
				lnk.Text = lnk.Text.Replace("((", "").Replace("))", "");
				lnk.LinkArea = new LinkArea(start, end - 2 - start); // the -2 is because of the (( which has now been removed
			}
		}

		public static void Translate(ToolStripItem item)
		{
			item.Text = Translate(item.Text);
			if (!(item is ToolStripMenuItem)) return;
			foreach (ToolStripItem objSub in ((ToolStripMenuItem)item).DropDownItems)
			{
				Translate(objSub);
			}
		}

		public static void Translate(Menu item)
		{
			if (item is MenuItem)
				((MenuItem)item).Text = Translate(((MenuItem)item).Text);
			foreach (Menu sub in item.MenuItems)
			{
				Translate(sub);
			}
		}

		#region Dev support
		/// <summary>True if StartTranslationMode has been called: all text lookups/translations will append the code used</summary>
		public static bool IsTranslationMode { get; private set; }

		/// <summary>For development: all translations in future will append the code used in []</summary>
		public static void StartTranslationMode()
		{
			IsTranslationMode = true;
			Load();
		}

		/// <summary>As Translate but ignores translate mode.  To be used sparingly (used for ctrl- in keys otherwise menus become unreadable)</summary>
		[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
		public static string ItemNoTranslateMode(string ID)
		{
			string text = Item(ID);
			int index = text.LastIndexOf("[");
			if (index > 0)
				text = text.Substring(index);
			return text;
		}

		#endregion

		public static void SaveList(string file, bool onlyMissing)
		{
			// we will load the text again, omitting the English to give us in memory a list of what is in the translated files
			// and then process the file again
			Load(true, true);
			try
			{
				System.IO.StreamReader input = new System.IO.StreamReader(g_Folder + "\\strings.txt", System.Text.Encoding.Default);
				System.IO.StreamWriter output = new System.IO.StreamWriter(file, false, System.Text.Encoding.Default);
				string line = input.ReadLine();
				while (line != null)
				{
					if (line != "" && !line.StartsWith("#"))
					{
						Match match = g_regexLine.Match(line);
						if (!match.Success)
							output.WriteLine("!! Could not interpret line: " + line);
						else
						{
							string ID = match.Groups[1].Captures[0].Value;
							// grp 0 is entire string
							//Dim strText As String = objMatch.Groups(2).Captures(0).Value
							if (g_Strings.ContainsKey(ID))
							{
								if (!onlyMissing)
									output.WriteLine(ID + " = " + g_Strings[ID]);
							}
							else // if missing the entire English line is written always, preceeded by >>
							{
								if (!onlyMissing)
									output.Write(">> ");
								output.WriteLine(line);
							}
						}
					}
					else
					{
						// just write blanks and comments into the output directly
						if (!onlyMissing)
							output.WriteLine(line);
					}
					line = input.ReadLine();
				}
				input.Close();
				output.Close();
			}
			finally
			{
				Load(false, false);
			}
		}

		/// <summary>All the loaded codes from the translations file</summary>
		public static Dictionary<string, string>.KeyCollection Keys
		{ get { return g_Strings.Keys; } }

		private static HashSet<string> m_Once = new HashSet<string>();
		/// <summary>can be used to track messages which should only be displayed once.  This function returns if the message should be displayed
		/// it will then return false in future for the same ID</summary>
		public static bool OnceMessage(string ID)
		{
			Debug.Assert(!ID.StartsWith("[") && !ID.Contains(" "), "Strings.OnceMessage: parameter should be ID not translatable message or message");
			if (m_Once.Contains(ID))
				return false;
			m_Once.Add(ID);
			return true;
		}
	}
}
