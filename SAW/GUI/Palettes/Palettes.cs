using System.Collections.Generic;
using System;
using System.Drawing;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Windows.Forms;
using System.Linq;
using SAW.Functions;
using Action = SAW.Functions.Action;

#pragma warning disable CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()

namespace SAW
{
	/// <summary>persistable data storing position of a palette </summary>
	public class PalettePosition
	{
		/// <summary>only None,Left and Right supported at the moment</summary>
		public DockStyle Dock = DockStyle.Right;
		/// <summary>how close to edge of screen if more than one accordion ' higher is closer to edge (will often be -ve)</summary>
		public int DockLevel;
		/// <summary>Position within this accordion Register initialises this to increasing values. Doesn't NEED to be contiguous - they are just sorted into this order</summary>
		public int DockIndex;
		/// <summary>Only relevant if not docked.  RELATIVE Bounds of the control itself (excluding the containing form framework)
		/// NOTE this is in screen local coords of main screen.  [Now] Width also stored if docked.  All docked should have same width and any one is used for width of container</summary>
		public Rectangle Bounds;
		/// <summary>only relevant if NOT docked</summary>
		public bool Open = true;
		// if adding fields see copy from

		public int AccordionKey
		{ get { return Convert.ToInt32(Dock) * 10000 + DockLevel; } }

		public static int CalculateAccordionKey(DockStyle dock, int dockLevel)
		{
			return Convert.ToInt32(dock) * 10000 + dockLevel;
		}

		public void CopyFrom(PalettePosition other)
		{
			PalettePosition palettePosition = other;
			Dock = palettePosition.Dock;
			DockIndex = palettePosition.DockIndex;
			DockLevel = palettePosition.DockLevel;
			Bounds = palettePosition.Bounds;
			Open = palettePosition.Open;
		}

		public void Save(DataWriter writer)
		{
			writer.Write((int)Dock);
			writer.Write(DockLevel);
			writer.Write(DockIndex);
			writer.Write(Bounds);
			writer.Write(Open);
		}

		public void Load(DataReader reader)
		{
			Dock = (DockStyle)reader.ReadInt32();
			DockLevel = reader.ReadInt32();
			DockIndex = reader.ReadInt32();
			Bounds = reader.ReadRectangle();
			Open = reader.ReadBoolean();
			if (reader.Version < 75 && Dock != DockStyle.None || Bounds.Width < 10)
				Bounds.Width = 200; // wasn't previously set
		}

		public PalettePosition Clone()
		{
			PalettePosition create = new PalettePosition();
			create.CopyFrom(this);
			return create;
		}

		public override bool Equals(object obj)
		{
			if (obj is PalettePosition)
			{
				PalettePosition palettePosition = (PalettePosition)obj;
				if (Dock != palettePosition.Dock || DockLevel != palettePosition.DockLevel || DockIndex != palettePosition.DockIndex)
					return false;
				// Open is deliberately not checked; there isn't really a default state for Open
				if (Dock == DockStyle.None && !Bounds.Equals(palettePosition.Bounds))
					return false;
				// if Dock the contents of Bounds is pretty much undefined; therefore if it differs that does not count as different objects
				return true;
			}
			return false;
		}

		/// <summary>Sets the Bounds to be left or right of the given form, but within the same monitor (overlapping the form if needed)</summary>
		public void MoveBesideForm(Form target, bool left)
		{
			Debug.Assert(Dock == DockStyle.None);
			Rectangle newBounds = new Rectangle(0, target.Top, Bounds.Width, Bounds.Height);
			Screen screen = Screen.FromControl(target);
			if (left)
				newBounds.X = Math.Max(screen.WorkingArea.Left, target.Left - Bounds.Width);
			else 
				newBounds.X = Math.Min(screen.WorkingArea.Right - Bounds.Width, target.Right);
			newBounds.Y = Math.Max(screen.WorkingArea.Y, newBounds.Y);
			Bounds = target.RectangleToClient(newBounds); // because bounds is in form local coords
			//Debug.WriteLine("Move palette beside " + target.Text + " at " + newBounds + " with left=" + left);
		}

	}

	/// <summary>Container gathering all information about one possible palette.  Defined whether shown or not.  Data here is not editable.</summary>
	public class Palette
	{
		/// <summary>the actual control implementing this.  Must implement IPalette</summary>
		public readonly Control Control;
		public readonly Bitmap Icon;
		/// <summary>Name which appears on palette.  May be several with same name.  May be [] for translation</summary>
		public readonly string Title;
		/// <summary>the name which can be used when selecting/editing (always unambiguous, whereas Title may be the same for different palettes of a single purpose)</summary>
		public readonly string EditDescription;
		public readonly PalettePosition Position = new PalettePosition();
		public readonly Document CustomDocument; // only defined if this is UserDefined
		public Purpose PalettePurpose; // Cannot be readonly as purpose can be edited (rare but possible)

