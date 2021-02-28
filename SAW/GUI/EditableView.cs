using System.Collections.Generic;
using System;
using System.Drawing;
using System.Diagnostics;
using System.Windows.Forms;
using System.Linq;
using SAW.Functions;

namespace SAW
{
	public sealed class EditableView : StaticView, IShapeParent, IKeyControl
	{
		// the complete document view class which supports all editing

		#region Events
		public event ShapeEventHandler ChangeDiagnostic; // causes the containing form to display diagnostic info for this shape
		public event ShapeEventHandler DisplayShapeStyles;
		public event ShapeEventHandler ApplyDefaultStyles;
		public event NullEventHandler StyleStateChanged;

		public delegate void DisplayMousePositionEventHandler(ClickPosition position);

		public event DisplayMousePositionEventHandler DisplayMousePosition; // this just reports the mouse coordinates; parameter is nothing if mouse outside the control

		// if using separate cursor is only the keyboard cursor is reported
		public delegate void DisplayShapeInfoEventHandler(string text);

		public event DisplayShapeInfoEventHandler DisplayShapeInfo; // displayed information regarding the current shape (current could be currently being drawn or selected etc)

		public delegate void DisplayContextMenuEventHandler(Point pt);

		/// <summary>Tells the container to display the editing context menu.  pt is the location in local graphics coordinates</summary>
		public event DisplayContextMenuEventHandler DisplayContextMenu;

		public delegate void RequestPaletteExclusionHandler(Rectangle screen);

		public event RequestPaletteExclusionHandler RequestPaletteExclusion;
		#endregion

		// MORE MEMBERS WITHIN REGIONS
		/// <summary>shape being placed; not currently on page</summary>
		private Shape m_CurrentShape;

		/// <summary>if we are typing into an existing shape, this transaction covers the changes
		/// this is nothing if m_shpCurrent is defined</summary>
		private Transaction m_TypingTransaction;

		/// <summary>when the current shape was completed it said that it wanted to start another shape.
		/// the following shape is stored here until the fade in is </summary>
		private Shape m_SpawnShape;

		/// <summary>the shape before m_shpCurrent when generating a series, such as pie chart (allows us to select the last one
		/// on cancel; and the selection changed is useful to trigger a verb button update in frmMain).  Should not be set with NumberGrid tool</summary>
		private Shape m_SpawnedFrom;

		// 
		/// <summary>the target/container, if any, which the current shape will be dropped into.</summary>
		private Shape m_TargetHover;
		// Must be a shape which implements IShapeTarget; it might implement IShapeContainer in which case the shapes are actually moved into it

		// transactions should probably be attached to documents eventually...
		private Shape.SnapModes m_SnapMode = Shape.SnapModes.Off;

		public EditableView()
		{
			this.BackColor = Color.Gray;
			m_Buffers.Add(m_BaseBuffer);
			m_Buffers.Add(m_CurrentBuffer);
			m_Buffers.Add(m_SelectedBuffer);
			if (this.DesignMode)
				return;

			m_tmrGraphics.Interval = 30;
			m_tmrGraphics.Enabled = true;

			Globals.CurrentPageSizeChanged += PageSizeChanged;

			this.GotFocus += EditableView_GotLostFocus;
			this.LostFocus += EditableView_GotLostFocus;
			this.MouseDoubleClick += EditableView_MouseDoubleClick;
			this.MouseDown += EditableView_MouseDown;
			this.MouseLeave += EditableView_MouseLeave;
			this.MouseUp += EditableView_MouseUp;
			this.Scroll += EditableView_Scroll;
			this.MouseMove += EditableView_MouseMove;
			m_tmrDelayedClick.Tick += m_tmrDelayedClick_Tick;
			m_tmrGraphics.Tick += m_tmrGraphics_Tick;
			m_Solidify.IsComplete += m_Solidify_IsComplete;
			m_Solidify.Refresh += m_Solidify_Refresh;
		}

		static EditableView()
		{
			g_WhiteTransparent = GUIUtilities.GetImageAttrForAlpha(35);
			g_WhiteTransparent.SetColorKey(Color.White, Color.White);
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			m_CurrentBuffer?.Discard();
			m_CurrentBuffer = null;
			m_SelectedBuffer?.Discard();
			m_SelectedBuffer = null;
			Globals.CurrentPageSizeChanged -= PageSizeChanged;
		}

		public override void DisplayPage(Page page, Document document)
		{
			if (m_Page != null)
				ConcludeOngoing();
			base.DisplayPage(page, document);

			EnableGrabSpots = m_Tool == Shape.Shapes.Selector;
			m_CurrentShape?.Dispose();
			m_SpawnShape?.Dispose();
			m_CurrentShape = null;
			m_SpawnShape = null;
			m_SpawnedFrom = null;
			m_Status = Status.Waiting;
			StaticView_Resize(this, null);
			m_GrabSpots = null;
			m_GrabSpotHit = null;
			m_GrabMove = null;
			Debug.Assert(m_OngoingTransaction == null);
			Cursor = Cursors.Default; // will be Hand if displaying a prompt page
			if (page != null)
				Focus();
		}

		private void PageSizeChanged()
		{
			// handles the Globals event for the current page size changing
			if (m_Page != Globals.Root.CurrentPage)
				return; // we are not displaying the page being edited
			m_ClientSize = new SizeF(m_Page.Size.Width * m_Zoom * m_PixelsPerDocumentX, m_Page.Size.Height * m_Zoom * m_PixelsPerDocumentY).ToSize();
			StaticView_Resize(this, EventArgs.Empty);
			InvalidateAll();
		}

		public override void EndDisplayPage()
		{
			m_Status = Status.Waiting;
			m_CurrentShape = null; // Doesn't even call AbandonCurrent
			m_ShapeHover = null;
			m_SpawnShape = null;
			m_PromptsDirty = false;
			m_TypingTransaction = null;
			base.EndDisplayPage(); // This clears m_Page, so mustn't be done first
		}

		#region Public information and properties

		public Shape OngoingShape
		{
			get { return m_CurrentShape; }
		}

		public Shape.Shapes OngoingShapeType
		{
			get { return m_CurrentShape?.ShapeCode ?? (Shape.Shapes)(-1); }
		}

