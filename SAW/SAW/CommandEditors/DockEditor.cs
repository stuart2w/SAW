using System;
using System.ComponentModel;
using System.Windows.Forms;
using SAW.Commands;

namespace SAW.CommandEditors
{
	[ToolboxItem("false")]
	public partial class DockEditor : UserControl, ICommandEditor
	{
		private bool m_Filling;

		public DockEditor()
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
				switch ((m_Command as CmdDockWindow).Direction)
				{
					case CmdDockWindow.Directions.Above:
						rdoAbove.Checked = true;
						break;
					case CmdDockWindow.Directions.Below:
						rdoBelow.Checked = true;
						break;
					case CmdDockWindow.Directions.Left:
						rdoLeft.Checked = true;
						break;
					case CmdDockWindow.Directions.Right:
						rdoRight.Checked = true;
						break;
				}
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

		private void rdo_CheckedChanged(object sender, EventArgs e)
		{ // This same function is used for all 4 radio buttons
			if (m_Filling)
				return;
			RadioButton rdo = (RadioButton) sender;
			if (!rdo.Checked)
				return;
			int value = int.Parse((string) rdo.Tag);
			(m_Command as CmdDockWindow).Direction = (CmdDockWindow.Directions) value;
			UserChanged?.Invoke(sender, EventArgs.Empty);
		}

		#endregion

	}
}
