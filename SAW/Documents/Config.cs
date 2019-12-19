using System.Collections.Generic;
using System;
using System.Drawing;
using System.Diagnostics;
using System.Windows.Forms;
using System.Linq;


namespace SAW
{
	/// <summary>A complete set of settings.  Predominantly a dictionary of key/value pairs, but has some specific data.  Links to a document which can store a pair of these, one for run mode one for editing.
	/// Several of these can be applied at once, generating an AppliedConfig, although this isn't used so much in SAW. </summary>
	public partial class Config : Datum
	{
		// Stores the settings in a dictionary.  Settings can always be omitted.  Usually there is more than one of these
		// and they are chained together in priority order, so the current value is determined by the first Config in the list specifying that value
		// values are referenced by a text key (case sensitive).  All values are stored as strings, although helper functions
		// can access them as numbers etc

		/// <summary>The ID of the floating toolbox.  </summary>
		public static Guid ToolboxPaletteID = new Guid("{ca647552-fc3f-4c69-908c-5a4a4790e3c5}");

		/// <summary>Extension for files </summary>
		public const string Extension = ".sawcfg";

		internal Dictionary<string, string> Values = new Dictionary<string, string>();
		/// <summary>the document containing this configuration
		/// for system, user and activity settings a document with no pages is always generated (v2: 1 page added for page size etc to activities</summary>
		public Document Document;
		/// <summary>only in User</summary>
		public MemoryImage Picture;
		/// <summary>likewise only in User (but includes teacher).
		/// V2 was array[2].  Changed to extensible list in V3 to support SAW</summary>
		public List<ButtonStyle> ButtonStyle; // = new List<ButtonStyle>();
		public const int BUTTONSTYLES = 10; //leaves 0-1 unused as Splash;  2-9 are Item.StyleTypes.  5 (2+3) won't actually be used as that is the code for custom
											/// <summary>is flagged to true if AM.LoadConfig created this because it couldn't load the file
											/// this is not stored in data; but will prevent an activity being saved (otherwise the system will think it has the activity later, whereas the file would actually be junk )</summary>
		public bool AutoCreated = false;
		/// <summary>Sequence that the shapes should appear; each entry is either a shape number for an inbuilt one; or a Guid for a PaletteShape.  Both converted to strings</summary>
		/// <remarks>If usually Nothing which means no sequence specified here</remarks>
		public string[] ShapeSequence;

		#region enums

		public enum CompatibilityModes
		{
			Splash,
			SAW6
		}

		#endregion

		#region String constants for config keys

		// also Timing_XXX are done thru Switching.Engine.TimingConfigField(t)

		/// <summary>0 for mouse;  default 1. </summary>
		public const string Number_Switches = "Number_Switches";
		/// <summary>The integer value of the engine ID</summary>
		public const string Switch_Engine = "Switch_Engine";
		/// <summary>Type of switch.  Append number which is 0 or 1.  (NOT 1 or 2)</summary>
		public const string Switch_Type_ = "Switch_Type_";
		public const string Switch_Param_ = "Switch_Param_";

		public const string Reverse_Switches = "Reverse_Switches";
		public const string Use_Swap_Switch = "Use_Swap_Switch";
		public const string SAW_Prompts = "SAW_Help_Prompts";
		public const string Sound_Click = "Sound_Click";
		public const string Repeat_PressStop = "Repeat_PressStop";

		public const string Output_Send = "Output_Send";
		public const string Output_Speech = "Output_Speech";
		public const string Output_SpeechVisit = "Output_SpeechVisit";
		public const string Output_Sound = "Output_Sound";
		public const string Hide_Title = "Hide_Title";
		public const string HideTime_OnSend = "HideTime_OnSend";

		public const string Mouse_Continuous = "Mouse_Continuous";
		public const string Wait_Multipler = "Wait_Multiplier";
		public const int Wait_Multipler_Default = 100;

		public static SizeF Page_Size_Default = new SizeF(750, 500);