		// both of these might be set (once constructed we keep the controls in memory), but only one will be applicable at a time
		public Accordion Accordion;
		public frmPalette Form;

		/// <summary>The tooltip to use if not overridden by a specific container.  Applied to palette via IPalette.SetToolTips during Register using this.  Palette form may call that again with its own.
		/// Main editor form sets this once on construction</summary>
		public static ToolTip DefaultTooltip;

		public Bitmap Preview;
		// can be used when moving to display the palette even if the actual control is elsewhere
		/// <summary>the name of the shortcut key; whether it is displayed still depends on the settings this is updated in frmMain.ApplyConfigurationVisibility</summary>
		/// <remarks>Only affects display - key's action does the functionality</remarks>
		public string ShortcutKey = ""; //

		/// <summary>If not Valid, the palette cannot be displayed and is not returned by Item.  Used mainly for custom palettes.  As they go out of scope the record is not deleted, but the control and record are kept because it will probably come back in scope again</summary>
		public bool IsValid { get; private set; } = true;

		// ID is defined by one of:
		public readonly Parameters PaletteParameter = Parameters.Undefined;
		/// <summary>this now has priority (user palettes can possibly have both this and parameter set later) </summary>
		private readonly string m_CustomID = "";

		/// <summary>is the CustomID if defined, and if not it is based on the parameter</summary>
		public string ID
		{
			get
			{
				if (!string.IsNullOrEmpty(m_CustomID))
					return m_CustomID;
				return "Parameter_" + (int)PaletteParameter;
			}
		}

		public Palette(Parameters parameter, Control ctr, string title, string imageName)
		{
			Debug.Assert(ctr is IPalette);
			Control = ctr;
			PaletteParameter = parameter;
			Title = Strings.Translate(title);
			if (Strings.IsTranslationMode)
				Title = "(" + ctr.GetType() + ") " + Title;
			EditDescription = Title + " " + Strings.Item("Palette_DefaultSuffix");
			if (!string.IsNullOrEmpty(imageName))
				Icon = (Bitmap)GUIUtilities.RM.GetObject(imageName);
			PalettePurpose = new Purpose(parameter);
		}

		public Palette(string customID, Purpose purpose, Control ctr, string title, string imageName)
		{
			Debug.Assert(ctr is IPalette);
			m_CustomID = customID;
			Control = ctr;
			Title = Strings.Translate(title);
			EditDescription = Title + " " + Strings.Item("Palette_DefaultSuffix");
			if (Strings.IsTranslationMode)
				Title = "(" + ctr.GetType() + ") " + Title;
			if (!string.IsNullOrEmpty(imageName))
				Icon = (Bitmap)GUIUtilities.RM.GetObject(imageName);
			PalettePurpose = purpose;
		}

		public Palette(Document document)
		{
			// version for user-defined palette
			m_CustomID = document.ID.ToString();
			CustomDocument = document;
			Control = new PaletteView(document, this);

			Title = Strings.Translate(document.PaletteTitle);
			if (Strings.IsTranslationMode)
				Title = "{" + document.ID + "} " + Title;
			EditDescription = Strings.Translate(document.PaletteEditingFullDescription);
			PalettePurpose = document.PalettePurpose;
			string imageName = PalettePurpose.ImageName;
			if (!string.IsNullOrEmpty(imageName))
				Icon = (Bitmap)GUIUtilities.RM.GetObject(imageName);
		}

		public bool Sizeable => !(Control is PalettePanel);
		/// <summary>True if defined from a document.  May still be system wide standard - ie from system config</summary>
		public bool UserDefined => CustomDocument != null;

		/// <summary>To be called when any existing preview may longer be valid</summary>
		public void InvalidatePreview()
		{
			Preview?.Dispose();
			Preview = null;
		}

		public void SetDefaultPosition()
		{
			Position.Dock = DockStyle.Right;
			Position.DockLevel = 0;
			Position.DockIndex = PalettePurpose.DefaultDockIndex();
		}

