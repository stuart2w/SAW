using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using SAW.Commands;

namespace SAW.CommandEditors
{
	[ToolboxItem(false)]
	public partial class DesktopCommandEditor : UserControl, ICommandEditor
	{

		private bool m_Filling;

		public DesktopCommandEditor()
		{
			m_Filling = true;
			InitializeComponent();
			Strings.Translate(this);
			m_Filling = false;
		}

		private void FillUI()
		{
			m_Filling = true;
			try
			{
				txtFile.Text = m_Command.GetParamAsString(0, true);
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
			if (txtFile.Text == "")
				return Strings.Item("SAW_CommandEdit_DesktopEmptyFile");
			return null;
		}

		#endregion

		private void txtFile_TextChanged(object sender, EventArgs e)
		{
			try
			{
				lblMissing.Visible = txtFile.Text!= "" && m_Command is CmdOpenDesktop && !System.IO.File.Exists(txtFile.Text);
				// no message given for empty text cos that looks silly, especially when it starts empty adding a command
			}
			catch
			{
				lblMissing.Visible = true; // error is presumably a bad path or similar
			}
			if (m_Filling)
				return;
			if (m_Command.ParamList.Any())
				m_Command.ParamList[0] = new StringParam(txtFile.Text);
			else
				m_Command.ParamList.Add(new StringParam(txtFile.Text));
			UserChanged?.Invoke(this, EventArgs.Empty);
		}

		private void btnBrowse_Click(object sender, EventArgs e)
		{
			dlgOpen.Filter = Strings.Item("SAW_Filter_Desktop");
			if (dlgOpen.ShowDialog() == DialogResult.OK)
				txtFile.Text = dlgOpen.FileName;
		}
	}
}
