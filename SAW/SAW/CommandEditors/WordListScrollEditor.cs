using System;
using System.Windows.Forms;
using SAW.Commands;

namespace SAW.CommandEditors
{
	public partial class WordListScrollEditor : UserControl, ICommandEditor
	{
		private bool m_Filling;

		public WordListScrollEditor()
		{
			m_Filling = true;
			InitializeComponent();
			Strings.Translate(this);
			m_Filling = false;
			//rdoTop.CheckedChanged += this.rdo_CheckedChanged;
			//rdoUp.CheckedChanged += this.rdo_CheckedChanged;
			//rdoDown.CheckedChanged += this.rdo_CheckedChanged;
			//rdoBottom.CheckedChanged += this.rdo_CheckedChanged;
		}

		private void FillUI()
		{
			m_Filling = true;
			try
			{
				rdoTop.Checked = m_Command.Direction == CmdWordListScroll.Directions.Top;
				rdoUp.Checked = m_Command.Direction == CmdWordListScroll.Directions.Up;
				rdoDown.Checked = m_Command.Direction == CmdWordListScroll.Directions.Down;
				rdoBottom.Checked = m_Command.Direction == CmdWordListScroll.Directions.Bottom;
			}
			finally { m_Filling = false; }
		}

		#region ICommandEditor
		private CmdWordListScroll m_Command;
		public event EventHandler UserChanged;
		public Command EditCommand
		{
			get { return m_Command; }
			set
			{
				m_Command = (CmdWordListScroll)value;
				FillUI();
			}
		}

		public string GetValidationError()
		{
			if (!rdoBottom.Checked && !rdoDown.Checked && !rdoUp.Checked && !rdoTop.Checked)
				return Strings.Item("SAW_CommandEdit_SelectPosition"); // not quite the perfect message - just repurposed an existing one
			return null;
		}

		#endregion

		private void rdoTop_CheckedChanged(object sender, EventArgs e)
		{
			if (m_Filling)
				return;
			if (!rdoTop.Checked)
				return;
			m_Command.Direction = CmdWordListScroll.Directions.Top;
			UserChanged?.Invoke(sender, e);
		}

		private void rdoUp_CheckedChanged(object sender, EventArgs e)
		{
			if (m_Filling)
				return;
			if (!rdoUp.Checked)
				return;
			m_Command.Direction = CmdWordListScroll.Directions.Up;
			UserChanged?.Invoke(sender, e);
		}

		private void rdoDown_CheckedChanged(object sender, EventArgs e)
		{
			if (m_Filling)
				return;
			if (!rdoDown.Checked)
				return;
			m_Command.Direction = CmdWordListScroll.Directions.Down;
			UserChanged?.Invoke(sender, e);
		}

		private void rdoBottom_CheckedChanged(object sender, EventArgs e)
		{
			if (m_Filling)
				return;
			if (!rdoBottom.Checked)
				return;
			m_Command.Direction = CmdWordListScroll.Directions.Bottom;
			UserChanged?.Invoke(sender, e);
		}
	}
}