		// Also: FileDialog generates its own keys
		internal const string Low_Graphics = "Low_Graphics";
		internal const string Show_PageList = "Show_PageList";
		internal const string Show_InfoMeasurement = "Show_InfoMeasurement";
		internal const string Show_Prompts = "Show_Prompts";
		internal const string Show_Tools = "Show_Tools";
		internal const string Show_Toolbar = "Show_Toolbar";
		internal const string Show_ToolbarSnap = "Show_ToolbarSnap";
		internal const string Show_ToolbarVerbs = "Show_ToolbarVerbs";
		internal const string SelectorAfterCreate = "SelectorAfterCreate";
		internal const string Tooltips = "Tooltips";
		/// <summary>floatingpoint, default = 1</summary>
		internal const string Radius_Step = "Radius_Step";
		internal const string Moving_Shadow = "Moving_Shadow";
		internal const string Advanced_Colours = "Advanced_Colours";
		internal const string Key_Shortcuts = "KeyShortcuts"; // _ is not permitted, names cannot start "Key_"
		internal const string Use_Speech = "Use_Speech";
		internal const string Speech_Voice = "Speech_Voice";
		internal const string Speech_Volume = "Speech_Volume";
		internal const string Speech_Speed = "Speech_Speed";
		//internal const string Speech_Pitch = "Speech_Pitch";
		internal const string Default_Zoom = "Default_Zoom"; // stored as an integer which is 100 times the actual zoom (because the config ComboBox uses integers)
		internal const string Multiple_Documents = "Multiple_Documents"; // only changeable in teacher settings (i.e. specifically at user level only).  This also assumed in frmEditConfig.FillPalettes
		internal const string Delta_Applied = "Delta_Applied"; // system only.  The last set of version Deltas (changes to default between versions) which have been applied
		internal const string Toolbar_OnlyExplicit = "Toolbar_OnlyExplicit";
		internal const string Toolbar_ButtonSize = "Toolbar_Button_Size";
		internal const string Show_DefaultOff = "Show_DefaultOff";
		internal const string Shapes_DefaultOff = "Shapes_DefaultOff";
		internal const string MouseStep_Small = "MouseStep_Small";
		internal const string MouseStep_Medium = "MouseStep_Medium";
		internal const string MouseStep_Large = "MouseStep_Large";
		internal const string Smaller_Tools = "Smaller_Tools"; // forces shapes palette to use small icons
		internal const string Page_Size = "Page_Size";
		internal const string Selection_Bounds = "Selection_Bounds";
		internal const string Display_Origin = "Display_Origin";
		internal const string Speak_Selected = "Speak_Selected";
		/// <summary>Enables full vector graphic editing, grab handles etc</summary>
		internal const string Advanced_Graphics = "Advanced_Graphics";
		internal const string Context_Menus = "Context_Menus";
		internal const string Windows_Colours = "Windows_Colours";

		// ReSharper disable UnusedMember.Local
		// removed in v2 - code to tidy up actual data
		private static string[] Removed = { "NumberPad_Position_X", "NumberPad_Position_Y", "Advanced_Underneath", "Display_Menu", "Advanced_Arrowheads" };
		// ReSharper restore UnusedMember.Local

		#endregion

		#region String constants specific to SAW
		/// <summary>Only stored in UserUser config - not any other level, or edit mode</summary>
		public const string SAW_EnableItemStyles = "Enable_Item_Styles";
		public const string Show_Hidden_When_Editing = "Show_Hidden_When_Editing";
		#endregion

		#region Shared variables containing the current settings
		/// <summary>created by application constructor</summary>
		public static Config SystemConfig;

		private static readonly Dictionary<Guid, Document> ActivityNew = new Dictionary<Guid, Document>(); // these are now only loaded as needed; since we use both the user and both settings, we store the documents here

		/// <summary>Returns config DOCUMENT for activity with given ID.  Activities.GetActivitySettings() returns document for currently selected; or Nothing</summary>
		public static Document ActivityConfig(Guid ID)
		{
			return !ActivityNew.ContainsKey(ID) ? null : ActivityNew[ID];
		}

		public static bool ActivityExists(Guid ID, bool rejectAutoCreated = false)
		{
			if (!ActivityNew.ContainsKey(ID))
				return false;
			if (rejectAutoCreated && ActivityNew[ID].UserSettings.AutoCreated)
				return false;
			return true;
		}

		public static IEnumerable<Document> ActivityConfigs()
		{
			return ActivityNew.Values;
		}

		public static void AddActivity(Document document)
		{
			ActivityNew.Add(document.ID, document);
		}

		/// <summary>Settings for current user and teacher.  For alternate users see Application.UserConfigs</summary>
		private static Config User;

		public static Config UserUser
		{
			get { return User; }
			set { User = value; }
		}

		/// <summary>Just user config.  In Splash this can be the teache config instead</summary>
		public static Config UserCurrent
		{
			get { return User; }
		}

		/// <summary>Dummy for compatibility with Splash.  This = Config.UserCurrent, but in Splash would be a different config</summary>
		public static Config UserEditor
		{
			get { return User; }
		}

		public static string[] StandardIDs = { "System", "User" };
		// note case sensitive!
		public static Config Standard(string ID)
		{
			switch (ID)
			{
				case "System":
					return SystemConfig;
				case "User":
					return UserUser;
				default:
					// must be the ID of an activity.  All activity configurations should already be loaded
					if (!Activities.AllActivitiesLoaded)
						throw new InvalidOperationException("Cannot use Deltas when not all activities loaded");
					Guid activityID;
					if (!Guid.TryParse(ID, out activityID))
						return null;
					if (!Config.ActivityNew.ContainsKey(activityID))
						return null; // we don't have this activity; if applying updates, we would just ignore this
					return ActivityNew[activityID].UserSettings;
			}
		}

		#endregion

