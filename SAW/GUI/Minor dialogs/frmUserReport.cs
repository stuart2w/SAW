using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SAW
{
	/// <summary>Used to report a message to the user during scanning.  Only displayed from RunView </summary>
	public partial class frmUserReport : Form
	{
		private DateTime m_EndTime;
		private Action m_WhenClosed;

		public frmUserReport()
		{
			InitializeComponent();
			Strings.Translate(this);
		}

		/// <summary>Displays the message, and calls the callback when the window is closed for any reason</summary>
		public static frmUserReport Display(string message, Control owner, int timeLimit = 0, Action whenClosed = null)
		{
			var frm = new frmUserReport() { m_WhenClosed = whenClosed };
			frm.lblMessage.Text = message;
			if (timeLimit <= 0)
				frm.m_EndTime = DateTime.Now.AddYears(1);
			else
			{
				frm.m_EndTime = DateTime.Now.AddMilliseconds(timeLimit);
				frm.tmrAutoClose.Enabled = true;
			}
			frm.Show(owner);
			return frm;
		}

		/// <summary>To be called by RunView when a switch is triggered while this is displayed.  This will close and invoke the callback </summary>
		public void SwitchActivated()
		{
			Close();
		}

		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			base.OnFormClosing(e);
			m_WhenClosed?.Invoke();
			m_WhenClosed = null; // just in case ensures it only happens once
			Dispose();
		}

		private void m_tmrAutoClose_Tick(object sender, EventArgs e)
		{
			if (DateTime.Now > m_EndTime)
				Close();
		}
	}
}
