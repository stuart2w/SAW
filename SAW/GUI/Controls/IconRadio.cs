using System.Collections.Generic;
using System;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;
using SAW.Shapes;

namespace SAW
{
	[System.ComponentModel.DefaultEvent("UserChecked")]
	internal sealed class IconRadio : Control, ILinearAnimated, IInvokeable
	{
		public event EventHandler CheckedChanged;
		public event EventHandler UserChecked; // called when user clicks control to select it (and when user deselect is if Toggle = true)

		private const int INSET = 6; // draw border this far within control, icon expands out when selected
		private bool m_Checked;
		private bool m_Hover; // mouse currently over the active part of control

		public IconRadio()
		{
			DoubleBuffered = true;
			this.SetStyle(ControlStyles.SupportsTransparentBackColor, true);
			this.MouseCaptureChanged += IconRadio_MouseCaptureChanged;
			this.MouseEnter += IconRadio_MouseEnter;
			this.MouseLeave += IconRadio_MouseLeave;
			this.MouseMove += IconRadio_MouseMove;
			this.MouseClick += IconRadio_MouseClick;
			this.LostFocus += IconRadio_LostFocus;
			this.Resize += IconRadio_Resize;
		}

		#region Basic properties
		private Image m_Image;
		public Image Image
		{
			get { return m_Image; }
			set
			{
				if (value != m_Image)
				{
					m_Image = value;
					Invalidate();
				}
			}
		}

		private Image m_LargeImage;
		public Image LargeImage
		{
			get { return m_LargeImage; }
			set
			{
				if (value != m_LargeImage)
				{
					m_LargeImage = value;
					Invalidate();
				}
			}
		}

		private string m_VariableImage = "";
		/// <summary>If defined the images will be replaced if this is resized</summary>
		[System.ComponentModel.DefaultValue("")]
		public string VariableSizeImageName
		{
			get { return m_VariableImage; }
			set
			{
				m_VariableImage = value;
				// this deliberately doesn't change the images - will only be changed within designer
			}
		}

		[System.ComponentModel.DefaultValue(false)]
		public bool Checked
		{
			get { return m_Checked; }
			set
			{
				if (m_Checked == value)
					return;
				SetSelectionGUI(value);
				CheckedChanged?.Invoke(this, EventArgs.Empty);
				if (m_Checked && !this.DesignMode && Parent != null)
				{
					// deselect any others
					foreach (IconRadio control in Parent.Controls.OfType<IconRadio>().Where(x => x != this && x.RadioGroup == RadioGroup))
					{
						control.Checked = false;
					}
				}
				Invalidate();
			}
		}

		[System.ComponentModel.DefaultValue("")]
		public string RadioGroup { get; set; } = "";

		[System.ComponentModel.DefaultValue(ButtonPanel.BlendDirection.Off)]
		private ButtonPanel.BlendDirection m_Blend = ButtonPanel.BlendDirection.Off;
		public ButtonPanel.BlendDirection Blend
		{
			get { return m_Blend; }
			set
			{
				if (m_Blend != value)
					Invalidate();
				m_Blend = value;
			}
		}

		private string m_ShortcutDisplay = ""; // shortcut letter (or text) to be displayed top right.  This only affects the display
		[System.ComponentModel.DefaultValue("")]
		public string ShortcutDisplay
		{
			get { return m_ShortcutDisplay; }
			set
			{
				//  If m_ShortcutDisplay = value Then Exit Property
				// we need to invalidate anyway, because changes to the KeyboardUser setting change whether the shortcuts should be displayed
				// which needs invalidation
				m_ShortcutDisplay = value;
				Invalidate();
			}
		}

		// If set, this will display the prompt automatically as the mouse hovers over this button
		internal Shape.Prompt AutoPrompt { get; set; }

		/// <summary>true if this button toggles back off if clicked when in the on state</summary>
		[field: System.ComponentModel.DefaultValue(false)]
		public bool Toggle { get; set; } = false;

		/// <summary>Whether this should change state gradually, expanding the image in steps.  Default is true</summary>
		[System.ComponentModel.DefaultValue(true)]
		private bool Animate { get; set; } = true;
		#endregion

		#region Mouse response

		private void IconRadio_MouseCaptureChanged(object sender, EventArgs e)
		{
			// changes with button up/down
			Invalidate();
		}

		private void IconRadio_MouseClick(object sender, MouseEventArgs e)
		{
			if (this.DesignMode)
				return;
			if (m_Hover) // hover checks user was over active area
			{
				if (!m_Checked)
				{
					Checked = true;
					UserChecked?.Invoke(this, EventArgs.Empty);
				}
				else if (Toggle)
				{
					Checked = false;
					UserChecked?.Invoke(this, EventArgs.Empty);
				}
			}
		}

