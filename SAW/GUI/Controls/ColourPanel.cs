using System.Collections.Generic;
using System;
using System.Drawing;
using System.Diagnostics;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.Linq;
using SAW.Functions;
using Action = SAW.Functions.Action;

namespace SAW
{
	[System.ComponentModel.DefaultEvent("UserSelectedColour")]
	internal sealed class ColourPanel : Control, ILinearAnimated, IAnimationNotifyComplete, IPalette, IInvokeable, IKeyControl
	{
		// The main part of the panel is a grid of colours.  Then there is a vertical dividing line and up to 3 special buttons down the right-hand side

		public event NullEventHandler UserSelectedColour;

		private const int INAPPLICABLEALPHADIVISION = 5; // Actual colour alpha is reduced by this (can't set an absolute value, because colours could be transparent anyway)
		private List<Color> m_Colours = new List<Color>();
		private bool m_Applicable = true; // if false this selection does not currently apply (e.g. selected shape is not filled and this is fill colour)
		private Color m_Current = Color.Black; // currently displayed colour.  Might not be one of the colours listed

		// other values returned from HitTest
		private const int BACKGROUND = -1;
		private const int MOREBUTTON = -2; // the buttons are only displayed if Advanced_Colours option in the configuration is on
		private const int TRANSPARENTBUTTON = -3;

		public ColourPanel()
		{
			this.DoubleBuffered = true; //False '
			this.Size = new Size(200, 86);
			this.MouseDown += ColourPanel_MouseDown;
			this.GotFocus += ColourPanel_GotFocus;
			this.LostFocus += ColourPanel_LostFocus;
			this.Paint += ColourPanel_Paint;
			this.Resize += ColourPanel_Resize;
		}

		~ColourPanel()
		{
			this.MouseDown -= ColourPanel_MouseDown;
			this.GotFocus -= ColourPanel_GotFocus;
			this.LostFocus -= ColourPanel_LostFocus;
			this.Paint -= ColourPanel_Paint;
			this.Resize -= ColourPanel_Resize;
		}

		public void Initialise()
		{
			SetLayout(Size);
		}

		private void ColourPanel_MouseDown(object sender, MouseEventArgs e)
		{
			if (this.DesignMode)
				return;
			if (!m_Applicable)
				return;
			GUIUtilities.FocusIfNotInPalette(this);
			HandleClick(HitTest(e.X, e.Y));
		}

		public void PerformClick()
		{
			Point pt = PointToClient(Cursor.Position);
			ColourPanel_MouseDown(this, new MouseEventArgs(MouseButtons.Left, 1, pt.X, pt.Y, 0));
		}

		private void HandleClick(int index)
		{
			Color result;
			switch (index)
			{
				case MOREBUTTON:
					if (Globals.Root.CurrentConfig.ReadBoolean(Config.Windows_Colours))
					{
						using (ColorDialog dlgcolour = new ColorDialog())
						{
							dlgcolour.AnyColor = true;
							dlgcolour.Color = CurrentColour;
							if (dlgcolour.ShowDialog() != DialogResult.OK)
								return;
							result = dlgcolour.Color;
						}

					}
					else
					{
						if (m_Current.IsEmpty)
							return;
						result = frmMoreColours.Display(m_Current, GetAlternateColours(m_Current), false, this.FindForm());
					}
					if (!result.IsEmpty)
					{
						CurrentColour = result;
						UserSelectedColour?.Invoke();
					}
					break;
				case TRANSPARENTBUTTON:
					if (!AllowTransparent)
						return;
					result = frmMoreColours.Display(m_Current, GetTransparentColours(m_Current), true, FindForm());
					if (!result.IsEmpty)
					{
						CurrentColour = result;
						UserSelectedColour?.Invoke();
					}
					break;
				default:
					if (index < 0)
						return;
					else
					{
						CurrentColour = m_Colours[index];
						UserSelectedColour?.Invoke();
					}
					Accessed?.Invoke(this, EventArgs.Empty);
					break;
			}
		}

