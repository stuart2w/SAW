using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using SAW.Commands;

namespace SAW.CommandEditors
{
	/// <summary>Does CmdMoveTo ("movewindow") which moves a window based on EXE name</summary>
	[ToolboxItem(false)]
	public partial class MoveWindowEditor : UserControl, ICommandEditor
	{
		private bool m_Filling;

		public MoveWindowEditor()
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
				txtApplication.Text = m_Command.ParamList[0].ValueAsString();
				chkUseOutput.Checked = (txtApplication.Text == CmdOpenApp.USEOUTPUT);
				if (chkUseOutput.Checked)
					txtApplication.Text = "";
				// by using create=true param here we can just assume everywhere else that the params exist
				txtX.Value = m_Command.GetParamAsInt(1, true);
				txtY.Value = m_Command.GetParamAsInt(2, true);
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
				FillUI();
			}
		}

		public string GetValidationError()
		{
			if (!txtX.IsValid || !txtY.IsValid)
				return Strings.Item("SAW_CommandEdit_NotNumber");
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
				m_Command.ParamList[0] = new StringParam(CmdOpenApp.USEOUTPUT);
			else
				m_Command.ParamList[0] = new StringParam(txtApplication.Text);
			UserChanged?.Invoke(this, EventArgs.Empty);
		}

		private void txtApplication_TextChanged(object sender, EventArgs e)
		{
			if (m_Filling)
				return;
			m_Command.ParamList[0] = new StringParam(txtApplication.Text);
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

		private void txtX_TextChanged(object sender, EventArgs e)
		{
			if (m_Filling)
				return;
			if (txtX.IsValid)
			{
				m_Command.ParamList[1] = new IntegerParam(txtX.Value);
				UserChanged?.Invoke(this, EventArgs.Empty);
			}
		}

		private void txtY_TextChanged(object sender, EventArgs e)
		{
			if (m_Filling)
				return;
			if (txtY.IsValid)
			{
				m_Command.ParamList[2] = new IntegerParam(txtY.Value);
				UserChanged?.Invoke(this, EventArgs.Empty);
			}
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
