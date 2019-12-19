using System;
using System.ComponentModel;
using System.Windows.Forms;
using SAW.Commands;

namespace SAW.CommandEditors
{
	[ToolboxItem("false")]
	public partial class OnOffEditor : UserControl, ICommandEditor
	{
		private bool m_Filling;

		public OnOffEditor()
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
				if (m_Command.GetParamAsBool(0, true))
					rdoOn.Checked = true;
				else
					rdoOff.Checked = true;
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
			return null;
		}
		#endregion

		#region Events
		private void rdoOn_CheckedChanged(object sender, EventArgs e)
		{
			if (m_Filling)
				return;
			if (!rdoOn.Checked) return;
			m_Command.m_ParamList[0] = new BoolParam(true);
			UserChanged?.Invoke(this, EventArgs.Empty);
		}

		private void rdoOff_CheckedChanged(object sender, EventArgs e)
		{
			if (m_Filling)
				return;
			if (!rdoOff.Checked) return;
			m_Command.m_ParamList[0] = new BoolParam(true);
			UserChanged?.Invoke(this, EventArgs.Empty);
		}

		#endregion

	}
}
