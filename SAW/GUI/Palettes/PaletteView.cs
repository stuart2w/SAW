using System.Collections.Generic;
using System;
using System.Drawing;
using System.Diagnostics;
using System.Windows.Forms;
using System.Linq;
using SAW.Functions;
using Action = SAW.Functions.Action;
using SAW.Shapes;

namespace SAW
{
	/// <summary>The control representing any custom palette when in use; based upon the standard document view,
	/// but implementing some extra bits needed by palettes</summary>
	internal sealed class PaletteView : StaticView, IPalette, IKeyControl
	{

		private readonly Palette m_Palette;
		/// <summary>Applicability hasn't been updated since not visible</summary>
		private bool m_Dirty;

		public PaletteView(Document document, Palette palette)
		{
			//objDocument = objDocument.Clone(New Dictionary(Of Guid, Datum))
			m_Document = document;
			m_Palette = palette;
			m_Buffers.Add(m_BaseBuffer);
			this.AutoScrollMinSize = this.ClientSize;
			this.AutoScroll = false;
			DisplayPage(document.Page(0), document);
			if (IsFlow)
				ChangeZoom(1);
			else
				ChangeZoom((float)SpecialZooms.FitPage);

			Globals.ParameterChanged += ParameterChanged;
			Globals.ApplicabilityChanged += ApplicabilityChanged;
			Globals.KeyApplicabilityChanged += KeyApplicabilityChanged;
			ApplicabilityChanged(); // to get the initial states correct.  This also updates the selected status where applicable
			UpdateVerbApplicability();
			Globals.Root.CurrentDocumentChanged += CurrentDocumentChanged;

			this.HoverShapeChanged += PaletteView_HoverShapeChanged;
			this.GotFocus += PaletteView_GotFocus;
			this.VisibleChanged += (s, e) =>
			{
				if (Visible && m_Dirty)
				{
					UpdateVerbApplicability();
					ApplicabilityChanged();
					m_Dirty = false;
				}
			};
			Globals.VerbApplicabilityChanged += UpdateVerbApplicability;
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			Globals.ParameterChanged -= ParameterChanged;
			Globals.ApplicabilityChanged -= ApplicabilityChanged;
			Globals.KeyApplicabilityChanged -= KeyApplicabilityChanged;
			Globals.Root.CurrentDocumentChanged -= CurrentDocumentChanged;
			Globals.VerbApplicabilityChanged -= UpdateVerbApplicability;
		}

		/// <summary>true if the palette is currently being edited </summary>
		private bool BeingEdited => Globals.Root.CurrentDocument == m_Document;

		private bool m_WasBeingEdited;
		private void CurrentDocumentChanged()
		{
			bool edited = BeingEdited;
			if (edited != m_WasBeingEdited)
			{
				m_WasBeingEdited = edited;
				InvalidateAll();
				if (edited)
				{
					// Want to reset all buttons to their normal state.  Don't want them appearing selected or disabled in the editor
					m_Page.Iterate(obj =>
					{
						if (obj is ButtonShape) ((ButtonShape)obj).State = ButtonShape.States.Normal;
					});
				}
				else
				{
					DisplayPage(m_Document.Page(0), m_Document);
					// it is necessary to do this to trigger the AddShapeForParameters again
					//ApplicabilityChanged(m_ctrGUI) ' Will set the correct state where necessary
				}
			}
		}

		protected override bool UseActiveContent => !BeingEdited;

		protected override void ShapePressed(Shape shape)
		{
			if (!(shape is ButtonShape))
				return;
			ButtonShape buttonShape = (ButtonShape)shape;
			if (buttonShape.State == ButtonShape.States.Disabled)
				return;
			Accessed?.Invoke(this, EventArgs.Empty);
			switch (buttonShape.Action.Change)
			{
				case Parameters.Action_Key:
				case Parameters.Action_Text:
				case Parameters.Action_Character:
					// any typing button needs to put focus back first
					Globals.Root.PerformAction(Verb.Find(Codes.RestoreFocus));
					break;
			}
			Globals.Root.PerformAction(buttonShape.Action, ClickPosition.Sources.Pad);
		}

		protected override void m_Page_ShapeNotifiedIndirectChange(Shape shape, ChangeAffects affected, RectangleF area)
		{
			if ((affected & ChangeAffects.RepaintNeeded) > 0)
			{
				if (area.IsEmpty)
					InvalidateData(shape.RefreshBounds(false), InvalidationBuffer.All);
				else
					InvalidateData(area, InvalidationBuffer.All);
			}
		}

