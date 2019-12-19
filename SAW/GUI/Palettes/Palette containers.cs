using System;
using System.Drawing;
using System.Diagnostics;
using System.Windows.Forms;
using System.Linq;
using System.Drawing.Imaging;
using SAW.Functions;

// Various palette containing controls.  Palettes are in 2 parts, the content control, and a container -and the same content might be moved between different containers

namespace SAW
{
	/// <summary>Must be implemented on all palette controls - the ones which are either docked in the Accordion controls or floating in frmPalettes
	/// ie each control implementing this is an entire, single, palette</summary>
	/// <remarks>the control should raise FocusChanged on Gotfocus/LostFocus - including within any sub controls
	/// (which don't trigger the containing controls normal GotFocus, LostFocus events)</remarks>
	public interface IPalette
	{
		void Initialise(); // called when the accordion is initialised; let some of the controls do some further set up which is guaranteed to be after the normal initialisation
		void SetToolTips(ToolTip tt); // control must add all tooltips to the given controller.  Usually called once for frmMain and once for the tear-off form
									  // Not guaranteed to be called

		/// <summary>Called when the palette is activated by a keyboard only user, using the keyboard.
		/// Should put the cursor over the first button, UNLESS the palette responds to the arrow keys internally (eg by navigating), in which case it can be ignored</summary>
		void BringPointerInsideOnActivation();

		/// <summary>given requested size, returns actual desired size</summary>
		Size SizeFromSize(Size requested);
		/// <summary>given the requested width (ie when docked), returns the correct size for this control
		/// width will probably be made at least as large as intRequested whatever is returned (so fixed controls will get dead space to right if accordion column too wide for them)</summary>
		Size SizeFromWidth(int requested);

		void SubscribeEvents(EventHandler AccessedHandler, NullEventHandler FocusChangedHandler);
	}

	/// <summary>must be implemented by the controls which can contain palette controls; i.e. Accordion and frmPalette</summary>
	public interface IPaletteContainer
	{
		void PaletteVerb(Codes code); // frmMain calls this to trigger some actions within a palette

	}

	/// <summary>Implements a heading with a single control underneath, which can be collapsed</summary>
	public sealed class Accordion : ContainerControl, ILinearAnimated, IAnimationNotifyComplete, IInvokeable, IComparable<Accordion>, IPaletteContainer, IKeyControl
	{
		// on the mac once collapsed the contained panel doesn't seem to be correctly clipped.  It's also slow, so best to just hide and not slide open/closed

		public event EventHandler Opening; // raised to container can collapse others if space is tight

		private bool m_Hover = false; // whether the mouse is currently over the title

