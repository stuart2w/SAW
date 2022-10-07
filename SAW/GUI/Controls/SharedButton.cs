using System;
using System.Drawing;
using System.Diagnostics;
using System.Windows.Forms;
using SAW.Functions;
using SAW.Shapes;

namespace SAW
{
	internal abstract class SharedButton : Control, ILinearAnimated, IInvokeable, IKeyControl
	{
		// shared between RoundButton and CustomButton.  New version which uses Button styles

		public event EventHandler RightClick;// Note that this will usually fire in addition to the standard action of the button (which happens first)

		protected bool m_Selected; // actual state - display may not have caught up yet
		protected bool m_Hover;
		/// <summary>255 is fully displayed selected </summary>
		protected int m_SelectionDisplayFraction = 0;
		/// <summary>whether this parameter is currently applicable(for RoundButton)</summary>
		protected bool m_Applicable = true;
		/// <summary>shortcut letter (or text) to be displayed top right.  This only affects the display</summary>
		public string ShortcutDisplay = "";

		/// <summary>Distance to move within the button ON EACH SIDE to get content such as key shortcuts inside the border</summary>
		protected const int INSET = 4; // This and ExcessSize could be a little smaller for Windows style backrooms, but doesn't look wrong

		/// <summary>Amount that the image is drawn smaller than the control (Total, not per side)</summary>
		/// <remarks>Sounds large, but for some backgrounds the border is about 4 each side</remarks>
		public const int EXCESSSIZE = 16;

		protected SharedButton()
		{
			if (!this.DesignMode)
			{
				// ReSharper disable VirtualMemberCallInConstructor
				this.DoubleBuffered = true;
				// on Mac transparency both is V slow, and causes glitches where backgrounds get drawn in front of foregrounds
				// BackColour overridden below to set this again if really needed
				SetStyle(ControlStyles.SupportsTransparentBackColor, true);
				this.BackColor = Color.Transparent;
				// ReSharper restore VirtualMemberCallInConstructor
			}
			this.GotFocus += SharedButton_GotFocus;
			this.KeyDown += SharedButton_KeyDown;
			this.LostFocus += SharedButton_LostFocus;
			this.MouseEnter += SharedButton_MouseEnter;
			this.MouseLeave += SharedButton_MouseLeave;
		}

		~SharedButton()
		{
			this.GotFocus -= SharedButton_GotFocus;
			this.KeyDown -= SharedButton_KeyDown;
			this.LostFocus -= SharedButton_LostFocus;
			this.MouseEnter -= SharedButton_MouseEnter;
			this.MouseLeave -= SharedButton_MouseLeave;
		}

		public void PerformClick()
		{
			this.Trigger();
		}

		public abstract void Trigger();

		protected void OnRightClick(EventArgs e)
		{
			RightClick?.Invoke(this, e);
		}

		#region Mouse

		protected virtual void SharedButton_MouseEnter(object sender, EventArgs e)
		{
			if (this.DesignMode || !m_Applicable || !Enabled)
				return;
			m_Hover = true;
			Invalidate();
			if (AutoPrompt != null) 
				Globals.SetHoverPrompt(AutoPrompt);
		}

		protected virtual void SharedButton_MouseLeave(object sender, EventArgs e)
		{
			if (this.DesignMode || !m_Applicable || !Enabled)
				return;
			m_Hover = false;
			Invalidate();
			if (AutoPrompt != null) 
				Globals.ClearHover();
		}

		#endregion

		#region Graphics
		// Various things can be displayed to indicate what is being selected...
		protected string m_ImageName = "";
		public string ImageName
		{
			get{return m_ImageName;}
			set
			{
				m_ImageName = value;
				Invalidate();
			}
		}

		// or an image can be provided directly.  Will dispose of image once released
		protected Image m_Image = null;
		public Image Image
		{
			get{return m_Image;}
			set
			{
				if (m_Image == value)
					return;
				m_Image?.Dispose();
				m_Image = value;
				Invalidate();
			}
		}

		protected bool m_NoUser; // if true then this has a faded background indicating buttons not available in user mode
		public virtual bool NoUser
		{
			get { return m_NoUser; }
			set
			{
				if (value != m_NoUser)
					Invalidate();
				m_NoUser = value;
			}
		}

		protected abstract string BackgroundImagePrefix { get; }

