using System;
using System.Drawing;
using System.Windows.Forms;


namespace SAW
{
	public partial class frmLineWidth
	{
		public frmLineWidth()
		{
			InitializeComponent();
		}

		/// <summary>Returns Single.NaN if cancelled.  Value given in mm</summary>
		public static float Ask(float initial = float.NaN)
		{
			frmLineWidth frm = new frmLineWidth();
			if (!float.IsNaN(initial))
			{
				try
				{
					frm.nudWidth.Value = (decimal)(initial / Geometry.POINTUNIT); // exception handler in case it is out of bounds
				}
				catch (Exception)
				{
				}
			}
			if (frm.ShowDialog() != System.Windows.Forms.DialogResult.OK)
			{
				return float.NaN;
			}
			return (float)frm.nudWidth.Value * Geometry.POINTUNIT;
		}

		public void pnlSample_Paint(object sender, PaintEventArgs e)
		{
			e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
			using (Pen pn = new Pen(Brushes.Black, (float)nudWidth.Value * Geometry.POINTUNIT * (e.Graphics.DpiX / Geometry.INCH)) { EndCap = System.Drawing.Drawing2D.LineCap.Round, StartCap = System.Drawing.Drawing2D.LineCap.Round })
			{
				e.Graphics.DrawLine(pn, 12.0F, pnlSample.Height / 2f, pnlSample.Width - 12.0F, pnlSample.Height / 2f);
			}

		}

		public void nudWidth_KeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
		{
			pnlSample.Invalidate();
		}

		public void nudWidth_ValueChanged(object sender, System.EventArgs e)
		{
			pnlSample.Invalidate();
		}

		public void frmLineWidth_Load(object sender, EventArgs e)
		{
			GUIUtilities.ScaleDPI(this);
			Strings.Translate(this);
		}
	}
}