		#region Datum stuff etc
#if DEBUG
		internal string SourceFile; // where it was loaded from (if created by FromFile)
#endif
		public override byte TypeByte
		{
			get
			{
				return (byte)FileMarkers.Config;
			}
		}

		public override void Load(DataReader reader)
		{
			base.Load(reader);
			// the values are written as pairs of strings, with an empty string to terminate
			string key = reader.ReadString();
			while (!string.IsNullOrEmpty(key))
			{
				string value = reader.ReadString();
				Values.Add(key, value);
				key = reader.ReadString();
			}
			int ignoreFolders = reader.ReadInt16(); // for compatibility (shape folders in Splash)
			for (int i = 0; i < ignoreFolders; i++)
			{
				reader.ReadByte();
				reader.ReadInt32();
				reader.ReadGuid();
				reader.ReadString();
			}
			int discard = reader.ReadInt16(); // palette shapes.  Must be able to discard to load old settings for now
			for (int i = 0; i < discard; i++)
			{
				reader.ReadByte(); // type
				reader.ReadInt32(); // size
				reader.ReadGuid();
				reader.ReadData();
				reader.ReadString();
				_ = new MemoryImage(reader);
				_ = new MemoryImage(reader);
				_ = new MemoryImage(reader);
				reader.ReadBoolean();
				reader.ReadBoolean();
				reader.ReadBoolean();
			}
			RecentFiles = reader.ReadListString();
			if (reader.ReadByte() != 0)
				throw new Exception("Invalid file: Config.StartItem type must be 0"); // contains other splash data
			Picture = reader.ReadOptionalMemoryImage();
			if (reader.ReadInt32() != 0)
				throw new Exception("Invalid config: contains licences");

			// Likewise this was used in version 71, but that has also been used for v1
			int count = reader.ReadInt32();
			if (count > 0)
			{
				PalettePositions = new Dictionary<string, PalettePosition>();
				// ReSharper disable RedundantAssignment
				for (int index = 0; index <= count - 1; index++)
				{
					// ReSharper restore RedundantAssignment
					key = reader.ReadString();
					PalettePosition position = new PalettePosition();
					position.Load(reader);
					PalettePositions.Add(key, position);
				}
			}
			CustomPalettes.Clear();
			count = reader.ReadInt32();
			while (count > 0)
			{
				Document create = Document.FromReader(reader);
				create.PaletteWithin = this;
				create.Filename = create.PaletteDescription;
				if (!CustomPalettes.ContainsKey(create.ID))
					CustomPalettes.Add(create);
				else
					Utilities.LogSubError("Config " + ID + " contains duplicate palette: " + create.ID);
				count -= 1;
			}
			int n = reader.ReadInt32();
			if (n == 0)
				ButtonStyle = null;
			else
			{
				ButtonStyle = new List<ButtonStyle>();
				for (int i = 0; i < n; i++)
					ButtonStyle.Add((ButtonStyle)reader.ReadData(FileMarkers.ButtonStyle));
			}
			ShapeSequence = reader.ReadBoolean() ? reader.ReadListString().ToArray() : null;
		}

		public override void Save(DataWriter writer)
		{
			base.Save(writer);
			foreach (string key in Values.Keys)
			{
				writer.Write(key);
				writer.Write(Values[key]);
			}
			writer.Write("");
			writer.WriteDatumList(null); // folders
			writer.Write((short)0); // palette shapes
			writer.Write(RecentFiles);
			writer.WriteByte(0); // no StartItem
			writer.WriteOptionalMemoryImage(Picture);
			writer.Write(0); // licences
			if (PalettePositions == null)
				writer.Write(0);
			else
			{
				writer.Write(PalettePositions.Count);
				foreach (string key in PalettePositions.Keys)
				{
					writer.Write(key);
					PalettePositions[key].Save(writer);
				}
			}
			writer.Write(CustomPalettes.Count);
			foreach (Document document in CustomPalettes.Values)
			{
				writer.Write(document);
			}
			if (ButtonStyle?[0] == null)
				writer.Write(0); // no styles stored (the case for most configs)
			else
			{
				writer.Write(ButtonStyle.Count);
				for (int i = 0; i < ButtonStyle.Count; i++)
				{
					writer.Write(ButtonStyle[i]);
				}
			}
			writer.Write(ShapeSequence != null);
			if (ShapeSequence != null)
				writer.Write(ShapeSequence);
		}

