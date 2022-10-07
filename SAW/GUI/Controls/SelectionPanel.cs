using System;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;

namespace SAW
{
	/// <summary>Extends a FlowLayoutPanel to support selecting one of the contents by clicking on it
		/// the click handler will only work correctly for single controls, not controls containing other controls</summary>
	[System.ComponentModel.DefaultEvent("UserSelected")]
	internal sealed class SelectionPanel : Panel
	{

		#region Properties and Fields
		private int m_Selected = -1;
		private Color m_OriginalColour = Color.Empty; // the original colour of any selected item
		private Color m_SelectedColour = Color.FromKnownColor(KnownColor.Highlight);
		private Color m_OriginalTextColour = Color.Empty;
		private Color m_SelectedTextColour = Color.Empty; // the foreground colour to use on any selected item

		[System.ComponentModel.DefaultValue(true)]
		public bool AutoSizeControls { get; set; } = true;

		/// <summary>If true the controls are positioned horizontally.  Requires AutoSizeControls= False</summary>
		/// <value></value>
		/// <returns></returns>
		/// <remarks></remarks>
		[System.ComponentModel.DefaultValue(false)]
		public bool Horizontal { get; set; } = false;

		/// <summary>Ideally the items automatically size themselves.  This is the minimum.  The problem is the ones with images, which use the Button class
		/// These don't seem to resize vertically if the text overflows and some can be lost</summary>
		/// <returns></returns>
		public int MinimumItemHeight { get; set; } = 12;
		#endregion

		public event EventHandler SelectedIndexChanged;
		public event EventHandler UserSelected;// fires after the above, if the user clicked to change

		public SelectionPanel()
		{
			this.Padding = new Padding(3);
			this.AutoScroll = true;
			this.ControlRemoved += SelectionPanel_ControlRemoved;
			this.KeyDown += SelectionPanel_KeyDown;
			this.Paint += SelectionPanel_Paint;
			this.ControlAdded += SelectionPanel_ControlAdded;
			this.Layout += SelectionPanel_Layout;
			this.Resize += SelectionPanel_Resize;
		}

		[System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden), System.ComponentModel.Browsable(false)]
		public int SelectedIndex
		{
			get { return m_Selected; }
			set
			{
				if (value >= Controls.Count)
					throw new ArgumentException("SelectedIndex");
				if (m_Selected >= 0)
				{
					Controls[m_Selected].BackColor = m_OriginalColour;
					if (!m_SelectedTextColour.IsEmpty)
						Controls[m_Selected].ForeColor = m_OriginalTextColour;
					if (Controls[m_Selected] is Button)
						UpdateButtonColours((Button)Controls[m_Selected]);
				}
				m_Selected = value;
				if (m_Selected >= 0)
				{
					m_OriginalColour = Controls[m_Selected].BackColor;
					Controls[m_Selected].BackColor = m_SelectedColour;
					if (!m_SelectedTextColour.IsEmpty)
					{
						m_OriginalTextColour = Controls[m_Selected].ForeColor;
						Controls[m_Selected].ForeColor = m_SelectedTextColour;
					}
					else
						m_OriginalTextColour = Color.Empty;
					if (Controls[m_Selected] is Button)
						UpdateButtonColours((Button)Controls[m_Selected]);
				}
				SelectedIndexChanged?.Invoke(this, EventArgs.Empty);
			}
		}

		public Color SelectedColour
		{
			get{return m_SelectedColour;}
			set
			{
				m_SelectedColour = value;
				if (m_Selected >= 0)
					Controls[m_Selected].BackColor = m_SelectedColour;
			}
		}

		public Color SelectedTextColour
		{
			get { return m_SelectedTextColour; }
			set
			{
				m_SelectedTextColour = value;
				if (m_Selected >= 0 && !value.IsEmpty)
					Controls[m_Selected].ForeColor = value;
			}
		}

		/// <summary>If true, then clicking the selected item deselected leaving nothing selected.  Default is false</summary>
		[System.ComponentModel.DefaultValue(false)]
		public bool ToggleSelection { get; set; }

		private void SelectionPanel_ControlRemoved(object sender, ControlEventArgs e)
		{
			e.Control.Click -= Control_Click;
		}

		private void Control_Click(object sender, EventArgs e)
		{
			this.Focus();
			int index = Controls.IndexOf((Control)sender);
			if (index == m_Selected && ToggleSelection)
				index = -1;
			SelectedIndex = index;
			UserSelected?.Invoke(this, EventArgs.Empty);
		}

