using System;
using System.Collections.Generic;

namespace SAW.Functions
{

	#region Verb enum

	public enum Codes
	{
		// at the moment the numbers are not stored in the data, although names are.  The numbers are used in the configuration screen to divide them up
		// note: '_' cannot be used within verb names
		// numbers are not continuous as they have been kept matching Splash values (which has extra functions)
		None = 0,
		Open,
		Save,
		SaveAs,
		SaveAsWorksheet,
		Print,
		PrintDirectly,
		PrintPreview,
		ExportEMF,
		ExportJPEG,
		ExportPNG,
		ExportPageAsDocument,
		ImportImage,
		NewDocument,
		Start,
		Quit = 17,
		CloseDocument = 19, // will return to menu unless multiples
		ExportSVG = 24,

		Undo = 100, // edits...
		Redo,
		Group,
		Ungroup,
		BringFront,
		SendBack,
		Copy,
		Paste,
		Cut,
		Texture,
		Delete = 111,
		Stack,
		TextSmaller,
		TextLarger,
		DoubleClick,
		BringFrontOneStep,
		SendBackOneStep,
		QuickAddButtons,
		FreeTextToTextLine,
		ConvertToPath = 124,
		//MoveToPixels,
		//TypeDegree,
		MakeChild = 127,
		MoveOutOfContainer,
		ToggleBold=129,
		ToggleUnderline,
		ToggleItalic,


		AlignLeft = 150, // edits, alignment
		AlignCentre,
		AlignRight,
		AlignTop,
		AlignMiddle,
		AlignBottom,
		SpreadHorizontal,
		SpreadVertical,
		EqualiseWidth,
		EqualiseHeight,
		_AfterAlignment = EqualiseHeight + 1, // value must be unused
		FlipHorizontal,
		FlipVertical,
		RotateRight,
		RotateLeft,
		TidyGrid,
		TidyShape,
		TidyAngle, // 167
		NudgeLeft,
		NudgeRight,
		NudgeUp,
		NudgeDown,
		SmallestWidth,
		SmallestHeight,
		LargestWidth,
		LargestHeight,

		AddVertex = 400, // advanced graphics/individual vertex editing
		RemoveVertex,
		ConvertToLine,
		ConvertToBezier,
		SmoothVertex,
		CornerVertex,
		MakeMask,
		RemoveMask,

		ZoomIn = 200, // options
		ZoomOut,
		Zoom100,
		ZoomPage,
		ZoomWidth,
		ConfigUser,
		//ConfigDocument,
		UserUser = 208,
		/// <summary>Display in edit mode - name is left over from Splash </summary>
		UserTeacher,
		StoreStylesAsDefault = 211,
		ClearDefaultStyles,
		ResetPalettes,
		StoreStylesAsUserDefault = 215,
		TogglePrompts = 218,
		ToggleFullScreen = 219,

		PageNext = 300, // not implemented on the menu as a Verb_ because the name changes
		PagePrevious,
		PageDelete,
		PageClear,
		PageMoveDown,
		PageMoveUp,
		PageInsertBefore,
		PageInsertAfter,
		EditPaper,
		PageSize,
		SetOrigin,
		PageDuplicate,
		//DeletePixels,

		// 400 used above
		StartupScript = 500,
		DefaultItemScript,
		DefaultEscapeScript,
		DefaultGroupScript,
		ShowPopup,
		HidePopup,
		ShowAllPopups,
		HideAllPopups,
		SetPromptItem,
		ShowTextSelection,
		HideTextSelection,
		ShowGraphicSelection,
		HideGraphicSelection,
		GridWizard,
		ViewPredictionWords,
		ExportAsText,
		EditItemBounds,
		EditWindowBounds,
		CopyScripts,
		CopyPresentation,
		/// <summary>Update images and/or text using concept coding framework</summary>
		CCFUpdate,
		/// <summary>Specific to SAW - all internal in Splash</summary>
		SaveSettings,
		/// <summary>Specific to SAW - all internal in Splash</summary>
		LoadSettings,
		ImportIRM,
		ExportIRM,
		MakeActive,
		MakeInactive,
		IncludeTextInRotation,
		SAWManual,
		YoctoTest,

		// any verb over 1000 does not (automatically) abandon any ongoing shape
		Choose = 1000, // Drawing shapes...
		Complete,
		Cancel,
		Increment,
		Decrement,
		ChooseExisting, // choose action on existing shapes.  Not direcly mouse click

		// note additions here won't appear in key config - it stops at Decrement

		Up = 1100, // mouse control/navigation while drawing
		Right,
		Down,
		Left,
		MouseSmallStep,
		MouseMediumStep,
		MouseLargeStep,
		RestoreFocus = 1115,
		//SyncCursor,
		//DescribeUnderMouse,

		PageUp = 1150, // scrolling
		PageDown,
		ScrollUp,
		ScrollDown,
		ScrollLeft,
		ScrollRight,

		SelectNext = 1200, // select next shape
		SelectPrevious,
		SelectAll,
		SelectNone,

		MovePalette = 1250, // 1250-1299 for miscellaneous, not listed in groups in key editor
		ResizePalette,

		// no _ allowed !
	}