		public override void CopyFrom(Datum other, Datum.CopyDepth depth, Mapping mapID)
		{
			base.CopyFrom(other, depth, mapID);
			if (depth >= CopyDepth.Undo)
			{
				Values.Clear();
				Config config = (Config)other;
				foreach (string key in config.Values.Keys)
					Values.Add(key, config.Values[key]);
				// RecentFiles is deliberately not copied; we don't really want undo to forget any changes to the current list
				// likewise Start is not copied - we don't use the undo mechanism in the start screen
				Picture = config.Picture; // they share a reference; the picture itself won't be edited
				if (config.PalettePositions == null)
					PalettePositions = null;
				else
				{
					PalettePositions = new Dictionary<string, PalettePosition>();
					foreach (string key in config.PalettePositions.Keys)
						PalettePositions.Add(key, config.PalettePositions[key].Clone());
				}
				if (depth >= CopyDepth.Duplicate)
				{
					CustomPalettes.Clear();
					foreach (var datum in config.CustomPalettes.Values)
					{
						Document palette = (Document)datum;
						Document copy = (Document)palette.Clone(mapID);
						CustomPalettes.Add(copy);
						copy.PaletteWithin = this;
					}
				}
				else
				{
					CustomPalettes.Clear();
					CustomPalettes.Add(config.CustomPalettes.Values); // Can just copy the list of objects; they should individually be in the transaction if they have been edited
				}
				if (config.ButtonStyle == null)
					ButtonStyle = null;
				else
				{
					ButtonStyle = new List<ButtonStyle>();
					for (int index = 0; index < config.ButtonStyle.Count; index++)
					{
						if (config.ButtonStyle[index] != null && !config.ButtonStyle[index].IsUserDefault)
						{
							ButtonStyle.Add(new ButtonStyle());
							ButtonStyle[index].CopyFrom(config.ButtonStyle[index], depth, mapID);
						}
						else
							ButtonStyle.Add(config.ButtonStyle[index]); // whether it is a shared one or Nothing
					}
				}
				ShapeSequence = (string[])config.ShapeSequence?.Clone();
			}
		}

		public override void UpdateReferencesObjectsCreated(Document document, DataReader reader)
		{
		}

		public static Config FromFile(string file, Config.Levels levelHint)
		{
			if (levelHint == Levels.ActivityBoth)
				levelHint = Levels.ActivityUser;
			if (levelHint == Levels.DocumentBoth)
				levelHint = Levels.DocumentUser;
			DataReader reader = new DataReader(file, FileMarkers.ConfigDocumentFile);
			try
			{
				Document.ConfigLevelHint = levelHint;
				switch (reader.FileType)
				{
					case FileMarkers.ConfigDocumentFile:
						Document document = (Document)reader.ReadData(FileMarkers.Document);
						//reader.Close()
						document.UpdateReferencesObjectsCreated(document, reader);
#if DEBUG
						document.UserSettings.SourceFile = file;
						if (document.BothSettings != null)
							document.BothSettings.SourceFile = file;
#endif
						return document.UserSettings;
					default:
						throw new System.IO.IOException("Invalid file: file type must be Config or ConfigDocumentFile");
				}
			}
			finally
			{
				reader.Close();
			}
		}

		private static bool SaveFailureReported = false;
		public void SaveTo(string file)
		{
			Debug.Assert(!file.Contains("\\bin\\"), "Saving config within bin folder !?");
			try
			{
				if (Document == null)
					Document = new Document(true) { UserSettings = this };
				Debug.Assert(Document.UserSettings == this); // just in case we have referenced the BothSettings
				using (System.IO.MemoryStream stream = new System.IO.MemoryStream(10000))
				{
					// write to a buffer first in case of error - don't want to lose all settings!
					using (DataWriter writer = new DataWriter(stream, FileMarkers.ConfigDocumentFile))
					{
						writer.Write(Document);
					}

					System.IO.File.WriteAllBytes(file, stream.ToArray());
				}

			}
			catch (Exception ex)
			{
				if (!SaveFailureReported)
					MessageBox.Show(Strings.Item("Config_Save_Failed") + "  " + ex.Message);
				SaveFailureReported = true;
			}
		}

		public override bool IdenticalTo(Datum other)
		{
			Config config = (Config)other;
			foreach (var key in Values.Keys)
			{
				if (!config.Values.ContainsKey(key))
					return false;
				if (Values[key] != config.Values[key])
					return false;
			}
			Debug.Assert(Picture == null);
			Debug.Assert(ButtonStyle?[0] == null);
			Debug.Assert(PalettePositions == null);
			return true;
		}

		#endregion

		#region Level stuff
		public enum Levels
		{
			DocumentUser = 0, // must be 0-based and ascending since these are used as an array index
			DocumentBoth = 1,
			ActivityUser,
			ActivityBoth,
			User,
			System,

			_First = DocumentUser,
			_Last = System,
			_Count = _Last + 1
		}

		public static bool LevelIsUserOnly(Levels level)
		{
			return level == Levels.ActivityUser || level == Levels.DocumentUser;
		}

		#endregion

		#region Value access functions
		public string ReadString(string key) // returns nothing if missing
		{
			if (!Values.ContainsKey(key))
				return null;
			return Values[key];
		}

		/// <summary>returns null if missing.  Can use ReadBooleanEx to return false if missing</summary>
		public bool? ReadBoolean(string key)
		{
			if (!Values.ContainsKey(key))
				return null;
			return Convert.ToBoolean(Values[key]);
		}

