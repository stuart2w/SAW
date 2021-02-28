using System;
using System.Drawing;
using System.Diagnostics;
using System.Windows.Forms;
using SAW.Functions;
using Action = SAW.Functions.Action;

namespace SAW
{
	public class RoundButton : SharedButton // usually used within palettes; where there are several which can be selected as alternatives
	{

		// Usually the state is reflected through the GUIParameter attached to this.
		// Of it can be used as an independent control using the Selected property

		public RoundButton()
		{
			Width = STANDARDSIZE;
			Height = STANDARDSIZE;
			this.MouseEnter += RoundButton_MouseEnter;
			this.MouseLeave += RoundButton_MouseLeave;
			this.MouseClick += RoundButton_MouseClick;
			this.Click += RoundButton_Click;
			this.SizeChanged += RoundButton_SizeChanged;
		}

		public event EventHandler UserChangedSelected;      // only raised if the parameter is not attached
		public event EventHandler WasSelected;// raised either in response to click or if selected by parameter change

		public const int STANDARDSIZE = 48;

		#region Mouse

		private void RoundButton_MouseEnter(object sender, EventArgs e)
		{
			// mostly implemented in base class
			if (m_Parameter != Parameters.None)
				Globals.SetHover(Action.Create(m_Parameter, m_ParameterValue));
		}

		private void RoundButton_MouseLeave(object sender, EventArgs e)
		{
			// mostly implemented in base class
			if (m_Parameter != Parameters.None)
				Globals.ClearHover();
		}

		private void RoundButton_MouseClick(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Right && Globals.Root.User == Users.Editor && !Globals.CheckDiscardClick(this))
				OnRightClick(EventArgs.Empty);
		}

		private void RoundButton_Click(object sender, EventArgs e)
		{
			// is also called indirectly if the button is triggered by other means
			if (Globals.CheckDiscardClick(this))
				return;
			GUIUtilities.FocusIfNotInPalette(this.Parent);
			if (this.DesignMode || !m_Applicable || !Enabled)
				return; // Or m_bolSelected - need to resend Tool=Measure in particular
			if (m_Parameter != Parameters.None)
			{
				Globals.SetParameterValue(m_ParameterValue, m_Parameter);
				// it should raise the value changed event back again which will update the GUI
			}
			else
			{
				SetSelectionGUI(!m_Selected);
				UserChangedSelected?.Invoke(this, EventArgs.Empty);
			}
			WasSelected?.Invoke(this, EventArgs.Empty);
		}

		public override void Trigger()
		{
			RoundButton_Click(this, EventArgs.Empty);
		}

		#endregion

		#region Graphics

		protected override void OnPaint(PaintEventArgs e)
		{
			if (!this.Visible || Parent != null && !Parent.Visible)
				return;
			DrawInternalBackground(e, false);

			// Presumably only one of the image, line and hatching will be defined...

			Image image = null;
			int drawSize = 0; // usually the image is drawn at its actual size, but if we are to make it expand we must draw at a specific size
			if (!string.IsNullOrEmpty(m_ImageName))
				image = (Image)GUIUtilities.RM.GetObject(m_ImageName);
			else if (m_Parameter == Parameters.Tool)
			{
				drawSize = this.Width - EXCESSSIZE;
				drawSize += EXCESSSIZE * m_SelectionDisplayFraction / 300; // maximum value of m_SelectionDisplayFraction is 255; deliberately dividing by bit more to keep the image smaller
				image = GUIUtilities.ShapeImage((Shape.Shapes)m_ParameterValue, drawSize);
			}
			if (m_Image != null)
				image = m_Image;

			if (m_Parameter == Parameters.Tool)
			{
				if ((Shape.Shapes)m_ParameterValue == Shape.Shapes.Polygon)
				{
					// for the polygon we draw the current number of sides
					Rectangle rctNumber = new Rectangle(INSET + 1, INSET + 4, Width - INSET * 2 - 5, Height - INSET * 2 - 5);
					e.Graphics.DrawString(Polygon.CURRENTSIDES.ToString(), GUIUtilities.ShortcutFont, Brushes.Black, rctNumber, GUIUtilities.StringFormatCentreCentre);
				}
			}

			if (image != null)
			{
				DrawStandardImage(e.Graphics, image, drawSize);
				if (image != m_Image)
					image.Dispose();
			}

			if (m_SampleLine != null)
			{
				e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
				e.Graphics.DrawLine(m_SampleLine, SAMPLEINSET, SAMPLEINSET, Width - SAMPLEINSET, Height - SAMPLEINSET);
			}

			if (m_SampleHatch != null)
				e.Graphics.FillRectangle(m_SampleHatch, SAMPLEINSET, SAMPLEINSET, Width - SAMPLEINSET * 2, Height - SAMPLEINSET * 2);

			DrawHighlightAndKey(e);
		}