	#endregion

	/// <summary>Special case of Action for things which can be attached to menus and keys.  Always fully identified by a single numeric code (see Codes enum) with no further parameters</summary>
	public abstract class Verb : Action
	{
		/// <summary>Note code is assigned later - which makes the registration functions easier to write </summary>
		protected Verb() : base(Parameters.Action_Verb)
		{
		}

		/// <summary>Set when the list is being constructed and shouldn't be changed thereafter.  Logically readonly, but making it so is a real pain in terms of the syntax during construction</summary>
		public Codes Code { get; private set; }
		public override string PersistValueText
		{ get { return Code.ToString(); } }

		#region Information
		public virtual bool AbandonsCurrent
		{ get { return (int)Code < 1000; } }

		public virtual bool AutoRefreshAfterTrigger
		{ get { return false; } }

		/// <summary>Should override to true if any updates should be forced thru instantly (applies particularly to the page list which is on a delay) </summary>
		public virtual bool ImmediateRefresh
		{ get { return false; } }

		public override bool HideFromContextMenuIfUnavailable
		{ get { return true; } }

		public override bool IncludeOnContextMenu
		{ get { return true; } }

		public override string DescriptionWithAccelerator(bool forSpeech = false)
		{
			return Strings.Item("Verb_" + Code);
		}

		public override string GetSampleImageResourceID(int size = 32)
		{
			return GUIUtilities.VariableSizeImageResourceID("Verb", "_" + Code, size);
		}

		internal override Shape.Prompt GetPrompt()
		{
			// These use the verb name as the first text, and a second, longer, custom item as the second text
			// the prompt is only displayed if that custom text is defined
			string text = "Verb_Prompt_" + Code;
			if (Strings.Exists(text))
				return new Shape.Prompt(Shape.ShapeVerbs.Info, "Verb_" + Code, "Verb_" + Code, text);
			return null;
		}

		//public override string ToString() => "Verb: " +Code;

		#endregion

		#region Global list

		/// <summary>Must be called once on startup to register all verb objects.  Also call any registration for application specific ones</summary>
		public static void CreateList()
		{
			DrawingVerbs.RegisterDrawingVerbs();
			SelectionVerb.RegisterSelectionVerbs();
			DocumentVerbs.RegisterDocumentVerbs();
			GenericEditVerbs.RegisterVerbs();
			ObjectEdits.RegisterVerbs();
			OptionsVerbs.RegisterVerbs();
			PageVerbs.RegisterVerbs();
			AdvancedGraphics.RegisterVerbs();
		}

		private static Dictionary<Codes, Verb> g_List = new Dictionary<Codes, Verb>();

		/// <summary>Must be called once for each to register the implementation</summary>
		public static void Register(Codes code, Verb implementation)
		{
			implementation.Code = code;
			g_List.Add(code, implementation);
		}

		/// <summary>Version of Register that just takes a trigger function, for an always available verb.  No custom class is required.
		/// Returns the object created so it can be modified</summary>
		public static LambdaVerb Register(Codes code, LambdaVerb.TriggerFunction triggerFunction, bool abandonsCurrent = true, Predicate<EditableView> applicableFunction = null)
		{
			LambdaVerb implementation = new LambdaVerb(triggerFunction, abandonsCurrent, applicableFunction);
			implementation.Code = code;
			g_List.Add(code, implementation);
			return implementation;
		}

		/// <summary>Returns object or null if not found </summary>
		public static Verb Find(Codes code)
		{
			if (!g_List.ContainsKey(code))
				return null;
			return g_List[code];
		}

		#endregion

	}

	/// <summary>Placeholder for missing/unknown verbs </summary>
	class NullVerb : Verb
	{// it's OK to construct these - they are added as SAW/Splash stubs in places.  But they shouldn't be triggered

		public override void Trigger(EditableView.ClickPosition.Sources source, EditableView pnlView, Transaction transaction)
		{
			Utilities.LogSubError("Triggering null verb");
		}
	}

	/// <summary>Wrapper that lets a verb be constructed just with an inline function.  Verb is always available. </summary>
	public class LambdaVerb : Verb
	{
		public delegate void TriggerFunction(EditableView.ClickPosition.Sources source, EditableView pnlView, Transaction transaction);

		private TriggerFunction m_Function;
		private Predicate<EditableView> m_ApplicableFunction;
		public bool MightOpenDialogValue;

		public LambdaVerb(TriggerFunction fn, bool abandonsCurrent, Predicate<EditableView> applicableFunction)
		{
			m_Function = fn;
			AbandonsCurrent = abandonsCurrent;
			m_ApplicableFunction = applicableFunction;
		}

		public override void Trigger(EditableView.ClickPosition.Sources source, EditableView pnlView, Transaction transaction)
		{
			m_Function.Invoke(source, pnlView, transaction);
		}

		public override bool AbandonsCurrent { get; }

		public override bool IsApplicable(EditableView pnlView)
		{
			return m_ApplicableFunction?.Invoke(pnlView) ?? base.IsApplicable(pnlView);
		}

		public override bool MightOpenModalDialog
		{ get { return MightOpenDialogValue; } }
	}

}