		protected override void View_Paint(object sender, PaintEventArgs e)
		{
			// We don't use the base class logic because the last several bits we don't really want:
			// we don't want the usual page background with a grid.  Helpful to have the grid when editing, but we don't want it to display at runtime
			// There is also no need to draw the outside page area; it's nicer just to extend the background colour is needed
			m_BaseBuffer.InvalidateAll();
			if (this.DesignMode)
				return;
			if (m_Page == null)
			{
				e.Graphics.Clear(Color.Tomato);
				return;
			}
			try
			{
				using (Graphics gr = m_BaseBuffer.PrepareDraw())
				{
					using (NetCanvas canvas = new NetCanvas(gr))
					{
						if (gr != null)
						{
							// some redrawing is required
							PrepareGraphics(gr);
							gr.Clear(Color.FromKnownColor(KnownColor.Control));
							//gr.Clear(m_Page.Colour)
							//m_Page.DrawBackground(gr, m_zoom)' Would also draw grid and (possibly) origin
							if (!BeingEdited)
								m_Page.DrawShapes(canvas, m_Zoom, m_PixelsPerDocumentX, this);
							else
							{
								using (System.Drawing.Drawing2D.HatchBrush br = new System.Drawing.Drawing2D.HatchBrush(System.Drawing.Drawing2D.HatchStyle.DiagonalCross, Color.FromArgb(255, 200, 200), Color.Empty))
								{
									gr.FillRectangle(br, m_Page.Bounds);
								}

								using (Font objFont = new Font(FontFamily.GenericSansSerif, 14f / (m_Zoom * m_PixelsPerDocumentX), FontStyle.Bold, GraphicsUnit.Point)
								) // Division is because a scale transform is applied to increase size by this
								{
									gr.DrawString(Strings.Item("Palette_BeingEdited"), objFont, Brushes.DarkGray, m_Page.Bounds, GUIUtilities.StringFormatCentreCentre);
								}
							}
							// Does not draw the outside valid page grey area
						}
					}
				}

				// now that the buffers are ready, we can draw it on screen
				e.Graphics.DrawImageUnscaled(m_BaseBuffer.m_bmp, 0, 0);
			}
			catch (Exception ex)
			{
				Utilities.LogSubError(ex.Message);
			}
		}

		/// <summary>True if this contains a document which can flow, and therefore does not scale</summary>
		public bool IsFlow => m_Page.IsSingleAutoSize;

		protected override void StaticView_Resize(object sender, EventArgs e)
		{
			base.StaticView_Resize(sender, e);
			if (IsFlow)
			{
				m_RezoomNeeded = false;
				m_SpecialZoom = SpecialZooms.None;
				m_ClientSize = new Size((int)(m_Page.Size.Width * m_Zoom * m_PixelsPerDocumentX), (int)(m_Page.Size.Height * m_Zoom * m_PixelsPerDocumentY));
				InvalidateAll(InvalidationBuffer.Base);
			}

			if (m_RezoomNeeded)
				// normal view does this on timer to avoid scroll bars.  No timer here, and prob no danger of scroll bars
				ChangeZoom((float)m_SpecialZoom);
		}

		protected override void OnMouseWheel(MouseEventArgs e)
		{
			if (ModifierKeys > 0)
				return; // don't want ctrl+mouse to zoom! Which is implemented in StaticView.  (was pinch-zooming on *some* laptops)
			base.OnMouseWheel(e);
		}

		private void PaletteView_HoverShapeChanged(Shape shape)
		{
			if (m_ShapeHover == null) // Condition actually redundant at the moment, but maybe need to hover other things later
			{
				Globals.ClearHover();
				m_Tooltips.SetToolTip(this, null);
			}
			else
			{
				Globals.SetHover(m_ShapeHover.Action);
				m_Tooltips.SetToolTip(this, m_ShapeHover.Action.DescriptionWithoutAccelerator());
			}
		}

		public void UpdateVerbApplicability()
		{
			if (!Visible)
			{
				m_Dirty = true;
				return;
			}
			foreach (ButtonShape button in m_OtherApplicable)
			{
				button.State = Globals.ActionApplicable(button.Action) ? ButtonShape.States.Normal : ButtonShape.States.Disabled;
			}
		}

		#region Keys
		protected override bool IsInputKey(Keys keyData)
		{
			return true; // can process everything in these
		}

