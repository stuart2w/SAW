using System;
using System.ComponentModel;
using System.Windows.Forms;
using SAW.Commands;

namespace SAW.CommandEditors
{
	[ToolboxItem(false)]
	public partial class KeyDelayEditor : UserControl, ICommandEditor
	{
		private bool m_Filling;

		public KeyDelayEditor()
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
				txtDelay.Text = m_Command.GetParamAsInt(0).ToString();
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
			int value;
			if (!int.TryParse(txtDelay.Text, out value) || value <0 || value>CmdSlowKeys.MAXDELAY)
				return Strings.Item("SAW_CommandEdit_BadDelay");
			return null;
		}
		#endregion

		private void txtDelay_TextChanged(object sender, EventArgs e)
		{
			if (m_Filling)
				return;
			int value;
			if (!int.TryParse(txtDelay.Text, out value) || value < 0 || value > CmdSlowKeys.MAXDELAY)
				return;
			m_Command.m_ParamList[0] = new IntegerParam(value);
			UserChanged?.Invoke(sender, EventArgs.Empty);
		}

	}
}
