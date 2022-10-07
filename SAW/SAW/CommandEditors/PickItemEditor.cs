using System;
using System.ComponentModel;
using System.Windows.Forms;
using SAW.Commands;

namespace SAW.CommandEditors
{

	/// <summary>Editor for any command with an item ID as the only param</summary>
	[ToolboxItem(false)]
	public partial class PickItemEditor : UserControl, ICommandEditor
	{
		private bool m_Filling;

		public PickItemEditor()
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
				txtID.Value = m_Command.GetParamAsInt(0, true);
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
			return txtID.GetValidationError();
		}

		#endregion

		private void txtID_TextChanged(object sender, EventArgs e)
		{
			if (m_Filling)
				return;
			m_Command.ParamList.Clear();
			m_Command.ParamList.Add(new IntegerParam(txtID.Value));
			UserChanged?.Invoke(this, EventArgs.Empty);
		}

	}
}