		[System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
		public DateTime LastAccessed { get; set; } = DateTime.Now;

		public readonly Palette Palette;

		public Accordion(Palette objPalette)
		{
			Debug.Assert(objPalette != null);

			Palette = objPalette;
			Text = objPalette.Title;
			Open = objPalette.Position.Open;

			Controls.Add(objPalette.Control);
			objPalette.Control.Click += Control_Accessed;
			if (objPalette.Control is IPalette palette)
				palette.SubscribeEvents(Control_Accessed, null);
			this.Paint += Accordion_Paint;
			this.Click += Accordion_Click;
			this.ControlRemoved += Accordion_ControlRemoved;
			this.MouseDown += Accordion_MouseDown;
			this.MouseLeave += Accordion_MouseLeave;
			this.MouseMove += Accordion_MouseMove;
			this.MouseUp += Accordion_MouseUp;
			this.Resize += Accordion_Resize;
			// no need to do any sizing yet - InitialisePosition will always be called
		}

		#region Other properties
		// we need to know if these are supposed to be Visible (control itself returns false if form is not displayed yet)
		private bool m_VisibleRequested = true;

		public new bool Visible
		{
			get { return m_VisibleRequested; }
			set
			{
				base.Visible = value;
				m_VisibleRequested = value;
			}
		}

		public override string ToString() => "{" + Text + "}";

		#endregion

		#region Graphics
		internal static Font SharedFont = new Font(FontFamily.GenericSansSerif, 12, FontStyle.Regular);
		//Private Shared g_objTitleFormat As System.Drawing.StringFormat
		internal static Color OpenBorderColour = Color.FromArgb(121, 183, 231); // this is the colour of the border on the open images
																				// we continue a square border around the rest of the control matched to this

		protected override void OnPaintBackground(PaintEventArgs e)
		{
			e.Graphics.Clear(this.BackColor);
		}
		
		private void Accordion_Paint(object sender, PaintEventArgs e)
		{
			if (!this.DesignMode)
			{
				ImageAttributes attributes = GUIUtilities.DrawImageNormal;

				EnsureImages();
				Image[] images;
				if (m_Open)
					images = new[] { g_imgLeftOpen, g_imgMiddleOpen, g_imgRightOpen };
				else if (m_Hover)
					images = new[] { g_imgLeftHover, g_imgMiddleHover, g_imgRightHover };
				else
					images = new[] { g_imgLeft, g_imgMiddle, g_imgRight };
				DrawHeaderImages(images, this, e.Graphics, attributes);

				using (Pen pn = new Pen(OpenBorderColour, 1))
				{
					// if not fully open the bottom line is omitted by moving it one pixel too low - (cos it leaves artifacts behind during animation)
					e.Graphics.DrawRectangle(pn, 0, TITLEHEIGHT, Width - 1, Height - (Height >= OpenHeight - 4 ? 1 : 0) - TITLEHEIGHT);
				}
			}
			else
				e.Graphics.FillRectangle(Brushes.LightBlue, new Rectangle(0, 0, Width, TITLEHEIGHT));
			DrawPaletteHeaderContent(Palette, this, e.Graphics);
		}

		public static void DrawPaletteHeaderContent(Palette palette, Control ctr, Graphics gr)
		{
			// also used by frmPalette
			int X = 23;
			if (palette.Icon != null)
			{
				int Y = (TITLEHEIGHT - palette.Icon.Height) / 2;
				gr.DrawImage(palette.Icon, new Rectangle(X, Y, palette.Icon.Width, palette.Icon.Height));
				//DrawImageUnscaled is expanding the Lock symbol.  Some sort of DPI mismatch
				X += palette.Icon.Width + 2;
			}
			if (ctr.Width > 100) // Text is truncated, but if the control is very narrow we don't bother trying to draw any text (likely to be a bit ridiculous)
			{
				if (!Utilities.Low_Graphics_Safe())
					gr.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
				gr.DrawString(palette.Title, SharedFont, Brushes.Black, new Rectangle(X, 0, ctr.Width - X, TITLEHEIGHT), GUIUtilities.StringFormatCentreLeftNoWrap);
			}
		}

		public static void DrawHeaderImages(Image[] images, Control control, Graphics gr, ImageAttributes tint)
		{
			Debug.Assert(images[0].Height == TITLEHEIGHT);
			gr.DrawImage(images[0], new Rectangle(0, 0, images[0].Width, images[0].Height), 0, 0, images[0].Width, images[0].Height, GraphicsUnit.Pixel, tint);
			int X = images[0].Width;
			while (X < control.Width - images[2].Width)
			{
				// Was using texture brush, but this is isn't working when I apply the green tint (it just comes out a flat grey colour?)
				gr.DrawImage(images[1], new Rectangle(X, 0, images[1].Width, images[1].Height), 0, 0, images[1].Width, images[1].Height, GraphicsUnit.Pixel, tint);
				X += images[1].Width;
			}
			gr.DrawImage(images[2], new Rectangle(control.Width - images[2].Width, 0, images[2].Width, images[2].Height), 0, 0, images[2].Width, images[2].Height, GraphicsUnit.Pixel, tint);
		}

		private static Image g_imgLeft;
		private static Image g_imgRight;
		private static Image g_imgMiddle;
		private static Image g_imgLeftHover;
		private static Image g_imgRightHover;
		private static Image g_imgMiddleHover;
		private static Image g_imgLeftOpen;
		private static Image g_imgRightOpen;
		private static Image g_imgMiddleOpen;

		private static void EnsureImages()
		{
			if (g_imgLeft == null)
			{
				g_imgLeft = Resources.AM.ACCORDIONLEFTCLOSED;
				g_imgRight = Resources.AM.ACCORDIONRIGHTCLOSED;
				g_imgMiddle = Resources.AM.ACCORDIONMIDCLOSED;
				g_imgLeftHover = Resources.AM.ACCORDIONLEFTHOVER;
				g_imgRightHover = Resources.AM.ACCORDIONRIGHTHOVER;
				g_imgMiddleHover = Resources.AM.ACCORDIONMIDHOVER;
				g_imgLeftOpen = Resources.AM.ACCORDIONLEFTOPEN;
				g_imgRightOpen = Resources.AM.ACCORDIONRIGHTOPEN;
				g_imgMiddleOpen = Resources.AM.ACCORDIONMIDOPEN;
			}
		}
		#endregion

		#region Events, including mouse
		private bool m_IgnoreClick = false; // is set to true when right clicking
		private void Accordion_Click(object sender, EventArgs e)
		{
			if (this.DesignMode)
				return;
			if (m_IgnoreClick)
			{
				m_IgnoreClick = false;
				return;
			}
			AnimationController.EnsureNoAnimation(this);
			Open = !m_Open;
			LastAccessed = DateTime.Now;
			// not sure why this was used: it makes number keypad, for example, problematic (NumberGrid loses focus)
			//If Not AM.CurrentConfig.Separate_Cursor Then Me.Controls(0).Focus()
		}

		private void Accordion_MouseLeave(object sender, EventArgs e)
		{
			SetHover(false);
		}

		private Rectangle m_DragTrigger = Rectangle.Empty; // is defined after a click when the user might be initiating a drag.  Anything outside this rectangle starts dragging
		private void Accordion_MouseDown(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left) // Open AndAlso
			{
				// can only move these when open (? or should we allow it to be dragged out when closed?)
				Size dragSize = SystemInformation.DragSize;
				m_DragTrigger = new Rectangle(e.X - dragSize.Width / 2, e.Y - dragSize.Height / 2, dragSize.Width, dragSize.Height);
			}
			else if (e.Button == MouseButtons.Right && e.Y < TITLEHEIGHT)
			{
				((frmMain)Parent.Parent).ShowPaletteMenu(this, new Point(e.X - 20, TITLEHEIGHT), Palette);
				m_IgnoreClick = true;
			}
		}

