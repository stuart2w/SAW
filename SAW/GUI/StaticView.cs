using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace SAW
{
	/// <summary>Simplified view of page which does not support any editing, only displays a page.  Base class for other views which do more interesting stuff</summary>
	public abstract class StaticView : ScrollableControl
	{

		public delegate void DisplayedAreaChangedEventHandler(RectangleF displayed);
		public event DisplayedAreaChangedEventHandler DisplayedAreaChanged;// notifies the part of the page displayed in data coordinates; RectangleF.empty if the entire page is displayed

		public delegate void HoverShapeChangedEventHandler(Shape shape);
		public event HoverShapeChangedEventHandler HoverShapeChanged; // shape being hovered over has changed.  Parameter is new one

		protected Document m_Document;
		protected Page m_Page;
		protected Size m_ClientSize; // the size of the page AFTER ZOOMING IN PIXELS

		protected StaticView()
		{
			// ReSharper disable VirtualMemberCallInConstructor
			// overriding classes aren't going to be bothered by this happening before construction:
			this.BackColor = Color.Gray;
			this.DoubleBuffered = true;
			// ReSharper restore VirtualMemberCallInConstructor
			m_Buffers.Add(m_BaseBuffer);
			using (Graphics gr = CreateGraphics())
			{
				m_DpiX = gr.DpiX;
				m_DpiY = gr.DpiY;
			}

			this.MouseEnter += StaticView_MouseEnter;
			this.MouseDown += StaticView_MouseDown;
			this.MouseLeave += StaticView_MouseLeave;
			this.MouseMove += StaticView_MouseMove;
			this.MouseUp += StaticView_MouseUp;
			this.Resize += StaticView_Resize;
			this.Scroll += StaticView_Scroll;
			this.Paint += View_Paint;
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			if (m_BaseBuffer != null)
			{
				m_BaseBuffer.Discard();
			}
			m_BaseBuffer = null;
			m_Page = null;
			m_Document = null;
		}

		public virtual void DisplayPage(Page page, Document document)
		{
			RemovePageHandlers();
			m_Page = page;
			m_Document = document;
			m_SelectionBoundsDrawn = RectangleF.Empty;
			if (document != null)
			{ // otherwise leave PixelsPerDoc - won't be needed until a doc is displayed
				var unit = new SizeF(1, 1);
				m_Document.ApplyUnitScale(ref unit, m_DpiX, m_DpiY);
				m_PixelsPerDocumentX = unit.Width;
				m_PixelsPerDocumentY = unit.Height;
			}
			if (m_Page != null)
			{
				m_Page.RefreshSelection += m_Page_RefreshSelection;
				m_Page.RequestRefresh += m_Page_RequestRefresh;
				m_Page.SelectionChanged += m_Page_SelectionChanged;
				m_Page.ShapeNotifiedIndirectChange += m_Page_ShapeNotifiedIndirectChange;
				m_Page.AssumeRefreshScale = m_Zoom;
				m_ClientSize = new Size((int)(m_Page.Size.Width * m_Zoom * m_PixelsPerDocumentX), (int)(m_Page.Size.Height * m_Zoom * m_PixelsPerDocumentY));
			}
			if (this.AutoScroll)
			{
				m_SpecialZoom = SpecialZooms.None;
				this.AutoScrollMinSize = m_ClientSize;
				this.AutoScrollPosition = new Point(0, 0);
			}
			InvalidateAll();
			NotifyDisplayedArea(); // might be different once the page has changed
		}

		/// <summary>Called when the form is being hidden and is no longer displaying the page</summary>
		public virtual void EndDisplayPage()
		{
			RemovePageHandlers();
			m_Page = null;
		}

		protected virtual void RemovePageHandlers()
		{
			if (m_Page == null)
				return;
			m_Page.RefreshSelection -= this.m_Page_RefreshSelection;
			m_Page.RequestRefresh -= this.m_Page_RequestRefresh;
			m_Page.SelectionChanged -= this.m_Page_SelectionChanged;
			m_Page.ShapeNotifiedIndirectChange -= this.m_Page_ShapeNotifiedIndirectChange;
		}

		#region General control events

		// note most of these also handled in the complete class

		private void StaticView_MouseEnter(object sender, EventArgs e)
		{
			// we don't capture the mouse if a button was pressed on leaving, so detect the mouse re-entry
			if (m_ShapePressed != null && MouseButtons != MouseButtons.Left)
			{
				m_ShapePressed = null;
				ClearShapeHover();
			}
		}

		private void StaticView_MouseLeave(object sender, EventArgs e)
		{
			ClearShapeHover();
		}

		private void StaticView_MouseMove(object sender, MouseEventArgs e)
		{
			UpdateShapeHover(MouseToData(e.Location));
		}

		private void StaticView_MouseDown(object sender, MouseEventArgs e)
		{
			if (m_ShapeHover != null && m_ShapeHover.State != ButtonShape.States.Disabled)
			{
				m_ShapePressed = m_ShapeHover;
				if (m_ShapeHover.SelectDuringUserPress)
					m_ShapeHover.State = ButtonShape.States.Selected;
			}
			else
				m_ShapePressed = null;
		}

		private void StaticView_MouseUp(object sender, MouseEventArgs e)
		{
			if (m_ShapePressed != null)
			{
				ButtonShape pressed = m_ShapePressed;
				m_ShapePressed = null;
				ClearShapeHover();
				ShapePressed(pressed);
			}
		}

		protected virtual void ShapePressed(Shape shape)
		{
			// Only needed in PaletteView, which uses this to respond to button clicks
		}

		protected virtual void StaticView_Resize(object sender, EventArgs e)
		{
			if (this.DesignMode)
				return;
			foreach (Buffer buffer in m_Buffers)
			{
				buffer.Size = this.Size;
			}
			// will update the zoom if fitting page/width
			m_RezoomNeeded = true; // needed even if not special scroll to update scrollbars
			NotifyDisplayedArea();
		}

		private void StaticView_Scroll(object sender, ScrollEventArgs e)
		{
			if (this.DesignMode)
				return;
			InvalidateAll();
			NotifyDisplayedArea();
		}

		protected override void OnMouseWheel(MouseEventArgs e)
		{
			// Need to override rather than handle the event in order to prevent the standard behaviour
			if (this.DesignMode)
				return;
			if ((ModifierKeys & Keys.Control) > 0)
			{
				if (e.Delta > 0)
					ChangeZoom(m_Zoom * ZOOMSTEP);
				else if (e.Delta < 0)
					ChangeZoom(m_Zoom / ZOOMSTEP);
			}
			else if ((ModifierKeys & Keys.Shift) > 0)
			{
				// want to scroll horizontally instead of vertically
				DoScroll(new Size(e.Delta / 10, 0)); // 10 is a guess, but seems to make it work similarly to the vertical scroll
			}
			else
			{
				base.OnMouseWheel(e);
				InvalidateAll();
				NotifyDisplayedArea();
			}
		}

		#endregion

		#region Public information and properties
		[System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
		public float Zoom
		{
			[DebuggerStepThrough]
			get { return m_Zoom; }
		}

		public Page CurrentPage
		{
			[DebuggerStepThrough()]
			get
			{
				return m_Page;
			}
		}

		#endregion

		#region Painting and invalidation
		private static readonly Brush OUTSIDEPAGECOLOUR = new SolidBrush(Color.LightGray);
		protected RectangleF m_SelectionBoundsDrawn = RectangleF.Empty;
		// we need to remember where we have drawn selection boundary because the page itself only requests refreshes of the objects whose selection changes
		// and not the entire selection box

		protected virtual void View_Paint(object sender, PaintEventArgs e)
		{
			if (this.DesignMode)
				return;
			if (m_Page == null || m_Page.IsDisposed)
			{
				e.Graphics.Clear(Color.Tomato);
				return;
			}
			try
			{
				Graphics gr = m_BaseBuffer.PrepareDraw();
				if (gr != null)
				{
					var canvas = new NetCanvas(gr);
					try
					{
						// some redrawing is required
						PrepareGraphics(gr);
						m_Page.DrawBackground(canvas, m_Zoom, true, !(this is RunView), !(this is RunView)); // grid (and origin) disabled when running
						if (!Globals.Root.CurrentConfig.Low_Graphics)
							gr.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
						// if the page does not fill the control, then draw a grey area outside page
						// it is helpful of this is done after drawing the page itself in case anything on the page has fallen off
						m_Page.DrawShapes(canvas, m_Zoom, m_PixelsPerDocumentX, this);

						// if the page is too short, then the bottom of the screen contains the actual page
						gr.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None; // AA would blur the pixel boundary between the two, leaving it neither page colour nor outside colour
						if (Width > m_ClientSize.Width)
							gr.FillRectangle(OUTSIDEPAGECOLOUR, m_Page.Size.Width, -10000 / m_Zoom, 10000 / m_Zoom, 11000 / m_Zoom);
						gr.PageUnit = GraphicsUnit.Pixel;
						gr.ResetTransform();
						if (Height > m_ClientSize.Height)
							gr.FillRectangle(OUTSIDEPAGECOLOUR, 0, Convert.ToInt32(m_Page.Size.Height * m_Zoom * m_PixelsPerDocumentY), Width, Height - m_Page.Size.Height * m_Zoom * m_PixelsPerDocumentY);
					}
					finally
					{
						gr.Dispose();
						canvas.Dispose();
					}
				}
				PaintEditing(e);

				// now that the buffers are ready, we can draw it on screen
				e.Graphics.DrawImageUnscaled(m_BaseBuffer.m_bmp, 0, 0);
				PaintEditingBuffers(e);
			}
			catch (Exception ex) when (!Globals.Root.IsDebug)
			{
				// Leave a breakpoint here for debug mode (multiple assertions can get nasty)
				Utilities.LogSubError(ex.Message);
			}
		}

		protected virtual void PaintEditing(PaintEventArgs e)
		{
			// the parts of the painting to buffers which only apply to the full version
		}

		protected virtual void PaintEditingBuffers(PaintEventArgs e)
		{
			// Second part of the above, which draws the buffers to the screen
		}

		protected void PrepareGraphics(Graphics gr, bool withLog = false)
		{
			// set scrolling position, offsets etc in the graphics
			gr.TranslateTransform(+AutoScrollPosition.X, m_ClientSize.Height + AutoScrollPosition.Y - 1);
			gr.ScaleTransform(m_Zoom, m_Zoom);
			m_Document.ApplyUnitScale(gr);
			if (withLog)
			{
				Globals.Root.Log.WriteLine("this.AutoScrollPosition.Y = " + AutoScrollPosition.Y);
				Globals.Root.Log.WriteLine("m_PixelsPerDocumentY = " + m_PixelsPerDocumentY);
				Globals.Root.Log.WriteLine("gr.DpiY = " + gr.DpiY);
				Globals.Root.Log.WriteLine("ClientSize.Height = " + m_ClientSize.Height);
			}
			if (!Utilities.Low_Graphics_Safe())
			{
				gr.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
				gr.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias; // otherwise text on transparent buffer is naff
			}
		}

		protected override void OnPaintBackground(PaintEventArgs e)
		{
			// we don't want to paint the background at all - the normal painting will cover everything.  Except in design mode - when the painting does nothing
			if (this.DesignMode)
				base.OnPaintBackground(e);
		}

		[Flags()]
		public enum InvalidationBuffer
		{
			/// <summary>stored shapes</summary>
			Base = 1,
			/// <summary>if selection changed currently base must also be repainted - but this could change.
			/// ExcludedArea should be added manually as needed (ExcludedArea usually drawn in this buffer)</summary>
			Selection = 2,
			/// <summary>only selection buffer - this included in Selection</summary>
			SelectionHighlightOnly = 4,
			/// <summary>shape currently being drawn</summary>
			Current = 8,
			All = 255,
			NotApplicable = 256 // not used in View - used in diagnostics when drawing shapes a bit
		}

		public virtual void InvalidateData(RectangleF invalid, InvalidationBuffer buffer, bool textFlowExpand = true)
		{
			// invalidate part of the picture - the area is specified in data coordinates
			// must invalidate both the buffer and Windows itself
			// last parameter can be false if we know that the change will not move the text flow arrows
			if (invalid.IsEmpty || m_Page == null || m_BaseBuffer == null)
				return;
			Rectangle screen = Rectangle.Round(DataToClient(invalid));
			Invalidate(screen);
			if ((buffer & (InvalidationBuffer.Base | InvalidationBuffer.Selection)) > 0)
				m_BaseBuffer.Invalidate(screen); //: Debug.WriteLine("Invalidate base: " + rctInvalid.ToString)
		}

		public virtual void InvalidateAll(InvalidationBuffer buffer = InvalidationBuffer.All)
		{
			// NB selection does not update Base, unlike above
			if (m_Page == null || m_BaseBuffer == null)
				return;
			if ((buffer & InvalidationBuffer.Base) > 0)
				m_BaseBuffer.InvalidateAll();
			Invalidate();
		}

		protected virtual void m_Page_RefreshSelection(RectangleF dataRect, bool dataInvalid, bool updateGrabSpots)
		{
			// Overridden in EditableView to handle GrabSpots
			InvalidateData(dataRect, dataInvalid ? InvalidationBuffer.All : InvalidationBuffer.Selection);
		}

		private void m_Page_RequestRefresh(RectangleF dataRect)
		{
			// note that this is not fired when moving the current shape - the current shape is not part of the page until it is completed
			InvalidateData(dataRect, InvalidationBuffer.All);
		}

		protected virtual void m_Page_SelectionChanged()
		{
			// mostly the page will request an update using m_Page_RefreshSelection
			// however, it does not take account of the selection boundary, and only requests for the area covering the shapes which have changed
			// if the boundary was in use we need to be sure to refresh the entire thing
			if (!m_SelectionBoundsDrawn.IsEmpty && Globals.Root.CurrentConfig.ReadBoolean(Config.Selection_Bounds))
			{
				m_SelectionBoundsDrawn.Inflate(1, 1); // to account for the fact that the line is quite wide
				InvalidateData(m_SelectionBoundsDrawn, InvalidationBuffer.Selection);
				m_SelectionBoundsDrawn = RectangleF.Empty; // because there is no need to make this request again until it actually gets drawn
														   // (which sets m_rctSelectionBoundsDrawn).  In particular if we allow to repeat it might get repeatedly inflated
			}
		}

		protected abstract void m_Page_ShapeNotifiedIndirectChange(Shape shape, ChangeAffects affected, RectangleF area);
		// Nothing is implemented in the base class, because although the first step of invalidation could be, EditableView would like to update eAffected to track what has been handled

		#endregion

		#region Coordinate conversion and any other coordinate logic
		protected float m_Zoom = 1; // as 0..1, not percentage
		protected float m_DpiX;
		protected float m_DpiY;
		/// <summary>Depends on doc units.  Set in DisplayPage (which should be called again if units change)</summary>
		protected float m_PixelsPerDocumentX;
		protected float m_PixelsPerDocumentY;
		protected SpecialZooms m_SpecialZoom = SpecialZooms.None; // -1 for fit page; -2 for fit width
		public enum SpecialZooms
		{
			None = 0,
			FitPage = -1,
			FitWidth = -2
		}
		/// <summary>Forces zoom (and therefore scrollbars) to be recalculated on timer</summary>
		protected bool m_RezoomNeeded; // = false;

		public const float MINIMUMZOOM = 0.4f;
		public const float MAXIMUMZOOM = 5;
		public static bool UnrestrictedZoom = false; // true when diagnostics panel open
		public const float ZOOMSTEP = 1.25f;

		internal PointF MouseToData(Point pt)
		{
			// converts the mouse coordinate (local to this control) into data coordinates
			float Y = pt.Y - m_ClientSize.Height - this.AutoScrollPosition.Y;
			float X = (pt.X - this.AutoScrollPosition.X) / m_Zoom / m_PixelsPerDocumentX;
			Y = Y / m_Zoom / m_PixelsPerDocumentY;
			return new PointF(X, Y);
		}

		protected RectangleF DataToClient(RectangleF dataRect)
		{
			RectangleF screenRect = new RectangleF(dataRect.X * m_Zoom * m_PixelsPerDocumentX, dataRect.Y * m_Zoom * m_PixelsPerDocumentY,
				dataRect.Width * m_Zoom * m_PixelsPerDocumentX, dataRect.Height * m_Zoom * m_PixelsPerDocumentY);
			screenRect.X += this.AutoScrollPosition.X;
			screenRect.Y = screenRect.Y + this.AutoScrollPosition.Y + m_ClientSize.Height; //- rctScreen.Height
			return screenRect;
		}

		internal PointF DataToClient(PointF dataPoint)
		{
			PointF screenPoint = new PointF(dataPoint.X * m_Zoom * m_PixelsPerDocumentX, dataPoint.Y * m_Zoom * m_PixelsPerDocumentY);
			screenPoint.X += this.AutoScrollPosition.X;
			screenPoint.Y = screenPoint.Y + this.AutoScrollPosition.Y + m_ClientSize.Height;
			//  If m_szClient.Height < Me.Height Then ptScreen.Y += Me.Height - m_szClient.Height
			return screenPoint;
		}

		protected SizeF DataToClient(SizeF dataSize)
		{
			return new SizeF(dataSize.Width * m_Zoom * m_PixelsPerDocumentX, dataSize.Height * m_Zoom * m_PixelsPerDocumentY);
		}

		protected SizeF ClientToData(SizeF clientSize)
		{
			return new SizeF(clientSize.Width / (m_Zoom * m_PixelsPerDocumentX), clientSize.Height / (m_Zoom * m_PixelsPerDocumentY));
		}

		/// <summary>Returns the displayed part of the page in data coordinates, never going outside the page</summary>
		public RectangleF ViewableArea()
		{
			PointF topLeft = MouseToData(Point.Empty);
			PointF bottomRight = MouseToData(new Point(ClientSize));
			// if the display is bigger than the page, then we want to use the right-hand side of the page, ignoring the wasted display
			if (bottomRight.X > m_Page.Size.Width)
				bottomRight.X = m_Page.Size.Width;
			if (-topLeft.Y > m_Page.Size.Height)
				topLeft.Y = -m_Page.Size.Height;
			return new RectangleF(topLeft.X, topLeft.Y, bottomRight.X - topLeft.X, bottomRight.Y - topLeft.Y);
		}

		/// <summary>Public request to change zoom, accepts SpecialZooms codes</summary>
		public void ChangeZoom(float newZoom)
		{
			// ReSharper disable CompareOfFloatsByEqualityOperator
			if (newZoom == (int)SpecialZooms.FitPage)
			{
				m_SpecialZoom = SpecialZooms.FitPage;
				float xRequired = (Width - 2) / m_PixelsPerDocumentX / m_Page.Size.Width; // -2 empirical Hscroll keeps appearing w/o this
				float yRequired = Height / m_PixelsPerDocumentY / m_Page.Size.Height;
				newZoom = Math.Min(xRequired, yRequired);
			}
			else if (newZoom == (int)SpecialZooms.FitWidth)
			{
				m_SpecialZoom = SpecialZooms.FitWidth;
				newZoom = (this.ClientSize.Width - 2) / m_PixelsPerDocumentX / m_Page.Size.Width;
			}
			else
				m_SpecialZoom = 0;
			// ReSharper restore CompareOfFloatsByEqualityOperator
			newZoom = Math.Max(newZoom, MINIMUMZOOM);
			newZoom = Math.Min(newZoom, UnrestrictedZoom ? 10000.0F : MAXIMUMZOOM);
			if (m_Page != null && m_Page.SelectedCount > 0)
			{
				// centre on selected shapes, but only if they are visible
				RectangleF selectedBounds = m_Page.SelectedBounds(true);
				if (selectedBounds.IntersectsWith(ViewableArea()))
				{
					selectedBounds = DataToClient(selectedBounds);
					ChangeZoom(newZoom, new Point((int)(selectedBounds.X + selectedBounds.Width / 2), (int)(selectedBounds.Y + selectedBounds.Height / 2)));
				}
				else
					ChangeZoom(newZoom, new Point(Width / 2, Height / 2));
			}
			else
			{
				// centre on visible area
				ChangeZoom(newZoom, new Point(Width / 2, Height / 2));
			}
		}

		protected virtual void ChangeZoom(float newZoom, Point ptMaintainLocal)
		{
			// ptMaintainLocal is the point in local coordinates which should remain stationary (subject to page being big enough)
			// ReSharper disable once CompareOfFloatsByEqualityOperator
			if (newZoom == m_Zoom && !m_RezoomNeeded)
				return;
			if (m_Page == null)
			{
				m_Zoom = newZoom;
				return;
			}
			AutoScroll = newZoom > 1 || !Globals.Root.CurrentConfig.ReadBoolean(Config.Resize_Document_ToWindow, true);
			InvalidateAll();
			PointF ptMaintainData = MouseToData(ptMaintainLocal); // the data coordinates for the points which should be maintained
			m_Zoom = newZoom;
			m_Page.AssumeRefreshScale = m_Zoom;
			m_ClientSize = new Size((int)(m_Page.Size.Width * m_Zoom * m_PixelsPerDocumentX), (int)(m_Page.Size.Height * m_Zoom * m_PixelsPerDocumentY));
			// now we need to adjust the scrollbars to get ptMaintainData to appear at ptMaintainLocal again
			Point ptScroll = new Point();
			if (m_ClientSize.Width <= Width || m_SpecialZoom == SpecialZooms.FitWidth)
				ptScroll.X = 0; // page is smaller than this control
			else
				ptScroll.X = (int)(ptMaintainData.X * m_Zoom * m_PixelsPerDocumentX - ptMaintainLocal.X);
			if (m_ClientSize.Height <= Height)
				ptScroll.Y = 0;
			else
				ptScroll.Y = (int)((m_Page.Size.Height + ptMaintainData.Y) * m_Zoom * m_PixelsPerDocumentY - ptMaintainLocal.Y);
			if (this.AutoScroll) // Will be false for PaletteViews
			{
				if (m_SpecialZoom == SpecialZooms.FitWidth)
					this.AutoScrollMinSize = new Size(0, m_ClientSize.Height);
				else
					this.AutoScrollMinSize = m_ClientSize;
				this.AutoScrollPosition = ptScroll;
			}
			NotifyDisplayedArea();
			m_RezoomNeeded = false;
		}

		public SpecialZooms SpecialZoom
		{ get { return m_SpecialZoom; } }

		public void CentreDisplayOn(PointF dataPoint)
		{
			// Triggered when the user clicks in the page list and we centre the display where they clicked
			if (m_Page == null)
				return;
			Debug.Assert(this.AutoScroll, "CentreDisplayOn on a view which does not scroll");

			// converts to screen coordinates, ignoring current scrolling
			// this is the part of the client area to have at the top left of the display, measured top down
			PointF screenPoint = new PointF(dataPoint.X * m_Zoom * m_PixelsPerDocumentX - Width / 2f, (m_Page.Size.Height - dataPoint.Y) * m_Zoom * m_PixelsPerDocumentY - Height / 2f);
			screenPoint = new PointF(Math.Max(Math.Min((int)screenPoint.X, m_ClientSize.Width - Width), 0),
				Math.Max(Math.Min((int)screenPoint.Y, m_ClientSize.Height - Height), 0));

			if (this.AutoScroll)
			{
				Point ptScroll = new Point((int)screenPoint.X, (int)screenPoint.Y);
				this.AutoScrollPosition = ptScroll;
			}
			NotifyDisplayedArea();
			InvalidateAll(); // because paint will just repaint the buffers onto screen
		}

		public void PageUpDown(int delta) // + for down
		{
			if (this.AutoScroll)
				return;
			Point scroll = this.AutoScrollPosition;
			scroll.Y = -(scroll.Y + this.Height / 2 * -delta); // needs to be written in opposite sense to read
			scroll.X = -scroll.X;
			this.AutoScrollPosition = scroll;
			NotifyDisplayedArea();
			InvalidateAll(); // because paint will just repaint the buffers onto screen
		}

		public void DoScroll(Size change)
		{
			Debug.Assert(this.AutoScroll, "DoScroll on a view which does not scroll");
			// size should be unit size, eg (-1,0)
			Point scroll = this.AutoScrollPosition;
			scroll.Y = -(scroll.Y + 8 * -change.Height);
			scroll.X = -(scroll.X + 8 * -change.Width);
			this.AutoScrollPosition = scroll;
			NotifyDisplayedArea();
			InvalidateAll(); // because paint will just repaint the buffers onto screen
		}

		protected void NotifyDisplayedArea()
		{
			if (m_Page == null)
				return;
			RectangleF displayed = ViewableArea();
			if (displayed.ApproxEqual(new RectangleF(new PointF(0, 0), m_Page.Size)))
				displayed = RectangleF.Empty; // this makes it easier for the outside world to know that the entire page is displayed
			DisplayedAreaChanged?.Invoke(displayed);
		}

		public float ApproxPixelsPerDocUnit
		{// This doesn't distinguish X and Y; only intended as approximate
			get { return m_PixelsPerDocumentX; }
		}

		/// <summary>Returns bounds of the shape in control local or screen coordinates.  Assumes that the shape is on the current page</summary>
		public Rectangle ShapeBounds(Shape shp, bool screen)
		{
			var rct = DataToClient(shp.Bounds).ToRectangle();
			if (screen)
				rct = this.RectangleToScreen(rct);
			return rct;
		}

		/// <summary>Returns rectangle of visible part of document as screen rectangle</summary>
		public Rectangle GetDocumentScreenCoords()
		{
			// first in view coords, intersect document itself and the view
			Rectangle document = DataToClient(m_Page.Bounds).ToRectangle();
			Rectangle control = new Rectangle(0, 0, Width, Height);
			Rectangle intersect = Rectangle.Intersect(document, control);
			Rectangle result = RectangleToScreen(intersect);
			Debug.WriteLine("GetDocumentScreenCoords=" + result);
			return result;
		}

		#endregion

		#region Buffers
		// in order to speed up the drawing we keep one or more bitmaps containing the picture pre-drawn
		// we are not just using the .net double buffering mechanism because it would be quite handy to keep separate buffers for
		// shapes behind the current one, just the current shape, any shapes in front of the current one so we can just update the current shape buffer (fast) and redraw
		protected Buffer m_BaseBuffer = new Buffer(false);
		// generally there was only one of Current and Selected used; but I think at some times it might be helpful to have both, so I have kept them as separate buffers
		protected List<Buffer> m_Buffers = new List<Buffer>(); //  the constructor inserts all of the above objects into this list
		protected class Buffer
		{
			public Bitmap m_bmp = null; // can be nothing when it is completely invalid (i.e. size changed)
			private Size m_Requested = new Size(0, 0);
			private Rectangle m_InvalidArea = Rectangle.Empty; // the part if any of this buffer which is out of date
			private bool m_TotalInvalid = false; // just flags if the entire image is invalid - in which case it won't bother setting a clipping region
			public bool StartTransparent; // if true the buffer is automatically cleared to transparent
										  // (not needed for the background buffer - which will always be filled by the main graphics functionality; however buffers for individual objects will want this)

			public Buffer(bool startTransparent)
			{
				StartTransparent = startTransparent;
			}

			public void Discard()
			{
				// mainly called when changing size etc.  Is also equivalent to Dispose method ie should be called before destroying the view
				m_bmp?.Dispose();
				m_bmp = null;
			}

			public Size Size
			{
				get { return m_Requested; }
				set
				{
					if (!value.Equals(m_Requested))
					{
						m_Requested = value;
						Discard();
					}
				}
			}

			public void Invalidate(Rectangle screenRect)
			{
				if (m_bmp == null)
					return;
				Geometry.Extend(ref m_InvalidArea, screenRect);
			}

			public void InvalidateAll()
			{
				m_TotalInvalid = true;
			}

			public Graphics PrepareDraw()
			{
				// ensures that the buffer is up to date ready to be drawn to screen
				// if redrawing is required then return is a graphics object with the correct clipping region set
				// if not, it returns nothing.  The caller must dispose of the graphics object
				if (m_bmp == null)
				{
					m_bmp = new Bitmap(m_Requested.Width, m_Requested.Height);
					m_TotalInvalid = true;
				}
				if (m_TotalInvalid == false && m_InvalidArea.IsEmpty)
					return null;

				Graphics gr = Graphics.FromImage(m_bmp);
				if (!m_TotalInvalid)
					gr.SetClip(m_InvalidArea);
				if (StartTransparent)
					gr.Clear(Color.Transparent);
				m_TotalInvalid = false;
				m_InvalidArea = Rectangle.Empty;
				return gr;
			}

			public RectangleF InvalidArea
			{
				get
				{
					if (m_TotalInvalid)
						return new RectangleF(0, 0, m_Requested.Width, m_Requested.Height);
					return m_InvalidArea;
				}
			}

			public Rectangle Bounds
			{ get { return new Rectangle(0, 0, m_bmp.Width, m_bmp.Height); } }

		}
		#endregion

		#region Shape/button hover
		/// <summary>shape currently being hovered over (or the one pressed with a button if mouse held down)</summary>
		protected ButtonShape m_ShapeHover = null;
		private ButtonShape m_ShapePressed = null;
		// sometimes only one of these is defined (if the user does not release, the pressed shape still remembered even if the  hover is cleared)
		// at the moment both must be ButtonShapes, but that could maybe change in the future

		/// <summary>returns true if active content, such as buttons, should do their stuff.  Overridden by PaletteView to ignore teacher mode, but to exclude when being edited</summary>
		protected virtual bool UseActiveContent
		{ get { return Globals.Root.User == Users.User; } }

		private void UpdateShapeHover(PointF pt)
		{
			if (!UseActiveContent || m_Page == null)
				return;
			Shape shapeHit = m_Page.HitTest(pt, m_Zoom, Page.HitTestMode.ActiveContent);
			if (!(shapeHit is ButtonShape))
				shapeHit = null;
			if (shapeHit != m_ShapeHover)
			{
				ClearShapeHover();
				if (m_ShapePressed != null && m_ShapePressed != shapeHit)
					return; // if a button is pressed, we don't show any hover feedback over other buttons
				m_ShapeHover = (ButtonShape)shapeHit;
				if (m_ShapeHover != null && m_ShapeHover.State == ButtonShape.States.Normal)
					m_ShapeHover.State = m_ShapePressed == shapeHit ? ButtonShape.States.Selected : ButtonShape.States.Highlight;
				HoverShapeChanged?.Invoke(m_ShapeHover);
			}
		}

		/// <summary>called on mouse exit</summary>
		private void ClearShapeHover()
		{
			if (m_ShapeHover != null) // clear old selection
			{
				if (m_ShapeHover.State == ButtonShape.States.Highlight || m_ShapeHover.State == ButtonShape.States.Selected && m_ShapeHover.SelectDuringUserPress)
					// Only deselect if it was responding to the presence of the mouse
					m_ShapeHover.State = ButtonShape.States.Normal;
				m_ShapeHover = null;
				HoverShapeChanged?.Invoke(m_ShapeHover);
			}
		}

		#endregion

		#region Stubs for complete classes

		public virtual Shape TypingShape(bool onlyIfFocused = true)
		{
			return null;
		}
		#endregion

	}
}