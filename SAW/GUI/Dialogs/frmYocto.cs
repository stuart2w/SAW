using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SAW.Commands;

namespace SAW.GUI.Dialogs
{
	public partial class frmYocto : Form
	{

		private List<YRelay> m_Relays;

		private bool m_FillingRight;

		public frmYocto()
		{
			InitializeComponent();
			Strings.Translate(this);
			GUIUtilities.ScaleDPI(this);

			if (CmdYocto.RegisterYocto(false))
				FillList();
			lstRelays_SelectedIndexChanged(this, null);
		}

		/// <summary>Display an instance of the form, modally.</summary>
		public static void Display()
		{
			frmYocto frm = new frmYocto();
			frm.ShowDialog();
		}

		#region List and selection
		private void FillList()
		{
			lstRelays.Items.Clear();
			m_Relays = new List<YRelay>();
			foreach (YRelay relay in CmdYocto.IterateRelays())
			{
				m_Relays.Add(relay);
				lstRelays.Items.Add(relay.FriendlyName);
			}
		}

		private YRelay Selected => lstRelays.SelectedIndex >= 0 ? m_Relays[lstRelays.SelectedIndex] : null;

		private void btnRefresh_Click(object sender, EventArgs e)
		{
			YFunction._ClearCache();
			FillList();
		}

		#endregion

		#region Currently selected

		private void lstRelays_SelectedIndexChanged(object sender, EventArgs e)
		{
			m_FillingRight = true;
			tblDetails.Enabled = Selected != null;
			try
			{
				if (lstRelays.SelectedIndex >= 0)
				{
					txtName.Text = Selected.get_logicalName();
					txtSerial.Text = Selected.get_serialNumber();
					txtState.Text = Selected.StateName;
				}
				else
					txtName.Text = txtSerial.Text = txtState.Text = "";
				btnSave.Enabled = false; // it is then enabled when the user edits
			}
			catch (Exception ex)
			{
				Globals.Root.Log.WriteLine(ex.ToString());
				MessageBox.Show(ex.Message);
				tblDetails.Enabled = false;
			}
			m_FillingRight = false;
		}

		private void btnTest_Click(object sender, EventArgs e)
		{
			if (Selected == null)
				return;
			Selected.pulse(3000);
			m_tmrState.Enabled = true;
		}

		private void m_tmrState_Tick(object sender, EventArgs e)
		{
			if (Selected == null)
				m_tmrState.Enabled = false;
			else
			{
				txtState.Text = Selected.StateName;
				// this is intended just for the test function, so stop updating once the pulse has ended
				if (!Selected.StateActive)
					m_tmrState.Enabled = false;
			}
		}

		private void btnSave_Click(object sender, EventArgs e)
		{
			if (Selected == null)
				return;
			if (string.IsNullOrEmpty(txtName.Text))
				throw new UserException("[Yocto_EmptyName]");
			// All and numbers have a special meaning when identifying a relay in a script command
			if (txtName.Text.Equals(CmdYocto.ALLID, StringComparison.InvariantCultureIgnoreCase) || int.TryParse(txtName.Text, out _))
				throw new UserException("[Yocto_BadName]");
			if (!YAPI.CheckLogicalName(txtName.Text))
			throw new UserException("[Yocto_BadName]");
			try
			{
				Selected.set_logicalName(txtName.Text);
				Selected.module().saveToFlash();
				btnSave.Enabled = false;
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		}

		private void txtName_TextChanged(object sender, EventArgs e)
		{
			if (!m_FillingRight) // if USER edits, then the save button is enabled
				btnSave.Enabled = true;
		}

		#endregion

	}
}