		private void Accordion_MouseMove(object sender, MouseEventArgs e)
		{
			SetHover(e.Y < TITLEHEIGHT);
			if (!m_DragTrigger.IsEmpty && !m_DragTrigger.Contains(e.X, e.Y))
			{
				Size offset = new Size(this.Controls[0].Left - e.X, this.Controls[0].Top - e.Y);
				((frmMain)this.FindForm()).StartAccordionMove(this, offset);
				m_DragTrigger = Rectangle.Empty;
			}
		}

		private void Accordion_MouseUp(object sender, MouseEventArgs e)
		{
			m_DragTrigger = Rectangle.Empty;
		}

		private void SetHover(bool state)
		{
			if (m_Hover != state)
			{
				m_Hover = state;
				if (!m_Open)
					Invalidate(); // if it is open then it makes no difference to the display, so invalidating just creates flicker
			}
		}

		public void Trigger()
		{
			// Called when we are switching to this control due to something other than a mouse click
			AnimationController.EnsureNoAnimation(this);
			Open = true;
			LastAccessed = DateTime.Now;
			// focus now done by caller
		}

		public void PerformClick()
		{
			// Different from Trigger, because that only ever opens it (which is appropriate for a keypress)
			Accordion_Click(this, EventArgs.Empty);
			this.Controls[0].Focus();
		}


		#endregion

		#region Sub control and positioning
		private bool m_Open = true;
		public const int TITLEHEIGHT = 27;
		private const int FURLSTEP = 16; // when opening or closing the amount move in each iteration
		private const int MINHEIGHT = 32; // mainly for sanity
		private bool m_IgnoreResize = false;

		private void Accordion_Resize(object sender, EventArgs e)
		{
			// will be triggered by frmPalette which just resizes this
			if (this.Controls.Count == 0 || m_IgnoreResize)
				return;
			if (m_Open)
				this.Height = this.Controls[0].Height + TITLEHEIGHT + 2;
			this.Controls[0].Width = Width - 2;
			Invalidate(new Rectangle(0, 0, Width, TITLEHEIGHT));
		}

		/// <summary>called by AccordionContainer to change width.  Can't just resize as that may not respect either set X or Y due to IPalette.SizeFromSize</summary>
		/// <returns>Returns the requested control width (would usually be the same as the specified width; but many controls have minima)</returns>
		/// <remarks>this must use Width and use IPalette.SizeFromWidth</remarks>
		public int SetWidth(int width)
		{
			if (Controls[0] == null)
			{
				Width = width;
				return Width;
			}
			m_IgnoreResize = true;
			Size size = ((IPalette)Controls[0]).SizeFromWidth(width - 2);
			Controls[0].Size = size;
			// width is used whether the control wants it or not
			if (m_Open)
				this.Size = new Size(width, size.Height + 2 + TITLEHEIGHT);
			else
				this.Width = width;
			m_IgnoreResize = false;
			return size.Width + 2;
		}

		private int ControlHeight()
		{
			// kept in a separate function so that it handles if the control was empty
			return this.Controls.Count == 0 ? MINHEIGHT : this.Controls[0].Height;
		}

		[System.ComponentModel.DefaultValue(true)]
		public bool Open
		{
			get { return m_Open; }
			set
			{
				if (m_Open == value)
					return;
				m_Open = value;
				Invalidate();
				if (m_Open)
				{
					Opening?.Invoke(this, EventArgs.Empty);
					AnimationController.EnsureNoAnimation(this, true); // otherwise new one below ignored
					if (CanAnimate)
						AnimationLinear.CreateStart(this, ControlHeight() + 3, FURLSTEP);
					else
						Height = ControlHeight() + 3 + TITLEHEIGHT;
				}
				else
				{
					if (CanAnimate)
						AnimationLinear.CreateStart(this, 0, -FURLSTEP);
					else
						Height = TITLEHEIGHT;
				}
				Palette.Position.Open = m_Open;
			}
		}

		/// <summary>height when fully open </summary>
		public int OpenHeight => ControlHeight() + TITLEHEIGHT + 2;
		public int ClosedHeight => TITLEHEIGHT;