		[System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
		public Shape.SnapModes SnapMode
		{
			[DebuggerStepThrough]
			get { return m_SnapMode; }
			[DebuggerStepThrough]
			set
			{
				if (m_SnapMode == value)
					return;
				m_SnapMode = value;
				if (m_Page != null && m_Page.HasTargets)
				{
					m_Page.ClearTargets();
					InvalidateData(m_Page.Bounds, InvalidationBuffer.Current);
				}
				Globals.OnParameterChanged(Parameters.Action_Snap);
			}
		}

		#endregion

		#region Keys

		/// <summary>Estimated number of keystrokes formed within the current m_TypingTransaction.  Used to decide if we should store it as a separate transaction</summary>
		/// <remarks>Only counts keys which seem to perform an action (i.e. did not return Unchanged or similar)</remarks>
		private int m_KeystrokesInTransaction;

		public void CombinedKeyDown(CombinedKeyEvent e)
		{
			if (DesignMode)
				return;
			Shape shp = TypingShape();
			Debug.Assert(shp == null || shp.Parent != null, "TypingShape.Parent = Nothing;  CombinedKey may fail");
			Shape.VerbResult result = Shape.VerbResult.Unexpected;
			Transaction newTransaction = null; // only to be used for actual typing, not activities
			RectangleF refresh = RectangleF.Empty;
			if (shp != null && (shp.Allows & Shape.AllowedActions.Typing) > 0)
			{
				if (m_CurrentShape == null && m_TypingTransaction == null)
				{
					// if any changes are made we will need to store this transaction
					newTransaction = new Transaction();
					newTransaction.Edit(shp);
					m_KeystrokesInTransaction = 0;
					Shape.StoreCaretState(newTransaction);
				}
				refresh = shp.RefreshBounds();
				result = shp.CombinedKey(e.KeyData, e.Character, m_Page, e.Simulated, this);
				// check that the control does have the caret (if appropriate)
			}
			else if (e.KeyCode == Keys.ControlKey && IsSingleSelector)
			{
				ForceUpdateGrabSpots();
				// changing the control key can change which GrabSpots are shown (e.g. in container)
			}
			else if (e.KeyCode == Keys.Apps && Globals.Root.CurrentConfig.ReadBoolean(Config.Context_Menus))
			{
				var target = new ClickPosition(MouseToData(CursorPositionLocal), m_Page, m_Zoom, EffectiveSnap(), m_SnapMode, this, ClickPosition.Sources.Keyboard);
				TriggerContextMenu(target, CursorPositionLocal);
			}
			if (result == Shape.VerbResult.Unexpected && EffectiveTool == Shape.Shapes.Selector)
			{
				// try doing an activity... (if none of the above set a result code)
				shp = m_Page.FindSingleActivity();
				if (shp != null)
				{
					newTransaction = new Transaction(); // will be cleared if result in unexpected or unchanged (which are pretty much the defaults)
					newTransaction.Edit(shp);
					m_KeystrokesInTransaction = 0;
					refresh = shp.RefreshBounds();
					result = shp.CombinedKey(e.KeyData, e.Character, m_Page, e.Simulated, this); // never actually makes any changes to the activity that would need to be stored in the data
																								 // but might need a refresh
				}
			}
			Debug.Assert(result > 0, "Shape.CombinedKey: did not set return value"); // 0 is not used by any of the return codes
			if (result == Shape.VerbResult.Unchanged)
				Debug.WriteLine("pnlView swallowing key on Unchanged result - behaviour change.  Shapes should return Unexpected for unhandled keys");
			e.Handled = e.Handled || ProcessKeyVerbResult(shp, result, newTransaction, refresh);
			if (m_TypingTransaction != null && m_KeystrokesInTransaction > 5)
			{
				Globals.Root.StoreNewTransaction(m_TypingTransaction);
				m_TypingTransaction = null;
			}
		}

		public void CombinedKeyUp(CombinedKeyEvent e)
		{
			if (e.KeyCode == Keys.ControlKey && IsSingleSelector)
				ForceUpdateGrabSpots();
		}

		// not sure why this is separate from DoVerbResult - escape in 100G goes there for example, and never here
		private bool ProcessKeyVerbResult(Shape shp, Shape.VerbResult result, Transaction newTransaction, RectangleF previousBounds)
		{
			// returns true if the key was handled.  This is done via bolHandled variable...
			bool handled = false;
			switch (result)
			{
				case Shape.VerbResult.Completed:
					if (m_Status == Status.Adding)
						StartSolidifying();
					else
					{
						// This wasn't in version 1, and is included causes NumberGrid to fail (it usually returns Completed)
						//m_Page.DeselectAll()
					}
					handled = true;
					InvalidateAll();
					break;
				case Shape.VerbResult.Destroyed:
					shp.CaretLose();
					if (m_Status == Status.Adding)
						AbandonCurrent();
					else
					{
						if (newTransaction != null)
							m_TypingTransaction = newTransaction;
						if (m_TypingTransaction != null)
						{
							m_Page.Delete(shp, newTransaction);
							//objNewTransaction.Delete(shp)
							Globals.Root.StoreNewTransaction(newTransaction);
							newTransaction = null; // this is counted as complete, we don't want to remember it below and then try and store it again later
							m_TypingTransaction = null;
						}
						m_KeystrokesInTransaction = 0;
					}
					handled = true;
					InvalidateAll();
					break;
				case Shape.VerbResult.Substitute:
					m_KeystrokesInTransaction += 1;
					Geometry.Extend(ref previousBounds, shp.RefreshBounds());
					InvalidateData(previousBounds, InvalidationBuffer.All);
					Shape create = shp.Spawn();
					m_SpawnShape = null;
					create.Parent = m_Page;
					m_Status = Status.Waiting;
					m_CurrentShape = null;
					if (!m_Page.Contains(create))
					{
						m_OngoingTransaction.Create(create);
						m_Page.AddNew(create, m_OngoingTransaction);
					}
					m_Page.SelectOnly(create);
					handled = true;
					OngoingShapeChanged();
					break;

				case Shape.VerbResult.Continuing:
					m_KeystrokesInTransaction += 1;
					handled = true; // this is only returned if the key has been processed
					Geometry.Extend(ref previousBounds, shp.RefreshBounds());
					InvalidateData(previousBounds, InvalidationBuffer.All);
					if (m_Status == Status.Waiting)
						UpdateDiagnostic();
					break;
				case Shape.VerbResult.Unchanged:
					newTransaction = null;
					handled = true; // note in version 1 this result was treated as not handling the key
					break;
				case Shape.VerbResult.Unexpected: // just means that the key has been ignored
				case 0:
					newTransaction = null;
					break;
				case Shape.VerbResult.Spawn:
					handled = true;
					m_SpawnShape = shp.Spawn(); //  will work OK if it returns nothing (which is sort of permissible)
					if (m_SpawnShape != null)
						m_SpawnedFrom = shp;
					if (m_Status == Status.Adding)
						StartSolidifying(); // will do the spawning
					else
					{
						if (m_SpawnShape != null)
						{
							// previous shape requested that we immediately start another
							m_SpawnShape.Parent = m_Page;
							// how we handle this must depend on whether the shape already exists in the page
							InvalidateData(m_SpawnShape.RefreshBounds(), InvalidationBuffer.All);
							if (m_Page.Contains(m_SpawnShape))
							{
								// select this shape
								m_Status = Status.Waiting;
								m_Page.SelectOnly(m_SpawnShape);
								m_CurrentShape = null;
							}
							else
							{
								// this shape becomes the new shape, no selection
								m_Status = Status.Adding;
								m_CurrentShape = m_SpawnShape;
								m_Page.DeselectAll();
								InitialiseCurrentShape();
							}
							DisplayShapeStyles?.Invoke(m_SpawnShape); // needed for spawn from NG - can have different lines to those showing in palette
							m_SpawnShape = null;
							OngoingShapeChanged();
						}
					}
					break;
				case Shape.VerbResult.Custom:
					switch (shp.ShapeCode)
					{
						default:
							Utilities.LogSubError("Unexpected Custom result from shape type: " + shp.ShapeCode);
							break;
					}
					break;
				case Shape.VerbResult.Rejected:
					Debug.WriteLine("Verb rejected");
					break;
				default:
					Debug.Fail("Unexpected verb result: " + result);
					break;
			}
			if (newTransaction != null)
				m_TypingTransaction = newTransaction;
			return handled;
		}

		// enabling these breaks the equation editor completely
		//protected override bool IsInputChar(char charCode)
		//{
		//	return true;
		//}

		//protected override bool IsInputKey(Keys keyData)
		//{
		//	// tells Windows to give the key to the control even if it would normally something like focus change (tab)
		//	return true;
		//}

		private void EditableView_GotLostFocus(object sender, EventArgs e)
		{
			// also called for lost
			UpdateHoverPrompts();
			GUIUtilities.CurrentFocus = Focused ? this : null;
		}

		public bool IsKeyApplicable(char ch)
		{
			var shp = TypingShape();
			return shp?.IsKeyApplicable(ch) ?? true;
		}

		public bool IsKeyApplicable(Keys key)
		{
			var shp = TypingShape();
			return shp?.IsKeyApplicable(key) ?? true;
		}

		// these are needed to enable using the cursor keys to do the Nudge commands in SAW
		protected override bool IsInputChar(char charCode)
		{
			return true;
		}

		protected override bool IsInputKey(Keys keyData)
		{
			// tells Windows to give the key to the control even if it would normally something like focus change (tab)
			return true;
		}



		#endregion

		#region Mouse responses and dragging and grabs

		/// <summary>is defined after a click when the user might be initiating a drag.  Anything outside this rectangle starts dragging uses screen coordinates, not data coordinates</summary>
		private Rectangle m_DragTrigger = Rectangle.Empty;

		/// <summary>defined each time we start drawing a shape.  Is true if the first Choose is being done after mouse drag and release (like drag mode in AccessMaths 4)</summary>
		private bool m_DragMode = false;

		/// <summary>we want to tend away from dragging slightly as we support click-click and drag, so this is added to SystemInformation.DragSize</summary>
		internal const int DRAGSIZEINCREASE = 16;

		private Shape m_SelectClickHit; // used when selecting - defined if click hit something (in which case drag moves, otherwise it multi-selects)

		// also if defined and didn't drag then releasing will select-only (can't on mouse down as that would prevent a multi-move)
		private Shape.GrabSpot m_GrabSpotHit; // GrabSpot hit when the user initially clicked
		private ClickPosition m_OriginalClickPoint; // where the MouseDown event originally occurred
		private int m_Rejections; // safety mechanism; if we get too many rejected verb results in a row we abandon the current shape
		private bool m_ButtonDown; // true if any mouse button was down; used to filter out when we get only a mouse up event (e.g. because a dialogue over the top of this has disappeared)

		private Transaction m_OngoingTransaction; // only used in certain cases until storage
												  // during grabs most of the movement just done by transforming; during create usually shape only transacted when finished (m_shpCurrent doesn't count as data)
												  // however some actions have side-effects which get stored in here, and cancelled as needed

		// grab spot mechanism includes just picking a shape up by it's background and moving it, which automatically creates an imaginary GrabSpot
		private ClickPosition m_PointGrabLastApplied; // last data point applied by GrabMove

		/// <summary>only defined when movement is underway</summary>
		private Shape.GrabMovement m_GrabMove;

		/// <summary>whether this control was already focused before the mouse down event</summary>
		private bool m_WasFocused = true;

		private DateTime m_tmPreviousClick = DateTime.MinValue;

		private void EditableView_MouseDown(object sender, MouseEventArgs e)
		{
			// If not yet creating a shape we can use the following edge of the mouse as the trigger for the Start command
			if (this.DesignMode)
				return;
			Globals.StoreEvent("MouseDown " + e.Button);
			m_ActionIsKeyboard = false;
			m_WasFocused = this.Focused;
			Focus();
			m_ButtonDown = true;
			m_DragMode = false; // in case left running previously (if mouse up was lost)
			m_SelectClickHit = null;
			m_GrabSpotHit = null;
			m_OriginalClickPoint = MouseEventCoordinates(e.X, e.Y, ClickPosition.Sources.Mouse);
			m_OriginalClickPoint.PossibleDoubleClick = DateTime.Now.Subtract(m_tmPreviousClick).TotalMilliseconds < SystemInformation.DoubleClickTime;
			m_tmPreviousClick = DateTime.Now;

			// set up the possibility of dragging... (this applies to starting a new shape as well as the selector)
			if (m_Status == Status.Adding || m_GrabMove != null)
				m_DragTrigger = Rectangle.Empty;
			else
			{
				// not currently in the middle of a shape.  Only left button is active
				if (e.Button != MouseButtons.Left)
					return;
				Size dragSize = SystemInformation.DragSize;
				if (m_GrabSpotHit == null)
				{
					// Sys default on my PC is a bit too sensitive; I drag by accident sometimes.
					// however the sensitivity not decreased if the user actually clicked on a GrabSpot
					dragSize.Width += DRAGSIZEINCREASE;
					dragSize.Height += DRAGSIZEINCREASE;
				}
				m_DragTrigger = new Rectangle(e.X - dragSize.Width / 2, e.Y - dragSize.Height / 2, dragSize.Width, dragSize.Height);
				// m_rctDragTrigger must be set first - some items want to clear it (eg Label shape)
			}
			if (e.Button != MouseButtons.Left)
				return;
			ChooseDown(m_OriginalClickPoint, false);
			UpdateDiagnostic();
		}

		private void EditableView_MouseUp(object sender, MouseEventArgs e)
		{
			Globals.StoreEvent("MouseUp " + e.Button);
			m_DragTrigger = Rectangle.Empty; // dragging is triggered by mouse move if the user moves far enough.  If they haven't already done so, then we will no longer initiate a drag
											 // we only use the mouse up at the end of that first movement if the user is working in drag mode
											 // otherwise we use the mouse down to trigger everything
			if (!m_ButtonDown)
				return;
			m_ButtonDown = false;
			if (m_GrabMove != null)
			{
				if (e.Button == MouseButtons.Right || m_PointGrabLastApplied == null)
					CancelGrab();
				else
					StoreGrab();
				return;
			}
			ClickPosition target = MouseEventCoordinates(e.X, e.Y, ClickPosition.Sources.Mouse);
			m_OriginalClickPoint = null;

			if (m_DragMode && m_CurrentShape != null && e.Button == MouseButtons.Left)
			{
				TriggerVerb2(m_CurrentShape, Codes.Choose, target, false);
				m_DragMode = false; // the rest of the placement (if any) continues in the normal click, click mode
			}
			else if (m_Status == Status.Adding)
			{
				//normal clicks for adding shapes done rising edge (to help with double clicks - so at least the double only counted once)
				Debug.Assert(m_CurrentShape != null);
				switch (e.Button)
				{
					case MouseButtons.Left:
						TriggerVerb2(m_CurrentShape, Codes.Choose, target, false);
						break;
					case MouseButtons.Right:
						TriggerVerb2(m_CurrentShape, Codes.Cancel, target, false);
						break;
				}
			}
			else if (e.Button == MouseButtons.Left)
			{
				UpdateMousePosition(ref target, false);
				ChooseUp(target, false);
			}
			else // Must be right click with nothing ongoing
			{
				if (m_Page.SelectedCount == 1 && m_Page.SelectedShapes[0].AllowVerbWhenComplete(Codes.Cancel))
					TriggerVerb(Codes.Cancel, ClickPosition.Sources.Mouse);
				else if (Globals.Root.CurrentConfig.ReadBoolean(Config.Context_Menus))
					TriggerContextMenu(target, e.Location);
			}
			m_DragMode = false;
			UpdateDiagnostic();
		}

		public void TriggerContextMenu(ClickPosition target, Point location)
		{
			Shape firstSelected = m_Page.SelectedShapes.FirstOrDefault();
			if (m_Page.SelectedCount == 0 || !firstSelected.Bounds.Contains(target.Exact))
			{
				// if nothing is selected, or we haven't clicked within the selected shape, then we try to select what is under the current click
				// so right-clicking can change selection (people expect menu to act on what is clicked on, and mechanism for that is to change selection - this is normal for other software)
				// however... only want to change selection if something was clicked on (dont want to change from something selected to nothing selected)
				// ie right-clicking on background will act on selection rather than clicked-item (none).   therefore check that first...
				Shape hit = m_Page.HitTest(target.Exact, m_Zoom, Page.HitTestMode.StartFront);
				if (hit != null)
					SelectorClick(target, false);
			}
			else if (m_Page.SelectedCount == 1 && (target.Source == ClickPosition.Sources.Mouse && firstSelected.Bounds.Contains(target.Exact)
					 || HitTestGrabSpots(target.Exact)?.Shape == firstSelected))
				SelectOnly(firstSelected, target.Exact); // can update the selected vertex/line
			DisplayContextMenu?.Invoke(location);
		}

		private void EditableView_MouseMove(object sender, MouseEventArgs e)
		{
			// this is the part which is specific to the mouse; see DoCursorMove for the parts which are shared with the keyboard cursor
			if (this.DesignMode)
				return;
			ClickPosition target = MouseEventCoordinates(e.X, e.Y, ClickPosition.Sources.Mouse);
			DisplayMousePosition?.Invoke(target);
			if (m_GrabMove != null)
			{
				// already moving a grab spot - works whether drag or move version
				MoveGrab(target); // does invalidation internally
				return;
			}
			// check for starting some sort of drag...
			if (!m_DragMode && !m_DragTrigger.IsEmpty && !m_DragTrigger.Contains(e.X, e.Y) && Shape.GetClass(m_Tool) != Shape.Classes.Transformation && m_Status == Status.Waiting)
			{
				// start dragging - the user has moved far enough
				m_ActionIsKeyboard = false;
				if (m_Tool == Shape.Shapes.Selector)
				{
					if (m_SelectClickHit != null)
					{
						// dragged from within a shape - move it
						// if shift-selecting, make sure the clicked shape stays selected
						if (SelectToggles && m_SelectClickHit != null && !m_Page.IsShapeSelected(m_SelectClickHit))
							m_Page.ToggleSelection(m_SelectClickHit);
						StartGrab(target, false);
					}
					else
					{
						// dragged from background: do multi select - drag out selection bounds
						m_ClickDeferredPoint = null; // prevent a select-only on release
						m_CurrentShape = Shape.CreateShape(Shape.Shapes.SelectionBox);
						m_Rejections = 0;
						InitialiseCurrentShape();
						((SelectionBox)m_CurrentShape).MultiMode = SelectToggles;
						m_Status = Status.Adding;
						DoVerbResult2(m_CurrentShape, m_CurrentShape.Start(target), RectangleF.Empty, false,
							Codes.None); // will ignore grid.  Last param bogus - there is no float verb; but only used for Custom response anyway
					}
				}
				else if (e.Button == MouseButtons.Left && m_CurrentShape == null && m_OriginalClickPoint != null)
				{
					{
						TriggerStart(m_OriginalClickPoint, false); // this would have happened on mouse up, but need to do it immediately so we can start dragging
						if (m_Status == Status.Adding)
							m_DragMode = true;
						else
							m_ButtonDown = false; // needed to prevent adding another new shape on the rising edge.  If not adding it implied the shape completed immediately
					}
				}
			}
			DoCursorMove(target, false);
		}

		private void DoCursorMove(ClickPosition target, bool withKeyboard)
		{
			// the part of cursor movement which is shared between keyboard and mouse cursors
			if (m_Page == null)
			{
				Utilities.LogSubError("DoCursorMove with no page", false, true);
				return;
			}
			RectangleF invalidArea = RectangleF.Empty;
			//Dim bolSnapToShapes As Boolean = False ' if we want to shape-shape snap it must happen after Float (whereas snap to grid, or snap mouse to shapes is before)
			// generate targets
			Geometry.Extend(ref invalidArea, m_Page.SocketRefreshBoundary());
			target.SnapMode = EffectiveSnap();
			invalidArea = m_Page.TargetRefreshBoundary(m_Zoom);
			if (m_CurrentShape is TransformMove)
			{
				// special case - this snaps shape-shape not point-shape. This is done by the Float verb
				// update of points is done below
			}
			else if (target.SnapMode == Shape.SnapModes.Shape)
				m_Page.GenerateTargets(UserSocket.CreateForPoint(target.Exact), m_Zoom);
			if (target.SnapMode == Shape.SnapModes.Socket)
			{
				m_Page.Sockets = m_Page.FindSockets(target.Exact, m_Zoom);
				Geometry.Extend(ref invalidArea, m_Page.SocketRefreshBoundary());
			}
			else
				m_Page.Sockets.Clear();
			if (m_Status == Status.Adding && m_CurrentShape != null)
			{
				Geometry.Extend(ref invalidArea, m_CurrentShape.RefreshBounds());
				if (m_CurrentShape.Float(target) == Shape.VerbResult.Unchanged)
				{
				}
				else
					Geometry.Extend(ref invalidArea, m_CurrentShape.RefreshBounds());
				//rctInvalid.Inflate(10, 10) ' what is this for?!
				if (m_CurrentShape.GetClass() == Shape.Classes.Real)
					UpdateTargetHover(m_CurrentShape.Middle(), m_CurrentShape);
				UpdateDiagnostic();
			}
			ActiveGrabSpot = HitTestGrabSpots(target.Exact);
			UpdateHoverPrompts();
			if (!invalidArea.IsEmpty)
			{
				InvalidateData(invalidArea, InvalidationBuffer.Current); // current includes targets and sockets
				if (m_Tool == Shape.Shapes.SelectionBox)
					InvalidateData(invalidArea, InvalidationBuffer.Selection);
			}
		}

		private void EditableView_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			if (this.DesignMode)
				return;
			m_ButtonDown = false; // ensures that the mouse up is ignored
			m_DragTrigger = Rectangle.Empty; // and (on mono) stops drag triggering if dbl click slow to execute
			if (e.Button != MouseButtons.Left)
				return;
			if (m_Tool == Shape.Shapes.Selector && !Grabbing && (m_Page.SelectedShapes.FirstOrDefault()?.CanDoubleClickWith(m_Page.SelectedShapes) ?? false))
			{
				Shape shape = m_Page.SelectedShapes[0];
				if (shape.DoubleClickText() != null)
				{
					shape.DoDoubleClick(this, ClickPosition.Sources.Mouse);
					return; // in this case it is important for the NaL that the code below doesn't also run, as that may cancel the newly created arc again
				}
			}
			if ((m_Status == Status.Adding || m_Status == Status.MakingSolid))
			{
				// it is possible to trigger this even if the shape made itself completed on the previous click (Choose verb)
				// e.g. the Pie chart changes its mind if it is genuinely completed
				if (m_SpawnShape != null)
				{
					// in this case we need to complete the original shape, and ditch the spawned shape
					// this relies on the double-click being faster than the solidifying time, so that m_shpCurrent is still the previous shape
					m_SpawnShape = null;
					m_Status = Status.Waiting;
				}
				if (m_Status == Status.MakingSolid)
					ForceConcludeSolidifying();
				Shape.VerbResult result = Shape.VerbResult.Unchanged;
				RectangleF refresh = RectangleF.Empty;
				if (m_CurrentShape != null)
				{
					refresh = m_CurrentShape.RefreshBounds();
					result = m_CurrentShape.CompleteRetrospective();
					m_Status = Status.Adding; // for DoVerbResult
				}
				DoVerbResult2(m_CurrentShape, result, refresh, false, Codes.Complete);
				StoreCurrentTx();
			}
			UpdateDiagnostic();
		}

		#region ClickPosition

		/// <summary>Data about a click.  All coords are in data coordinates </summary>
		public class ClickPosition
		{
			/// <summary>we need to keep this, because some tools might use snapping, but use the exact coordinates in certain circumstances
			/// (e.g. transformation tool selecting a different shape on start)</summary>
			public PointF Exact;

			public PointF Snapped; // but will be the same as Exact if the current tool does not need snapping.
			public Shape.SnapModes SnapMode; // actual snap applied to .Snapped
			public Shape.SnapModes RequestedSnap; // current user config
			public Page Page; // the page on which we are working
			public float Zoom; // the scale factor of the editing window where the click occurred.  Only occasionally used; can adjust tolerances slightly
			/// <summary>The view the click was within.  Not defined in some other uses of this</summary>
			public StaticView View;
			public Transaction Transaction; // only created sometimes
			public bool WasFocused = true;

			/// <summary>only needed occasionally (to sort out, eg, space in numbergrid which can choose or toggle the 10s)
			/// and not really needed if this is just a move</summary>
			public Sources Source;

			/// <summary>true only for mouse clicks, and if the click is fast enough that it could be the second click of a double-click</summary>
			/// <remarks>Not actually used at the moment</remarks>
			public bool PossibleDoubleClick;

			public enum Sources
			{
				Mouse,
				Keyboard,
				VerbButton, // one of the buttons on the toolbar which triggers any action - probably mouse in effect
				Pad, // popup numberpad or similar
				Deferred, // probably keyboard - but action has been deferred and triggered from a timer later (to avoid Modal handling during keyboard)

				Irrelevant // can be used when (eg) moving cursors - since it isn't going to be needed, no point tracking the source of the move
						   // any function which needs Source should assert/check against Irrelevant
			}

			public ClickPosition(PointF pt, Page page, float zoom, Shape.SnapModes snapMode, Shape.SnapModes requestedSnap, StaticView view, Sources source)
			{
				Exact = pt;
				Snapped = pt;
				SnapMode = snapMode;
				Page = page;
				Zoom = zoom;
				View = view;
				RequestedSnap = requestedSnap;
				Source = source;
			}

			public float ScalarSnapStep(float indicativeAngle)
			{
				return SnapMode == Shape.SnapModes.Grid ? Page.Paper.ScalarSnapStep(indicativeAngle) : 0;
			}

			/// <summary>Only valid until next update - this is deduced from the page</summary>
			public Target SnapTarget
			{
				get
				{
					if (SnapMode != Shape.SnapModes.Shape)
						return null;
					if (Page.ActiveTarget == null || !Page.ActiveTarget.Position.Equals(Snapped))
						return null;
					return Page.ActiveTarget;
				}
			}

			/// <summary>Returns Snapped, unless grid snap and Snapped coincides with the parameter, in which case it jumps one grid space</summary>
			/// <remarks>This is usually used for the first line in a shape so that something is drawn right away</remarks>
			public PointF SnappedExcluding(PointF exclude)
			{
				if (SnapMode != Shape.SnapModes.Grid || !exclude.ApproxEqual(Snapped))
					return Snapped;
				float X = Page.Paper.XInterval;
				return new PointF(Snapped.X + X, Snapped.Y);
			}

			public override string ToString()
			{
				// Only used for diagnostic events
				return Exact + " from " + Source;
			}

		}

		#endregion

		private ClickPosition MouseEventCoordinates(int X, int Y, ClickPosition.Sources source)
		{
			// returns the coordinates to use for this mouse event in data coordinates, and also snapped, if appropriate
			ClickPosition target = new ClickPosition(MouseToData(new Point(X, Y)), m_Page, m_Zoom, EffectiveSnap(), m_SnapMode, this, source)
			{ Transaction = m_OngoingTransaction, WasFocused = m_WasFocused };
			// snap the coordinates, if appropriate...
			if (EffectiveSnap() == Shape.SnapModes.Off)
			{
			}
			else if (EffectiveSnap() == Shape.SnapModes.Shape)
			{
				if (m_Page.HasActiveTarget)
					target.Snapped = m_Page.ActiveTarget.Position;
			}
			else if (EffectiveSnap() == Shape.SnapModes.Grid)
				target.Snapped = m_Page.Paper.SnapPoint2(target.Snapped);
			else if (EffectiveSnap() == Shape.SnapModes.Socket)
			{
				target.Snapped = m_Page.NearestSocket(target.Exact).Position;
				if (target.Snapped.IsEmpty)
					target.Snapped = target.Exact;
			}
			else if (EffectiveSnap() == Shape.SnapModes.Angle)
			{
				if (m_CurrentShape != null)
					target.Snapped = m_CurrentShape.DoSnapAngle(target.Exact);
				else
					Debug.Fail("Cannot angle snap - no current shape");
			}
			else
				Debug.Fail("Unknown snap mode");
			//    End If
			//End If
			return target;
		}

		private void UpdateMousePosition(ref ClickPosition target, bool withKeyboard)
		{
		}

		internal bool Dragging => m_DragMode;

