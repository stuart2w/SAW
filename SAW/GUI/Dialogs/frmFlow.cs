using System;
using System.Drawing;
using System.Diagnostics;
using System.Windows.Forms;
using SAW.Shapes;


namespace SAW
{
	internal partial class frmFlow
	{

		private Flow m_Flow;
		private StringAlignment m_Alignment; // currently selected alignment (needs to be stored since the control is cleared at times)

		public frmFlow(Flow flow)
		{
			m_Flow = flow;
			m_Alignment = flow.Alignment; // The ComboBox will be populated and assigned when cmbdirection is assigned
			InitializeComponent();
			for (int index = 0; index <= 11; index++)
			{
				cmbDirection.Items.Add(index);
			}
			cmbDirection.SelectedIndex = (int)flow.Direction;
			for (int index = 0; index <= 2; index++)
			{
				cmbOverflow.Items.Add(index);
			}
			cmbOverflow.SelectedIndex = (int)flow.Overflow;
			try
			{
				nudPadding.Value = (decimal)m_Flow.Padding;
				nudShapeSeparation.Value = (decimal)m_Flow.ShapeSeparation;
			}
			catch // In case of out of range errors; this will just display the default value
			{
			}
			chkDragWithin.Checked = m_Flow.DragWithin;
			chkShowHighlight.Checked = m_Flow.ShowHighlight;
		}

		public void frmFlow_Load(object sender, EventArgs e)
		{
			Strings.Translate(this);
			GUIUtilities.ScaleDPI(this);
		}

		public static DialogResult Display(Flow flow)
		{
			frmFlow frmNew = new frmFlow(flow);
			DialogResult result = frmNew.ShowDialog();
			if (result == System.Windows.Forms.DialogResult.OK)
			{
				flow.Direction = (Flow.Directions)frmNew.cmbDirection.SelectedIndex;
				flow.Overflow = (Container.OverflowModes)frmNew.cmbOverflow.SelectedIndex;
				flow.Padding = (float)frmNew.nudPadding.Value;
				flow.ShapeSeparation = (float)frmNew.nudShapeSeparation.Value;
				flow.DragWithin = frmNew.chkDragWithin.Checked;
				flow.ShowHighlight = frmNew.chkShowHighlight.Checked;
				flow.Alignment = frmNew.m_Alignment;
			}
			return result;
		}

		public void cmbDirection_DrawItem(object sender, DrawItemEventArgs e)
		{
			Flow.Directions direction = (Flow.Directions)e.Index;
			e.DrawBackground();
			Image image = (Image)GUIUtilities.RM.GetObject("Flow_Direction_" + direction);
			Debug.Assert(cmbDirection.ItemHeight == 32);
			e.Graphics.DrawImage(image, new Rectangle(e.Bounds.X, e.Bounds.Y, 32, 32));
			e.Graphics.DrawString(Strings.Item("Flow_Direction_" + direction), cmbDirection.Font, Brushes.Black, new RectangleF(e.Bounds.Left + 40, e.Bounds.Top, e.Bounds.Width - 40, e.Bounds.Height), GUIUtilities.StringFormatCentreLeft);
			if ((e.State & DrawItemState.Focus) > 0)
				e.DrawFocusRectangle();
		}

		public void cmbOverflow_DrawItem(object sender, DrawItemEventArgs e)
		{
			Container.OverflowModes overflow = (Container.OverflowModes)e.Index;
			e.DrawBackground();
			Image image = (Image)GUIUtilities.RM.GetObject("Flow_Overflow_" + overflow);
			Debug.Assert(cmbOverflow.ItemHeight == 32);
			e.Graphics.DrawImage(image, new Rectangle(e.Bounds.X, e.Bounds.Y, 32, 32));
			e.Graphics.DrawString(Strings.Item("Flow_Overflow_" + overflow), cmbOverflow.Font, Brushes.Black, new RectangleF(e.Bounds.Left + 40, e.Bounds.Top, e.Bounds.Width - 40, e.Bounds.Height), GUIUtilities.StringFormatCentreLeft);
			if ((e.State & DrawItemState.Focus) > 0)
				e.DrawFocusRectangle();
		}

		private bool m_FillingAlignment;
		public void cmbDirection_SelectedIndexChanged(object sender, EventArgs e)
		{
			m_FillingAlignment = true;
			try
			{
				cmbAlignment.Items.Clear();
				string prefix = "Flow_" + (cmbDirection.SelectedIndex < Flow.LASTHORIZONTALDIRECTION ? "Horizontal_" : "Vertical_");
				for (int index = 0; index <= 2; index++)
				{
					cmbAlignment.Items.Add(Strings.Item(prefix + (StringAlignment)index));
				}
				cmbAlignment.SelectedIndex = (int)m_Alignment;
			}
			catch (Exception ex) // mainly in case of out of bounds
			{
				Utilities.LogSubError(ex);
			}
			finally
			{
				m_FillingAlignment = false;
			}
		}

		public void cmbAlignment_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (!m_FillingAlignment)
				m_Alignment = (StringAlignment)cmbAlignment.SelectedIndex;
		}
	}
}