		protected override string BackgroundImagePrefix
		{
			get { return  "RoundButton"; }
		}

		private void RoundButton_SizeChanged(object sender, EventArgs e)
		{
			Invalidate();
		}

		#region Sample lines and hatchings
		private const int SAMPLEINSET = INSET + 6;
		private Pen m_SampleLine = null; // if displaying line widths or dash patterns this can be defined.
		[System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
		public Pen SampleLine
		{
			get { return m_SampleLine; }
			set
			{
				m_SampleLine = value;
				m_SampleLine.EndCap = System.Drawing.Drawing2D.LineCap.Round;
				m_SampleLine.StartCap = System.Drawing.Drawing2D.LineCap.Round;
				Invalidate();
			}
		}

		private Brush m_SampleHatch = null; // can display various fill patterns
		[System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
		public Brush SampleHatch
		{
			get { return m_SampleHatch; }
			set
			{
				m_SampleHatch = value;
				Invalidate();
			}
		}

		#endregion

		#endregion

		#region Parameter
		// if this control can automatically update the GUI, this is the parameter it changes:
		protected Parameters m_Parameter = Parameters.None;
		protected int m_ParameterValue = 0;

		public virtual void AttachParameter(Parameters parameter, int value)
		{
			Debug.Assert(parameter != Parameters.None);
			DetachParameter();
			m_Parameter = parameter;
			m_ParameterValue = value;
			Globals.ParameterChanged += ParameterChanged;
			Globals.ApplicabilityChanged += ApplicabilityChanged;
			SetSelectionGUI(Globals.ParameterValue(m_Parameter) == m_ParameterValue);
			if (m_Parameter == Parameters.Tool)
			{
				if ((Shape.Shapes)value == Shape.Shapes.Polygon)
					Polygon.CurrentSidesChanged += ShapeNeedsRefresh;
			}
		}

		public void DetachParameter()
		{
			if (m_Parameter != Parameters.None)
			{
				Globals.ParameterChanged -= ParameterChanged;
				Globals.ApplicabilityChanged -= ApplicabilityChanged;
			}
			if (m_Parameter == Parameters.Tool)
			{
				if (m_ParameterValue == (int)Shape.Shapes.Polygon)
					Polygon.CurrentSidesChanged -= ShapeNeedsRefresh;
			}
			m_Parameter = Parameters.None;
		}

		protected virtual void ParameterChanged(Parameters parameter)
		{
			if (parameter == m_Parameter)
			{
				bool bolSelected = Globals.ParameterValue(m_Parameter) == m_ParameterValue;
				SetSelectionGUI(bolSelected);
				if (bolSelected)
					WasSelected?.Invoke(this, EventArgs.Empty);
			}
		}

		private void ApplicabilityChanged()
		{
			if (m_Applicable != Globals.ParameterApplicable(m_Parameter))
			{
				m_Applicable = !m_Applicable;
				if (!m_Applicable)
					m_Hover = false;
				TabStop = m_Applicable;
				if (Enabled)
					Invalidate();
			}
		}

		public void SetTool(Shape.Shapes shape)
		{
			AttachParameter(Parameters.Tool, (int)shape);
			var key = Globals.Root.CurrentConfig.GetFirstKeyForAction(new ToolAction(shape));
			if (Utilities.Key_Shortcuts_Safe())
				ShortcutDisplay = GUIUtilities.KeyShortDescription(key);
		}

		private void ShapeNeedsRefresh()
		{
			// is attached to Polygon.CurrentSidesChanged only if this is displaying the polygon tool
			// ditto Arc.RadiusLockedChanged
			Invalidate();
		}

		public int ParameterValue
		{ get { return m_ParameterValue; } }

		#endregion

	}

	#region Arrowhead
	public class ArrowheadButton : RoundButton
	{

		// Like RoundButton.  Not sure at the moment was I want this to also match the other parameter
		// probably won't for the moment
		private Parameters m_OtherParameter;
		private int m_OtherCurrentValue = 0;

		// keeps a Line data object to do the graphics
		private Line m_Arrowhead;
		private Lined.ArrowheadC m_Style; // the style object relevant for this end of the arrow head

		public ArrowheadButton()
		{
			m_Arrowhead = new Line();
			m_Arrowhead.InitialiseArrowheadGUISample();
		}

		public override void AttachParameter(Parameters parameter, int value)
		{
			base.AttachParameter(parameter, value);
			switch (parameter)
			{
				case Parameters.ArrowheadEndSize:
					m_OtherParameter = Parameters.ArrowheadEndType;
					m_Style = m_Arrowhead.EndArrowhead;
					break;
				case Parameters.ArrowheadEndType:
					m_OtherParameter = Parameters.ArrowheadEndSize;
					m_Style = m_Arrowhead.EndArrowhead;
					break;
				case Parameters.ArrowheadStartSize:
					m_OtherParameter = Parameters.ArrowheadStartType;
					m_Style = m_Arrowhead.StartArrowhead;
					break;
				case Parameters.ArrowheadStartType:
					m_OtherParameter = Parameters.ArrowheadStartSize;
					m_Style = m_Arrowhead.StartArrowhead;
					break;
				default:
					throw new ArgumentException("ArrowheadButton can only be attached to parameters relevant to a Arrowhead");
			}
			m_OtherCurrentValue = Globals.ParameterValue(m_OtherParameter);
		}

		protected override void ParameterChanged(Parameters parameter)
		{
			base.ParameterChanged(parameter);
			if (parameter == m_OtherParameter)
			{
				m_OtherCurrentValue = Globals.ParameterValue(parameter);
				Invalidate();
			}
			if ((m_Parameter == Parameters.ArrowheadEndSize || parameter == Parameters.ArrowheadStartSize) && m_OtherCurrentValue == (int)Lined.ArrowheadC.Styles.None)
			{
				// if the arrow head is switched off, don't show any of the sizes as selected
				SetSelectionGUI(false);
			}
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			if (m_Style == null)
				return;
			DrawInternalBackground(e, false);
			m_Style.SetParameterValue(m_ParameterValue, m_Parameter);
			m_Style.SetParameterValue(m_OtherCurrentValue, m_OtherParameter);
			if (m_Parameter == Parameters.ArrowheadEndSize || m_Parameter == Parameters.ArrowheadStartSize)
			{
				// if showing size difference, ensure we show something, even if current head is 'None'
				if (m_Style.Style == Lined.ArrowheadC.Styles.None)
					m_Style.Style = Lined.ArrowheadC.Styles.SimpleSolid;
			}
			m_Style.Size *= 2; // we need multiply the value because the data and this GUI are using different scales
							   // the number is not precise, but we probably cannot display it exactly to scale as the button will be big enough
			System.Drawing.Drawing2D.SmoothingMode old = e.Graphics.SmoothingMode;
			if (!Utilities.Low_Graphics_Safe())
				e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
			m_Arrowhead.Draw(new NetCanvas(e.Graphics), 1, 1, null, StaticView.InvalidationBuffer.Base);
			e.Graphics.SmoothingMode = old;
			DrawHighlightAndKey(e);
		}

	}
	#endregion

}