		public void InitialisePosition()
		{
			// called once after whenever container is initialised, ie can assume this is called after construction.
			// may be called again if containers are reorganised
			if (this.Controls.Count == 0)
				return;
			Debug.Assert(Width > 0);
			// width is assumed to be fixed
			if (Width > 2)
				Palette.Control.Width = Width - 2;
			//If TypeOf Palette.Control Is IPalette Then
			Palette.Control.Size = ((IPalette)Palette.Control).SizeFromWidth(Width);
			//.Height = Math.Max(CType(Palette.Control, IPalette).RequiredHeight, MINHEIGHT)
			Palette.Control.Location = new Point(1, TITLEHEIGHT + 1);
			((IPalette)Palette.Control).Initialise();
			//End If
			this.Height = Palette.Control.Height + TITLEHEIGHT + 2;
			m_Open = Palette.Position.Open;
			this.Height = Palette.Position.Open ? OpenHeight : ClosedHeight;
		}

		private void Control_Accessed(object sender, EventArgs e)
		{
			LastAccessed = DateTime.Now;
		}

		private bool CanAnimate => base.Visible && this.Parent != null;

		#endregion

		#region Animation
		// Variable value controls the amount height allocated to the sub control
		[System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
		public int VariableValue
		{
			get { return Height - TITLEHEIGHT; }
			set { Height = value + TITLEHEIGHT; }
		}

		public void Complete()
		{
			if (!m_Open)
				Invalidate();
			// the display of the header changes as it finishes collapsing (and as it starts opening - so that was invalidated before the animation started)
		}
		#endregion

		public int CompareTo(Accordion other)
		{
			// sorts them into correct display order
			return Palette.Position.DockIndex.CompareTo(other.Palette.Position.DockIndex);
		}

		public void PaletteVerb(Codes code)
		{
			switch (code)
			{
				case Codes.MovePalette:
					Cursor.Position = PointToScreen(new Point(Width / 2, TITLEHEIGHT / 2)); // puts the cursor in the middle of the title; it could be anywhere at the moment
					Size szOffset = new Size(this.Controls[0].Left - Width / 2, this.Controls[0].Top - TITLEHEIGHT / 2);
					((frmMain)this.FindForm()).StartAccordionMove(this, szOffset);
					Cursor.Current = Cursors.Hand;
					break;
				case Codes.ResizePalette: // Is ignored; palettes cannot be resized within an accordion
					break;
				default:
					Debug.Fail("Unexpected palette verb: " + code);
					break;
			}
		}

		public void CombinedKeyDown(CombinedKeyEvent e)
		{
			switch (e.KeyCode)
			{
				case Keys.Apps:
					((frmMain)Parent.Parent).ShowPaletteMenu(this, new Point(10, TITLEHEIGHT), Palette);
					e.Handled = true;
					break;
			}
		}

		public void CombinedKeyUp(CombinedKeyEvent e)
		{
		}

		private void Accordion_ControlRemoved(object sender, ControlEventArgs e)
		{
			//Debug.WriteLine("control removed");
		}
	}

	/// <summary>can be placed as the one control in an Accordion.  Used as the base class for most of the custom control palettes
	/// Requires that MinimumSize is set in designer</summary>
	public class PalettePanel : UserControl, IPalette
	{

		//public event EventHandler Accessed;
		public event NullEventHandler FocusChanged;
		public void SubscribeEvents(EventHandler AccessedHandler, NullEventHandler FocusChangedHandler)
		{
			if (FocusChangedHandler != null)
				FocusChanged += FocusChangedHandler;
		}

		public void Initialise()
		{
			foreach (Control objControl in this.Controls)
			{
				objControl.GotFocus += SubControl_FocusChanged;
				objControl.LostFocus += SubControl_FocusChanged;
			}
		}

		private void SubControl_FocusChanged(object sender, EventArgs e)
		{
			// it doesn't matter that this will raise a lot of spurious events; the accordion will filter them out to avoid excessive repainting
			FocusChanged?.Invoke();
		}

		protected override bool ProcessTabKey(bool forward)
		{
			GUIUtilities.IterateTabStop(this, forward);
			return true; // says that we have processed the key.  We will do any normal moving between controls type navigation
		}

		public virtual void SetToolTips(ToolTip tt)
		{
			// check for verb buttons
			if (tt == null)
				return;
			foreach (Control button in Controls)
			{
				if (button is Button)
				{
					if (button.Tag != null && button.Tag.ToString().StartsWith("Verb/"))
					{
						string verb = button.Tag.ToString().Substring(5);
						Codes code = (Codes)Enum.Parse(typeof(Codes), verb);
						if (code != Codes.None)
							tt.SetToolTip(button, Strings.Item("Verb_" + code).Replace("&", ""));
					}
				}
			}
		}

		public void BringPointerInsideOnActivation()
		{
			// ignored since we can tab
			// or could move over first control
		}

