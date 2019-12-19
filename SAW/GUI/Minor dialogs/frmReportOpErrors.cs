using System;
using System.Drawing;
using System.Windows.Forms;

namespace SAW
{
	public partial class frmReportOpErrors : Form
	{
		public frmReportOpErrors()
		{
			InitializeComponent();
			Strings.Translate(this);
		}

		/// <summary>Displays errors, returns OK if the user elects to continue</summary>
		public static bool Display(string errorList, string operationName)
		{
			using (var frm = new frmReportOpErrors())
			{
				frm.txtErrors.Text = errorList;
				frm.lblTitle.Text = operationName;
				return (frm.ShowDialog() == DialogResult.OK);
			}
		}

		private void tableLayoutPanel1_Resize(object sender, EventArgs e)
		{
			lblPrompt.MaximumSize = new Size(tableLayoutPanel1.Width - 6, 0);
		}
	}
}