		/// <summary>Changes m_shpContainerHover.  Forces repaint within the current layer internally</summary>
		/// <param name="pt">The reference point - usually the mouse position, but is the middle of the shape if it is being newly created.</param>
		/// <remarks>Caller is probably doing its own invalidation, but seems more efficient to make a separate Invalidate call here, because this is probably affecting fewer buffers</remarks>
		private void UpdateTargetHover(PointF pt, Shape forShape = null)
		{
			List<Shape> forList = forShape == null ? m_Page.SelectedShapes : new List<Shape>() { forShape };
			Shape hover = (Shape)m_Page.FindTarget(pt, forList);
			RectangleF refreshBounds = RectangleF.Empty;
			if (m_TargetHover != null)
				refreshBounds = m_TargetHover.RefreshBounds();
			if (hover != null)
				Geometry.Extend(ref refreshBounds, hover.RefreshBounds());
			if (!refreshBounds.IsEmpty)
			{
				refreshBounds.Inflate(GUIUtilities.LARGEEXTRAREFRESH, GUIUtilities.LARGEEXTRAREFRESH); // to cope with the rather wider highlight borders
				InvalidateData(refreshBounds, InvalidationBuffer.Current, false);
			}
			m_TargetHover = hover;
		}

		private void RemoveTargetHover()
		{
			if (m_TargetHover == null)
				return;
			var refreshBounds = m_TargetHover.RefreshBounds();
			refreshBounds.Inflate(GUIUtilities.LARGEEXTRAREFRESH, GUIUtilities.LARGEEXTRAREFRESH); // to cope with the rather wider highlight borders
			InvalidateData(refreshBounds, InvalidationBuffer.Current, false);
			m_TargetHover = null;
		}

		/// <summary>Puts all of the given shapes into the container, adding any changed containers to objTransaction</summary>
		private void PutShapesInContainer(List<Shape> list, IShapeContainer container, PointF target, Transaction transaction)
		{
			// currently used to put the selection into the target container when moving; but by specifying things as parameters this might be useful elsewhere
			Debug.Assert(transaction != null);
			if (container == null)
				container = m_Page; // Nothing indicates no container shape selected; in this case the shapes are being put on the page
			HashSet<IShapeContainer>
				hashChangedContainers = new HashSet<IShapeContainer>(); // containers which we need to call FinishedModifyingContents.  ObjContainer will be added to this only if it is changed
			foreach (Shape shape in list)
			{
				Debug.Assert(shape != container);
				if (shape.Parent != container && (!(shape.Parent is Shape) || !shape.IsWithin(list))) // !list.Contains((Shape)shape.Parent)))
				{
					// last condition is needed in case select all has selected both containers and contents, and both are being moved
					// second (typeof) condition needed, otherwise the colList.Contains would crash with a type cast error
					Debug.Assert(!(container is Shape) || ((Shape)container).Parent != shape); // another check that we are not putting a container within a container inside itself
					transaction.Edit((Datum)container);
					transaction.Edit((Datum)shape.Parent);
					hashChangedContainers.Add(container); // Putting this inside the loop just avoids calling Finished... if nothing has changed
					hashChangedContainers.Add((IShapeContainer)shape.Parent);
					shape.Container.Contents.Remove(shape);
					shape.Parent = (IShapeParent)container;
					container.Contents.Add(shape);
				}
			}
			if (hashChangedContainers.Count == 0)
			{
				// i.e. nothing was changed; all shapes were already in this container
				// in this case we give the container a chance to rearrange the shapes if it wants to
				if (container.MoveWithin(list, target, m_GrabMove, transaction))
					hashChangedContainers.Add(container);
			}
			foreach (IShapeContainer changed in hashChangedContainers)
			{
				changed.FinishedModifyingContents(transaction, m_GrabMove);
			}
		}

		#region Grab movement

		private void StartGrab(ClickPosition dataPosition, bool keyCursor)
		{
			//Debug.WriteLine("StartGrab");
			Debug.Assert(m_GrabMove == null);
			Globals.StoreEvent("StartGrab at " + dataPosition);
			m_DragTrigger = Rectangle.Empty;
			Shape shape = m_SelectClickHit;
			m_ClickDeferredPoint = null; // prevent a select-only on release
			if (m_Page.SelectedCount == 0)
				return;
			m_ActionIsKeyboard = keyCursor;
			m_OngoingTransaction = new Transaction();
			if (m_GrabSpotHit != null)
			{
				m_GrabMove = new Shape.GrabMovement(m_GrabSpotHit, m_Page, m_SnapMode, m_OngoingTransaction);
				m_Page.SelectPathOnShape(shape, Target.FromGrabSpot(m_GrabSpotHit));
			}
			else
			{
				if (shape != null) // Allow the shape the first opportunity to do some custom action (this only applies if no actual GrabSpot was clicked on)
					m_GrabMove = shape.GetCustomGrabMove(dataPosition, m_OriginalClickPoint, m_OngoingTransaction);
				if (m_GrabMove == null)
				{
					if (!m_Page.SelectionAnyAllows(Shape.AllowedActions.TransformMove))
						return;
					m_Page.FilterSelectionByAllows(Shape.AllowedActions.TransformMove);
					Shape.GrabSpot grabSpot = new Shape.GrabSpot(shape, Shape.GrabTypes.Move, dataPosition.Exact);
					m_GrabMove = new Shape.GrabMovement(grabSpot, m_Page, m_SnapMode, m_OngoingTransaction);
				}
			}
			if (!AllowSnap())
				m_GrabMove.SnapMode = Shape.SnapModes.Off;
			m_GrabMove.ControlKey = m_ControlGrab;
			m_GrabMove.PositionMouseUnsnapped = dataPosition.Exact;
			if (m_GrabMove.SnapMode == Shape.SnapModes.Grid)
			{
				// so that movement is in grid multiples (_to_ position will be snapped; if _from_ wasn't we would move in fractions of a grid
				// in fact moving onto the grid - but I think it is better to move in grid steps than to jump an off-grid object onto the grid unexpectedly)
				m_GrabMove.PositionGridSnapped = m_Page.Paper.SnapPoint2(m_GrabMove.Position);
			}
			//List<Shape> shapes = new List<Shape>(); 
			foreach (Shape shp in m_Page.SelectedShapes)
			{
				Shape use = shp;
				m_OngoingTransaction.Edit(use);
				//shapes.Add(use);
				if (!use.StartGrabMove(m_GrabMove))
				{
					AbortGrab();
					return;
				}
			}

			if (m_GrabMove.GrabType == Shape.GrabTypes.BezierInactive)
			{
				AbortGrab();
				if (MessageBox.Show(Strings.Item("ConvertPath_Auto"), RootApplication.AppName, MessageBoxButtons.YesNo) == DialogResult.Yes)
					Globals.Root.PerformAction(Verb.Find(Codes.ConvertToPath));
				return;
			}
			m_GrabMove.SetGrabMoveTransform(m_Page, m_GrabSpotHit);

			if (m_GrabMove.GrabType == Shape.GrabTypes.Move && m_SnapMode == Shape.SnapModes.Angle)
				InvalidateAll(InvalidationBuffer.Current); // for the guidelines to give feedback that it is orthogonal
			m_Page.MovingSelection = m_GrabMove.UseMovingLogic;
			CheckAngleSnapDisplaysMove();
		}

		/// <summary>ONLY for use during Start.  CancelGrab ends one which has been started</summary>
		private void AbortGrab()
		{
			Globals.StoreEvent("AbortGrab");
			m_GrabMove = null;
			m_DragMode = false;
			m_OngoingTransaction.Cancel();
			m_OngoingTransaction = null;
		}

		private void MoveGrab(ClickPosition position)
		{
			//Debug.WriteLine("MoveGrab");
			// mouse moved during adjustment via grab point
			RectangleF invalid = m_Page.SelectedRefreshBoundary();
			m_GrabMove.CustomPositioning = false;
			if (m_GrabMove.GrabType == Shape.GrabTypes.Move && (ModifierKeys & Keys.Control) == 0)
			{
				// Check for movement between containers only when moving shapes, not resizing
				UpdateTargetHover(position.Exact);
				// The lit up target might or might not be an actual container
				PutShapesInContainer(m_Page.SelectedShapes, m_TargetHover?.AsContainer, position.Exact, m_OngoingTransaction);
			}
			m_PointGrabLastApplied = position;
			if (!m_GrabMove.CustomPositioning)
				DoGrabTransform(position);
			Geometry.Extend(ref invalid, m_Page.SelectedRefreshBoundary());
			InvalidateData(invalid, InvalidationBuffer.Base | InvalidationBuffer.Selection);
			UpdateDiagnostic();
			Globals.OnUpdateInfo();
		}

		private void DoGrabTransform(ClickPosition position)
		{
			// objTransaction is defined iff we are ending
			m_GrabMove.Current = position;
			bool performShapeSnap = false;
			RectangleF invalid = m_Page.TargetRefreshBoundary(m_Zoom); // will often be Empty
			if (AllowSnap())
			{
				switch (m_GrabMove.SnapMode)
				{
					case Shape.SnapModes.Off:
						break;
					case Shape.SnapModes.Shape:
						if (m_GrabMove.Transform is TransformMove)
						{
							if (!m_GrabMove.SimpleShapeSnap)
							{
								// shape snapping is somewhat complicated.  We don't really want to snap the mouse position
								// we want to try and snap every point in the available shapes
								// however, this needs to be done after the initial transformation, we need to try and snap the shape from the location would be given by
								// the current mouse coordinates (which may be very different to the last mouse coordinates applied)
								performShapeSnap = true;
							}
							else
							{
								m_Page.GenerateTargets(UserSocket.CreateForPoint(position.Exact), m_Zoom);
								if (m_Page.HasActiveTarget)
									position.Snapped = m_Page.ActiveTarget.Position;
							}
						}
						break;
					case Shape.SnapModes.Grid:
						// grid snapping is done the usual way
						position.Snapped = m_Page.Paper.SnapPoint2(position.Snapped);
						break;
					case Shape.SnapModes.Socket:
						InvalidateData(m_Page.SocketRefreshBoundary(), InvalidationBuffer.Current);
						m_Page.Sockets = m_Page.FindSockets(position.Exact, m_Zoom);
						InvalidateData(m_Page.SocketRefreshBoundary(), InvalidationBuffer.Current);
						position.Snapped = m_Page.NearestSocket(position.Exact).Position; // if sockets active (ie over shape then one always used)
						if (position.Snapped.IsEmpty)
							position.Snapped = position.Exact;
						break;
					case Shape.SnapModes.Angle:
						Debug.Assert(m_GrabMove.Current == position);
						if (m_Page.SelectedCount > 1)
							m_GrabMove.DoGrabAngleSnapMultiShapes();
						else
							m_Page.SelectedShapes[0].DoGrabAngleSnap(m_GrabMove);
						break;
					default:
						Utilities.LogSubError("Unexpected snap mode");
						break;
				}
			}
			m_GrabMove.Transform?.SetGrabTransform(m_GrabMove.PositionGridSnapped, position.Snapped);
			foreach (Shape shape in m_Page.SelectedShapes)
			{
				shape.GrabMove(m_GrabMove);
			}
			if (performShapeSnap)
			{
				// This has been changed from version 1 to enable snapping when multiple shapes are dragged.  This is needed for the jigsaw selector
				// I'm not sure if it might cause problems at other times though.  Some of the safety tests, such as dropping over identical shapes only work with a single shape
				List<UserSocket> vertices = null;
				Shape singleShape = null; // Can remain nothing and still have snapping
				PointF originalCentre = PointF.Empty;
				PointF currentCentre = PointF.Empty;
				if (m_Page.SelectedCount == 1)
				{
					// Snap command below also assumes only one shape is selected
					singleShape = m_Page.SelectedShapes[0];
					vertices = singleShape.GetPointsWhichSnapWhenMoving();
					originalCentre = m_GrabMove.OriginalShape(singleShape).Middle(); // centre of original shape
					currentCentre = singleShape.Middle();
				}
				else if (m_Page.SelectedCount > 1)
				{
					vertices = new List<UserSocket>();
					RectangleF originalBounds = new RectangleF(); // calculate the original total bounds, to get the original centre of these shapes
					foreach (Shape shape in m_Page.SelectedShapes)
					{
						List<UserSocket> socketsForShape = shape.GetPointsWhichSnapWhenMoving(); // can be nothing to mean no such points
						if (socketsForShape != null)
							vertices.AddRange(socketsForShape);
						Geometry.Extend(ref originalBounds, m_GrabMove.OriginalShape(shape).Bounds);
					}
					currentCentre = m_Page.SelectedBounds(false).Centre();
					originalCentre = originalBounds.Centre();
				}
				// can be nothing meaning either the transformation does not like this, or the shapes being moved can't do this
				if (vertices != null && vertices.Count > 0)
				{
					m_Page.ClearTargets();
					if (((TransformMove)m_GrabMove.Transform).Snap(vertices, m_Zoom, m_Page, singleShape, originalCentre, currentCentre))
					{
						// the transform will have updated itself.  We need to apply it to the shapes again however
						foreach (Shape shape in m_Page.SelectedShapes)
						{
							shape.GrabMove(m_GrabMove);
						}
					}
				}
			}
			Geometry.Extend(ref invalid, m_Page.TargetRefreshBoundary(m_Zoom));
			InvalidateData(invalid, InvalidationBuffer.Current); // all targets are in the current layer.  Ignores if Empty
		}

		private void CancelGrab()
		{
			if (m_GrabMove == null)
				return;
			Globals.StoreEvent("CancelGrab");
			RectangleF invalid = m_Page.SelectedRefreshBoundary();
			foreach (Shape shape in m_Page.SelectedShapes)
			{
				try
				{
					Shape original = m_GrabMove.OriginalShape(shape);
					shape.CopyFrom(original, Datum.CopyDepth.Transform, null);
				}
				catch (Exception ex)
				{
					Utilities.LogSubError(ex);
				}
			}
			m_OngoingTransaction?.Cancel(); // Tidy will =nothing
			TidyGrab(invalid);
			Globals.OnUpdateInfo();
		}

		/// <summary>Stores changes from grab.  It's possible for this to divert to CancelGrab instead</summary>
		private void StoreGrab()
		{
			Globals.StoreEvent("StoreGrab");
			if (m_PointGrabLastApplied == null)
			{
				// mouse never moved - may as well cancel instead
				CancelGrab();
				return;
			}
			RectangleF invalid = m_Page.SelectedRefreshBoundary();
			DoGrabTransform(m_PointGrabLastApplied);
			var newRect = m_Page.SelectedRefreshBoundary();
			if (!newRect.IsEmpty && !newRect.Intersects(m_Page.Bounds))
			{
				// don't allow dragging off page
				CancelGrab();
				return;
			}
			if (m_Page.IsSingleAutoSize && m_Page.SelectedCount == 1 && m_Page.SelectedShapes.First() is Flow && // AndAlso Not AutoSizeWarning Then
				(m_GrabMove.Transform is TransformLinearScale || m_GrabMove.Transform is TransformScale))
			{
				// The user is moving the container to which it automatically match the page size.
				// Better to just update it
				var container = m_Page.SelectedShapes.First();
				m_OngoingTransaction.Edit(m_Page);
				m_Page.SetSizeExcludingMargin(container.Bounds.Size);
				((IAutoSize)container).SetBounds(m_Page.Bounds, null);
				Globals.OnCurrentPageSizeChanged();
			}
			List<Shape> shapes = m_Page.SelectedShapes.Clone(); // Need to work on a copy of the list since DropNotContainer can delete shapes
			foreach (Shape shape in shapes)
			{
				// mark these shapes as changed, in case other shapes are linked to them
				shape.Status = Shape.StatusValues.Moved;
				if (m_TargetHover != null)
				{
					if (m_TargetHover.AsContainer == null)
					{
						// When dropping shapes onto containers PutShapesInContainer has already moved the shape into the container
						// for other types of target they perform any necessary updates now:
						((IShapeTarget)m_TargetHover).DropNotContainer(shape, m_OngoingTransaction);
					}
					else
						((IShapeContainer)m_TargetHover).FinishedModifyingContents(m_OngoingTransaction); // Flow, for example, needs to update positions now that DoGrabTransform has corrupted them
				}
			}
			Globals.Root.StoreNewTransaction(m_OngoingTransaction);
			TidyGrab(invalid);
		}

		/// <summary>Shared between CancelGrab and StoreGrab.  rctInvalid should be refresh boundary before current changes</summary>
		private void TidyGrab(RectangleF invalidArea)
		{
			if (m_GrabMove != null)
			{
				if (m_GrabMove.GrabType == Shape.GrabTypes.Move && m_SnapMode == Shape.SnapModes.Angle)
					InvalidateAll(InvalidationBuffer.Current); // for the guidelines to give feedback that it is orthogonal
				m_GrabMove.Dispose();
			}
			RemoveTargetHover();
			m_GrabMove = null;
			m_Page.ClearTargets();
			m_Page.Sockets.Clear();
			m_PointGrabLastApplied = null;
			m_OngoingTransaction = null;
			Geometry.Extend(ref invalidArea, m_Page.SelectedRefreshBoundary());
			m_Page.MovingSelection = false;
			InvalidateData(invalidArea, InvalidationBuffer.Base | InvalidationBuffer.Selection);
			UpdateDiagnostic();
			CheckAngleSnapDisplaysMove();
		}

		internal bool Grabbing
		{
			get { return m_GrabMove != null; }
		}

		#endregion

		private void EditableView_MouseLeave(object sender, EventArgs e)
		{
			if (this.DesignMode || m_Page == null)
				return;
			if (m_Page.HasTargets)
			{
				RectangleF invalid = m_Page.TargetRefreshBoundary(m_Zoom);
				m_Page.ClearTargets();
				InvalidateData(invalid, InvalidationBuffer.Current);
			}
			DisplayMousePosition?.Invoke(null);
		}