		#region Shared list
		/// <summary>all of the created palettes.  Items are added by their constructors</summary>
		public static Dictionary<string, Palette> List = new Dictionary<string, Palette>();
		public static void Register(Palette palette)
		{
			if (List.ContainsKey(palette.ID))
			{
				List.Remove(palette.ID);
				Debug.WriteLine("Removing existing palette with ID " + palette.ID + " due to conflict.  This should only happen if reopening the main screen");
			}
			List.Add(palette.ID, palette);
			if (palette.PaletteParameter != Parameters.Undefined)
				palette.Position.Dock = DockStyle.Right;
			if (palette.Position.DockIndex == 0)
				palette.Position.DockIndex = palette.PalettePurpose.DefaultDockIndex();
			Debug.Assert(DefaultTooltip != null);
			Strings.Translate(palette.Control, DefaultTooltip);
			if (DefaultTooltip != null)
				((IPalette)palette.Control).SetToolTips(DefaultTooltip);
		}

		public static void Deregister(string ID)
		{
			if (!List.ContainsKey(ID))
				Utilities.LogSubError("Palettes.Deregister: unknown ID: " + ID);
			else
			{
				Palette palette = List[ID];
				List.Remove(ID);
				Debug.Assert(palette.UserDefined);
				palette.Form?.Dispose();
				palette.Form = null;
			}
		}

		public static Palette Item(Parameters parameter)
		{
			string key = "Parameter_" + (int) parameter;
			Palette palette;
			if (List.ContainsKey(key))
				palette = List[key];
			else
				palette = List[Globals.Root.CurrentConfig.PaletteSelection(new Purpose(parameter), false)];
			if (palette != null && !palette.IsValid)
				palette = null;
			return palette;
		}

		public static Palette Item(string ID)
		{
			Palette palette = List[ID];
			if (palette != null && !palette.IsValid)
				palette = null;
			return palette;
		}

		/// <summary>Only valid for Palettes with custom IDs - ie ones defined within the software</summary>
		public static Palette Item(Guid ID)
		{
			return Item(ID.ToString());
		}

		public static Palette FindDockedAt(DockStyle dock, int level, int index)
		{
			// finds the item, if any, docked at this position
			foreach (Palette palette in List.Values)
			{
				if (palette.Position.Dock == dock && palette.Position.DockIndex == index && palette.Position.DockLevel == level && palette.IsValid)
					return palette;
			}
			return null;
		}

		#endregion

		public static void UpdateCustomPalettes(AppliedConfig applied)
		{
			// should be called (by frmMain) when the configuration changes.  Adds any new custom palettes to the list.
			// Any custom palettes listed here which are no longer valid are not deleted, rather just marked Valid = false.  The chances are they will come back in scope again
			// if we are moving between user and teacher mode or changing between documents in a multi-document view.  Deleting the palette completely would delete its position information
			// causing it to reappear somewhere else.  So these records are kept.  This is analogous to what happens with the system palettes - the Palette object isn't wiped!
			Dictionary<string, Document> customPalettes = applied.CustomPalettes(true);
			// First check any palettes already in our list, to see if they are still valid
			foreach (Palette palette in List.Values.Where(x => x.UserDefined))
			{
				palette.IsValid = customPalettes.ContainsKey(palette.ID);
			}
			foreach (string custom in customPalettes.Keys)
			{
				if (!List.ContainsKey(custom))
					Register(new Palette(customPalettes[custom]));
			}
		}

		#region Purpose
		/// <summary>Represents the purpose of a palette; it is basically just a number, but can come from 2 different enums.
		/// Implicit operators allow a values from Parameters, Purpose.Specials or a GUID to be provided where a Purpose is expected</summary>
		/// <remarks>Also has limited support for listing custom palettes within custom</remarks>
		public struct Purpose
		{
			private readonly int m_Purpose;
			/// <summary>Undefined unless IsCustom.  Ignored by most of the properties of this</summary>
			/// <remarks>Is used in equality and hash code allowing use for indexing</remarks>
			public readonly Guid CustomID;

			public static Purpose Custom = new Purpose(Specials.Custom); // For convenience a reference which can be used
			public static Purpose None = new Purpose(0);
			// also update IsPopupPad

			public enum Specials // These must not be renumbered as they are stored in data
			{
				// These represent palettes which can be turned on and off, but don't relate to any Parameter (not even one of the dummy Parameter values)
				// Must not clash with Parameters values
				Custom = 1000,
				Transform=1005,
				Alignment,
				Number=1009, // Numbers tools, as opposed to the number pad (was Number in IParameterGUI in version 1)

				Sockets=2002,
				DocumentOutline,
				Rotation,
				CoordEdit,
				Scale
			}

			#region Constructors and conversion to/from Purpose/Special enums
			public Purpose(Specials special)
			{
				m_Purpose = (int)special;
				CustomID = Guid.Empty;
			}

			public static implicit operator Purpose(Specials special)
			{ return new Purpose(special); }

			public Purpose(Parameters parameter)
			{
				m_Purpose = (int)parameter;
				CustomID = Guid.Empty;
			}