		/// <summary>returns true/false, defaulting to false</summary>
		public bool ReadBooleanEx(string key)
		{
			if (!Values.ContainsKey(key))
				return false;
			return Convert.ToBoolean(Values[key]);
		}

		public int ReadInteger(string key, int defaultValue = 0)
		{
			if (!Values.ContainsKey(key))
				return defaultValue;
			return int.Parse(Values[key]);
		}

		public float ReadSingle(string key) // returns 0 if missing
		{
			return !Values.ContainsKey(key) ? 0 : Convert.ToSingle(Values[key], System.Globalization.CultureInfo.InvariantCulture);
		}

		public Guid ReadGuid(string key)
		{
			return !Values.ContainsKey(key) ? Guid.Empty : new Guid(Values[key]);
		}

		public SizeF ReadSize(string key)
		{
			if (!Values.ContainsKey(key))
				return SizeF.Empty;
			// values are separated by | (because , might be the decimal)
			string[] parts = Values[key].Split('|');
			if (parts.Length != 2)
			{
				Utilities.LogSubError("Config.ReadSize(" + key + ") - value is not correctly formatted");
				return SizeF.Empty;
			}
			float X = Convert.ToSingle(parts[0], System.Globalization.CultureInfo.InvariantCulture);
			float Y = Convert.ToSingle(parts[1], System.Globalization.CultureInfo.InvariantCulture);
			return new SizeF(X, Y);
		}

		// Some of the writing functions accepts special values which remove the key instead (e.g. As string = nothing)
		public void Write(string key, string value)
		{
			if (string.IsNullOrEmpty(key))
			{
				Debug.Fail("Writing empty config key");
				return;
			}
			if (value == null)
				RemoveValue(key);
			else
				Values[key] = value;
		}

		public void Write(string key, int value)
		{
			if (string.IsNullOrEmpty(key))
			{
				Debug.Fail("Writing empty config key");
				return;
			}
			Values[key] = value.ToString();
		}

		public void Write(string key, float value)
		{
			if (string.IsNullOrEmpty(key))
			{
				Debug.Fail("Writing empty config key");
				return;
			}
			Values[key] = Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture);
		}

		public void Write(string key, bool value)
		{
			if (string.IsNullOrEmpty(key))
			{
				Debug.Fail("Writing empty config key");
				return;
			}
			Values[key] = value.ToString();
		}

		public void Write(string key, Guid guid)
		{
			if (string.IsNullOrEmpty(key))
			{
				Debug.Fail("Writing empty config key");
				return;
			}
			Values[key] = guid.ToString();
		}

		public void Write(string key, SizeF sz)
		{
			if (string.IsNullOrEmpty(key))
			{
				Debug.Fail("Writing empty config key");
				return;
			}
			Values[key] = Convert.ToString(sz.Width, System.Globalization.CultureInfo.InvariantCulture) + "|" + Convert.ToString(sz.Height, System.Globalization.CultureInfo.InvariantCulture);
		}

		/// <summary>Writing NULL removes the key</summary>
		public void WriteOptionalBoolean(string key, bool? state)
		{
			if (string.IsNullOrEmpty(key))
			{
				Debug.Fail("Writing empty config key");
				return;
			}
			if (!state.HasValue)
				RemoveValue(key);
			else
				Write(key, state.Value);
		}

		public bool HasValue(string key)
		{
			return Values.ContainsKey(key);
		}

		public void RemoveValue(string key)
		{
			if (Values.ContainsKey(key))
				Values.Remove(key);
		}

		public int Count// Only really used for diagnostics
		{ get { return Values.Count; } }

		/// <summary>Removes any keys referencing a particular value; mainly intended for removing keys referencing an action which is no longer applicable (e.g. deleted PaletteShape)</summary>
		public void RemoveByValue(string value)
		{
			foreach (var strKey in (from k in Values.Keys where Values[k] == value select k).ToList()) // Extra ToList needed to ensure we're not modifying the iterated list
				Values.Remove(strKey);
		}

		#endregion

		#region Key name definitions

		public static string ShapeEnableKey(Shape.Shapes shape)
		{
			return "Shape_" + shape; // value should be a boolean
		}

		public static string KeyKey(Keys key)
		{
			return "Key_" + Convert.ToInt32(key).ToString("x");
		}

		public static Keys DecodeKeyKey(string key)
		{
			if (!key.StartsWith("Key_"))
			{
				Debug.Fail("Invalid key configuration key: " + key);
				return Keys.None;
			}
			return (Keys)Convert.ToInt32(key.Substring(4), 16);
		}

