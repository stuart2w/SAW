using System.Collections.Generic;
using System;
using System.Drawing;
using System.Diagnostics;
using System.Windows.Forms;
using System.Linq;
using Action = SAW.Functions.Action;


namespace SAW
{
	/// <summary>represents a list of Config objects
	/// values are locked up in the configs in order until a definition is found</summary>
	public class AppliedConfig
	{

		#region List of configurations, construction
		/// <summary>all values will be defined, no nothing values. first configuration has priority </summary>
		private readonly List<Config> Configurations = new List<Config>();

		public AppliedConfig()
		{
		}

		public AppliedConfig(IEnumerable<Config> list)
		{
			Configurations.AddRange(list.Where(c => c != null));
		}

		public AppliedConfig(Config config)
		{
			Configurations.Add(config ?? new Config()); // with no data, so will have no effect
		}

		public void AddConfigWithPriority(Config config)
		{
			// the new one has priority over the others
			// this deliberately ignores a null parameter for convenience
			if (config == null)
				return;
			Configurations.Insert(0, config);
		}

		public void AddConfigAtEnd(Config config)
		{
			if (config == null)
				return;
			Configurations.Add(config);
		}

		#endregion

		#region Value type look up functions
		// all of these functions direct through ReadString
		public string ReadString(string key)
		{
			// returns nothing if not defined anywhere.  This is used as the basis for all of the other look up types
			foreach (Config config in Configurations)
			{
				if (config.HasValue(key))
					return config.ReadString(key);
			}
			return "";
		}

		public bool ReadBoolean(string key, bool defaultValue = false)
		{
			string value = ReadString(key);
			if (string.IsNullOrEmpty(value))
				return defaultValue;
			return bool.Parse(value);
		}

		public bool? ReadBooleanAsNullable(string key)
		{
			string value = ReadString(key);
			bool result;
			if (value == null || !bool.TryParse(value, out result))
				return null;
			return result;
		}

		public int ReadInteger(string key, int defaultValue = 0)
		{
			string value = ReadString(key);
			int result;
			if (value == null || !int.TryParse(value, out result))
				return defaultValue;
			return result;
		}

		public T ReadEnum<T>(string key, T defaultValue = default) where T : struct, IConvertible
		{ // https://stackoverflow.com/questions/79126/create-generic-method-constraining-t-to-an-enum
			string value = ReadString(key);
			int result;
			if (value == null || !int.TryParse(value, out result))
				return defaultValue;
			return (T)(object)result; // note intermediate (object) case is needed: https://stackoverflow.com/questions/10387095/cast-int-to-generic-enum-in-c-sharp
		}

		public float ReadSingle(string key, float defaultValue = 0)
		{
			string value = ReadString(key);
			float result;
			if (value == null || !float.TryParse(value, out result))
				return defaultValue;
			return result;
		}

		public Guid ReadGuid(string key)
		{
			// always returns Guid.empty if unknown
			string value = ReadString(key);
			if (string.IsNullOrEmpty(value))
				return Guid.Empty;
			return new Guid(value);
		}

		public SizeF ReadSize(string key, SizeF defaultValue)
		{
			foreach (Config config in Configurations)
			{
				if (config.HasValue(key))
					return config.ReadSize(key);
			}
			return defaultValue;
		}

		public bool HasValue(string key)
		{
			foreach (Config config in Configurations)
			{
				if (config.HasValue(key))
					return true;
			}
			return false;
		}

		#endregion

		#region Specialist look up functions

		public bool ShapeEnabled(Shape.Shapes shape)
		{
			var key = Config.ShapeEnableKey(shape);
			if (Configurations[0].ReadBooleanEx(Config.Shapes_DefaultOff))
				return Configurations[0].ReadBooleanEx(key);
			return ReadBoolean(key, true);
		}

		public Action KeyAction(Keys key)
		{
			string action = ReadString(Config.KeyKey(key));
			if (string.IsNullOrEmpty(action))
				return Action.Empty;
			return Action.CreateFromConfigString(action);
		}