		private void IconRadio_MouseLeave(object sender, EventArgs e)
		{
			if (this.DesignMode)
				return;
			SetHover(false);
			if (AutoPrompt != null)
				Globals.ClearHover();
		}

		private void IconRadio_MouseMove(object sender, MouseEventArgs e)
		{
			if (this.DesignMode)
				return;
			SetHover(e.X >= INSET && e.X < Width - INSET && e.Y >= INSET && e.Y <= Height - INSET);
		}

		private void IconRadio_MouseEnter(object sender, EventArgs e)
		{
			if (AutoPrompt != null)
				Globals.SetHoverPrompt(AutoPrompt);
		}

		private void SetHover(bool hover)
		{
			if (hover == m_Hover || this.DesignMode)
				return;
			m_Hover = hover;
			Invalidate();
		}

		private void IconRadio_LostFocus(object sender, EventArgs e)
		{
			Invalidate();
		}

		public void PerformClick()
		{
			IconRadio_MouseClick(this, new MouseEventArgs(MouseButtons.Left, 0, 0, 0, 0));
		}

		#endregion

		#region Graphics
		int m_SelectionDisplayPercentage; // unlike RoundButton this really is a percentage

		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);
			ButtonPanel.BlendPanelBackground(this, e.Graphics, m_Blend);
			Rectangle border = new Rectangle(INSET, INSET, Width - INSET * 2, Height - INSET * 2);
			if (Enabled)
			{
				if (m_SelectionDisplayPercentage == 0 && !m_Hover && !Focused)
				{
					using (SolidBrush br = new SolidBrush(Color.FromKnownColor(KnownColor.Control)))
					{
						e.Graphics.FillRectangle(br, border);
					}
				}
				else
				{
					// both hover and selection use a gradient brush
					Color light;
					Color dark;
					if (m_Hover || Focused) // choose the colour without any selection percentage, and then mix in the selection percentage
					{
						light = Color.White;
						dark = Color.FromArgb(210, 230, 255);
					}
					else
					{
						light = Color.White;
						dark = Color.FromKnownColor(KnownColor.Control);
					}
					if (m_SelectionDisplayPercentage > 0)
					{
						light = Color.FromArgb(255, 237, 217).MixWith(light, m_SelectionDisplayPercentage);
						dark = Color.FromArgb(246, 195, 92).MixWith(dark, m_SelectionDisplayPercentage);
					}
					// the gradient brush it doesn't reach right to the edges, so we need to fill the rectangle with the background colour first
					using (SolidBrush br = new SolidBrush(dark))
					{
						e.Graphics.FillRectangle(br, border);
					}

					using (Brush br = CreateGradientBrush(Width - INSET * 2, light, dark))
					{
						e.Graphics.FillRectangle(br, border);
					}

				}
			}
			using (Pen pnBorder = new Pen(Color.FromKnownColor(KnownColor.ControlDark)))
			{
				e.Graphics.DrawRectangle(pnBorder, border);
			}

			if (Focused)
			{
				border.Inflate(2, 2);
				e.Graphics.DrawRectangle(GUIUtilities.FocusPen, border);
				border.Inflate(-2, -2);
			}

			Image image;
			if (m_SelectionDisplayPercentage == 100)
			{
				if (m_LargeImage == null && !string.IsNullOrEmpty(m_VariableImage) && !this.DesignMode)
					m_LargeImage = GUIUtilities.VariableSizeImage(m_VariableImage, "", this.Width - 8);
				image = m_LargeImage ?? m_Image; // no point creating m_Image using variablesize - if large failed then so would small.  Condition only useful for when no variablesize name is given
			}
			else
			{
				if (m_Image == null && !string.IsNullOrEmpty(m_VariableImage) && !this.DesignMode)
					m_Image = GUIUtilities.VariableSizeImage(m_VariableImage, "", this.Width - 24);
				image = m_Image ?? m_LargeImage;
			}