		#region Sizing
		// Requires that MinimumSize is set in designer
		public virtual Size SizeFromSize(Size requested)
			=> new Size(Math.Max(requested.Width, MinimumSize.Width), Math.Max(requested.Height, MinimumSize.Height));
		public virtual Size SizeFromWidth(int requested) => new Size(Math.Max(requested, MinimumSize.Width), MinimumSize.Height);

		public static int ChooseGridLayout(int buttons, ref Size size)
		{
			// chooses rows and columns (returned in size) and button szie to arrange intButtons within size pixels
			// return value is button size.  All buttons square, and size should INCLUDE margin
			Size availableSpace = size; // copy so we can update size
			if (buttons < 0 || size.Width <= 0 || size.Height <= 0)
				throw new ArgumentException();
			if (buttons == 0)
			{
				Debug.Fail("ChooseGridLayout - no buttons");
				size = new Size(1, 1);
				return 20;
			}
			float AR = (float)size.Width / size.Height;
			// trivial cases of 1 line or columns
			if (AR >= buttons)
			{
				size = new Size(buttons, 1);
				return availableSpace.Height;
			}
			else if (AR <= 1f / buttons)
			{
				size = new Size(1, buttons);
				return availableSpace.Width;
			}
			if (AR >= 1)
			{
				// choose rows.  Low is low rows - high AR
				int rowsLow = (int)Math.Floor(Math.Sqrt(buttons / AR)); // low option. try this and one higher
				int columnsLow = (int)Math.Ceiling((float)buttons / rowsLow);
				int rowsHigh = rowsLow + 1;
				int columnsHigh = (int)Math.Ceiling((float)buttons / rowsHigh);
				// choose which based on which AR is closest to request
				if (Math.Abs(rowsLow - columnsLow / AR) < Math.Abs(columnsHigh - rowsHigh * AR))
					size = new Size(columnsLow, rowsLow);
				else
					size = new Size(columnsHigh, rowsHigh);
				return Math.Min(availableSpace.Width / size.Width, availableSpace.Height / size.Height);
			}
			else
			{
				int columnsLow = (int)Math.Floor(Math.Sqrt(buttons * AR)); // low option. try this and one higher
				int rowsLow = (int)Math.Ceiling((float)buttons / columnsLow);
				int columnsHigh = columnsLow + 1;
				int rowsHigh = (int)Math.Ceiling((float)buttons / columnsHigh);
				// choose which based on which AR is closest to request
				if (Math.Abs(columnsLow - rowsLow * AR) < Math.Abs(rowsHigh - columnsHigh / AR))
					size = new Size(columnsLow, rowsLow);
				else
					size = new Size(columnsHigh, rowsHigh);
				return Math.Min(availableSpace.Width / size.Width, availableSpace.Height / size.Height);
			}
		}

		#endregion

	}

	/// <summary>Container for a single control representing a palette, when not docked</summary>
	/// <remarks></remarks>
	public sealed class frmPalette : Form, IPaletteContainer, IKeyControl
	{
		// IKeyControl only used when resizing

		private readonly Palette m_Palette;
		private IPalette m_Control; // Use Controls(0) to refer to as Control class
		private const int BORDER = 1; // not added to top - accordion.titleheight is added there
		private static readonly Size g_Offset = new Size(BORDER, Accordion.TITLEHEIGHT); // offset of the control within this form
		private readonly ToolTip ttMain;

		public frmPalette(Palette palette)
		{
			m_Palette = palette;
			this.ShowInTaskbar = false;
			this.ControlBox = false;
			this.MinimizeBox = false;
			this.MaximizeBox = false;
			this.Text = "";
			this.FormBorderStyle = FormBorderStyle.None;
			// This has no border; we will draw our own title, resembling the accordion one
			this.ClientSize = new Size(palette.Control.Size.Width + BORDER * 2, palette.Control.Size.Height + Accordion.TITLEHEIGHT + BORDER);
			this.KeyPreview = true;
			this.StartPosition = FormStartPosition.Manual;
			this.Location = new Point(300, 300); // just in case its NOT set anywhere
			this.AutoScroll = false;
			ttMain = new ToolTip() { AutoPopDelay = 10000, ReshowDelay = 100, InitialDelay = 500, ShowAlways = true };
			this.Disposed += frmPalette_Disposed;
			this.GotFocus += frmPalette_GotFocus;
			this.LostFocus += frmPalette_LostFocus;
			this.Paint += frmPalette_Paint;
			this.Resize += frmPalette_Resize;
			this.MouseDown += Form_MouseDown;
			this.MouseMove += Form_MouseMove;
			this.MouseUp += Form_MouseUp;
#if DEBUG
			this.ControlRemoved += frmPalette_ControlRemoved;
#endif
		}

