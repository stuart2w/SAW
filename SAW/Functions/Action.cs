using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using SAW.Shapes;

namespace SAW.Functions
{

	/// <summary>A generic action attached to a menu or button.  Persisted as a parameter code and a text value.  The parameter code determines which sub-class is used,
	/// and the sub-class can treat the text data in any way that it likes</summary>
	public abstract class Action
	{
		public readonly Parameters Change;

		public static Action Empty = new NullAction();

		#region Constructors

		/// <summary>Factory method must be used for construction, to ensure correct type is used.  Derived types can be constructed directly </summary>
		public static Action Create(Parameters parameter, string value)
		{
			switch (parameter)
			{
				case Parameters.None:
				case Parameters.Undefined: return new NullAction();
				case Parameters.Action_Verb: return Verb.Find((Codes)Enum.Parse(typeof(Codes), value));
				case Parameters.Action_Key: return new KeyAction((Keys)int.Parse(value));
				case Parameters.Action_Character: return new CharAction(value[0]);
				case Parameters.Action_Text: return new TextAction(value);
				case Parameters.Tool:
					Shape.Shapes shape = (Shape.Shapes)Utilities.IntVal(value);
					return new ToolAction(shape);
				case Parameters.Action_Palette: return new PaletteAction(value);
				case Parameters.Action_Palette_Button: return new PaletteButtonAction(Utilities.IntVal(value));
				case Parameters.Action_ShowPalette: return new ShowPaletteAction(value);
				case Parameters.Action_Snap: return new SnapAction((Shape.SnapModes)Utilities.IntVal(value));
				case Parameters.Action_ShowGrid: return new ShowGridAction(Utilities.IntVal(value));
				default:
					if (parameter >= Parameters.None)
						return new ParameterAction(parameter, Utilities.IntVal(value));
					Utilities.LogSubError($"Unknown parameter code: ({(int)parameter})");
					return new NullAction();
			}
		}

		/// <summary>Factory method must be used for construction, to ensure correct type is used.  Derived types can be constructed directly </summary>
		public static Action Create(Parameters parameter, int value)
		{
			Debug.Assert(parameter != Parameters.Action_Verb, "Functions.Action.New (parameter = Action_Verb) will fail because it stores the value as a number - use the verb constructor");
			return Create(parameter, value.ToString());
		}

		/// <summary>Creates from string persisted in config in form Change=Value</summary>
		public static Action CreateFromConfigString(string config)
		{
			// construct from string stored in configuration, which is concatenation of Change and TextValue separated by =
			if (!config.Contains("="))
			{
				Debug.Fail("Invalid Functions.Action: " + config);
				return new NullAction();
			}
			else
			{
				string[] parts = config.Split(new[] { '=' }, 2);
				return Create((Parameters)Utilities.IntVal(parts[0]), parts[1]);
			}
		}

		public virtual Action Clone() => Create(Change, PersistValueText);

		protected Action(Parameters eParameter)
		{
			Change = eParameter;
		}

		public static Action CreateForShape(Shape.Shapes shape, bool transient) => Create(Parameters.Tool, ((int)shape).ToString());

		#endregion

		public abstract void Trigger(ClickPosition.Sources source, EditableView pnlView, Transaction transaction);

		#region Values and object stuff
		public abstract string PersistValueText { get; }

		public override sealed string ToString() => Convert.ToInt32(Change) + "=" + PersistValueText;

		public bool IsEmpty
		{
			[DebuggerStepThrough()]
			get { return Change == Parameters.None; }
		}

		[DebuggerStepThrough]
		public override bool Equals(object obj)
		{
			if (!(obj is Action))
				return false;
			Action a = (Action)obj;
			return Change == a.Change && PersistValueText == a.PersistValueText;
		}

		public override int GetHashCode() => Change.GetHashCode() ^ PersistValueText.GetHashCode();

		#endregion

		#region Display - Description and sample images
		[DebuggerStepThrough]
		public string DescriptionWithoutAccelerator(bool forSpeech = false)
		{
			// description with any & indicating accelerator key removed.
			return DescriptionWithAccelerator(forSpeech).Replace("&", "");
		}

		/// <summary>Description which MIGHT contain ampersand indicating accelerator key.  If forSpeech then a simplified version is returned in some cases</summary>
		public abstract string DescriptionWithAccelerator(bool forSpeech = false);

		public Bitmap CreateSampleImage(int size = 32)
		{
			string resourceID = GetSampleImageResourceID(size);
			if (!string.IsNullOrEmpty(resourceID))
				return (Bitmap)GUIUtilities.VariableSizeImage(resourceID, "", size); // RM.GetObject(strResourceID)
			return CreateSampleImage2(size);
		}

		/// <summary>returns a bitmap which can be used as a sample of this parameter value the caller should dispose of it</summary>
		protected virtual Bitmap CreateSampleImage2(int size) => null;

		#endregion