		#endregion

		#region Key cursor

		// but that would mean editing it if the control moved)
		private static readonly Size KEYCURSORSIZE = new Size(24, 32); // area which needs to be invalidated to draw it (actually the icon is slightly bigger, but transparent at the edge)

		/// <summary>if using separate cursors, both can float and drag (but not simulataneously if not m_bolSeparateCursors). Ss defined, but not necessarily enforce (to be determined) if not m_bolSeparateCursors</summary>
		private bool m_ActionIsKeyboard;

		/// <summary>the location of the key cursor, if used, otherwise standard cursor</summary>
		internal Point CursorPositionLocal
		{
			get { return PointToClient(Cursor.Position); }
			set { Cursor.Position = this.PointToScreen(value); }
		}

		/// <summary>True if the current action, if any, is using the keyboard cursor rather than the Windows cursor.  Is defined now regardless of whether split cursors are used
		/// Returns UseDefault if nothing is happening</summary>
		internal bool? ActionUsingKeyboard
		{
			get
			{
				if (m_CurrentShape == null && m_GrabMove == null)
					return null;
				return m_ActionIsKeyboard;
			}
			set
			{
				if (!value.HasValue)
					return;
				m_ActionIsKeyboard = value.Value;
			}
		}

		/// <summary>Move cursor.  Implication is that this is in response to the keyboard, so the keyboard cursor moves if there are split cursors</summary>
		internal void MoveCursor(Point screenPoint, bool pointIsData = false)
		{
			Cursor.Position = screenPoint;
			Point local = this.PointToClient(screenPoint);
			ClickPosition target = MouseEventCoordinates(local.X, local.Y, ClickPosition.Sources.Irrelevant);
			DoCursorMove(target, true);
		}

		/// <summary>Move cursor.  Implication is that this is in response to the keyboard, so the keyboard cursor moves if there are split cursors</summary>
		internal void MoveCursor(Size difference)
		{
			MoveCursor(Cursor.Position + difference);
		}

		#endregion

		#region Verbs and editing


		private ClickPosition m_ClickDeferredPoint; // defined if the selection, and any other effects of "clicking" have not been processed immediately

		// (Since the click may be start of a drag, or the first of a double-click, we cannot do the clicking selects the next item until we are sure
		// that this action isn't actually something which should act on the current shape)
		private Timer m_tmrDelayedClick = new Timer() { Interval = SystemInformation.DoubleClickTime, Enabled = false };

		private void ChooseDown(ClickPosition target, bool wasKeyboard)
		{
			// first part of the choose verb, equivalent to mouse down.  (It is split into two parts because some of this code
			// needs to wait and see if the user is about to drag)
			m_tmrDelayedClick.Enabled = false;
			if (m_GrabMove != null)
				return;
			if (m_CurrentShape != null)
			{
				if ((m_CurrentShape.Flags & Shape.GeneralFlags.ChooseFallingEdge) > 0)
					TriggerVerb2(m_CurrentShape, Codes.Choose, target, false);
				return;
			}

			if (m_TypingTransaction != null)
			{
				// abandon any typing; but we mustn't do this if the user clicks again on the current typing shape
				// (because double clicking when already typing has a different meaning, and if we start typing on the first click it would have the original start typing meaning)
				Shape shp = m_Page.HitTest(target.Exact, m_Zoom, Page.HitTestMode.OnlySelection);
				if (shp == null || shp != TypingShape(false))
					ConcludeTyping();
			}
			m_GrabSpotHit = HitTestGrabSpots(target.Exact);

			// equation works rather like the pointer tool when clicking on an equation (especially it can select text the same)
			if (IsSingleSelector)
				SelectorClick(target, wasKeyboard);
			//else if (m_Tool == Shape.Shapes.Label)
			//{
			//	// no need to wait for up on Label - just label the first shape hit
			//	m_DragTrigger = Rectangle.Empty; // this never drags to start
			//	Shape shp = m_Page.HitTest(target.Exact, m_Zoom, Page.HitTestMode.StartSelectionFilled);
			//	// note cannot merge the two HitTest calls as they are different
			//	if (shp != null && shp.SupportsTextLabel)
			//	{
			//		shp.CreateLabel((Shape.TextStyleC)Globals.StyleParameterDefaultObject(Parameters.TextColour));
			//		//Globals.ParameterValue(Parameters.Tool) = Shape.Shapes.Selector' It did this in v1; but not sure now why.  Eive requested change, in email 25/11/13
			//		m_Page.SelectOnly(shp);
			//	}
			//	//Else all other tools start a new shape, but on mouse up
			//}
		}

		private void ChooseUp(ClickPosition target, bool fromKeyboard)
		{
			// the second part of the choose verb, equivalent to mouse up.  If this is in response to mouse activity, this may not happen if the mouse was dragged
			// check if we need to do a select-only
			// bolFromKeyboard is used to distinguish key-drags and mouse drags
			if (m_GrabMove != null)
			{
				StoreGrab();
				return;
			}
			if (m_SelectClickHit != null && m_ClickDeferredPoint != null && !fromKeyboard)
			{
				// We can be sure the user wasn't dragging, but we still need to be sure it isn't a double-click
				// as far as I can see the only way to do this is on a timer
				m_tmrDelayedClick.Enabled = true;
			}
			// check for click on grab spot
			Shape.GrabSpot grabSpot = HitTestGrabSpots(target.Exact);
			if (m_GrabSpotHit != null && m_GrabSpotHit == grabSpot)
			{
				StartGrab(target, fromKeyboard);
				return;
			}
			if (m_Status == Status.Adding)
			{
				//'normal clicks for adding shapes done in the mouse event handler itself
			}
			else
			{
				if (m_Status == Status.MakingSolid)
					ForceConcludeSolidifying();
				// not adding, or doing any sort of drag - normal click
				if (!IsSingleSelector) // select  done in mouse down (ish) all other tools start a new shape
					TriggerStart(target, fromKeyboard);
			}
		}

		public void TriggerVerb(Codes code, ClickPosition.Sources source)
		{
			// this version called externally - always in response to verbs; effectively keyboard presses
			this.Focus();
			m_DragMode = false;
			m_DragTrigger = Rectangle.Empty;
			m_ButtonDown = false;
			Point ptMouse = CursorPositionLocal;
			ClickPosition clickPosition = MouseEventCoordinates(ptMouse.X, ptMouse.Y, source);
			// do any special actions which would have been done by the mouse handlers internally...
			// (these may not be so easy inside the next TriggerVerb due to the line filtering out if m_shpCurrent is nothing)
			switch (code)
			{
				case Codes.Cancel:
					if (m_GrabMove != null)
					{
						CancelGrab();
						return;
					}
					break;
			}
			Shape.VerbResult result = Shape.VerbResult.Unexpected;
			if (m_CurrentShape == null && m_GrabMove == null)
			{
				if (m_Page.SelectedCount == 1 && m_Page.SelectedShapes[0].AllowVerbWhenComplete(code))
					result = TriggerVerb2(m_Page.SelectedShapes[0], code, clickPosition, true);
				if (result == Shape.VerbResult.Unexpected)
				{
					// Not handled by the shape itself.  Otherwise ignored, except for one special case, which can change the default rather than the current shape
					switch (code)
					{
						case Codes.Increment:
						case Codes.Decrement:
							if (m_Tool == Shape.Shapes.Polygon)
								Polygon.ChangeDefaultSides(code == Codes.Increment ? 1 : -1);
							result = Shape.VerbResult.Completed;
							break;
					}
				}
				if (result != Shape.VerbResult.Unexpected)
					return;
			}
			if ((code == Codes.Choose || code == Codes.Complete || m_CurrentShape != null) && code != Codes.ChooseExisting)
			{
				// when creating shapes most verbs only happen once the shape is started.
				// this used? - yes for space and return.
				TriggerVerb2(m_CurrentShape, code, clickPosition, true);
			}
		}

		private Shape.VerbResult TriggerVerb2(Shape shp, Codes code, ClickPosition target, bool withKeyboard)
		{
			// bolKeyboard indicates whether this verb was triggered by the mouse or keyboard (only important for Choose, Complete)
			// can only be called if shp is defined, except for Choose
			// the verb result is handled internally, but also returned; mainly so that the caller can detect if anything at all happened
			bool modifyingExisting = shp != null && shp != m_CurrentShape;
			Shape.VerbResult result = Shape.VerbResult.Unchanged;
			if (m_OngoingTransaction == null)
				m_OngoingTransaction = new Transaction();
			target.Transaction = m_OngoingTransaction;
			RectangleF refreshBounds = RectangleF.Empty;
			if (shp != null)
				refreshBounds = shp.RefreshBounds(m_Page.MovingSelection);
			switch (code)
			{
				case Codes.Choose:
					// this can be called in response to mouse up
					if (m_Status == Status.MakingSolid)
						ForceConcludeSolidifying();
					UpdateMousePosition(ref target, false);

					if (shp != null)
					{
						if (shp.GetClass() == Shape.Classes.Transformation && ((Transformation)shp).NextChoosePerformsSelection())
						{
							// not Shape.TransformationType(m_eTool) - ImportedImage can occur with any tool selected
							DoTransformationSelectVerb(target, true);
						}
						result = shp.Choose(target);
					}
					else
					{
						Debug.Assert(withKeyboard); // I think at the moment the mouse will do this in the event handler without using this function
						if (m_OngoingTransaction.Empty)
						{
							if (m_GrabMove == null)
								m_OngoingTransaction = null; // because ChooseUp expects this
						}
						else
						{
							//Debug.Fail("TriggerVerb: Choose: ongoing transaction is not empty with no shp")
							// triggers when moving from stack with space.  Not a problem
						}
						if (m_GrabMove == null)
							ChooseDown(target, withKeyboard); // which might call back to this function as needed
															  // ChooseDown not helpful if grab moving - tends to change selected and cause some oddly delayed errors
						ChooseUp(target, withKeyboard);
						return Shape.VerbResult.Unchanged;
					}
					break;
				case Codes.ChooseExisting: // only used when a shape is already complete
					Debug.Assert(m_CurrentShape == null);
					result = shp.Choose(target);
					break;
				case Codes.Cancel:
					result = shp.Cancel(target);
					break;
				case Codes.Complete:
					if (shp == null) // E.g. for GrabSpot - acts as choose
						return TriggerVerb2(null, Codes.Choose, target, withKeyboard);
					else
						result = shp.Complete(target);
					break;
				case Codes.Increment:
				case Codes.Decrement:
					result = shp.OtherVerb(target, code);
					break;
				default:
					Debug.Fail("Unexpected Shape verb in view.TriggerVerb: " + code);
					break;
			}
			if (result == Shape.VerbResult.Unexpected && modifyingExisting)
				result = Shape.VerbResult.Unchanged; // when modifying an existing shape, Unexpected will often be the result
			DoVerbResult2(shp, result, refreshBounds, modifyingExisting, code);
			if (m_CurrentShape == null)
				StoreCurrentTx();
			return result;
		}

		private bool DoTransformationSelectVerb(ClickPosition target, bool WithKeyboard)
		{
			// we are on the Start/Choose step for the transformation which fixes the shapes to be moved.  We need to update the selection in various cases...
			// we need to also check that the user has hit something selected.  This function returns false if this verb should not proceed
			if (m_Page.SelectedCount == 0)
			{
				// if using a transformation and nothing is currently selected, select whatever the user clicked on
				SelectorClick(target, WithKeyboard);
				if (m_Page.SelectedCount == 0)
					return false;
			}
			else
			{
				// if the user did not click on any of the selected shapes, but a different shape instead, select that shape and continue
				if (m_Page.HitTest(target.Exact, m_Zoom, Page.HitTestMode.OnlySelection) == null)
				{
					Shape clicked = m_Page.HitTest(target.Exact, m_Zoom, Page.HitTestMode.StartBehindSelectionWithinBounds);
					if (clicked == null)
						return false; // transformation only starts if clicking on a shape
					SelectorClick(target, WithKeyboard); // and continue using that shape
				}
			}
			return true;
		}

		/// <summary>Processes the result returned from most of the shape verbs</summary>
		/// <param name="shp"></param>
		/// <param name="result"></param>
		/// <param name="currentBoundsBefore">rctCurrentBefore is RefreshBounds of shp before verb was applied (Empty for start)</param>
		/// <param name="modifyingExisting">Should be true is this is an existing (completed) shape to which a verb has been applied; only makes minor differences</param>
		/// <param name="code"></param>
		private void DoVerbResult2(Shape shp, Shape.VerbResult result, RectangleF currentBoundsBefore, bool modifyingExisting, Codes code)
		{
			switch (result)
			{
				case Shape.VerbResult.TransformationComplete:
					m_CurrentShape = null;
					m_Status = Status.Waiting;
					m_Page.MovingSelection = false;
					StoreCurrentTx();
					OngoingShapeChanged();
					break;
				case Shape.VerbResult.Completed:
					m_DragTrigger = Rectangle.Empty; // in case the shape is completed on the initial mouse down; in which case dragging shouldn't do anything!
					if (m_Status == Status.Adding)
						StartSolidifying();
					else
					{
						// if the shape had previously spawned a new one, then returning Completed here cancels that
						m_SpawnShape = null;
						AnimationController.EnsureNoAnimation(m_Solidify);
					}
					break;
				case Shape.VerbResult.Substitute: // Now permits new shape to be Nothing; indicating a shape has completed, but doesn't actually want to be added to the page (e.g. pixel layer drawing)
					InvalidateData(shp.RefreshBounds(), InvalidationBuffer.All);
					Shape create = shp.Spawn();
					m_SpawnShape = null;
					m_Status = Status.Waiting;
					m_CurrentShape = null;
					if (create != null)
					{
						create.Parent = m_Page;
						if (!m_Page.Contains(create))
						{
							m_OngoingTransaction.Create(create);
							m_Page.AddNew(create, m_OngoingTransaction);
						}
					}
					StoreCurrentTx();
					m_Page.SelectOnly(create);
					OngoingShapeChanged();
					break;
				case Shape.VerbResult.Unchanged:
					break;
				case Shape.VerbResult.Destroyed:
					Shape spawnedFrom = m_SpawnedFrom; // this is cleared by AbandonCurrent
					AbandonCurrent(true);
					if (spawnedFrom != null)
						m_Page.SelectOnly(spawnedFrom);
					else if (m_Page.SelectedCount > 0) // this is important for SelectionBox
						ForceUpdateGrabSpots();
					break;
				case Shape.VerbResult.Unexpected:
				case 0:
					Debug.Fail("Shape verb returned Unexpected");
					break;
				case Shape.VerbResult.Spawn:
					if (modifyingExisting)
					{
						// Was the NumberArcLine, but not used now probably
						m_SpawnShape = null; // Because this variable is mainly used to start the shape during the Conclude/Solidifying functions
						m_CurrentShape = shp.Spawn(); // must not assign .Parent, because the NumberArcLine does that differentlym_shpCurrent =
						m_Status = Status.Adding;
					}
					else if (m_Status == Status.Adding)
					{
						if (Shape.GetClass(shp.ShapeCode) != Shape.Classes.Transformation)
						{
							m_SpawnShape = shp.Spawn(); //  will work OK if it returns nothing (which is sort of permissible)
							if (m_SpawnShape != null)
							{
								m_SpawnShape.Parent = m_Page;
								m_SpawnedFrom = shp;
							}
							StartSolidifying();
							// m_shpCurrent is now the spawned shape.  The status could not be changed before StartSolidifying, because this might cause an assertion if storing a transaction
							if (m_CurrentShape != null)
								m_CurrentShape.Status = Shape.StatusValues.Creating;
						}
						else // if a transformation returns Spawn it doesn't want to create a new transformation, this signals that it wants to store the current transaction
							StoreCurrentTx();
						if (m_CurrentShape != null)
							DisplayShapeStyles?.Invoke(m_CurrentShape); // needed for spawn from NG - can have different lines to those showing in palette
					}
					else
					{
						// otherwise... the spawned object should already be set
						Debug.Assert(m_Status == Status.MakingSolid);
						Debug.Assert(m_SpawnShape != null,
							"A shape should not return Spawn from CompleteRetrospective if the shape is already complete and did not return Spawn at that time - if a new shape must be created then the result code Spawn must be returned BOTH by the initial Choose/Complete and by CompleteRetrospective");
					}
					break;
				case Shape.VerbResult.Continuing:
					if (modifyingExisting)
						ForceUpdateGrabSpots();
					if (m_Status == Status.Adding)
						UpdateDiagnostic();
					break;
				case Shape.VerbResult.Rejected: // start has already dealt with this.  Others just ignore it.
					if (!modifyingExisting) // if it is an existing shape there are all sorts of reasons why it might want to reject a verb, so this is not considered such a problem
					{
						// also, it is less likely that the interface gets stuck: the user can just select a different shape.
						m_Rejections += 1;
						if (m_Rejections >= 5)
						{
							AbandonCurrent();
							Debug.WriteLine("Excessive Rejected results - abandoning the current shape as a safety measure");
							m_Rejections = 0;
						}
					}
					break;
				case Shape.VerbResult.Custom:
					if (m_OngoingTransaction == null)
						m_OngoingTransaction = new Transaction();
					CustomVerbResponse(shp, currentBoundsBefore, ref m_OngoingTransaction, code);
					break;
				default:
					Debug.Fail("Unexpected code returned from Shape.Start"); // Spawn is not allowed
					break;
			}

			if (result != Shape.VerbResult.Unchanged)
			{
				RectangleF refresh = currentBoundsBefore;
				if (shp != null)
					Geometry.Extend(ref refresh, shp.RefreshBounds());
				if (modifyingExisting)
					InvalidateData(refresh, InvalidationBuffer.Base | InvalidationBuffer.Selection);
				else if (result == Shape.VerbResult.Completed || result == Shape.VerbResult.Spawn)
					InvalidateData(refresh, InvalidationBuffer.Base | InvalidationBuffer.Current);
				else
					InvalidateData(refresh, InvalidationBuffer.Current);
			}
			PromptsDirty();
			UpdateDiagnostic();
		}

