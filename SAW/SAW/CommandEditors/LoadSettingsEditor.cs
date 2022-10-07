using System;
using System.Windows.Forms;

namespace SAW.CommandEditors
{
	public partial class LoadSettingsEditor : UserControl, ICommandEditor
	{

		private bool m_Filling;

		public LoadSettingsEditor()
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
				txtFilename.Text = m_Command.GetParamAsString(0, true);
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
			if (string.IsNullOrWhiteSpace(txtFilename.Text))
				return Strings.Item("[SAW_CommandEdit_MissingFilename]");
			// tried doing File.Exists to see if path was valid, but it seems to throw no exceptions for invalid ones
			return null;
		}

		#endregion

		#region Events

		private void txtFilename_TextChanged(object sender, EventArgs e)
		{
			if (m_Filling)
				return;
			m_Command.ParamList[0] = new Commands.StringParam(txtFilename.Text); // array must contain [0] as FillUI used the create option
			UserChanged?.Invoke(this, EventArgs.Empty);
		}

		private void btnBrowse_Click(object sender, EventArgs e)
		{
			string file = FileDialog.ShowOpen(FileDialog.Context.OtherUserDoc, SAW.Functions.SaveSettings.ExtensionFilter);
			if (file == null)
				return;
			txtFilename.Text = file; // and event will update data
		}

		#endregion

	}
}
