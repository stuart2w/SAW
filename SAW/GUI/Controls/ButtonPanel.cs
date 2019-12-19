using System.Collections.Generic;
using System;
using System.Drawing;
using System.Diagnostics;
using System.Collections;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.Linq;

namespace SAW
{
	public class ButtonPanel : ContainerControl, IPalette, IKeyControl, IEnumerable<RoundButton>
	{

		// if control itself has TabStop, then it will intercept tab and make it SELECT the next button
		// it this does not have TabStop then it will make all the buttons TabStop=true, and tab will iterate around the buttons WITHOUT selecting them
		// (they get the hover highlight when focused)

		private readonly List<RoundButton> m_Buttons = new List<RoundButton>();
		private int m_Columns = 3;

		internal bool RadioMode = false;
		// if true it will make sure that only one button is selected.  Not needed on many of the palettes
		// because the buttons themselves implement this using the GUI parameter

		public event EventHandler RightClick; // Note that this will usually fire in addition to the standard action of the button (which happens first)

		public ButtonPanel()
		{
			this.Resize += ButtonPanel_Resize;
			this.GotFocus += ButtonPanel_GotFocus;
			this.LostFocus += ButtonPanel_LostFocus;
			this.MouseClick += ButtonPanel_MouseClick;
		}

		#region Adding and removing buttons

		public RoundButton AddButton()
		{
			RoundButton create = new RoundButton();
			AddExisting(create);
			ButtonPanel_Resize(this, null);
			return create;
		}

		public ArrowheadButton AddArrowheadButton()
		{
			ArrowheadButton create = new ArrowheadButton();
			AddExisting(create);
			ButtonPanel_Resize(this, null);
			return create;
		}

		public void AddExisting(RoundButton existing)
		{
			existing.Width = m_ButtonSize;
			existing.Height = m_ButtonSize;
			existing.Left = m_Buttons.Count % m_Columns * m_ButtonSize;
			existing.Top = m_Buttons.Count / m_Columns * m_ButtonSize;
			existing.TabIndex = this.TabIndex + m_Buttons.Count;
			existing.TabStop = !this.TabStop;
			this.Controls.Add(existing);
			m_Buttons.Add(existing);
			existing.WasSelected += Button_Selected;
			existing.LostFocus += ButtonPanel_LostFocus;
			existing.GotFocus += ButtonPanel_GotFocus;
		}

		public void Clear(bool disposeButtons)
		{
			this.Controls.Clear();
			foreach (RoundButton button in m_Buttons)
			{
				button.WasSelected -= Button_Selected;
				button.LostFocus -= ButtonPanel_LostFocus;
				button.GotFocus -= ButtonPanel_GotFocus;
				if (disposeButtons) button.Dispose();
			}
			m_Buttons.Clear();
		}

		public void RemoveButton(RoundButton button)
		{
			Debug.Assert(m_Buttons.IndexOf(button) == m_Buttons.Count - 1); // at the moment this makes no attempt to move the other buttons up
			m_Buttons.Remove(button);
			this.Controls.Remove(button);
		}

		public void SelectIndex(int selected)
		{
			for (int index = 0; index <= m_Buttons.Count - 1; index++)
			{
				m_Buttons[index].Selected = index == selected;
			}
		}

		IEnumerator<RoundButton> IEnumerable<RoundButton>.GetEnumerator()
		{
			return m_Buttons.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return m_Buttons.GetEnumerator();
		}

		#endregion

		#region Positioning

		#region ButtonSize
		private int m_ButtonSize = 48;
		[System.ComponentModel.DefaultValue(48)]
		public int ButtonSize
		{
			get { return m_ButtonSize; }
			set
			{
				if (m_ButtonSize == value)
					return;
				m_ButtonSize = Math.Max(value, 12);
				m_Columns = Math.Max(1, (Width - Padding.Horizontal) / m_ButtonSize);
				RedoPositions();
			}
		}

		#endregion

		private void RedoPositions()
		{
			this.SuspendLayout();
			int Y = 0;
			for (int index = 0; index <= m_Buttons.Count - 1; index++)
			{
				m_Buttons[index].Bounds = new Rectangle(index % m_Columns * m_ButtonSize + this.Padding.Left, index / m_Columns * m_ButtonSize + Y + this.Padding.Top, m_ButtonSize, m_ButtonSize);
			}
			this.ResumeLayout();
		}

		private void ButtonPanel_GotFocus(object sender, EventArgs e)
		{
			// Unfortunately the event is raised here after the button when the button is focused
			if (GUIUtilities.CurrentFocus == null || !this.Controls.Contains(GUIUtilities.CurrentFocus))
				GUIUtilities.CurrentFocus = this;
			FocusChanged?.Invoke();
		}

		private void ButtonPanel_LostFocus(object sender, EventArgs e)
		{
			FocusChanged?.Invoke();
		}