		private void frmPalette_Disposed(object sender, EventArgs e)
		{
			ttMain.Dispose();
			m_Control = null;
			try
			{
				// had a problem where m_Control was set, but Controls did not contain it.  Only way I can see is if the form was disposed.
				// [still occurring] the same error can occur if the control itself was directly disposed.  That removes it from the collection, but the object here would be set still
				if (Controls.Count > 0)
				{
					// finally reproduced and it appeared to be on exit from the app -  control had been disposed.  So we can just ignore it here
					Controls[0].Dispose();
					Controls.RemoveAt(0);
				}
			}
			catch (ObjectDisposedException) // not sure if this is allowed during dispose
			{
			}
			Debug.Assert(Globals.Root.Closing || Palette.List.Values.All(x => x.Form != this)); // first needed since I think close app can dispose this form early
		}

		private void frmPalette_Paint(object sender, PaintEventArgs e)
		{
			// draw title bar and frame.  Somewhat copied from Accordion
			Color c = Color.FromArgb(unchecked((int)0xFFF6FCFC));
			if (this.ContainsFocus)
				c = Color.FromArgb(unchecked((int)0xFFE0FFE0)); // roughly above with DrawImageGreenTint colour matrix applied
			using (SolidBrush br = new SolidBrush(c))
			{
				e.Graphics.FillRectangle(br, new Rectangle(0, 0, Width, Accordion.TITLEHEIGHT));
			}

			Accordion.DrawPaletteHeaderContent(m_Palette, this, e.Graphics);
			// not using the header images as we can't get the round edge (no transparent background), and the open one is almost flat colour anyway
			//	Accordion.DrawHeaderImages(New Image() {My.Resources.AM.PALETTELEFT, My.Resources.AM.ACCORDIONMIDOPEN, My.Resources.AM.ACCORDIONRIGHTOPEN}, Me, e.Graphics, DrawImageNormal)

			if (Controls.Count == 0 && m_Palette.Preview != null)
			{
				// if we don't have the actual control, draw the preview instead
				e.Graphics.DrawImage(m_Palette.Preview, BORDER, Accordion.TITLEHEIGHT);
			}
			// frame is drawn over the top of all that, so that it does not get obscured:
			using (Pen pn = new Pen(Accordion.OpenBorderColour))
			{
				e.Graphics.DrawRectangle(pn, new Rectangle(0, 0, Width - 1, Height - 1));
				e.Graphics.DrawLine(pn, 0, Accordion.TITLEHEIGHT - 1, Width, Accordion.TITLEHEIGHT - 1);
			}

		}

		public Point ControlLocation
		{// The location of the control in screen coordinates.  It is more convenient for frmMain to update this then .Location when moving
			get { return Location + g_Offset; }
			set { Location = value - g_Offset; }
		}

		public Rectangle ControlBounds
		{
			get { return new Rectangle(Bounds.X + BORDER, Bounds.Y + Accordion.TITLEHEIGHT, Bounds.Width - BORDER * 2, Bounds.Height - Accordion.TITLEHEIGHT - BORDER); }
			set
			{
				Debug.Assert(!IsDisposed);
				value.Y -= Accordion.TITLEHEIGHT;
				value.X -= BORDER;
				if (m_Control != null)
				{
					value.Size = m_Control.SizeFromSize(value.Size);
					Controls[0].Size = value.Size;
				}
				// expand for my borders to set my size
				value.Height += Accordion.TITLEHEIGHT + BORDER;
				value.Width += BORDER * 2;
				value = GUIUtilities.CheckRectOnScreen(value);
				Bounds = value; // New Rectangle(value.X - BORDER, value.Y, value.Width + BORDER * 2, value.Height + BORDER)
			}
		}

		public void GainControl()
		{
			if (this.Controls.Count == 0)
			{
				m_Control = (IPalette)m_Palette.Control;
				m_Control.SubscribeEvents(m_Control_Accessed, m_Control_FocusChanged);
				Controls.Add(m_Palette.Control);
				m_Palette.Control.Location = new Point(BORDER, Accordion.TITLEHEIGHT);
				m_Palette.Control.Height = m_Control.SizeFromWidth(Width - 2).Height;
				UpdateSize();
				m_Control.SetToolTips(ttMain);
				Controls[0].MouseDown += InnerControl_MouseDown;
				Controls[0].MouseMove += InnerControl_MouseMove;
				Controls[0].MouseUp += InnerControl_MouseUp;
			}
		}

		/// <summary>Sets the size of the form based on the control</summary>
		/// <remarks>Mostly used by GainControl, but also called when this is dragging an image preview</remarks>
		public void UpdateSize()
		{
			m_IgnoreResize = true;
			Height = m_Palette.Control.Height + BORDER + Accordion.TITLEHEIGHT;
			Width = m_Palette.Control.Width + BORDER * 2;
			Invalidate(new Rectangle(0, 0, Width, Accordion.TITLEHEIGHT));
			m_IgnoreResize = false;
		}

		#region Initiating drag and resize
		private Rectangle m_DragTrigger = Rectangle.Empty; // is defined after a click when the user might be initiating a drag.  Anything outside this rectangle starts dragging
		private bool m_DragControlResize; // true is the (possible) dragging is in the bottom right of the control

