using System;
using System.ComponentModel;
using System.Windows.Forms;
using SAW.Commands;

namespace SAW.CommandEditors
{
	[ToolboxItem(false)]
	public partial class AnyItemStateEditor : UserControl, ICommandEditor
	{
		private bool m_Filling;
		 
		public AnyItemStateEditor()
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
				rdoThis.Checked = m_Command.AffectsThis;
				rdoOther.Checked = !m_Command.AffectsThis;
				if (!m_Command.AffectsThis)
					txtID.Value = m_Command.ItemID;
				else
					txtID.Text = "";
			}
			finally { m_Filling = false; }
		}

		#region ICommandEditor
		private _CmdItemState m_Command;
		public event EventHandler UserChanged;
		public Command EditCommand
		{
			get { return m_Command; }
			set
			{
				m_Command = (_CmdItemState)value;
				FillUI();
			}
		}

		public string GetValidationError()
		{
			if (rdoOther.Checked && txtID.GetValidationError() != null)
				return txtID.GetValidationError();
			return null;
		}

		#endregion

		private void rdoOther_CheckedChanged(object sender, EventArgs e)
		{// they are not in same container, so we need to manually implement the exclusivity
			if (rdoThis.Checked && rdoOther.Checked)
				rdoThis.Checked = false;
			txtID.Enabled = rdoOther.Checked;
			if (!m_Filling && rdoOther.Checked)
			{
				txtID_TextChanged(sender, e); // will read the ID field and update object and fire UserChanged
				// (easier than repeating it all here)
			}
		}

		private void rdoThis_CheckedChanged(object sender, EventArgs e)
		{ // they are not in same container, so we need to manually implement the exclusivity
			if (rdoThis.Checked && rdoOther.Checked)
				rdoOther.Checked = false;
			if (!m_Filling && rdoThis.Checked)
			{
				m_Command.AffectsThis = true;
				UserChanged?.Invoke(this, EventArgs.Empty);
			}
		}

		private void txtID_TextChanged(object sender, EventArgs e)
		{
			if (m_Filling)
				return;
				m_Command.AffectsThis = (txtID.Text == "");
				if (!m_Command.AffectsThis)
					m_Command.ItemID = txtID.Value;
				UserChanged?.Invoke(this, EventArgs.Empty);
		}


	}
}
