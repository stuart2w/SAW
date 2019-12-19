using System;
using System.Collections.Generic;
using System.Diagnostics;
using RepoB;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Update
{
	sealed class Bootstrap
	{

		public static string Version = "";
		public const string AppName = "SAW7"; // must match main project RootApplication

		[STAThread]
		public static void Main(string[] cmdArgs)
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			try
			{
				Strings.Load(); // <--- leave breakpoint here in case of starting wrong project by accident!
				Globals.Root.Log("Initialised bootstrap");

				string parameter = "";
				if (cmdArgs != null && (cmdArgs.Length - 1) >= 0)
					parameter = cmdArgs[0]; // version number
#if DEBUG
				parameter = "0.15.0";
#endif
				// can be followed by /Auto (checked in frmUpdate)
				if (parameter == "")
				{
					MessageBox.Show(Strings.Item("Install_NoDirect"));
					// the application must have been run directly by the user
					return;
				}
				Globals.Root.Log("Parameter = " + parameter);

				Version = parameter;
				string versionFile = Globals.Root.UpdateFolder + "\\" + Version + ".repo";
				if (!File.Exists(versionFile))
				{
					MessageBox.Show("Cannot locate index file: " + versionFile);
					return;
				}
				Globals.Root.Log("Version file exists");
				RepoVersion version = RepoList.LoadVersionFile(versionFile);
				if (version.Signature != "" && !string.Equals(version.Signature, version.CalculateSignature(), StringComparison.CurrentCultureIgnoreCase))
				{
					MessageBox.Show("Update index cannot be used, it has an invalid signature.   The index that has been downloaded is not a true copy of the index.");
					return;
				}
				version.SetLocalPath(Globals.Root.InternalFolder + "\\");
				version.SetLocalPath("%shared%", Globals.Root.SharedFolder + "\\");
				Globals.Root.Log("Opening form");
				frmUpdate form = new frmUpdate(version);
				Application.Run(form);
			}
			catch (Exception ex) when (!Globals.Root.IsDebug)
			{
				MessageBox.Show(ex.ToString());
			}

		}

	}

	#region Dummy Application object
	internal class RootApplication
	{

		public string InternalFolder; // written to - must be program files
		public string SharedFolder; // written to - must be actual shared app data
		public string EXEFolder;
		public string UpdateFolder; // where update files are stored - IN DEBUG WILL BE IN DEVELOPMENT CODE
		public readonly bool IsDebug;

		public RootApplication()
		{
			// folders...
			EXEFolder = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location); // default for InternalFolder, but also used below
			SharedFolder = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\" + Bootstrap.AppName;
			InternalFolder = EXEFolder;
			UpdateFolder = SharedFolder + "\\Update";
#if DEBUG
			UpdateFolder = @"D:\Data\ACE\SAW\Test data\update";
			IsDebug = true;
#else
			IsDebug = false;
#endif
		}

		public void Log(params string[] str)
		{

		}
		public void PermanentLog(params string[] str)
		{

		}

	}
	internal sealed class Globals
	{
		public static RootApplication Root = new RootApplication();

		internal static void LogSubError(Exception ex)
		{
			Debug.Fail(ex.Message);
		}

		internal static void LogSubError(string message)
		{
			Debug.Fail(message);
		}

	}
	#endregion

	#region Strings - copied from main project (well, it's an old copy now)

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
			if ((Directory.Exists("f:\\LPUK") || Directory.Exists("d:\\data\\Siberix")) && !g_Folder.ToLower().Contains("program"))
			{
				g_Folder = System.Reflection.Assembly.GetExecutingAssembly().Location.Replace("\\x86", "").Replace("\\bin", ""); // will work if x86 present or not
				// still need to remove filename and go up outside bin folder
				g_Folder = Path.GetDirectoryName(Path.GetDirectoryName(g_Folder));
			}
			//culture = "sv-xx"