			public static implicit operator Purpose(Parameters param)
			{ return new Purpose(param); }

			public Purpose(int i)
			{
				m_Purpose = i;
				CustomID = Guid.Empty;
			}

			public Purpose(Guid idCustom)
			{
				m_Purpose = (int)Specials.Custom;
				CustomID = idCustom;
			}

			public static implicit operator Purpose(Guid idCustom) => new Purpose(idCustom);
			public Parameters Parameter => (Parameters)m_Purpose;
			public Specials Special => (Specials)m_Purpose;

			#endregion

			#region IsXYZ
			public bool IsParameter => m_Purpose < 1000;
			public bool IsCustom => m_Purpose == (int)Specials.Custom;
			public bool IsNone => m_Purpose == 0;

			#endregion

			private static List<Purpose> g_ListPossible;
			public static List<Purpose> PossiblePurposes
			{
				get // Returns list of all possible purposes, for display in the editing screens
				{
					if (g_ListPossible == null)
					{
						g_ListPossible = new List<Purpose>();
						// Not all the Parameter enum actually refer to palettes:
						foreach (Parameters eParameter in new[] {Parameters.Tool, Parameters.FillColour, Parameters.FillPattern,
							Parameters.LineColour, Parameters.LinePattern, Parameters.FontSize, 
							Parameters.ArrowheadEndType})
							g_ListPossible.Add(new Purpose(eParameter));
						foreach (Specials eSpecial in Enum.GetValues(typeof(Specials)))
						{
							g_ListPossible.Add(new Purpose(eSpecial));
						}
					}
					return g_ListPossible;
				}
			}

			[Pure]
			public int ToInt32() => m_Purpose;

			#region Object overrides
			public override bool Equals(object obj)
			{
				if (!(obj is Purpose))
					return false;
				Purpose objPurpose = (Purpose)obj;
				return m_Purpose == objPurpose.ToInt32() && CustomID.Equals(objPurpose.CustomID);
			}

			public override int GetHashCode() => m_Purpose == (int)Specials.Custom ? CustomID.GetHashCode() : m_Purpose.GetHashCode();
			public override string ToString() => IsParameter ? Parameter.ToString() : Special.ToString();

			#endregion

			#region Other info

			public string Name
			{
				get
				{
					switch (Special)
					{
						case Specials.Custom:return Strings.Item("Palette_Custom");
						case Specials.Transform:return Strings.Item("Advanced_Transform");
						case Specials.Alignment:return Strings.Item("Advanced_Alignment");
						case Specials.Number:return Strings.Item("Palette_Numbers");
						case Specials.Scale: return Strings.Item("Palette_ShowScale"); // because Palette_Scale was already used for something else
						default:
							if ((Parameters)Special == Parameters.LinePattern)
								return Strings.Item("Palette_LineStyle"); // the GetParameterTypeName returns "Dot/Dash" pattern which is a bit daft
																		  // (because the palette references 2 parameters, so the parameter name is not quite right in this case)
							else if ((int)Special < 1000)
								return ParameterSupport.GetParameterTypeName(Parameter);
							else
								return Strings.Item("Palette_" + Special);
					}

				}
			}

			/// <summary>Returns the name of the image to be used in the palette header</summary>
			public string ImageName
			{
				get
				{
					if (m_Purpose == (int)Specials.Transform)
						return "Verb_RotateRight";
					if (m_Purpose == (int)Specials.Alignment)
						return "Verb_AlignLeft";
					if (m_Purpose == (int)Specials.Number)
						return "NumberPalette";
					else if (m_Purpose < 1000)
					{
						// Parameters
						return ((Parameters)m_Purpose).ToString();
						//Case Parameters.FillPattern
						//Case Parameters.LineColour
					}
					return "";
				}
			}

			/// <summary>Returns an action which selects a palette of this purpose</summary>
			public Action GetSelectAction()
			{
				//if (!CustomID.IsEmpty())
				//	return  Action.Create(Parameters.Action_Palette, CustomID.ToString());
				return new PaletteAction(this);
			}

			/// <summary>Returns default index when resetting all positions - intended to be same as sequence in v1</summary>
			public int DefaultDockIndex()
			{
				switch ((Parameters)Special)
				{
					case Parameters.LineColour:
						return 1;
					case Parameters.LinePattern:
						return 2;
					case Parameters.FillColour:
						return 3;
					case Parameters.FillPattern:
						return 4;
					case Parameters.TextColour:
						return 5;
					case (Parameters)Specials.Number:
						return 6;
					case (Parameters)Specials.Custom:
						return 4000;
					default:
						return m_Purpose; // most special can appear in the order of the enum
				}
			}
			#endregion

		}
		#endregion

	}


}