		public static string PaletteKeyFragment(Palette.Purpose purpose)
		{
			if (purpose.IsParameter)
			{
				if (purpose.Parameter == Parameters.FillColour || purpose.Parameter == Parameters.FillPattern || purpose.Parameter == Parameters.LineColour || purpose.Parameter == Parameters.LinePattern || purpose.Parameter == (Parameters)Palette.Purpose.Specials.Number || purpose.Parameter == Parameters.Tool)
					return purpose.Parameter.ToString();
				else if (purpose.Parameter == Parameters.FontSize)
					return "Font";
				else if (purpose.Parameter >= Parameters.ArrowheadEndType && purpose.Parameter <= Parameters.ArrowheadStartSize)
					return "ArrowheadEndType";
				else
				{
					Utilities.LogSubError("Invalid PaletteKeyFragment: " + purpose.Parameter);
					return "ignore";
				}
			}
			return purpose.Special.ToString();
		}

		public static string ShowPaletteKey(Palette palette)
		{
			string fragment;
			if (palette.PalettePurpose.Special == Palette.Purpose.Specials.Custom)
				fragment = palette.ID;
			else
				fragment = PaletteKeyFragment(palette.PalettePurpose);
			if (fragment == "ignore")
				return "ignore";
			return "Show_Palette_" + fragment;
		}

		public static string ShowPaletteKey(Parameters whichPalette)
		{
			// Generally the version with a Palette parameter is better, but this is useful in the wizard to control the standard palettes
			return "Show_Palette_" + PaletteKeyFragment(new Palette.Purpose(whichPalette));
		}

		public static string SelectPaletteKey(Palette.Purpose purpose)
		{
			// Which palette should be used for the given purpose; stored as the string palette ID
			if (purpose.IsCustom)
				throw new ArgumentException("Config.SelectPaletteKey not applicable to custom purpose"); // all custom purpose palettes are shown or hidden individually; there is no selection between them
			string fragment = PaletteKeyFragment(purpose);
			if (fragment == "ignore")
				return "Ignore";
			return "Select_Palette_" + fragment;
		}

		public static string ToolbarIncludeKey(Functions.Codes code)
		{
			return "TBShow_" + code;
		}

		#endregion

		#region Defaults
		/// <summary>Default value for VI_Background if VI_ReplaceBackground is selected</summary>
		internal const int VI_Background_Default = unchecked((int)0xFFFFF196);
		/// <summary>Default value for VI_TextColour if VI_ChangeText is selected</summary>
		internal const int VI_Text_Default = unchecked((int)0xFF000088);
		internal const int Toolbar_ButtonSize_Default = 40; // v1 value - size of buttons in TB (img inside is smaller)

		public enum MouseStepDefaults
		{
			Small = 1,
			Medium = 4,
			Large = 25
		}

		internal void InitialiseAsDefaultSystem()
		{
		}

		/// <summary>Checks that complex objects used only in user resources exist</summary>
		internal void EnsureUserResources()
		{
			if (RecentFiles == null)
				RecentFiles = new List<string>();
			if (ButtonStyle == null)
				ButtonStyle = new List<ButtonStyle>();
			while (ButtonStyle.Count < BUTTONSTYLES)
				ButtonStyle.Add(null);
			SAW.ButtonStyle.EnsureConfigUserDefaultsSet(this);
			for (int i = 2; i < BUTTONSTYLES; i++)
			{ // all SAW ones
				if (ButtonStyle[i] == null)
				{
					ButtonStyle[i] = new ButtonStyle() { ImageType = SAW.ButtonStyle.ImageTypes.None };
					ButtonStyle[i].Initialise();
					ButtonStyle[i].LineStyle[0] = new Shape.LineStyleC() { Colour = Color.Black, Width = 1 };
					ButtonStyle[i].LineStyle[1] = new Shape.LineStyleC() { Colour = Color.Red, Width = 1 };
					ButtonStyle[i].LineStyle[2] = new Shape.LineStyleC() { Colour = Color.Red, Width = 1 };// not used, but nulls fail
					ButtonStyle[i].FillStyle[0] = new Shape.FillStyleC() { Colour = Color.LightGray, Pattern = Shape.FillStyleC.Patterns.Solid };
					ButtonStyle[i].FillStyle[1] = new Shape.FillStyleC() { Colour = Color.White, Pattern = Shape.FillStyleC.Patterns.Solid };
					ButtonStyle[i].FillStyle[2] = new Shape.FillStyleC() { Colour = Color.White, Pattern = Shape.FillStyleC.Patterns.Solid };
					ButtonStyle[i].TextColour[0] = Color.Black;
					ButtonStyle[i].TextColour[1] = Color.Black;
					ButtonStyle[i].TextColour[2] = Color.Black;
				}
			}
		}

		#endregion

		#region Actions

		#endregion

		#region MRU - recent files
		/// <summary>only actually stored in Config.User.  List will be empty in all other config objects.  most recent file is at the END</summary>
		public List<string> RecentFiles = new List<string>();
		private const int MAXIMUMRECENTFILES = 10;

