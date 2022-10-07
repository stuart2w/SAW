using System;
using System.Drawing;
using System.Diagnostics;
using System.Windows.Forms;
using SAW.Shapes;

namespace SAW
{
	internal partial class ctrScriptableEdit : UserControl
	{
		private Scriptable m_Scriptable;
		private bool m_Filling;
		// there is no option to hide these, they need to be added and removed from the tab bar...
		private bool m_RepeatTabsVisible = true;
		/// <summary>Which script is currently displayed</summary>
		private int m_CurrentlyDisplayedScriptIndex = -1;

		private bool m_IsDefault;

		public ctrScriptableEdit()
		{
			m_Filling = true;
			InitializeComponent();
			Debug.Assert(chkRepeats.Checked); // must start checked as the tabs start present
			m_Filling = false;
			tcMain.SelectedIndex = 1; // start with the select tab
		}

		#region Top level data read/write
		/// <summary>Display the given scriptable object for editing. Use isDefault when editing one of the set defaults</summary>
		public void Edit(Scriptable item, bool isDefault = false)
		{
			m_Scriptable = item;
			chkRepeats.Visible = !isDefault;
			m_IsDefault = isDefault;
			FillUI();
		}

		/// <summary>Called whenever the user makes a change.  The UI will already have written the changes to the data object</summary>
		private void Edited()
		{
		}

		/// <summary>Fill the UI from the data</summary>
		private void FillUI()
		{
			m_Filling = true;
			try
			{
				this.Enabled = m_Scriptable != null;
				if (m_Scriptable == null)
					return;
				chkRepeats.Checked = m_Scriptable.RepeatTimeout >= 0;
				txtTimeout.Text = (Math.Max(0, m_Scriptable.RepeatTimeout) / 1000f).ToString("0.0");
				AssignScript();
			}
			finally
			{ m_Filling = false; }
		}

		/// <summary>Assigns the script in the individual script editor, creating it if necessary</summary>
		private void AssignScript()
		{
			int which = tcMain.SelectedIndex;
			if (m_Scriptable.Scripts[which] == null)
				m_Scriptable.Scripts[which] = new Script();
			if (m_IsDefault)
				ctrScriptEditor.Edit(m_Scriptable.Scripts[which], null); // makes the editor show no defaults etc.
			else
				ctrScriptEditor.Edit(m_Scriptable, which);
			m_CurrentlyDisplayedScriptIndex = which;
		}

		/// <summary>Returns an error message, or null if none</summary>
		public string GetValidationError()
		{ // not using the in-built validation as it mostly works on LostFocus on internal controls, and is a pain
			return ctrScriptEditor.GetValidationError();
		}

		#endregion

		#region Control events
		private void chkRepeats_CheckedChanged(object sender, EventArgs e)
		{ // All of the first part must run whether we are filling the UI or not
			lblTimeout.Visible = chkRepeats.Checked;
			txtTimeout.Visible = chkRepeats.Checked;
			btnTestRepeat.Visible = chkRepeats.Checked;
			if (m_RepeatTabsVisible && !chkRepeats.Checked)
			{
				m_RepeatTabsVisible = false;
				tcMain.TabPages.Remove(tpPre);
				tcMain.TabPages.Remove(tpRepeat);
				tcMain.TabPages.Remove(tpPost);
			}
			else if (!m_RepeatTabsVisible && chkRepeats.Checked)
			{
				m_RepeatTabsVisible = true;
				tcMain.TabPages.Add(tpPre);
				tcMain.TabPages.Add(tpRepeat);
				tcMain.TabPages.Add(tpPost);
			}
			if (m_Filling)
				return;
			txtTimeout_TextChanged(sender, e);
			// writes RepeatTimeout based on both this and value.  Also calls Edited()
		}

		private void txtTimeout_TextChanged(object sender, EventArgs e)
		{
			if (m_Filling)
				return;
			float value;
			if (!float.TryParse(txtTimeout.Text, out value))
			{
				txtTimeout.BackColor = Color.Tomato;
				m_Scriptable.RepeatTimeout = chkRepeats.Checked ? 0 : -1;
				return;
			}
			txtTimeout.BackColor = Color.White;
			m_Scriptable.RepeatTimeout = chkRepeats.Checked ? (int)(value * 1000) : -1;
			Edited();
		}

		private void tcMain_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (m_Filling)
				return;
			if (m_CurrentlyDisplayedScriptIndex >= 0 && tcMain.SelectedIndex != m_CurrentlyDisplayedScriptIndex)
			{
				string error = ctrScriptEditor.GetValidationError();
				if (error != null)
				{
					m_Filling = true; // prevents the line below triggering this and running AssignScript which would refill the GUI, overwriting the error
					tcMain.SelectedIndex = m_CurrentlyDisplayedScriptIndex;
					MessageBox.Show(error);
					m_Filling = false;
					return;
				}
			}
			AssignScript();
		}

		#endregion

	}
}