		/// <summary>Stores m_OngoingTransaction, if defined, and clears it</summary>
		private void StoreCurrentTx()
		{
			if (m_OngoingTransaction != null)
				Globals.Root.StoreNewTransaction(m_OngoingTransaction);
			m_OngoingTransaction = null;
		}

		private void CustomVerbResponse(Shape shape, RectangleF currentBoundsBefore, ref Transaction transaction, Codes code)
		{
			transaction.Edit(m_Page);
			switch (shape.ShapeCode)
			{
				case Shape.Shapes.SetOrigin:
					m_Page.Origin = ((SetOrigin)shape).Origin;
					InvalidateData(shape.Bounds, InvalidationBuffer.Current);
					if (Globals.Root.CurrentConfig.ReadBoolean(Config.Display_Origin))
						InvalidateAll(InvalidationBuffer.Base); // fills width and height if drawn
					PromptsDirty();
					break;
				case Shape.Shapes.SetRotation:
					PointF point = ((SetRotation)shape).SelectedPoint;
					TransformRotate.RotationPoint = point;
					m_Page.SetCurrentRotationPoint(point, true);
					InvalidateData(shape.Bounds, InvalidationBuffer.Current);
					Globals.OnRotationInfoChanged();
					// no need to worry about invalidating the displayed marker within page as that happens on a timer anyway
					break;
				default:
					Debug.Fail("Unexpected Custom verb response, shape type=" + shape.ShapeCode);
					break;
			}
			m_CurrentShape = null;
			m_Status = Status.Waiting;
			Globals.Root.StoreNewTransaction(transaction);
			transaction = null;
		}

		private void SelectorClick(ClickPosition target, bool wasKeyboard)
		{
			m_ClickDeferredPoint = null;
			m_tmrDelayedClick.Enabled = false;
			if (m_Page.SelectedCount == 0)
				Shape.CurrentHighlightColour = Shape.HIGHLIGHTCOLOUR1;
			// it looks better if the selector always starts the same colour.  Otherwise sometimes it starts at an odd point in the transition and the effect is rather peculiar
			if (SelectToggles)
			{
				// transformations act on the selected shapes; if we need to change the selection, for a transformation
				// we don't want to do select behind (e.g. ladybirds on flower problem - tended to pick up the background)
				m_SelectClickHit = m_Page.HitTest(target.Exact, m_Zoom, Page.HitTestMode.StartFront);
			}
			else
				m_SelectClickHit = m_Page.HitTest(target.Exact, m_Zoom, Page.HitTestMode.StartBehindSelectionWithinBounds);
			// if we hit a GrabSpot, we must count it as hitting that shape, even though it might have been slightly outside for the above HitTest
			if (m_GrabSpotHit != null)
			{
				// we know which shape it is, because GrabSpot is only permitted if there is one shape selected xx-and only one-xx
				Debug.Assert(m_Page.SelectedCount >= 1);
				m_SelectClickHit = m_Page.SelectedShapes[0];
			}
			if (SelectToggles)
				m_Page.ToggleSelection(m_SelectClickHit); // OK if it is nothing
			else
			{
				// ultimately we want to do a SelectOnly on the hit shape; but if the user is about to start dragging we need to leave
				// the current shape selected for the moment IF the user has clicked inside it.  Therefore MouseUp does the selection in most cases
				// exception is if the user clicked on nothing, then we do the selection now (partly because MouseUp wants to see a shape in m_shpSelectClickHit to be sure it should do the selection)
				if (m_SelectClickHit == null)
					m_Page.SelectOnly(m_SelectClickHit); // doesn't use "null" as param as that is ambiguous
				else
				{
					Shape selectedHit = m_Page.HitTest(target.Exact, m_Zoom, Page.HitTestMode.OnlySelection);
					Debug.Assert(selectedHit == null || m_Page.SelectedShapes.Contains(selectedHit));
					if ((selectedHit == null || wasKeyboard) && m_GrabSpotHit == null)
					{
						// the user HASN'T clicked on an already selected shape, we can do the selection immediately
						// Also there is no confusion between single click and double-click from the keyboard
						ProcessClickEffects(target);
					}
					else
						m_ClickDeferredPoint = target;
				}
			}
		}

		/// <summary>Performs some parts of "clicking" (or equivalent) which can only be done when we are sure it is not a double-click or drag</summary>
		private void ProcessClickEffects(ClickPosition target)
		{
			m_tmrDelayedClick.Enabled = false;
			RectangleF boundsBefore = m_SelectClickHit.RefreshBounds();
			SelectOnly(m_SelectClickHit, target.Exact);
			if (m_OngoingTransaction == null)
				m_OngoingTransaction = new Transaction();
			target.Transaction = m_OngoingTransaction;
			Shape.VerbResult result = m_SelectClickHit.ClickExisting(target); // in case the shape wants to do anything special
			DoVerbResult2(m_SelectClickHit, result, boundsBefore, true, Codes.ChooseExisting);
			if (m_CurrentShape == null)
				StoreCurrentTx();
			m_ClickDeferredPoint = null;
		}

		/// <summary>Selects one shape (or null).  The location is the click location and is used to set SelectedPath to a target indicating one vertex/line within the selected shape.</summary>
		private void SelectOnly(Shape shape, PointF selectPathLocation)
		{
			bool alreadySelected = false;
			if (m_Page.SelectedShapes.Count == 1 && m_Page.SelectedShapes[0] == shape)
				alreadySelected = true;
			else
				m_Page.SelectOnly(shape);
			if (!Globals.Root.CurrentConfig.ReadBoolean(Config.Advanced_Graphics))
				return;

			Target oldSelected = m_Page.SelectedPath;
			Shape.GrabSpot grabSpot = HitTestGrabSpots(selectPathLocation);
			if (grabSpot != null)
				m_Page.SelectedPath = Target.FromGrabSpot(grabSpot);
			else
			{
				List<Target> col;
				col = m_Page.SelectedShapes?.FirstOrDefault()?.GenerateTargets(UserSocket.CreateForPoint(selectPathLocation));
				if (col == null)
					return;
				m_Page.SelectedPath = (from t in col
									   where (t.Type == Target.Types.Vertex || t.Type == Target.Types.Line) && t.AdjustedDistance < 3 * GUIUtilities.MillimetreSize
									   orderby t.AdjustedDistance
									   select t).FirstOrDefault();
			}
			//Debug.WriteLine($"Selectonly with path={SelectedPath?.ToString() ?? "null"}");
			if (alreadySelected && (oldSelected == null || !oldSelected.Matches(m_Page.SelectedPath)))
				Globals.OnApplicableChanged();
		}


		private void m_tmrDelayedClick_Tick(object sender, EventArgs e)
		{
			m_tmrDelayedClick.Enabled = false;
			if (m_SelectClickHit != null && m_ClickDeferredPoint != null) // These need to be checked in case some other event has cleared them in the meantime (e.g. it was a double click)
				ProcessClickEffects(m_ClickDeferredPoint);
		}

		private void InitialiseCurrentShape()
		{
			// anything which is needed when m_shpCurrent has just been created (either directly or by spawning)
			if (m_CurrentShape.Parent == null)
				m_CurrentShape.Parent = m_Page; // condition needed because this can be from (eg) NaL arcs - which already have parent assigned
			m_CurrentShape.Status = Shape.StatusValues.Creating;
		}

		// Ancillary functions, mostly terminating ongoing

		public void ConcludeOngoing(bool neverAutoStore = false)
		{
			// kills any ongoing editing; e.g. when the user clicks on the Clear page button
			if (m_Status == Status.MakingSolid)
				ForceConcludeSolidifying();
			AbandonCurrent(neverAutoStore);
			ConcludeTyping();
		}

		private void AbandonCurrent(bool neverAutoStore = false, bool loseCaret = true)
		{
			RemoveTargetHover();
			var wasAdding = false;
			try
			{
				if (m_Status == Status.Adding && m_CurrentShape != null)
				{
					InvalidateData(m_CurrentShape.RefreshBounds(), InvalidationBuffer.Current);
					if ((m_CurrentShape.Flags & Shape.GeneralFlags.StoreIfAbandoned) > 0 && neverAutoStore == false)
					{
						m_CurrentShape.CompleteRetrospective();
						// Complete might be more meaningful, but would require mouse coordinates and might adjust the shape
						// this just allows the shape to cleanup without changing much
						DoConcludeCurrent();
					}
					else
					{
						m_CurrentShape.Dispose();
						m_CurrentShape = null;
						m_OngoingTransaction?.Cancel();
					}
					wasAdding = true;
				}
				else
				{
					if (m_OngoingTransaction != null)
						m_OngoingTransaction.Cancel();
				}
			}
			catch (Exception ex) // so following stuff all happens
			{
				Utilities.LogSubError(ex, bolWithDistributorFire: true);
			}
			if (loseCaret)
				Shape.CaretDestroy();
			m_Status = Status.Waiting;
			m_OngoingTransaction = null;
			if (m_CurrentShape != null)
			{
				m_CurrentShape = null;
				OngoingShapeChanged();
			}
			if (m_GrabMove != null)
				CancelGrab();
			if (m_Page != null)
			{
				m_Page.MovingSelection = false; // it is efficient to keep reassigning the same value to this
				m_SpawnShape?.Dispose();
				m_SpawnShape = null;
				m_SpawnedFrom = null;
				InvalidateData(m_Page.TargetRefreshBoundary(m_Zoom), InvalidationBuffer.Current);
				m_Page.ClearTargets();
				PromptsDirty();
			}
			m_ActionIsKeyboard = false; // just to guard against this being left set to true when the configuration is changed so that there is no separate cursor
			if (wasAdding)
				Globals.NotifyVerbApplicabilityChanged(); // needed for +/- on toolbar for pointer users
		}

		public void ApplyUserStyleChangesToOngoingShape(Parameters parameter, int value)
		{
			// called by the editor form when a style change is applied, so that the current floating object, if any can be updated
			Shape.StyleBase objStyle = m_CurrentShape?.StyleObjectForParameter(parameter);
			if (objStyle != null)
			{
				var refreshBounds = m_CurrentShape.RefreshBounds();
				int old = objStyle.ParameterValue(parameter);
				if (old != value)
				{
					objStyle.SetParameterValue(value, parameter);
					m_CurrentShape.NotifyStyleChanged(parameter, old, value);
					Geometry.Extend(ref refreshBounds, m_CurrentShape.RefreshBounds());
					InvalidateData(refreshBounds, InvalidationBuffer.Current);
				}
			}
			PromptsDirty(); // because this can trigger or remove the invisibility warning
		}

		private void ConcludeTyping()
		{
			// concludes and stores any ongoing typing transaction
			// done on mouse down - slightly excessive, as it is possible that the mouse down will not change the selected shape
			// but the easiest way of covering most of the selection possibilities
			if (m_TypingTransaction != null)
			{
				Globals.Root.StoreNewTransaction(m_TypingTransaction);
				m_TypingTransaction = null;
			}
			Shape focal = TypingShape(false);
			if (focal != null)
			{
				if (focal.LabelMode == Shape.LabelModes.Allowed || focal.LabelMode == Shape.LabelModes.IntrinsicTextPlusLabel)
				{
					if (focal.LabelText != null && focal.LabelText == "")
						focal.LabelText = null; // otherwise it will keep saying "start typing..."
				}
				InvalidateData(focal.RefreshBounds(), InvalidationBuffer.Selection);
				focal.CaretLose();
			}
			Shape.CaretDestroy(); // should be implicit above, but some edge cases, such as when not focused might get through
		}

		/// <summary>Starts placing the given shape.  This will call Start on the shape</summary>
		public void StartNewShape(Shape shp, ClickPosition position = null)
		{
			AbandonCurrent();
			if (position == null)
			{
				var pt = CursorPositionLocal;
				position = MouseEventCoordinates(pt.X, pt.Y, ClickPosition.Sources.Irrelevant);
			}
			m_CurrentShape = shp;
			InitialiseCurrentShape();
			m_Status = Status.Adding;
			var result = shp.Start(position);
			DoVerbResult2(shp, result, RectangleF.Empty, false, Codes.Choose);
			ForceUpdateGrabSpots(); // in case they are currently displayed
			Globals.NotifyVerbApplicabilityChanged(); // needed for +/- on toolbar for pointer users
		}

		public void IterateSelectionTo(Shape shape)
		{
			// changes selection to given shape when using Shift-F9/Ctrl-F9
			// this is in View not page as we need to update jump targets etc and would like to move the mouse there
			if (shape == null)
				return; // no shape available
			m_Page.SelectOnly(shape);
			Cursor.Position = this.PointToScreen(DataToClient(shape.Middle()).ToPoint());
		}

		private void TriggerStart(ClickPosition target, bool keyboard)
		{
			// target is in data coordinates
			m_CurrentBuffer.InvalidateAll(); // clear the buffer for the "current" shape in case it has something left from previously
			Debug.Assert(m_OngoingTransaction == null);
			m_SpawnedFrom = null;
			if (m_Tool == Shape.Shapes.Null)
				return;
			m_OngoingTransaction = new Transaction(); // shape only added during solidification; Tx needed in case of any other effects (Stacks spawning, etc)
			target.Transaction = m_OngoingTransaction;
			m_CurrentShape = Shape.CreateShape(m_Tool);
			m_ActionIsKeyboard = keyboard;
			InitialiseCurrentShape();
			m_Status = Status.Adding;
			if (m_CurrentShape.GetClass() == Shape.Classes.Transformation)
			{
				if (((Transformation)m_CurrentShape).NextChoosePerformsSelection() && !DoTransformationSelectVerb(target, false))
				{
					m_Page.DeselectAll();
					AbandonCurrent();
					return;
				}
			}
			else if (m_CurrentShape.GetClass() != Shape.Classes.Selection) // transformation was filtered out by the first If
				m_Page.DeselectAll();
			OngoingShapeChanged();
			Debug.Assert(m_Tool != Shape.Shapes.Selector);
			Shape.VerbResult result = m_CurrentShape.Start(target);
			if (result == Shape.VerbResult.Completed || result == Shape.VerbResult.Continuing)
			{
				UpdateTargetHover(target.Exact, m_CurrentShape);
				ApplyDefaultStyles?.Invoke(m_CurrentShape);
				if (m_CurrentShape.DefaultStylesApplied())
				{
					// the shape has changed one or more styles
					DisplayShapeStyles?.Invoke(m_CurrentShape);
				}
				Globals.NotifyVerbApplicabilityChanged(); // needed for +/- on toolbar for pointer users
			}
			else if (result == Shape.VerbResult.Rejected || result == Shape.VerbResult.Destroyed)
			{
				AbandonCurrent();
				result = Shape.VerbResult.Unchanged;
			}
			DoVerbResult2(m_CurrentShape, result, RectangleF.Empty, false, Codes.Choose);
			if (keyboard)
			{
				// because when the mouse clicks there seems to be an automatic mouse move (at least one!) afterwards, which makes
				// for some differences in the initial behaviour of shapes if this line is not included
				DoCursorMove(target, true);
			}
		}

		private void OngoingShapeChanged()
		{
		}

		#endregion

		#region Prompts

		// actually recalculated on graphics timer. not buffered in Globals as that is also doing Hover response
		private bool m_PromptsDirty = false;

		private void PromptsDirty()
		{
			m_PromptsDirty = true;
		}