		protected void DrawInternalBackground(PaintEventArgs e, bool actionStyle)
		{
			// should be called as the first statement in OnPaint, or any overridden version of it
			ButtonShape.States state = ButtonShape.States.Normal;
			if (this.DesignMode && GUIUtilities.SystemDPI <= 1)
			{
				GUIUtilities.SystemDPI = 96;
				GUIUtilities.SystemDPIRelative = 1; // NetCanvas requires these, but they are usually set during frmMenu_Load
			}
			using (Shape.DrawResources resources = new Shape.DrawResources(1, 255, 255, false, false, new NetCanvas(e.Graphics), StaticView.InvalidationBuffer.Base, 1))
			{
				if (!Enabled)
					state = ButtonShape.States.Disabled;
				else if (m_SelectionDisplayFraction == 255 && m_Applicable && Enabled)
					state = ButtonShape.States.Selected;
				else if (m_Hover || Focused)
					state = ButtonShape.States.Highlight;
				else if (m_NoUser)
				{
					resources.FillAlpha = 90; // Need to fade quite a lot, because it is only the background which fades.  Even 100 wasnt visible
					resources.EdgeAlpha = 90;
				}
				ButtonStyle style;
				if (this.DesignMode)
					style = ButtonStyle.UserDefaultSelectionInstance;
				else
					style = Config.UserUser.ButtonStyle[actionStyle ? 0 : 1];
				style.PrepareResources(state, resources);
				RectangleF border = new RectangleF(0, 0, Width, Height);
				var styleState = state == ButtonShape.States.Disabled ? ButtonShape.States.Normal : state; // The index for the actual style object used (there isn't one for disabled)
																											  // For the default backgrounds (only) it is also necessary to scale the graphics to work in (approximately) mm
				if (style.ImageType == ButtonStyle.ImageTypes.None)
				{
					float scale = e.Graphics.DpiX / Geometry.INCH;
					border.Width = (border.Width - 1) / scale; // -1 because the graphics need the rectangle to be within the bounds for DrawRectangle
					border.Height = (border.Height - 1) / scale;
					// The standard border draws exactly ALONG the rectangle; we need to draw inside!
					border.Inflate(-style.LineStyle[(int)styleState].Width / 2 - 1, -style.LineStyle[(int)styleState].Width / 2 - 1);
					e.Graphics.ScaleTransform(scale, scale);
				}
				using (NetCanvas canvas = new NetCanvas(e.Graphics))
				{
					style.Draw(canvas, resources, border, state);
					if (m_SelectionDisplayFraction > 0 && m_SelectionDisplayFraction < 255 && m_Applicable && Enabled)
					{
						// fading in the selection colour
						resources.FillAlpha = m_SelectionDisplayFraction;
						resources.EdgeAlpha = m_SelectionDisplayFraction;
						style.Draw(canvas, resources, border, ButtonShape.States.Selected);
					}
				}

				e.Graphics.ResetTransform();
			}

		}
		/// <summary>Draws the image, faded if disabled.  If drawSize if specified it is drawn exactly that size, otherwise there is some tolerance on the default to match the available image</summary>
		protected void DrawStandardImage(Graphics gr, Image image, int drawSize = 0)
		{
			Debug.Assert(image != null); // should only be called if the image is defined; derived class should be dealing with image disposal, for example
			Debug.Assert(image.Width == image.Height);
			if (drawSize <= 0)
			{
				// Automatic size
				if (image.Width >= Width - EXCESSSIZE - 4 && image.Width < Width - EXCESSSIZE && GUIUtilities.SystemDPIRelative == 1)
				{
					// v 1.12: added last condition because this is not working on machines with modified scale (and not worth fussing around trying to correct it exactly)
					// Version 2, first condition; actual image size only used now if it is very close to what we would normally use
					drawSize = image.Width;
				}
				else
					drawSize = Width - EXCESSSIZE;
			}

			//  Unlike version 1 this is working in integers; might be a bit more steppy when animating
			Rectangle imageBounds = new Rectangle((Width - drawSize) / 2, 1 + (Height - drawSize) / 2, drawSize, drawSize);
			if (!Enabled || !m_Applicable)
				gr.DrawImage(image, imageBounds, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, GUIUtilities.DrawImageDisableNearGrey);
			else if (drawSize == image.Width && Enabled && image.HorizontalResolution == gr.DpiX)
				gr.DrawImageUnscaled(image, imageBounds); // Drawing at natural size.  This function should be faster, and I don't know whether FloorImage would call through to this automatically
			else
				gr.DrawImage(image, imageBounds);

		}
		protected void DrawBackgroundImage(PaintEventArgs e, int alpha, string suffix)
		{
			int width = Convert.ToInt32(Math.Ceiling(Width / 16f) * 16);
			using (Bitmap image = (Bitmap)GUIUtilities.VariableSizeImageRoundButton(BackgroundImagePrefix + suffix, "", width))
			{
				if (image == null)
				{
					if (!this.DesignMode)
						Utilities.LogSubError("RoundButton::DrawBackgroundImage failed to load image for suffix = " + suffix);
					return;
				}
				if (alpha == 255)
					e.Graphics.DrawImage(image, new Rectangle(0, 0, Width, Height), 0, 0, image.Width, image.Height, GraphicsUnit.Pixel);
				else
				{
					using (System.Drawing.Imaging.ImageAttributes objAttr = GUIUtilities.GetImageAttrForAlpha(alpha))
					{
						e.Graphics.DrawImage(image, new Rectangle(0, 0, Width, Height), objAttr);
					}

				}
			}

		}
		protected void DrawHighlightAndKey(PaintEventArgs e)
		{
			// should be called as the last statement in OnPaint, or any overridden version of it
			if (Focused)
				e.Graphics.DrawRectangle(GUIUtilities.FocusPen, new Rectangle(0, 0, Width - 1, Height - 1));

			if (string.IsNullOrEmpty(ShortcutDisplay))
				return;
			if (!Utilities.Low_Graphics_Safe())
				e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
			int X = Width <= 48 ? 0 : 1;
			Rectangle shortcut = new Rectangle(INSET, INSET - 1 + X, Width - INSET * 2 - X * 2, Height - INSET * 2 - 2 - X);
			SizeF textSize = e.Graphics.MeasureString(ShortcutDisplay, GUIUtilities.ShortcutFont, 1000, GUIUtilities.ShortcutFormat);
			Font font = GUIUtilities.ShortcutFont;
			if (textSize.Width > shortcut.Width)
				font = GUIUtilities.ShortcutFontVerySmall;
			else if (Width >= 60)
				font = GUIUtilities.ShortcutFontLarge;
			e.Graphics.DrawString(ShortcutDisplay, font, Brushes.Green, shortcut, GUIUtilities.ShortcutFormat);
		}

