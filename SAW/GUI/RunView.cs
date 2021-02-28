using System;
using System.Collections.Generic;
using System.Drawing;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using Blade;
using SAW.Commands;
using Switches.Engines;
using Switches.Switching;

namespace SAW
{
	/// <summary>The document view used to run the selection set for the user.  EditView does the editing version (both inherit from StaticView)</summary>
	public class RunView : StaticView
	{
		private Switches.Engine m_Engine;
		public string PreviousSelectionSet = "";

		#region Construct, dispose and sizing

		public RunView()
		{
			KeySend.Instance.ExtraKeyForPredictions += KeySend_ExtraKeyForPredictions;
			m_RepeatTimer = new Timer();
			m_RepeatTimer.Tick += m_RepeatTimer_Tick;
			m_ZoomTimer = new Timer();
			m_ZoomTimer.Interval = 100;
			m_ZoomTimer.Enabled = true;
			m_ZoomTimer.Tick += m_ZoomTimerTick;
			Globals.SettingsChanged += Engine_SettingsChanged;
		}

		private void SetEngine(Switches.Engine engine)
		{
			if (m_Engine != null)
			{
				m_Engine.Iterate -= m_Engine_Iterate;
				m_Engine.Trigger -= m_Engine_Trigger;
				m_Engine.EndSelect -= m_Engine_EndSelect;
				m_Engine.Dispose();
			}
			m_Engine = engine;
			if (m_Engine != null)
			{
				m_Engine.Iterate += m_Engine_Iterate;
				m_Engine.Trigger += m_Engine_Trigger;
				m_Engine.EndSelect += m_Engine_EndSelect;
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (m_RepeatTimer != null)
				m_RepeatTimer.Tick -= m_RepeatTimer_Tick;
			m_RepeatTimer?.Dispose();
			if (m_ZoomTimer != null)
				m_ZoomTimer.Tick -= m_ZoomTimerTick;
			m_ZoomTimer?.Dispose();
			EnsureCharacterHandlerRemoved();
			SetEngine(null);
			KeySend.Instance.ExtraKeyForPredictions -= KeySend_ExtraKeyForPredictions;
			Globals.SettingsChanged -= Engine_SettingsChanged;
			base.Dispose(disposing);
		}

		public override void DisplayPage(Page page, Document document)
		{ // note this may happen while scanning
			base.DisplayPage(page, document);
			this.ChangeZoom((float)SpecialZooms.FitPage);
			SelectInitialItem();
			if (IsScanning)
				StartScanOnDocument();
		}

		private Timer m_ZoomTimer; // used for some things which run continously

		private void m_ZoomTimerTick(object sender, EventArgs e)
		{
			if (m_Page == null)
				return;
			if (m_RezoomNeeded)
				ChangeZoom(m_SpecialZoom != SpecialZooms.None ? (float)m_SpecialZoom : m_Zoom);
		}

		#endregion

		#region Start, stop

		public void StartScan()
		{ // only called when run mode initially starts
			ResetBlade();
			try
			{
				PointerSwitch.View = this;
				SetEngine(Switches.Engine.PrepareSwitchingFromConfig());
				m_MouseInput = (m_Engine is PointerEngine);
				m_Engine.StartScanning();

				PhysicalSwitch.AddHotKey(Keys.R | Keys.Control, () =>
				{
					Globals.Root.ShowEditScreen(m_Document);
				});
				Globals.Root.CurrentDocumentChanged += DocumentChanged;

				StartScanOnDocument();
			}
			catch (Exception) when (!Globals.Root.IsDebug)
			{// if scan fails to start it could be in an inconsistent state if some parts of the above ran
				StopScan();
				throw;
			}
		}

		private void StartScanOnDocument()
		{ // called when first scanning OR document changes while scannign
			m_Continuous = null;
			IterateScriptable(s => { if (s.Popup) s.Shown = false; });
			LastPopupShown = null;
			SavedPopup = null;
			m_WordPredictionContainers.Clear();

			Command.ExecutionContext context = new Command.ExecutionContext(null, this, m_Page, m_Document, m_Engine, Scriptable.ScriptTypes.Select);
			InvokeScriptCommands(m_Document.StartupScript, context);
		}

		/// <summary>Can safely be called when not scanning.  Stops the scan engine and unhighlights all items</summary>
		public void StopScan()
		{
			KeySend.Instance.ReleaseAll();
			PhysicalSwitch.MasterSwitch = PhysicalSwitch.MasterModes.Off;
			PhysicalSwitch.RemoveHotKey(Keys.R | Keys.Control, true);
			m_Blade?.SaveData(true);
			m_CurrentMove?.End(false);
			m_CurrentMove = null;
			EnsureCharacterHandlerRemoved();
			StopCustomRepeat(); // does nowt if none
			StopContinuous(m_Continuous); // ditto
			SetEngine(null);
			PointerSwitch.View = null;
			IterateScriptable(i => SetItemState(i, ButtonShape.States.Normal));
			Globals.Root.CurrentDocumentChanged -= DocumentChanged;
		}

		/// <summary>Performs action on all scriptable items in DOCUMENT (not just current page)</summary>
		public void IterateScriptable(Action<Scriptable> scriptable)
		{
			foreach (Page p in m_Document.Pages)
				p.Iterate(i =>
				{
					if (i is Scriptable scriptable2)
						scriptable.Invoke(scriptable2);
				});
		}

		/// <summary>Just an alias for Globals.Root.CurrentConfig since it is used a lot here </summary>
		private AppliedConfig CurrentConfig
		{ [DebuggerStepThrough]get { return Globals.Root.CurrentConfig; } }

		private void DocumentChanged()
		{ // only triggered while scanning
		  //DisplayPage(Globals.Root.CurrentPage, Globals.Root.CurrentDocument);
		}

		/// <summary>True if scan running </summary>
		public bool IsScanning
		{
			get
			{
				return PointerSwitch.View == this;
				// saves storing the state in another variable.  This is defined when and only when scan running in this view
			}
		}

		private void Engine_SettingsChanged()
		{
			if (!IsScanning)
				return;
			SetEngine(Switches.Engine.PrepareSwitchingFromConfig());
			base.InvalidateAll();
		}


		#endregion

		#region Scan iteration

		/// <summary>The currently selected shape</summary>
		private Scriptable m_Current;
		/// <summary>True if "scan" is actually mouse input.  Should ignore all attempts to select items, except from mouse </summary>
		private bool m_MouseInput;

		private void m_Engine_Iterate(object sender, SingleFieldEventClass<int> direction)
		{
			if (m_frmReport != null)
				return;
			if (m_Continuous != null) // see Activation section
				m_Continuous.Iterate();
			else if (m_Current != null)
				InvokeScript(m_Current, Scriptable.ScriptTypes.Next);
			else
				SelectInitialItem();
		}

		/// <summary>Changes the highlight state of the item.  Doesn't need to be the current one</summary>
		public void SetItemState(Scriptable item, ButtonShape.States state)
		{
			//Debug.WriteLine("SetItemState " + item.Description + " = " + state);
			item.State = state;
			m_Page.NotifyIndirectChange(item, ChangeAffects.RepaintNeeded, item.RefreshBounds());
		}

		/// <summary>Selects first item on page to start scan</summary>
		private void SelectInitialItem()
		{
			SelectItem(m_Page.Contents.OfType<Scriptable>().FirstOrDefault(i => !i.NotVisited && i.Shown), true);
		}

		/// <summary>Highlights the item, calling the Visit script.  directSelect=true IFF this is a request from the mouse input</summary>
		public void SelectItem(Scriptable item, bool directSelect)
		{
			if (m_MouseInput && !directSelect)
				return; // ignore all other changes when using mouse input
			Debug.Assert(item == null || !item.NotVisited);
			if (m_Current != null && directSelect) // these may be unnecessary now with above?
			{
				// can't clear m_Current before call (would help avoid infinite loops) since m_Current is always the execution context
				InvokeScript(m_Current, Scriptable.ScriptTypes.Next, true);
				// SAW was changed to include the next script in direct selection modes
				// originally it only did manual deselection
				// and it still manually unhighlights
			}
			m_Current = item; // must be before script - which might re-assign it
			if (item != null)
				InvokeScript(item, Scriptable.ScriptTypes.Visit);
		}

		public Scriptable HitTest(Point pt)
		{
			var dataPoint = MouseToData(pt);
			var hit = m_Page.HitTestAll(dataPoint, m_Zoom, Shape.Shapes.Scriptable, true).OfType<Scriptable>();
			// need to use HitTestAll since it has the type filter option
			//Debug.WriteLine("Hit test:");
			//foreach (var x in hit)
			//	Debug.WriteLine(x.ToString());
			return hit.FirstOrDefault(s => s.Shown && !s.NotVisited);
		}

		#endregion

		#region Activation and scripts

		/// <summary>The item that is currently, possibly, being repeated, if any</summary>
		private Scriptable m_RepeatItem;
		/// <summary>Used for EC/custom repeating - this needs to fire without any help from the switches.  So they always used this and ignore any switch repeat</summary>
		private Timer m_RepeatTimer;

		/// <summary>The command, if any, which has claimed subsequent scan output to make a selection.
		/// This is NOT repetition by holding a switch - instead it accepts both iterate and trigger</summary>
		private IContinuousCommand m_Continuous;

		/// <summary>Used by popup commands</summary>
		public Scriptable LastPopupShown;
		/// <summary>Popup remembered by the save popup command. Only used by popup commands</summary>
		public Scriptable SavedPopup;

		private void m_Engine_Trigger(bool isRepeat)
		{
			if (m_frmReport != null)
			{
				m_frmReport.SwitchActivated();
				return;
			}
			if (m_RepeatItem?.HasRepeatingScript ?? false)
			{ // currently doing custom repeats scripts 
				if (CurrentConfig.ReadBoolean(Config.Repeat_PressStop) && !isRepeat)
					StopCustomRepeat();
				// either way no further action is taken on this - the separate timer activates this
				return;
			}
			if (m_Continuous != null)
			{ // a command has captured the output to make a selection
				if (!m_Continuous.Trigger(isRepeat))
				{
					m_Continuous?.Stop(); // ? needed in case the command removed itself already
					m_Continuous = null;
				}
				return;
			}
			var item = isRepeat ? m_RepeatItem : m_Current;
			if (item == null) // usually this will be because repeating is not enabled on the last item, but also covers the case where nothing was selected for a genuine switch activation
				return;
			if (CurrentConfig.ReadBoolean(Config.Use_Swap_Switch))
				LogicalSwapper.Swap = !LogicalSwapper.Swap && !item.ResetSwap;
			// if item has ResetSwap then the state is forced to false
			// when repeating we explicitly use the remembered item -it may have selected something else as "current"
			//Debug.WriteLine("Trigger");
			var current = m_Current;
			if (isRepeat && item.HasRepeatingScript)
				InvokeScript(item, Scriptable.ScriptTypes.Repeat, true, item);
			else
				InvokeScript(m_Current, Scriptable.ScriptTypes.Select);
			if (!isRepeat) // for first activation, set up repeating if needed.
			{ // if item is not AutoRepeat, failing to set this will ensure the repeat triggers are ignored
				if (current.HasRepeatingScript)
				{ // start up the timer for custom repeat scripts
					m_RepeatItem = current;
					InvokeScript(m_RepeatItem, Scriptable.ScriptTypes.PreRepeat, true, m_RepeatItem); // this one fires immediately.
					m_RepeatTimer.Interval = m_Engine.ConfiguredTiming(Switches.Engine.Timings.FirstRepeat);
					m_RepeatTimer.Enabled = true;
				}
				else if (current.AutoRepeat)
					m_RepeatItem = current;
			}
		}

		private void m_Engine_EndSelect()
		{
			// switch which was repeat selecting released.  Ensure no repeats (although trigger shouldn't be called anyway!)
			if (m_RepeatItem?.HasRepeatingScript ?? false)
			{// custom repeating scripts, this would usually stop
			 // but first check if this actually ends, or if they are in the alternate mode:
				if (CurrentConfig.ReadBoolean(Config.Repeat_PressStop))
					return; // this doesn't actually stop until clicked again.  The switch up does nothing
				StopCustomRepeat();
			}
			else
				m_RepeatItem = null;
		}

		private void m_RepeatTimer_Tick(object sender, EventArgs e)
		{ // used for EC scripts
			if (m_RepeatItem == null)
			{ // must have been cancelled
				m_RepeatTimer.Enabled = false;
				return;
			}
			m_RepeatTimer.Interval = m_Engine.ConfiguredTiming(Switches.Engine.Timings.SubsequentRepeat);
			InvokeScript(m_RepeatItem, Scriptable.ScriptTypes.Repeat, true, m_RepeatItem);
		}

		private void StopCustomRepeat()
		{
			if (m_RepeatItem != null) // just in case.  Both protects vs null and from running it twice
				InvokeScript(m_RepeatItem, Scriptable.ScriptTypes.PostRepeat, true, m_RepeatItem);
			m_RepeatItem = null;
			m_RepeatTimer.Enabled = false;
		}


		/// <summary>Run one of the scripts on the item.  If noVisit then the VisitItem is not used (but explicit Visit scripts would still run)
		/// The item which acts as the context can be provided, but is the Current/selected item normally (NOT the item on which the script is invoked - exec commands still
		/// run in the context of the pressed button)</summary>
		public void InvokeScript(Scriptable item, Scriptable.ScriptTypes which, bool noVisit = false, Scriptable contextItem = null)
		{
			//Debug.WriteLine("Invoke script " + which + " on " + item.Description);
			Script script = item.GetScript(which);
			if (contextItem == null)
				contextItem = m_Current;
			// find defaults first, if needed (unlike SAW6 this is possible even if there are genuine scripts)
			Script def = null;
			if (script?.RunDefault ?? true) // if object missing entirely(?!?) default is used
				def = item.GetDefaultScript(which, m_Document);
			Command.ExecutionContext context = new Command.ExecutionContext(contextItem, this, m_Page, m_Document, m_Engine, which);
			if (def != null)
				InvokeScriptCommands(def, context);
			if ((script?.CommandList?.Count ?? 0) != 0) // either null object, or no commands (only latter possible?) either counts the same
				InvokeScriptCommands(script, context);

			if (noVisit || context.Terminate)
				return;
			// however the default visit is done after any custom commands
			Scriptable target = null;
			if (script != null)
				target = ResolveVisitTarget(script.Visit, item);
			if (target == null && def != null) // default is only used if self is set to None:
				target = ResolveVisitTarget(def.Visit, item);
			if (target == null && which == Scriptable.ScriptTypes.Next)
				target = item; // a Next script which references nothing useful should re-select the same item
			if (target != null)
				SelectItem(target, false);
		}

		/// <summary>Runs the actual commands, without any defaults, or the Visit part.
		/// contextItem may be null for startup script</summary>
		private void InvokeScriptCommands(Script script, Command.ExecutionContext context)
		{
			if (script == null) // I don't think this is actually allowed, but in case
				return;
			foreach (Command c in script.CommandList)
			{
				//Debug.WriteLine("Execute: " + c.GetScriptWithParams(false));
				c.Execute(context);
				if (context?.Terminate == true)
					break;
			}
		}

		/// <summary>Returns the item which is referenced by the visit object, or null if nothing found (which usually has same effect as "Me")</summary>
		public Scriptable ResolveVisitTarget(Script.VisitTarget visit, Scriptable source)
		{
			//Debug.WriteLine("Resolve: " + visit.GetDescription() + " from " + (source?.SAWID.ToString() ?? "null"));
			List<Scriptable> items;
			if (source != null)
				items = (source.Parent as IShapeContainer).Contents.OfType<Scriptable>().Where(i => i.Shown && !i.NotVisited).ToList();
			else
				items = new List<Scriptable>();
			int currentIndex = items.IndexOf(source);
			switch (visit.VisitType)
			{
				case Script.VisitTarget.VisitTypes.None:
					return null;
				case Script.VisitTarget.VisitTypes.Me:
					return source;
				case Script.VisitTarget.VisitTypes.First:
					return items.FirstOrDefault();
				case Script.VisitTarget.VisitTypes.Last:
					return items.LastOrDefault();
				case Script.VisitTarget.VisitTypes.Previous:
					Debug.Assert(currentIndex >= 0);
					currentIndex -= 1;
					if (currentIndex < 0)
						currentIndex = items.Count - 1;
					return items[currentIndex];
				case Script.VisitTarget.VisitTypes.Next:
					Debug.Assert(currentIndex >= 0);
					currentIndex += 1;
					if (currentIndex >= items.Count)
						currentIndex = 0;
					return items[currentIndex];
				case Script.VisitTarget.VisitTypes.Down:
					return ((source.Element as IShapeContainer)?.Contents.OfType<Scriptable>()).FirstOrDefault(i => i.Shown && !i.NotVisited);
				case Script.VisitTarget.VisitTypes.Up:
					return (source.Parent as Shape)?.Parent as Scriptable; // will return null if not within an element within a Scriptable
				case Script.VisitTarget.VisitTypes.Item:
					return m_Page.FindScriptableByID(visit.ItemID);
				default:
					OnError("Unknown visit type: " + visit.VisitType);
					return null;
			}
		}

		/// <summary>Causes the given command to capture all further scan output until StopContinuous</summary>
		public void StartContinuous(IContinuousCommand command)
		{
			if (m_Continuous == command)
				return; // so it doesn't have stop called
			m_Continuous?.Stop();
			m_Continuous = command;
			m_RepeatItem = null;
		}

		/// <summary>Called to stop the command being given scan output.  Stating the command allows this to be ignored if it already lost the output</summary>
		public void StopContinuous(IContinuousCommand command)
		{
			if (m_Continuous == command)
			{
				m_Continuous?.Stop(); // ? allows this to be called safely with (m_Continuous) as param and it can't blow up
				m_Continuous = null;
			}
		}

		/// <summary>Really just a place holder until method is determined.  The config that scripts edit.</summary>
		public Config ConfigToEdit
		{ get { return Config.UserUser; } }

		#endregion

		#region Output

		/// <summary>Any delay set by keydelay.  Must persist while the selection set is running</summary>
		public int KeyDelay;
		/// <summary>The time, in seconds, for which to hide SAW after text output, if >0.  If 0 or negative then this is off (the default).
		/// Changed only by set hide time script command (SAW 6 did store in settings, but had no settings UI for it)</summary>
		public float HideTime;

		private AudioPlayer Player = new AudioPlayer();
		//public void PlaySoundFile(string file)
		//{
		//	Globals.Root.Speech.StopAll();
		//	Player.Stop();
		//	Player.PlayAsync(file);
		//}

		/// <summary>Use the sound resource as the parameter which should be an unmanagedmemorystream</summary>
		public void PlaySoundResource(System.IO.Stream stream)
		{
			Globals.Root.Speech.StopAll();
			Player.Stop();
			Player.PlayAsync(stream);
		}

		/// <summary>Just synthesises the text by using Globals.Root.Speech, but calling here can also do logic regarding interrupting previous sounds</summary>
		public void Speak(string text)
		{
			Globals.Root.Speech.StopAll();
			Player?.Stop();
			Globals.Root.Speech.Speak(text);
		}

		/// <summary>Sends using Gidei unless 'enclosed' or &lt;?new&gt; is encountered.  If delay is not specified then the KeyDelay property is used.
		/// This checks the focus is not in SAW before sending to SendKeys</summary>
		public void SendKeys(string text, int delay = -1)
		{
			//if (FindForm().Focused) this seems to be true spuriously at time;  this may be better:
			if (Windows.GetForegroundWindow().Equals(FindForm().Handle))
				frmFocusWarning.Display();
			else
			{
				try
				{
					if (delay < 0)
						delay = KeyDelay;
					KeySend.Instance.SendStringKeys(text, delay);
				}
				catch (UserException e) when (!Globals.Root.IsDebug)
				{ MessageBox.Show(e.Message); }
			}
		}


		#endregion

		#region Mouse

		public int MouseStep
		{
			get { return CurrentConfig.MouseStep(MouseStepType); }
		}

		public AppliedConfig.MouseSteps MouseStepType = AppliedConfig.MouseSteps.Medium;

		#endregion

		#region Reporting
		/// <summary>Used to report errors in scripts.  These are unexpected, invalid situations  Messages don't always translate - not decided yet whether they should</summary>
		public void OnError(string message)
		{
			Console.Beep();
			Utilities.LogSubError(message);
		}

		/// <summary>Reports a script that failed in a non-exceptional way (eg nothing selected for it to act on).  Currently beeps.  
		/// The message is optional.  This could be updated to output those messages</summary>
		public void OnFail(string message = "")
		{
			Console.Beep();
			Globals.Root.Log.WriteLine(message);
		}

		/// <summary>Popup message screen shown to user over scanning.  If this is visible switches are sent to it instead. </summary>
		private frmUserReport m_frmReport;
		public void ShowReportWindow(string message, bool isError, int timeLimitMS = 0)
		{
			if (isError)
				Globals.Root.Log.WriteLine(message);
			m_frmReport = frmUserReport.Display(message, this, timeLimitMS, () =>
			{// called when form is closed for any reason
				m_frmReport = null;
			});
		}


		#endregion

		protected override void m_Page_ShapeNotifiedIndirectChange(Shape shape, ChangeAffects affected, RectangleF area)
		{
			if ((affected & ChangeAffects.RepaintNeeded) > 0)
			{
				if (area.IsEmpty)
					InvalidateData(shape.RefreshBounds(), InvalidationBuffer.All);
				else
					InvalidateData(area, InvalidationBuffer.All);
				affected -= ChangeAffects.RepaintNeeded;
			}
		}

		#region Movement commands
		// mouse movement;  other commands can capture this
		private IMoveCommand m_CurrentMove;

		public void SetMouseMove(IMoveCommand move)
		{
			m_CurrentMove?.End(false); // cancel the old one, if any
			m_CurrentMove = move;
		}

		/// <summary>Triggers a move command from a mouse move script.  X/Y should be +-1</summary>
		public void DoMoveCommand(int X, int Y)
		{
			if (m_CurrentMove == null)
				m_CurrentMove = new CmdMouseMove(); // will move mouse as default.  This shouldn't really happen, but safe to do as a fallback
			m_CurrentMove.Move(X * MouseStep, Y * MouseStep);
		}

		/// <summary>Should be called on OK/Cancel commands.  Returns true if it is consumed by ending a previous move</summary>
		public bool DoOKCancel(bool ok)
		{
			if (m_CurrentMove != null && !(m_CurrentMove is CmdMouseMove))
			{
				m_CurrentMove.End(ok);
				m_CurrentMove = null;
				return true;
			}
			return false;
		}
		#endregion

		#region Word prediction
		/// <summary>Each of these is a SAW item containing the buttons to display word predictions.
		/// Usually only one will be used, but it is possible to specify multiple ones, in which case they all show the same contents (used if they are in different pop ups) </summary>
		private List<Item> m_WordPredictionContainers = new List<Item>();

		private Blade.Engine m_Blade;

		/// <summary>Called by SetWordList to define the (/an) output area.  Does not update settings</summary>
		public void SetWordList(int itemID)
		{
			Item item = (m_Page.FindScriptableByID(itemID)?.Element as Item); // must be a SAW item, since it must have contents
			if (item == null)
			{
				OnError(Strings.Item("Script_Fail_MissingWordListItem", itemID.ToString()));
				return;
			}
			m_WordPredictionContainers.Add(item);
			EnsureBlade();
			m_Blade.Case = Cases.Normal;
			// UpdateWordPredictions(); // not needed as the script will also call BladeSettings
		}

		private void EnsureBlade()
		{
			if (m_Blade == null)
				m_Blade = BladeWrapper.Blade; // creates the instance if needed
			if (!CharacterCaptureAttached)
				KeySwitch.CharacterTyped += KeySwitch_CharacterTyped; // this may be needed even if Blade attached, as StopScan removes it
			CharacterCaptureAttached = true;
		}

		/// <summary>Resets word prediction system, IF running</summary>
		private void ResetBlade()
		{
			if (m_Blade == null)
				return;
			m_Blade.Case = Cases.Normal;
			m_Blade.TrackNewMessage();
		}

		/// <summary>Sends the settings string to the engine; also does UpdateWordPredictions.  See the Blade documentation for the meaning of the settings</summary>
		public void BladeSettings(string settings)
		{
			EnsureBlade();
			m_Blade.SetSettings(settings);
			UpdateWordPredictions();
		}

		/// <summary>Updates word predictions, if in use.  Ignored otherwise </summary>
		public void UpdateWordPredictions()
		{
			if (m_Blade == null || !m_WordPredictionContainers.Any())
				return;
			List<string> predictions = m_Blade.Predict(m_Blade.TrackedMessage);
			foreach (var container in m_WordPredictionContainers)
				container.ShowPredictions(predictions);
		}

		public void ScrollWordList(CmdWordListScroll.Directions direction)
		{
			foreach (var container in m_WordPredictionContainers)
				container.ScrollWordList(direction);
		}

		public void SelectPrediction(string text)
		{
			m_Blade.Type(text, true);
			//UpdateWordPredictions();
		}

		//public Blade.Engine Blade
		//{
		//	get
		//	{
		//		EnsureBlade();
		//		return m_Blade;
		//	}
		//}

		#region Various callbacks and their helpers

		private bool CharacterCaptureAttached;
		private void KeySwitch_CharacterTyped(object sender, SingleFieldEventClass<char> e)
		{ // all CHARACTERS typed sent here by keyswitch, except a few undetectable sent by KeySend to KeySend_ExtraKeyForPredictions
		  //Debug.WriteLine("Track char: " + e.Value);
			if (m_Blade == null)
				return;
			m_Blade.TrackCharacterTyped(e, 0);
			UpdateWordPredictions();
		}
		private void KeySend_ExtraKeyForPredictions(object sender, SingleFieldEventClass<char> e)
		{
			// this is triggered by KeySend when it sends an undetectable character
			if (m_Blade == null)
				return;
			m_Blade.TrackCharacterTyped(e, 0);
			UpdateWordPredictions();
		}

		private void EnsureCharacterHandlerRemoved()
		{
			if (CharacterCaptureAttached)
				KeySwitch.CharacterTyped -= KeySwitch_CharacterTyped;
			CharacterCaptureAttached = false;
		}

		#endregion

		#endregion

	}
}