		private void ButtonPanel_Resize(object sender, EventArgs e)
		{
			int columns = Math.Max(1, (Width - this.Padding.Horizontal) / m_ButtonSize);
			if (columns != m_Columns)
			{
				m_Columns = columns;
				RedoPositions();
			}
		}

		#endregion

		#region Keys, mouse and navigation
		// If true tabbing through actually selects each button
		// otherwise it just moves the focus, and these must press to select
		[System.ComponentModel.DefaultValue(true)]
		public bool UseArrowKeys { get; set; } = true; // If true the arrow keys will navigate selection around the panel

		[System.ComponentModel.DefaultValue(true)]
		public bool AutoIterate { get; set; } = true;
		protected override bool IsInputKey(Keys keyData)
		{
			// would like to share with ColourPanel - same function - however they derive from different classes
			switch (keyData & Keys.KeyCode)
			{
				case Keys.Tab:
					return true;
				case Keys.Left:
				case Keys.Right:
				case Keys.Up:
				case Keys.Down:
					return true;
				default:
					return base.IsInputKey(keyData);
			}
		}

		private int FindSelected()
		{
			for (int index = 0; index <= m_Buttons.Count - 1; index++)
			{
				if (m_Buttons[index].Selected)
					return index;
			}
			return -1;
		}

		private int FindFocus()
		{
			for (int index = 0; index <= m_Buttons.Count - 1; index++)
			{
				if (m_Buttons[index].Focused)
					return index;
			}
			return -1;
		}

		protected override bool ProcessTabKey(bool forward)
		{
			return Iterate(forward ? 1 : -1, false);
		}

		private bool Iterate(int delta, bool canWrap = true)
		{
			// returns true if did iterate.  False if rejected (only if not canWrap)
			int original = AutoIterate ? FindSelected() : FindFocus();
			if (original < 0)
				original = delta > 0 ? m_Buttons.Count - 1 : 0; // so it starts counting in from the end whichever direction we are going in
			if (!canWrap)
			{
				if (original == 0 && delta <= 0 || original >= m_Buttons.Count - 1 && delta > 0)
					return false;
			}
			int index = (original + delta + m_Buttons.Count) % m_Buttons.Count;
			// iterate until we reach a button which is actually active
			while (index != original)
			{
				if (m_Buttons[index].Applicable)
				{
					if (AutoIterate)
						m_Buttons[index].Trigger();
					else
						m_Buttons[index].Focus();
					return true;
				}
				if (!canWrap)
				{
					if (index == 0 && delta <= 0 || index >= m_Buttons.Count - 1 && delta > 0)
						return false;
				}
				index = (index + delta + m_Buttons.Count) % m_Buttons.Count;
			}
			Debug.Fail("Failed to iterate ButtonPanel");
			return false;
		}

		private void ButtonPanel_MouseClick(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Right && Globals.Root.User == Users.Editor)
				RightClick?.Invoke(this, EventArgs.Empty);
		}

		public void CombinedKeyDown(CombinedKeyEvent e)
		{
			if (UseArrowKeys)
			{
				switch (e.KeyCode)
				{
					case Keys.Left:
						Iterate(-1);
						Accessed?.Invoke(this, EventArgs.Empty);
						e.Handled = true;
						break;
					case Keys.Right:
						Iterate(1);
						Accessed?.Invoke(this, EventArgs.Empty);
						e.Handled = true;
						break;
					case Keys.Up:
						Iterate(-m_Columns);
						Accessed?.Invoke(this, EventArgs.Empty);
						e.Handled = true;
						break;
					case Keys.Down:
						Iterate(m_Columns);
						Accessed?.Invoke(this, EventArgs.Empty);
						e.Handled = true;
						break;
				}
			}
			if (e.KeyData == Keys.Tab)
			{
				if (!Iterate(1, false))
					GUIUtilities.IterateTabStop(this.Parent, true);
				Accessed?.Invoke(this, EventArgs.Empty);
				e.Handled = true;
			}
			else if (e.KeyData == (Keys.Tab | Keys.Shift))
			{
				if (!Iterate(-1, false))
					GUIUtilities.IterateTabStop(this.Parent, false);
				Accessed?.Invoke(this, EventArgs.Empty);
				e.Handled = true;
			}
		}

		public void CombinedKeyUp(CombinedKeyEvent e)
		{ }

		public void BringPointerInsideOnActivation()
		{
			// ignored as the arrow keys will step selection
			if (!AutoIterate && this.Any())
				m_Buttons[0].Focus();
		}

		#endregion

		#region Blended background
		// part of this is shared so that it can be used to paint the background of any control.
		private BlendDirection m_Blend = BlendDirection.Off;

		public BlendDirection Blend
		{
			get { return m_Blend; }
			set
			{
				if (m_Blend != value)
					Invalidate();
				m_Blend = value;
			}
		}