		private void Form_MouseDown(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				// can only move these when open (? or should we allow it to be dragged out when closed?)
				if (e.Y < Accordion.TITLEHEIGHT)
				{
					Size szDragSize = SystemInformation.DragSize;
					m_DragTrigger = new Rectangle(e.X - szDragSize.Width / 2, e.Y - szDragSize.Height / 2, szDragSize.Width, szDragSize.Height);
					m_DragControlResize = false;
				}
			}
			else if (e.Button == MouseButtons.Right && e.Y < Accordion.TITLEHEIGHT)
			{
				// Right clicking in the title displays a popup menu
				Main.ShowPaletteMenu(this, new Point(e.X - 20, Accordion.TITLEHEIGHT), m_Palette);
			}
			Main.BringPaletteWindowToFront(this);
		}

		private void Form_MouseMove(object sender, MouseEventArgs e)
		{
			if (!m_DragTrigger.IsEmpty && !m_DragTrigger.Contains(e.X, e.Y) && !m_DragControlResize)
			{
				Size offset = new Size(-e.X, Accordion.TITLEHEIGHT - e.Y);
				Main.StartPaletteFormMove(m_Palette, offset);
				m_DragTrigger = Rectangle.Empty;
			}
			else if (IsResizing && Controls.Count > 0)
			{
				// Currently resizing
				Size sz = m_ResizeOriginal + e.Location.Subtract(m_StartResize);
				if (sz.Width < 10 || sz.Height < 10)
					return;
				sz = m_Control.SizeFromSize(sz);
				if (sz.Equals(Controls[0].Size))
					return;
				Rectangle rct = new Rectangle(this.Left, this.Top, sz.Width + BORDER * 2, sz.Height + BORDER + Accordion.TITLEHEIGHT);
				GUIUtilities.CheckRectOnScreen(rct);
				Controls[0].Size = sz;
				this.Bounds = rct;
				Invalidate(new Rectangle(0, 0, Width, Accordion.TITLEHEIGHT + BORDER));
			}
		}

		private void Form_MouseUp(object sender, MouseEventArgs e)
		{
			m_DragTrigger = Rectangle.Empty;
			if (IsResizing)
				EndResize(e.Button != MouseButtons.Right); // right button will cancel
		}

		private Point m_StartResize; // reszing when not empty
		private Size m_ResizeOriginal; // of control

		// The following are attached by GainControl
		private void InnerControl_MouseDown(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left && Controls.Count > 0)
			{
				// can only move these when open (? or should we allow it to be dragged out when closed?)
				if (e.X >= Controls[0].Width - 12 && e.Y >= Controls[0].Height - 12)
				{
					Size dragSize = SystemInformation.DragSize;
					m_DragTrigger = new Rectangle(e.X - dragSize.Width / 2, e.Y - dragSize.Height / 2, dragSize.Width, dragSize.Height);
					m_DragControlResize = true;
				}
			}
		}

		private void InnerControl_MouseMove(object sender, MouseEventArgs e)
		{
			if (Controls.Count == 0)
				return;
			Control control = Controls[0];
			if (!m_DragTrigger.IsEmpty && !m_DragTrigger.Contains(e.X, e.Y) && m_DragControlResize)
			{
				m_StartResize = PointToClient(control.PointToScreen(m_DragTrigger.Centre())); // converts from sub-control coords to my local coords (which later mouse events use)
				StartResize();
			}
			else if (e.X >= control.Width - 12 && e.Y >= control.Height - 12)
				control.Cursor = Cursors.SizeNWSE;
			else
			{
				// We need to check the existing cursor in case the control has reassigned it to something else internally
				if (control.Cursor == Cursors.SizeNWSE)
					control.Cursor = Cursors.Default;
			}
		}

		private void InnerControl_MouseUp(object sender, MouseEventArgs e)
		{
			m_DragTrigger = Rectangle.Empty;
		}

		public void PaletteVerb(Codes code)
		{
			// Both are much like the mouse options above, but there are slight differences
			if (Controls.Count == 0)
				return;
			switch (code)
			{
				case Codes.MovePalette:
					Cursor.Position = PointToScreen(new Point(Width / 2, Accordion.TITLEHEIGHT / 2)); // puts the cursor in the middle of the title; it could be anywhere at the moment
					Size offset = new Size(this.Controls[0].Left - Width / 2, this.Controls[0].Top - Accordion.TITLEHEIGHT / 2);
					Main.StartPaletteFormMove(m_Palette, offset);
					Cursor.Current = Cursors.Hand;
					break;
				case Codes.ResizePalette:
					m_StartResize = new Point(Width - 4, Height - 4);
					Cursor.Position = PointToScreen(m_StartResize);
					StartResize();
					break;
				default:
					Debug.Fail("Unexpected palette verb: " + code);
					break;
			}
		}