		public List<Functions.Codes> GetToolbarVerbs()
		{
			// returns a list of all the verbs which need to appear on the toolbar
			// this is not particularly efficient, and we don't cache the result.  However this will only be called once by the main screen as the configuration is applied
			// so there's no point storing the result here
			List<Functions.Codes> list = new List<Functions.Codes>();
			foreach (Functions.Codes verb in Enum.GetValues(typeof(Functions.Codes)))
			{
				if (ReadBoolean(Config.ToolbarIncludeKey(verb)))
					list.Add(verb);
			}
			return list;
		}

		public float DefaultZoom() // returns the actual value as used by the GUI (not *100 as stored in the config)
		{
			return ReadInteger(Config.Default_Zoom, 100) / 100f;
		}

		public bool ShowArea(string key, bool defaultValue = false)
		{
			// Basically ReadBoolean, but also checks for the explicit only mode
			bool? state = ReadBooleanAsNullable(key);
			if ((!state.HasValue || state == false) && Configurations[0].ReadBooleanEx(Config.Show_DefaultOff))
				state = false;
			if (!state.HasValue)
				return defaultValue;
			return state == true;
		}
		// see also palettes section - ShowPalette

		public enum MouseSteps
		{
			/// <summary>Only used in SAW</summary>
			Single = 0,
			// Config.DefaultMouseSteps contains the default value
			Small = 1, // the names cannot be changed, as these must match the Verb enum and images
			Medium = 2,
			Large = 3
		}

		internal int MouseStep(MouseSteps step)
		{
			switch (step)
			{
				case MouseSteps.Single: return 1; // fixed
				case MouseSteps.Small: return ReadInteger(Config.MouseStep_Small, (int)Config.MouseStepDefaults.Small);
				case MouseSteps.Medium: return ReadInteger(Config.MouseStep_Medium, (int)Config.MouseStepDefaults.Medium);
				case MouseSteps.Large: return ReadInteger(Config.MouseStep_Large, (int)Config.MouseStepDefaults.Large);
				default:
					Utilities.LogSubError("Invalid mouse step: " + step);
					return ReadInteger(Config.MouseStep_Small, (int)Config.MouseStepDefaults.Small);
			}
		}

		#endregion

		#region Specific value look up functions, only implemented for common lookups
		// some of these are now cached for speed, as they might be looked up very repeatedly
		// if the user configuration changes then a new applied configuration is generated, so the cache will automatically refresh
		private bool m_Cache = false;

		private void LookupCache()
		{
			if (!m_Cache)
			{
				m_LowGraphics = ReadBoolean(Config.Low_Graphics);
				m_Cache = true;
			}
		}

		private bool m_LowGraphics;
		public bool Low_Graphics
		{
			get
			{
				if (!m_Cache)
					LookupCache();
				return m_LowGraphics;
			}
		}

		/// <summary>Can be used by editor when they value is changed which might otherwise the cached</summary>
		public void DiscardCache()
		{
			m_Cache = false;
		}

		#endregion

		#region keys 

		public List<Shape.Shapes> GetAllShapesInUse()
		{
			List<Shape.Shapes> list = new List<Shape.Shapes>(Shape.UserShapes);
			// and finally need to sort the null folder according to any sequences
			List<Shape.Shapes> sorted = new List<Shape.Shapes>();
			foreach (var sequence in from c in Configurations where c.ShapeSequence != null select c.ShapeSequence)
			{
				foreach (string ID in sequence)
				{
					Shape.Shapes shape;
					if (ID.Length <= 5)
						shape = (Shape.Shapes)int.Parse(ID);
					else
						continue;
					if (!list.Contains(shape))
						continue; // either not displayed, or is within a folder
					if (!sorted.Contains(shape))
						sorted.Add(shape);
				}
			}
			if (sorted.Count > 0)
			{
				// if there were some sequenced shapes... add any which weren't listed
				foreach (var shape in list)
				{
					if (!sorted.Contains(shape))
						sorted.Add(shape);
				}
				return sorted;
			}
			return list;
		}