		private void SelectionPanel_KeyDown(object sender, KeyEventArgs e)
		{
			switch (e.KeyCode)
			{
				case Keys.Left:
				case Keys.Up:
					if (m_Selected == -1)
					{
						SelectedIndex = Controls.Count - 1;
						UserSelected?.Invoke(this, EventArgs.Empty);
					}
					else if (m_Selected > 0)
					{
						int newSelection = m_Selected - 1;
						while (newSelection > 0 && Controls[m_Selected].Visible == false)
						{
							newSelection -= 1;
						}
						if (Controls[newSelection].Visible == false)
							return; // failed to find any visible control in this direction
						SelectedIndex = newSelection;
						UserSelected?.Invoke(this, EventArgs.Empty);
						// If m_Selected = 0 then no change
					}
					break;
				case Keys.Right:
				case Keys.Down:
					if (m_Selected == -1)
					{
						SelectedIndex = 0;
						UserSelected?.Invoke(this, EventArgs.Empty);
					}
					else if (m_Selected < Controls.Count - 1)
					{
						int newSelection = m_Selected + 1;
						while (newSelection < Controls.Count - 1 && Controls[newSelection].Visible == false)
						{
							newSelection += 1;
						}
						if (Controls[newSelection].Visible == false)
							return; // failed to find any visible control in this direction
						SelectedIndex = newSelection;
						UserSelected?.Invoke(this, EventArgs.Empty);
					}
					break;
			}
		}

		protected override bool IsInputKey(Keys keyData)
		{
			switch (keyData)
			{
				case Keys.Left:
				case Keys.Up:
				case Keys.Right:
				case Keys.Down:
					return true;
			}
			return base.IsInputKey(keyData);
		}

		private void SelectionPanel_Paint(object sender, PaintEventArgs e)
		{
			// But doesn't act as a TabStop even is the property is set
			if (this.Focused)
			{
				//ControlPaint.DrawFocusRectangle(e.Graphics, New Rectangle(0, 0, Width - 1, Height - 1))
				// to reinstate this need to invalidate on change focus and resize
			}
		}

		public Label AddLabel(string text)
		{
			Label label = new Label() { Text = text, AutoSize = true, Padding = new Padding(2) };
			this.Controls.Add(label);
			return label;
		}

		public Button AddWithImage(string text, Bitmap image)
		{
			// This needs to use a button in order to get the text image relationship.  With a label both are superimposed
			Button button = new Button {Text = Strings.Translate(text), Padding = new Padding(2), FlatStyle = FlatStyle.Flat};
			button.FlatAppearance.BorderSize = 0;
			button.Image = image;
			button.ImageAlign = ContentAlignment.MiddleLeft;
			button.TextImageRelation = TextImageRelation.ImageBeforeText;
			button.Margin = new Padding(1, 1, 1, 2);
			button.AutoSize = true;
			UpdateButtonColours(button);
			button.AutoEllipsis = true;
			this.Controls.Add(button);
			return button;
		}

		private void SelectionPanel_ControlAdded(object sender, ControlEventArgs e)
		{
			// Effectively shared between AddLabel and AddWithImage
			e.Control.Click += Control_Click;
			if (AutoSizeControls)
			{
				e.Control.MinimumSize = new Size(this.Width - this.Padding.Horizontal - SystemInformation.VerticalScrollBarWidth, MinimumItemHeight);
				e.Control.MaximumSize = new Size(this.Width - this.Padding.Horizontal - (VScroll ? SystemInformation.VerticalScrollBarWidth : 0), Math.Max(MinimumItemHeight, 100));
			}
			e.Control.Cursor = Cursors.Hand;
			e.Control.BackColor = this.BackColor;
		}

		private void SelectionPanel_Layout(object sender, LayoutEventArgs e)
		{
			int pos = 0;
			foreach (Control control in from Control c in Controls where c.Visible select c)
			{
				if (Horizontal)
				{
					pos += control.Margin.Left;
					control.Left = pos;
					control.Top = this.Padding.Top;
					pos += control.Width + control.Margin.Right;
				}
				else
				{
					pos += control.Margin.Top;
					control.Top = pos;
					control.Left = this.Padding.Left;
					pos += control.Height + control.Margin.Bottom;
				}
			}
		}

		private static void UpdateButtonColours(Button button)
		{
			button.FlatAppearance.MouseOverBackColor = button.BackColor.StepColour(Color.DarkBlue, 5);
			button.FlatAppearance.MouseDownBackColor = button.BackColor.StepColour(Color.DarkBlue, 20);
		}

		private void SelectionPanel_Resize(object sender, EventArgs e)
		{
			// If items with an image and multiple lines of text don't fit, then the container should adjust the MinimumItemHeight property
			if (AutoSizeControls)
				foreach (Control control in Controls)
				{
					control.MaximumSize = new Size(this.Width - this.Padding.Horizontal - (VScroll ? SystemInformation.VerticalScrollBarWidth : 0), 100);
				}
		}

	}

}
