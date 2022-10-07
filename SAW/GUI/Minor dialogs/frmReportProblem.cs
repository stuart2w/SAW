using System;
using System.Windows.Forms;

namespace SAW
{
	internal partial class frmReportProblem
	{
		public frmReportProblem()
		{
			InitializeComponent();
		}

		public void frmReportProblem_Load(object sender, EventArgs e)
		{
			Strings.Translate(this);
			GUIUtilities.ScaleDPI(this);
		}

		public static void Display()
		{
			frmReportProblem frmNew = new frmReportProblem();
			if (frmNew.ShowDialog() != System.Windows.Forms.DialogResult.OK)
				return;
			Application.DoEvents(); // will hopefully get rid of the error box before the screen grab is done
			Globals.Root.RequestDelayedCall(() => frmErrorReport.ProcessErrorReport("Manual problem report", frmNew.txtInfo.Text, true));
		}

		public void txtInfo_TextChanged(object sender, EventArgs e)
		{
			btnSend.Enabled = !string.IsNullOrEmpty(txtInfo.Text);
		}
	}
}