		private void SetPrompts()
		{
			m_PromptsDirty = false;
			// only updates standard prompts (DisplayPrompt)
			if (m_Page == null)
				return;
			if (m_Status == Status.MakingSolid)
			{
				// previous shape is complete and being solidified.  We could display the prompts to start a new one immediately,
				// but I think is probably best to go for a holding option, especially in case of spawning
				Globals.DisplayPrompts(null);
				return;
			}
			Shape shape = m_CurrentShape;
			if (shape == null && m_Page.SelectedCount == 1 && (m_Page.SelectedShapes[0].Flags & Shape.GeneralFlags.PromptExistingAsNew) > 0)
			{
				// for numbergrid there is little difference between new and existing
				shape = m_Page.SelectedShapes[0];
			}
			if (shape == null)
			{
				// see if we have a prompt for starting the current shape type
				// we can't ask the shape itself for a prompt! the
				string ID = EffectiveTool + "_Start";
				if (Strings.Exists("Prompts_" + ID)) // in which case the image must be defined as well
				{
					List<Shape.Prompt> list = new List<Shape.Prompt>();
					list.Add(new Shape.Prompt(Shape.ShapeVerbs.Start, ID, ID));
					if (EffectiveTool == Shape.Shapes.Connector)
						list.Add(new Shape.Prompt(Shape.ShapeVerbs.Info, "Prompts_Connector_Hover", "Prompts_Connector_Hover"));
					else if (EffectiveTool == Shape.Shapes.UserSocket)
						list[0] = new Shape.Prompt(Shape.ShapeVerbs.Choose, "UserSocket_Start", "UserSocket_Start", "Prompts_UserSocket_Start_2");
					else if (EffectiveTool == Shape.Shapes.FreeText)
					{
						switch ((StringAlignment)Globals.StyleParameterDefaultObject(Parameters.TextAlignment).ParameterValue(Parameters.TextAlignment))
						{
							case StringAlignment.Near: // this is the default prompt anyway
								break;
							case StringAlignment.Center:
								list[0] = new Shape.Prompt(Shape.ShapeVerbs.Choose, "FreeText_Start_Centre", "FreeText_Start_Centre");
								break;
							case StringAlignment.Far:
								list[0] = new Shape.Prompt(Shape.ShapeVerbs.Choose, "FreeText_Start_Right", "FreeText_Start_Right");
								break;
						}
					}
					SendDisplayPrompts(list);
				}
				else
					Globals.DisplayPrompts(null);
			}
			else
			{
				// check current settings would render a new shape invisible
				bool relevant = false; // true if either line or fill was relevant
									   // if neither of these styles apply, the shape is probably drawing itself anyway
				bool visible = false; // true if either contain a genuine colour
				Shape.StyleBase line = shape.StyleObjectForParameter(Parameters.LineColour);
				if (line is Shape.LineStyleC style)
				{
					relevant = true;
					Color col = style.Colour;
					if (col.A > 0 && !col.Equals(m_Page.Colour))
						visible = true;
				}
				Shape.StyleBase fill = shape.StyleObjectForParameter(Parameters.FillColour);
				if (fill is Shape.FillStyleC style2)
				{
					relevant = true;
					Color col = style2.Colour;
					if (col.A > 0 && !col.Equals(m_Page.Colour))
						visible = true;
				}
				if (relevant && !visible)
				{
					DisplaySinglePrompt(new Shape.Prompt(Shape.ShapeVerbs.Warning, "Warning_Invisible", "info_64"));
					return;
				}
				SendDisplayPrompts(shape.GetPrompts());
			}
		}

		/// <summary>Used only within PromptsDirty</summary>
		private void SendDisplayPrompts(List<Shape.Prompt> prompts)
		{
			if (prompts != null && Dragging)
			{
				// remove any not-drag only ones
				int index = 0;
				while (index < prompts.Count)
				{
					if ((prompts[index].Verbs & Shape.ShapeVerbs.NotDrag) > 0)
						prompts.RemoveAt(index);
					else
						index += 1;
				}
			}
			Globals.DisplayPrompts(prompts);
		}

		private void DisplaySinglePrompt(Shape.Prompt prompt)
		{
			Globals.DisplayPrompts(new List<Shape.Prompt> { prompt });
		}

		private void UpdateHoverPrompts()
		{
			// only used at the moment for some GrabSpots
			Shape.GrabSpot grabSpot = ActiveGrabSpot;
			if (grabSpot != null)
			{
				List<Shape.Prompt> list = new List<Shape.Prompt>(); // can be used as needed below
				if (grabSpot.Prompts != null)
				{
					list.AddRange(grabSpot.Prompts);
					if (grabSpot.GrabType == Shape.GrabTypes.Move && m_SnapMode == Shape.SnapModes.Angle)
					{
						// The GrabSpot cannot easily add this one as it has no access to the snapping mode
						list.Add(new Shape.Prompt(Shape.ShapeVerbs.Info, "Prompts_Move_Snapped", "AngleSnapMove_64"));
					}
					Globals.SetHover(list);
					return;
				}
			}
			Globals.ClearHover();
		}

		#endregion

		#region Painting and invalidation

		private static readonly System.Drawing.Imaging.ImageAttributes g_WhiteTransparent;

		private bool m_SelectedBufferUsed; // Only used during PaintEditing, but value needs to persist until PaintEditingBuffers
		private bool m_CurrentBufferUsed;

		/// <summary>rectangle IN DATA COORDINATES that cannot be obscured</summary>
		private RectangleF m_PaletteExclusionArea;

		protected override void PaintEditing(PaintEventArgs e)
		{
			// Must draw all the other buffers
			// now mark areas where the user cannot create content
			Graphics gr;
			Canvas canvas;
			m_SelectedBufferUsed = false;
			m_CurrentBufferUsed = false;

			// do the semitransparent copy of the selected objects (if any)
			// also the selection bounds
			if (m_Page.SelectedCount > 0)
			{
				gr = m_SelectedBuffer.PrepareDraw();
				m_SelectedBufferUsed = true;
				if (gr != null)
				{
					canvas = new NetCanvas(gr);
					try
					{
						PrepareGraphics(gr);
						if (!TransformationActive()) // selection highlight is not drawn while moving a transformation
						{
							m_Page.DrawSelected(canvas, this, m_Zoom, m_PixelsPerDocumentX, false);
							if (!m_Page.MovingSelection)
								DrawGrabSpots(gr, ActiveGrabSpotPhase());
							m_SelectionBoundsDrawn = m_Page.SelectedBounds(false);
							if (Globals.Root.CurrentConfig.ReadBoolean(Config.Selection_Bounds))
								m_Page.DrawSelectionBoundary(m_SelectionBoundsDrawn, canvas);
						}
						else
							m_SelectionBoundsDrawn = RectangleF.Empty;
					}
					finally
					{
						gr.Dispose();
						canvas.Dispose();
					}
				}
			}

			// and finally, the current, incomplete shape (if any).
			// Also draws ShapeSnap targets - possible that m_shpCurrent is nothing if the targets are being displayed in order to add the first point
			// same but using move transform (I think once it is shpcurrent, then it has picked up shapes)
			if (m_CurrentShape != null || m_Page.HasTargets || m_Page.Sockets.Count > 0 || m_TargetHover != null || m_Page.RecentRotationPoints.Any())
			{
				gr = m_CurrentBuffer.PrepareDraw();
				m_CurrentBufferUsed = true;
				if (gr != null)
				{
					canvas = new NetCanvas(gr);
					try
					{
						// some redrawing is required of current shape
						PrepareGraphics(gr);
						m_CurrentShape?.Draw(canvas, m_Zoom, m_PixelsPerDocumentX, this, InvalidationBuffer.Current, GetCurrentAlpha(), 255, m_Document.ReverseRenderOrder);
						if (m_Status != Status.MakingSolid)
						{
							m_Page.DrawTargets(gr, ActiveTargetPhase(), m_Zoom);
							if (m_Page.Sockets.Count > 0)
							{
								// cannot test EffectiveSnap, because the snapping might be within a GrabSpot movement
								m_Page.DrawSockets(gr, m_Zoom);
							}
						}
						m_TargetHover?.AsTarget?.DrawHover(canvas, m_Zoom, m_PixelsPerDocumentX);
						// there is no need to draw a grey area outside the page - that is only needed in the background
						if (m_Page.RecentRotationPoints.Any())
						{
							m_Page.DrawRotationPoints(gr, m_PixelsPerDocumentX);
						}
					}
					finally
					{
						gr.Dispose();
						canvas.Dispose();
					}
				}
			}
		}

		/// <summary>Returns size for a 1 millimetre graphics (eg line width)</summary>
		private float MillimetreLine
		{
			get { return Geometry.MILLIMETRE / Geometry.INCH / m_DpiX; }
		}

		protected override void PaintEditingBuffers(PaintEventArgs e)
		{
			base.PaintEditingBuffers(e);
			if (m_SelectedBufferUsed)
				e.Graphics.DrawImageUnscaled(m_SelectedBuffer.m_bmp, 0, 0);
			if (m_CurrentBufferUsed)
				e.Graphics.DrawImageUnscaled(m_CurrentBuffer.m_bmp, 0, 0);
		}

		public override void InvalidateData(RectangleF invalid, InvalidationBuffer buffer, bool textFlowExpand = true)
		{
			base.InvalidateData(invalid, buffer, textFlowExpand);
			if (m_Document == null || m_SelectedBuffer == null) // latter if this disposed
			{
				Utilities.LogSubError("View:InvalidateData with AM.CurrentDocument / m_SelectedBuffer = null");
				return;
			}
			Rectangle screen = Rectangle.Round(DataToClient(invalid));
			if ((buffer & InvalidationBuffer.Current) > 0)
				m_CurrentBuffer.Invalidate(screen);
			if ((buffer & (InvalidationBuffer.Selection | InvalidationBuffer.SelectionHighlightOnly)) > 0)
				m_SelectedBuffer.Invalidate(screen);

		}

		public override void InvalidateAll(InvalidationBuffer buffer = InvalidationBuffer.All)
		{
			base.InvalidateAll(buffer);
			if ((buffer & InvalidationBuffer.Current) > 0)
				m_CurrentBuffer.InvalidateAll();
			if ((buffer & (InvalidationBuffer.Selection | InvalidationBuffer.SelectionHighlightOnly)) > 0)
				m_SelectedBuffer.InvalidateAll();
		}

		public void NotifyIndirectChange(Shape shape, ChangeAffects affected)
		{
			// the current shape is given the view as a parent, so might send some notifications directly here
			affected = affected & ~(ChangeAffects.GrabSpots); // these can only be handled in the page
			Debug.Assert(shape == m_CurrentShape);
			m_Page_ShapeNotifiedIndirectChange(shape, affected, RectangleF.Empty);
		}

		public void NotifyIndirectChange(Shape shape, ChangeAffects affected, RectangleF area)
		{
			affected = affected & ~(ChangeAffects.GrabSpots); // these can only be handled in the page
			Debug.Assert(shape == m_CurrentShape);
			m_Page_ShapeNotifiedIndirectChange(shape, affected, area);
		}

		protected override void m_Page_ShapeNotifiedIndirectChange(Shape shape, ChangeAffects affected, RectangleF area)
		{
			if ((affected & ChangeAffects.GrabSpots) > 0)
			{
				// if it is not selected, then there will be no GrabSpots for the shape anyway, so we can ignore it
				if (m_Page.SelectedShapes.Contains(shape))
				{
					affected = affected | ChangeAffects.RepaintNeeded; // view will cause repaint
					if (area.IsEmpty)
						area = shape.RefreshBounds();
					IncludeGrabSpotsInRefresh(ref area);
					SetGrabSpots();
				}
				affected -= ChangeAffects.GrabSpots;
			}
			if ((affected & ChangeAffects.RepaintNeeded) > 0)
			{
				if (area.IsEmpty)
					InvalidateData(shape.RefreshBounds(), InvalidationBuffer.All);
				else
					InvalidateData(area, InvalidationBuffer.All);
				affected -= ChangeAffects.RepaintNeeded;
			}
			if ((affected & ChangeAffects.Diagnostic) > 0)
			{
				if (m_Page.SelectedCount == 1)
					UpdateDiagnostic();
				affected -= ChangeAffects.Diagnostic;
			}
			if ((affected & ChangeAffects.StyleInformation) > 0)
			{
				StyleStateChanged?.Invoke();
				affected -= ChangeAffects.StyleInformation;
			}
			if ((affected & ChangeAffects.ApplyDefaultStyles) > 0)
			{
				ApplyDefaultStyles?.Invoke(shape);
				affected -= ChangeAffects.ApplyDefaultStyles;
			}
			if ((affected & ChangeAffects.UpdatePrompts) > 0)
			{
				PromptsDirty();
				affected -= ChangeAffects.UpdatePrompts;
			}
			if ((affected & ChangeAffects.MoveMouse) > 0)
			{
				// rctArea should be size 0, and pointing to mouse position
				if (area.X == 0 && area.Y == 0)
					Debug.Fail("IShapeParent.ChangeAffects.MoveMouse: rctArea not specified");
				else
				{
					PointF ptTarget = DataToClient(area.Location);
					CursorPositionLocal = new Point((int)ptTarget.X, (int)ptTarget.Y);
				}
				affected -= ChangeAffects.MoveMouse;
			}
			if ((affected & ChangeAffects.UpdateVerbButtonApplicability) > 0)
			{
				Globals.NotifyVerbApplicabilityChanged();
				affected -= ChangeAffects.UpdateVerbButtonApplicability;
			}
			if ((affected & ChangeAffects.SetPaletteExclusion) > 0)
			{
				m_PaletteExclusionArea = area;
				SendPaletteExclusion();
				affected -= ChangeAffects.SetPaletteExclusion;
			}

			if ((affected & ChangeAffects.Test) > 0)
			{
				// for any temporary, experimental code
				//m_frmEditor.ParameterValue(Parameters.Tool) = Shape.Shapes.Selector
				affected -= ChangeAffects.Test;
			}
			if ((affected & ChangeAffects.Bounds) > 0)
				affected -= ChangeAffects.Bounds; // currently ignoring this.  not needed
			Debug.Assert(affected == 0, "ShapeNotifiedIndirectChange effect has not been handled: " + affected);
		}

		private void SendPaletteExclusion()
		{
			Rectangle screen;
			if (m_PaletteExclusionArea.IsEmpty)
				screen = Rectangle.Empty;
			else
				screen = RectangleToScreen(DataToClient(m_PaletteExclusionArea).ToRectangle());
			RequestPaletteExclusion?.Invoke(screen);
		}

		protected override void m_Page_SelectionChanged()
		{
			base.m_Page_SelectionChanged();
			if (m_CurrentShape == null)
				UpdateDiagnostic();
		}

		protected override void m_Page_RefreshSelection(RectangleF dataRect, bool dataInvalid, bool updateGrabSpots)
		{
			if (updateGrabSpots)
			{
				IncludeGrabSpotsInRefresh(ref dataRect);
				SetGrabSpots();
				IncludeGrabSpotsInRefresh(ref dataRect);
			}
			base.m_Page_RefreshSelection(dataRect, dataInvalid, updateGrabSpots);
		}

		#region Animation

		private void PulseSelected()
		{
			if (m_Page == null)
				return;
			if (m_Page.SelectedCount > 0)
				InvalidateData(m_Page.SelectedRefreshBoundary(), InvalidationBuffer.SelectionHighlightOnly, false);
			if (m_CurrentShape != null && (m_CurrentShape.Flags & Shape.GeneralFlags.ContinuousRefreshDuringCreation) > 0)
				InvalidateData(m_CurrentShape.RefreshBounds(), InvalidationBuffer.Current, false);
		}

		internal sealed class SelectedPulseProxy : IBlendableColourDisplay
		{

			private readonly EditableView m_View;
			private readonly frmMain m_frm;

			public SelectedPulseProxy(EditableView view, frmMain form)
			{
				m_View = view;
				m_frm = form;
			}

			public Color VariableColour
			{
				get { return Shape.CurrentHighlightColour; }
				set
				{
					Shape.CurrentHighlightColour = value;
					// RaiseEvent PulseColourChanged()
					m_View.PulseSelected();
				}
			}
		}

		private SolidifyAnimationProxy m_Solidify = new SolidifyAnimationProxy();

		private class SolidifyAnimationProxy : IAnimationNotifyComplete, ILinearAnimated
		{

			public int Alpha = 100;
			public const int ADDINGALPHA = 100; // the a to use when adding a shape

			public event NullEventHandler IsComplete;
			public event NullEventHandler Refresh;


			public void Complete()
			{
				Alpha = ADDINGALPHA;
				IsComplete?.Invoke();
			}

			public int VariableValue
			{
				get { return Alpha; }
				set
				{
					Alpha = value;
					Refresh?.Invoke();
				}
			}
		}

		#endregion

		private void EditableView_Scroll(object sender, ScrollEventArgs e)
		{
			// need to send on scroll since screen coords will have changed.  form will ignore repeat calls with same data
			SendPaletteExclusion();
		}

		protected override void ChangeZoom(float newZoom, Point ptMaintainLocal)
		{
			base.ChangeZoom(newZoom, ptMaintainLocal);
			SendPaletteExclusion();
		}

		#endregion

		#region Snapping

		private bool AllowSnap()
		{
			// mainly checks for ctrl disables.  Also checks whether appropriate for various xform types, before the first point is placed
			// (after a point placed can call Shape.SnapNext).  Some transforms just select the shape clicked on; so don't want to snap that point
			if (m_Status == Status.Waiting && Shape.GetClass(m_Tool) == Shape.Classes.Transformation)
			{
				if (!Shape.TransformationTypeWhichSnapsFirstPoint(m_Tool))
					return false;
			}
			return !((ModifierKeys & Keys.Control) > 0);
		}

		private Shape.SnapModes EffectiveSnap()
		{
			// snap mode modified to off when it is not applicable
			if (m_Status == Status.Waiting && Shape.GetClass(m_Tool) == Shape.Classes.Transformation)
			{
				if (!Shape.TransformationTypeWhichSnapsFirstPoint(m_Tool))
					return Shape.SnapModes.Off;
			}
			if ((ModifierKeys & Keys.Control) > 0)
				return Shape.SnapModes.Off;
			Shape.SnapModes snapMode = m_SnapMode;
			if (Shape.GetClass(EffectiveTool) == Shape.Classes.Selection)
				return Shape.SnapModes.Off;
			else if (Shape.GetClass(EffectiveTool) == Shape.Classes.Modifier)
			{
				switch (EffectiveTool)
				{
					case Shape.Shapes.SetOrigin: // these do support snapping
					case Shape.Shapes.Scissors:
						break;
					default:
						return Shape.SnapModes.Off; // no need for shape snap
				}
			}
			if (m_CurrentShape != null)
				snapMode = m_CurrentShape.SnapNext(snapMode);
			if (m_CurrentShape == null && snapMode == Shape.SnapModes.Angle)
				snapMode = Shape.SnapModes.Off; // angle only works after placing first point
												// these probably equivalent - but might differ when moving existing?
			switch (EffectiveTool)
			{
				case Shape.Shapes.Connector:
					snapMode = Shape.SnapModes.Socket;
					break;
			}
			return snapMode;
		}

