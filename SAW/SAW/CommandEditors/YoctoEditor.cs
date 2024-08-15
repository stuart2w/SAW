using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SAW.Commands;

namespace SAW.CommandEditors
{
	public partial class YoctoEditor : UserControl, ICommandEditor
	{

		private bool m_Filling;

		public YoctoEditor()
		{
			m_Filling = true;
			InitializeComponent();
			Strings.Translate(this);
			if (CmdYocto.RegisterYocto(false))
				FillRelays();
			m_Filling = false;
		}

		private void FillUI()
		{
			m_Filling = true;
			try
			{
				rdoAll.Checked = m_Command.IsAll;
				rdoSingle.Checked = !m_Command.IsAll;
				if (!m_Command.IsAll)
					cmbRelayID.Text = m_Command.RelayID;
				cmbAction.SelectedIndex = (int)m_Command.Command;
				if (CmdYocto.CommandUsesTime(m_Command.Command))
					txtDuration.Value = m_Command.Millisecond;
				else
					txtDuration.Value = 1000;

			}
			finally { m_Filling = false; }
		}

		private void FillRelays()
		{
			cmbRelayID.Items.Clear();
			foreach (YRelay relay in CmdYocto.IterateRelays())
			{
				cmbRelayID.Items.Add(relay.SAWName);
			}
		}

		#region ICommandEditor
		private CmdYocto m_Command;
		public event EventHandler UserChanged;

		public Command EditCommand
		{
			get { return m_Command; }
			set
			{
				m_Command = (CmdYocto)value;
				FillUI();
			}
		}

		public string GetValidationError()
		{
			if (CmdYocto.CommandUsesTime(m_Command.Command) && !txtDuration.IsValidAndNotEmpty)
				return "[Script_Error_BadYoctoTime]";
			if (rdoSingle.Checked && string.IsNullOrWhiteSpace(cmbRelayID.Text))
				return "[Script_Error_YoctoIDEmpty]";
			return null;
		}

		#endregion

		#region Events

		private void rdoAll_CheckedChanged(object sender, EventArgs e)
		{ // rdoSingle is in a container, so the automatic single selection doesn't work
			if (rdoAll.Checked)
				rdoSingle.Checked = false;
			ChangedID();
		}

		private void rdoSingle_CheckedChanged(object sender, EventArgs e)
		{
			if (rdoSingle.Checked)
				rdoAll.Checked = false;
			cmbRelayID.Enabled = rdoSingle.Checked;
			ChangedID();
		}

		private void cmbRelayID_TextChanged(object sender, EventArgs e)
		{
			ChangedID();
		}

		private void ChangedID()
		{ // called when any of the controls which can specify the ID has changed value
			string id = rdoAll.Checked ? CmdYocto.ALLID : cmbRelayID.Text;
			lblRelayNotFound.Visible = rdoSingle.Checked && !CmdYocto.FindRelay(id).Any();
			if (m_Filling)
				return;
			m_Command.RelayID = id;
			m_Command.UpdateParams();
			UserChanged?.Invoke(this, EventArgs.Empty);
		}

		private void txtDuration_TextChanged(object sender, EventArgs e)
		{
			if (m_Filling || !txtDuration.IsValidAndNotEmpty)
				return;
			m_Command.Millisecond = txtDuration.Value;
			m_Command.UpdateParams();
			UserChanged?.Invoke(this, EventArgs.Empty);
		}

		private void cmbAction_SelectedIndexChanged(object sender, EventArgs e)
		{
			CmdYocto.Commands command = (CmdYocto.Commands)cmbAction.SelectedIndex;
			lblDuration.Enabled = txtDuration.Enabled = (command == CmdYocto.Commands.Pulse || command == CmdYocto.Commands.Query);
			lblDuration.Text = Strings.Item(command == CmdYocto.Commands.Query ? "SAW_CommandEdit_Yocto_DisplayDuration" : "SAW_CommandEdit_Yocto_Duration");
			lblQueryDurationNote.Visible = (command == CmdYocto.Commands.Query);
			if (m_Filling)
				return;
			m_Command.Command = command;
			// query mode always defaults to no timeout
			if (command == CmdYocto.Commands.Query)
				txtDuration.Value = m_Command.Millisecond = 5000;
			else if (command == CmdYocto.Commands.Pulse && m_Command.Millisecond <= 0) 
				txtDuration.Value = m_Command.Millisecond = 1000;
			m_Command.UpdateParams();
			UserChanged?.Invoke(this, EventArgs.Empty);
		}

		#endregion

	}
}
