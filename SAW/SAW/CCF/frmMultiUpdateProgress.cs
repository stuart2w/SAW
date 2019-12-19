using System.Windows.Forms;

namespace SAW.CCF
{
	public partial class frmMultiUpdateProgress : Form
	{
		public frmMultiUpdateProgress(bool logMode)
		{
			InitializeComponent();
			Strings.Translate(this);
			ControlBox = logMode;
			if (!logMode)
			{
				txtLog.Visible = false;
				Height = txtLog.Top;
			}
		}

		public void Display(int done, string current)
		{
			lblDone.Text = done.ToString();
			lblCurrent.Text = current;
			lblDone.Refresh();
			lblCurrent.Refresh();
			this.Update();
		}

		public void Log(string text)
		{
			txtLog.AppendText(text + "\r\n");
			txtLog.Refresh();
		}
	}
}