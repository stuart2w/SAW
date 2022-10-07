using System.Collections.Generic;
using System;
using System.Drawing;
using System.Diagnostics;
using System.Windows.Forms;
using System.Linq;
using System.ComponentModel;
using System.Text;
using Microsoft.Win32;
using SAW.Functions;
using Action = SAW.Functions.Action;
using SAW.Shapes;

namespace SAW
{
	/// <summary>Clearing house whereby disparate parts of the S/W can communicate without a direct reference to each other.
	/// Many of the more common events on here are delayed on a timer</summary>
	public static class Globals
	{
		/// <summary>The root application object.  Responsible for list of documents etc.
		/// MUST be nothing in Design mode (some controls within controls check for this since DesignMode doesn't work for them)</summary>
		public static RootApplication Root { get; internal set; }

		private static readonly Timer m_tmr;
		private static frmMain Editor; // defined by menu when it is created

		/// <summary>True if this is not running as its own application, but InitialiseExternal has been called </summary>
		internal static bool IsEmbedded;

		static Globals()
		{
			m_tmr = new Timer { Interval = 100, Enabled = false };
			m_tmr.Tick += SendDelayedEvents;
		}

		#region Simple Events
		private static bool m_SettingsChanged = true;
		/// <summary>Fired when applicable settings (in main screen) changed - can be new document, change user mode etc...</summary>
		public static event NullEventHandler SettingsChanged;

		/// <summary>Automatically called if document or user changes; this sets some global variables, and ON A DELAY raises the event which triggers the necessary changes in the main screen</summary>
		public static void OnSettingsChanged()
		{
			// The event must be delayed.  Changing document relies on this; this is called first and then the document changed event is raised.  But we want the document
			// event first, so that the settings apply to the right document.  But the document changed would like AM.CurrentConfig to be correct
			Root.CurrentConfig = Root.GenerateAppliedConfig(Root.User, Root.CurrentDocument);
			m_SettingsChanged = true;
			m_KeyApplicableChanged = true;
			m_tmr.Enabled = true;
		}

		public delegate void RequestCloseEventHandler(CancelEventArgs e);
		public static event RequestCloseEventHandler RequestClose;

		/// <summary>Checks if it is OK to close all current document.  Returns false to cancel</summary>
		public static bool DoRequestClose()
		{
			CancelEventArgs e = new CancelEventArgs();
			RequestClose?.Invoke(e);
			return !e.Cancel;
		}

		/// <summary>Triggered when the possible activities (or their text/icon) is changed.  GUIs should invalidate any displayed lists of activities</summary>
		public static event NullEventHandler AvailableActivitiesChanged;

		public static void OnAvailableActivitiesChanged()
		{
			AvailableActivitiesChanged?.Invoke();
		}

		/// <summary>Raised in the size of the page currently being edited is changed.  Not raised when changing between different pages, only when the page is edited</summary>
		public static event NullEventHandler CurrentPageSizeChanged;

		public static void OnCurrentPageSizeChanged()
		{
			CurrentPageSizeChanged?.Invoke();
		}

		/// <summary>Arguments for any event with a Transaction parameter </summary>
		public class TransactionEventArgs : EventArgs
		{
			public Transaction Transaction;
			/// <summary>True if this is an Undo operation </summary>
			public bool Undo;
		}
		/// <summary>Invoked immediately at the end of StoreTransaction.  Sender is always null</summary>
		public static event EventHandler<TransactionEventArgs> TransactionStored;

		public static void OnTransactionStored(Transaction t, bool undo = false)
		{
			TransactionStored?.Invoke(null, new TransactionEventArgs() { Transaction = t, Undo = undo });
		}

		private static bool m_UpdateInfo = false;

		public static void OnUpdateInfo()
		{
			m_UpdateInfo = true;
			m_tmr.Enabled = true;
		}

		/// <summary>Invoked when diagnostic or status info might need to be updated.  Not necessarily invoked if a transaction was stored.
		/// This is used to notify smaller changes (eg during dragging) that might require an update of status panels</summary>
		public static event NullEventHandler UpdateInfo;

		public static event NullEventHandler RotationInfoChanged;

		/// <summary>Fires event synchronously.  Used when current rotation point is changed, or the rotate about centres/point is changed
		/// But not when page is changed (which implies a change of rotation point)</summary>
		public static void OnRotationInfoChanged()
		{
			RotationInfoChanged?.Invoke();
		}