		[System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
		internal Color SelectedColour
		{
			get { return m_Current; }
			set { SetSelectionGUI(value); }
		}

		#region Graphics
		private Bitmap m_bmpMore = null; // the image to use for the more button; created as the colour changes; not displayed if nothing

		private void ColourPanel_Paint(object sender, PaintEventArgs e)
		{
			if (this.DesignMode && m_Colours.Count == 0)
				SetStandard18Colours();

			//ButtonPanel.BlendPanelBackground(Me, e.Graphics, m_eBlend)

			if (m_BackgroundHatching)
			{
				using (HatchBrush br = new HatchBrush(HatchStyle.LargeGrid, Color.LightGray, BackColor))
				{
					int Y = m_Colours.Count / m_Columns * m_SizeAndSpacing + m_Spacing;
					e.Graphics.FillRectangle(br, new Rectangle(0, 0, m_Columns * m_SizeAndSpacing + m_Spacing, Y));
				}
			}

			// draw the selection borders first...
			if (m_OldSelection >= 0 && m_SelectionBalance < 100)
				PaintSelectionBorder(e.Graphics, m_OldSelection, 100 - m_SelectionBalance);
			if (SelectionIndex >= 0 && m_SelectionBalance > 0)
				PaintSelectionBorder(e.Graphics, SelectionIndex, m_SelectionBalance);

			Pen border = new Pen(Color.DarkGray);
			Rectangle colour;
			for (int index = 0; index <= m_Colours.Count - 1; index++)
			{
				colour = ColourRectangle(index);
				if (m_Colours[index].IsEmpty)
				{
					// leave it at the background colour of the control, and we add a red x in the middle...
					Image image = m_BlackWhite ? Resources.AM.BlackWhiteText : GUIUtilities.VariableSizeImage("Transparent", "", colour.Width); // My.Resources.AM.RedCross)
					if (image != null && m_Applicable)
					{
						e.Graphics.DrawImage(image, colour);
						image.Dispose();
					}
				}
				else
				{
					// if not applicable, colour is drawn but at low alpha
					using (SolidBrush br = new SolidBrush(Color.FromArgb(m_Colours[index].A / (m_Applicable ? 1 : INAPPLICABLEALPHADIVISION), m_Colours[index])))
					{
						e.Graphics.FillRectangle(br, colour);
					}
					e.Graphics.DrawRectangle(border, colour);
				}
			}

			if (DisplayAdvanced) // must use the public property, not the private variable to check the configuration
			{
				// The vertical dividing line
				e.Graphics.DrawLine(border, m_Columns * m_SizeAndSpacing + m_Spacing, 2, m_Columns * m_SizeAndSpacing + m_Spacing, Height - 2);
				// the special buttons...
				if (m_bmpMore != null)
				{
					colour = ColourRectangle(MOREBUTTON);
					e.Graphics.DrawImage(m_bmpMore, colour, m_Applicable ? null : GUIUtilities.DrawImage40Percent);
				}
				if (AllowTransparent)
				{
					colour = ColourRectangle(TRANSPARENTBUTTON);
					// this only starts from alpha=200, because even that looks pretty solid.  Starting at solid tends to make the whole button look too solid
					// uses max alpha = 50 when not applicable
					using (LinearGradientBrush transparent = new LinearGradientBrush(colour, Color.FromArgb(m_Applicable ? 200 : 50, m_Current), Color.FromArgb(0, m_Current), LinearGradientMode.ForwardDiagonal))
					{
						e.Graphics.FillRectangle(transparent, colour);
					}

					e.Graphics.DrawRectangle(border, colour);
				}
			}
			border.Dispose();
		}

		private void PaintSelectionBorder(Graphics gr, int index, int balance)
		{
			// balance is the percentage solidity to draw (see animation section)
			if (!m_Applicable)
				return;
			Color colour = Color.Orange.MixWith(BackColor, balance);
			Rectangle outside = ColourRectangleIncludingSelection(index);
			Debug.Assert(outside.Width == outside.Height);
			using (Pen pn = new Pen(colour, 2))
			{
				using (GraphicsPath path = GUIUtilities.CreateRoundedRectPath(outside.X, outside.Y, outside.Width, 3, 3))
				{
					gr.DrawPath(pn, path);
				}
			}

		}

		private void GenerateMoreBitmap()
		{
			Invalidate(ColourRectangle(MOREBUTTON));
			m_bmpMore?.Dispose();
			m_bmpMore = GenerateMoreColoursBitmap(GetAlternateColours(m_Current));
		}

		public static Bitmap GenerateMoreColoursBitmap(List<Color> colours)
		{
			Bitmap bmp = new Bitmap(20, 20);
			using (Graphics gr = Graphics.FromImage(bmp))
			{
				gr.Clear(Color.White);
				const int SIZE = 5; // size of each rectangle
				int step = 2; // thru the list - if doing full list of alternates only show every other to get better representation of complete set
				if (colours.Count < 32)
					step = 1;
				for (int index = 0; index <= Math.Min(15, colours.Count - 1); index++) // just display the first 16 in a 4x4 grid
				{
					Rectangle colour = new Rectangle(index % 4 * SIZE, index / 4 * SIZE, SIZE, SIZE);
					using (SolidBrush br = new SolidBrush(colours[index * step]))
					{
						gr.FillRectangle(br, colour);
					}

				}
			}

			return bmp;
		}

		#endregion

		#region Settings

		[System.ComponentModel.DefaultValue(true)]
		public bool AllowTransparent { get; set; } = true;

		private bool m_DisplayAdvanced = true;
		// note that this is ignored if UseSettings on and AM.CurrentConfig.ReadBoolean(Config.Advanced_Colours) = False
		// Although the private boolean remembers the request, the public property return false if they are configured offAll
		public bool DisplayAdvanced
		{
			get
			{
				if (m_UseSettings && Globals.Root != null && Globals.Root.CurrentConfig != null && Globals.Root.CurrentConfig.ReadBoolean(Config.Advanced_Colours, true) == false)
					return false;
				return m_DisplayAdvanced;
			}
			set
			{
				m_DisplayAdvanced = value;
				Invalidate();
			}
		}

		private bool m_UseSettings = true;
		public bool UseSettings
		{
			get { return m_UseSettings; }
			set
			{
				m_UseSettings = value;
				if (m_DisplayAdvanced)
					Invalidate();
			}
		}

		private bool m_BackgroundHatching = false;
		// if true a background hatching is displayed so that we can see transparent colours
		[System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
		public bool BackgroundHatching
		{
			[DebuggerStepThrough]
			get { return m_BackgroundHatching; }
			set
			{
				if (m_BackgroundHatching == value)
					return;
				m_BackgroundHatching = value;
				Invalidate();
			}
		}

		/// <summary>If true an empty colour is displayed as a black-and-white box</summary>
		private bool m_BlackWhite = false;
		/// <summary>If true an empty colour is displayed as a black-and-white box</summary>
		[System.ComponentModel.DefaultValue(false)]
		public bool BlackWhite
		{
			[DebuggerStepThrough()]
			get { return m_BlackWhite; }
			set
			{
				m_BlackWhite = value;
				int index = IndexOf(Color.Empty);
				if (index >= 0)
					Invalidate(ColourRectangle(index));
			}
		}

		[System.ComponentModel.DefaultValue(ButtonPanel.BlendDirection.Off)]
		private ButtonPanel.BlendDirection m_eBlend = ButtonPanel.BlendDirection.Off;
		public ButtonPanel.BlendDirection Blend
		{
			get { return m_eBlend; }
			set
			{
				if (m_eBlend != value)
					Invalidate();
				m_eBlend = value;
			}
		}

		#endregion

		#region Coordinates
		private int m_Columns = 6;
		private int m_Rows = 3; // if very wide not all of these are necessarily used if DisplayAdvanced
		private int m_ColourSize = 20;
		private int m_Spacing = 7; // the full amount is used at the far left and right (to allow for a selection border outside the selected colour)
								   // double this is allowed at the divider; i.e. this amount to the left and right of the divider line
		private int m_SizeAndSpacing = 27; //m_ColourSize + m_Spacing

		private void SetLayout(Size sz)
		{
			sz.Width = Math.Max(sz.Width, 20); // make both directions enough for a button
			sz.Height = Math.Max(sz.Height, 20);
			int area = sz.Width * sz.Height;
			int minimum = m_Colours.Count * 225; // min about 15*15 per button, including border
			if (area < minimum)
			{
				sz.Width *= (int)Math.Sqrt((float)minimum / area);
				sz.Height *= (int)Math.Sqrt((float)minimum / area);
			}
			Size layout = sz;
			int buttonSize = PalettePanel.ChooseGridLayout(m_Colours.Count, ref layout); // szLayout is updated with cols, rows
			if (DisplayAdvanced)
			{
				// Is the layout is very tall then just adding a column isn't really correct (it makes it much taller
				// if the grid generated only one mode of probably best to go to 2, changing the columns
				if (layout.Height == 1)
					layout.Width = (int)Math.Ceiling(layout.Width / 2f);
				if (layout.Height < 3)
					layout.Height = 3; // assumed extra row give enough slots to hold 3 buttons
				buttonSize = Math.Min(sz.Width / (layout.Width + 1), sz.Height / layout.Height);
			}
			// buttonSize is only first pass - assumes 1 margin per button.  We want full margin at all edges, and also extra for divider
			// base m_Spacing on this, and make button size use remaining available (not other way round since second more prone to rounding errors and spacing is smaller to start with)
			SetSpacingFromApproxTotalSize(buttonSize);
			m_ColourSize = Math.Min((sz.Width - m_Spacing * (layout.Width + 1 + (DisplayAdvanced ? 2 : 0))) / layout.Width, (sz.Height - m_Spacing * (layout.Height + 1)) / layout.Height);
			m_SizeAndSpacing = m_ColourSize + m_Spacing;
			m_Columns = layout.Width;
			m_Rows = layout.Height;
		}

		private void SetSpacingFromApproxTotalSize(int buttonSize)
		{
			// Sets m_Spacing after calculating roughly the width available to each button (including spacing).  proportion given to the spacing vary somewhat depending on size
			if (buttonSize >= 20 && buttonSize < 50)
				m_Spacing = buttonSize / 3;
			else
			{
				// Less spacing used if buttons very small (just need to concentrate on making the buttons visible)
				// or if buttons very big, in which case spacing can look bit too big
				m_Spacing = buttonSize / 4;
			}
		}

		// assuming(ish) that these are always applied once they are requested
		public Size SizeFromSize(Size requested)
		{
			SetLayout(requested);
			return new Size(m_Columns * m_SizeAndSpacing + m_Spacing + (DisplayAdvanced ? m_SizeAndSpacing + m_Spacing : 0), m_Rows * m_SizeAndSpacing + m_Spacing);
		}

		public Size SizeFromWidth(int requested)
		{
			// Always tries to achieve 6 columns
			int each = requested / (DisplayAdvanced ? 7 : 6);
			SetSpacingFromApproxTotalSize(each);
			m_ColourSize = (requested - m_Spacing * (DisplayAdvanced ? 9 : 7)) / (DisplayAdvanced ? 7 : 6);
			m_SizeAndSpacing = m_ColourSize + m_Spacing;
			int rows = m_Colours.Count / 6;
			return new Size(requested, rows * m_SizeAndSpacing + m_Spacing);
		}

		private Rectangle ColourRectangle(int index)
		{
			int row = index / m_Columns;
			if (index <= -2)
			{
				// one of the extra buttons down the right-hand side
				row = -2 - index;
				return new Rectangle(m_Spacing * 2 + m_SizeAndSpacing * m_Columns, m_Spacing + m_SizeAndSpacing * row, m_ColourSize, m_ColourSize);
			}
			int column = index % m_Columns;
			return new Rectangle(m_Spacing + m_SizeAndSpacing * column, m_Spacing + m_SizeAndSpacing * row, m_ColourSize, m_ColourSize);
		}

		private Rectangle ColourRectangleIncludingSelection(int index)
		{
			Rectangle total = ColourRectangle(index);
			int extra = Math.Min(m_Spacing / 2 + 4, m_Spacing);
			// roughly halfway between the colours
			// (Was previously just m_Spacing, but this doesn't work so well now that the palettes can be resized)
			total.Inflate(extra, extra);
			return total;
		}

		private int HitTest(int X, int Y)
		{
			// returns the index of the entry at the given coordinates.  Coordinates between entries return -1
			if (X < m_Spacing || Y < m_Spacing)
				return BACKGROUND;
			X -= m_Spacing;
			Y -= m_Spacing;
			int column = X / m_SizeAndSpacing;
			int row = Y / m_SizeAndSpacing;
			X -= column * m_SizeAndSpacing;
			Y -= row * m_SizeAndSpacing;
			if (column >= m_Columns)
				X -= m_Spacing; // the extra spacing where the divider line is
			if (X < 0)
				return BACKGROUND;
			if (X > m_ColourSize || Y > m_ColourSize)
				return BACKGROUND; // it is in the gap beneath or to the right of the one we have selected
			if (column == m_Columns)
			{
				if (!DisplayAdvanced)
					return BACKGROUND;
				return -2 - row; // one of the special buttons (this is not check if the button is allowed)
			}
			if (column >= m_Columns)
				return BACKGROUND; // completely off the right
			int index = column + row * m_Columns;
			if (index >= m_Colours.Count)
				return BACKGROUND;
			return index;
		}

		private void ColourPanel_Resize(object sender, EventArgs e)
		{
			if (m_Colours.Count <= 0)
				return;
			SetLayout(Size);
			Invalidate();
		}

		#endregion

		#region Colour list
		// see also Animation implementation which does indices
		public void SetStandard18Colours()
		{
			m_Columns = 6;
			m_Colours = GetStandard17Colours();
			m_Colours.Add(this.DesignMode ? Color.FromArgb(0, 0, 0) : Color.Empty);
		}

		public static List<Color> GetStandard17Colours()
		{
			// gets the standard 18 colours excluding the empty colour
			return new List<Color>
			{
				Color.FromArgb(0, 0, 0),
				Color.FromArgb(255, 255, 255),
				Color.FromArgb(128, 128, 128),
				Color.FromArgb(255, 0, 0),
				Color.FromArgb(255, 180, 90),
				Color.FromArgb(255, 255, 0),
				Color.FromArgb(0, 255, 255),
				Color.FromArgb(0, 0, 255),
				Color.FromArgb(111, 111, 255),
				Color.FromArgb(0, 0, 119),
				Color.FromArgb(255, 0, 255),
				Color.FromArgb(250, 125, 125),
				Color.FromArgb(191, 0, 0),
				Color.FromArgb(0, 255, 0),
				Color.FromArgb(79, 170, 79),
				Color.FromArgb(0, 108, 0),
				Color.FromArgb(185, 122, 87)
			};
		}

		private int IndexOf(Color colour, bool ignoreTransparent = false)
		{
			// if ignoreTransparent then any transparency in colour can be ignored
			// this means that the highlight is still shown in the main colour panel is even when there is some transparency
			// however transparency in the panel colour must be counted, otherwise in the transparency panel every colour would match
			for (int index = 0; index <= m_Colours.Count - 1; index++)
			{
				// If m_Colours[index].Equals(colour) Then Return index << cannot use this since we get empty not equalling empty
				// (at least if the empty has been converted to ARGB and back again
				if (m_Colours[index].ToArgb() == colour.ToArgb())
					return index;
				if (ignoreTransparent && m_Colours[index].A == 255 && (m_Colours[index].ToArgb() & 0xFFFFFF) == (colour.ToArgb() & 0xFFFFFF) && colour.A > 0)
					return index;
				// final condition stops transparent (0,0,0,0) matching black (255,0,0,0)
			}
			return -1;
		}

		public void DisplayColours(List<Color> colours, int columns)
		{
			m_Colours = colours;
			m_Columns = columns;
			Invalidate();
		}

		// Changing CurrentColour triggers parameter changed event and updates the GUI.  To just change the GUI use SetSelectionGUI
		[System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
		public Color CurrentColour
		{
			get { return m_Current; }
			set
			{
				if (m_Parameter != Parameters.None)
				{
					m_Current = value;
					Globals.SetParameterValue(value.ToArgb(), m_Parameter);
					// selection display will be updated in response to the parameter changed event.  Don't want to do it explicitly here,
					// because triggering the change twice will skip the animation
				}
				else
					SetSelectionGUI(value);
			}
		}

		public int SelectionIndex { get; private set; } = -1;// can be -1 if no colour in this list is selected

		#endregion

		#region Animation implementation
		private int m_OldSelection = -1;

		private int m_SelectionBalance = 100; // the amount by which the old and current selections are displayed.  Runs from 0 (old displayed)
											  // to 100 (new displayed)

		private void SetSelectionGUI(Color selection)
		{
			m_Current = selection;
			AnimateTo(IndexOf(selection, true));
			GenerateMoreBitmap();
			Invalidate(ColourRectangle(TRANSPARENTBUTTON));
		}

		private void SetSelectionGUI(int index)
		{
			if (index >= 0)
				m_Current = m_Colours[index];
			AnimationController.EnsureNoAnimation(this);
			GenerateMoreBitmap();
			Invalidate(ColourRectangle(TRANSPARENTBUTTON));
		}

		private void AnimateTo(int index)
		{
			if (SelectionIndex == index)
				return;
			// if we were still partway through fading out something else, then it needs to be refreshed without any highlight
			if (m_OldSelection >= 0)
				Invalidate(ColourRectangleIncludingSelection(m_OldSelection));
			m_OldSelection = SelectionIndex;
			SelectionIndex = index;
			m_SelectionBalance = 0;
			AnimationLinear.CreateStart(this, 100, 10);
		}

		public void Complete()
		{
			m_OldSelection = -1; // we don't need to remember the old selection once we have completed the transition of the display
		}

		public int VariableValue
		{
			get { return m_SelectionBalance; }
			set
			{
				m_SelectionBalance = value;
				if (m_OldSelection >= 0)
					Invalidate(ColourRectangleIncludingSelection(m_OldSelection));
				if (SelectionIndex >= 0)
					Invalidate(ColourRectangleIncludingSelection(SelectionIndex));
			}
		}

		#endregion

		#region Parameter

		private Parameters m_Parameter = Parameters.None;

		public void AttachParameter(Parameters parameter)
		{
			Debug.Assert(parameter != Parameters.None);
			DetachParameter();
			m_Parameter = parameter;
			Globals.ParameterChanged += ParameterChanged;
			Globals.ApplicabilityChanged += ApplicabilityChanged;
			Globals.SettingsChanged += SettingsChanged;
			SetSelectionGUI(Color.FromArgb(Globals.ParameterValue(m_Parameter)));
		}

		public void DetachParameter()
		{
			if (m_Parameter != Parameters.None)
			{
				Globals.ParameterChanged -= ParameterChanged;
				Globals.ApplicabilityChanged -= ApplicabilityChanged;
			}
			m_Parameter = Parameters.None;
		}

		private void ParameterChanged(Parameters parameter)
		{
			if (parameter == m_Parameter)
				SetSelectionGUI(Color.FromArgb(Globals.ParameterValue(m_Parameter)));
		}

		private void ApplicabilityChanged()
		{
			if (m_Applicable != Globals.ParameterApplicable(m_Parameter))
			{
				m_Applicable = !m_Applicable;
				Invalidate();
			}
		}

		private void SettingsChanged()
		{
			Invalidate(); // because the advanced buttons might need to be added or removed
		}

		#endregion

		#region Accordion

		private void ColourPanel_GotFocus(object sender, EventArgs e)
		{
			if (this.DesignMode)
				return;
			FocusChanged?.Invoke();
			GUIUtilities.CurrentFocus = this;
		}

		private void ColourPanel_LostFocus(object sender, EventArgs e)
		{
			if (this.DesignMode)
				return;
			FocusChanged?.Invoke();
			GUIUtilities.CurrentFocus = null;
		}

		public void SetToolTips(ToolTip tt)
		{
			// none in this control
		}

		public event EventHandler Accessed;
		public event NullEventHandler FocusChanged;
		public void SubscribeEvents(EventHandler AccessedHandler, NullEventHandler FocusChangedHandler)
		{
			if (AccessedHandler != null)
				Accessed += AccessedHandler;
			if (FocusChangedHandler != null)
				FocusChanged += FocusChangedHandler;
		}

		// see coords section for Size requests

		#endregion

		#region Alternate colours
		/// <summary>Returns a list of colours to offer as alternatives to the starting colour; i.e. colours that are similar</summary>
		private List<Color> GetAlternateColours(Color startingColour)
		{
			List<Color> colours = new List<Color>();
			int[] aParameterRange = { 1, 2, 1 };
			// the number of alternatives to offer in each of R,G,B.  This is offered both +ve and -ve
			// and the original value is also offered.  So 1 here actually gives 3 values; 2 gives 5 values
			if (startingColour.R == startingColour.B && startingColour.R == startingColour.G && startingColour.R > 0 && startingColour.R < 255)
			{
				// the original colour is grey; return a greyscale
				// not necessary to return black and white, but it does not matter if these are included
				const int COLOURS = 45; // must be adjusted if aParameterRange is adjusted
				const int STEP = 255 / COLOURS;
				const int LOSS = 255 - STEP * COLOURS; // the range which will not be covered
				for (int index = 0; index <= COLOURS - 1; index++)
				{
					int value = LOSS / 2 + index * STEP;
					colours.Add(Color.FromArgb(value, value, value));
				}
				return colours;
			}
			int[][] parameterValues = new int[3][]; // the range of values to use for each parameter
			for (int parameter = 0; parameter <= 2; parameter++) // 0 = R
			{
				// decide the range of colours to work through.  If the starting colour is near the limit it cannot be symmetrical
				int range = aParameterRange[parameter];
				int original = ColourParameter(parameter, startingColour);
				int step = 56 / range; // usually we go + -64, i.e. covering total half of the 255 range
				if (startingColour.ToArgb() == Color.White.ToArgb())
					step /= 3; // when modifying white I would prefer the colours will to be quite close to white (without this line they were a bit too coloured)
				else if (IndexOf(Color.FromArgb(255, startingColour)) < 0)
				{
					//   step = 24 \ intRange ' if already using a refined colour then refine more narrowly.  But - ignore transparency when checking if current appears in list
					// v0.20 - Cath/David prefer to keep original range
					step = 38 / range;
				}
				int[] values = new int[range * 2 + 1]; // we need intRange * 2 + 1 values, but VB adds one on automatically
				if (original < 8)
					EquallySpacedValues(0, step, values);
				else if (original > 246)
				{
					EquallySpacedValues(255 - range * step * 2, step, values);
				} // original will be the second item in the list; just need to adjust the first item to be 0 not negative
				else if (original < step)
				{
					EquallySpacedValues(original - step, step, values);
					values[0] = 0;
				}
				else if (original > 255 - step)
				{
					// this actually stores the values backwards, but that doesn't really matter
					EquallySpacedValues(original + step, -step, values);
					values[0] = 255;
				}
				else if (original < range * step)
				{
					// this will only apply if intRange >1
					values[range] = original;
					int lowStep = original / range; // subdivide the area between the original and 0 into the necessary number of steps
					for (int index = 0; index <= range - 1; index++)
					{
						values[index] = 0 + lowStep * index;
						values[range + index + 1] = original + step * index;
					}
				}
				else if (original > 255 - range * step)
				{
					values[range] = original;
					int highStep = (255 - original) / range; // subdivide the area between the original and 0 into the necessary number of steps
					for (int index = 0; index <= range - 1; index++)
					{
						values[range + index + 1] = original - step * (range - index);
						values[index] = original + highStep * index;
					}
				} // we are not near either edge, can just use the normal range
				else
				{
					EquallySpacedValues(original - step * range, step, values);
				}
				parameterValues[parameter] = values;
			}
			int alpha = startingColour.A;
			colours.AddRange(from G in parameterValues[1] from R in parameterValues[0] from B in parameterValues[2] select Color.FromArgb(alpha, R, G, B));
			return colours;
		}

		private static int ColourParameter(int parameter, Color colour)
		{
			switch (parameter)
			{
				case 0: return colour.R;
				case 1: return colour.G;
				case 2: return colour.B;
				default: throw new ArgumentException("ColourParameter(parameter) out of range");
			}
		}

		private void EquallySpacedValues(int minimum, int step, int[] values)
		{
			for (int index = 0; index <= values.Length - 1; index++)
			{
				values[index] = minimum;
				minimum += step;
			}
		}

		private List<Color> GetTransparentColours(Color starting)
		{
			List<Color> colours = new List<Color>();
			for (int index = 0; index <= 31; index++)
			{
				int alpha = 7 + 8 * index;
				colours.Add(Color.FromArgb(alpha, starting));
			}
			return colours;
		}

		#endregion

		#region Keys and navigation

		protected override bool IsInputKey(Keys keyData)
		{
			// would like to share with ButtonPanel - same function - however they derive from different classes
			// this is still needed - the form doesn't intercept without this
			if ((keyData & Keys.KeyCode) == Keys.Tab)
				return true;
			else if ((keyData & Keys.KeyCode) == Keys.Left || (keyData & Keys.KeyCode) == Keys.Right || (keyData & Keys.KeyCode) == Keys.Up || (keyData & Keys.KeyCode) == Keys.Down)
				return true;
			else
				return base.IsInputKey(keyData);
		}

		private void Iterate(int delta)
		{
			int index = (IndexOf(CurrentColour, true) + delta + m_Colours.Count) % m_Colours.Count;
			CurrentColour = m_Colours[index];
			UserSelectedColour?.Invoke();
			Accessed?.Invoke(this, EventArgs.Empty);
		}

		public void CombinedKeyDown(CombinedKeyEvent e)
		{
			if (!Enabled || !m_Applicable)
				return;
			if (e.KeyData == Keys.Right || e.KeyData == Keys.Tab)
			{
				Iterate(1);
				e.Handled = true;
			}
			else if (e.KeyData == Keys.Left || e.KeyData == (Keys.Tab | Keys.Shift))
			{
				Iterate(-1);
				e.Handled = true;
			}
			else if (e.KeyData == Keys.Up)
			{
				if (IndexOf(CurrentColour, true) >= m_Columns)
					Iterate(-m_Columns);
				e.Handled = true;
			}
			else if (e.KeyData == Keys.Down)
			{
				if (IndexOf(CurrentColour, true) < m_Colours.Count - m_Columns)
					Iterate(m_Columns);
				e.Handled = true;
			}
			else if (e.KeyData == Keys.Enter || e.KeyData == Keys.Return)
			{
				if (Focused)
				{
					Globals.Root.PerformAction(Verb.Find(Codes.RestoreFocus));
					e.Handled = true;
				}
			}
		}

		public void CombinedKeyUp(CombinedKeyEvent e)
		{ }

		public void TriggerAction(Action action)
		{
			// called by frmMain when it detects one of the palette control actions and this colour panel is focused
			if (!this.Enabled || !m_Applicable)
				return;
			switch ((action as PaletteButtonAction)?.Index ?? 0)
			{ // will be 0 for any other actions, and drop into default
				case 1:
					HandleClick(MOREBUTTON);
					break;
				case 2:
					HandleClick(TRANSPARENTBUTTON);
					break;
				default:
					Utilities.LogSubError("Unexpected code in ColourPanel::TriggerAction");
					return;
			}
			if (this.Focused)
				Globals.Root.PerformAction(Verb.Find(Codes.RestoreFocus));
			// for keyboard users need to return to main area, as selecting a colour does
		}

		public void BringPointerInsideOnActivation()
		{
			// can be ignored as arrow keys work on the selection
		}

		#endregion

	}

}