		#endregion

		#region Animation implementation
		private const int SELECTANIMATIONSTEP = 32;

		[System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
		public int VariableValue
		{
			get{return m_SelectionDisplayFraction;}
			set
			{
				m_SelectionDisplayFraction = value;
				if (m_Applicable && Enabled) 
					Invalidate();
			}
		}

		protected virtual void SetSelectionGUI(bool selected)
		{
			// sets the GUI into the selected (or not) state.  Does not update
			if (selected == m_Selected)
				return;
			AnimationController.EnsureNoAnimation(this);
			m_Selected = selected;
			if (Utilities.Low_Graphics_Safe() || !Visible)
			{
				m_SelectionDisplayFraction = m_Selected ? 255 : 0;
				Invalidate();
			}
			else
			{
				if (selected)
					AnimationLinear.CreateStart(this, 255, SELECTANIMATIONSTEP);
				else
					AnimationLinear.CreateStart(this, 0, -SELECTANIMATIONSTEP);
			}
		}
		#endregion

		#region Keys
		private void SharedButton_GotFocus(object sender, EventArgs e)
		{
			Invalidate();
			GUIUtilities.CurrentFocus = this;
		}

		private void SharedButton_LostFocus(object sender, EventArgs e)
		{
			Invalidate();
		}

		private void SharedButton_KeyDown(object sender, KeyEventArgs e)
		{ 		}

		protected override bool IsInputKey(Keys keyData)
		{
			switch (keyData)
			{
				case Keys.Left:
				case Keys.Right:
				case Keys.Up:
				case Keys.Down:
					return true;
			}
			return base.IsInputKey(keyData);
		}

		public void CombinedKeyDown(CombinedKeyEvent e)
		{
			switch (e.KeyCode)
			{
				case Keys.Enter: // Return has same value
					Trigger();
					e.Handled = true;
					if (this.Focused)
						Globals.Root.PerformAction(Verb.Find(Codes.RestoreFocus)); // Main set within palettes, when a selection is made the focus then restored to the typing area
					break;
				case Keys.Left:
				case Keys.Right:
				case Keys.Up:
				case Keys.Down:
					(Parent as ButtonPanel)?.CombinedKeyDown(e);
					break;
			}
		}

		public void CombinedKeyUp(CombinedKeyEvent e)
		{ }

		#endregion

		#region Properties
		[System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
		public bool Selected
		{
			get{return m_Selected;}
			set{SetSelectionGUI(value);}
		}

		public bool Applicable => m_Applicable;

		// If set, this will display the prompt automatically as the mouse hovers over this button
		internal Shape.Prompt AutoPrompt { get; set; }

		#endregion

	}

}
