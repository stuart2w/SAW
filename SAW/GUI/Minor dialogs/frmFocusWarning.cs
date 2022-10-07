using System;
using System.Drawing;
using System.Windows.Forms;

namespace SAW
{
	internal partial class frmFocusWarning : Form
	{
		private const int DURATION = 10;
		private DateTime m_StartTime;

		public static void Display(Form owner = null)
		{
			using (var frm = new frmFocusWarning())
			{
				if (owner != null)
					frm.Owner = owner;
				frm.ShowDialog();
			}
		}

		protected override CreateParams CreateParams
		{ //see frmRum
			get
			{
				CreateParams p = base.CreateParams;
				p.ExStyle |= (int) (Windows.WS_EX_NOACTIVATE | Windows.WS_EX_TOPMOST | Windows.WS_EX_TOOLWINDOW);
				return p;
			}
		}

		public frmFocusWarning()
		{
			InitializeComponent();
			Strings.Translate(this);
		}

		private void frmFocusWarning_Load(object sender, EventArgs e)
		{
			m_StartTime = DateTime.Now;
		}

		private void pnlCountdown_Paint(object sender, PaintEventArgs e)
		{
			int Y = (int)(pnlCountdown.Height * FractionElapsed());
			e.Graphics.FillRectangle(Brushes.Blue, 0, Y, pnlCountdown.Width, pnlCountdown.Height - Y);
		}

		private void tmrCountdown_Tick(object sender, EventArgs e)
		{
			if (FractionElapsed() > 1)
				DialogResult = DialogResult.OK;
			pnlCountdown.Refresh();
		}

		private float FractionElapsed()
		{
			return (float)(DateTime.Now.Subtract(m_StartTime).TotalSeconds / DURATION);
		}

	}
}