			if (image != null)
			{
				border.Inflate(-2, -2); // this is now where the image goes when not selected
				if (m_SelectionDisplayPercentage == 100)
					border = new Rectangle(0, 0, Width - 1, Height - 1);
				else if (m_SelectionDisplayPercentage > 0)
				{
					// interpolate the size
					float inversePercentage = (100 - m_SelectionDisplayPercentage) / 100f; // it is mostly this we need in the line below, and this makes it more readable
					border = new RectangleF(border.X * inversePercentage, border.Y * inversePercentage, border.Width * inversePercentage + Width * m_SelectionDisplayPercentage / 100f, border.Height * inversePercentage + Height * m_SelectionDisplayPercentage / 100f).ToRectangle();
				}
				if (Enabled)
					e.Graphics.DrawImage(image, border, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel);
				else
					e.Graphics.DrawImage(image, border, GUIUtilities.DrawImageDisableNearGrey);
			}
			//if (!string.IsNullOrEmpty(ShortcutDisplay) && !Checked && Enabled && !this.DesignMode)
			//{
			//	if (!Utilities.Low_Graphics_Safe())
			//		e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
			//	SizeF textSize = e.Graphics.MeasureString(ShortcutDisplay, GUIUtilities.ShortcutFont, 1000, GUIUtilities.ShortcutFormat);
			//	Rectangle shortcut = new Rectangle(0, 0, Width - 1, (int)textSize.Height); 
			//	using (SolidBrush br2 = new SolidBrush(Color.FromArgb(180, Color.White)))
			//	{
			//		e.Graphics.FillRectangle(br2, shortcut);
			//	}
			//	e.Graphics.DrawString(ShortcutDisplay, GUIUtilities.ShortcutFont, Brushes.Green, shortcut, GUIUtilities.ShortcutFormat);
			//}
		}

		// There is no radial gradient brush.  The only way to emulate this is to make a path gradient brush, using a number of points
		// arranged in a circle.  We store the list of points to avoid can calculating them continuously
		private static PointF[] CreateRadialPoints(PointF centre, int radius)
		{
			List<PointF> list = new List<PointF>();
			for (int angle = 0; angle <= 350; angle += 10)
			{
				// Replace last iteration point with new empty point struct
				PointF circumference = new PointF(centre.X + radius * (float)Math.Cos(Geometry.Radians(angle)), centre.Y + radius * (float)Math.Sin(Geometry.Radians(angle)));
				list.Add(circumference);
			}
			return list.ToArray();
		}

		private static readonly Dictionary<int, PointF[]> g_RadialPoints = new Dictionary<int, PointF[]>(); // key is the size of the rectangle; value is the list of points needed for a gradient brush
		private static Brush CreateGradientBrush(int size, Color light, Color dark)
		{
			if (!g_RadialPoints.ContainsKey(size))
			{
				PointF[] points = CreateRadialPoints(new PointF(size / 3 + INSET, size / 3 + INSET), size / 2);
				g_RadialPoints.Add(size, points);
			}

			System.Drawing.Drawing2D.PathGradientBrush br = new System.Drawing.Drawing2D.PathGradientBrush(g_RadialPoints[size]);
			// Create new color blend to tell the PathGradientBrush what colors to use and where to put them
			System.Drawing.Drawing2D.ColorBlend gradientSpecifications = new System.Drawing.Drawing2D.ColorBlend(3);

			// Define positions of gradient colors
			gradientSpecifications.Positions = new float[] { 0, 1 };
			// Define gradient colors and their alpha values, adjust alpha of gradient colors to match intensity
			gradientSpecifications.Colors = new Color[] { dark, light };

			// Pass off color blend to PathGradientBrush to instruct it how to generate the gradient
			br.InterpolationColors = gradientSpecifications;
			return br;
		}

		[System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
		public int VariableValue
		{
			get { return m_SelectionDisplayPercentage; }
			set
			{
				m_SelectionDisplayPercentage = value;
				Invalidate();
			}
		}

		private const int SELECTANIMATIONSTEP = 20;
		private void SetSelectionGUI(bool isChecked)
		{
			// sets the GUI into the selected (or not) state.  Does not update
			if (this.DesignMode)
				return;
			if (isChecked == m_Checked)
				return;
			m_Checked = isChecked;
			if (!Animate || Utilities.Low_Graphics_Safe())
				m_SelectionDisplayPercentage = m_Checked ? 100 : 0;
			else
			{
				AnimationController.EnsureNoAnimation(this);
				if (isChecked)
					AnimationLinear.CreateStart(this, 100, SELECTANIMATIONSTEP, 0);
				else
					AnimationLinear.CreateStart(this, 0, -SELECTANIMATIONSTEP, 0);
			}
		}

		private void IconRadio_Resize(object sender, EventArgs e)
		{
			Invalidate();
			if (!string.IsNullOrEmpty(m_VariableImage) && !DesignMode)
			{
				// select the images again on next paint
				m_Image = null;
				m_LargeImage = null;
			}
		}
		#endregion

	}

}