		// Reverse lookup keys is stored, but only generated when first requested
		// (Because AppliedConfig objects can otherwise be generated quite cheaply and used for fairly simple look up)
		private Dictionary<string, List<Keys>> m_KeyLookup;
		// the keys in the dictionary are the strings describing the actions

		public void DiscardKeyLookup()
		{
			// used by the config editing screen when it changes keys
			m_KeyLookup = null;
		}

		private void EnsureKeyLookup()
		{
			if (m_KeyLookup != null)
				return;
			m_KeyLookup = new Dictionary<string, List<Keys>>();
			// we need to keep track of which keys have been used in case the same key is set at a different priority levels
			Dictionary<Keys, Keys> used = new Dictionary<Keys, Keys>();
			foreach (Config config in Configurations)
			{
				foreach (string key in config.Values.Keys)
				{
					if (key.StartsWith("Key_"))
					{
						var keyCode = Config.DecodeKeyKey(key);
						if (keyCode != Keys.None && !used.ContainsKey(keyCode))
						{
							string action = config.ReadString(key);
							used.Add(keyCode, keyCode);
							if (m_KeyLookup.ContainsKey(action))
								m_KeyLookup[action].Add(keyCode);
							else
								m_KeyLookup.Add(action, new List<Keys> { keyCode });
						}
					}
				}
			}
		}

		public List<Keys> GetKeysForAction(Action action)
		{
			return GetKeysForAction(action.ToString());
		}

		public List<Keys> GetKeysForAction(string action)
		{
			EnsureKeyLookup();
			if (!m_KeyLookup.ContainsKey(action))
				return null;
			return m_KeyLookup[action];
		}

		public Keys GetFirstKeyForAction(Action action)
		{
			// sometimes a more convenient version of the above, when labelling GUI elements with their key (usually the GUI can only display one key)
			// this saves checking for empty lists etc
			EnsureKeyLookup();
			string text = action.ToString();
			if (!m_KeyLookup.ContainsKey(text))
				return Keys.None;
			return m_KeyLookup[text][0];
		}

		#endregion

		#region Palettes and CustomPalettes

		// everything in Config.CustomPalettes, indexed by ID (Guid converted to string)
		// Key is done as a string not Guid because the palette handling has other palettes identified using strings, and this save converting back to GUIDs to check them in this list
		private Dictionary<string, Document> m_CustomPalettes = null;
		// list of all allowed palettes in this configuration.  This is not necessarily match Palettes.List, because that is based
		// around the current configuration(s).  This must match the palettes accessible from this configuration exactly.
		private List<Palette> m_AllPalettes;

		private void CreatePaletteLists(bool registerCustom = false)
		{
			m_CustomPalettes = new Dictionary<string, Document>();
			m_AllPalettes = new List<Palette>();
			// First add the system palettes which are not in any configuration:
			m_AllPalettes.AddRange(Palette.List.Values.Where(x => x.UserDefined == false));
			foreach (Config config in Configurations)
			{
				foreach (Document document in config.CustomPalettes.Values)
				{
					string ID = document.ID.ToString();
					if (!m_CustomPalettes.ContainsKey(ID)) // just in case
					{
						m_CustomPalettes.Add(ID, document);
						// Now add the palette object.  If possible the one from the global list is used; is not a new object is created
						if (Palette.List.ContainsKey(ID))
							m_AllPalettes.Add(Palette.List[ID]);
						else
						{
							Palette palette = new Palette(document);
							if (registerCustom)
								Palette.Register(palette);
							m_AllPalettes.Add(palette);
						}
					}
				}
			}
		}

		/// <summary>If registerCustom, then Palette.Register is called for any not already registered</summary>
		public Dictionary<string, Document> CustomPalettes(bool registerCustom)
		{
			if (m_CustomPalettes == null)
				CreatePaletteLists(registerCustom);
			return m_CustomPalettes;
		}

		public List<Palette> AvailablePalettes()
		{
			// All palettes available as of this configuration.  Include both static system ones and ones defined in this configuration
			if (m_AllPalettes == null)
				CreatePaletteLists();
			return m_AllPalettes;
		}