		public void CombinedKeyDown(CombinedKeyEvent e)
		{
			//if ((((e.KeyCode == System.Windows.Forms.Keys.Right) || (e.KeyCode == System.Windows.Forms.Keys.Left)) || (e.KeyCode == System.Windows.Forms.Keys.Up)) || (e.KeyCode == System.Windows.Forms.Keys.Down))
			//{
			//}
			//else if ((e.KeyCode == System.Windows.Forms.Keys.Tab) || (e.KeyCode == (System.Windows.Forms.Keys.Tab | System.Windows.Forms.Keys.Shift)))
			//{
			//}
		}

		public void CombinedKeyUp(CombinedKeyEvent e)
		{ }

		//private Point m_ptOldCurser;
		public void BringPointerInsideOnActivation()
		{
			var shapes = Shape.FlattenList(m_Page.Shapes.ToList(), Shape.FlatListPurpose.ExposedShapes).Where(x => x.ShapeCode == Shape.Shapes.Button).ToList();
			shapes.Sort(Shape.YXSort);
			if (!shapes.Any())
				return;
			//m_ptOldCurser = Cursor.Position;
			MovePointerTo(shapes.First().Middle());
		}

		private void MovePointerTo(PointF dataPoint)
		{
			PointF GUIPoint = this.DataToClient(dataPoint);
			Point screenPoint = this.PointToScreen(GUIPoint.ToPoint());
			Cursor.Position = screenPoint;
		}

		#endregion

		#region Linking to ParameterGUI
		/// <summary>This maintains a list of all the shapes which need to be updated when each parameter is changed
		/// All actions on contained buttons will be of type ValueSelectableAction</summary>
		private readonly Dictionary<Parameters, List<ButtonShape>> m_Parameters = new Dictionary<Parameters, List<ButtonShape>>();
		// and these need to be updated when the main form changes verb applicability
		private readonly List<ButtonShape> m_OtherApplicable = new List<ButtonShape>();
		/// <summary>List of shapes which generate keys and could be disabled when KeyApplicabilityChanged</summary>
		private readonly List<ButtonShape> m_KeyShapes = new List<ButtonShape>();

		public override void DisplayPage(Page page, Document document)
		{
			base.DisplayPage(page, document);
			m_Parameters.Clear();
			m_OtherApplicable.Clear();
			page.Iterate(AddShapeForParameters); // Will iterate through all nested shapes
			if (Globals.Root.CurrentMainScreen() is frmMain)
				UpdateVerbApplicability();
		}

		private void AddShapeForParameters(Datum obj)
		{
			// Adds the given shape to m_Parameters
			if (!(obj is ButtonShape))
				return;
			Action action = ((ButtonShape)obj).Action;
			if (action is ValueSelectableAction)
			{
				// Need to check if we have a list for this parameter yet...
				if (!m_Parameters.ContainsKey(action.Change))
					m_Parameters.Add(action.Change, new List<ButtonShape>());
				m_Parameters[action.Change].Add((ButtonShape)obj);
			}
			else if (action.Change == Parameters.Action_Key || action.Change == Parameters.Action_Character || action.Change == Parameters.Action_Text)
				m_KeyShapes.Add((ButtonShape)obj);
			else// if (action.Change == Parameters.Action_Verb)
				m_OtherApplicable.Add((ButtonShape)obj);
		}

		private void ParameterChanged(Parameters parameter)
		{
			if (BeingEdited)
				return;
			if (!m_Parameters.ContainsKey(parameter))
				return;
			int value = Globals.ParameterValue(parameter);
			foreach (ButtonShape button in m_Parameters[parameter])
			{
				button.State = (button.Action as ValueSelectableAction).ValueAsInteger == value ? ButtonShape.States.Selected : ButtonShape.States.Normal;
			}
		}

		private void ApplicabilityChanged()
		{
			// This does not specify the parameter as it is called once when everything updates
			if (BeingEdited)
				return;
			if (!Visible)
			{ m_Dirty = true; return; }
			foreach (Parameters parameter in m_Parameters.Keys)
			{
				bool applicable = Globals.ParameterApplicable(parameter);
				foreach (ButtonShape button in m_Parameters[parameter])
				{
					if (parameter == Parameters.Tool) // bit of a bodge at the moment, but tools can be individually applicable
						applicable = button.Action.IsApplicable(null);
					if (applicable)
						button.State = Globals.ParameterValue(parameter) == (button.Action as ValueSelectableAction).ValueAsInteger ? ButtonShape.States.Selected : ButtonShape.States.Normal;
					else
						button.State = ButtonShape.States.Disabled;
				}
			}
		}