		public enum BlendDirection
		{
			Off,
			Down,
			Right
		}

		private static readonly Color colBlendIn = Color.DarkBlue; // only ever used about 10%, so needs to be quite dark
		public static void BlendPanelBackground(Control pnl, Graphics gr, BlendDirection direction)
		{
			// for use during the paint event handler of the given control
			if (Utilities.Low_Graphics_Safe())
			{
				return;
			}
			// In order to cope with multiple rows (or lines) we need to work within the top-level blended control, which I think will always be placed directly on the form
			// calculate the bounding box of the container, relative to my coordinates
			Rectangle container = new Rectangle(0, 0, pnl.Width, pnl.Height);
			Control testControl = pnl;
			while (testControl.Parent != null && !(testControl.Parent is Form))
			{
				container.X -= testControl.Left;
				container.Y -= testControl.Top;
				testControl = testControl.Parent;
				container.Size = testControl.Size;
			}
			Point dark;
			switch (direction)
			{
				case BlendDirection.Off:
					return;
				case BlendDirection.Down:
					dark = new Point(0, container.Height); // pnl.Height)
					container.X = 0;
					break;
				default:
					dark = new Point(pnl.Width, 0);
					container.Y = 0;
					Debug.Assert(direction == BlendDirection.Right);
					break;
			}
			using (LinearGradientBrush br = new LinearGradientBrush(container.Location, dark, Color.White, Color.Black)) // colours changed below
			{
				ColorBlend blend = new ColorBlend(3);
				blend.Positions = new float[] { 0, 0.7f, 0.85f, 1 };
				Color baseColour = Color.FromKnownColor(KnownColor.Control);
				blend.Colors = new Color[] { baseColour, baseColour.MixWith(colBlendIn, 98), baseColour.MixWith(colBlendIn, 95), baseColour.MixWith(colBlendIn, 92) };
				br.InterpolationColors = blend;
				gr.FillRectangle(br, 0, 0, pnl.Width, pnl.Height);
			}
		}

		public new bool DoubleBuffered
		{
			get { return base.DoubleBuffered; }
			set { base.DoubleBuffered = value; }
		}

		#endregion

		#region Accordion

		private void Button_Selected(object sender, EventArgs e)
		{
			Accessed?.Invoke(this, EventArgs.Empty);
			RoundButton button = (RoundButton)sender;
			if (RadioMode)
			{
				foreach (RoundButton button2 in Controls)
				{
					if (button2 != sender)
						button2.Selected = false;
				}
			}
		}

		public void Initialise()
		{
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


		#region Sizing

		public Size SizeFromSize(Size requested)
		{
			return SizeFromWidth(requested.Width);
		}

		public Size SizeFromWidth(int requested)
		{
			int width = Math.Max(m_ButtonSize + Padding.Horizontal, requested);
			int columns = Math.Max(1, (Width - this.Padding.Horizontal) / m_ButtonSize);
			int height = Math.Max((m_Buttons.Count + columns - 1) / columns, 1) * m_ButtonSize;
			return new Size(width, height);
			//Return Me.DefaultSize
		}

		#endregion

		#endregion

		#region Construct some standard palettes

		public void ConstructLinePalette(int[] widths = null)
		{
			if (widths == null)
				widths = ParameterSupport.StandardLineWidths;
			foreach (int width in widths)
			{
				AddLine(width, DashStyle.Solid, Parameters.LineWidth);
			}
			foreach (DashStyle pattern in ParameterSupport.StandardLinePatterns)
			{
				// the second line width is always used when demonstrating the pattern (changing the width does change the spacing of the pattern
				AddLine(ParameterSupport.StandardLineWidths[1], pattern, Parameters.LinePattern);
			}
		}

		private void AddLine(int width, DashStyle pattern, Parameters parameter)
		{
			// width is specified in data coordinates
			RoundButton button = AddButton();
			button.SampleLine = new Pen(Color.Black, width / 100f / Geometry.POINTUNIT) { DashStyle = pattern };

			if (parameter == Parameters.LineWidth)
				button.AttachParameter(parameter, width);
			else
				button.AttachParameter(parameter, (int)pattern);
		}

		public void ConstructFillStylePalette()
		{
			foreach (Shape.FillStyleC.Patterns pattern in ParameterSupport.StandardFillPatterns)
			{
				RoundButton create = AddButton();
				switch (pattern)
				{
					case Shape.FillStyleC.Patterns.Solid: // solid
						create.SampleHatch = new SolidBrush(Color.Black);
						break;
					case Shape.FillStyleC.Patterns.Empty: // none
						create.SampleHatch = new SolidBrush(Color.White);
						break;
					default:
						create.SampleHatch = new HatchBrush((HatchStyle)pattern, Color.Black, Color.White);
						break;
				}
				create.AttachParameter(Parameters.FillPattern, (int)pattern);
			}
		}

		#endregion

	}

}