#endif
			if (culture.ToLower() == "da-dk" || culture.ToLower() == "no" || culture.StartsWith("nb-"))
			{
				Globals.Root.Log("locale (" + culture + ") will be replaced by sv-SE");
				culture = "sv-SE";
			}

			List<string> suffixes = new List<string>();
			suffixes.Add("");
			suffixes.Add(".SAW");

			foreach (string suffix in suffixes)
			{
				if (!omitEnglish)
					LoadFile(g_Folder + Path.DirectorySeparatorChar + "strings" + suffix + ".txt");
				LoadFile(g_Folder + Path.DirectorySeparatorChar + "strings." + culture.Substring(0, 2) + suffix + ".txt");
				LoadFile(g_Folder + Path.DirectorySeparatorChar + "strings." + culture + suffix + ".txt");
			}

			// we cannot iterate through m_colStrings directly, because assigning breaks the iteration (even if the item already exists) ((AAARGH!))
			// therefore make a list of the keys which need changing
			if (!ignoreIncludes)
			{
				List<string> substitutions = new List<string>();
				foreach (string strKey in g_Strings.Keys)
				{
					if (g_Strings[strKey].Contains("%="))
						substitutions.Add(strKey);
				}
				foreach (string strKey in substitutions)
				{
					g_Strings[strKey] = ProcessIncludes(g_Strings[strKey]);
				}
			}
		}

		private static string ProcessIncludes(string str)
		{
			MatchCollection matches = g_regInclude.Matches(str);
			if (matches.Count == 0)
				Globals.LogSubError("Invalid %= in: " + str);
			foreach (Match match in matches)
			{
				Group g = match.Groups["num"];
				if (g.Success)
				{
					foreach (Capture objCapture in g.Captures) //objMatch.Captures
					{
						string ID = objCapture.Value;
						if (!g_Strings.ContainsKey(ID))
						{
							Globals.LogSubError("Invalid reference: %=" + ID + "% in " + str);
								str = str.Replace("%=" + ID + "%", "???");
							// else just leaves the %=xxx% in
						}
						else
						{
							string insert = g_Strings[ID];
							if (insert.EndsWith("..."))
								insert = insert.Substring(0, insert.Length - 3);
							insert = insert.Replace("&",
								""); // remove accelerator keys.  When eg a menu name is referenced in explanatory text the & would probably be displayed - explanations, prompts etc. often don't support accelerators
							str = str.Replace("%=" + ID + "%", insert);
						}
					}
				}
			}
			return str;
		}

		private static Regex g_regexLine = new Regex("^\\s*(?<id>[a-zA-Z0-9_\']+)\\s*=\\s*(?<text>.+)$", RegexOptions.Singleline | RegexOptions.ExplicitCapture | RegexOptions.Compiled);
		private static void LoadFile(string file)
		{
			if (!File.Exists(file))
			{
				Debug.WriteLine("Strings file does not exist: " + Path.GetFileName(file));
				return;
			}
			Debug.WriteLine("Loading strings: " + Path.GetFileName(file));
			StreamReader input = new StreamReader(file, System.Text.Encoding.Default);
			string line = input.ReadLine();
			while (line != null)
			{
				if (line != "" && !line.StartsWith("#"))
				{
					Match objMatch = g_regexLine.Match(line);
					if (!objMatch.Success)
						Debug.WriteLine("Could not interpret line: " + line);
					else
					{
						string strID = objMatch.Groups[1].Captures[0].Value; // grp 0 is entire string
						string strText = objMatch.Groups[2].Captures[0].Value;
						g_Strings[strID] = strText;
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
			foreach (Control objSub in control.Controls)
			{
				Translate(objSub, tt);
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

		/// <summary>All the loaded codes from the translations file</summary>
		public static Dictionary<string, string>.KeyCollection Keys
		{ get { return g_Strings.Keys; } }

	}
	#endregion

}