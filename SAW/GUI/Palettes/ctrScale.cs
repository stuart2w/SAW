using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SAW
{
	/// <summary>Floating palette which allows the user to enter an amount to scale by</summary>
	public partial class ctrScale : PalettePanel
	{
		private bool m_Percent = true;
		private bool m_Filling;

		public ctrScale()
		{
			InitializeComponent();
			AddGripper(new Size(14, 14));
		}

		private void rdoPercent_CheckedChanged(object sender, EventArgs e)
		{
			if (rdoPercent.Checked == m_Percent)
				return;
			m_Filling = true;
			float value = Value;
			m_Percent = rdoPercent.Checked;
			if (m_Percent)
			{
				nudValue.DecimalPlaces = 0;
				nudValue.Minimum = 10;
				nudValue.Maximum = 1000;
				nudValue.Value = (decimal)(value * 100);
			}
			else
			{
				nudValue.DecimalPlaces = 3;
				nudValue.Minimum = (decimal)0.1;
				nudValue.Maximum = 10;
				nudValue.Value = (decimal)value;
			}
			m_Filling = false;
		}

		private float Value
		{
			get
			{
				float value = (float)nudValue.Value;
				return m_Percent ? value / 100 : value;
			}
		}

		private void btnBigger_Click(object sender, EventArgs e)
		{
			DoScale(Value);
		}

		private void btnSmaller_Click(object sender, EventArgs e)
		{
			DoScale(1 / Value);
		}

		private void DoScale(float factor)
		{
			Page page = Globals.Root.CurrentPage;
			TransformScale transformScale = new TransformScale(PointF.Empty, factor, true); // new TransformRotate(page.RecentRotationPoints.FirstOrDefault(), Geometry.Radians(angle));
			Transaction transaction = new Transaction();
			foreach (Shape s in page.SelectedShapes)
			{
				transaction.Edit(s);
				transformScale.DoTransform(s);
			}
			Globals.Root.StoreNewTransaction(transaction, true);

		}
	}
}