		#region Binary persistence - not used in configuration
		//  not used in the configuration, which stores them as strings in its general dictionary.  This is used by ButtonShape however
		public virtual void Write(DataWriter writer)
		{
			writer.Write((int)Change);
			writer.WriteBufferedString(PersistValueText); // Mainly because this handles Nothing
		}

		public static Action Read(DataReader reader)
		{
			return Create((Parameters)reader.ReadInt32(), reader.ReadBufferedString());
		}

		#endregion

		#region Information
		public virtual bool IsApplicable(EditableView pnlView) => true;

		/// <summary>Verb overrides this to default to true.  Not sure for base - probably no actions anyway on these </summary>
		public virtual bool HideFromContextMenuIfUnavailable => false;

		public virtual bool IncludeOnContextMenu => false;

		/// <summary>returns true if action might open modal dialog</summary>
		public virtual bool MightOpenModalDialog => false;

		/// <summary>Returns the resource ID of the sample for this action if there is one, otherwise Nothing
		/// this can return Nothing even if CreateImage Would Return an Image.  Only resources within the AM resource set are valid
		/// Although it is nice to refer to resources using compiled names, using the strings here allows them to be stored in documents </summary>
		public virtual string GetSampleImageResourceID(int size = 32) => "";

		internal virtual Shape.Prompt GetPrompt() => null;

		#endregion

		#region Protected properties giving compatibility to frmMain

		protected static Document CurrentDocument => Globals.Root.CurrentDocument;
		private protected static frmMain Editor => Globals.Root.Editor;
		protected static int CurrentPageIndex => Globals.Root.CurrentPageIndex;
		protected static Page CurrentPage => Globals.Root.CurrentPage;

		#endregion

	}

	/// <summary>Base for any Action that can be currently "selected" in the UI, based on a (integer) value in a parameter </summary>
	internal abstract class ValueSelectableAction : Action
	{
		protected ValueSelectableAction(Parameters eParameter) : base(eParameter)
		{ }

		public abstract int ValueAsInteger { get; }

	}

	internal class NullAction : Action
	{
		public NullAction() : base(Parameters.None)
		{
		}

		public override string PersistValueText => "";

		public override void Trigger(ClickPosition.Sources source, EditableView pnlView, Transaction transaction)
		{// not an error condition?
		}

		#region Info
		public override bool IsApplicable(EditableView pnlView) => false;
		public override string DescriptionWithAccelerator(bool forSpeech = false) => Strings.Item("Config_KeyNone");

		#endregion
	}

	internal class ToolAction : ValueSelectableAction
	{

		#region Values construct and persist
		public readonly Shape.Shapes Tool;

		public ToolAction(Shape.Shapes tool) : base(Parameters.Tool)
		{
			Tool = tool;
		}

		public override string PersistValueText => ((int)Tool).ToString();
		public override int ValueAsInteger => (int)Tool;

		#endregion

		public override void Trigger(ClickPosition.Sources source, EditableView pnlView, Transaction transaction)
		{
			Editor.ChangeTool(Tool);
		}

		#region Info
		public override bool IsApplicable(EditableView pnlView) => Globals.Root.CurrentConfig.ShapeEnabled(Tool);
		public override string DescriptionWithAccelerator(bool forSpeech = false) => Strings.Item("Shape_" + Tool);
		public override string GetSampleImageResourceID(int size = 32) => GUIUtilities.ShapeImageResourceID(Tool, size);

		protected override Bitmap CreateSampleImage2(int size)
		{
			Debug.Fail("CreateSampleImage should have been handled via resource ID");
			return null;
		}

		internal override Shape.Prompt GetPrompt()
		{
			string strExplainID = "Shape_" + Tool + "_Subtext";
			return new Shape.Prompt(Shape.ShapeVerbs.Info, "Shape_" + Tool, "TOOLIMG_64_" + Tool.ToString().ToUpper(), Strings.Exists(strExplainID) ? strExplainID : "");
		}

		//public override string ToString() => Tool.ToString();

		#endregion

	}

	internal class SnapAction : ValueSelectableAction
	{
		#region Values construct and persist

		public readonly Shape.SnapModes Mode;

		public SnapAction(Shape.SnapModes eSnap) : base(Parameters.Action_Snap)
		{
			Mode = eSnap;
		}

		public override string PersistValueText => ((int)Mode).ToString();

		#endregion

		public override void Trigger(ClickPosition.Sources source, EditableView pnlView, Transaction transaction)
		{
			Editor.UserChangeSnapMode(Mode);
		}

		#region Info
		public override int ValueAsInteger => (int)Mode;

		public override string DescriptionWithAccelerator(bool forSpeech = false) => Strings.Item("Action_Snap_" + Mode);

		public override string GetSampleImageResourceID(int size = 32)
		{
			switch (Mode)
			{
				case Shape.SnapModes.Angle: return "AngleSnap";
				case Shape.SnapModes.Shape: return "ShapeSnap";
				case Shape.SnapModes.Grid: return "GridSnap";
				case Shape.SnapModes.Off: return "NoSnap";
				default:
					Debug.Fail(" Unexpected snapping mode in Config.Action.CreateSampleImage: " + Mode);
					return "";
			}
		}