		#endregion

		#region Buffers

		private Buffer m_CurrentBuffer = new Buffer(true); // contains only m_shpCurrent.  The state of this buffer is ignored if m_shpCurrent is nothing
		private Buffer m_SelectedBuffer = new Buffer(true); // contains a second (semitransparent, with highlight) copy of any selected shapes

		#endregion

		#region Tool selection and parameter implementation

		private Shape.Shapes m_Tool;

		[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
		public void NotifyCurrentToolChanged(Shape.Shapes newTool, bool dontDeselect)
		{
			// changes should only be made through frmMain.CurrentTool, which calls this
			switch (m_Status)
			{
				case Status.Adding:
					break;
				case Status.MakingSolid:
					ForceConcludeSolidifying();
					break;
			}
			m_Tool = newTool;
			AbandonCurrent(false, m_Tool != Shape.Shapes.Selector); // gets rid of anything else which is left over - is safe using new tool (poss better as it calls PromptsDirty)
																	// Caret is not destroyed when switching to the selector (this is essential for equations if the automatic F9 reset option is enabled)
			if (m_Page == null || m_Page.IsDisposed)
				return; // must be after m_eTool assigned!
			EnableGrabSpots = Shape.ShapeIsDirectSelector(m_Tool);
			if (m_Page.HasTargets && Shape.GetClass(m_Tool) != Shape.Classes.Real && Shape.GetClass(m_Tool) != Shape.Classes.Measure)
			{
				m_Page.ClearTargets();
				InvalidateAll();
			}
			if (m_Page.Sockets.Count > 0)
			{
				InvalidateData(m_Page.SocketRefreshBoundary(), InvalidationBuffer.Current);
				m_Page.Sockets.Clear();
			}
			if (!dontDeselect && newTool != Shape.Shapes.Selector && Shape.GetClass(newTool) != Shape.Classes.Transformation)
				m_Page.SelectOnly(new List<Shape>());
			PromptsDirty();
			CheckAngleSnapDisplaysMove();
		}

		internal bool IsSingleSelector
		{
			get
			{
				// true if selector tool active (either selected, or via shift)
				return Shape.ShapeIsDirectSelector(m_Tool) || (ModifierKeys & Keys.Shift) > 0 && m_TypingTransaction == null;
				// pressing shift does not select while typing into a shape
			}
		}

		public void ImportImage(string file)
		{
			AbandonCurrent();
			m_OngoingTransaction = new Transaction();
			try
			{
				m_CurrentShape = new ImportedImage(file, m_Document, m_OngoingTransaction);
				InitialiseCurrentShape();
				m_CurrentShape.Start(new ClickPosition(new PointF(100, 100), m_Page, m_Zoom, Shape.SnapModes.Off, Shape.SnapModes.Off, this, ClickPosition.Sources.Irrelevant));
			}
			catch (Exception ex)
			{
				m_Status = Status.Waiting;
				m_CurrentShape = null;
				m_OngoingTransaction = null;
				MessageBox.Show(ex.Message);
				return;
			}
			InvalidateAll();
			m_Status = Status.Adding;
			OngoingShapeChanged();
		}

		/// <summary>Does not support SVG</summary>
		/// <param name="memory"></param>
		public void ImportImage(System.IO.MemoryStream memory)
		{
			AbandonCurrent();
			m_OngoingTransaction = new Transaction();
			try
			{
				m_CurrentShape = new ImportedImage(memory, m_Document, m_OngoingTransaction);
				InitialiseCurrentShape();
				m_CurrentShape.Start(new ClickPosition(new PointF(100, 100), m_Page, m_Zoom, Shape.SnapModes.Off, Shape.SnapModes.Off, this, ClickPosition.Sources.Irrelevant));
			}
			catch (Exception ex)
			{
				m_Status = Status.Waiting;
				m_CurrentShape = null;
				m_OngoingTransaction = null;
				MessageBox.Show(ex.Message);
				return;
			}
			InvalidateAll();
			m_Status = Status.Adding;
			OngoingShapeChanged();
		}

		internal Shape.Shapes EffectiveTool
		{
			get
			{
				if (m_CurrentShape != null)
					return m_CurrentShape.ShapeCode;
				return m_Tool;
			}
		}

		/// <summary>true if current selection works as toggle (ie shift pressed - but extracted here so we can change the key w/o changing all the code) </summary>
		internal bool SelectToggles => (ModifierKeys & Keys.Shift) > 0;

		#endregion

		#region Dynamic graphics, including solidifying new objects

		private Timer m_tmrGraphics = new Timer(); // used for some things which run continously

		private enum Status
		{
			Waiting,
			Adding, // part way through adding a shape
			MakingSolid
		}

		private Status m_Status = Status.Waiting;

		private void m_tmrGraphics_Tick(object sender, EventArgs e)
		{
			if (m_Page == null)
				return;
			if (m_RezoomNeeded)
				ChangeZoom(m_SpecialZoom != SpecialZooms.None ? (float)m_SpecialZoom : m_Zoom);
			if (m_Page.HasActiveTarget)
				InvalidateData(m_Page.TargetRefreshBoundary(m_Zoom), InvalidationBuffer.Current);
			foreach (RectangleF rect in m_Page.GetRotationPointInvalidationAreas(m_PixelsPerDocumentX))
				InvalidateData(rect, InvalidationBuffer.Current, false);
			if (ActiveGrabSpot != null)
				InvalidateData(GrabSpotRefreshBounds(ActiveGrabSpot), InvalidationBuffer.Selection);
			if (m_PromptsDirty)
				SetPrompts();
		}

		private void StartSolidifying()
		{
			m_Page.MovingSelection = false;
			if (m_CurrentShape == null || m_Status != Status.Adding)
			{
				Debug.Fail("Inappropriate call to StartSolidifying");
				return;
			}
			m_Status = Status.MakingSolid;
			//	if (!(m_CurrentShape is FloatingLabel)) // mustn't do it for FloatingLabel as it want
			m_CurrentShape.CaretLose(); // mustnt CaretDestroy, cos FreeText (eg) will already have assigned caret to another spawned shape in some cases
										//If m_shpCurrent.Shape <> Shape.Shapes.Equation Then Shape.CaretDestroy() ' mustn't remove caret with equations if automatic F9 reset is enabled
										//AnimationController.EnsureNoAnimation(m_shpCurrent)
			AnimationLinear.CreateStart(m_Solidify, 255, 30);
			Shape currentShape = m_CurrentShape; // needed for the final paragraph - because ForceConcludeSolidifying will clear m_shpCurrent
												 // condition below is:  Selector enabled AND ((A) OR (B) OR (C)) - B and C containing ANDs
			if ((m_CurrentShape.Flags & Shape.GeneralFlags.AlwaysResetToolAfterCreation) > 0 ||
				Globals.Root.CurrentConfig.ReadBoolean(Config.SelectorAfterCreate) && (m_CurrentShape.Flags & Shape.GeneralFlags.DisallowResetToolAfterCreation) == 0 && m_SpawnShape == null
			)
			{
				// the shape wants to reset to the selection tool (usually used for complex shapes where placing another one wouldn't make much sense)
				// this was a condition applied to both parts of OR: AM.CurrentConfig.ShapeEnabled(Shape.Shapes.Selector) AndAlso _
				ForceConcludeSolidifying();
				Globals.SetParameterValue((int)Shape.Shapes.SelectorOrNull, Parameters.Tool);
			}
			else if (m_CurrentShape.IsFilled == false)
			{
				// no point solidifying - we only adjust the fill colour, and there isn't one, so just force to end state
				ForceConcludeSolidifying();
			}
			if ((currentShape.Flags & Shape.GeneralFlags.DoubleClickAfterCreation) > 0)
			{
				ForceConcludeSolidifying(); // doesn't matter if already run above
				m_Page.SelectOnly(currentShape); // otherwise the action won't run as it is not available unless there is one item selected
				Globals.Root.PerformAction(Verb.Find(Codes.DoubleClick));
				if (!Shape.LastDoubleClickResult)
				{
					// If the user cancelled the dialog, we should destroy the button completely
					Debug.WriteLine("Removing last shape since Shape.LastDoubleClickResult == false");
					Globals.Root.PerformAction(Verb.Find(Codes.Delete));
				}
			}
		}

		private void m_Solidify_IsComplete()
		{
			// ForceConcludeSolidifying calls thru to this immediately (albeit indirectly)
			m_Status = Status.Waiting;
			m_Solidify.Alpha = SolidifyAnimationProxy.ADDINGALPHA;
			DoConcludeCurrent();
			Shape oldCurrent = m_CurrentShape;
			m_CurrentShape = null;
			if (m_SpawnShape != null)
			{
				// previous shape requested that we immediately start another
				m_CurrentShape = m_SpawnShape;
				InitialiseCurrentShape();
				m_SpawnShape = null;
				if (m_Page.Contains(m_CurrentShape))
				{
					// shape already exists - select it rather than adding it
					m_Page.SelectOnly(m_CurrentShape);
					m_CurrentShape = null; // status left as waiting
				}
				else
				{
					m_Status = Status.Adding;
					m_Page.DeselectAll();
				}
				Point ptMouse = CursorPositionLocal;
				EditableView_MouseMove(this, new MouseEventArgs(MouseButtons.Left, 1, ptMouse.X, ptMouse.Y, 0));
			}
			else
			{
				if (oldCurrent != null && (oldCurrent.Flags & Shape.GeneralFlags.NoSelectAfterCreate) == 0 && oldCurrent.Parent == m_Page)
					m_Page.SelectOnly(oldCurrent);
			}
			OngoingShapeChanged();
			PromptsDirty();
			Globals.NotifyVerbApplicabilityChanged(); // needed for +/- on toolbar for pointer users
		}

		private void DoConcludeCurrent()
		{
			// was part of m_Solidify_IsComplete. Extracted so that AbandonCurrent can do this as well when auto-storing
			if (m_CurrentShape == null)
				return; // should not be nothing - condition just added as a sanity check
			if (m_OngoingTransaction == null)
				m_OngoingTransaction = new Transaction();
			if (!m_OngoingTransaction.Contains(m_CurrentShape))
				m_OngoingTransaction.Create(m_CurrentShape); // Create gives an error if already included
			if (m_CurrentShape.Parent != m_Page || m_CurrentShape.Degenerate && (m_CurrentShape.Flags & Shape.GeneralFlags.IgnoreDegenerateWhenCreating) == 0)
			{
				// NumberArc is already attached to the NumberArcLine
				// Degenerate is specifically to prevent erasors being added to the page in pixel mode (the object returns Complete but mustn't be put on the page)
			}
			else if (m_TargetHover is IShapeContainer)
			{
				IShapeContainer container = (IShapeContainer)m_TargetHover;
				m_CurrentShape.Parent = (IShapeParent)m_TargetHover;
				container.Contents.Add(m_CurrentShape);
				container.FinishedModifyingContents(m_OngoingTransaction);
				InvalidateData(m_CurrentShape.RefreshBounds(), InvalidationBuffer.All);
			}
			else
			{
				m_Page.AddNew(m_CurrentShape, m_OngoingTransaction); // note that this will trigger a refresh event
			}
			m_CurrentShape?.OnConclude(m_OngoingTransaction);
			RemoveTargetHover(); // Must be after the Target check above
			m_Page.ClearTargets();
			Globals.Root.StoreNewTransaction(m_OngoingTransaction);
			m_OngoingTransaction = null;
		}

		private void m_Solidify_Refresh()
		{
			if (m_CurrentShape != null)
				InvalidateData(m_CurrentShape.RefreshBounds(), InvalidationBuffer.Current);
			else
			{
				m_Solidify.Complete();
				Debug.WriteLine("m_Solidify_Refresh: m_shpCurrent is nothing");
			}
		}

		private void ForceConcludeSolidifying()
		{
			// finishes solidifying the current shape either because the colour change has finished, or the user has clicked elsewhere
			// importantly, this adds the shape to the main page
			AnimationController.EnsureNoAnimation(m_Solidify);
			// will trigger fn above
		}

		private int GetCurrentAlpha()
		{
			// returns the A to use for the current shape.  This is ADDINGALPHA while the user is moving it, and then changes progressively to 255 once the shape is complete
			return m_Solidify.Alpha;
		}

		// Targets stuff:
		private DateTime m_dtTargetPhaseBase = DateTime.Now;

		private int ActiveTargetPhase()
		{
			// when the user is close enough to a target to activated the graphics is animated through a limited number of phases
			return (int)(DateTime.Now.Subtract(m_dtTargetPhaseBase).TotalMilliseconds / Target.ACTIVEANIMATIONINTERVAL) % (Target.ACTIVEMAXIMUMPHASE + 1);
		}

		private int ActiveGrabSpotPhase()
		{
			// when the user is close enough to a target to activated the graphics is animated through a limited number of phases
			return (int)(DateTime.Now.Subtract(m_dtTargetPhaseBase).TotalMilliseconds / Shape.GrabSpot.PHASEINTERVAL) % (Shape.GrabSpot.MAXIMUMACTIVEPHASE + 1);
		}

		#endregion

		#region Other functions

		/// <summary>create an image which can be saved to disk</summary>
		public Image CreateImage(bool isVector, bool includeBackground, bool selectionOnly)
		{
			RectangleF bounds; // bounds is the part of the page to save
			if (!selectionOnly)
				bounds = new RectangleF(0, -m_Page.Size.Height, m_Page.Size.Width, m_Page.Size.Height);
			else
				bounds = m_Page.SelectedRefreshBoundary();
			Image image;
			Bitmap bitmap = new Bitmap(Math.Max(4, (int)(bounds.Width * m_PixelsPerDocumentX)),
				Math.Max(4, (int)(bounds.Height * m_PixelsPerDocumentY)));
			Graphics grBitmap = Graphics.FromImage(bitmap);
			Graphics gr;
			if (isVector)
			{
				image = new System.Drawing.Imaging.Metafile(grBitmap.GetHdc(),
					System.Drawing.Imaging.EmfType.EmfPlusOnly); // gives smaller files with smoothing.  Not sure how much it restricts the software that can load it
				Graphics grMetafile = Graphics.FromImage(image);
				gr = grMetafile;
			}
			else
			{
				image = bitmap;
				gr = grBitmap;
			}
			using (NetCanvas canvas = new NetCanvas(gr))
			{
				gr.ScaleTransform(gr.DpiX / Geometry.INCH, gr.DpiY / Geometry.INCH);
				gr.TranslateTransform(-bounds.Left, -bounds.Top);
				if (includeBackground)
				{
					gr.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
					m_Page.DrawBackground(canvas, 1);
				}
				else if (!isVector)
					gr.Clear(Color.White); // Transparent isn't working for clipboard - may get converted to format which doesn't support alpha
				gr.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
				gr.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
				m_Page.DrawShapes(canvas, 1, gr.DpiX / Geometry.INCH, null);
				gr.Dispose();
				if (isVector)
					grBitmap.Dispose();
				return image;
			}

		}

		/// <summary>Returns SVG XML image for view/selection.  Like CreateImage, except for output (and always vector implicitly)</summary>
		public string CreateSVG(bool includeBackground, bool selectionOnly)
		{
			RectangleF bounds;
			if (!selectionOnly)
				bounds = new RectangleF(0, -m_Page.Size.Height, m_Page.Size.Width, m_Page.Size.Height);
			else
				bounds = m_Page.SelectedRefreshBoundary();
			using (SVGCanvas canvas = new SVGCanvas(bounds.Size))
			{
				canvas.IntersectClip(bounds);
				canvas.TranslateTransform(-bounds.Left, m_Page.Bounds.Bottom - bounds.Bottom);
				if (includeBackground)
					m_Page.DrawBackground(canvas, 1);
				m_Page.DrawShapes(canvas, 1, 1, null);
				canvas.RestoreClip();
				return canvas.ToString();
			}

		}

		private bool TransformationActive()
		{
			if (m_CurrentShape == null || !m_CurrentShape.TransformationType())
				return false;
			return ((Transformation)m_CurrentShape).Active();
		}

		public bool ShiftCanSelect
		{
			get { return m_Status != Status.Adding && m_TypingTransaction == null; }
		}

		private void UpdateDiagnostic()
		{
			try
			{
				var shape = m_CurrentShape;
				if (shape == null && m_Page.SelectedCount == 1)
					shape = m_Page.SelectedShapes[0];
				if (shape == null)
					shape = TypingShape(false);
				if (shape != null)
				{
					ChangeDiagnostic?.Invoke(shape);
					DisplayShapeInfo?.Invoke(shape.StatusInformation(m_Status == Status.Adding));
				}
				else
					DisplayShapeInfo?.Invoke("");
			}
			catch (Exception ex) when (!Globals.Root.IsDebug)
			{
				DisplayShapeInfo?.Invoke("");
				Debug.Fail("UpdateDiagnostic failed: " + ex);
			}
		}

		public override Shape TypingShape(bool onlyIfFocused = true)
		{
			// shape into which typing goes.  Doesn't check if it actually accepts typing.  Doesn't necessarily have caret
			// can't just check caret because shapes can consume keys without the caret - eg NumberGrid doesn't bother with caret
			if (m_Page == null)
				return null;
			if (onlyIfFocused && this.Focused == false)
				return null;
			if (Shape.CaretShape != null)
				return Shape.CaretShape; // Version 1 just did the code below, but this fails with the NumberArcs where the actual typing shape may be a child of the selected shape on the page
			if (m_CurrentShape != null)
			{
				if ((m_CurrentShape.Allows & Shape.AllowedActions.Typing) == 0)
					return null;
				return m_CurrentShape;
			}
			else if (m_Page.SelectedCount == 1)
			{
				Shape shape = m_Page.SelectedShapes[0];
				if ((shape.Allows & Shape.AllowedActions.Typing) == 0)
					return null;
				return shape;
			}
			return null;
		}

		public string ChooseText()
		{
			// the ID of the text required to describe the choose action (might be clicking or releasing the button)
			if (m_DragMode && m_Status == Status.Adding)
				return "Prompts_ChooseRelease";
			return "Prompts_ChooseClick";
		}

		public void TransformModeChanged()
		{
			if (m_CurrentShape != null && m_CurrentShape.TransformationType())
			{
				((Transformation)m_CurrentShape).ChangeMode(Transformation.CopyMode ? Transformation.Modes.Copy : Transformation.Modes.Move, m_Page);
				PromptsDirty();
			}
		}

		public void ApplyConfiguration()
		{
			// called by frmMain.ApplyConfiguration
			InvalidateAll();
			AutoScroll = !Globals.Root.CurrentConfig.ReadBoolean(Config.Resize_Document_ToWindow, true);
		}

		/// <summary>Treats the given shape as if a new shape being drawn using a tool - intended for virtual shapes such as changing the rotation point </summary>
		internal void StartCustomShape(Shape custom)
		{
			AbandonCurrent();
			m_CurrentShape = custom;
			ClickPosition pt = new ClickPosition(m_Page.Origin, m_Page, m_Zoom, Shape.SnapModes.Off, Shape.SnapModes.Off, this, ClickPosition.Sources.Irrelevant);
			m_CurrentShape.Start(pt);
			m_Status = Status.Adding;
			InvalidateData(m_CurrentShape.Bounds, InvalidationBuffer.Current);
			PromptsDirty();
		}


		#region Choice of angle snap icons

		internal bool AngleSnapDisplaysMove; // true when the alternate icons should be used
		public event NullEventHandler AngleSnapDisplaysMoveChanged; // raised when above changes

		private void CheckAngleSnapDisplaysMove()
		{
			bool move = m_Tool == Shape.Shapes.TransformMove || m_Tool == Shape.Shapes.TransformMoveVH;
			if (m_GrabMove != null && m_GrabMove.GrabType == Shape.GrabTypes.Move && m_SnapMode == Shape.SnapModes.Angle)
				move = true;
			if (move != AngleSnapDisplaysMove)
			{
				AngleSnapDisplaysMove = move;
				AngleSnapDisplaysMoveChanged?.Invoke();
			}
		}

		#endregion

		#endregion

		#region GrabSpots

		/// <summary>list of GrabSpots for currently selected shape.  null or empty list if none currently displayed </summary>
		private List<Shape.GrabSpot> m_GrabSpots;

		private bool m_EnableGrabSpots;

		/// <summary>the one that would be triggered by a mouse click at the current location; if any</summary>
		private Shape.GrabSpot m_ActiveGrabSpot;

		/// <summary>whether control was pressed when the GrabSpots were created (effects which ones are created) </summary>
		private bool m_ControlGrab;

		// GrabSpots are only enabled when the tool is the standard selector
		public bool EnableGrabSpots
		{
			get { return m_EnableGrabSpots; }
			set
			{
				if (value == m_EnableGrabSpots)
					return;
				RectangleF invalid = RectangleF.Empty;
				IncludeGrabSpotsInRefresh(ref invalid);
				m_EnableGrabSpots = value;
				SetGrabSpots();
				IncludeGrabSpotsInRefresh(ref invalid);
				InvalidateData(invalid, InvalidationBuffer.Selection);
			}
		}

		/// <summary>Does SetGrabSpots, but also forces invalidation of paint buffers as needed </summary>
		public void ForceUpdateGrabSpots()
		{
			RectangleF invalid = RectangleF.Empty;
			IncludeGrabSpotsInRefresh(ref invalid);
			SetGrabSpots();
			IncludeGrabSpotsInRefresh(ref invalid);
			InvalidateData(invalid, InvalidationBuffer.Selection);
		}

		private void SetGrabSpots()
		{
			m_ControlGrab = (ModifierKeys & Keys.Control) > 0;
			m_ActiveGrabSpot = null;
			if (!m_EnableGrabSpots || m_Page.SelectedCount == 0 || m_Status == Status.Adding) // last condition usually redundant, but matters when adding Arcs to NaL
				m_GrabSpots = null;
			else if (m_Page.SelectedCount > 1)
			{

				m_GrabSpots = new List<Shape.GrabSpot>();
				m_GrabSpots.Add(new Shape.GrabSpot(null, Shape.GrabTypes.Move, m_Page.SelectedBounds(false).Centre()));
				RectangleF bounds = m_Page.SelectedBounds(true);
				Shape.AddBoundingGrabSpotsForRectangle(m_GrabSpots, bounds, Shape.AllowedActions.TransformScale | Shape.AllowedActions.TransformLinearStretch, m_Zoom);
				if (m_Page.SelectedShapes.All(s => (s.Allows & Shape.AllowedActions.TransformRotate) > 0))
					Shape.AddStandardRotationGrabSpotAt(m_GrabSpots, bounds.Centre(), m_Page);
			}
			else
			{
				Shape selected = m_Page.SelectedShapes[0];
				if (!selected.HasCaret)
					m_GrabSpots = selected.GetGrabSpots(m_Zoom);
				else
					m_GrabSpots = null;
				if (m_GrabSpots != null)
				{
					// in terms of protection it is assumed that all the GrabSpots reported by the shapes come under the Resize heading
					// the only GrabSpot which moves the shape is the one which is added for keyboard users
					// add a move grab spot.  Note that if the object returns an empty list this can be added; if it returns Nothing the movement GrabSpot should not be added
					// several shapes rely on this to block the movement GrabSpot
					if ((selected.Allows & Shape.AllowedActions.TransformMove) > 0)
						m_GrabSpots.Add(new Shape.GrabSpot(selected, Shape.GrabTypes.Move, selected.Middle()));
				}
			}
		}

		// If the display is zoomed in the GrabSpots are reduced in size somewhat, so that they only scale at the square root of the scale factor
		// if the display is zoomed out, they display at a fixed data size; i.e. the zoom is not used
		// the refresh bounds always uses Zoom=1, which may return a slightly excessive area when zoomed in
		// (but passing the actual scale factor to this would require updating hundreds of function calls)
		private void DrawGrabSpots(Graphics gr, int activePhase)
		{
			// is never called when printing
			if (m_GrabSpots == null)
				return;
			float scale = m_Zoom < 1 ? 1 : (float)(1 / Math.Sqrt(m_Zoom));
			// need to make larger if working in pixels...
			scale = scale * (gr.DpiX / Geometry.INCH) / m_Document.ApproxUnitScale;

			// this is now the adjustment to apply to the data coordinates
			// the scale factor has been applied directly to the graphics context, this reverses some of that affect
			using (Pen pn = new Pen(Color.Black, Geometry.THINLINE * scale)) // pen for the graphic showing if this GrabSpot changes the shape
			using (Pen pnTransform = new Pen(Color.DarkGray, Geometry.THINLINE * 2 * scale) { LineJoin = System.Drawing.Drawing2D.LineJoin.Round })
			{
				foreach (Shape.GrabSpot grabSpot in m_GrabSpots)
				{
					PointF pt = grabSpot.Position;
					float size = 1; // is adjusted if active
					if (grabSpot == m_ActiveGrabSpot && grabSpot.GrabType != Shape.GrabTypes.Fixed)
						size += activePhase * Shape.GrabSpot.PHASEGROWSIZE;
					size *= scale;
					if (m_GrabSpots.Count > 1 && grabSpot.Shape != null) // reduce size for v small shapes
					{
						float minSize = 0.5f * scale; // never go below this even if shape is small
						float maxSize = Math.Min(grabSpot.Shape.Bounds.Width, grabSpot.Shape.Bounds.Height) / 6; // divides by 6 since size is half the grabspot
						size = Math.Min(size, maxSize);
						size = Math.Max(size, minSize);
					}

					switch (grabSpot.GrabType)
					{
						case Shape.GrabTypes.Radius:
							SizeF outwards = grabSpot.Focus.VectorTo(grabSpot.Position);
							gr.DrawRectangle(pn, pt.X - size, pt.Y - size, size * 2, size * 2);
							DrawRadiusGrabSpotArrows(gr, pnTransform, pt, outwards, size);
							break;
						case Shape.GrabTypes.SingleVertex:
						case Shape.GrabTypes.CornerResize:
							gr.DrawRectangle(pn, pt.X - size, pt.Y - size, size * 2, size * 2);
							break;
						case Shape.GrabTypes.Rotate:
							gr.DrawRectangle(pn, pt.X - size, pt.Y - size, size * 2, size * 2);
							float radius = Geometry.DistanceBetween(pt, grabSpot.Focus);
							// could just subtract the X of each one at the moment
							// but this is more general, not assuming that the rotation point is necessarily on the horizontal axis of the shape
							pnTransform.EndCap = System.Drawing.Drawing2D.LineCap.ArrowAnchor;
							gr.DrawArc(pnTransform, grabSpot.Focus.X - radius, grabSpot.Focus.Y - radius, radius * 2, radius * 2, -45, 90);
							pnTransform.EndCap = System.Drawing.Drawing2D.LineCap.NoAnchor;
							break;
						case Shape.GrabTypes.Move:
							gr.DrawRectangle(pn, pt.X - size, pt.Y - size, size * 2, size * 2);
							DrawRadiusGrabSpotArrows(gr, pnTransform, pt, new SizeF(0, 1), size);
							DrawRadiusGrabSpotArrows(gr, pnTransform, pt, new SizeF(1, 0), size);
							break;
						case Shape.GrabTypes.Fixed:
							gr.FillEllipse(Brushes.Black, pt.X - size, pt.Y - size, size * 2, size * 2);
							break;
						case Shape.GrabTypes.EdgeMoveH:
							gr.DrawRectangle(pn, pt.X - size, pt.Y - size, size * 2, size * 2);
							DrawRadiusGrabSpotArrows(gr, pnTransform, pt, new SizeF(1, 0), size);
							break;
						case Shape.GrabTypes.EdgeMoveV:
							gr.DrawRectangle(pn, pt.X - size, pt.Y - size, size * 2, size * 2);
							DrawRadiusGrabSpotArrows(gr, pnTransform, pt, new SizeF(0, 1), size);
							break;
						case Shape.GrabTypes.Invisible:
							break;
						case Shape.GrabTypes.Bezier:
							size *= 0.75F; // these are smaller than most
							gr.DrawEllipse(pn, Geometry.RectangleFromPoint(pt, size));
							gr.DrawLine(pn, pt, grabSpot.Focus);
							break;
						case Shape.GrabTypes.BezierInactive:
							size *= 0.75F; // these are smaller than most
							gr.DrawEllipse(pnTransform, Geometry.RectangleFromPoint(pt, size));
							gr.DrawLine(pnTransform, pt, grabSpot.Focus);
							break;
						default:
							Utilities.ErrorAssert(false, "No graphics defined for GrabSpot type: " + grabSpot.GrabType);
							break;
					}
				}
			}
		}

		private void DrawRadiusGrabSpotArrows(Graphics gr, Pen transformPen, PointF pt, SizeF outwards, float size)
		{
			outwards = outwards.ChangeLength(size * 3);
			// Must be able to draw at any angle.  This is the vector from the GrabSpot to the centre of the outward V:
			SizeF right = outwards.RotateBy(Geometry.Radians(135)).ChangeLength(size * 1.5f);
			SizeF left = outwards.RotateBy(Geometry.Radians(-135)).ChangeLength(size * 1.5f);
			PointF middle = PointF.Add(pt, outwards);
			gr.DrawLines(transformPen, new PointF[] { PointF.Add(middle, right), middle, PointF.Add(middle, left) });
			middle = PointF.Subtract(pt, outwards);
			gr.DrawLines(transformPen, new PointF[] { PointF.Subtract(middle, right), middle, PointF.Subtract(middle, left) });
		}

		private void IncludeGrabSpotsInRefresh(ref RectangleF invalid)
		{
			if (m_GrabSpots == null)
				return;
			foreach (Shape.GrabSpot grabSpot in m_GrabSpots)
			{
				Geometry.Extend(ref invalid, GrabSpotRefreshBounds(grabSpot));
			}
		}

		public RectangleF GrabSpotRefreshBounds(Shape.GrabSpot grabSpot)
		{
			SizeF sz; // this is the size of the GrabSpot - this is the amount required on both sides of the centrepoint of the GrabSpot
					  // the size of the graphics for the GrabSpot depends on what type it is
			PointF pt = grabSpot.Position;
			float size = 1;
			float scale = m_Zoom < 1 ? 1 : (float)(1 / Math.Sqrt(m_Zoom));
			// need to make larger if working in pixels...
			scale = scale * GUIUtilities.MillimetreSize;
			// if active calculate size for maximum of pulsing size...
			if (grabSpot == m_ActiveGrabSpot)
				size += Shape.GrabSpot.MAXIMUMACTIVEPHASE * Shape.GrabSpot.PHASEGROWSIZE;
			switch (grabSpot.GrabType)
			{
				case Shape.GrabTypes.Move:
				case Shape.GrabTypes.Radius:
				case Shape.GrabTypes.EdgeMoveH:
				case Shape.GrabTypes.EdgeMoveV:
					sz = new SizeF((size * 3 + Geometry.THINLINE) * scale, (size * 3 + Geometry.THINLINE) * scale);
					break;
				// does not check the direction, does a square big enough to cover the arrows whichever way they are pointing
				case Shape.GrabTypes.Rotate: // see graphics
					float radius = Geometry.DistanceBetween(pt, grabSpot.Focus); // could just subtract the X of each one at the moment
					sz = new SizeF(radius, radius); // this is correct vertically, a little bit too much to the left, but there is no inefficiency there is that will be inside the shape anyway
					break;
				// and therefore redrawn.  It does include a bit too much to the right which is technically inefficient, but probably not worth worrying about
				case Shape.GrabTypes.SingleVertex:
				case Shape.GrabTypes.Fixed:
				case Shape.GrabTypes.CornerResize:
					sz = new SizeF((size + 1) * scale, (size + 1) * scale);
					break;
				case Shape.GrabTypes.Invisible:
					sz = new SizeF(0, 0);
					break;
				case Shape.GrabTypes.Bezier:
				case Shape.GrabTypes.BezierInactive:
					sz = new SizeF(size * 0.8f * scale, size * 0.8f * scale);
					break;
				default:
					Utilities.ErrorFail("No refresh bounds defined for GrabSpot type: " + grabSpot.GrabType);
					sz = new SizeF(0, 0);
					break;
			}
			//sz = new SizeF(20, 20);
			return new RectangleF(pt.X - sz.Width, pt.Y - sz.Height, sz.Width * 2, sz.Height * 2);
		}

		public Shape.GrabSpot HitTestGrabSpots(PointF clickPoint)
		{
			// returns the GrabSpot which this point hits, nothing otherwise
			float scale = m_Zoom < 1 ? 1 : (float)(1 / Math.Sqrt(m_Zoom));
			// this is now the adjustment to apply to the size of the grabspots
			// need to make larger if working in pixels...
			scale = scale * (m_DpiX / Geometry.INCH) / m_Document.ApproxUnitScale;
			if (m_GrabSpots == null)
				return null;
			foreach (Shape.GrabSpot grabSpot in m_GrabSpots)
			{
				float size = scale;
				if (m_GrabSpots.Count > 1 && grabSpot.Shape != null) // reduce size for v small shapes
				{
					float minSize = 0.5f * scale; // never go below this even if shape is small
					float maxSize = Math.Min(grabSpot.Shape.Bounds.Width, grabSpot.Shape.Bounds.Height) / 6; // divides by 6 since size is half the grabspot
					size = Math.Min(size, maxSize);
					size = Math.Max(size, minSize);
				}
				switch (grabSpot.GrabType)
				{
					case Shape.GrabTypes.Fixed: // never hit
						break;
					case Shape.GrabTypes.Invisible:
						// allows much bigger tolerance because we can't see it
						// however, in this case the mouse must be also inside the shape
						if (Geometry.DirtyDistance(clickPoint, grabSpot.Position) < Shape.GrabSpot.HITTOLERANCEINVISIBLE * size && grabSpot.Shape != null &&
							grabSpot.Shape.HitTestDetailed(clickPoint, size, true))
							return grabSpot;
						break;
					default:
						if (Geometry.DirtyDistance(clickPoint, grabSpot.Position) < Shape.GrabSpot.HITTOLERANCE * size)
							return grabSpot;
						break;
				}
			}
			return null;
		}

		public Shape.GrabSpot ActiveGrabSpot
		{
			get { return m_ActiveGrabSpot; }
			set
			{
				if (value != m_ActiveGrabSpot && m_ActiveGrabSpot != null)
				{
					// must invalidate old active to shrink it back to default presentation
					InvalidateData(GrabSpotRefreshBounds(m_ActiveGrabSpot), InvalidationBuffer.Selection);
				}
				m_ActiveGrabSpot = value;
			}
		}

		#endregion

	}

}