		#endregion

		#region Misc functions

		/// <summary>Set to the targeted control when using right click to speak.  We should attempt to ignore any Click from this control</summary>
		internal static Control DiscardClickFrom;

		/// <summary>Checks of this control should have any clicks discarded, returning true if Click should be ignored, and either way clears DiscardClickFrom</summary>
		/// <remarks>The intention is that this can be called on the click event.  It clears the Discard setting because if anything else is clicked it implies
		/// we're not still receiving the matching Click from the attempt to speak</remarks>
		internal static bool CheckDiscardClick(Control control)
		{
			var old = DiscardClickFrom;
			DiscardClickFrom = null;
			return control == old;
		}

		internal static void RestoreFocus()
		{
			(Root.CurrentMainScreen() as frmMain)?.pnlView.Focus();
		}

		internal static void SetFontAsDefault(Font font)
		{
			(Root.CurrentMainScreen() as frmMain)?.SetFontAsDefault(font);
		}

		public static List<Shape> SelectedOrCurrentShapes()
		{
			if (Editor == null)
				return new List<Shape>();
			return Editor.SelectedOrCurrentShapes;
		}

		public static void AbandonOngoing(bool neverAutoStore)
		{
			Editor?.pnlView.ConcludeOngoing(neverAutoStore);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		internal static void SetEditor(frmMain frm)
		{
			Editor = frm;
			UseParameterScreen(Editor);
		}

		/// <summary>There is a problem using the Windows button renderer within palettes under classic theme (probably because we are not using pixel scale)</summary>
		internal static bool ClassicTheme = false;
		internal static void CheckThemeOnStartup()
		{
			try
			{
				var key = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\ThemeManager");
				var value = key.GetValue("ThemeActive");
				if (value == null || value.ToString() != "1")
				{
					ClassicTheme = true;
					Root.Log.WriteLine("Detected Windows classic theme on startup");
				}
			}
			catch (Exception ex)
			{
				Root.Log.WriteLine("Failed to test Windows theme on startup: " + ex.Message);
			}
		}

		#endregion

		#region Prompts
		/// <summary>Fired when the requested list changes.  Param may be empty</summary>
		internal delegate void CurrentPromptsChangedEventHandler(List<Shape.Prompt> list);
		private static CurrentPromptsChangedEventHandler CurrentPromptsChangedEvent;

		internal static event CurrentPromptsChangedEventHandler CurrentPromptsChanged
		{
			add
			{
				CurrentPromptsChangedEvent = (CurrentPromptsChangedEventHandler)Delegate.Combine(CurrentPromptsChangedEvent, value);
			}
			remove
			{
				CurrentPromptsChangedEvent = (CurrentPromptsChangedEventHandler)Delegate.Remove(CurrentPromptsChangedEvent, value);
			}
		}

		/// <summary>the prompts requested by the current shape </summary>
		private static List<Shape.Prompt> m_ShapePrompts;
		/// <summary>the prompts requested by whatever the user is hovering over.
		/// As either changes it takes priority.  either list may be nothing meaning that nothing is currently appropriate </summary>
		private static List<Shape.Prompt> m_HoverPrompts;

		/// <summary>Displays prompts for current activity - not just mouse hovering over something</summary>
		internal static void DisplayPrompts(List<Shape.Prompt> prompts)
		{
			if (prompts != null && prompts.Count > 0)
				m_HoverPrompts = null;
			m_ShapePrompts = prompts;
			SetPrompts();
		}

		internal static void SetHover(List<Shape.Prompt> prompts)
		{
			m_HoverPrompts = prompts;
			SetPrompts();
		}

		internal static void ClearHover()
		{
			m_HoverPrompts = null;
			SetPrompts();
		}

		internal static void SetHover(Action action)
		{
			Shape.Prompt prompt = action.GetPrompt();
			if (prompt != null)
			{
				m_HoverPrompts = new List<Shape.Prompt>();
				m_HoverPrompts.Add(prompt);
			}
			else
				m_HoverPrompts = null;
			SetPrompts();
		}

		internal static void SetHoverPrompt(string textID, string imageID, string secondText = "")
		{
			m_HoverPrompts = new List<Shape.Prompt> { new Shape.Prompt(Shape.ShapeVerbs.Info, textID, imageID, secondText) };
			SetPrompts();
		}

		internal static void SetHoverPrompt(Shape.Prompt prompt)
		{
			m_HoverPrompts = new List<Shape.Prompt> { prompt };
			SetPrompts();
		}

		private static void SetPrompts()
		{
			// usually only one of the two is set; if both are set Hover takes priority (setting this doesn't actually clear the shapes list)
			// we don't need to cache the prompts particularly, because the assignment below checks if they have actually changed
			if (m_HoverPrompts != null)
				CurrentPromptsChangedEvent?.Invoke(m_HoverPrompts);
			else
				CurrentPromptsChangedEvent?.Invoke(m_ShapePrompts);
		}

		#endregion

		#region Parameter
		// this mostly just calls through to frmMain.  The advantage of having it here is partly removing the clumsy references to AM.ParameterGUI, and also
		// this will function when the form has not been created - by showing all as inapplicable with no value
		// and in future the implementation could change further.  For the parameter values it is possible to register a different screen to receive them

		// this also is the gateway for key applicability, which is only used intermittently, and values are not stored.
		// Key applicability mainly used for custom palettes which function by sending keys

		public static event NullEventHandler ApplicabilityChanged; // (of parameters) the parameter is not specified, as usually most will change at once

		public delegate void ParameterChangedEventHandler(Parameters parameter);
		public static event ParameterChangedEventHandler ParameterChanged;

		/// <summary>Delayed notification that applicable keys may have changed.  Only sent on selection change and sometimes by individual shapes.</summary>
		/// <remarks>is a separate event as in practice few controls listen to this - only PaletteView.  Also set by OnSettingsChanged</remarks>
		public static event NullEventHandler KeyApplicabilityChanged;
		public static event NullEventHandler VerbApplicabilityChanged;

		private static readonly bool[] m_aApplicable = new bool[(int)Parameters.Toggle_Last + 1]; // whether the parameters are currently applicable.  Starts at true
		private static bool m_ApplicableChanged = true; // we record when the applicable values actually change to avoid raising excessive events, which could have dozens (at least) of listeners
		private static readonly HashSet<Parameters> m_ParametersChanged = new HashSet<Parameters>();
		private static readonly Stack<IParameterConsumer> m_StackParameterScreens = new Stack<IParameterConsumer>(); // Previous values of m_ParameterScreen
		private static IParameterConsumer m_ParameterScreen; // Where parameter values are currently directed to (main screen, but can be replaced by modal dialogs)
		private static bool m_KeyApplicableChanged;
		private static bool m_VerbApplicabilityChanged;

		/// <summary>Assign whether param applicable (used by frmMain).  On changes event will be raised shortly afterwards automatically on a timer</summary>
		public static void SetApplicable(bool value, Parameters parameter)
		{
			if (m_aApplicable[(int)parameter] == value) return;
			m_ApplicableChanged = true;
			m_aApplicable[(int)parameter] = value;
			m_tmr.Enabled = true;
		}

		public static int ParameterValue(Parameters parameter)
		{
			if (m_ParameterScreen == null)
				return 0;
			return m_ParameterScreen.ParameterValue(parameter);
		}
		public static void SetParameterValue(int value, Parameters parameter)
		{
			m_ParameterScreen?.SetParameterValue(value, parameter);
		}

		public static void OnParameterChanged(Parameters parameter)
		{
			m_ParametersChanged.Add(parameter);
			m_tmr.Enabled = true;
		}

		internal static void UseParameterScreen(IParameterConsumer screen)
		{
			if (m_ParameterScreen != null)
				m_StackParameterScreens.Push(m_ParameterScreen);
			m_ParameterScreen = screen;
		}

		internal static void RemoveParameterScreen(IParameterConsumer screen)
		{
			if (screen != m_ParameterScreen)
				throw new ArgumentException();
			m_ParameterScreen = m_StackParameterScreens.Count == 0 ? null : m_StackParameterScreens.Pop();
		}

		/// <summary>returns which of the default style objects applicable to this parameter.  The value can then be read using StyleBase.ParameterValue</summary>
		internal static Shape.StyleBase StyleParameterDefaultObject(Parameters parameter)
		{
			return Editor?.StyleParameterDefaultObject(parameter);
		}

		public static bool ParameterApplicable(Parameters eParameter)
		{
			if (Editor == null)
				return false;
			switch (eParameter)
			{
				case Parameters.Tool:
				case Parameters.Action_ShowGrid:
				case Parameters.Action_Snap:
					return true;
				default:
					if (eParameter >= Parameters.FirstStyle && eParameter <= Parameters.LastStyle)
						return m_aApplicable[(int)eParameter];
					else
					{
						Debug.Fail("Unexpected parameter in View.ParameterApplicable");
						return true;
					}
			}
		}

		/// <summary>Will trigger the applicable changed event.  NOT USUALLY NEEDED - this usually happens automatically via setting applicability of a parameter</summary>
		public static void OnApplicableChanged()
		{
			m_ApplicableChanged = true;
			m_tmr.Enabled = true;
		}

		#endregion

		/// <summary>Raises any deferred events notifying parameter changes or settings changes.  Usually triggered internally on a timer, but can be publicly triggered to force changes to happen immediately</summary>
		public static void SendDelayedEvents(object sender, EventArgs e)
		{
			m_tmr.Enabled = false;
			if (m_ApplicableChanged)
			{
				m_ApplicableChanged = false;
				//Debug.WriteLine("Applicable changed")
				ApplicabilityChanged?.Invoke();
			}
			var parameters = m_ParametersChanged.ToList(); // take a copy in case of sync issues
			m_ParametersChanged.Clear();
			foreach (Parameters parameter in parameters)
			{
				//Debug.WriteLine("Sending param change: " + parameter.ToString)
				ParameterChanged?.Invoke(parameter);
			}
			if (m_SettingsChanged)
			{
				m_SettingsChanged = false;
				m_VerbApplicabilityChanged = false; // We don't raise both events.  Handlers should assume that all verbs change if settings change
				SettingsChanged?.Invoke();
			}
			if (m_KeyApplicableChanged)
			{
				m_KeyApplicableChanged = false;
				KeyApplicabilityChanged?.Invoke();
			}
			if (m_VerbApplicabilityChanged)
			{
				m_VerbApplicabilityChanged = false;
				VerbApplicabilityChanged?.Invoke();
			}
			// from error handling
			if (m_InvalidDocument)
			{
				Root.Log.WriteLine("Globals raising InvalidCurrentDocument");
				m_InvalidDocument = false;
				InvalidCurrentDocument?.Invoke();
			}
			if (m_InvalidState)
			{
				Root.Log.WriteLine("Globals raising InvalidCurrentState");
				m_InvalidState = false;
				InvalidCurrentState?.Invoke();
			}
			if (m_UpdateInfo)
			{
				m_UpdateInfo = false;
				UpdateInfo?.Invoke();
			}
			lock (m_Queued)
				while (m_Queued.Any())
				{
					m_Queued.Dequeue().Invoke();
				}
		}

		private static Queue<System.Action> m_Queued = new Queue<System.Action>();
		/// <summary>Queues the action to run when the Windows message pump fires timers </summary>
		public static void QueueDelayedAction(System.Action a)
		{
			lock (m_Queued)
				m_Queued.Enqueue(a);
			m_tmr.Enabled = true;
		}

		#region Other action stuff
		/// <summary>Raises the KeyApplicabilityChanged on a delay.  Multiple calls collapsed into one; so can safely be called repeatedly</summary>
		/// <remarks>Called by frmMain when selection changes (easier than adding lines to every place in Page where it raises the SelectionChanged event!)</remarks>
		public static void NotifyKeyApplicabilityChanged()
		{
			m_KeyApplicableChanged = true;
			m_tmr.Enabled = true;
		}

		/// <summary>Erases the VerbApplicabilityChanged on a delay.  This was effectively frmMain.VerbButtonsUpdateEnabled previously.  This is not raised if SettingsChanged was raised</summary>
		public static void NotifyVerbApplicabilityChanged()
		{
			m_VerbApplicabilityChanged = true;
			m_tmr.Enabled = true;
		}

		/// <summary>Checks with editor, which mainly asks the current shape.  Default is always to true</summary>
		public static bool IsKeyApplicable(char ch)
		{
			if (Editor == null)
				return true;
			if (!Editor.pnlView.Focused)
				return true; // assume all keys OK when outside the work area
			return Editor.pnlView.IsKeyApplicable(ch);
		}

		/// <summary>Checks with editor, which mainly asks the current shape.  Default is always to true</summary>
		public static bool IsKeyApplicable(Keys key)
		{
			if (Editor == null)
				return true;
			if (!Editor.pnlView.Focused)
				return true; // assume all keys OK when outside the work area
			return Editor.pnlView.IsKeyApplicable(key);
		}

		public static bool ActionApplicable(Action action)
		{
			return action.IsApplicable(Editor?.pnlView);
		}

		public static bool VerbApplicable(Codes code)
		{
			return Editor?.VerbApplicable(code) ?? false;
		}

		public static void PerformAction(Action action, ClickPosition.Sources source)
		{
			Editor?.PerformAction(action, source);
		}

		#endregion

		#region Error handling and reporting
		private static bool m_InvalidState;
		private static bool m_InvalidDocument;
		internal static event NullEventHandler InvalidCurrentState;
		internal static event NullEventHandler InvalidCurrentDocument;

		/// <summary>Tells the GUI to abort current operations - something is wrong</summary>
		internal static void NotifyInvalidCurrentState()
		{
			Root.Log.WriteLine("Globals.InvalidCurrentState");
			m_InvalidState = true;
			m_tmr.Enabled = true;
		}

		internal static void NotifyInvalidDocument()
		{
			Root.Log.WriteLine("Globals.NotifyInvalidDocument");
			m_InvalidDocument = true;
			m_tmr.Enabled = true;
		}

		private static readonly Queue<DiagnosticEvent> g_Events = new Queue<DiagnosticEvent>();
		public struct DiagnosticEvent
		{
			public readonly string Text;
			public readonly int Ticks;

			public DiagnosticEvent(string text)
			{
				Text = text;
				Ticks = Environment.TickCount;
			}
		}

		internal static void StoreEvent(string text)
		{
			g_Events.Enqueue(new DiagnosticEvent(text));
			//Debug.WriteLine(strText);
			while (g_Events.Count > 20)
			{
				g_Events.Dequeue();
			}
		}

		internal static void WriteDiagnosticEvents(DataWriter writer)
		{
			writer.Write(1); // Can be used for future expansion without changing the main data file versions
			writer.Write(g_Events.Count);
			foreach (DiagnosticEvent diagEvent in g_Events)
			{
				writer.Write(diagEvent.Text);
				writer.Write(diagEvent.Ticks);
			}
		}

		/// <summary>Returns text summary of the events stored in an error report</summary>
		internal static string ReadDiagnosticEvents(DataReader reader)
		{
			Debug.Assert(reader.ReadInt32() == 1);
			var count = reader.ReadInt32();
			StringBuilder output = new StringBuilder();
			for (int index = 0; index <= count - 1; index++)
			{
				var text = reader.ReadString();
				var ticks = reader.ReadInt32();
				output.Append("@").Append(ticks % 10000).Append(": ").AppendLine(text);
			}
			return output.ToString();
		}

		// support for reporting non-fatal errors in large operations.  Not implemented yet

		/// <summary>Reports an error which may be non-fatal in some cases</summary>
		/// <param name="text">Translatable text</param>
		/// <param name="throwOutsideOperation">If true this throws an exception if not in a context where a group of non-fatal errors can be reported</param>
		internal static void NonFatalOperationalError(string text, bool throwOutsideOperation = true)
		{
			text = Strings.Translate(text);
			Root.Log.WriteLine("Op error: " + text);
			if (CurrentOperation != null)
			{
				if (OperationErrors.ContainsKey(text))
					OperationErrors[text] += 1;
				else
					OperationErrors.Add(text, 1);
			}
			else if (throwOutsideOperation)
				throw new UserException(text);
		}

		private static Operation CurrentOperation;
		private static readonly Dictionary<string, int> OperationErrors = new Dictionary<string, int>();

		/// <summary>Instantiate one of these to encapsulate a larger operation that can survive minor errors</summary>
		internal class Operation : IDisposable
		{
			private readonly string Name;

			public Operation(string name)
			{
				Name = Strings.Translate(name);
				if (CurrentOperation != null)
					throw new Exception("Overlapping operations not permitted");
				CurrentOperation = this;
				OperationErrors.Clear();
			}

			public void Dispose()
			{
				CurrentOperation = null;
			}

			/// <summary>Returns true if there are no errors, or the user chooses to ignore them</summary>
			public bool ConfirmSuccess(bool noAsserts = false)
			{
				if (OperationErrors.Count == 0)
					return true;
				StringBuilder list = new StringBuilder();
				foreach (var error in OperationErrors)
				{
					list.Append(error.Value).Append("*").AppendLine(error.Key);
				}
				Debug.Assert(noAsserts, "Operation errors" + list);
				return frmReportOpErrors.Display(list.ToString(), Name);
			}

		}

		#endregion
	}

}