		protected override Bitmap CreateSampleImage2(int size)
		{
			Debug.Fail("CreateSampleImage should have been handled via resource ID");
			return null;
		}
		#endregion

	}

	internal class ShowGridAction : ValueSelectableAction
	{

		#region Values construct and persist
		public readonly int Value;

		public ShowGridAction(int value) : base(Parameters.Action_ShowGrid)
		{
			Value = value;
		}

		public override string PersistValueText => Value.ToString();

		#endregion

		public override int ValueAsInteger => Value;

		public override void Trigger(ClickPosition.Sources source, EditableView pnlView, Transaction transaction)
		{
			Editor.ChangeGridVisible(Value == 1, true); // nominally value 1 is on, 0 off, although function will actually ignore the value
		}

		#region Info
		public override string GetSampleImageResourceID(int size = 32)
			=> GUIUtilities.VariableSizeImageResourceID("ShowGrid", "", size);

		protected override Bitmap CreateSampleImage2(int size)
		{
			Debug.Fail("CreateSampleImage should have been handled via resource ID");
			return null;
		}

		public override string DescriptionWithAccelerator(bool forSpeech = false) => Strings.Item("Action_ShowGrid");

		#endregion

	}

	internal class PaletteAction : Action
	{

		#region Values construct and persist
		public readonly Palette.Purpose Purpose;

		public PaletteAction(Palette.Purpose purpose) : base(Parameters.Action_Palette)
		{
			Purpose = purpose;
		}

		public override string PersistValueText => Purpose.IsCustom ? Purpose.CustomID.ToString() : Purpose.ToInt32().ToString();

		public PaletteAction(string persist) : base(Parameters.Action_Palette)
		{
			int id;
			if (int.TryParse(persist, out id))
				Purpose = new Palette.Purpose(id);
			else
				Purpose = new Palette.Purpose(new Guid(persist));
		}

		#endregion

		public override void Trigger(ClickPosition.Sources source, EditableView pnlView, Transaction transaction)
		{
			Editor.TriggerPalette(Purpose, true);
		}

		public override string DescriptionWithAccelerator(bool forSpeech = false)
			=> Strings.Item("Config_SwitchToPalette") + " (" + Purpose.Name + ")";

	}

	/// <summary>Toggles the display of the given palette </summary>
	internal class ShowPaletteAction : Action
	{

		#region Values construct and persist
		public readonly Palette.Purpose Purpose;

		public ShowPaletteAction(Palette.Purpose purpose) : base(Parameters.Action_ShowPalette)
		{
			Purpose = purpose;
		}

		public override string PersistValueText => Purpose.IsCustom ? Purpose.CustomID.ToString() : Purpose.ToInt32().ToString();

		public ShowPaletteAction(string persist) : base(Parameters.Action_ShowPalette)
		{
			int id;
			if (int.TryParse(persist, out id))
				Purpose = new Palette.Purpose(id);
			else
				Purpose = new Palette.Purpose(new Guid(persist));
		}

		#endregion

		public override void Trigger(ClickPosition.Sources source, EditableView pnlView, Transaction transaction)
		{
			string key = Config.ShowPaletteKey(Purpose);
			Config.UserCurrent.Write(key, !Config.UserCurrent.ReadBooleanEx(key));
			Globals.OnSettingsChanged();
		}

		public override string DescriptionWithAccelerator(bool forSpeech = false) => Strings.Item("Action_ShowPalette", Purpose.Name);


	}

	/// <summary>Used internally in colour panel palette </summary>
	internal class PaletteButtonAction : Action
	{

		#region Values construct and persist
		public readonly int Index;

		public PaletteButtonAction(int index) : base(Parameters.Action_Palette_Button)
		{
			Index = index;
			Debug.Assert(index != 3); // in splash, but not SAW
		}

		public override string PersistValueText => Index.ToString();

		#endregion

		public override void Trigger(ClickPosition.Sources source, EditableView pnlView, Transaction transaction)
		{
			// passed to the current colour panel if any
			Control ctrFocused = GUIUtilities.GetFocusControl();
			(ctrFocused as ColourPanel)?.TriggerAction(this);
		}

		#region Info

		public override bool MightOpenModalDialog
		{
			get
			{
				switch (Index)
				{
					case 1: // HandleClick(MOREBUTTON)
						return true;
					case 2: // HandleClick(TRANSPARENTBUTTON)
						return true;
				}
				return false;
			}
		}

		public override string DescriptionWithAccelerator(bool forSpeech = false)
		{
			switch (Index)
			{
				case 1: return Strings.Item("Config_Keys_Palette_Refine");
				case 2: return Strings.Item("Config_Keys_Palette_Transparent");
				default:
					Debug.Fail("Unexpected Parameter.Action_Palette_Button Value");
					return Index.ToString();
			}
		}

		#endregion
	}


}