		public void RememberFile(string file)
		{
			if (string.IsNullOrEmpty(file))
				return;
			if (RecentFiles.Contains(file))
				RecentFiles.RemoveAt(RecentFiles.IndexOf(file));
			RecentFiles.Add(file);
			while (RecentFiles.Count > MAXIMUMRECENTFILES)
				RecentFiles.RemoveAt(0);
		}

		#endregion

		#region Delta
		// Any value which has been removed is written into the change with a value of ""
		public Config CalculateDelta(Config fromConfig)
		{
			// calculate all of the changes from the parameter to this object
			Config newConfig = new Config();
			foreach (string key in this.Values.Keys)
			{
				if (!fromConfig.Values.ContainsKey(key) || fromConfig.Values[key] != this.Values[key])
				{
					if (key == Delta_Applied)
						continue;
					if (key.StartsWith("Open_Folder"))
						continue; // current folder locations for various file types.  Don't want to propagate from me to them!
					newConfig.Values.Add(key, this.Values[key]);
				}
			}
			// now check for anything which has been removed
			foreach (string key in fromConfig.Values.Keys)
			{
				if (!Values.ContainsKey(key) && key != Delta_Applied)
					newConfig.Values.Add(key, "");
			}
			try
			{
				IdenticalForPaletteDelta = true;
				foreach (var pair in CustomPalettes)
				{
					if (!fromConfig.CustomPalettes.ContainsKey(pair.Key) || !pair.Value.IdenticalTo(fromConfig.CustomPalettes[pair.Key])) // CompareDocuments(objPair.Value, CustomPalettes(objPair.Key)) Then
					{
#if DEBUG
						// spurious looking #IF condition is needed since SourceFile not declared in release
						Debug.WriteLine("Palettes changed: " + ((Document)pair.Value).PaletteEditingFullDescription + " in delta from file: " + fromConfig.SourceFile);
#endif
						newConfig.CustomPalettes.Add(pair.Key, pair.Value);
					}
				}
			}
			finally
			{
				IdenticalForPaletteDelta = false;
			}
			return newConfig;
		}

		// changes to shape folders and PaletteShapes are ignored at the moment.  But the data is stored as a config object, and could contain these
		// we would need to implement the difference detection, and processing of the update.
		// UserOnly is ignored is only applies to the document
		// RecentFiles is ignored as we wouldn't want to change this!

		internal struct Delta
		{
			// used temporarily when generating the Deltas
			public Config Before;
			public Config After;
			public Config Changes;
			public string ID; // also part of the filename.  Identifies which one this is
			public string Name;

#if DEBUG
			public Delta(string ID, Levels level, string name = "", Config before = null, Config after = null)
			{
				this.ID = ID;
				Before = before;
				After = after;
				Name = name;
				if (string.IsNullOrEmpty(Name))
					Name = this.ID;
				if (Before == null)
				{
					string strFolder = Globals.Root.ConfigFolder + "\\previous version configs";
					Before = FromFile(strFolder + "\\" + ID + Extension, level);
				}
				if (After == null)
					After = Standard(ID);
				Changes = After.CalculateDelta(Before);
				foreach (string remove in g_aRemove)
				{
					if (Changes.HasValue(remove))
						Changes.RemoveValue(remove);
				}
			}

			private static string[] g_aRemove = { };

			internal static void SaveDeltas(string file, Dictionary<string, Delta> deltas)
			{
				// this is stored as the IDs followed by the config objects.  Document objects are not included
				// empty string terminates
				using (DataWriter writer = new DataWriter(file, FileMarkers.ConfigDelta))
				{
					foreach (string key in deltas.Keys)
					{
						Delta delta = deltas[key];
						if (!delta.IsEmpty)
						{
							writer.Write(key);
							writer.Write(delta.Name);
							writer.Write(delta.Changes);
						}
					}
					writer.Write("");
				}

			}
#endif

			public bool IsEmpty => Changes.Values.Count == 0 && Changes.CustomPalettes.Count == 0;

			internal static Dictionary<string, Config> LoadDeltas(string file)
			{
				// note that this does not return Delta structures; rather each set of changes is represented just by the Config object
				// which was Changes within the Delta object
				DataReader reader = new DataReader(file, FileMarkers.ConfigDelta);
				Dictionary<string, Config> deltas = new Dictionary<string, Config>();
				string ID = reader.ReadString();
				while (!string.IsNullOrEmpty(ID))
				{
					reader.ReadString(); // this can be ignored when applying the deltas ' Dim strName As String =
					Config config = (Config)reader.ReadData(FileMarkers.Config);
					deltas.Add(ID, config);
					ID = reader.ReadString();
				}
				reader.Close();
				return deltas;
			}

			private struct Update : IComparable<Update>
			{

				public readonly float Version;
				public readonly string File;

				public Update(float version, string file)
				{
					Version = version;
					File = file;
				}

				public int CompareTo(Update other)
				{
					return Version.CompareTo(other.Version);
				}

			}