		private void KeyApplicabilityChanged()
		{
			if (BeingEdited)
				return;
			foreach (var shape in m_KeyShapes)
			{
				var bolApplicable = true;
				if (shape.Action.Change == Parameters.Action_Key)
					bolApplicable = Globals.IsKeyApplicable((shape.Action as KeyAction).Key);
				else if (shape.Action.Change == Parameters.Action_Character)
					bolApplicable = Globals.IsKeyApplicable((shape.Action as CharAction).Character);
				else if (shape.Action.Change == Parameters.Action_Text)
				{
					string text = (shape.Action as TextAction).Text;
					if (text.Length > 0)
					{
						// just tests first char - acts as a test that text generally can be typed.
						// or might this give odd results if equation palette open with something that actually responds to keys
						bolApplicable = Globals.IsKeyApplicable(text[0]);
					}
				}
				shape.State = bolApplicable ? ButtonShape.States.Normal : ButtonShape.States.Disabled;
			}
		}

		#endregion

		#region IPalette stuff
		public event EventHandler Accessed;
		public event NullEventHandler FocusChanged;
		private ToolTip m_Tooltips;

		public void SubscribeEvents(EventHandler AccessedHandler, NullEventHandler FocusChangedHandler)
		{
			if (AccessedHandler != null)
				Accessed += AccessedHandler;
			if (FocusChangedHandler != null)
				FocusChanged += FocusChangedHandler;
		}

		public void Initialise()
		{
		}

		public void SetToolTips(ToolTip tt)
		{
			m_Tooltips = tt;
		}

		public Size SizeFromSize(Size requested)
		{
			if (IsFlow)
			{
				SizeF szData = ClientToData(requested);
				if (!m_Document.PaletteDesignSize.IsEmpty)
				{
					// scaling is only permitted if we have a definite original size
					float zoom = (float)(Math.Sqrt(requested.ToSizeF().Area() / m_Document.PaletteDesignSize.Area()) / m_PixelsPerDocumentX);
					szData = szData.MultiplyBy(m_Document.PaletteDesignSize.Area() / szData.Area()); // Has the new aspect ratio, but the same total area as the design size
																									 // not sure why we want to reset to original size (which appears to be small!)
					szData = ((IAutoSize)m_Page.Shapes.First()).AutoSize(szData);
					m_SpecialZoom = 0;
					if (m_Zoom != zoom)
						m_Palette.InvalidatePreview();
					m_Zoom = zoom;
					// Note that this must not use the base class methods such as ChangeZoom, because they will try and scroll the "page"
				}
				else
					szData = ((IAutoSize)m_Page.Shapes.First()).AutoSize(szData);
				m_Page.SetSize(szData, 0);
				return DataToClient(szData).ToSize();
			}
			return m_Page.Size.ChangeLength(requested.Length()).ToSize(); // same shape as page, but diagonal length of request
		}

		public Size SizeFromWidth(int requested)
		{
			if (IsFlow)
			{
				// If the page contains only a flow layout, then it is allowed to resize itself
				if (requested == 0)
					return DataToClient(m_Page.Size).ToSize(); // This can occur when the palettes are initialised
				SizeF dataSize = ClientToData(new SizeF(requested, 0));
				if (BeingEdited && !m_Document.PaletteDesignSize.IsEmpty)
				{
					// While the palette is being edited, we don't want to adjust the page size here!
					dataSize = m_Document.PaletteDesignSize;
				}
				else
				{
					if (!m_Document.PaletteDesignSize.IsEmpty)
					{
						// can scale
						dataSize.Width *= m_Zoom; // size if zoom had been 1 originally
						dataSize = ((IAutoSize)m_Page.Shapes.First()).AutoSize(dataSize);
						m_Zoom = requested / (dataSize.Width * m_PixelsPerDocumentX); // return width should be pretty much intRequested as this will scale the chosen size up to the request
						m_SpecialZoom = SpecialZooms.None;
					}
					else
						dataSize = ((IAutoSize)m_Page.Shapes.First()).AutoSize(dataSize);
					m_Page.SetSize(dataSize, 0);
				}
				return DataToClient(dataSize).ToSize();
			}
			int height = (int)(requested * m_Page.Size.Height / m_Page.Size.Width);
			if (!m_Document.PaletteDesignSize.IsEmpty)
			{
				// if it would be quite close to 100% scale, then use that
				// This stops the buttons being slightly expanded all the time
				float design = m_Document.PaletteDesignSize.Height * m_PixelsPerDocumentY;
				if (height >= design && height < design * 1.2)
					return new Size(requested, (int)design);
			}
			return new Size(requested, height);
		}

		private void PaletteView_GotFocus(object sender, EventArgs e)
		{
			FocusChanged?.Invoke();
		}

		#endregion

	}

}
