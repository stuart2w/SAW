using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using SAW.Commands;

namespace SAW.CommandEditors
{
	/// <summary>Does CmdOpenApp, CmdParamApp, CmdAlternateApp, CmdSwitchToApp, CmdOpenSelectionSet</summary>
	[ToolboxItem(false)]
	public partial class AppCommandEditor : UserControl, ICommandEditor
	{
		private bool m_Filling;

		public AppCommandEditor()
		{
			m_Filling = true;
			InitializeComponent();
			Strings.Translate(this);
			m_Filling = false;
		}

		private void FillUI()
		{ // also EditCommand set, sets the top label
			m_Filling = true;
			try
			{
				lblAlternateID.Visible = m_Command is CmdAlternateApp;
				txtAlternateID.Visible = m_Command is CmdAlternateApp;
				lblParams2.Visible = m_Command is CmdAlternateApp;
				txtParams2.Visible = m_Command is CmdAlternateApp;
				lblParams.Visible = m_Command is CmdParamApp || m_Command is CmdAlternateApp;
				txtParams.Visible = m_Command is CmdParamApp || m_Command is CmdAlternateApp;
				btnRunningApps.Visible = m_Command is CmdSwitchToApp;
				if (m_Command is CmdAlternateApp)
				{
					txtAlternateID.Text = m_Command.m_ParamList[3].ValueAsString();
					txtParams2.Text = m_Command.m_ParamList[2].ValueAsString();
				}
				if (m_Command is CmdParamApp || m_Command is CmdAlternateApp)
					txtParams2.Text = m_Command.m_ParamList[1].ValueAsString();
				txtApplication.Text = m_Command.m_ParamList[0].ValueAsString();
				chkUseOutput.Checked = (txtApplication.Text == CmdOpenApp.USEOUTPUT);
				if (chkUseOutput.Checked)
					txtApplication.Text = "";
				if (m_Command is CmdOpenSelectionSet)
					lblOpen.Text = Strings.Item("SAW_CommandEdit_SetToOpen");
			}
			finally { m_Filling = false; }
		}

		#region ICommandEditor
		private Command m_Command;
		public event EventHandler UserChanged;

		public Command EditCommand
		{
			get { return m_Command; }
			set
			{
				m_Command = value;
				lblOpen.Text = Strings.Item(m_Command is CmdSwitchToApp ? "SAW_CommandEdit_ApplicatonToSwitch" : "SAW_CommandEdit_ApplicatonToOpen");
				FillUI();
			}
		}

		public string GetValidationError()
		{
			return null;
		}
		#endregion

		#region Control events
		private void chkUseOutput_CheckedChanged(object sender, EventArgs e)
		{
			txtApplication.Enabled = !chkUseOutput.Checked; // this runs even when filling UI
			btnBrowse.Enabled = !chkUseOutput.Checked;
			if (m_Filling)
				return;
			if (chkUseOutput.Checked)
				m_Command.m_ParamList[0] = new StringParam(CmdOpenApp.USEOUTPUT);
			else
				m_Command.m_ParamList[0] = new StringParam(txtApplication.Text);
			UserChanged?.Invoke(this, EventArgs.Empty);
		}

		private void txtAlternateID_TextChanged(object sender, EventArgs e)
		{
			if (m_Filling)
				return;
			m_Command.m_ParamList[3] = new StringParam(txtAlternateID.Text);
			UserChanged?.Invoke(this, EventArgs.Empty);
		}

		private void txtParams2_TextChanged(object sender, EventArgs e)
		{
			if (m_Filling)
				return;
			m_Command.m_ParamList[2] = new StringParam(txtParams2.Text);
			UserChanged?.Invoke(this, EventArgs.Empty);
		}

		private void txtParams_TextChanged(object sender, EventArgs e)
		{
			if (m_Filling)
				return;
			m_Command.m_ParamList[1] = new StringParam(txtParams.Text);
			UserChanged?.Invoke(this, EventArgs.Empty);
		}

		private void txtApplication_TextChanged(object sender, EventArgs e)
		{
			if (m_Filling)
				return;
			m_Command.m_ParamList[0] = new StringParam(txtApplication.Text);
			UserChanged?.Invoke(this, EventArgs.Empty);
		}

		private void btnBrowse_Click(object sender, EventArgs e)
		{
			if (dlgOpen.ShowDialog() != DialogResult.OK)
				return;
			string file = dlgOpen.FileName;
			if (m_Command is CmdOpenSelectionSet)
			{// for selection sets (only) if the file is in the same folder as the current set, then use a local filename:
				string currentFolder = Path.GetDirectoryName(Globals.Root.CurrentDocument.Filename ?? "");
				if (!string.IsNullOrEmpty(currentFolder) && string.Equals(currentFolder, Path.GetDirectoryName(file), StringComparison.CurrentCultureIgnoreCase))
					file = Path.GetFileName(file);
			}
			txtApplication.Text = file; // will trigger the event to save the changes
		}

		#endregion

		#region Running menu
		private void btnRunningApps_Click(object sender, EventArgs e)
		{
			if (ctxRunningApplications.Items.Count == 0)
			{
				var processes = (from name in CmdSwitchToApp.GetRunningProcesses() orderby name select name).Distinct();
				foreach (string process in processes)
				{
					ctxRunningApplications.Items.Add(process).Click += RunningMenu_Click;
				}
			}
			ctxRunningApplications.Show(btnRunningApps, Point.Empty, ToolStripDropDownDirection.Right);
		}

		private void RunningMenu_Click(object sender, EventArgs e)
		{
			txtApplication.Text = (sender as ToolStripMenuItem)?.Text ?? "";
		}

		#endregion

	}
}