			internal static void ApplyDeltas(string folder)
			{
				Activities.EnsureAllActivitiesLoaded();
				// applies any changes required from the files in the given folder
				// a high version number can be used to force it to always apply; the Delta_Applied is set to the software version
				// only put system changes in this!
				List<Update> updates = new List<Update>();
				foreach (string file in System.IO.Directory.GetFiles(folder))
				{
					string versionName = System.IO.Path.GetFileNameWithoutExtension(file);
					// can be either version number (0.31.1) or 2-char language followed by this separated by hyphen (sv-0.31.1)
					if (versionName.Length > 3 && versionName[2] == '-')
					{
						string language = versionName.Substring(0, 2);
						versionName = versionName.Substring(3, 0);
						if (language != Server.Language2)
							continue; // wrong language - skip this one
					}
					try
					{
						float version = SoftwareVersion.VersionNumberFromString(versionName);
						updates.Add(new Update(version, file));
					}
					catch (Exception)
					{
						Utilities.LogSubError("Could not parse delta file: " + versionName);
					}
				}
				updates.Sort(); // will sort into ascending numerical version order
				float done = SystemConfig.ReadSingle(Delta_Applied);
				foreach (Update update in updates)
				{
					if (update.Version > done)
					{
						// must be applied
						ApplyDeltaFile(update.File, update.Version);
						Globals.Root.LogPermanent.WriteLine("Applied delta file: " + System.IO.Path.GetFileName(update.File));
					}
				}
				SystemConfig.Write(Delta_Applied, SoftwareVersion.Version);
			}

			private static void ApplyDeltaFile(string file, float version)
			{
				try
				{
					Dictionary<string, Config> deltas = Delta.LoadDeltas(file);
					foreach (string configID in deltas.Keys)
					{
						List<Config> configs = new List<Config>();
						// may be more than one config - for standard users we need to update all of them
						if (configID == "User")
							configs.Add(Config.User);
						else
						{
							Config config = Config.Standard(configID); // can be nothing if it is an old activity we are not interested in
							if (config != null)
								configs.Add(config); //ApplySingleDelta will be happy with an empty list
						}
						Delta.ApplySingleDelta(deltas[configID], configs);
					}

				}
				catch (Exception ex)
				{
					Utilities.LogSubError(ex);
				}
			}

			/// <summary>applies one .delta file which contains changes to all configs for one version of splash</summary>

			internal static void ApplySingleDelta(Config delta, List<Config> configs)
			{
				// applies single config object containing changes to all of the configs in the given list
				foreach (Config config in configs)
				{
					foreach (string key in delta.Values.Keys)
					{
						if (string.IsNullOrEmpty(delta.Values[key]))
							config.RemoveValue(key);
						else
							config.Write(key, delta.Values[key]);
					}
					foreach (var pair in delta.CustomPalettes)
					{
						Globals.Root.LogPermanent.WriteLine("Applied palette in config delta: " + pair.Key + " to " + config.ID);
						config.CustomPalettes[pair.Key] = pair.Value;
					}
				}
			}

		}
		#endregion

		#region Palettes and positions

		public Dictionary<string, PalettePosition> PalettePositions; // only created as needed.  Only in user configs.  Key is the palette ID

		public void StorePalettePositions()
		{
			// only stores ones currently visible
			// any other items already stored in me are left unchanged (does give some possibility of DockIndex collisions, but not that critical)
			// must be called while AM.CurrentConfig is old settings
			if (PalettePositions == null)
				PalettePositions = new Dictionary<string, PalettePosition>();
			foreach (Palette palette in Palette.List.Values)
			{
				if (Globals.Root.CurrentConfig.ShowPalette(palette))
				{
					if (!PalettePositions.ContainsKey(palette.ID))
						PalettePositions.Add(palette.ID, palette.Position.Clone());
					else
						PalettePositions[palette.ID].CopyFrom(palette.Position);
				}
			}
			// But we delete any records for palettes which don't even exist
			List<string> remove = new List<string>();
			foreach (string key in PalettePositions.Keys)
			{
				if (!Palette.List.ContainsKey(key))
					remove.Add(key);
			}
			foreach (string key in remove)
			{
				PalettePositions.Remove(key);
			}
		}

		public void RestorePalettePositions()
		{
			// updates palette list with my list.  Updates all whether currently visible or not
			if (PalettePositions == null)
				return;
			foreach (string key in PalettePositions.Keys)
			{
				if (Palette.List.ContainsKey(key))
					Palette.List[key].Position.CopyFrom(PalettePositions[key]);
				else
					Debug.WriteLine("Cannot restore palette position, unknown palette: " + key);
			}
		}

		/// <summary>User-defined palettes.  Each is a document. PalettePositions will usually be a much larger list than this.  Palettes is typically empty, but the positions records the positions of all the standard palettes </summary>
		public DatumList CustomPalettes = new DatumList();

		#endregion

	}
}