		public List<Palette> AvailablePalettesForPurpose(Palette.Purpose purpose)
		{
			// There is no index - it is probably just as efficient to do a flat search given the relatively small number
			if (m_AllPalettes == null)
				CreatePaletteLists();
			Debug.Assert(!purpose.IsCustom); // This is perfectly possible, but it seems less likely that it is intended
			return m_AllPalettes.Where(p => p.PalettePurpose.Equals(purpose)).ToList();
		}

		public bool ShowPalette(Palette palette)
		{
			// Similar to ShowArea, but checks both if palette purpose is displayed, and this given palette is selected as the current one
			if (palette == null || palette.PalettePurpose.ToInt32() == 9)
				return false; // just in case
			Debug.Assert(this == Globals.Root.CurrentConfig); // Needed to use PaletteSelection (, true)
			string showKey = Config.ShowPaletteKey(palette);
			bool? state = ReadBooleanAsNullable(showKey);
			if ((!state.HasValue || !state.Value) && Configurations[0].ReadBooleanEx(Config.Show_DefaultOff))
				state = false;
			if (!state.HasValue)
				state = false; // palette.DefaultShow ' defaults should be given for all in system, except popup pads
			if (state == false)
				return false; // if this purpose is not selected then no need to check which palette is selected
			if (!palette.PalettePurpose.IsCustom)
			{
				// Custom palettes have no selection, they just turn on and off individually.  For all other palettes only one for each purpose is selected
				if (PaletteSelection(palette.PalettePurpose, true) != palette.ID)
					return false; // Some other palette is selected
			}
			return state.Value;
		}

		/// <summary>returns the ID of the palette selected for the given purpose</summary>
		public string PaletteSelection(Palette.Purpose purpose, bool verify)
		{
			// 
			// This is stored in the configuration, but we need to check for some special cases, such as if nothing is yet selected (must select something by default)
			// or if a palette has been selected and then later deleted.  Again it is essential that we return a valid palette, assuming there is one
			// But in the latter case, verification is difficult unless Palette.List matches this applied configuration.  This will be the case in the GUI
			// but not always in the editing.  Therefore the second parameter controls whether this check should be performed
			if (purpose.IsCustom)
				throw new ArgumentException("AppliedConfig.PaletteSelection: purpose cannot be \'custom\'");
			string key = Config.SelectPaletteKey(purpose);
			string selected = this.ReadString(key);
			if (!string.IsNullOrEmpty(selected) && verify)
			{
				if (!Palette.List.ContainsKey(selected))
				{
					Debug.WriteLine("Selected palette not available; selection reset for purpose: " + purpose.Name);
					selected = "";
				}
			}
			if (String.IsNullOrEmpty(selected))
			{
				// nothing selected - find most appropriate palette
				Palette selectedPalette = null;
				foreach (Palette possible in AvailablePalettesForPurpose(purpose))
				{
					if (string.IsNullOrEmpty(selected) || possible.UserDefined == false)
					{
						// Selects any palette, with built-in system ones having priority
						selected = possible.ID;
						selectedPalette = possible;
					}
				}
				if (!string.IsNullOrEmpty(selected)) // Store the value back again; it is rather slow searching each time
				{
					// find where it is defined
					Config sourceConfig = null;
					if (selectedPalette.UserDefined)
					{
						foreach (Config config in Configurations)
						{
							if (config.CustomPalettes.ContainsValue(selectedPalette.CustomDocument))
								sourceConfig = config;
						}
					}
					else
					{
						sourceConfig = Config.SystemConfig; // actually defined in S/W - can safely store in sys config
						Config.SystemConfig.Write(key, selected);
					}
					if (sourceConfig != null)
						sourceConfig.Write(key, selected);
					else
						Utilities.LogSubError("AppliedConfig.PaletteSelection: Failed to find place to write auto palette selection");
				}
			}
			return selected;
		}

		#endregion

	}

}
