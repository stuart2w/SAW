using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

// test for form transparency and similar.  Can be removed later

namespace SAW.GUI
{ 

	public partial class frmTest : Form
	{
		public frmTest()
		{
			InitializeComponent();
			this.Paint += frmTest_Paint;
			//this.BackColor = Color.Magenta;
			//this.TransparencyKey = Color.Magenta;
			//this.FormBorderStyle = FormBorderStyle.None;
			//this.Opacity = 0;
			//timer1.Enabled = true;
			//timer1.Tick += (s, e) =>
			//{
			//	if (this.Opacity == 1)
			//		timer1.Enabled = false;
			//	else
			//		Opacity = Math.Min(1, Opacity + 0.05f);
			//};
		}

		private void frmTest_Paint(object sender, PaintEventArgs e)
		{
			var pen = new Pen(Color.Red, 3);
			e.Graphics.DrawEllipse(pen, 0, 0, 100, 100);
		}


	}
}
