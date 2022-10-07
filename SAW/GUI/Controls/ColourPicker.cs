using System;
using System.Drawing;
using System.Diagnostics;
using System.Windows.Forms;


namespace SAW
{
	/// <summary>Panel that displays a colour and picks a new one when clicked </summary>
	[System.ComponentModel.DefaultEvent("UserChangedColour")]
	internal sealed partial class ColourPicker : IKeyControl
	{

		public event EventHandler UserChangedColour;

		private static Bitmap g_Bitmap16; // bitmap with 16 colours

		public ColourPicker()
		{
			InitializeComponent();
			base.Cursor = Cursors.Hand;
		}

		#region Properties
		private Color m_Current = Color.Empty;
		[System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
		public Color CurrentColour
		{
			get { return m_Current; }
			set
			{
				m_Current = value;
				Invalidate();
			}
		}

		private bool m_bolDisplayCurrentColour = true; // if false it displays a 16-colour option effect
		[System.ComponentModel.DefaultValue(true)]
		public bool DisplayCurrentColour
		{
			get { return m_bolDisplayCurrentColour; }
			set
			{
				m_bolDisplayCurrentColour = value;
				Invalidate();
			}
		}

		[System.ComponentModel.DefaultValue(false)]
		public bool AllowEmpty { get; set; }
		[System.ComponentModel.DefaultValue(false)]
		public bool BlackWhite { get; set; } // If true an empty colour is displayed as a black-and-white box in the popup


		[System.ComponentModel.DefaultValue(true)]
		public bool AllowTransparent { get; set; } = true;

		[System.ComponentModel.DefaultValue(true)]
		public bool UseSettings { get; set; } = true;

		#endregion

		#region Parameter
		// if this control can automatically update the GUI, this is the parameter it changes:
		private Parameters m_Parameter = Parameters.None;
		private bool m_Applicable = true; // whether this parameter is currently applicable
											   // always true if not parameterised

		public void AttachParameter(Parameters parameter, int value)
		{
			Debug.Assert(parameter != Parameters.None);
			DetachParameter();
			m_Parameter = parameter;
			Globals.ParameterChanged += ParameterChanged;
			Globals.ApplicabilityChanged += ApplicabilityChanged;
			m_Current = Color.FromArgb(Globals.ParameterValue(m_Parameter));
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
				m_Current = Color.FromArgb(Globals.ParameterValue(m_Parameter));
		}

		private void ApplicabilityChanged()
		{
			if (m_Applicable != Globals.ParameterApplicable(m_Parameter))
			{
				m_Applicable = !m_Applicable;
				Invalidate();
			}
			if (m_Applicable)
			{
				base.Cursor = Cursors.Hand;
				TabStop = true;
			}
			else
			{
				base.Cursor = Cursors.Default;
				TabStop = false;
			}
		}

		#endregion

		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);
			if (!m_Applicable)
				e.Graphics.Clear(this.BackColor);
			else if (!m_bolDisplayCurrentColour)
			{
				// draw standard selection marker (in design mode also)
				if (g_Bitmap16 == null)
					g_Bitmap16 = ColourPanel.GenerateMoreColoursBitmap(ColourPanel.GetStandard17Colours());
				e.Graphics.DrawImage(g_Bitmap16, new Rectangle(0, 0, Width, Height));
			}
			else if (this.DesignMode)
				e.Graphics.Clear(Color.LightBlue);
			else
			{
				if (m_Current.IsEmpty)
				{
					e.Graphics.Clear(Color.White);
					Image image = BlackWhite ? Resources.AM.BlackWhiteText : Resources.AM.RedCross;
					if (image != null && m_Applicable)
					{
						e.Graphics.DrawImageUnscaled(image, (Width - image.Width) / 2, (Height - image.Height) / 2);
						image.Dispose();
					}
				}
				else if (m_Current.A < 255)
				{
					e.Graphics.Clear(this.BackColor);
					using (SolidBrush br = new SolidBrush(m_Current))
					{
						e.Graphics.FillRectangle(br, 0, 0, Width, Height);
					}
				}
				else
					e.Graphics.Clear(m_Current);
			}
			if (!m_Applicable || !Enabled)
				using (Brush br = new System.Drawing.Drawing2D.HatchBrush(System.Drawing.Drawing2D.HatchStyle.DiagonalCross, Color.Gray))
					e.Graphics.FillRectangle(br, new Rectangle(0, 0, Width, Height));
			if (Focused)
				e.Graphics.DrawRectangle(GUIUtilities.FocusPen, 3, 3, Width - 6, Height - 6);
		}

		public void ColourPicker_Click(object sender, EventArgs e)
		{
			if (!Enabled || !m_Applicable)
				return;
			if (Globals.CheckDiscardClick(this))
				return;
			Color colNew = frmMoreColours.DisplayPickColour(this.FindForm(), m_Current, AllowEmpty, BlackWhite, AllowTransparent, UseSettings);
			if (colNew.Equals(m_Current))
				return;
			m_Current = colNew;
			UserChangedColour?.Invoke(this, EventArgs.Empty);
			if (m_Parameter != Parameters.None)
				Globals.SetParameterValue(m_Current.ToArgb(), m_Parameter);
			Invalidate();
		}

		public void ColourPicker_GotFocus(object sender, EventArgs e)
		{
			Invalidate();
		}

		/// <summary>Prevent form designer from setting the cursor </summary>
		[System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
		public override Cursor Cursor
		{
			get => base.Cursor;
			set {  }
		}

		#region Keys
		public void ColourPicker_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Space)
				ColourPicker_Click(sender, EventArgs.Empty);
		}

		public void CombinedKeyDown(CombinedKeyEvent e)
		{
			if (e.KeyCode == Keys.Space)
				ColourPicker_Click(this, EventArgs.Empty);
		}

		public void CombinedKeyUp(CombinedKeyEvent e)
		{
		}

		#endregion

	}

}