		/// <summary>status before resizing </summary>
		private bool m_ControlWasEnabled;
		private void StartResize()
		{
			Debug.Assert(!m_StartResize.IsEmpty); // Must be assigned before calling this
			Main.BringPaletteWindowToFront(this);
			m_ControlWasEnabled = Controls[0].Enabled;
			Controls[0].Enabled = false; // To stop the control using arrow keys to navigate around the palette
			this.Focus();
			GUIUtilities.CurrentFocus = this;
			m_ResizeOriginal = this.ControlBounds.Size;
			m_DragTrigger = Rectangle.Empty;
			Cursor.Current = Cursors.SizeNWSE;
			Capture = true;
		}

		private void EndResize(bool accept)
		{
			m_StartResize = Point.Empty;
			if (!accept) // put it back to the original size
			{
				Size sz = m_ResizeOriginal;
				Controls[0].Size = sz;
				this.Bounds = new Rectangle(this.Left, this.Top, sz.Width + BORDER * 2, sz.Height + BORDER + Accordion.TITLEHEIGHT);
			}
			Controls[0].Enabled = m_ControlWasEnabled;
			if (m_ControlWasEnabled)
				Controls[0].Focus();
			Cursor = Cursors.Default;
			Capture = false;
			m_DragTrigger = Rectangle.Empty;
		}

		public bool IsResizing => !m_StartResize.IsEmpty;

		/// <summary>True if any Resize events should be ignored (doesn't need to be set when IsResizing - deemed true in that case)</summary>
		private bool m_IgnoreResize;
		private void frmPalette_Resize(object sender, EventArgs e)
		{
			if (IsResizing || m_Control == null)
				return; // While resizing within this code any necessary updates are done above
			if (m_IgnoreResize)
				return;
			m_Palette.Control.Size = new Size(Width - BORDER * 2, Height - BORDER - Accordion.TITLEHEIGHT);
		}

#if DEBUG
		private void frmPalette_ControlRemoved(object sender, ControlEventArgs e)
		{
			if (e.Control.IsDisposed)
				Debug.Fail("Control disposed within frmPalette");
		}

#endif

		#endregion

		#region Windows stuff to stop it activating
		// copied from frmPad (don't want to inherit as we DONT want to always pass the focus back - the sub control can have the focus sometimes)
		private const int WM_MOUSEACTIVATE = 0x21;
		private const int MA_NOACTIVATE = 3;
		private const int WM_ACTIVATE = 0x6;
		[DebuggerStepThrough()]
		protected override void WndProc(ref Message m)
		{
			if (m.Msg == WM_MOUSEACTIVATE || m.Msg == WM_ACTIVATE)
				m.Result = (IntPtr)MA_NOACTIVATE;
			else
				base.WndProc(ref m);
		}

		#endregion

		private frmMain Main => (frmMain)Owner; // abstracted here as I'm experimenting between Owner and MdiParent

		#region Focus and access time
		private void m_Control_Accessed(object sender, EventArgs e)
		{
			Main.BringPaletteWindowToFront(this);
		}

		private void m_Control_FocusChanged()
		{
			this.Invalidate(new Rectangle(0, 0, Width, Accordion.TITLEHEIGHT));
		}

		private void frmPalette_GotFocus(object sender, EventArgs e)
		{
			// don't think we ever want to focus this directly?
			// without this closing a window over Splash puts the focus into the top palette
			if (!IsResizing)
				Globals.Root.CurrentMainScreen().Focus();
			this.Invalidate(new Rectangle(0, 0, Width, Accordion.TITLEHEIGHT));
		}

		private void frmPalette_LostFocus(object sender, EventArgs e)
		{
			this.Invalidate(new Rectangle(0, 0, Width, Accordion.TITLEHEIGHT));
		}
		#endregion

		#region Key handling
		protected override void OnKeyDown(KeyEventArgs e)
		{
			Main.KeyForm_KeyDown(this, e);
		}

		protected override void OnKeyPress(KeyPressEventArgs e)
		{
			Main.KeyForm_KeyPress(this, e);
		}

		protected override void OnKeyUp(KeyEventArgs e)
		{
			Main.KeyForm_KeyUp(this, e);
		}

		public void CombinedKeyDown(CombinedKeyEvent e)
		{
			if (IsResizing)
			{
				switch (e.KeyCode)
				{
					case Keys.Space:
						EndResize(true);
						e.Handled = true;
						break;
					case Keys.Escape:
						EndResize(false);
						e.Handled = true;
						break;
				}
			}
			else if (e.KeyCode == Keys.Apps)
			{
				// not available while resizing.  Could be outside if, but really seems a bad idea when resizing
				((frmMain)Owner).ShowPaletteMenu(this, new Point(10, Accordion.TITLEHEIGHT), m_Palette);
				e.Handled = true;
			}
		}

		public void CombinedKeyUp(CombinedKeyEvent e)
		{
		}

		#endregion

	}
}
